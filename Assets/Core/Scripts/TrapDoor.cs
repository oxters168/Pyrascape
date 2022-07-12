using UnityEngine;

public class TrapDoor : Door
{
    public bool randomlyOpenClose;
    public float interval = 3;
    private float lastCheck = float.MinValue;

    public override void Update()
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

        base.Update();
    }

    //Used in the animator
    public void ColliderOff()
    {
        SetCollider(false);
    }
    //Used in the animator
    public void ColliderOn()
    {
        SetCollider(true);
    }

    public void SetCollider(bool onOff)
    {
        var colliders = GetComponentsInChildren<Collider2D>();
        if (colliders != null && colliders.Length > 0)
            foreach (var collider in colliders)
                collider.enabled = onOff;
    }
}
