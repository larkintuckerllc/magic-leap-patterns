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
    /// Material Controller to modify tiling
    /// </summary>
    public class MaterialTilingController : MaterialController
    {
        #region Private Variables
        private Vector2 _texTiling;
        private const float MIN_TILING = 0.25f;
        private const float MAX_TILING = 4.0f;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validate material, store initial tiling
        /// </summary>
        void Start()
        {
            if (!_material.HasProperty("_MainTex"))
            {
                Debug.LogError("Error: MaterialTilingController._material does not have _MainTex (2D Texture), disabling script.");
                enabled = false;
                return;
            }
            _texTiling = _material.GetTextureScale("_MainTex");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adjust the tiling
        /// </summary>
        /// <param name="factor">Amount of tiling added</param>
        public override void OnUpdateValue(float factor)
        {
            _texTiling += new Vector2(factor, factor);
            _texTiling.x = Mathf.Clamp(_texTiling.x, MIN_TILING, MAX_TILING);
            _texTiling.y = Mathf.Clamp(_texTiling.y, MIN_TILING, MAX_TILING);
            _material.SetTextureScale("_MainTex", _texTiling);
        }
        #endregion
    }
}
