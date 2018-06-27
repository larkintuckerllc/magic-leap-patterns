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

/// <summary>
/// This class listens for eye tracking fixation violations and
/// adjust the MainCamera's near clip plane to accommodate.
/// </summary>
public class EyeTrackingFixationComfort : MonoBehaviour
{
    #region Private Variables
    private Camera _mainCamera;
    private bool _depthViolation = false;
    #endregion

    #region Unity Methods
    /// <summary>
    /// Set the default min clip distance.
    /// </summary>
    private void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("Error EyeTrackingFixationComfort._camera is null, disabling script.");
            enabled = false;
            return;
        }

        MLResult result = MLEyes.Start();
        if (!result.IsOk)
        {
            Debug.LogError("Error starting MLEyes, disabling script.");
            enabled = false;
            return;
        }

        // Register Listeners.
        MLEyes.OnFixationDepthViolationOccurred += HandleOnFixationViolationOccurred;
        MLEyes.OnFixationDepthViolationCleared += HandleOnFixationViolationCleared;

        // Set the initial minimum distance.
        _mainCamera.nearClipPlane = MagicLeapDevice.MinimumNearClipDistance;
    }

    /// <summary>
    /// Unregister listeners.
    /// </summary>
    private void OnDestroy()
    {
        // Unregister Listeners.
        MLEyes.OnFixationDepthViolationOccurred -= HandleOnFixationViolationOccurred;
        MLEyes.OnFixationDepthViolationCleared -= HandleOnFixationViolationCleared;

        if (MLEyes.IsStarted)
        {
            MLEyes.Stop();
        }
    }

    private void Update()
    {
        if (!_depthViolation)
        {
            // Gradually adjust the near clip plane further away as the remaining time decreases.
            _mainCamera.nearClipPlane = Mathf.Lerp(MagicLeapDevice.DefaultNearClipDistance, MagicLeapDevice.MinimumNearClipDistance, MLEyes.RemainingTimeAtUncomfortableDepth / MLEyes.MaximumTimeAtUncomfortableDepth);
        }
    }

    /// <summary>
    /// This event occurs if the user has fixated on a point closer than
    /// the min clip distance for longer than recommended within the last minute.
    /// </summary>
    private void HandleOnFixationViolationOccurred()
    {
        _depthViolation = true;
        _mainCamera.nearClipPlane = MagicLeapDevice.DefaultNearClipDistance;
    }

    /// <summary>
    /// This event occurs once the fixation violation has been cleared.
    /// </summary>
    private void HandleOnFixationViolationCleared()
    {
        _depthViolation = false;
        _mainCamera.nearClipPlane = MagicLeapDevice.MinimumNearClipDistance;
    }
    #endregion
}
