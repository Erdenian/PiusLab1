using UnityEngine;

public class TableController : MonoBehaviour
{

    public Vector3 Size = new Vector3(0.3f, 0.2f, 0.5f);
    public float DeltaSize = 0.001f;

    public Collider Knife;
    public MeshFilter MeshExternal, MeshInternal;

    public int SubdivideLevel = 50;

    private Vector3[] verticesExternal, verticesInternal;
    private float groundLevelExternal;

    void Awake()
    {
        MeshExternal.transform.localScale = Size;
        MeshInternal.transform.localScale = Size * (1.0f - DeltaSize);

        MeshExternal.transform.position = transform.position + Vector3.up * (Size.y / 2);
        MeshInternal.transform.position = MeshExternal.transform.position;

        MeshHelper.Subdivide(MeshExternal.mesh, SubdivideLevel);
        MeshHelper.Subdivide(MeshInternal.mesh, SubdivideLevel);

        verticesExternal = MeshExternal.mesh.vertices;
        verticesInternal = MeshInternal.mesh.vertices;

        groundLevelExternal = (MeshExternal.transform.position - MeshExternal.mesh.bounds.extents).y;
    }

    void Update()
    {
        var knifeBounds = MeshInternal.transform.InverseTransformBounds(Knife.bounds);
        var knifeBottom = MeshInternal.transform.InverseTransformPoint(Knife.transform.position - Knife.bounds.extents).y;

        var cores = SystemInfo.processorCount;
        var verticesPerCore = (verticesExternal.Length - 1) / cores + 1;

        bool changed = false;
        Parallel.For(cores, (core) =>
        {
            var start = core * verticesPerCore;
            var end = Mathf.Min((core + 1) * verticesPerCore, verticesExternal.Length);

            for (int i = start; i < end; i++)
                if (knifeBounds.Contains(verticesInternal[i]))
                {
                    verticesInternal[i].y = knifeBottom;
                    verticesExternal[i].y = groundLevelExternal;
                    changed = true;
                }
        });

        if (changed)
        {
            MeshInternal.mesh.vertices = verticesInternal;
            MeshExternal.mesh.vertices = verticesExternal;
        }
    }
}
