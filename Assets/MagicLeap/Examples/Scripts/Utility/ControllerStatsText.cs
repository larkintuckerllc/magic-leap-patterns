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
using UnityEngine.Serialization;
using UnityEngine.Experimental.XR.MagicLeap;

using System.Collections;

namespace MagicLeap
{
    /// <summary>
    /// This provides textual state feedback for the connected controller.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class ControllerStatsText : MonoBehaviour
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
        [SerializeField, Tooltip("Devices Allowed")]
        public DeviceTypesAllowed _allowedDevices = DeviceTypesAllowed.All;

        [Space, SerializeField, Tooltip("Controller hand to display stats for.")]
        private MLInput.Hand _hand;

        private MLInputController _controller;

        private Text _controllerStatsText;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes component data and starts MLInput.
        /// </summary>
        void Awake()
        {
            MLResult result = MLInput.Start();
            if (!result.IsOk)
            {
                Debug.LogError("Error ControllerStatsText starting MLInput, disabling script.");
                enabled = false;
                return;
            }

            _controllerStatsText = gameObject.GetComponent<Text>();
            _controllerStatsText.color = Color.white;

            _controller = MLInput.GetController(_hand);
        }

        /// <summary>
        /// Updates text with latest controller stats.
        /// </summary>
        void Update()
        {
            if (_controller != null && _controller.Connected)
            {
                if (_controller.Type == MLInputControllerType.Control &&
                    (_allowedDevices & DeviceTypesAllowed.Controller) == DeviceTypesAllowed.Controller)
                {
                    _controllerStatsText.text =
                    string.Format("" +
                    "Position:\t<i>{0}</i>\n" +
                    "Rotation:\t<i>{1}</i>\n\n" +
                    "<color=#ffc800>Buttons</color>\n" +
                    "Trigger:\t\t<i>{2}</i>\n" +
                    "Bumper:\t\t<i>{3}</i>\n\n" +
                    "<color=#ffc800>Touchpad</color>\n" +
                    "Location:\t<i>({4},{5})</i>\n" +
                    "Pressure:\t<i>{6}</i>\n\n" +
                    "<color=#ffc800>Gestures</color>\n" +
                    "<i>{7} {8}</i>",

                    _controller.Position.ToString("n2"),
                    _controller.Orientation.eulerAngles.ToString("n2"),
                    _controller.TriggerValue.ToString("n2"),
                    _controller.State.ButtonState[(int)MLInputControllerButton.Bumper],
                    _controller.Touch1Active ? _controller.Touch1PosAndForce.x.ToString("n2") : "0.00",
                    _controller.Touch1Active ? _controller.Touch1PosAndForce.y.ToString("n2") : "0.00",
                    _controller.Touch1Active ? _controller.Touch1PosAndForce.z.ToString("n2") : "0.00",
                    _controller.TouchpadGesture.Type.ToString(),
                    _controller.TouchpadGestureState.ToString());
                }
                else if (_controller.Type == MLInputControllerType.MobileApp &&
                    (_allowedDevices & DeviceTypesAllowed.MobileApp) == DeviceTypesAllowed.MobileApp)
                {
                    _controllerStatsText.text =
                    string.Format("" +
                    "Position:\t<i>{0}</i>\n" +
                    "Rotation:\t<i>{1}</i>\n\n" +
                    "<color=#ffc800>Buttons</color>\n" +
                    "Trigger:\t\t<i>{2}</i>\n" +
                    "Bumper:\t\t<i>{3}</i>\n\n" +
                    "<color=#ffc800>Touchpad</color>\n" +
                    "Touch 1 Location: <i>({4},{5})</i>\n" +
                    "Touch 2 Location: <i>({6},{7})</i>\n\n" +
                    "<color=#ffc800>Gestures</color>\n" +
                    "<i>{8} {9}</i>",

                    "No information available",
                    _controller.Orientation.eulerAngles.ToString("n2"),
                    _controller.TriggerValue.ToString("n2"),
                    _controller.State.ButtonState[(int)MLInputControllerButton.Bumper],
                    _controller.Touch1Active ? _controller.Touch1PosAndForce.x.ToString("n2") : "0.00",
                    _controller.Touch1Active ? _controller.Touch1PosAndForce.y.ToString("n2") : "0.00",
                    _controller.Touch2Active ? _controller.Touch2PosAndForce.x.ToString("n2") : "0.00",
                    _controller.Touch2Active ? _controller.Touch2PosAndForce.y.ToString("n2") : "0.00",
                    _controller.TouchpadGesture.Type.ToString(),
                    _controller.TouchpadGestureState.ToString());
                }
                else
                {
                    _controllerStatsText.text = "Invalid Controller!";
                }
            }
            else
            {
                _controllerStatsText.text = "";
            }
        }

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDestroy()
        {
            MLInput.Stop();
        }
        #endregion
    }
}
