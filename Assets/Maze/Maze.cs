using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Maze : MonoBehaviour {

    // Maze Generation Parameters
    [SerializeField] public Vector3 mazeDimensions = new Vector3(1, 1, 1);
    [SerializeField] public Vector3Int gridDimensions = new Vector3Int(5, 5, 5);
    [SerializeField] public float wallThickness = 0.02f;

    // Internal Maze Data
    private Dictionary<Vector3, List<Vector3>> connections;
    private List<Vector3> visited;
    private List<Vector3> unvisited;

    // Define the directions
    private static Vector3[] directions = new Vector3[] {
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1)
    };

    // Target for material to render around
    [SerializeField] public Transform target;
    [SerializeField] public float radius = 1.0f;

    // Component References
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    // Start is called before the first frame update
    void Start() {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        // Define the maze cells
        DefineMazeCells(gridDimensions);

        // Pick a random starting node
        Vector3 start = unvisited[Random.Range(0, unvisited.Count)];
        visited.Add(start);
        unvisited.Remove(start);

        // Generate the maze
        GenerateMaze();

        // Generate the walls
        meshFilter.mesh = GenerateWalls(mazeDimensions, gridDimensions, wallThickness);
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    // Update is called once per frame
    void Update() {
        meshRenderer.material.SetVector("_ProxyTarget", target.position);
        meshRenderer.material.SetFloat("_ProxyRadius", radius);
    }

    // Generates a maze mesh using Wilson's algorithm on defined cells in a grid
    // OBS: Causes an infinite loop sometimes, idk don't ask
    public void GenerateMaze(int maxWalks = 100) {

        // Generate the maze using wilson's algorithm
        while (unvisited.Count > 0 && maxWalks-- > 0) {

            // Add the path to the maze if it reaches a visited node
            Stack<Vector3> path = RandomWalk(100);
            if (path.Count > 0 && visited.Contains(path.Peek())) {
                Vector3 current = path.Pop();
                while (path.Count > 0) {
                    Vector3 next = path.Pop();
                    connections[current].Add(next);
                    visited.Add(next);
                    unvisited.Remove(next);
                    current = next;
                }
            }
        }
    }

    // Define valid cells in the maze
    private void DefineMazeCells(Vector3 gridDimensions) {
        connections = new Dictionary<Vector3, List<Vector3>>();
        visited = new List<Vector3>();
        unvisited = new List<Vector3>();

        // Initialize unvisited list
        for (int x = 0; x < gridDimensions.x; x++) {
            for (int y = 0; y < gridDimensions.y; y++) {
                for (int z = 0; z < gridDimensions.z; z++) {

                    // Only add the border cells
                    if (x == 0 || x == gridDimensions.x - 1 || y == 0 || y == gridDimensions.y - 1 || z == 0 || z == gridDimensions.z - 1) {
                        unvisited.Add(new Vector3(x, y, z));
                        connections[new Vector3(x, y, z)] = new List<Vector3>();
                    }
                }
            }
        }
    }

    // Returns a random walk path from a random unvisited node to a visited node, or an empty stack if no visited node is reached
    private Stack<Vector3> RandomWalk(int maxSteps) {
        Stack<Vector3> path = new Stack<Vector3>();
        Vector3 current = unvisited[Random.Range(0, unvisited.Count)];
        Vector3 lastDirection = Vector3.zero;
        path.Push(current);
        
        // Walk a random path until we reach a visited node
        while (!visited.Contains(current) && maxSteps-- > 0) {

            // Count valid directions
            List<Vector3> validDirections = new List<Vector3>();
            foreach (Vector3 direction in directions) {
                Vector3 nextNode = current + direction;
                if (nextNode != -lastDirection && (unvisited.Contains(nextNode) || visited.Contains(nextNode))) {
                    validDirections.Add(direction);
                }
            }
            
            // Pick a random direction, if next node creates a loop, remove the loop
            current += validDirections[Random.Range(0, validDirections.Count)];
            if (path.Contains(current)) { while (path.Pop() != current); }
            path.Push(current);
        }

        // Return the path if we reached a visited node, otherwise return an empty stack
        return visited.Contains(current) ? path : new Stack<Vector3>();
    }

    // Generates the walls of the maze based on the connections
    private Mesh GenerateWalls(Vector3 mazeScale, Vector3 gridDimensions, float wallThickness) {

        // Calculate the cell size
        Vector3 cellSize = new Vector3(
            mazeScale.x / this.gridDimensions.x,
            mazeScale.y / this.gridDimensions.y,
            mazeScale.z / this.gridDimensions.z
        );

        // Create the mesh
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        
        // Wall on only one side
        Vector3[] oneDirection = new Vector3[] {
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1)
        };

        // Generate vertices and triangles
        for (int x = 0; x < gridDimensions.x; x++) {
            for (int y = 0; y < gridDimensions.y; y++) {
                for (int z = 0; z < gridDimensions.z; z++) {
                    Vector3 cell = new Vector3(x, y, z);

                    // Check all sides of the current cell
                    foreach (Vector3 direction in oneDirection) {
                        Vector3 next = cell + direction;

                        // Generate a cube between the cells if there is no connection
                        if (connections.ContainsKey(cell) && !connections[cell].Contains(next)) {
                            int vertexIndex = vertices.Count;

                            // Skip all outer bound walls
                            if (next.x < 0 || next.x >= gridDimensions.x || next.y < 0 || next.y >= gridDimensions.y || next.z < 0 || next.z >= gridDimensions.z) {
                                continue;
                            }

                            // Generate Cube
                            Vector3 cornerVertex = new Vector3(1 - Mathf.Abs(direction.x), 1 - Mathf.Abs(direction.y), 1 - Mathf.Abs(direction.z));
                            Mesh cubeMesh = MeshGenerator.GenerateCube(Vector3.Scale(cornerVertex * (1 + wallThickness) + direction * wallThickness, cellSize));

                            // Offset the vertices
                            Vector3[] offsetVertices = cubeMesh.vertices;
                            for (int i = 0; i < offsetVertices.Length; i++) {
                                offsetVertices[i] += Vector3.Scale(cell + direction * 0.5f, cellSize) + (cellSize - mazeScale) * 0.5f;
                            }

                            // Add all the vertices, normals, uvs and triangles
                            vertices.AddRange(offsetVertices);
                            normals.AddRange(cubeMesh.normals);
                            uvs.AddRange(cubeMesh.uv);
                            foreach (int triangle in cubeMesh.triangles) {
                                triangles.Add(triangle + vertexIndex);
                            }
                        }
                    }
                }
            }
        }

        // Set the mesh data
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);

        // Recalculate the mesh bounds and tangents
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }
}
