using UnityEngine;
using System.Linq;

public class Splitscreen : MonoBehaviour
{
    public float refreshTime = 3;
    private float lastRefresh = float.MinValue;

    int prevCharacterCount;

    float prevScreenWidth, prevScreenHeight;

    void Update()
    {
        if (Time.time - lastRefresh >= refreshTime)
        {
            var spawnedCharacters = CharacterRegistry.GetCurrentCharacters();
            int currentCharacterCount = spawnedCharacters.Length;

            if (prevCharacterCount != currentCharacterCount || !Mathf.Approximately(Screen.width, prevScreenWidth) || !Mathf.Approximately(Screen.height, prevScreenHeight))
            {
                var screenRects = BSP.Partition(new Rect(0, 0, Screen.width, Screen.height), (uint)currentCharacterCount).EnumerateRects().ToArray();
                for (int cameraIndex = 0; cameraIndex < currentCharacterCount; cameraIndex++)
                    spawnedCharacters[cameraIndex].spawnedCamera.GetComponent<Camera>().rect = new Rect(screenRects[cameraIndex].x / Screen.width, screenRects[cameraIndex].y / Screen.height, screenRects[cameraIndex].width / Screen.width, screenRects[cameraIndex].height / Screen.height);

                prevCharacterCount = currentCharacterCount;
                prevScreenWidth = Screen.width;
                prevScreenHeight = Screen.height;
            }

            lastRefresh = Time.time;
        }

    }
}
