using UnityEngine;

public class TableController : MonoBehaviour
{

    public Vector3 BlockSize = new Vector3(0.3f, 0.2f, 0.5f);
    public Vector3 KnifeSize = new Vector2(0.01f, 0.01f);
    public float MaxCutDepth = 0.02f;
    public float DeltaSize = 0.001f;

    public Collider Knife;
    public float Speed = 1.0f;
    public MeshFilter MeshExternal, MeshInternal;

    public int SubdivideLevel = 50;

    public ParticleSystem Particles;

    public Vector3[] points;

    public string Message = "";

    private int currentPoint = 0;

    private Vector3[] verticesOrigin, verticesExternal, verticesInternal;
    private float groundLevelExternal;

    private bool newCut = true;
    private bool reset = true;

    void Awake()
    {
        Application.targetFrameRate = 600;

        MeshExternal.transform.localScale = BlockSize;
        MeshInternal.transform.localScale = BlockSize * (1.0f - DeltaSize);

        MeshExternal.transform.position = transform.position + Vector3.up * (BlockSize.y / 2);
        MeshInternal.transform.position = MeshExternal.transform.position;

        MeshHelper.Subdivide(MeshExternal.mesh, SubdivideLevel);
        MeshHelper.Subdivide(MeshInternal.mesh, SubdivideLevel);

        verticesOrigin = MeshExternal.mesh.vertices;
        verticesExternal = MeshExternal.mesh.vertices;
        verticesInternal = MeshInternal.mesh.vertices;

        groundLevelExternal = (MeshExternal.transform.position - MeshExternal.mesh.bounds.extents).y;
    }

    void Update()
    {
        if (newCut)
        {
            Message = "";
            if (reset)
            {
                MeshExternal.transform.localScale = BlockSize;
                MeshInternal.transform.localScale = BlockSize * (1.0f - DeltaSize);

                MeshExternal.transform.position = transform.position + Vector3.up * (BlockSize.y / 2);
                MeshInternal.transform.position = MeshExternal.transform.position;

                MeshExternal.mesh.vertices = verticesOrigin;
                MeshInternal.mesh.vertices = verticesOrigin;
                verticesExternal = MeshExternal.mesh.vertices;
                verticesInternal = MeshInternal.mesh.vertices;

                reset = false;
            }

            Knife.transform.localScale = new Vector3(KnifeSize.x, Knife.transform.localScale.y, KnifeSize.z);
            transform.position = points[0];
            currentPoint = 1;

            newCut = false;
        }

        if (currentPoint < points.Length)
        {
            transform.position = Vector3.MoveTowards(transform.position, points[currentPoint], Speed);
            if (transform.position == points[currentPoint]) currentPoint++;

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
                        if (verticesExternal[i].y - knifeBottom <= MaxCutDepth)
                        {
                            verticesInternal[i].y = knifeBottom;
                            verticesExternal[i].y = groundLevelExternal;
                            changed = true;
                        }
                        else
                        {
                            Message = "Нож входит слишком глубоко";
                            currentPoint = points.Length;
                            break;
                        }
                    }
            });

            var emission = Particles.emission;
            if (changed)
            {
                MeshInternal.mesh.vertices = verticesInternal;
                MeshExternal.mesh.vertices = verticesExternal;
                emission.enabled = true;
            }
            else
            {
                emission.enabled = false;
            }
        }
        else
        {
            if (Message.Length == 0) Message = "Работа завершена";
            var emission = Particles.emission;
            emission.enabled = false;
        }
    }

    public void StartCutting(bool reset, Vector3 blockSize, Vector3 knifeSize, Vector3[] points)
    {
        newCut = true;

        this.reset = reset;
        BlockSize = blockSize;
        KnifeSize = new Vector2(knifeSize.x, knifeSize.y);
        MaxCutDepth = knifeSize.y;
        this.points = points;
    }
}
