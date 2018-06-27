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
using UnityEngine.UI;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This example demonstrates using the magic leap raycast functionality to calculate intersection with the physical space.
    /// It demonstrates casting rays from the users headpose, controller, and eyes position and orientation.
    ///
    /// This example uses several raycast visualizers which represent this intersection with the physical space.
    /// </summary>
    public class RaycastExample : MonoBehaviour
    {
        public enum RaycastMode
        {
            Controller,
            Head,
            Eyes
        }

        #region Private Variables
        [SerializeField, Tooltip("The headpose canvas for example status text.")]
        private Text _statusLabel;

        [SerializeField, Tooltip("The headpose canvas for example instruction text.")]
        private Text _calibrationInstructionsLabel;

        [SerializeField, Tooltip("Raycast from controller.")]
        private WorldRaycastController _raycastController;

        [SerializeField, Tooltip("Raycast from headpose.")]
        private WorldRaycastHead _raycastHead;

        [SerializeField, Tooltip("Raycast from eyegaze.")]
        private WorldRaycastEyes _raycastEyes;

        private RaycastMode _raycastMode = RaycastMode.Controller;
        private int _modeCount = System.Enum.GetNames(typeof(RaycastMode)).Length;

        private float _confidence = 0.0f;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validate all required components and sets event handlers.
        /// </summary>
        void Awake()
        {
            MLResult result = MLInput.Start();
            if (!result.IsOk)
            {
                Debug.LogError("Error RaycastExample starting MLInput, disabling script.");
                enabled = false;
                return;
            }

            if (_statusLabel == null)
            {
                Debug.LogError("Error RaycastExample._statusLabel is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_raycastController == null)
            {
                Debug.LogError("Error RaycastExample._raycastController is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_raycastHead == null)
            {
                Debug.LogError("Error RaycastExample._raycastHead is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_raycastEyes == null)
            {
                Debug.LogError("Error RaycastExample._raycastEyes is not set, disabling script.");
                enabled = false;
                return;
            }

#if !UNITY_EDITOR // Removing calibration step from ML Remote Host builds.
            _calibrationInstructionsLabel.text += "Home Button Tap:\n * Calibrate controller to static model.\n * Toggle back to calibration step.";
#endif

            MLInput.OnControllerButtonDown += OnButtonDown;
            UpdateRaycastMode();
        }

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDestroy()
        {
            MLInput.OnControllerButtonDown -= OnButtonDown;
            MLInput.Stop();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates type of raycast and enables correct cursor.
        /// </summary>
        private void UpdateRaycastMode()
        {
            // Default all objects to inactive and then set active to the appropriate ones.
            _raycastController.gameObject.SetActive(false);
            _raycastController.Controller.gameObject.SetActive(false);

            _raycastHead.gameObject.SetActive(false);
            _raycastEyes.gameObject.SetActive(false);

            switch (_raycastMode)
            {
                case RaycastMode.Controller:
                {
                    _raycastController.gameObject.SetActive(true);
                    _raycastController.Controller.gameObject.SetActive(true);
                    break;
                }
                case RaycastMode.Head:
                {
                    _raycastHead.gameObject.SetActive(true);
                    break;
                }
                case RaycastMode.Eyes:
                {
                    _raycastEyes.gameObject.SetActive(true);
                    break;
                }
            }
        }

        /// <summary>
        /// Updates Status Label with latest data.
        /// </summary>
        private void UpdateStatusText()
        {
            _statusLabel.text = string.Format("Raycast Mode: {0}\nRaycast Hit Confidence: {1}", _raycastMode.ToString(), _confidence.ToString());
            if(_raycastMode == RaycastMode.Eyes && MLEyes.IsStarted)
            {
                _statusLabel.text += string.Format("\n\nEye Calibration Status: {0}", MLEyes.CalibrationStatus.ToString());
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event for button down and cycles the raycast mode.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void OnButtonDown(byte controller_id, MLInputControllerButton button)
        {
            if (button == MLInputControllerButton.Bumper)
            {
                _raycastMode = (RaycastMode)((int)(_raycastMode + 1) % _modeCount);
                UpdateRaycastMode();
                UpdateStatusText();
            }
        }

        /// <summary>
        /// Callback handler called when raycast has a result.
        /// Updates the confidence value to the new confidence value.
        /// </summary>
        /// <param name="state"> The state of the raycast result.</param>
        /// <param name="result">The hit results (point, normal, distance).</param>
        /// <param name="confidence">Confidence value of hit. 0 no hit, 1 sure hit.</param>
        public void OnRaycastHit(MLWorldRays.MLWorldRaycastResultState state, RaycastHit result, float confidence)
        {
            _confidence = confidence;
            UpdateStatusText();
        }
        #endregion
    }
}
