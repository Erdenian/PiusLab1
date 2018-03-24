using UnityEngine;

public class TableController : MonoBehaviour
{

    public Vector3 Size = new Vector3(0.3f, 0.2f, 0.5f);
    public float DeltaSize = 0.001f;

    public Collider Knife;
    public MeshFilter MeshExternal, MeshInternal;

    public int SubdivideLevel = 50;

    private Vector3[] verticesExternal, verticesInternal;

    void Awake()
    {
        MeshExternal.transform.localScale = Size;
        MeshInternal.transform.localScale = Size * (1 - DeltaSize);

        MeshExternal.transform.position = transform.position + Vector3.up * (Size.y / 2);
        MeshInternal.transform.position = MeshExternal.transform.position;

        MeshHelper.Subdivide(MeshExternal.mesh, SubdivideLevel);
        MeshHelper.Subdivide(MeshInternal.mesh, SubdivideLevel);

        verticesExternal = MeshExternal.mesh.vertices;
        verticesInternal = MeshInternal.mesh.vertices;
    }

    void Update()
    {
        var knifeBottomWorld = Knife.transform.position - Knife.bounds.extents;
        var cores = SystemInfo.processorCount;

        {
            var knifeBounds = MeshExternal.transform.InverseTransformBounds(Knife.bounds);
            var knifeBottom = MeshExternal.transform.InverseTransformPoint(knifeBottomWorld).y;
            var vertices = verticesExternal;
            var verticesPerCore = (vertices.Length - 1) / cores + 1;

            Parallel.For(SystemInfo.processorCount, (core) =>
            {
                var start = core * verticesPerCore;
                var end = Mathf.Min((core + 1) * verticesPerCore, vertices.Length);

                for (int i = start; i < end; i++)
                    if (knifeBounds.Contains(vertices[i]))
                        vertices[i].y = knifeBottom - DeltaSize;
            });
            MeshExternal.mesh.vertices = vertices;
        }
        
        {
            var knifeBounds = MeshInternal.transform.InverseTransformBounds(Knife.bounds);
            var knifeBottom = MeshInternal.transform.InverseTransformPoint(knifeBottomWorld).y;
            var vertices = verticesExternal;
            var verticesPerCore = (vertices.Length - 1) / cores + 1;

            Parallel.For(SystemInfo.processorCount, (core) =>
            {
                var start = core * verticesPerCore;
                var end = Mathf.Min((core + 1) * verticesPerCore, vertices.Length);

                for (int i = start; i < end; i++)
                    if (knifeBounds.Contains(vertices[i]))
                        vertices[i].y = knifeBottom;
            });
            MeshInternal.mesh.vertices = vertices;
        }
    }
}
