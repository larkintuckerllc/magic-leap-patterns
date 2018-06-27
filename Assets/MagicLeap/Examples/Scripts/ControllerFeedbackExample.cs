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
    /// <summary>
    /// This class provides examples of how you can use haptics and LEDs
    /// on the Control.
    /// </summary>
    public class ControllerFeedbackExample : MonoBehaviour
    {
        #region Private Variables
        [Space, SerializeField, Tooltip("Hand to get feedback for.")]
        private MLInput.Hand _hand = MLInput.Hand.Left;

        private MLInputController _mlInputController;

        private int _lastLEDindex = -1;

        #region Const Variables
        private const float TRIGGER_DOWN_MIN_VALUE = 0.2f;

        // UpdateLED - Constants
        private const float HALF_HOUR_IN_DEGREES = 15.0f;
        private const float DEGREES_PER_HOUR = 12.0f / 360.0f;

        private const int MIN_LED_INDEX = (int)(MLInputControllerFeedbackPatternLED.Clock12);
        private const int MAX_LED_INDEX = (int)(MLInputControllerFeedbackPatternLED.Clock6And12);
        private const int LED_INDEX_DELTA = MAX_LED_INDEX - MIN_LED_INDEX;
        #endregion
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
                Debug.LogError("Error ControllerFeedbackExample starting MLInput, disabling script.");
                enabled = false;
                return;
            }

            MLInputController controller = MLInput.GetController(_hand);
            if (controller != null && controller.Connected && (controller.Type == MLInputControllerType.Control))
            {
                _mlInputController = controller;
            }
            else
            {
                _mlInputController = null;
            }

            MLInput.OnControllerConnected += HandleOnControllerConnected;
            MLInput.OnControllerDisconnected += HandleOnControllerDisconnected;
            MLInput.OnControllerButtonUp += HandleOnButtonUp;
            MLInput.OnControllerButtonDown += HandleOnButtonDown;
            MLInput.OnTriggerDown += HandleOnTriggerDown;
        }

        /// <summary>
        /// Update controller input based feedback.
        /// </summary>
        void Update()
        {
            if (_mlInputController != null && (_mlInputController.Type == MLInputControllerType.Control))
            {
                UpdateLED();
            }
            else if (_mlInputController != null)
            {
                _mlInputController = null;
            }
        }

        /// <summary>
        /// Stop input api and unregister callbacks.
        /// </summary>
        void OnDestroy()
        {
            if (MLInput.IsStarted)
            {
                MLInput.OnTriggerDown -= HandleOnTriggerDown;
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
        /// Updates LED on the physical controller based on touch pad input.
        /// </summary>
        private void UpdateLED()
        {
            if (_mlInputController.Touch1Active)
            {
                // Get angle of touchpad position.
                float angle = -Vector2.SignedAngle(Vector2.up, _mlInputController.Touch1PosAndForce);
                if (angle < 0.0f)
                {
                    angle += 360.0f;
                }

                // Get the correct hour and map it to [0,6]
                int index = (int)((angle + HALF_HOUR_IN_DEGREES) * DEGREES_PER_HOUR) % LED_INDEX_DELTA;

                // Pass from hour to MLInputControllerFeedbackPatternLED index  [0,6] -> [MAX_LED_INDEX, MIN_LED_INDEX + 1, ..., MAX_LED_INDEX - 1]
                index = (MAX_LED_INDEX + index > MAX_LED_INDEX) ? MIN_LED_INDEX + index : MAX_LED_INDEX;

                if (_lastLEDindex != index)
                {
                    // a duration of 0 means leave it on indefinitely
                    _mlInputController.StartFeedbackPatternLED((MLInputControllerFeedbackPatternLED)index, MLInputControllerFeedbackColorLED.BrightCosmicPurple, 0);
                    _lastLEDindex = index;
                }
            }
            else if (_lastLEDindex != -1)
            {
                _mlInputController.StopFeedbackPatternLED();
                _lastLEDindex = -1;
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
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void HandleOnButtonDown(byte controllerId, MLInputControllerButton button)
        {
            if (_mlInputController != null && _mlInputController.Id == controllerId &&
                button == MLInputControllerButton.Bumper)
            {
                // Demonstrate haptics using callbacks.
                _mlInputController.StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.ForceDown, MLInputControllerFeedbackIntensity.Medium);
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
                    // Demonstrate haptics using callbacks.
                    _mlInputController.StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.ForceUp, MLInputControllerFeedbackIntensity.Medium);
                }
            }
        }

        /// <summary>
        /// Handles the event for trigger down.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="value">The value of the trigger button.</param>
        private void HandleOnTriggerDown(byte controllerId, float value)
        {
            if (_mlInputController != null && _mlInputController.Id == controllerId)
            {
                MLInputControllerFeedbackIntensity intensity = (MLInputControllerFeedbackIntensity)((int)(value * 2.0f));
                _mlInputController.StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.Buzz, intensity);
            }
        }
        #endregion
    }
}
