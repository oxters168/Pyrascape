using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator Doornimator { get { if (_doornimator == null) _doornimator = GetComponentInChildren<Animator>(); return _doornimator; } }
    private Animator _doornimator;

    public bool isOpen;
    private bool prevIsOpen;

    public bool randomlyOpenClose;
    public float interval = 3;
    private float lastCheck = float.MinValue;

    void Update()
    {
        if (randomlyOpenClose)
        {
            if (Time.time - lastCheck >= interval)
            {
                if (Random.value > 0.5f)
                    isOpen = !isOpen;
                lastCheck = Time.time;
            }
        }

        if (isOpen != prevIsOpen)
        {
            Doornimator.SetTrigger(isOpen ? "Open" : "Close");
            prevIsOpen = isOpen;
        }
    }

    public void ColliderOff()
    {
        ToggleCollider(false);
    }
    public void ColliderOn()
    {
        ToggleCollider(true);
    }

    public void ToggleCollider(bool onOff)
    {
        var colliders = GetComponentsInChildren<Collider2D>();
        if (colliders != null && colliders.Length > 0)
            foreach (var collider in colliders)
                collider.enabled = onOff;
    }
}
