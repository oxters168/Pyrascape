using UnityEngine;
using UnityEngine.UI;

public class AutoScroller : MonoBehaviour
{	
	public Scrollbar vertBar;
	public int frameDelay = 10;
	private int framesPassed;

	// Update is called once per frame
	void Update ()
	{
		framesPassed++;
		if (framesPassed >= frameDelay)
		{
			framesPassed = 0;
			vertBar.value = 0;
		}
	}
}
