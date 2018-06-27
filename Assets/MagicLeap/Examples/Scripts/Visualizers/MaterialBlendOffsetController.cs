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

namespace MagicLeap
{
    /// <summary>
    /// Controller to modify the speeds of the offsets of the main and blend texture
    /// </summary>
    public class MaterialBlendOffsetController : MaterialController
    {
        #region Private Variables
        private Vector2 _mainTexOffset;
        private Vector2 _blendTexOffset;

        private const float MAX_SPEED =  1.0f;
        private const float BLENDTEX_SPEED_FACTOR = 0.25f;

        [SerializeField, Range(-MAX_SPEED, MAX_SPEED), Tooltip("Speed of the X Offset of the texture")]
        private float _xOffsetSpeed = 0.1f;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validate material, store initial offset
        /// </summary>
        void Start ()
        {
            if (!_material.HasProperty("_MainTex"))
            {
                Debug.LogError("Error: MaterialBlendOffsetController._material does not have _MainTex (2D Texture), disabling script.");
                enabled = false;
                return;
            }
            if (!_material.HasProperty("_BlendTex"))
            {
                Debug.LogError("Error: MaterialBlendOffsetController._material does not have _BlendTex (2D Texture), disabling script.");
                enabled = false;
                return;
            }

            _mainTexOffset = _material.GetTextureOffset("_MainTex");
            _blendTexOffset = _material.GetTextureOffset("_BlendTex");
        }

        /// <summary>
        /// Increment offset over time
        /// </summary>
        void Update ()
        {
            _mainTexOffset.x += _xOffsetSpeed * Time.deltaTime;
            _blendTexOffset.x += _xOffsetSpeed * Time.deltaTime * BLENDTEX_SPEED_FACTOR;
            _material.SetTextureOffset("_MainTex", _mainTexOffset);
            _material.SetTextureOffset("_BlendTex", _blendTexOffset);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adjust the speed
        /// </summary>
        /// <param name="factor">Increment to the speed</param>
        public override void OnUpdateValue(float factor)
        {
            _xOffsetSpeed += factor * 0.5f;
            _xOffsetSpeed = Mathf.Clamp(_xOffsetSpeed, -MAX_SPEED, MAX_SPEED);
        }
        #endregion
    }
}
