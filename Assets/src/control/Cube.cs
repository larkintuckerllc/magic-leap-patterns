using UnityEngine;
using UnityEngine.Experimental.XR.MagicLeap;

public class Cube : MonoBehaviour
{

    private static int BUTTON_ACTIVE = 1;
    private static int BUTTON_INACTIVE = 0;
    private static Color BUTTON_ACTIVE_COLOR = Color.blue;
    private static Color BUTTON_INACTIVE_COLOR = Color.white;
    private MLInputController _controller;
    private Renderer _renderer;
    private bool _first = true;
    private bool _active = false;

    void Awake()
    {
        if (!(MLInput.Start().IsOk))
        {
            Debug.LogError("Error Cube starting MLInput, disabling script.");
            enabled = false;
            return;
        }
        _controller = MLInput.GetController(MLInput.Hand.Left);
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (!_controller.Connected)
        {
            return;
        }
        var button = (int)MLInputControllerButton.Bumper;
        var buttonState = _controller.State.ButtonState[button];
        if (_first && buttonState == BUTTON_ACTIVE)
        {
            _first = false;
            _active = false;
        }
        if (_first && buttonState == BUTTON_INACTIVE)
        {
            _first = false;
            _active = true;
        }
        if (!_active && buttonState == BUTTON_ACTIVE)
        {
            _active = true;
            _renderer.material.color = BUTTON_ACTIVE_COLOR;
        }
        if (_active && buttonState == BUTTON_INACTIVE)
        {
            _active = false;
            _renderer.material.color = BUTTON_INACTIVE_COLOR;
        }
    }

    void OnDestroy()
    {
        MLInput.Stop();
    }
}
