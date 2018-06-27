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

namespace MagicLeap
{
    /// <summary>
    /// The one true controller to rule the other material controllers.
    /// </summary>
    public class MasterMaterialController : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("Plane Visualizer")]
        private PlaneVisualizer _visualizer;

        private MaterialController [] _materialControllers;
        private MaterialController _materialControllerInGaze;

        private MLInputController _controller;

        [SerializeField, Tooltip("Status Text to show which object is currently being manipulated")]
        private Text _statusText;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validate variables
        /// </summary>
        void Start ()
        {
            if (null == _visualizer)
            {
                Debug.LogError("MasterMaterialController._visualizer not set, disabling script");
                enabled = false;
                return;
            }

            _materialControllers = GetComponents<MaterialController>();
            if (_materialControllers.Length < 1)
            {
                Debug.LogError("MasterMaterialController._materialControllers is empty, disabling script.");
                enabled = false;
                return;
            }
            MLResult result = MLInput.Start();
            if (!result.IsOk)
            {
                Debug.LogError("Error MasterMaterialController starting MLInput, disabling script.");
                enabled = false;
                return;
            }

            _statusText.text = "";
            _controller = MLInput.GetController(MLInput.Hand.Left);
            MLInput.OnControllerButtonUp += HandleOnButtonUp;
        }

        /// <summary>
        /// Unregister event handlers
        /// </summary>
        void OnDestroy()
        {
            MLInput.OnControllerButtonUp -= HandleOnButtonUp;
            MLInput.Stop();
        }

        /// <summary>
        /// Update the specific material, on the plane that the user is facing, based on controller input
        /// </summary>
        void Update ()
        {
            if (!_controller.Connected)
            {
                return;
            }

            // Manipulate material in view
            if (_controller.TouchpadGesture.Type == MLInputControllerTouchpadGestureType.Swipe &&
                _controller.TouchpadGestureState != MLInputControllerTouchpadGestureState.End)
            {
                if (_controller.TouchpadGesture.Direction == MLInputControllerTouchpadGestureDirection.Right)
                {
                    UpdateMaterialController(0.5f * Time.deltaTime);
                }
                else if (_controller.TouchpadGesture.Direction == MLInputControllerTouchpadGestureDirection.Left)
                {
                    UpdateMaterialController(-0.5f * Time.deltaTime);
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update the parameter on the material controller that owns the renderer
        /// </summary>
        /// <param name="value">Adjustment value</param>
        private void UpdateMaterialController(float value)
        {
            if (_materialControllerInGaze)
            {
                _materialControllerInGaze.OnUpdateValue(value);
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Toggle viewing custom materials and plane borders on button press
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void HandleOnButtonUp(byte controller_id, MLInputControllerButton button)
        {
            if (button == MLInputControllerButton.Bumper)
            {
                _visualizer.ToggleShowingPlanes();
            }
        }

        /// <summary>
        /// Update the selected material. This function is called frequently (like Update).
        /// </summary>
        /// <param name="state"> The state of the raycast result.</param>
        /// <param name="result"> The hit results (point, normal, distance).</param>
        /// <param name="confidence"> Confidence value of hit. 0 no hit, 1 sure hit.</param>
        public void HandleOnRaycastHit(MLWorldRays.MLWorldRaycastResultState state, RaycastHit result, float confidence)
        {
            if (confidence > 0 && null != result.transform)
            {
                // TODO: keep track of the last transform seen
                Renderer planeRenderer = result.transform.GetComponent<Renderer>();
                if (null != planeRenderer)
                {
                    foreach (MaterialController controller in _materialControllers)
                    {
                        if (controller.ReferenceMaterial == planeRenderer.sharedMaterial)
                        {
                            _materialControllerInGaze = controller;
                            _materialControllerInGaze.UpdateTextOnView();
                            return;
                        }
                    }
                }
            }
            _statusText.text = "";
            _materialControllerInGaze = null;
        }
        #endregion
    }
}
