using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public Transform Protagonist;
    void Start()
    {
        
    }

    void Update()
    {
        CharacterState state = Protagonist.GetComponent<ProtagonistControl>().GetCharacterState();
        Protagonist.transform.position += (Vector3)(state.Speed * Time.deltaTime);
    }
}
