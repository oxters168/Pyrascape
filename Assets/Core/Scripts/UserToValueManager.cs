using UnityEngine;
using UnityHelpers;
using Rewired;

public class UserToValueManager : MonoBehaviour
{
    public int playerId;
    private Player player;

    [RequireInterface(typeof(IValueManager))]
    public GameObject _controlledObject;
    private GameObject _prevControlledObject;
    private IValueManager InputDevice { get { if (_controlledObject != _prevControlledObject) { _inputDevice = _controlledObject.GetComponent<IValueManager>(); _prevControlledObject = _controlledObject; } return _inputDevice; } }
    private IValueManager _inputDevice;
    
    void Start()
    {
        player = ReInput.players.GetPlayer(playerId);
    }
    
    void Update()
    {
        if (InputDevice != null)
        {
            InputDevice.SetAxis("Horizontal", player.GetAxis("Horizontal"));
            InputDevice.SetAxis("Vertical", player.GetAxis("Vertical"));
            InputDevice.SetToggle("ButtonA", player.GetButton("ButtonA"));
        }
    }
}
