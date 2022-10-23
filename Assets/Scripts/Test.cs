using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
        Gen();
    }

    private void Update()
    {
        world.Update(Time.deltaTime);
        for (int i = 0; i < world.RigidbodyCount; i++)
        {
            var aabb = world.GetRigidbody(i).GetAABB();
            if (aabb.Min.y < Min.y)
            {
                world.RemoveRigidbody(i);
                Debug.Log($"remove {i}  {aabb.Min}");
                i--;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            MBoxCollider boxCollider = new MBoxCollider(
                new Vector2(Random.Range(PolygonSize.x, PolygonSize.y), Random.Range(PolygonSize.x, PolygonSize.y)),
                2, 0.6f, false);
            boxCollider.MoveTo(GetMousePos());
            world.AddRigidbody(boxCollider);
        }

        if (Input.GetMouseButtonDown(1))
        {
            var cir = new MCircleCollider(Random.Range(CircleSize.x, CircleSize.y), 1, 0.6f, false);
            cir.MoveTo(GetMousePos());
            world.AddRigidbody(cir);
        }
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
            if (rigi is MBoxCollider boxCollider)
            {
                var vertices = boxCollider.GetVertices();
                for (var i = 0; i < boxCollider.trangles.Length; i += 3)
                {
                    Vector3 p1 = new Vector3(vertices[boxCollider.trangles[i]].x, 0,
                        vertices[boxCollider.trangles[i]].y);
                    Vector3 p2 = new Vector3(vertices[boxCollider.trangles[i + 1]].x, 0,
                        vertices[boxCollider.trangles[i + 1]].y);
                    Vector3 p3 = new Vector3(vertices[boxCollider.trangles[i + 2]].x, 0,
                        vertices[boxCollider.trangles[i + 2]].y);
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
