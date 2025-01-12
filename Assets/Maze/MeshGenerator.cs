using UnityEngine;
using System.Collections;

// This Unity script demonstrates how to create a Mesh (in this case a Cube) purely through code.
// Source: https://gist.github.com/prucha/866b9535d525adc984c4fe883e73a6c7
public static class MeshGenerator {

	public static Mesh GenerateCube(Vector3 scale) {
        Mesh mesh = new Mesh();

        // Define the coordinates of each corner of the cube 
        Vector3[] c = new Vector3[] {
            new Vector3(-scale.x * .5f, -scale.y * .5f, scale.z * .5f),
            new Vector3(scale.x * .5f, -scale.y * .5f, scale.z * .5f),
            new Vector3(scale.x * .5f, -scale.y * .5f, -scale.z * .5f),
            new Vector3(-scale.x * .5f, -scale.y * .5f, -scale.z * .5f),

            new Vector3(-scale.x * .5f, scale.y * .5f, scale.z * .5f),
            new Vector3(scale.x * .5f, scale.y * .5f, scale.z * .5f),
            new Vector3(scale.x * .5f, scale.y * .5f, -scale.z * .5f),
            new Vector3(-scale.x * .5f, scale.y * .5f, -scale.z * .5f)
        };

        // Define the vertices that the cube is composed of
        Vector3[] vertices = new Vector3[] {
	        c[0], c[1], c[2], c[3], // Bottom
	        c[7], c[4], c[0], c[3], // Left
	        c[4], c[5], c[1], c[0], // Front
	        c[6], c[7], c[3], c[2], // Back
	        c[5], c[6], c[2], c[1], // Right
	        c[7], c[6], c[5], c[4]  // Top
        };

        // Define each vertex's Normal
        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 forward = Vector3.forward;
        Vector3 back = Vector3.back;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;

        Vector3[] normals = new Vector3[] {
	        down, down, down, down,             // Bottom
	        left, left, left, left,             // Left
	        forward, forward, forward, forward,	// Front
	        back, back, back, back,             // Back
	        right, right, right, right,         // Right
	        up, up, up, up	                    // Top
        };

        // Define each vertex's UV co-ordinates
        Vector2 uv00 = new Vector2(0f, 0f);
        Vector2 uv10 = new Vector2(1f, 0f);
        Vector2 uv01 = new Vector2(0f, 1f);
        Vector2 uv11 = new Vector2(1f, 1f);

        Vector2[] uvs = new Vector2[] {
	        uv11, uv01, uv00, uv10, // Bottom
	        uv11, uv01, uv00, uv10, // Left
	        uv11, uv01, uv00, uv10, // Front
	        uv11, uv01, uv00, uv10, // Back	        
	        uv11, uv01, uv00, uv10, // Right 
	        uv11, uv01, uv00, uv10  // Top
        };

        // Define the Polygons (triangles) that make up the our Mesh (cube)
        //IMPORTANT: Unity uses a 'Clockwise Winding Order' for determining front-facing polygons.
        //This means that a polygon's vertices must be defined in 
        //a clockwise order (relative to the camera) in order to be rendered/visible.
        int[] triangles = new int[] {
	        3, 1, 0,        3, 2, 1,        // Bottom	
	        7, 5, 4,        7, 6, 5,        // Left
	        11, 9, 8,       11, 10, 9,      // Front
	        15, 13, 12,     15, 14, 13,     // Back
	        19, 17, 16,     19, 18, 17,	    // Right
	        23, 21, 20,     23, 22, 21,	    // Top
        };

        //8) Build the Mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.Optimize();

        return mesh;
    }
}
