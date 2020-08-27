using UnityEngine;
using UnityHelpers;
using UnityEngine.N3DS;

public class UserToValueManager : MonoBehaviour
{
    [RequireInterface(typeof(IValueManager))]
    public GameObject _controlledObject;
    private IValueManager InputDevice { get { if (_inputDevice == null) _inputDevice = _controlledObject.GetComponent<IValueManager>(); return _inputDevice; } }
    private IValueManager _inputDevice;

    private bool left, right, up, down;
    private bool b;
    
    void Update()
    {        
        // InputDevice.SetAxis("Horizontal", Input.GetAxis("Horizontal"));
        // InputDevice.SetAxis("Vertical", Input.GetAxis("Vertical"));
        // InputDevice.SetToggle("ButtonA", Input.GetKey(KeyCode.Space));

        PollValues();

        Vector2 dpad = new Vector2((left ? -1 : 0) + (right ? 1 : 0), (up ? 1 : 0) + (down ? -1 : 0));

        InputDevice.SetAxis("Horizontal", Mathf.Clamp(dpad.x + GamePad.CirclePad.x, -1, 1));
        InputDevice.SetAxis("Vertical", Mathf.Clamp(dpad.y + GamePad.CirclePad.y, -1, 1));
        InputDevice.SetToggle("ButtonA", b);
    }

    private void PollValues()
    {
        if (GamePad.GetButtonTrigger(N3dsButton.Left))
            left = true;
        else if (GamePad.GetButtonRelease(N3dsButton.Left))
            left = false;

        if (GamePad.GetButtonTrigger(N3dsButton.Right))
            right = true;
        else if (GamePad.GetButtonRelease(N3dsButton.Right))
            right = false;

        if (GamePad.GetButtonTrigger(N3dsButton.Up))
            up = true;
        else if (GamePad.GetButtonRelease(N3dsButton.Up))
            up = false;

        if (GamePad.GetButtonTrigger(N3dsButton.Down))
            down = true;
        else if (GamePad.GetButtonRelease(N3dsButton.Down))
            down = false;

        if (GamePad.GetButtonTrigger(N3dsButton.B))
            b = true;
        else if (GamePad.GetButtonRelease(N3dsButton.B))
            b = false;
    }
}
