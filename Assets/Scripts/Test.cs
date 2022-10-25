using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
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
        world = new PhysicsWorld();
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
                2, 0.3f, false);
            boxCollider.MoveTo(GetMousePos());
            world.AddRigidbody(boxCollider);
        }

        if (Input.GetMouseButtonDown(1))
        {
            var cir = new MCircleCollider(Random.Range(CircleSize.x, CircleSize.y), 1, 0.3f, false);
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
        MPolygonCollider polygonCollider = new MPolygonCollider(2, 0.3f, false);
        Vector2[] vertexes = new[] { new Vector2(-0.707f, 0), new Vector2(0, 1), new Vector2(0.707f, 0) };
        int[] trangle = new[] { 0, 1, 2 };
        polygonCollider.SetVertexAndTriangles(vertexes, trangle);
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

    private void Gen()
    {
        MBoxCollider ground = new MBoxCollider(new Vector2(Max.x - Min.x, 1), 1, 1, true);
        ground.MoveTo(new Vector2(0, Min.y + 0.5f));
        world.AddRigidbody(ground);
    }

    #region Draw

    private void OnDrawGizmos()
    {
        if(world == null) return;
        Gizmos.color = Color.black;
        for (int w = 0; w < world.RigidbodyCount; w++)
        {
            var rigi = world.GetRigidbody(w);
            if (rigi is MPolygonCollider boxCollider)
            {
                var vertices = boxCollider.GetVertices();
                for (var i = 0; i < boxCollider.triangles.Length; i += 3)
                {
                    Vector3 p1 = new Vector3(vertices[boxCollider.triangles[i]].x, 0,
                        vertices[boxCollider.triangles[i]].y);
                    Vector3 p2 = new Vector3(vertices[boxCollider.triangles[i + 1]].x, 0,
                        vertices[boxCollider.triangles[i + 1]].y);
                    Vector3 p3 = new Vector3(vertices[boxCollider.triangles[i + 2]].x, 0,
                        vertices[boxCollider.triangles[i + 2]].y);
                    Gizmos.color = rigi.IsStatic ? Color.red : Color.black;
                    Gizmos.DrawLine(p1, p2);
                    Gizmos.DrawLine(p2, p3);
                    Gizmos.DrawLine(p1, p3);
                }
            }
            else if (rigi is MCircleCollider circleCollider)
            {
                Gizmos.color = rigi.IsStatic ? Color.red : Color.black;
                Vector3 pos = new Vector3(circleCollider.Position.x, 0, circleCollider.Position.y);
                DrawGizmosCircle(pos, circleCollider.Radius, 20);
            }

            Vector3 pos1 = new Vector3(rigi.Position.x, 0, rigi.Position.y);
            Handles.Label(pos1, rigi.Id.ToString());
        } 
        
        Gizmos.color = Color.green;
        foreach (var manifold in world.contactManifolds)
        {
            if (manifold.ContactCount >= 1)
            {
                Vector3 pos = new Vector3(manifold.Contact1.x, 0, manifold.Contact1.y);
                DrawGizmosCircle(pos, 0.2f, 5);
                if (manifold.ContactCount >= 2)
                {
                    Vector3 pos2 = new Vector3(manifold.Contact2.x, 0, manifold.Contact2.y);
                    DrawGizmosCircle(pos2, 0.2f, 5);
                }
            }
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
