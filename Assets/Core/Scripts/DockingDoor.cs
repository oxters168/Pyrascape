using UnityEngine;

public class DockingDoor : Door
{
    // public Door TheDoor { get { if (_theDoor == null) _theDoor = GetComponentInChildren<Door>(); return _theDoor; } }
    // private Door _theDoor;
    public DiggerDock TheDock { get { if (_theDock == null) _theDock = GetComponentInChildren<DiggerDock>(); return _theDock; } }
    private DiggerDock _theDock;

    public override void Update()
    {
        isOpen = TheDock.diggerDocked;

        base.Update();
    }
}
