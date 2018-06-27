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
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    [DisallowMultipleComponent]
    public class MediaPlayerSlider : MediaPlayerButton
    {
        #region Public Events
        public System.Action<float> OnValueChanged;
        #endregion

        #region Private Variables
        [SerializeField, Tooltip("Local position of beginning point")]
        private Vector3 _beginRelative;
        private Vector3 _begin;

        [SerializeField, Tooltip("Local position of ending point")]
        private Vector3 _endRelative;
        private Vector3 _end;

        [SerializeField, Tooltip("Handle of the Slider")]
        private Transform _handle;

        private float _value = 0;
        #endregion // Private Variables

        #region Public Properties
        /// <summary>
        /// Value represents a percentage, clamped in the range [0, 1] inclusive
        /// Invokes OnValueChanged if needed
        /// </summary>
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                value = Mathf.Clamp01(value);
                if (Mathf.Approximately(value, _value))
                {
                    return;
                }

                _value = value;
                _handle.position = Vector3.Lerp(_begin, _end, _value);
                if (OnValueChanged != null)
                {
                    OnValueChanged(_value);
                }
            }
        }
        #endregion // Public Properties

        #region Unity Methods
        private void Awake()
        {
            if (_handle == null)
            {
                Debug.LogError("Error MediaPlayerSlider._handle not set, disabling script.");
                enabled = false;
                return;
            }
            CalculateSliderDetails();
        }

        private void OnEnable()
        {
            _handle.gameObject.SetActive(true);

            OnControllerDrag += HandleControllerDrag;
        }

        private void OnDisable()
        {
            _handle.gameObject.SetActive(false);

            OnControllerDrag -= HandleControllerDrag;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.TransformPoint(_beginRelative), 0.02f);
            Gizmos.DrawWireSphere(transform.TransformPoint(_endRelative), 0.02f);
        }

        private void Update()
        {
            if (transform.hasChanged)
            {
                CalculateSliderDetails();
            }
        }
        #endregion // Unity Methods

        #region Private Methods
        /// <summary>
        /// Calculates the slider length and direction to be used when updating the slider.
        /// This method should be called again when _begin or _end changes.
        /// </summary>
        private void CalculateSliderDetails()
        {
            _begin = transform.TransformPoint(_beginRelative);
            _end = transform.TransformPoint(_endRelative);
        }
        #endregion // Private Methods

        #region Event Handlers
        /// <summary>
        /// Find the point on the slider that's closest to the line defined by
        /// the controller's position and direction.
        /// </summary>
        /// <param name="controller">Information on the controller</param>
        private void HandleControllerDrag(MLInputController controller)
        {
            // Line 1 is the Controller's ray
            Vector3 P1 = controller.Position;
            Vector3 V1 = controller.Orientation * Vector3.forward;

            // Line 2 is the slider
            Vector3 P2 = _begin;
            Vector3 V2 = _end - _begin;

            Vector3 deltaP = P2 - P1;
            float V21 = Vector3.Dot(V2, V1);
            float V11 = Vector3.Dot(V1, V1);
            float V22 = Vector3.Dot(V2, V2);

            float tNum = Vector3.Dot(deltaP, V21 * V1 - V11 * V2);
            float tDen = V22 * V11 - V21 * V21;

            // Lines are parallel
            if (Mathf.Approximately(tDen, 0.0f))
            {
                return;
            }

            // closest point on Line 2 to Line 1 is P2 + t * V2
            // but we're only concerned with t
            Value = tNum / tDen;
        }
        #endregion // Event Handlers
    }
}
