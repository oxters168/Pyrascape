using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator Doornimator { get { if (_doornimator == null) _doornimator = GetComponentInChildren<Animator>(); return _doornimator; } }
    private Animator _doornimator;

    public bool isOpen;
    private bool prevIsOpen;

    public virtual void Update()
    {
        if (isOpen != prevIsOpen)
        {
            Doornimator.SetTrigger(isOpen ? "Open" : "Close");
            prevIsOpen = isOpen;
        }
    }
}
