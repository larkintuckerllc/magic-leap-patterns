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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    ///  Shows all Persistent Points in the world around you.
    /// </summary>
    public class PersistentPointsVisualizer : MonoBehaviour
    {
        #region Variables
        [SerializeField, Tooltip("Prefab to represent a PCF visually")]
        private GameObject _representativePrefab;
        private List<GameObject> _pcfObjs = new List<GameObject>();
        #endregion

        #region functions
        /// <summary>
        /// Start this instance.
        /// </summary>
        void Start()
        {
            MLResult result = MLPersistentStore.Start();
            if (!result.IsOk)
            {
                SetError("Failed to start persistent store. Disabling component");
                enabled = false;
                return;
            }
            result = MLPersistentCoordinateFrames.Start();
            if (!result.IsOk)
            {
                MLPersistentStore.Stop();
                SetError("Failed to start coordinate frames system. disabling component");
                enabled = false;
                return;
            }

            if (_representativePrefab == null)
            {
                SetError("Error: _representativePrefab must be set");
                enabled = false;
                return;
            }

            List<MLPCF> pcfList;
            result = MLPersistentCoordinateFrames.GetAllPCFs(out pcfList, int.MaxValue);
            if (!result.IsOk)
            {
                MLPersistentStore.Stop();
                MLPersistentCoordinateFrames.Stop();
                SetError(result.ToString());
                enabled = false;
                return;
            }

            TryShowingAllPCFs(pcfList);
        }

        /// <summary>
        /// Sets the error.
        /// </summary>
        /// <param name="errorString">Error string.</param>
        void SetError(string errorString)
        {
            Debug.LogError(errorString);
        }

        /// <summary>
        /// Tries the showing all PCF.
        /// </summary>
        /// <param name="pcfList">Pcf list.</param>
        void TryShowingAllPCFs(List<MLPCF> pcfList)
        {
            foreach (MLPCF pcf in pcfList)
            {
                if (pcf.CurrentResult == MLResultCode.Pending)
                {
                    MLPersistentCoordinateFrames.GetPCFPosition(pcf, (r, p) =>
                    {
                        if (r.IsOk)
                        {
                            AddPCFObject(p);
                        }
                        else
                        {
                            SetError("failed to get position for pcf : " + p);
                        }
                    });
                }
                else
                {
                    AddPCFObject(pcf);
                }
            }
        }

        /// <summary>
        /// Creates the PCF game object.
        /// </summary>
        /// <param name="pcf">Pcf.</param>
        void AddPCFObject(MLPCF pcf)
        {
            if(!_pcfObjs.Contains(pcf.GameObj))
            {
                GameObject repObj = Instantiate(_representativePrefab, Vector3.zero, Quaternion.identity);
                repObj.name = pcf.GameObj.name;
                repObj.transform.parent = pcf.GameObj.transform;
                _pcfObjs.Add(pcf.GameObj);
            }
        }

        /// <summary>
        /// Clean up
        /// </summary>
        void OnDestroy()
        {
            if (MLPersistentStore.IsStarted)
            {
                MLPersistentStore.Stop();
            }
            if (MLPersistentCoordinateFrames.IsStarted)
            {
                MLPersistentCoordinateFrames.Stop();
            }

            foreach (GameObject go in _pcfObjs)
            {
                Destroy(go);
            }
        }
        #endregion
    }
}
