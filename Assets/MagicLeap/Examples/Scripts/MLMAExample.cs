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
using System;

namespace MagicLeap
{
    /// <summary>
    /// This represents a virtual controller visualization that mimics the current state of the
    /// Mobile Device running the Magic Leap Mobile Application. Button presses, touch pad are all represented along with
    /// the orientation of the mobile device. There is no position information available
    /// </summary>
    public class MLMAExample : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("The highlight for the left button.")]
        private GameObject _leftButtonHighlight;

        [SerializeField, Tooltip("The highlight for the right button.")]
        private GameObject _rightButtonHighlight;

        [SerializeField, Tooltip("The indicator for the home tap.")]
        private GameObject _homeTapIndicator;

        [SerializeField, Tooltip("Number of seconds to show home tap.")]
        private float _homeActiveDuration = 0.5f;
        private float _timeToDeactivateHome = 0;

        [SerializeField, Tooltip("The indicator for the first touch.")]
        private GameObject _touch1Indicator;

        [SerializeField, Tooltip("The indicator for the second touch.")]
        private GameObject _touch2Indicator;

        [SerializeField, Tooltip("The keyboard input text.")]
        private Text _keyboardText;

        [SerializeField, Tooltip("Renderer of the Mesh")]
        private MeshRenderer _modelRenderer;

        [Space, SerializeField, Tooltip("Hand to track mlma and show visualizers for.")]
        private MLInput.Hand _hand = MLInput.Hand.Left;

        private Color _origColor;

        private MLInputController _mlInputController;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes component data, starts MLInput, validates parameters, initializes indicator states
        /// </summary>
        void Awake()
        {
            if (!MLInput.Start().IsOk)
            {
                Debug.LogError("Error ControllerExample starting MLInput, disabling script.");
                enabled = false;
                return;
            }
            if (_leftButtonHighlight == null)
            {
                Debug.LogError("Error ControllerExample._moveButtonHighlight is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_rightButtonHighlight == null)
            {
                Debug.LogError("Error ControllerExample._appButtonHighlight is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_homeTapIndicator == null)
            {
                Debug.LogError("Error ControllerExample._homeTapIndicator is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_touch1Indicator == null)
            {
                Debug.LogError("Error ControllerExample._touch1Indicator is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_touch2Indicator == null)
            {
                Debug.LogError("Error ControllerExample._touch2Indicator is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_keyboardText == null)
            {
                Debug.LogError("Error ControllerExample._keyboardText is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_modelRenderer == null)
            {
                Debug.LogError("Error ControllerExample._modelRenderer is not set, disabling script.");
                enabled = false;
                return;
            }

            _leftButtonHighlight.SetActive(false);
            _rightButtonHighlight.SetActive(false);
            _homeTapIndicator.SetActive(false);
            _touch1Indicator.SetActive(false);
            _touch2Indicator.SetActive(false);

            _keyboardText.text = "";
            _origColor = _modelRenderer.material.color;

            MLInputController controller = MLInput.GetController(_hand);
            _mlInputController = (controller != null && controller.Type == MLInputControllerType.MobileApp) ? controller : null;

            MLInput.OnControllerConnected += HandleOnControllerConnected;
            MLInput.OnControllerDisconnected += HandleOnControllerDisconnected;
            MLInput.OnControllerButtonDown += HandleOnButtonDown;
            MLInput.OnControllerButtonUp += HandleOnButtonUp;
            MLInput.OnTriggerDown += HandleOnTriggerDown;
            MLInput.OnTriggerUp += HandleOnTriggerUp;
        }

        /// <summary>
        /// Updates effects on different input responses via input polling mechanism.
        /// </summary>
        void Update()
        {
            _modelRenderer.material.color = Color.red;

            if (_mlInputController != null && _mlInputController.Type == MLInputControllerType.MobileApp)
            {
                _modelRenderer.material.color = _origColor;
                UpdateTouchIndicator(_touch1Indicator, _mlInputController.Touch1Active, _mlInputController.Touch1PosAndForce);
                UpdateTouchIndicator(_touch2Indicator, _mlInputController.Touch2Active, _mlInputController.Touch2PosAndForce);
                UpdateHighlights();
            }

            else if (_mlInputController != null)
            {
                _mlInputController = null;
            }
        }

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDestroy()
        {
            if (MLInput.IsStarted)
            {
                MLInput.OnTriggerUp -= HandleOnTriggerUp;
                MLInput.OnTriggerDown -= HandleOnTriggerDown;
                MLInput.OnControllerButtonUp -= HandleOnButtonUp;
                MLInput.OnControllerButtonDown -= HandleOnButtonDown;
                MLInput.OnControllerDisconnected -= HandleOnControllerDisconnected;
                MLInput.OnControllerConnected -= HandleOnControllerConnected;

                MLInput.Stop();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Turn off HomeTap visualizer after certain time.
        /// </summary>
        private void UpdateHighlights()
        {
            if (_timeToDeactivateHome < Time.time)
            {
                _homeTapIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// Update visualizers for touchpad.
        /// </summary>
        /// <param name="indicator"> Visual object to place on touch position. </param>
        /// <param name="active"> State of the touch. </param>
        /// <param name="pos"> Raw data for touchpad touch position. </param>
        private void UpdateTouchIndicator(GameObject indicator, bool active, Vector3 pos)
        {
            indicator.SetActive(active);
            indicator.transform.localPosition = new Vector3(pos.x * 0.042f,
                pos.y * 0.042f + 0.01f, indicator.transform.localPosition.z);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event for controller connected.
        /// </summary>
        /// <param name="controllerId"> The id of the controller. </param>
        private void HandleOnControllerConnected(byte controllerId)
        {
            // Type not available on OnControllerConnected, checking on update.
            if (_hand == MLInput.GetController(controllerId).Hand && _mlInputController == null)
            {
                _mlInputController = MLInput.GetController(controllerId);
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
            }
        }

        /// <summary>
        /// Handles the event for button down.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void HandleOnButtonDown(byte controllerId, MLInputControllerButton button)
        {
            if (_mlInputController != null && _mlInputController.Id == controllerId &&
                button == MLInputControllerButton.Bumper)
            {
                _leftButtonHighlight.SetActive(true);
            }
        }

        /// <summary>
        /// Handles the event for button up.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void HandleOnButtonUp(byte controllerId, MLInputControllerButton button)
        {
            if (_mlInputController != null && _mlInputController.Id == controllerId)
            {
                if (button == MLInputControllerButton.Bumper)
                {
                    _leftButtonHighlight.SetActive(false);
                }
                else if (button == MLInputControllerButton.HomeTap)
                {
                    _homeTapIndicator.SetActive(true);
                    _timeToDeactivateHome = Time.time + _homeActiveDuration;
                }
            }
        }

        /// <summary>
        /// Handles the event for trigger down
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="value">The trigger value</param>
        private void HandleOnTriggerDown(byte controllerId, float value)
        {
            if (_mlInputController != null && _mlInputController.Id == controllerId)
            {
                _rightButtonHighlight.SetActive(true);
            }
        }

        /// <summary>
        /// Handles the event for trigger up
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="value">The trigger value</param>
        private void HandleOnTriggerUp(byte controllerId, float value)
        {
            if (_mlInputController != null && _mlInputController.Id == controllerId)
            {
                _rightButtonHighlight.SetActive(false);
            }
        }

        /// <summary>
        /// Keyboard events are propagated via Unity's event system. OnGUI is the preferred way
        /// to catch these events.
        /// </summary>
        private void OnGUI()
        {
            Event e = Event.current;

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Backspace)
                {
                    if (_keyboardText.text.Length > 0)
                    {
                        _keyboardText.text = _keyboardText.text.Substring(0, _keyboardText.text.Length - 1);
                    }
                }
                else if (e.keyCode == KeyCode.Return)
                {
                    _keyboardText.text += "\n";
                }
                else if (!Char.IsControl(e.character))
                {
                    _keyboardText.text += e.character;
                }
            }
        }
        #endregion
    }
}
