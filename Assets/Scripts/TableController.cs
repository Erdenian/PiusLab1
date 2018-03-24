using UnityEngine;

public class TableController : MonoBehaviour
{

    public Vector3 Size = new Vector3(0.3f, 0.2f, 0.5f);
    public float DeltaSize = 0.001f;

    public GameObject SpawnPoint, BrickExternal, BrickInternal;

    public int SubdivideLevel = 50;

    private MeshFilter meshFilterExternal, meshFilterInternal;
    
    private Vector3[] verticesExternal, verticesInternal;

    void Awake()
    {
        BrickExternal.transform.localScale = Size;
        BrickInternal.transform.localScale = Size * (1 - DeltaSize);

        BrickExternal.transform.position = SpawnPoint.transform.position + Vector3.up * (Size.y / 2);
        BrickInternal.transform.position = BrickExternal.transform.position;

        meshFilterExternal = BrickExternal.GetComponent<MeshFilter>();
        meshFilterInternal = BrickInternal.GetComponent<MeshFilter>();

        MeshHelper.Subdivide(meshFilterExternal.mesh, SubdivideLevel);
        MeshHelper.Subdivide(meshFilterInternal.mesh, SubdivideLevel);

        verticesExternal = meshFilterExternal.mesh.vertices;
        verticesInternal = meshFilterInternal.mesh.vertices;
    }

    void Update()
    {
        for (int i = 0; i < verticesExternal.Length; i++)
        {
            RaycastHit hit;
            Ray ray = new Ray(BrickExternal.transform.TransformPoint(verticesExternal[i]) + Vector3.down, Vector3.up);
            if (Physics.Raycast(ray, out hit, 1.0f))
            {
                verticesExternal[i] = BrickExternal.transform.InverseTransformPoint(hit.point + Vector3.down * DeltaSize);
            }
        }
        meshFilterExternal.mesh.vertices = verticesExternal;

        for (int i = 0; i < verticesInternal.Length; i++)
        {
            RaycastHit hit;
            Ray ray = new Ray(BrickInternal.transform.TransformPoint(verticesInternal[i]) + Vector3.down, Vector3.up);
            if (Physics.Raycast(ray, out hit, 1.0f))
            {
                verticesInternal[i] = BrickInternal.transform.InverseTransformPoint(hit.point);
            }
        }
        meshFilterInternal.mesh.vertices = verticesInternal;
    }
}
