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

            Vector3 cameraPosition = Camera.main.transform.position;
            Vector3 cameraForward = Camera.main.transform.forward;

            projectile.transform.position = cameraPosition;
            
            projectileComp.projectileData = new DestructionManager.ProjectileData
            {
                InitPosition = cameraPosition,
                Position = cameraPosition,
                Direction = cameraForward,
                MaxDistance = 50f,
                Speed = 1f
            };
        }
    }
}
