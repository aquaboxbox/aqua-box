using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Maze : MonoBehaviour {

    void Start() {
        Mesh mazeMesh = GenerateMaze(new Bounds(Vector3.zero, Vector3.one * 10), new Vector3(5, 5, 5));
        GetComponent<MeshFilter>().mesh = mazeMesh;
    }

    public static Mesh GenerateMaze(Bounds mazeBounds, Vector3 gridDimensions) {
        Dictionary<Vector3, List<Vector3>> connections = new Dictionary<Vector3, List<Vector3>>();
        List<Vector3> visited = new List<Vector3>();
        List<Vector3> unvisited = new List<Vector3>();

        // Initialize unvisited list
        for (int x = 0; x < gridDimensions.x; x++) {
            for (int y = 0; y < gridDimensions.y; y++) {
                for (int z = 0; z < gridDimensions.z; z++) {
                    unvisited.Add(new Vector3(x, y, z));
                    connections[new Vector3(x, y, z)] = new List<Vector3>();
                }
            }
        }

        // Pick a random starting node
        Vector3 start = unvisited[Random.Range(0, unvisited.Count)];
        visited.Add(start);
        unvisited.Remove(start);

        // Generate the maze using wilson's algorithm
        while (unvisited.Count > 0) {
            
            // Walk a random path until we reach a visited node
            Vector3 current = unvisited[Random.Range(0, unvisited.Count)];
            List<Vector3> path = new List<Vector3>() { current };
            while (true) {
                
                // Pick a random direction
                int r = 1 << Random.Range(0, 6);
                Vector3 next = new Vector3(
                    (r >> 0) & 1 - (r >> 1) & 1,
                    (r >> 2) & 1 - (r >> 3) & 1,
                    (r >> 4) & 1 - (r >> 5) & 1
                );

                // Check if the next node is within bounds
                Vector3 nextNode = current + next;
                if (nextNode.x < 0 || nextNode.x >= gridDimensions.x ||
                    nextNode.y < 0 || nextNode.y >= gridDimensions.y ||
                    nextNode.z < 0 || nextNode.z >= gridDimensions.z) {
                    continue;
                }
                current = nextNode;

                // Check if the next node is visited
                if (visited.Contains(nextNode) || path.Contains(nextNode)) {
                    break;
                }

                // Add the connection
                path.Add(nextNode);
            }

            // Check if the path is valid (ends in a visited node == valid, ends in path == invalid)
            if (visited.Contains(current)) {
                for (int i = 0; i < path.Count - 1; i++) {
                    connections[path[i + 1]].Add(path[i]);
                    visited.Add(path[i]);
                    unvisited.Remove(path[i]);
                }
                visited.Add(path[path.Count - 1]);
                unvisited.Remove(path[path.Count - 1]);
            }
        }

        // Create the mesh
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        Vector3[] directions = new Vector3[] {
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1)
        };

        // Generate the vertices and triangles
        Queue<(Vector3, Vector3)> queue = new Queue<(Vector3, Vector3)>();
        queue.Enqueue((start, start));
        while (queue.Count > 0) {
            (Vector3 previous, Vector3 current) = queue.Dequeue();
            foreach (Vector3 direction in directions) {
                Vector3 next = current + direction;

                // Check if wall should be added
                if (!connections[current].Contains(next) && next != previous) {
                    int vertexIndex = vertices.Count;
                    Vector3 cornerVertex = new Vector3(1 - Mathf.Abs(direction.x), 1 - Mathf.Abs(direction.y), 1 - Mathf.Abs(direction.z));
                    for (int i = 0; i < 4; i++) {
                        vertices.Add(current + direction + Quaternion.AngleAxis(90 * i, direction) * cornerVertex);
                        normals.Add(-direction);
                        uvs.Add(new Vector2(i % 2, i / 2));
                    }
                    triangles.AddRange(new int[] {
                        vertexIndex + 0, vertexIndex + 1, vertexIndex + 2,
                        vertexIndex + 0, vertexIndex + 2, vertexIndex + 3
                    });
                }
            }

            // Add the next cells to the queue
            foreach (Vector3 cell in connections[current]) {
                queue.Enqueue((current, cell));
            }
        }

        // Set the mesh data
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);

        return mesh;
    }
}
