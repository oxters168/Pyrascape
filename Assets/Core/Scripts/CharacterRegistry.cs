using UnityEngine;
using System.Collections.Generic;

public static class CharacterRegistry
{
    private static List<HumanControl> currentSpawnedCharacters = new List<HumanControl>();

    /// <summary>
    /// This function is called by CharacterSpawner in the Start function to add itself to the registry
    /// </summary>
    public static void AddCharacter(HumanControl character)
    {
        if (character == null)
            Debug.LogError("Cannot add null character to registry");
        else if (!currentSpawnedCharacters.Contains(character))
            currentSpawnedCharacters.Add(character);
        else
            Debug.LogError("Could not add already existing character to registry");
    }
    /// <summary>
    /// This function is called by CharacterSpawner in the OnDestroy function to remove itself from the registry
    /// </summary>
    public static void RemoveCharacter(HumanControl character)
    {
        if (character == null)
            Debug.LogError("Cannot remove null character from registry");
        else if (currentSpawnedCharacters.Contains(character))
            currentSpawnedCharacters.Remove(character);
        else
            Debug.LogError("Could not remove non-existing character from registry");
    }
    public static bool HasCharacter(HumanControl character)
    {
        return currentSpawnedCharacters.Contains(character);
    }

    public static HumanControl[] GetCurrentCharacters()
    {
        return currentSpawnedCharacters.ToArray();
    }
    public static int GetCurrentCharacterCount()
    {
        return currentSpawnedCharacters.Count;
    }
}
