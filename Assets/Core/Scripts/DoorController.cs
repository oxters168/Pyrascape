using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Door TheDoor { get { if (_theDoor == null) _theDoor = GetComponentInChildren<Door>(); return _theDoor; } }
    private Door _theDoor;
    public DiggerDock TheDock { get { if (_theDock == null) _theDock = GetComponentInChildren<DiggerDock>(); return _theDock; } }
    private DiggerDock _theDock;

    public WorldGenerator world;


    void Update()
    {
        TheDoor.isOpen = world.isIndoors || TheDock.diggerDocked;
        TheDock.gameObject.SetActive(!world.isIndoors);
    }
}
