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

    private List<MRigidbody> rigidbodies = new();
    private List<MBoxCollider> boxColliders = new();
    private List<MCircleCollider> circleColliders = new();

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
        selfRigidbody.Move(new Vector2(h, v) * (Time.deltaTime * 3));

        world.Update();
        foreach (var boxCollider in boxColliders)
        {
            boxCollider.Rotate(Time.deltaTime * 40);
        }
    }

    private bool HasCast(MBoxCollider collider)
    {
        foreach (var boxCollider in boxColliders)
        {
            if(boxCollider == collider) continue;
            if (PhysicsRaycast.PolygonsRaycast(collider, boxCollider) != Manifold.Null)
                return true;
        }
        return false;
    }

    private void Gen()
    {
        selfRigidbody = new MBoxCollider(new Vector2(Random.Range(PolygonSize.x,PolygonSize.y), Random.Range(PolygonSize.x,PolygonSize.y)), 1, 1, false);
        rigidbodies.Add(selfRigidbody);
        boxColliders.Add(selfRigidbody as MBoxCollider);
        world.AddRigidbody(selfRigidbody);
        for (int i = 0; i < boxCount; i++)
        {
            MBoxCollider boxCollider = new MBoxCollider(new Vector2(Random.Range(PolygonSize.x,PolygonSize.y), Random.Range(PolygonSize.x,PolygonSize.y)), 2, 1, false);
            boxCollider.MoveTo(new Vector2(Random.Range(Min.x, Max.x), Random.Range(Min.y, Max.y)));
            rigidbodies.Add(boxCollider);
            boxColliders.Add(boxCollider);
            world.AddRigidbody(boxCollider);
        }
        
        for (int i = 0; i < sphereCount; i++)
        {
            var cir = new MCircleCollider(Random.Range(CircleSize.x, CircleSize.y), 1, 1, false);
            cir.MoveTo(new Vector2(Random.Range(Min.x, Max.x), Random.Range(Min.y, Max.y)));
            rigidbodies.Add(cir);
            circleColliders.Add(cir);
            world.AddRigidbody(cir);
        }
    }

    #region Draw

    private void OnDrawGizmos()
    {
        if(rigidbodies == null) return;
        foreach (var rigi in rigidbodies)
        {
            if (rigi is MBoxCollider boxCollider)
            {
                bool hasCast = HasCast(boxCollider);
                hasCast = false;
                var vertices = boxCollider.GetVertices();
                for (var i = 0; i < boxCollider.trangles.Length; i += 3)
                {
                    Vector3 p1 = new Vector3(vertices[boxCollider.trangles[i]].x, 0,
                        vertices[boxCollider.trangles[i]].y);
                    Vector3 p2 = new Vector3(vertices[boxCollider.trangles[i + 1]].x, 0,
                        vertices[boxCollider.trangles[i + 1]].y);
                    Vector3 p3 = new Vector3(vertices[boxCollider.trangles[i + 2]].x, 0,
                        vertices[boxCollider.trangles[i + 2]].y);
                    Gizmos.color = hasCast ? Color.red : Color.black;
                    Gizmos.DrawLine(p1, p2);
                    Gizmos.DrawLine(p2, p3);
                    Gizmos.DrawLine(p1, p3);
                }
            }
            else if (rigi is MCircleCollider circleCollider)
            {
                Gizmos.color = Color.black;
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
