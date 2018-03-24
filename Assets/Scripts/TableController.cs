using UnityEngine;

public class TableController : MonoBehaviour
{

    public Vector3 Size = new Vector3(0.3f, 0.2f, 0.5f);
    public float DeltaSize = 0.001f;

    public Collider Knife;
    public MeshFilter BrickExternal, BrickInternal;

    public int SubdivideLevel = 50;

    private Vector3[] verticesExternal, verticesInternal;

    void Awake()
    {
        BrickExternal.transform.localScale = Size;
        BrickInternal.transform.localScale = Size * (1 - DeltaSize);

        BrickExternal.transform.position = transform.position + Vector3.up * (Size.y / 2);
        BrickInternal.transform.position = BrickExternal.transform.position;

        MeshHelper.Subdivide(BrickExternal.mesh, SubdivideLevel);
        MeshHelper.Subdivide(BrickInternal.mesh, SubdivideLevel);

        verticesExternal = BrickExternal.mesh.vertices;
        verticesInternal = BrickInternal.mesh.vertices;
    }

    void Update()
    {
        var knifeBottomWorld = Knife.transform.position - Knife.bounds.extents;
        var cores = SystemInfo.processorCount;

        {
            var knifeBounds = BrickExternal.transform.InverseTransformBounds(Knife.bounds);
            var knifeBottom = BrickExternal.transform.InverseTransformPoint(knifeBottomWorld);
            var vertices = verticesExternal;
            var verticesPerCore = (vertices.Length - 1) / cores + 1;

            Parallel.For(SystemInfo.processorCount, (core) =>
            {
                var start = core * verticesPerCore;
                var end = Mathf.Min((core + 1) * verticesPerCore, vertices.Length);

                for (int i = start; i < end; i++)
                    if (knifeBounds.Contains(vertices[i]))
                        vertices[i].y = knifeBottom.y - DeltaSize;
            });
            BrickExternal.mesh.vertices = vertices;
        }
        
        {
            var knifeBounds = BrickInternal.transform.InverseTransformBounds(Knife.bounds);
            var knifeBottom = BrickInternal.transform.InverseTransformPoint(knifeBottomWorld);
            var vertices = verticesExternal;
            var verticesPerCore = (vertices.Length - 1) / cores + 1;

            Parallel.For(SystemInfo.processorCount, (core) =>
            {
                var start = core * verticesPerCore;
                var end = Mathf.Min((core + 1) * verticesPerCore, vertices.Length);

                for (int i = start; i < end; i++)
                    if (knifeBounds.Contains(vertices[i]))
                        vertices[i].y = knifeBottom.y;
            });
            BrickInternal.mesh.vertices = vertices;
        }
    }
}
