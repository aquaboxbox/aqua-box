using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public interface IExitListener
{
    void OnParticleExited();
}

[RequireComponent(typeof(Maze))]
public class MazeExit : MonoBehaviour
{
    [Header("Exit Configuration")]
    [SerializeField] private GameObject exitPrefab;
    [SerializeField] private GameObject particle;
    [SerializeField, Range(0f, 1f)] private float exitPlacementDifficulty = 1f;
    [SerializeField, Range(0f, 1f)] private float randomnessFactor = 0.2f;
    
    private const float INWARD_EXTENSION = 0.02f;
    private const float VOXEL_SIZE = 0.04f;
    private const float HALF_MAZE_SIZE = 0.1f;
    
    private Maze maze;
    private MeshCollider mazeCollider;
    private GameObject exitVisual;
    private GameObject exitTrigger;
    private bool isExitCreated;
    private BoxCollider mazeEntryTrigger;

    private void Start()
    {
        maze = GetComponent<Maze>();
        mazeCollider = GetComponentInParent<MeshCollider>();
        
        if (mazeCollider == null || exitPrefab == null || particle == null)
        {
            Debug.LogError("MazeExitManager: Missing required components");
            return;
        }

        mazeEntryTrigger = gameObject.AddComponent<BoxCollider>();
        mazeEntryTrigger.isTrigger = true;
        mazeEntryTrigger.size = maze.mazeDimensions;
        mazeEntryTrigger.center = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == particle && !isExitCreated)
        {
            CreateExit(other.transform.position);
        }
    }
    
    private void CreateExit(Vector3 entryPosition)
    {
        Vector3Int entryVoxel = WorldToVoxelCoordinates(transform.InverseTransformPoint(entryPosition));
        Vector3Int exitVoxel = CalculateExitPosition(entryVoxel);
        
        CreateExitTrigger(exitVoxel);
        CreateExitVisual(exitVoxel);
        isExitCreated = true;
    }

    private Vector3Int WorldToVoxelCoordinates(Vector3 localPosition)
    {
        Vector3 normalized = new Vector3(
            (localPosition.x + maze.mazeDimensions.x / 2f) / maze.mazeDimensions.x,
            (localPosition.y + maze.mazeDimensions.y / 2f) / maze.mazeDimensions.y,
            (localPosition.z + maze.mazeDimensions.z / 2f) / maze.mazeDimensions.z
        );
        
        return new Vector3Int(
            Mathf.Clamp(Mathf.FloorToInt(normalized.x * maze.gridDimensions.x), 0, maze.gridDimensions.x - 1),
            Mathf.Clamp(Mathf.FloorToInt(normalized.y * maze.gridDimensions.y), 0, maze.gridDimensions.y - 1),
            Mathf.Clamp(Mathf.FloorToInt(normalized.z * maze.gridDimensions.z), 0, maze.gridDimensions.z - 1)
        );
    }

    private Vector3Int CalculateExitPosition(Vector3Int entryVoxel)
    {
        float maxDistance = 0;
        Vector3Int farthestVoxel = entryVoxel;
        
        // Find the farthest surface voxel from entry point
        for (int x = 0; x < maze.gridDimensions.x; x++)
        {
            for (int y = 0; y < maze.gridDimensions.y; y++)
            {
                for (int z = 0; z < maze.gridDimensions.z; z++)
                {
                    if (!IsVoxelOnSurface(x, y, z)) continue;

                    Vector3Int currentVoxel = new Vector3Int(x, y, z);
                    float distance = Vector3Int.Distance(currentVoxel, entryVoxel);
                    
                    float randomOffset = 1f + Random.Range(-randomnessFactor, randomnessFactor);
                    distance *= randomOffset;

                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        farthestVoxel = currentVoxel;
                    }
                }
            }
        }

        // Interpolate between entry and farthest point based on difficulty
        Vector3 interpolated = Vector3.Lerp(entryVoxel, farthestVoxel, exitPlacementDifficulty);
        return FindNearestSurfaceVoxel(Vector3Int.RoundToInt(interpolated));
    }

    private bool IsVoxelOnSurface(int x, int y, int z)
    {
        return x == 0 || x == maze.gridDimensions.x - 1 ||
               y == 0 || y == maze.gridDimensions.y - 1 ||
               z == 0 || z == maze.gridDimensions.z - 1;
    }

    private Vector3Int FindNearestSurfaceVoxel(Vector3Int voxel)
    {
        if (IsVoxelOnSurface(voxel.x, voxel.y, voxel.z)) return voxel;

        float minDistance = float.MaxValue;
        Vector3Int nearest = voxel;

        for (int x = 0; x < maze.gridDimensions.x; x++)
        {
            for (int y = 0; y < maze.gridDimensions.y; y++)
            {
                for (int z = 0; z < maze.gridDimensions.z; z++)
                {
                    if (!IsVoxelOnSurface(x, y, z)) continue;

                    Vector3Int current = new Vector3Int(x, y, z);
                    float distance = Vector3Int.Distance(current, voxel);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = current;
                    }
                }
            }
        }

        return nearest;
    }

    private void CreateExitTrigger(Vector3Int voxel)
    {
        exitTrigger = new GameObject("ExitTrigger");
        exitTrigger.transform.SetParent(transform);
        exitTrigger.layer = LayerMask.NameToLayer("ExitHole");

        BoxCollider triggerCollider = exitTrigger.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        Vector3 localPosition = CalculateVoxelLocalPosition(voxel);
        (Vector3 size, Vector3 position) = CalculateExitTriggerTransform(voxel, localPosition);
        
        triggerCollider.size = size;
        exitTrigger.transform.localPosition = position;
        
        var exitTriggerScript = exitTrigger.AddComponent<ExitTrigger>();
        exitTriggerScript.Initialize(mazeCollider, particle);
        exitTriggerScript.showDebug = true;
    }

    private void CreateExitVisual(Vector3Int voxel)
    {
        exitVisual = Instantiate(exitPrefab);
        exitVisual.transform.SetParent(transform, false);

        Vector3 localPosition = CalculateVoxelLocalPosition(voxel);
        (Vector3 position, Quaternion rotation) = CalculateExitVisualTransform(voxel, localPosition);
        
        exitVisual.transform.localPosition = position;
        exitVisual.transform.localRotation = rotation;
        exitVisual.transform.localScale = Vector3.one;
    }

    private Vector3 CalculateVoxelLocalPosition(Vector3Int voxel)
    {
        return new Vector3(
            (voxel.x * VOXEL_SIZE + VOXEL_SIZE * 0.5f) - HALF_MAZE_SIZE,
            (voxel.y * VOXEL_SIZE + VOXEL_SIZE * 0.5f) - HALF_MAZE_SIZE,
            (voxel.z * VOXEL_SIZE + VOXEL_SIZE * 0.5f) - HALF_MAZE_SIZE
        );
    }

    private (Vector3 size, Vector3 position) CalculateExitTriggerTransform(Vector3Int voxel, Vector3 basePosition)
    {
        Vector3 size = Vector3.one * VOXEL_SIZE;
        Vector3 position = basePosition;

        if (voxel.x == 0) // Left face
        {
            size = new Vector3(INWARD_EXTENSION, VOXEL_SIZE, VOXEL_SIZE);
            position.x = -HALF_MAZE_SIZE + INWARD_EXTENSION/2;
        }
        else if (voxel.x == maze.gridDimensions.x - 1) // Right face
        {
            size = new Vector3(INWARD_EXTENSION, VOXEL_SIZE, VOXEL_SIZE);
            position.x = HALF_MAZE_SIZE - INWARD_EXTENSION/2;
        }
        else if (voxel.y == 0) // Bottom face
        {
            size = new Vector3(VOXEL_SIZE, INWARD_EXTENSION, VOXEL_SIZE);
            position.y = -HALF_MAZE_SIZE + INWARD_EXTENSION/2;
        }
        else if (voxel.y == maze.gridDimensions.y - 1) // Top face
        {
            size = new Vector3(VOXEL_SIZE, INWARD_EXTENSION, VOXEL_SIZE);
            position.y = HALF_MAZE_SIZE - INWARD_EXTENSION/2;
        }
        else if (voxel.z == 0) // Back face
        {
            size = new Vector3(VOXEL_SIZE, VOXEL_SIZE, INWARD_EXTENSION);
            position.z = -HALF_MAZE_SIZE + INWARD_EXTENSION/2;
        }
        else // Front face
        {
            size = new Vector3(VOXEL_SIZE, VOXEL_SIZE, INWARD_EXTENSION);
            position.z = HALF_MAZE_SIZE - INWARD_EXTENSION/2;
        }

        return (size, position);
    }

    private (Vector3 position, Quaternion rotation) CalculateExitVisualTransform(Vector3Int voxel, Vector3 basePosition)
    {
        Vector3 position = basePosition;
        Quaternion rotation = Quaternion.identity;

        if (voxel.x == 0) // Left face
        {
            position.x = -HALF_MAZE_SIZE;
            rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (voxel.x == maze.gridDimensions.x - 1) // Right face
        {
            position.x = HALF_MAZE_SIZE;
            rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (voxel.y == 0) // Bottom face
        {
            position.y = -HALF_MAZE_SIZE;
            rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (voxel.y == maze.gridDimensions.y - 1) // Top face
        {
            position.y = HALF_MAZE_SIZE;
            rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (voxel.z == 0) // Back face
        {
            position.z = -HALF_MAZE_SIZE;
            rotation = Quaternion.Euler(0, 90, 0);
        }
        else // Front face
        {
            position.z = HALF_MAZE_SIZE;
            rotation = Quaternion.Euler(0, -90, 0);
        }

        return (position, rotation);
    }
    
    private void OnDestroy()
    {
        if (mazeEntryTrigger != null)
        {
            Destroy(mazeEntryTrigger);
        }
    }
}

public class ExitTrigger : MonoBehaviour
{
    public bool showDebug = true;
    private MeshCollider mazeCollider;
    private GameObject particle;
    private BoxCollider exitTrigger;
    private IExitListener exitListener;
    private bool isCollisionIgnored;
    private bool wasInBox = true;

    public void Initialize(MeshCollider meshCollider, GameObject particleObj)
    {
        mazeCollider = meshCollider;
        particle = particleObj;
        exitTrigger = GetComponent<BoxCollider>();
        exitListener = GetComponentInParent<IExitListener>();
    }

    private bool IsParticleContained(Collider particleCollider)
    {
        Vector3 localParticlePos = transform.InverseTransformPoint(particleCollider.transform.position);
        float particleRadius = ((SphereCollider)particleCollider).radius * particleCollider.transform.lossyScale.x;

        Vector3 halfSize = exitTrigger.size * 0.5f;
        Vector3 size = exitTrigger.size;
        int exitAxis = 0; // 0 = X, 1 = Y, 2 = Z
        if (size.y < size.x && size.y < size.z) exitAxis = 1;
        else if (size.z < size.x && size.z < size.y) exitAxis = 2;

        // Be strict on non-exit axes
        Vector3 minBounds = -halfSize + Vector3.one * (particleRadius * 0.9f);
        Vector3 maxBounds = halfSize - Vector3.one * (particleRadius * 0.9f);

        switch (exitAxis)
        {
            case 0: // X is exit direction
                return localParticlePos.y >= minBounds.y && localParticlePos.y <= maxBounds.y &&
                       localParticlePos.z >= minBounds.z && localParticlePos.z <= maxBounds.z;
            case 1: // Y is exit direction
                return localParticlePos.x >= minBounds.x && localParticlePos.x <= maxBounds.x &&
                       localParticlePos.z >= minBounds.z && localParticlePos.z <= maxBounds.z;
            case 2: // Z is exit direction
                return localParticlePos.x >= minBounds.x && localParticlePos.x <= maxBounds.x &&
                       localParticlePos.y >= minBounds.y && localParticlePos.y <= maxBounds.y;
            default:
                return false;
        }
    }

    private bool IsParticleInBox(Collider particleCollider)
    {
        if (mazeCollider == null) return false;

        Vector3 particlePos = particleCollider.transform.position;
        float radius = ((SphereCollider)particleCollider).radius * particleCollider.transform.lossyScale.x;
        Vector3 localParticlePos = mazeCollider.transform.InverseTransformPoint(particlePos);
        
        return Mathf.Abs(localParticlePos.x) <= 0.1f + radius &&
               Mathf.Abs(localParticlePos.y) <= 0.1f + radius &&
               Mathf.Abs(localParticlePos.z) <= 0.1f + radius;
    }

    private void Update()
    {
        var particleCollider = particle.GetComponent<Collider>();
        bool isInBox = IsParticleInBox(particleCollider);

        if (wasInBox && !isInBox)
        {
            exitListener?.OnParticleExited();
        }
        wasInBox = isInBox;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject != particle || mazeCollider == null) return;

        bool isContained = IsParticleContained(other);

        if (isContained && !isCollisionIgnored)
        {
            Physics.IgnoreCollision(other, mazeCollider, true);
            Physics.IgnoreLayerCollision(other.gameObject.layer, mazeCollider.gameObject.layer, true);
            isCollisionIgnored = true;
            Physics.SyncTransforms();
        }
        else if (!isContained && isCollisionIgnored)
        {
            Physics.IgnoreCollision(other, mazeCollider, false);
            Physics.IgnoreLayerCollision(other.gameObject.layer, mazeCollider.gameObject.layer, false);
            isCollisionIgnored = false;
            Physics.SyncTransforms();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != particle) return;

        if (isCollisionIgnored)
        {
            Physics.IgnoreCollision(other, mazeCollider, false);
            Physics.IgnoreLayerCollision(other.gameObject.layer, mazeCollider.gameObject.layer, false);
            isCollisionIgnored = false;
            Physics.SyncTransforms();
        }
    }

    private void OnDestroy()
    {
        if (particle != null && mazeCollider != null && isCollisionIgnored)
        {
            var collider = particle.GetComponent<Collider>();
            Physics.IgnoreCollision(collider, mazeCollider, false);
            Physics.IgnoreLayerCollision(collider.gameObject.layer, mazeCollider.gameObject.layer, false);
        }
    }
}
