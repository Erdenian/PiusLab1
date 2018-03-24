using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeDivider : MonoBehaviour
{

    private MeshFilter meshFilter;
    private Vector3[] vertices;

    public bool test = true;
    
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;
        MeshHelper.Subdivide(mesh, 50);
        meshFilter.mesh = mesh;

        vertices = mesh.vertices;
    }
    
    void Update()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            RaycastHit hit;
            Ray ray = new Ray(vertices[i] + Vector3.down, Vector3.up);
            if (Physics.Raycast(ray, out hit, 1.0f))
            {
                vertices[i] = (test) ? hit.point : hit.point + Vector3.down * 0.001f;
            }
        }
        meshFilter.mesh.vertices = vertices;
    }
}
