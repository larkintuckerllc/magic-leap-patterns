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
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This is a component you can use to make a specific game object a persistent
    /// anchor/point in space. This component would try to restore itself on Start and
    /// will notify the listener if it's restored correctly or not. If this is the first time
    /// it would automatically look for the right real world PCF to attach itself to. You can simply put
    /// your content you want to persist under the game object with this behavior attached to it.
    /// PLEASE NOTE: Once the PersistentPoint is found /restored it's transform is locked. You
    /// cannot move this persistent object at all.
    /// </summary>
    public class MLPersistentPoint : MonoBehaviour
    {
        #region public vars        
        /// <summary>
        /// Every persistent point in your project must have a unique Id
        /// </summary>

        [Tooltip("Unique id for this persistent point. If not provided the name of the GameObject would be used")]
        public string UniqueId;

        /// <summary>
        /// This event is raised when the persistent point is ready and available.
        /// </summary>
        public event System.Action OnAvailable;

        /// <summary>
        /// This event happens when there are errors.
        /// </summary>
        public event System.Action<MLResult> OnError;

        /// <summary>
        /// Gets the binding.
        /// </summary>
        /// <value>The binding.</value>
        public MLContentBinding Binding { get; private set; }

        /// <summary>
        /// The max real world PCFs to bind to. Tweak this number to control the
        /// number of neighboring PCfs to attach the persitent point to.
        /// Higher the number the more resilient it gets but you pay more cost in storage space
        /// </summary>
        [Tooltip("The max real world PCFs to bind to. Higher the number the more resilient it gets but you pay more cost in storage space")]
        public int MaxPCFsToBindTo = 3;

        #endregion
        #region private variables and types
        /// <summary>
        /// State.
        /// </summary>
        enum State
        {
            Unknown,
            RestoreBinding,
            BindToAllPCFs,
            BindingComplete,
            Locked
        }

        /// <summary>
        /// Represents the current state or restoration/binding
        /// </summary>
        private State _state = State.Unknown;

        /// <summary>
        /// locked transform
        /// </summary>
        private Transform _lockedTransform;

        private List<MLPCF> _allPCFs;
        #endregion

        #region functions
        /// <summary>
        /// Tries to restore the binding or find closest PCF. Note various errors
        /// can be shown during this step based on the state of the low level systems.
        /// </summary>
        void Start()
        {
            SetChildrenActive(false);
            _lockedTransform = gameObject.transform;

            if (string.IsNullOrEmpty(UniqueId))
            {
                Debug.LogWarning("Unique Id is empty will try to use game object's name. It's good to provide a unique id for virtual objects to avoid weird behavior.");
                if (string.IsNullOrEmpty(gameObject.name))
                {
                    SetError(new MLResult(MLResultCode.UnspecifiedFailure, "Either UniqueId or name should be non empty. Disabling component"));
                    enabled = false;
                    return;
                }
                UniqueId = gameObject.name;
            }
            else
            {
                gameObject.name = UniqueId;
            }

            MLResult result = MLPrivileges.Start();
            if (result.IsOk)
            {
                RequestPrivilege();
            }
            else
            {
                Debug.LogError("Privilege Error: failed to startup");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Requests privileges and calls the callback when the privilege request is
        /// complete.
        /// <param name="callback">Callback function to call when the privilege is granted </param>
        /// </summary>
        void RequestPrivilege()
        {
            Debug.Log("Requesting required privileges");
            MLResult result = MLPrivileges.RequestPrivilegeAsync(MLPrivilegeId.PwFoundObjRead, HandlePrivilegeAsyncRequest);
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("{0} Privilege Request Error: {1}", MLPrivilegeId.PwFoundObjRead, result);
                return;
            }
        }

        ///<summary>
        /// Starts the restoration process.
        /// </summary>
        void StartRestore()
        {
            MLResult result = MLPersistentStore.Start();
            if (!result.IsOk)
            {
                SetError(result);
                enabled = false;
                return;
            }

            result = MLPersistentCoordinateFrames.Start();
            if (!result.IsOk)
            {
                MLPersistentStore.Stop();
                SetError(result);
                enabled = false;
                return;
            }

            result = MLPersistentCoordinateFrames.GetAllPCFs(out _allPCFs, MaxPCFsToBindTo);
            if (!result.IsOk)
            {
                MLPersistentStore.Stop();
                MLPersistentCoordinateFrames.Stop();
                SetError(result);
                enabled = false;
                return;
            }

            StartCoroutine(TryRestoreBinding());
        }
        /// <summary>
        /// Sets the children active.
        /// </summary>
        /// <param name="active">If set to <c>true</c> active.</param>
        void SetChildrenActive(bool active)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// Utility function that shows the error and also raises the OnErrorEvent
        /// </summary>
        /// <param name="result">result to be shown.</param>
        void SetError(MLResult result)
        {
            Debug.LogError(result);
            if (OnError != null)
            {
                OnError(result);
            }
        }

        /// <summary>
        /// Tries the restore binding.
        /// </summary>
        /// <returns>The restore binding.</returns>
        IEnumerator TryRestoreBinding()
        {
            string suffix = "";
            int count = 0;
            string prefix = gameObject.name;
            for (int i = 0; i < MaxPCFsToBindTo; ++i)
            {
                gameObject.name = prefix + suffix;
                Debug.Log("Trying to look for persistent point attached to :" + gameObject.name);
                yield return StartCoroutine(RestoreBinding(gameObject.name));
                if (_state == State.BindingComplete)
                {
                    //in short binding wasn't found 
                    if (Binding == null || Binding.PCF == null || Binding.PCF.CurrentResult != MLResultCode.Ok)
                    {
                        suffix = "-" + count;
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (Binding != null && Binding.PCF != null && Binding.PCF.CurrentResult == MLResultCode.Ok)
            {
                SetAvailable();
            }
            else
            {
                SetError(new MLResult(MLResultCode.Pending, "Failed to find a suitable PCF"));
            }
        }

        /// <summary>
        /// Tries to restore the binding from persistent storage and PCF system
        /// </summary>
        IEnumerator RestoreBinding(string objId)
        {
            _state = State.RestoreBinding;

            if (MLPersistentStore.Contains(objId))
            {
                MLContentBinding binding;

                MLResult result = MLPersistentStore.Load(objId, out binding);
                if (!result.IsOk)
                {
                    SetError(result);
                    _state = State.BindingComplete;
                }
                else
                {
                    Binding = binding;
                    Debug.Log("binding result : " + Binding.PCF.CurrentResult);
                    Binding.GameObject = this.gameObject;
                    MLContentBinder.Restore(Binding, HandleBindingRestore);
                }
            }
            else
            {
                BindToAllPCFs();
            }

            while (_state != State.BindingComplete)
            {
                yield return null;
            }
            yield break;
        }

        /// <summary>
        /// Handler for binding restore 
        /// </summary>
        /// <param name="contentBinding">Content binding.</param>
        /// <param name="resultCode">Result code.</param>
        void HandleBindingRestore(MLContentBinding contentBinding, MLResult result)
        {
            _state = State.BindingComplete;
            Debug.Log("binding result : " + contentBinding.PCF.CurrentResult);
            if (!result.IsOk)
            {
                MLPersistentStore.DeleteBinding(contentBinding);
                Debug.LogFormat("Failed to restore : {0} - {1}. Result code:", gameObject.name, contentBinding.PCF.CFUID, result.Code);
            }
        }

        /// <summary>
        /// Finds the closest pcf for this persistent point.
        /// </summary>
        void BindToAllPCFs()
        {
            _state = State.BindToAllPCFs;
            string suffix = "";
            int count = 0;

            // In the loop below we try to associate the persitent point with not only
            // the closest but all pcfs in the surrounding. This will increase the probablilty
            // of restoration on reboots. It's costly in terms of disk space so we will limit it to 
            // a max
            foreach (MLPCF pcf in _allPCFs)
            {
                string objectName = gameObject.name + suffix;
                var returnResult = MLPersistentCoordinateFrames.GetPCFPosition(pcf, (result, returnPCF) =>
                {
                    if (result.IsOk && pcf.CurrentResult == MLResultCode.Ok)
                    {
                        Debug.Log("binding to PCF: " + pcf.CFUID);

                        Binding = MLContentBinder.BindToPCF(objectName, gameObject, pcf);
                        MLPersistentStore.Save(Binding);
                    }
                    else
                    {
                        Debug.LogWarningFormat("Failed to find the position for PCF {0}", returnPCF.CFUID);
                    }
                });
                if (!returnResult.IsOk)
                {
                    Debug.LogError("Failed to GetPCF");
                    break;
                }
                suffix = "-" + count;
                count++;
            }

            _state = State.BindingComplete;
        }

        /// <summary>
        /// Sets the available.
        /// </summary>
        void SetAvailable()
        {
            _state = State.Locked;
            _lockedTransform.transform.position = Binding.GameObject.transform.position;
            _lockedTransform.transform.rotation = Binding.GameObject.transform.rotation;

            Debug.Log("Transform locked for Persistent point : " + gameObject.name);
            if (OnAvailable != null)
            {
                OnAvailable();
            }
            SetChildrenActive(true);
        }

        /// <summary>
        /// Update this instance.
        /// </summary>
        void Update()
        {
            if (_state == State.Locked)
            {
                transform.position = _lockedTransform.position;
                transform.rotation = _lockedTransform.rotation;
            }
        }

        /// <summary>
        /// Shuts down the systems started in Start
        /// </summary>
        void OnDestroy()
        {
            if (MLPersistentCoordinateFrames.IsStarted)
            {
                MLPersistentCoordinateFrames.Stop();
            }
            if (MLPersistentStore.IsStarted)
            {
                MLPersistentStore.Stop();
            }

            MLPrivileges.Stop();
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Handles the result that is received from the query to the Privilege API.
        /// <param name="result">The resulting status of the query</param>
        /// <param name="privilegeId">The privilege being queried</param>
        /// </summary>
        private void HandlePrivilegeAsyncRequest(MLResult result, MLPrivilegeId privilegeId)
        {
            if ((MLPrivilegesResult)result.Code == MLPrivilegesResult.Granted)
            {
                Debug.LogFormat("{0} Privilege Granted", privilegeId);
                StartRestore();
            }
            else
            {
                Debug.LogErrorFormat("{0} Privilege Error: {1}", privilegeId, result);
            }
        }
        #endregion
    }
}