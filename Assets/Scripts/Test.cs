using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Test : UnityEngine.MonoBehaviour
{
    public int boxCount;
    public int sphereCount;
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
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        // selfRigidbody.Move(new Vector2(h, v) * (Time.deltaTime * 3));

        Vector2 dir = new Vector2(h, v).normalized;
        selfRigidbody.AddForce(dir * 3);
        
        if (Input.GetKey(KeyCode.Space))
        {
            // selfRigidbody.AddForce();
        }
        
        world.Update(Time.deltaTime);
        for (int i = 0; i < world.RigidbodyCount; i++)
        {
            var rig = world.GetRigidbody(i);
            rig.Rotate(Time.deltaTime * 40);
            var tarPos = rig.Position;
            if (tarPos.x < Min.x)
            {
                tarPos.x = Max.x;
            rig.MoveTo(tarPos);
            }

            if (tarPos.x > Max.x)
            {
                tarPos.x = Min.x;
            rig.MoveTo(tarPos);
            }

            if (tarPos.y < Min.y)
            {
                tarPos.y = Max.y;
            rig.MoveTo(tarPos);
            }

            if (tarPos.y > Max.y)
            {
                tarPos.y = Min.y;
            rig.MoveTo(tarPos);
            }
        }
    }

    private void Gen()
    {
        selfRigidbody = new MBoxCollider(new Vector2(Random.Range(PolygonSize.x,PolygonSize.y), Random.Range(PolygonSize.x,PolygonSize.y)), 1, 1, false);
        world.AddRigidbody(selfRigidbody);
        for (int i = 0; i < boxCount; i++)
        {
            MBoxCollider boxCollider = new MBoxCollider(new Vector2(Random.Range(PolygonSize.x,PolygonSize.y), Random.Range(PolygonSize.x,PolygonSize.y)), 2, 1, Random.Range(1,100) > 50);
            boxCollider.MoveTo(new Vector2(Random.Range(Min.x, Max.x), Random.Range(Min.y, Max.y)));
            world.AddRigidbody(boxCollider);
        }
        
        for (int i = 0; i < sphereCount; i++)
        {
            var cir = new MCircleCollider(Random.Range(CircleSize.x, CircleSize.y), 1, 1, Random.Range(1,100) > 50);
            cir.MoveTo(new Vector2(Random.Range(Min.x, Max.x), Random.Range(Min.y, Max.y)));
            world.AddRigidbody(cir);
        }
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
                    Gizmos.DrawLine(p1, p2);
                    Gizmos.DrawLine(p2, p3);
                    Gizmos.DrawLine(p1, p3);
                }
            }
            else if (rigi is MCircleCollider circleCollider)
            {
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
