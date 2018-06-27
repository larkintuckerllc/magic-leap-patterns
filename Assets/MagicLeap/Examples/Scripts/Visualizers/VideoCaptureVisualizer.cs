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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This class handles visualization of the video and the UI with the status
    /// of the recording.
    /// </summary>
    public class VideoCaptureVisualizer : MonoBehaviour
    {
        #region Private Variables
        [SerializeField]
        private VideoPlayer _videoPlayer;

        [SerializeField]
        private AudioSource _audioSource;

        [Header("Visuals")]
        [SerializeField, Tooltip("Text to show instructions for capturing video")]
        private UnityEngine.UI.Text _previewText;

        [SerializeField, Tooltip("Object that will show up when recording")]
        private GameObject _recordingIndicator;

        #endregion

        #region Unity Methods
        /// <summary>
        /// Check for all required variables to be initialized.
        /// </summary>
        void Start()
        {
            if(_videoPlayer == null)
            {
                Debug.Log("The VideoCaptureVisualizer component does not have it's _videoPlayer reference assigned. Disabling script.");
                enabled = false;
                return;
            }

            if(_audioSource == null)
            {
                Debug.Log("The VideoCaptureVisualizer component does not have it's _audioSource reference assigned. Disabling script.");
                enabled = false;
                return;
            }

            if(_previewText == null)
            {
                Debug.Log("The VideoCaptureVisualizer component does not have it's _previewText reference assigned. Disabling script.");
                enabled = false;
                return;
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles video capture being started.
        /// </summary>
        public void OnCaptureStarted()
        {
            // Manage canvas visuals
            if(_recordingIndicator != null)
            {
                _recordingIndicator.SetActive(true);
            }
            if(_previewText != null)
            {
                _previewText.text = "Press the bumper to stop capturing a video.";
            }

            // Disable the preview
            _videoPlayer.gameObject.SetActive(false);
        }

        /// <summary>
        /// Handles video capture ending.
        /// </summary>
        /// <param name="path">file path to load captured video to.</param>
        public void OnCaptureEnded(string path)
        {
            // Manage canvas visuals
            if(_recordingIndicator != null)
            {
                _recordingIndicator.SetActive(false);
            }

            if(_previewText != null)
            {
                _previewText.text = "Press the bumper to start capturing a video.";
            }

            // Load the captured video
            VideoCaptureExample.LoadCapturedVideo(path, ref _videoPlayer, ref _audioSource);

            // Enable the preview
            _videoPlayer.gameObject.SetActive(true);
        }
        #endregion
    }
}
