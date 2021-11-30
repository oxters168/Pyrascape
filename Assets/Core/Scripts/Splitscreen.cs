using UnityEngine;

public class Splitscreen : MonoBehaviour
{
    public float refreshTime = 3;
    private float lastRefresh = float.MinValue;

    int prevCharacterCount;

    void Update()
    {
        if (Time.time - lastRefresh >= refreshTime)
        {
            var spawnedCharacters = CharacterRegistry.GetCurrentCharacters();
            int currentCharacterCount = spawnedCharacters.Length;

            if (prevCharacterCount != currentCharacterCount)
            {
                int col = 1, row = 1;
                float width = 1, height = 1;
                if (currentCharacterCount > 1)
                {
                    col = Mathf.RoundToInt(Mathf.Log(currentCharacterCount, 2));
                    row = currentCharacterCount > col ? Mathf.RoundToInt(currentCharacterCount / (float)col) : 1;
                    width = 1f / col;
                    height = 1f / row;
                }

                for (int cameraIndex = 0; cameraIndex < currentCharacterCount; cameraIndex++)
                {
                    Rect cameraSplit = new Rect((cameraIndex % col) * width, height * (row - 1) - ((cameraIndex / col) * height), width, height);
                    spawnedCharacters[cameraIndex].spawnedCamera.GetComponent<Camera>().rect = cameraSplit;
                }

                prevCharacterCount = currentCharacterCount;
            }

            lastRefresh = Time.time;
        }

    }
}
