using UnityEngine;
using System.Linq;

public class Splitscreen : MonoBehaviour
{
    int prevCharacterCount;

    float prevScreenWidth, prevScreenHeight;

    void Update()
    {
        var spawnedCharacters = CharacterRegistry.GetCurrentCharacters();
        int currentCharacterCount = spawnedCharacters.Length;

        if (prevCharacterCount != currentCharacterCount || Screen.width != prevScreenWidth || Screen.height != prevScreenHeight)//|| !Mathf.Approximately(Screen.width, prevScreenWidth) || !Mathf.Approximately(Screen.height, prevScreenHeight))
        {
            var screenRects = UnityHelpers.BSP.Partition(new Rect(0, 0, Screen.width, Screen.height), (uint)currentCharacterCount).EnumerateRects().ToArray();
            for (int cameraIndex = 0; cameraIndex < currentCharacterCount; cameraIndex++)
                spawnedCharacters[cameraIndex].spawnedCamera.GetComponent<Camera>().rect = new Rect(screenRects[cameraIndex].x / Screen.width, screenRects[cameraIndex].y / Screen.height, screenRects[cameraIndex].width / Screen.width, screenRects[cameraIndex].height / Screen.height);

            prevCharacterCount = currentCharacterCount;
            prevScreenWidth = Screen.width;
            prevScreenHeight = Screen.height;
        }
    }
}
