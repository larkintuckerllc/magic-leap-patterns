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
using UnityEngine;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    public class ControllerVisualizer : MonoBehaviour
    {
        #region Public Enum
        [System.Flags]
        public enum DeviceTypesAllowed : byte
        {
            Controller = 1,
            MobileApp = 2,
            All = Controller | MobileApp,
        }
        #endregion

        #region Private Variables
        [SerializeField, Tooltip("The controller model.")]
        private GameObject _controller;

        [Header("Controller Parts"), Space]
        [SerializeField, Tooltip("The controller's trigger model.")]
        private GameObject _trigger;

        [SerializeField, Tooltip("The controller's touchpad model.")]
        private GameObject _touchpad;

        [SerializeField, Tooltip("The controller's home button model.")]
        private GameObject _homeButton;

        [SerializeField, Tooltip("The controller's bumper button model.")]
        private GameObject _bumperButton;

        [SerializeField, Tooltip("The Game Object showing the touch model on the touchpad")]
        private Transform _touchIndicatorTransform;

        [Space, SerializeField, Tooltip("Hand to use this visualizer for.")]
        private MLInput.Hand _hand = MLInput.Hand.Left;

        [Space, SerializeField, Tooltip("Device types to get visulization for.")]
        private DeviceTypesAllowed _devices = DeviceTypesAllowed.All;

        private MLInputController _mlInputController;

        // Color when the button state is idle.
        private Color _defaultColor = Color.white;
        // Color when the button state is active.
        private Color _activeColor = Color.grey;

        private Material _touchpadMaterial;
        private Material _triggerMaterial;
        private Material _homeButtonMaterial;
        private Material _bumperButtonMaterial;

        private float _touchpadRadius;

        private const float MAX_TRIGGER_ROTATION = -35.0f;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initialize variables, callbacks and check null references.
        /// </summary>
        void Start()
        {
            MLResult result = MLInput.Start();
            if (!result.IsOk)
            {
                Debug.LogError("Error ControllerVisualizer starting MLInput, disabling script.");
                enabled = false;
                return;
            }
            if (!_controller)
            {
                Debug.LogError("Error ControllerVisualizer._controller not set, disabling script.");
                enabled = false;
                return;
            }
            if (!_trigger)
            {
                Debug.LogError("Error ControllerVisualizer._trigger not set, disabling script.");
                enabled = false;
                return;
            }
            if (!_touchpad)
            {
                Debug.LogError("Error ControllerVisualizer._touchpad not set, disabling script.");
                enabled = false;
                return;
            }
            if (!_homeButton)
            {
                Debug.LogError("Error ControllerVisualizer._homeButton not set, disabling script.");
                enabled = false;
                return;
            }
            if (!_bumperButton)
            {
                Debug.LogError("Error ControllerVisualizer._bumperButton not set, disabling script.");
                enabled = false;
                return;
            }
            if (!_touchIndicatorTransform)
            {
                Debug.LogError("Error ControllerVisualizer._touchIndicatorTransform not set, disabling script.");
                enabled = false;
                return;
            }

            MLInputController controller = MLInput.GetController(_hand);
            if (controller != null && controller.Connected && ((uint)(controller.Type) & (uint)(_devices)) != 0)
            {
                _mlInputController = controller;
                SetVisibility(true);
            }
            else
            {
                _mlInputController = null;
                SetVisibility(false);
            }

            MLInput.OnControllerConnected += HandleOnControllerConnected;
            MLInput.OnControllerDisconnected += HandleOnControllerDisconnected;
            MLInput.OnControllerButtonUp += HandleOnButtonUp;
            MLInput.OnControllerButtonDown += HandleOnButtonDown;

            _triggerMaterial = FindMaterial(_trigger);
            _touchpadMaterial = FindMaterial(_touchpad);
            _homeButtonMaterial = FindMaterial(_homeButton);
            _bumperButtonMaterial = FindMaterial(_bumperButton);

            // Calculate the radius of the touchpad's mesh
            Mesh mesh = _touchpad.GetComponent<MeshFilter>().mesh;
            _touchpadRadius = Vector3.Scale(mesh.bounds.extents, _touchpad.transform.lossyScale).x;
        }

        /// <summary>
        /// Update controller input based feedback.
        /// </summary>
        void Update()
        {
            if (_mlInputController != null && ((uint)(_mlInputController.Type) & (uint)(_devices)) != 0)
            {
                UpdateTriggerVisuals();
                UpdateTouchpadIndicator();
            }
            else if (_mlInputController != null)
            {
                _mlInputController = null;
                SetVisibility(false);
            }
        }

        /// <summary>
        /// Stop input api and unregister callbacks.
        /// </summary>
        void OnDestroy()
        {
            if (MLInput.IsStarted)
            {
                MLInput.OnControllerButtonDown -= HandleOnButtonDown;
                MLInput.OnControllerButtonUp -= HandleOnButtonUp;
                MLInput.OnControllerDisconnected -= HandleOnControllerDisconnected;
                MLInput.OnControllerConnected -= HandleOnControllerConnected;
                MLInput.Stop();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Sets the visual pressure indicator for the appropriate button MeshRenderers.
        /// <param name="renderer">The meshrenderer to modify.</param>
        /// <param name="pressure">The pressure sensitivy interpolant for the meshrendere.r</param>
        /// </summary>
        private void SetPressure(MeshRenderer renderer, float pressure)
        {
            if (renderer.material.HasProperty("_Cutoff"))
            {
                renderer.material.SetFloat("_Cutoff", pressure);
            }
        }

        /// <summary>
        /// Update the touchpad's indicator: (location, directions, color).
        /// Also updates the color of the touchpad, based on pressure.
        /// </summary>
        private void UpdateTouchpadIndicator()
        {
            Vector3 updatePosition = new Vector3(_mlInputController.Touch1PosAndForce.x, 0.0f, _mlInputController.Touch1PosAndForce.y);
            float touchY = _touchIndicatorTransform.localPosition.y;
            _touchIndicatorTransform.localPosition = new Vector3(-updatePosition.x * _touchpadRadius, touchY, -updatePosition.z * _touchpadRadius);

            if (_mlInputController.Touch1Active)
            {
                _touchIndicatorTransform.gameObject.SetActive(true);
                float angle = Mathf.Atan2(_mlInputController.Touch1PosAndForce.x, _mlInputController.Touch1PosAndForce.y);
                _touchIndicatorTransform.localRotation = Quaternion.Euler(0, 180.0f + (angle * Mathf.Rad2Deg), 0);
            }
            else
            {
                _touchIndicatorTransform.gameObject.SetActive(false);
            }

            float force = _mlInputController.Touch1PosAndForce.z;
            _touchpadMaterial.color = Color.Lerp(_defaultColor, _activeColor, force);
        }

        /// <summary>
        /// Update the rotation and visual color of the trigger.
        /// </summary>
        private void UpdateTriggerVisuals()
        {
            // Change the color of the trigger
            _triggerMaterial.color = Color.Lerp(_defaultColor, _activeColor, _mlInputController.TriggerValue);

            // Set the rotation of the trigger
            Vector3 eulerRot = _trigger.transform.localRotation.eulerAngles;
            eulerRot.x = Mathf.Lerp(0, MAX_TRIGGER_ROTATION, _mlInputController.TriggerValue);
            _trigger.transform.localRotation = Quaternion.Euler(eulerRot);
        }

        /// <summary>
        /// Attempt to get the Material of a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to search for a material.</param>
        /// <returns>Material of the GameObject, if it exists. Otherwise, null.</returns>
        private Material FindMaterial(GameObject gameObject)
        {
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            return (renderer != null) ? renderer.material : null;
        }

        /// <summary>
        /// Sets the color of all Materials.
        /// </summary>
        /// <param name="color">The color to be applied to the materials.</param>
        private void SetAllMaterialColors(Color color)
        {
            _triggerMaterial.color = color;
            _touchpadMaterial.color = color;
            _homeButtonMaterial.color = color;
            _bumperButtonMaterial.color = color;
        }

        /// <summary>
        /// Coroutine to reset the home color back to the original color.
        /// </summary>
        private IEnumerator RestoreHomeColor()
        {
            yield return new WaitForSeconds(0.5f);
            _homeButtonMaterial.color = _defaultColor;
        }

        /// <summary>
        /// Set object visibility to value.
        /// </summary>
        /// <param name="value"> true or false to set visibility. </param>
        private void SetVisibility(bool value)
        {
            Renderer[] rendererArray = _controller.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rendererArray)
            {
                r.enabled = value;
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event for controller connected.
        /// Assign controller to connected controller if desired hand matches
        /// with new connected controller.
        /// </summary>
        /// <param name="controllerId"> The id of the controller. </param>
        private void HandleOnControllerConnected(byte controllerId)
        {
            if (_hand == MLInput.GetController(controllerId).Hand)
            {
                _mlInputController = MLInput.GetController(controllerId);
                SetVisibility(true);
            }
        }

        /// <summary>
        /// Handles the event for controller disconnected.
        /// Remove controller reference if controller id matches
        /// with disconnected controller.
        /// </summary>
        /// <param name="controllerId"> The id of the controller. </param>
        private void HandleOnControllerDisconnected(byte controllerId)
        {
            if (_mlInputController != null && _mlInputController.Id == controllerId)
            {
                _mlInputController = null;
                SetVisibility(false);
            }
        }

        /// <summary>
        /// Handles the event for button down.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void HandleOnButtonDown(byte controllerId, MLInputControllerButton button)
        {
            if (_mlInputController != null && _mlInputController.Id == controllerId &&
                button == MLInputControllerButton.Bumper)
            {
                // Sets the color of the Bumper to the active color.
                _bumperButtonMaterial.color = _activeColor;
            }
        }

        /// <summary>
        /// Handles the event for button up.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void HandleOnButtonUp(byte controllerId, MLInputControllerButton button)
        {
            if (_mlInputController != null && _mlInputController.Id == controllerId)
            {
                if (button == MLInputControllerButton.Bumper)
                {
                    // Sets the color of the Bumper to the default color.
                    _bumperButtonMaterial.color = _defaultColor;
                }

                else if (button == MLInputControllerButton.HomeTap)
                {
                    // Note: HomeTap is NOT a button. It's a physical button on the controller.
                    // But in the application side, the tap registers as a ButtonUp event and there is NO
                    // ButtonDown equivalent. We cannot detect holding down the Home (button). The OS will
                    // handle it as either a return to the icon grid or turning off the controller.
                    _homeButtonMaterial.color = _activeColor;
                    StartCoroutine(RestoreHomeColor());
                }
            }
        }
        #endregion
    }
}
