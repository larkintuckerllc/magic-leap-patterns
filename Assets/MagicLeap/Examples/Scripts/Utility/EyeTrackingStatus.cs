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

public class EyeTrackingStatus : MonoBehaviour
{
    [SerializeField, Tooltip("Displays information about eye tracking, including comfort, and depth violations.")]
    private Text _statusLabel;

    [SerializeField, Tooltip("An image that represent the current fixation comfort status.")]
    private Image _statusImage;

    private void Update()
    {
        if (MLEyes.IsStarted)
        {
            _statusLabel.text = string.Format(
                        "<color=#ffc800>Eye Calibration Status</color>\n<i>{0}</i>" +
                        "\n\nFixation Depth Uncomfortable:\t\t{1}" +
                        "\nUncomfortable Remaining Time:\t\t{2:0.00}" +
                        "\nFixation Depth Violated:\t\t\t\t\t\t{3}",
                        MLEyes.CalibrationStatus.ToString(),
                        MLEyes.IsFixationDepthUncomfortable,
                        MLEyes.RemainingTimeAtUncomfortableDepth,
                        MLEyes.HasFixationDepthViolationOccurred);

            if (MLEyes.HasFixationDepthViolationOccurred)
            {
                _statusImage.color = Color.red;
            }
            else
            {
                if (MLEyes.IsFixationDepthUncomfortable)
                {
                    _statusImage.color = Color.yellow;
                }
                else
                {
                    _statusImage.color = Color.green;
                }
            }
        }
    }
}
