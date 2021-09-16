using UnityEngine;
using UnityHelpers;

public class UnifiedInputForPod : MonoBehaviour, IValueManager
{
    private PodPhysics2D _pod;
    private PodPhysics2D Pod { get { if (_pod == null) _pod = GetComponentInChildren<PodPhysics2D>(); return _pod; } }

    public ValuesVault controlValues;

    void Update()
    {
        Pod.horizontal = Mathf.Clamp(GetAxis("Horizontal"), -1, 1);
        Pod.fly = Mathf.Clamp(GetAxis("Vertical"), -1, 1);
    }

    public void SetAxis(string name, float value)
    {
        controlValues.GetValue(name).SetAxis(value);
    }
    public float GetAxis(string name)
    {
        return controlValues.GetValue(name).GetAxis();
    }
    public void SetToggle(string name, bool value)
    {
        controlValues.GetValue(name).SetToggle(value);
    }
    public bool GetToggle(string name)
    {
        return controlValues.GetValue(name).GetToggle();
    }
    public void SetDirection(string name, Vector3 value)
    {
        controlValues.GetValue(name).SetDirection(value);
    }
    public Vector3 GetDirection(string name)
    {
        return controlValues.GetValue(name).GetDirection();
    }
    public void SetPoint(string name, Vector3 value)
    {
        controlValues.GetValue(name).SetPoint(value);
    }
    public Vector3 GetPoint(string name)
    {
        return controlValues.GetValue(name).GetPoint();
    }
    public void SetOrientation(string name, Quaternion value)
    {
        controlValues.GetValue(name).SetOrientation(value);
    }
    public Quaternion GetOrientation(string name)
    {
        return controlValues.GetValue(name).GetOrientation();
    }
}
