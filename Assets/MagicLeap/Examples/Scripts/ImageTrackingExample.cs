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

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.XR.MagicLeap;
using System.Collections.Generic;

namespace MagicLeap
{
    /// <summary>
    /// This provides an example of interacting with the image tracker visualizers using the controller
    /// </summary>
    public class ImageTrackingExample : MonoBehaviour
    {
        #region Public Enum
        public enum ViewMode : int
        {
            All = 0,
            AxisOnly,
            TrackingCubeOnly,
            DemoOnly,
        }

        public GameObject[] TrackerBehaviours;
        #endregion

        #region Private Variables
        private ViewMode _viewMode = ViewMode.All;

        [SerializeField, Tooltip("Image Tracking Visualizers to control")]
        private ImageTrackingVisualizer [] _visualizers;

        [SerializeField, Tooltip("The headpose canvas for example status text.")]
        private Text _statusLabel;

        private enum PrivilegeState
        {
            Off,
            Started,
            Requested,
            Granted,
            Denied
        }

        private PrivilegeState _currentPrivilegeState = PrivilegeState.Off;

        private MLPrivilegeId[] _privilegesNeeded = { MLPrivilegeId.CameraCapture };

        private List<MLPrivilegeId> _privilegesGranted = new List<MLPrivilegeId>();

        private bool _hasStarted = false;
        #endregion

        #region Unity Methods

        /// <summary>
        /// Start Privilege API.
        /// </summary>
        void Start()
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
        /// Unregister callbacks and stop input API.
        /// </summary>
        void OnDestroy()
        {
            if (MLInput.IsStarted)
            {
                MLInput.OnControllerButtonDown -= HandleOnButtonDown;
                MLInput.Stop();
            }

            if (_currentPrivilegeState != PrivilegeState.Off)
            {
                MLPrivileges.Stop();
            }
        }

        /// <summary>
        /// Cannot make the assumption that a privilege is still granted after
        /// returning from pause. Return the application to the state where it
        /// requests privileges needed and clear out the list of already granted
        /// privileges. Also, unregister callbacks.
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

                MLInput.OnControllerButtonDown -= HandleOnButtonDown;

                updateImageTrackerBehaviours(false);

                _hasStarted = false;
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
                StartCapture();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Enable/Disable the correct objects depending on view options
        /// </summary>
        void UpdateVisualizers()
        {
            foreach (ImageTrackingVisualizer visualizer in _visualizers)
            {
                visualizer.UpdateViewMode(_viewMode);
            }
        }

        /// <summary>
        /// Control when to enable to image trackers based on
        /// if the correct privileges are given.
        /// </summary>
        void updateImageTrackerBehaviours(bool enabled)
        {
            foreach (GameObject obj in TrackerBehaviours)
            {
                obj.SetActive(enabled);
            }
        } 
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event for button down.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void HandleOnButtonDown(byte controller_id, MLInputControllerButton button)
        {
            if (button == MLInputControllerButton.Bumper)
            {
                _viewMode = (ViewMode)((int)(_viewMode + 1) % Enum.GetNames(typeof(ViewMode)).Length);
                _statusLabel.text = string.Format("View Mode: {0}", _viewMode.ToString());
            }
            UpdateVisualizers();
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
        private void StartCapture()
        {
            if (!_hasStarted)
            {
                MLResult result = MLInput.Start();
                if (!result.IsOk)
                {
                    Debug.LogError("Failed to start MLInput on ImageCapture component. Disabling the script.");
                    enabled = false;
                    return;
                }

                updateImageTrackerBehaviours(true);

                if (_visualizers.Length < 1)
                {
                    Debug.LogError("Error ImageTrackingExample._visualizers not set, disabling script.");
                    enabled = false;
                    return;
                }
                if (null == _statusLabel)
                {
                    Debug.LogError("Error ImageTrackingExample._statusLabel is not set, disabling script.");
                    enabled = false;
                    return;
                }

                MLInput.OnControllerButtonDown += HandleOnButtonDown;

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
