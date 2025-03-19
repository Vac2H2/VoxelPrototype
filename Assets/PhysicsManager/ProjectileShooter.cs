using UnityEngine;

public class ProjectileShooter : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public DestructionManager destructionManager;
    VoxelDataManager voxelDataManager;
    void Start()
    {
        voxelDataManager = destructionManager.GetVoxelDataManager();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject projectile = Instantiate(ProjectilePrefab);
            Projectile projectileComp = projectile.GetComponent<Projectile>();
            projectileComp.voxelDataManager = voxelDataManager;
            projectileComp.destructionManager = destructionManager;
            projectileComp.projectileData = new DestructionManager.ProjectileData
            {
                InitPosition = transform.position,
                Position = transform.position,
                Direction = transform.forward,
                MaxDistance = 50f,
                Speed = 1f
            };
        }
    }
}
