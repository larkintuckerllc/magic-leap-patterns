// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2017 Magic Leap, Inc. (COMPANY) All Rights Reserved.
// Magic Leap, Inc. Confidential and Proprietary
//
//  NOTICE:  All information contained herein is, and remains the property
//  of COMPANY. The intellectual and technical concepts contained herein
//  are proprietary to COMPANY and may be covered by U.S. and Foreign
//  Patents, patents in process, and are protected by trade secret or
//  copyright law.  Dissemination of this information or reproduction of
//  this material is strictly forbidden unless prior written permission is
//  obtained from COMPANY.  Access to the source code contained herein is
//  hereby forbidden to anyone except current COMPANY employees, managers
//  or contractors who have executed Confidentiality and Non-disclosure
//  agreements explicitly covering such access.
//
//  The copyright notice above does not evidence any actual or intended
//  publication or disclosure  of  this source code, which includes
//  information that is confidential and/or proprietary, and is a trade
//  secret, of  COMPANY.   ANY REPRODUCTION, MODIFICATION, DISTRIBUTION,
//  PUBLIC  PERFORMANCE, OR PUBLIC DISPLAY OF OR THROUGH USE  OF THIS
//  SOURCE CODE  WITHOUT THE EXPRESS WRITTEN CONSENT OF COMPANY IS
//  STRICTLY PROHIBITED, AND IN VIOLATION OF APPLICABLE LAWS AND
//  INTERNATIONAL TREATIES.  THE RECEIPT OR POSSESSION OF  THIS SOURCE
//  CODE AND/OR RELATED INFORMATION DOES NOT CONVEY OR IMPLY ANY RIGHTS
//  TO REPRODUCE, DISCLOSE OR DISTRIBUTE ITS CONTENTS, OR TO MANUFACTURE,
//  USE, OR SELL ANYTHING THAT IT  MAY DESCRIBE, IN WHOLE OR IN PART.
//
// %COPYRIGHT_END%
// --------------------------------------------------------------------*/
// %BANNER_END%

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using UnityEngine.Experimental.XR.MagicLeap;
using System.Collections.Generic;

namespace MagicLeap
{
    /// <summary>
    /// This class handles video recording and loading based on controller
    /// input.
    /// </summary>
    public class VideoCaptureExample : MonoBehaviour
    {
        [System.Serializable]
        private class VideoCaptureEvent : UnityEvent<string>
        {}

        #region Private Variables
        [SerializeField, Tooltip("The maximum amount of time the camera can be recording for (in seconds.)")]
        private float _maxRecordingTime = 10.0f;

        [Header("Events")]
        [SerializeField, Tooltip("Event called when recording starts")]
        private UnityEvent OnVideoCaptureStarted;

        [SerializeField, Tooltip("Event called when recording stops")]
        private VideoCaptureEvent OnVideoCaptureEnded;

        private const string _validFileFormat = ".mp4";

        private const float _defaultVolume = 0.3f;

        private const float _minRecordingTime = 1.0f;

        // Is the camera currently recording
        private bool _isCapturing;

        // The file path to the active capture
        private string _captureFilePath;

        private bool _isCameraConnected = false;

        private float _captureStartTime;

        private enum PrivilegeState
        {
            Off,
            Started,
            Requested,
            Granted,
            Denied
        }

        private PrivilegeState _currentPrivilegeState = PrivilegeState.Off;

        private MLPrivilegeId[] _privilegesNeeded = { MLPrivilegeId.CameraCapture, MLPrivilegeId.AudioCaptureMic};

        private List<MLPrivilegeId> _privilegesGranted = new List<MLPrivilegeId>();

        private bool _hasStarted = false;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validate that _maxRecordingTime is not less than minimum possible.
        /// </summary>
        private void OnValidate()
        {
            if (_maxRecordingTime < _minRecordingTime)
            {
                Debug.LogWarning(string.Format("You can not have a MaxRecordingTime less than {0}, setting back to minimum allowed!", _minRecordingTime));
                _maxRecordingTime = _minRecordingTime;
            }
        }

        /// <summary>
        /// Start Privilege API.
        /// </summary>
        void OnEnable()
        {
            MLResult result = MLPrivileges.Start();
            if (result.IsOk)
            {
                _currentPrivilegeState = PrivilegeState.Started;
            }
            else
            {
                Debug.LogError("Privilege Error: failed to startup");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Move through the privilege stages before enabling the feature that requires privileges.
        /// </summary>
        void Update()
        {
            /// Privileges have not yet been granted, go through the privilege states.
            if (_currentPrivilegeState != PrivilegeState.Granted)
            {
                UpdatePrivilege();
            }
            /// Privileges have been granted, enable the feature and run any normal updates items.
            /// Done in a seperate if statement so enable can be done in the same frame as the
            /// privilege is granted.
            if (_currentPrivilegeState == PrivilegeState.Granted)
            {
                EnableCapture();

                if (_isCapturing)
                {
                    // If the recording has gone longer than the max time
                    if (Time.time - _captureStartTime > _maxRecordingTime)
                    {
                        EndCapture();
                    }
                }
            }
        }

        /// <summary>
        /// Stop the camera, unregister callbacks, and stop input and privileges APIs.
        /// </summary>
        void OnDisable()
        {
            if (MLInput.IsStarted)
            {
                MLInput.OnControllerButtonDown -= OnButtonDown;
                MLInput.Stop();
            }

            if (_isCameraConnected)
            {
                DisableMLCamera();
            }

            if(_currentPrivilegeState != PrivilegeState.Off)
            {
                MLPrivileges.Stop();

                _currentPrivilegeState = PrivilegeState.Off;
                _privilegesGranted.Clear();

            }
        }

        /// <summary>
        /// Cannot make the assumption that a privilege is still granted after
        /// returning from pause. Return the application to the state where it
        /// requests privileges needed and clear out the list of already granted
        /// privileges. Also, disable the camera and unregister callbacks.
        /// </summary>
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (_currentPrivilegeState != PrivilegeState.Off)
                {
                    _privilegesGranted.Clear();
                    _currentPrivilegeState = PrivilegeState.Started;
                }

                if (_isCameraConnected)
                {
                    DisableMLCamera();
                }

                MLInput.OnControllerButtonDown -= OnButtonDown;

                _hasStarted = false;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connects the MLCamera component and instantiates a new instance
        /// if it was never created.
        /// </summary>
        public bool EnableMLCamera()
        {
            MLResult result = MLCamera.Start();
            if (result.IsOk)
            {
                result = MLCamera.Connect();
                _isCameraConnected = result.IsOk;
            }
            return _isCameraConnected;
        }

        /// <summary>
        /// Disconnects the MLCamera if it was ever created or connected.
        /// Also stops any video recording if active.
        /// </summary>
        public void DisableMLCamera()
        {
            if(_isCapturing)
            {
                EndCapture();
            }
            MLCamera.Disconnect();
            _isCameraConnected = false;
            MLCamera.Stop();
        }

        /// <summary>
        /// Start capturing video.
        /// </summary>
        public void StartCapture()
        {
            string fileName = System.DateTime.Now.ToString("MM_dd_yyyy__HH_mm_ss") + _validFileFormat;
            StartCapture(fileName);
        }

        /// <summary>
        /// Start capturing video to input filename.
        /// </summary>
        /// <param name="fileName">File path to write the video to.</param>
        public void StartCapture(string fileName)
        {
            if(!_isCapturing && MLCamera.IsStarted && _isCameraConnected)
            {
                // Check file fileName extensions
                string extension = System.IO.Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(extension) || !extension.Equals(_validFileFormat, System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogErrorFormat("Invalid fileName extension '{0}' passed into Capture({1}).\n" +
                        "Videos must be saved in {2} format.", extension, fileName, _validFileFormat);
                    return;
                }

                string pathName = System.IO.Path.Combine(Application.persistentDataPath, fileName);

                MLResult result = MLCamera.StartVideoCapture(pathName);
                if (result.IsOk)
                {
                    _isCapturing = true;
                    _captureStartTime = Time.time;
                    _captureFilePath = pathName;
                    OnVideoCaptureStarted.Invoke();
                }
                else
                {
                    Debug.LogErrorFormat("Failure: Could not start video capture for {0}. Error Code: {1}",
                        fileName, MLCamera.GetErrorCode().ToString());
                }
            }
            else
            {
                Debug.LogErrorFormat("Failure: Could not start video capture for {0} because '{1}' is already recording!",
                    fileName, _captureFilePath);
            }
        }

        /// <summary>
        /// Stop capturing video.
        /// </summary>
        public void EndCapture()
        {
            if(_isCapturing)
            {
                MLResult result = MLCamera.StopVideoCapture();
                if (result.IsOk)
                {
                    _isCapturing = false;
                    _captureStartTime = 0;
                    OnVideoCaptureEnded.Invoke(_captureFilePath);
                    _captureFilePath = null;
                }
                else
                {
                    Debug.LogErrorFormat("Failure: Could not end video capture. Error Code: {0}",
                        MLCamera.GetErrorCode().ToString());
                }
            }
            else
            {
                Debug.LogError("Failure: Could not EndCapture() because the camera is not recording.");
            }
        }

        /// <summary>
        /// Attempts to load a captured video into the passed in VideoPlayer
        /// component and sets all of the nescessary values.
        /// </summary>
        /// <param name="pathName">The path and name of the video (including extension)</param>
        /// <param name="videoPlayer">The reference to the VideoPlayer component used to store the video.</param>
        public static bool LoadCapturedVideo(string pathName, ref VideoPlayer videoPlayer, ref AudioSource audioSource)
        {
            if(videoPlayer == null)
            {
                Debug.LogErrorFormat("Failure: The passed in VideoPlayer reference to LoadCapturedVideo was null.");
                return false;
            }

            if(audioSource == null)
            {
                Debug.LogErrorFormat("Failure: The passed in AudioSource reference to LoadCapturedVideo was null.");
                return false;
            }

            string extension = System.IO.Path.GetExtension(pathName);
            if(string.IsNullOrEmpty(extension) || extension != _validFileFormat)
            {
                Debug.LogErrorFormat("Failure: The passed in file at {0} does not have the valid extension type of {1}",
                    pathName, _validFileFormat);

                return false;
            }

            videoPlayer.url = pathName;
            videoPlayer.isLooping = true;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.SetDirectAudioVolume(0, _defaultVolume);
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audioSource);
            return true;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event for button down. Starts or stops recording.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void OnButtonDown(byte controllerId, MLInputControllerButton button)
        {
            if (MLInputControllerButton.Bumper == button)
            {
                if (!_isCapturing)
                {
                    StartCapture();
                }
                else if(_isCapturing && Time.time - _captureStartTime > _minRecordingTime)
                {
                    EndCapture();
                }
            }
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Handle the privilege states.
        /// </summary>
        private void UpdatePrivilege()
        {
            switch (_currentPrivilegeState)
            {
                /// Privilege API has been started successfully, ready to make requests.
                case PrivilegeState.Started:
                    {
                        RequestPrivileges();
                        break;
                    }
                /// Privilege requests have been made, wait until all privileges are granted before enabling the feature that requires privileges.
                case PrivilegeState.Requested:
                    {
                        foreach (MLPrivilegeId priv in _privilegesNeeded)
                        {
                            if (!_privilegesGranted.Contains(priv))
                            {
                                return;
                            }
                        }
                        _currentPrivilegeState = PrivilegeState.Granted;
                        break;
                    }
                /// Privileges have been denied, respond appropriately.
                case PrivilegeState.Denied:
                    {
                        enabled = false;
                        break;
                    }
            }
        }

        /// <summary>
        /// Once privileges have been granted, enable the camera and callbacks.
        /// </summary>
        private void EnableCapture()
        {
            if (!_hasStarted)
            {
                MLResult result = MLInput.Start();
                if (!result.IsOk)
                {
                    Debug.LogError("Failed to start MLInput on VideoCapture component. Disabling the script.");
                    enabled = false;
                    return;
                }

                if (!EnableMLCamera())
                {
                    Debug.LogError("MLCamera failed to connect. Disabling VideoCapture component.");
                    enabled = false;
                    return;
                }

                MLInput.OnControllerButtonDown += OnButtonDown;

                _hasStarted = true;
            }
        }

        /// <summary>
        /// Request each needed privilege.
        /// </summary>
        private void RequestPrivileges()
        {
            foreach (MLPrivilegeId priv in _privilegesNeeded)
            {
                MLResult result = MLPrivileges.RequestPrivilegeAsync(priv, HandlePrivilegeAsyncRequest);
                if (!result.IsOk)
                {
                    Debug.LogErrorFormat("{0} Privilege Request Error: {1}", priv, result);
                    _currentPrivilegeState = PrivilegeState.Denied;
                    return;
                }
            }

            _currentPrivilegeState = PrivilegeState.Requested;
        }

        /// <summary>
        /// Handles the result that is received from the query to the Privilege API.
        /// If one of the required privileges are denied, set the Privilege state to Denied.
        /// <param name="result">The resulting status of the query</param>
        /// <param name="privilegeId">The privilege being queried</param>
        /// </summary>
        private void HandlePrivilegeAsyncRequest(MLResult result, MLPrivilegeId privilegeId)
        {
            if ((MLPrivilegesResult)result.Code == MLPrivilegesResult.Granted)
            {
                _privilegesGranted.Add(privilegeId);
                Debug.LogFormat("{0} Privilege Granted", privilegeId);
            }
            else
            {
                Debug.LogErrorFormat("{0} Privilege Error: {1}, disabling example.", privilegeId, result);
                _currentPrivilegeState = PrivilegeState.Denied;
            }
        }
        #endregion
    }
}
