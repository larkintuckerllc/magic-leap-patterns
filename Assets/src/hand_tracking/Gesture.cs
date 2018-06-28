using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.XR.MagicLeap;

public class Gesture : MonoBehaviour
{

    private static float GESTURE_CONFIDENCE_THRESHOLD = 0.5f;
    private static Color COLOR_BOTH = Color.magenta;
    private static Color COLOR_LEFT = Color.red;
    private static Color COLOR_RIGHT = Color.blue;
    private static Color COLOR_NONE = Color.white;
    public MLHandKeyPose gesture;
    private bool _first = true;
    private bool _lastLeftGesture = false;
    private bool _lastRightGesture = false;
    private Image _image;

    private void Awake()
    {
        if (!(MLHands.Start().IsOk))
        {
            Debug.LogError("Error GesturesKeypointVisualizer starting MLHands, disabling script.");
            enabled = false;
            return;
        }
        _image = GetComponent<Image>();
    }
    void Update()
    {
        var enabledPoses = new MLHandKeyPose[] {
            MLHandKeyPose.C,
            MLHandKeyPose.Finger,
            MLHandKeyPose.Fist,
            MLHandKeyPose.L,
            MLHandKeyPose.Ok,
            MLHandKeyPose.OpenHandBack,
            MLHandKeyPose.Pinch,
            MLHandKeyPose.Thumb,
        };
        MLHands.KeyPoseManager.EnableKeyPoses(enabledPoses, true);
        var handLeft = MLHands.Left;
        var handRight = MLHands.Right;
        var leftGesture = false;
        var rightGesture = false;
        if (handLeft.KeyPose == gesture && handLeft.KeyPoseConfidence > GESTURE_CONFIDENCE_THRESHOLD)
        {
            leftGesture = true;
        }
        if (handRight.KeyPose == gesture && handRight.KeyPoseConfidence > GESTURE_CONFIDENCE_THRESHOLD)
        {
            rightGesture = true;
        }
        if (_first)
        {
            _first = false;
            _lastLeftGesture = !leftGesture;
            _lastRightGesture = !rightGesture;
        }
        if (leftGesture == _lastLeftGesture && rightGesture == _lastRightGesture)
        {
            return;
        }
        _lastLeftGesture = leftGesture;
        _lastRightGesture = rightGesture;
        if (leftGesture && rightGesture)
        {
            _image.color = COLOR_BOTH;
        }
        else if (leftGesture)
        {
            _image.color = COLOR_LEFT;
        }
        else if (rightGesture)
        {
            _image.color = COLOR_RIGHT;
        }
        else
        {
            _image.color = COLOR_NONE;
        }
    }
    private void OnDestroy()
    {
        MLHands.Stop();
    }
}