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
using System;
using System.IO;

namespace MagicLeap
{
    /// <summary>
    /// This class demonstrates using the MLMediaPlayer API
    /// </summary>
    public class MediaPlayerExample : MonoBehaviour
    {
        #region Private Variables
        private const float STARTING_VOLUME = 0.2f;

        [SerializeField, Tooltip("MeshRenderer to display media")]
        private MeshRenderer _screen;

        [SerializeField, Tooltip("Pause/Play Button")]
        private MediaPlayerToggle _pausePlayButton;
        private Renderer _pausePlayRenderer;

        [SerializeField, Tooltip("Play Material")]
        private Material _playMaterial;
        [SerializeField, Tooltip("Pause Material")]
        private Material _pauseMaterial;

        [SerializeField, Tooltip("Rewind Button")]
        private MediaPlayerButton _rewindButton;

        [SerializeField, Tooltip("Number of ms to rewind")]
        private int _rewindMS = -10000;

        [SerializeField, Tooltip("Forward Button")]
        private MediaPlayerButton _forwardButton;

        [SerializeField, Tooltip("Number of ms to forward")]
        private int _forwardMS = 10000;

        [SerializeField, Tooltip("Timeline Slider")]
        private MediaPlayerSlider _timelineSlider;

        [SerializeField, Tooltip("Buffer Bar")]
        private Transform _bufferBar;

        [SerializeField, Tooltip("Volume Slider")]
        private MediaPlayerSlider _volumeSlider;

        [SerializeField, Tooltip("Text Mesh for Elapsed Time")]
        private TextMesh _elapsedTime;

        // For online videos, web URLs are accepted
        // For local videos, the asset should be placed in Assets/StreamingAssets/
        //   and the url should be relative to Assets/StreamingAssets/
        [SerializeField, Tooltip("URL of Video to be played")]
        private string _url;

        // DRM-free videos should leave this blank
        [SerializeField, Tooltip("Optional URL of DRM video license server")]
        private string _licenseUrl;

        [SerializeField, Tooltip("Status Text (can be empty)")]
        private TextMesh _statusText;

        private MLMediaPlayer _mediaPlayer;
        private int _totalDurationMs = 0;
        private Button _lastButtonHit;
        private bool _isStreaming = false;
        private bool _wasPlaying = false;
        private bool _updateTime = true;
        #endregion // Private Variables

        #region Unity Methods
        private void Awake()
        {
            if (_screen == null)
            {
                Debug.LogError("Error MediaPlayerExample._screen is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_pausePlayButton == null)
            {
                Debug.LogError("Error MediaPlayerExample._pausePlay is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_playMaterial == null)
            {
                Debug.LogError("Error MediaPlayerExample._playMaterial is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_pauseMaterial == null)
            {
                Debug.LogError("Error MediaPlayerExample._pauseMaterial is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_rewindButton == null)
            {
                Debug.LogError("Error MediaPlayerExample._rewindButton is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_forwardButton == null)
            {
                Debug.LogError("Error MediaPlayerExample._forwardButton is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_timelineSlider == null)
            {
                Debug.LogError("Error MediaPlayerExample._timelineSlider is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_bufferBar == null)
            {
                Debug.LogError("Error MediaPlayerExample._bufferBar is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_volumeSlider == null)
            {
                Debug.LogError("Error MediaPlayerExample._volumeSlider is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_elapsedTime == null)
            {
                Debug.LogError("Error MediaPlayerExample._elapsedTime is not set, disabling script.");
                enabled = false;
                return;
            }

            _mediaPlayer = _screen.gameObject.AddComponent<MLMediaPlayer>();
            _pausePlayRenderer = _pausePlayButton.GetComponent<Renderer>();
        }

        private void Start()
        {
            _mediaPlayer.VideoSource = _url;
            _mediaPlayer.LicenseServer = _licenseUrl;
            _mediaPlayer.PrepareVideo();

            _volumeSlider.Value = STARTING_VOLUME;
            _elapsedTime.text = "--:--:--";
            _timelineSlider.enabled = false;
            _timelineSlider.Value = 0;

            _mediaPlayer.OnPause += HandlePause;
            _mediaPlayer.OnPlay += HandlePlay;
            _mediaPlayer.OnStop += HandleStop;
            _mediaPlayer.OnEnded += HandleEnded;
            _mediaPlayer.OnSeekUpdate += HandleSeek;
            _mediaPlayer.OnBufferingUpdate += HandleBufferUpdate;
            _mediaPlayer.OnError += HandleError;
            _mediaPlayer.OnInfo += HandleInfo;

            _pausePlayButton.OnToggle += PlayPause;
            _rewindButton.OnControllerTriggerDown += Rewind;
            _forwardButton.OnControllerTriggerDown += FastForward;
            _timelineSlider.OnValueChanged += Seek;
            _volumeSlider.OnValueChanged += SetVolume;
        }

        private void OnDestroy()
        {
            _mediaPlayer.OnPause -= HandlePause;
            _mediaPlayer.OnPlay -= HandlePlay;
            _mediaPlayer.OnStop -= HandleStop;
            _mediaPlayer.OnEnded -= HandleEnded;
            _mediaPlayer.OnSeekUpdate -= HandleSeek;
            _mediaPlayer.OnBufferingUpdate -= HandleBufferUpdate;
            _mediaPlayer.OnError -= HandleError;
            _mediaPlayer.OnInfo -= HandleInfo;

            _pausePlayButton.OnToggle -= PlayPause;
            _rewindButton.OnControllerTriggerDown -= Rewind;
            _forwardButton.OnControllerTriggerDown -= FastForward;
            _timelineSlider.OnValueChanged -= Seek;
            _volumeSlider.OnValueChanged -= SetVolume;
        }

        private void Update()
        {
            if ((_isStreaming && _updateTime) || _mediaPlayer.IsPlaying)
            {
                _timelineSlider.Value = _mediaPlayer.AnimationPosition;
                UpdateElapsedTime(_mediaPlayer.GetElapsedTimeMs());
            }
        }
        #endregion // Unity Methods

        #region Private Methods
        /// <summary>
        /// Function to update the elapsed time text
        /// </summary>
        /// <param name="elapsedTimeMs">Elapsed time in milliseconds</param>
        private void UpdateElapsedTime(long elapsedTimeMs)
        {
            TimeSpan timeSpan = new TimeSpan(elapsedTimeMs * TimeSpan.TicksPerMillisecond);
            _elapsedTime.text = String.Format("{0}:{1}:{2}",
                timeSpan.Hours.ToString(), timeSpan.Minutes.ToString("00"), timeSpan.Seconds.ToString("00"));
        }
        #endregion // Private Methods

        #region Event Handlers
        /// <summary>
        /// Event Handler when Media Player has reached the end of the media
        /// </summary>
        private void HandleEnded()
        {
            _pausePlayRenderer.material = _playMaterial;
        }

        /// <summary>
        /// Event Handler when the Media Player is stopped
        /// </summary>
        private void HandleStop()
        {
            _pausePlayRenderer.material = _playMaterial;
            _timelineSlider.enabled = false;
            _elapsedTime.text = "--:--:--";
        }

        /// <summary>
        /// Event Handler when the Media Player starts Playing
        /// </summary>
        /// <param name="durationMs">Total Duration of the media being played</param>
        private void HandlePlay(int durationMs)
        {
            _totalDurationMs = durationMs;
            _pausePlayRenderer.material = _pauseMaterial;
            _timelineSlider.enabled = true;
        }

        /// <summary>
        /// Event Handler when the Media Player is paused
        /// </summary>
        private void HandlePause()
        {
            _pausePlayRenderer.material = _playMaterial;
        }

        /// <summary>
        /// Event Handler when either versions Seek() is called
        /// Note: This is only called once per call to Seek()
        /// </summary>
        /// <param name="percent">Percent of whole duration (0.0f to 1.0f)</param>
        private void HandleSeek(float percent)
        {
            _timelineSlider.Value = percent;
            UpdateElapsedTime((long)(percent * _totalDurationMs));

            if (_isStreaming)
            {
                _updateTime = false;
                if (_mediaPlayer.IsPlaying)
                {
                    _wasPlaying = true;
                    _mediaPlayer.Pause();
                }
            }
        }

        /// <summary>
        /// Event handler when buffer gets updated. This is only called when the video is streaming.
        /// </summary>
        /// <param name="percent">Percent of the whole duration, [0, 1]</param>
        private void HandleBufferUpdate(float percent)
        {
            Vector3 barScale = _bufferBar.localScale;
            barScale.x = percent;
            _bufferBar.localScale = barScale;
        }

        /// <summary>
        /// Event Handler when an error occurs
        /// </summary>
        /// <param name="error">The MLMediaPlayerResult</param>
        /// <param name="errorString">String version of the error</param>
        private void HandleError(MLMediaPlayerResult error, string errorString)
        {
            Debug.LogError("MediaPlayerExample::HandleError " + errorString);
            if (_statusText != null)
            {
                _statusText.text = errorString;
            }
        }

        /// <summary>
        /// Event Handler for miscellaneous informational events
        /// </summary>
        /// <param name="info">The event that occurred</param>
        /// <param name="extra">The data associated with the event (if any), otherwise, 0</param>
        private void HandleInfo(MLMediaPlayerInfo info, int extra)
        {
            Debug.Log("MediaPlayerExample::HandleInfo " + info.ToString());
            if (info == MLMediaPlayerInfo.BufferingEnd)
            {
                // this is guaranteed to be called only when we're streaming
                // Note: we can't guarantee if this is last BufferingEnd event
                _updateTime = true;
                if (_wasPlaying)
                {
                    _mediaPlayer.Play();
                    _wasPlaying = false;
                }
            }
            else if (info == MLMediaPlayerInfo.NetworkBandwidth)
            {
                _isStreaming = true;
                // extra would contain bandwidth in kbps
            }
        }

        /// <summary>
        /// Handler when Play/Pause Toggle is triggered.
        /// See HandlePlay() and HandlePause() for more info
        /// </summary>
        /// <param name="shouldPlay">True when resuming, false when should pause</param>
        private void PlayPause(bool shouldPlay)
        {
            if (_mediaPlayer != null)
            {
                if (!shouldPlay && _mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Pause();
                }
                else if (shouldPlay && !_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Play();
                }
            }
        }

        /// <summary>
        /// Handler when Stop button has been triggered. See HandleStop() for more info.
        /// </summary>
        private void Stop()
        {
            _mediaPlayer.Stop();
        }

        /// <summary>
        /// Handler when Rewind button has been triggered.
        /// Moves the play head backward.
        /// </summary>
        /// <param name="triggerReading">Unused parameter</param>
        private void Rewind(float triggerReading)
        {
            // Note: this calls the int version of seek.
            // This moves the playhead by an offset in ms
            _mediaPlayer.Seek(_rewindMS);
        }

        /// <summary>
        /// Handler when Forward button has been triggered.
        /// Moves the play head forward.
        /// </summary>
        /// <param name="triggerReading">Unused parameter</param>
        private void FastForward(float triggerReading)
        {
            // Note: this calls the int version of seek.
            // This moves the playhead by an offset in ms
            _mediaPlayer.Seek(_forwardMS);
        }

        /// <summary>
        /// Handler when Timeline Slider has changed value.
        /// Moves the play head to a specific percentage of the whole duration.
        /// </summary>
        /// <param name="sliderValue">Normalized slider value</param>
        private void Seek(float sliderValue)
        {
            if (Mathf.Approximately(sliderValue, _mediaPlayer.AnimationPosition))
            {
                return;
            }

            // Note: this calls the float version of seek.
            // This moves the playhead to a percentage of the whole duration.
            _mediaPlayer.Seek(sliderValue);
        }

        /// <summary>
        /// Handler when Volume Sider has changed value.
        /// </summary>
        /// <param name="sliderValue">Normalized slider value</param>
        private void SetVolume(float sliderValue)
        {
            _mediaPlayer.SetVolume(sliderValue);
        }
        #endregion // Event Handlers
    }
}

