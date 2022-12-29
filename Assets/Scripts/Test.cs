using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Test : UnityEngine.MonoBehaviour
{
    public Vector2 Min;
    public Vector2 Max;

    public Vector2 CircleSize;
    public Vector2 PolygonSize;

    private PhysicsWorld world;

    private MRigidbody selfRigidbody;

    private void Awake()
    {
        world = new PhysicsWorld(40,24);
        sw = Stopwatch.StartNew();
        Gen();
    }

    private double calcTime;
    private void FixedUpdate()
    {
        sw.Restart();
        world.Update(Time.fixedDeltaTime);
        sw.Stop();
        timer += Time.fixedDeltaTime;
        if (timer >= 0.5f)
        {
            calcTime = (sw.ElapsedTicks * 1.0f / Stopwatch.Frequency) * 1000f;
            timer = 0;
        }
    }

    private float timer;
    private void OnGUI()
    {
        GUI.color = Color.green;
        GUI.skin.label.fontSize = 40;
        GUI.Label(new Rect(100, 100, 300, 200), $"count:{world.RigidbodyCount} time:{calcTime:0.0000}");
    }

    private Stopwatch sw;
    private void Update()
    {
        // print($"count:{world.RigidbodyCount}  time:{sw.ElapsedMilliseconds}");
        for (int i = 0; i < world.RigidbodyCount; i++)
        {
            var aabb = world.GetRigidbody(i).GetAABB();
            if (aabb.Min.y < Min.y)
            {
                world.RemoveRigidbody(i);
                i--;
            }
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            MBoxCollider boxCollider = new MBoxCollider(
                new Vector2(Random.Range(PolygonSize.x, PolygonSize.y), Random.Range(PolygonSize.x, PolygonSize.y)),
                2, 0.3f, 0.1f,false);
            boxCollider.MoveTo(GetMousePos());
            world.AddRigidbody(boxCollider);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            combineCollider.Rotate(5);
        }

        // Vector2 move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // selfRigidbody.Move(move * (Time.deltaTime * 5));

        if (Input.GetMouseButtonDown(1))
        {
            var cir = new MCircleCollider(Random.Range(CircleSize.x, CircleSize.y), 1, 0.3f,0.1f, false);
            cir.MoveTo(GetMousePos());
            world.AddRigidbody(cir);
        }

        if (Input.GetMouseButtonDown(2))
        {
            MPolygonCollider polygonCollider = CreateTriangle();
            polygonCollider.MoveTo(GetMousePos());
            world.AddRigidbody(polygonCollider);
        }
    }

    private MPolygonCollider CreateTriangle()
    {
        MPolygonCollider polygonCollider = new MPolygonCollider(2, 0.3f, 0.1f,false);
        Vector2[] vertexes = new[] { new Vector2(-0.707f, 0), new Vector2(0, 1), new Vector2(0.707f, 0) };
        polygonCollider.SetVertexAndTriangles(vertexes);
        return polygonCollider;
    }

    private Vector2 GetMousePos()
    {
        //获取鼠标在相机中（世界中）的位置，转换为屏幕坐标；
        var screenPosition = Camera.main.WorldToScreenPoint(transform.position);
//获取鼠标在场景中坐标
        var mousePositionOnScreen = Input.mousePosition;
//让场景中的Z=鼠标坐标的Z
        mousePositionOnScreen.z = screenPosition.z;
//将相机中的坐标转化为世界坐标
        var mousePositionInWorld = Camera.main.ScreenToWorldPoint(mousePositionOnScreen);
        return new Vector2(mousePositionInWorld.x, mousePositionInWorld.z);
    }

    private CombineCollider combineCollider;
    private void Gen()
    {
        MBoxCollider ground = new MBoxCollider(new Vector2(Max.x - Min.x, 1), 1, 1, 0.1f,true);
        ground.MoveTo(new Vector2(0, Min.y + 0.5f));
        world.AddRigidbody(ground);

        selfRigidbody = new MBoxCollider(new Vector2(2, 2), 1, 0.5f, 0.1f, false);
        world.AddRigidbody(selfRigidbody);

        float height = Max.y - Min.y;
        float width = Max.x - Min.x;
        // MBoxCollider wall = new MBoxCollider(new Vector2((Max.x - Min.x) / 2.5f, 1), 1, 1, 0.1f,true);
        // wall.MoveTo(new Vector2(Min.x + width / 3, Max.y - height / 2));
        // wall.Rotate(-10);
        // world.AddRigidbody(wall);
        //
        // MBoxCollider wall2 = new MBoxCollider(new Vector2((Max.x - Min.x) / 2.5f, 1), 1, 1, 0.1f,true);
        // wall2.MoveTo(new Vector2(Min.x + width / 3 * 2, Max.y - height / 4));
        // wall2.Rotate(10);
        // world.AddRigidbody(wall2);

        // CombineCollider collider = new CombineCollider(1, 1, 1, false);
        // collider.MoveTo(Vector2.zero);
        // MBoxCollider b1 = new MBoxCollider(Vector2.one, 1, 1, 1, true);
        // b1.MoveTo(Vector2.zero);
        // MBoxCollider b2 = new MBoxCollider(Vector2.one, 1, 1, 1, true);
        // b2.MoveTo(new Vector2(0.5f,0.5f));
        // b2.RotateTo(45);
        // collider.AddRigidbody(b1,b2);
        // combineCollider = collider;
        // world.AddRigidbody(collider);
    }

    #region Draw

    private void OnDrawGizmos()
    {
        if(world == null) return;
        // world.quadTree.DrawGizmos();
        Gizmos.color = Color.black;
        for (int w = 0; w < world.RigidbodyCount; w++)
        {
            var rigi = world.GetRigidbody(w);
            if (rigi is MPolygonCollider boxCollider)
            {
                DrawPolygon(boxCollider);
            }
            else if (rigi is MCircleCollider circleCollider)
            {
                DrawCircle(circleCollider);
            }
            else if (rigi is CombineCollider combineCollider)
            {
                foreach (var mRigidbody in combineCollider.GetRigidbodies())
                {
                    if (mRigidbody is MPolygonCollider boxCollider1)
                    {
                        DrawPolygon(boxCollider1);
                    }
                    else if (mRigidbody is MCircleCollider circleCollider1)
                    {
                        DrawCircle(circleCollider1);
                    }
                }
            }

            Vector3 pos1 = new Vector3(rigi.Position.x, 0, rigi.Position.y);
            Handles.Label(pos1, rigi.Id.ToString());
        } 
    }

    private static void DrawCircle(MCircleCollider circleCollider)
    {
        Gizmos.color = circleCollider.IsStatic ? Color.red : Color.black;
        Vector3 pos = new Vector3(circleCollider.Position.x, 0, circleCollider.Position.y);
        DrawGizmosCircle(pos, circleCollider.Radius, 20);
    }

    private static void DrawPolygon(MPolygonCollider polygonCollider)
    {
        var vertices = polygonCollider.GetVertices();
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 p1 = new Vector3(vertices[i].x, 0, vertices[i].y);
            Vector3 p2 = new Vector3(vertices[(i + 1) % vertices.Length].x, 0,
                vertices[(i + 1) % vertices.Length].y);
            Gizmos.color = polygonCollider.IsStatic ? Color.red : Color.black;
            Gizmos.DrawLine(p1, p2);
        }
    }

    private static void DrawGizmosCircle(Vector3 pos, float radius, int numSegments)
    {
        Vector3 normal = Vector3.up;
        Vector3 temp = (normal.x < normal.z) ? new Vector3(1f, 0f, 0f) : new Vector3(0f, 0f, 1f);
        Vector3 forward = Vector3.Cross(normal, temp).normalized;
        Vector3 right = Vector3.Cross(forward, normal).normalized;
 
        Vector3 prevPt = pos + (forward * radius);
        float angleStep = (Mathf.PI * 2f) / numSegments;
        for (int i = 0; i < numSegments; i++)
        {
            float angle = (i == numSegments - 1) ? 0f : (i + 1) * angleStep;
            Vector3 nextPtLocal = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
            Vector3 nextPt = pos + (right * nextPtLocal.x) + (forward * nextPtLocal.z);
            Gizmos.DrawLine(prevPt, nextPt);
            prevPt = nextPt;
        }
    }
    
    #endregion
}