using UnityEngine;
using UnityEngine.UI;

public class TableController : MonoBehaviour
{

    // размеры бруска
    public Vector3 BlockSize = new Vector3(0.3f, 0.2f, 0.5f);
    // размеры ножа
    public Vector3 KnifeSize = new Vector2(0.01f, 0.01f);
    // максимальная глубина резки
    public float MaxCutDepth = 0.02f;
    // разница в резмерах внешнего и внутреннего меша
    public float DeltaSize = 0.001f;

    // коллайдер ножа
    public Collider Knife;
    // скорость ножа
    public float Speed = 1.0f;
    // внешний и внутренний меши
    public MeshFilter MeshExternal, MeshInternal;

    // уровень подразделения на полигоны
    public int SubdivideLevel = 50;

    // опилки
    public ParticleSystem Particles;

    // список контрольных точек для стола
    public Vector3[] points;

    // сообщение
    public string Message = "";

    // интерфейс
    public Text posX, posY, posZ;
    public Text statement;

    // текущая точка
    private int currentPoint = 0;

    // массивы вершин
    private Vector3[] verticesOrigin, verticesExternal, verticesInternal;
    private float groundLevelExternal;

    private bool newCut = true;
    private bool reset = true;

    void Awake()
    {
        // устанавливаем фреймрейт
        Application.targetFrameRate = 600;

        // выставляем размеры мешей
        MeshExternal.transform.localScale = BlockSize;
        MeshInternal.transform.localScale = BlockSize * (1.0f - DeltaSize);

        // устанавливаем позиции мешей
        MeshExternal.transform.position = transform.position + Vector3.up * (BlockSize.y / 2);
        MeshInternal.transform.position = MeshExternal.transform.position;

        // делим меши
        MeshHelper.Subdivide(MeshExternal.mesh, SubdivideLevel);
        MeshHelper.Subdivide(MeshInternal.mesh, SubdivideLevel);

        // кешируем вершины
        verticesOrigin = MeshExternal.mesh.vertices;
        verticesExternal = MeshExternal.mesh.vertices;
        verticesInternal = MeshInternal.mesh.vertices;

        groundLevelExternal = MeshExternal.transform.TransformPoint(MeshExternal.transform.position - MeshExternal.mesh.bounds.extents).y;
    }

    void Update()
    {
        // показать на экране координаты и статус
        ShowPosition();
        ShowStatement();

        // если новый процесс, заново выставляем параметры
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

                transform.position = points[0];

                reset = false;
            }

            Knife.transform.localScale = new Vector3(KnifeSize.x, Knife.transform.localScale.y, KnifeSize.z);
            currentPoint = 0;

            newCut = false;
        }

        // пока не дошли до последней точки
        if (currentPoint < points.Length)
        {
            // двигаемся к следующей точке
            transform.position = Vector3.MoveTowards(transform.position, points[currentPoint], Speed * Time.deltaTime);
            if (transform.position == points[currentPoint]) currentPoint++;

            var knifeBounds = MeshInternal.transform.InverseTransformBounds(Knife.bounds);
            var knifeBottom = MeshInternal.transform.InverseTransformPoint(Knife.transform.position - Knife.bounds.extents).y;
            var maxCut = MaxCutDepth / MeshInternal.transform.localScale.y;

            var cores = SystemInfo.processorCount;
            var verticesPerCore = (verticesExternal.Length - 1) / cores + 1;

            // параллельно рассчитываем пересечения ножа с вершинами и корректируем меш
            bool changed = false;
            Parallel.For(cores, (core) =>
            {
                var start = core * verticesPerCore;
                var end = Mathf.Min((core + 1) * verticesPerCore, verticesExternal.Length);

                for (int i = start; i < end; i++)
                    if (knifeBounds.Contains(verticesInternal[i]))
                    {
                        // проверяем не превышается ли максимальная глубина резки
                        if (verticesInternal[i].y - knifeBottom <= maxCut)
                        {
                            verticesInternal[i].y = knifeBottom;
                            verticesExternal[i].y = groundLevelExternal;
                            changed = true;
                        }
                        else
                        {
                            Message = "Нож входит слишком глубоко";
                            currentPoint = points.Length;
                            Debug.Log(verticesInternal[i].y - knifeBottom + " " + maxCut);
                            break;
                        }
                    }
            });

            // включаем или отключаем щепки
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
        BlockSize = blockSize * 0.999f;
        KnifeSize = new Vector3(knifeSize.x * 1.001f, 0.0f, knifeSize.z * 1.001f);
        MaxCutDepth = knifeSize.y + 0.01f;
        this.points = points;
    }

    public void ShowPosition()
    {
        posX.text = transform.position.x.ToString();
        posY.text = transform.position.y.ToString();
        posZ.text = transform.position.z.ToString();
    }

    public void ShowStatement()
    {
        if (Message == "") statement.text = "Идет работа...";
        else statement.text = Message;
    }


}
