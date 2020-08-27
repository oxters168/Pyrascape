using UnityEngine;
using UnityEngine.N3DS;
// using Rewired;

public class N3DSRewiredBridge : MonoBehaviour
{
	// void OnEnable()
	// {
	// 	ReInput.InputSourceUpdateEvent += InputSourceUpdateEvent;
	// }
	// void OnDisable()
	// {
	// 	ReInput.InputSourceUpdateEvent -= InputSourceUpdateEvent;
	// }

	// private void InputSourceUpdateEvent ()
	// {
	// 	foreach (var player in ReInput.players.AllPlayers)
	// 		foreach (var con in player.controllers.CustomControllers)
	// 		{
	// 			var left = GamePad.GetButtonTrigger(N3dsButton.Left) ? -1 : 0;
	// 			var right = GamePad.GetButtonTrigger(N3dsButton.Right) ? 1 : 0;
	// 			con.SetAxisValue(0, left + right); // 0 DPadH

	// 			var up = GamePad.GetButtonTrigger(N3dsButton.Up) ? 1 : 0;
	// 			var down = GamePad.GetButtonTrigger(N3dsButton.Down) ? -1 : 0;
	// 			con.SetAxisValue(1, up + down); // 1 DPadV

	// 			con.SetAxisValue(2, GamePad.CirclePad.x); // 2 JoyH
	// 			con.SetAxisValue(3, GamePad.CirclePad.y); // 3 JoyV
	// 			con.SetAxisValue(4, GamePad.CirclePadPro.x); // 4 NubH
	// 			con.SetAxisValue(5, GamePad.CirclePadPro.y);// 5 NubV
				
	// 			con.SetButtonValue(6, GamePad.GetButtonTrigger(N3dsButton.A)); // 6 A
	// 			con.SetButtonValue(7, GamePad.GetButtonTrigger(N3dsButton.B)); // 7 B
	// 			con.SetButtonValue(8, GamePad.GetButtonTrigger(N3dsButton.X)); // 8 X
	// 			con.SetButtonValue(9, GamePad.GetButtonTrigger(N3dsButton.Y)); // 9 Y
	// 			con.SetButtonValue(10, GamePad.GetButtonTrigger(N3dsButton.L)); // 10 L
	// 			con.SetButtonValue(11, GamePad.GetButtonTrigger(N3dsButton.R)); // 11 R
	// 			con.SetButtonValue(12, GamePad.GetButtonTrigger(N3dsButton.ZL)); // 12 ZL
	// 			con.SetButtonValue(13, GamePad.GetButtonTrigger(N3dsButton.ZR)); // 13 ZR
	// 			con.SetButtonValue(14, GamePad.GetButtonTrigger(N3dsButton.Start)); // 14 Start
	// 			//con.SetButtonValue(15, GamePad.GetButtonTrigger(N3dsButton.Select)); // 15 Select
	// 		}
	// }
}
