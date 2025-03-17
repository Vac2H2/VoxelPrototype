using NUnit.Framework.Constraints;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public partial class PhysicsManager : MonoBehaviour
{
    public Transform Protagonist;
    public Transform WorldSimulation;
    VoxelDataManager voxelDataManager;
    void Start()
    {
        voxelDataManager = WorldSimulation.GetComponent<WorldSimulation>().GetVoxelDataManager();
    }

    void Update()
    {
        // CharacterState state = Protagonist.GetComponent<ProtagonistControl>().GetCharacterState();
        // float3 gravity = -9.81f;
        // state.Speed += gravity * Time.deltaTime;
        // Protagonist.transform.position += (Vector3)(state.Speed * Time.deltaTime);
        // state.Position = Protagonist.transform.position;
        // Protagonist.GetComponent<ProtagonistControl>().UpdateCharacterState(state);
    }
}
