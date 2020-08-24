using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public ParkourController parkourMan;

    void Update()
    {
        parkourMan.jump = Input.GetButton("Jump");
        parkourMan.vertical = Input.GetAxis("Vertical");
        parkourMan.horizontal = Input.GetAxis("Horizontal");
    }
}
