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

namespace MagicLeap
{
    /// <summary>
    /// Parent for the specific material controller
    /// </summary>
    public abstract class MaterialController : MonoBehaviour
    {
        #region Protected Variables
        [SerializeField, Tooltip("Material to be manipulated for all instances")]
        protected Material _material;

        [SerializeField, Tooltip("Helper text")]
        protected Text _statusText;
        [SerializeField, Tooltip("Text to show when viewing an object with this controller")]
        private string _textOnView;
        #endregion

        #region Properties
        public Material ReferenceMaterial
        {
            get
            {
                return _material;
            }
        }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validate variables
        /// </summary>
        void Awake()
        {
            if (null == _material)
            {
                Debug.LogError("MaterialController._material not set, disabling script", this);
                enabled = false;
                return;
            }
            if (null == _statusText)
            {
                Debug.LogError("MaterialController._statusText not set, disabling script.", this);
                enabled = false;
                return;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Overridable method called when the user holds the bumper while radially scrolling and looking at a plane
        /// </summary>
        /// <param name="value">The change in radial scroll angle. Possible multiplied by a constant factor.</param>
        public abstract void OnUpdateValue(float value);

        /// <summary>
        /// Inform user on what they're looking at
        /// </summary>
        public void UpdateTextOnView()
        {
            _statusText.text = _textOnView;
        }
        #endregion
    }
}
