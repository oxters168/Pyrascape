using UnityEngine;
using UnityHelpers;

public class EntityHider : MonoBehaviour
{
    private EntitySpawner EntityRoot { get { if (_entityRoot == null) _entityRoot = GetComponentInChildren<EntitySpawner>(); return _entityRoot; } }
    private EntitySpawner _entityRoot;

    private CharacterSpawner[] Characters { get { if (_characters == null) _characters = FindObjectsOfType<CharacterSpawner>(); return _characters; } }
    private CharacterSpawner[] _characters;

    void Update()
    {
        if (Characters != null)
        {
            bool characterIsAround = false;
            foreach (var character in Characters)
            {
                var characterCamera = character.spawnedCamera.GetComponentInChildren<Camera>();
                Vector2 worldViewSize = characterCamera.PerspectiveFrustum(Mathf.Abs(characterCamera.transform.position.z));
                Vector3 characterPosition = character.controlledObject.transform.position;

                //If character is outdoors and can see the entity then we can enable it or leave it enabled
                if (!character.IsIndoors && Mathf.Abs(characterPosition.x - EntityRoot.spawnedEntity.transform.position.x) < worldViewSize.x && Mathf.Abs(characterPosition.y - EntityRoot.spawnedEntity.transform.position.y) < worldViewSize.y)
                {
                    characterIsAround = true;
                    break;
                }
            }
            
            EntityRoot.spawnedEntity.SetActive(characterIsAround);
        }
        else
            EntityRoot.spawnedEntity.SetActive(false); 
    }
}
