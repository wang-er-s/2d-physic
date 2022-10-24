using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PhysicsWorld
{
    public const float MinBodySize = 0.01f * 0.01f;
    public const float MaxBodySize = 64f * 64f;

    private List<MRigidbody> rigidbodies = new();
    private List<MCircleCollider> circleColliders = new();
    private List<MBoxCollider> boxColliders = new();
    private Vector2 gravity;

    public int RigidbodyCount => rigidbodies.Count;

    public PhysicsWorld()
    {
        gravity = new Vector2(0, -9.81f);
    }

    public MRigidbody GetRigidbody(int index)
    {
        return rigidbodies[index];
    }

    public void RemoveRigidbody(int index)
    {
        rigidbodies.RemoveAt(index);
    }

    public void RemoveRigidbody(MRigidbody rigidbody)
    {
        rigidbodies.Remove(rigidbody);
    }

    public MRigidbody AddRigidbody(Collider collider)
    {
        MRigidbody rigidbody = null;
        if (collider is SphereCollider sphereCollider)
        {
            rigidbody = new MCircleCollider(sphereCollider.radius, sphereCollider.attachedRigidbody.mass, 1, sphereCollider.attachedRigidbody.isKinematic);
            circleColliders.Add(rigidbody as MCircleCollider);
        }else if (collider is BoxCollider boxCollider)
        {
            rigidbody = new MBoxCollider(new Vector2(boxCollider.size.x * 2, boxCollider.size.z * 2),
                boxCollider.attachedRigidbody.mass, 1, boxCollider.attachedRigidbody.isKinematic);
            boxColliders.Add(rigidbody as MBoxCollider);
        }

        rigidbody.MoveTo(new Vector2(collider.transform.position.x, collider.transform.position.z));
        rigidbodies.Add(rigidbody);
        return rigidbody;
    }

    public void AddRigidbody(MRigidbody rigidbody)
    {
        if (rigidbody is MCircleCollider circleCollider)
        {
            circleColliders.Add(circleCollider);
        }else if (rigidbody is MBoxCollider boxCollider)
        {
            boxColliders.Add(boxCollider);
        }
        rigidbodies.Add(rigidbody);
    }

    public void Update(float deltaTime)
    {
        //for (int j = 0; j < 10; j++)
        //{
        //    for (int i = 0; i < rigidbodies.Count; i++)
        //    {
        //        rigidbodies[i].Update(deltaTime / 10, gravity);
        //    }

        //    CheckCollide();
        //}
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            rigidbodies[i].Update(deltaTime, gravity);
        }

        CheckCollide();
    }

    private void ResolveCollision(Manifold manifold)
    {
        if(manifold == Manifold.Null) return;
        MRigidbody r1 = manifold.R1;
        MRigidbody r2 = manifold.R2;
        if(r1.IsStatic && r2.IsStatic) return;
        Vector2 relativeVelocity = r1.Velocity - r2.Velocity;
        if (relativeVelocity == Vector2.zero) return;
        // 如果目标方向已经是分离方向，就不用管了
        if (Vector2.Dot(relativeVelocity, manifold.Normal) < 0)
            return;
        float e = Mathf.Min(r1.Restitution, r2.Restitution);
        float j = -(1f + e) * Vector2.Dot(relativeVelocity, manifold.Normal);
        j /= (r1.InverseMass + r2.InverseMass);
        if (!r1.IsStatic)
            r1.Velocity += j * r1.InverseMass * manifold.Normal;
        if (!r2.IsStatic)
            r2.Velocity -= j * r2.InverseMass * manifold.Normal;
    }
    
    private const float percent = 0.8f; // usually 20% to 80%
    private const float slop = 0.01f; // usually 0.01 to 0.1

    void PositionalCorrection(Manifold manifold)
    {
        var r1 = manifold.R1;
        var r2 = manifold.R2;
        Vector2 correction = Mathf.Max(manifold.Penetration - slop, 0.0f) / (r1.InverseMass + r2.InverseMass) * percent * manifold.Normal;
        r1.Move(-r1.InverseMass * correction);
        r2.Move(r2.InverseMass * correction);
        Debug.Log($"fix {r1.Id} {(-r1.InverseMass * correction).y}");
        Debug.Log($"fix {r2.Id} {(r1.InverseMass * correction).y}");
    }
    
    private void CheckCollide()
    {
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            for (int j = i + 1; j < rigidbodies.Count; j++)
            {
                if (i == j) continue;
                Manifold result = Manifold.Null;
                var rig1 = rigidbodies[i];
                var rig2 = rigidbodies[j];
                if(rig1.IsStatic && rig2.IsStatic) continue;
                switch (rig1)
                {
                    case MPolygonCollider polygonCollider when rig2 is MPolygonCollider polygonCollider2:
                        result = PhysicsRaycast.PolygonsIntersect(polygonCollider, polygonCollider2);
                        break;
                    case MPolygonCollider polygonCollider:
                    {
                        if (rig2 is MCircleCollider circleCollider)
                        {
                            result = PhysicsRaycast.PolygonCircleIntersect(polygonCollider, circleCollider);
                        }

                        break;
                    }
                    case MCircleCollider circleCollider when rig2 is MCircleCollider circleCollider2:
                        result = PhysicsRaycast.CircleIntersect(circleCollider, circleCollider2);
                        break;
                    case MCircleCollider circleCollider:
                    {
                        if (rig2 is MPolygonCollider polygonCollider2)
                        {
                            result = PhysicsRaycast.PolygonCircleIntersect(polygonCollider2, circleCollider);
                        }

                        break;
                    }
                    default:
                        throw new Exception("rigidbody type error");
                }
                
                if (result == Manifold.Null) continue;
                
                if (!result.R1.IsStatic)
                {
                    var offset = -result.Normal * (result.Penetration * result.R1.InverseMass /
                                                   (result.R1.InverseMass + result.R2.InverseMass));
                    // if (offset.x > 0.01f || offset.y > 0.01f)
                    {
                        result.R1.Move(offset);
                        Debug.Log(
                            $"id={result.R1.Id} move:{offset.y} pos={result.R1.Position.y}");
                    }
                }
                
                if (!result.R2.IsStatic)
                {
                    var offset = result.Normal * (result.Penetration * result.R2.InverseMass /
                                                  (result.R1.InverseMass + result.R2.InverseMass));
                    // if (offset.x > 0.01f | offset.y >= 0.01f)
                    {
                        result.R2.Move(offset);
                        Debug.Log(
                            $"id={result.R2.Id} move:{offset.y} pos={result.R2.Position.y}");
                    }
                }
                
                ResolveCollision(result);
                PositionalCorrection(result);
            }
        }
    }
}