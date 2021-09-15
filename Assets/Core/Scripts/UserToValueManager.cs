using UnityEngine;
using UnityHelpers;
using Rewired;

public class UserToValueManager : MonoBehaviour
{
    public int playerId;

    [RequireInterface(typeof(IValueManager))]
    public GameObject _controlledObject;
    private GameObject _prevControlledObject;
    private IValueManager InputDevice { get { if (_controlledObject != _prevControlledObject) { _inputDevice = _controlledObject.GetComponent<IValueManager>(); _prevControlledObject = _controlledObject; } return _inputDevice; } }
    private IValueManager _inputDevice;
    
    void Update()
    {
        var player = ReInput.players.GetPlayer(playerId);
        
        InputDevice.SetAxis("Horizontal", player.GetAxis("Horizontal"));
        InputDevice.SetAxis("Vertical", player.GetAxis("Vertical"));
        InputDevice.SetToggle("ButtonA", player.GetButton("ButtonA"));
    }
}
