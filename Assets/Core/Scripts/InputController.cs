using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public ParkourController parkourMan;

    void Start()
    {
        
    }
    void Update()
    {
        parkourMan.jump = Input.GetButton("Jump");
    }
}
