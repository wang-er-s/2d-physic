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

    public int RigidbodyCount => rigidbodies.Count;

    public MRigidbody GetRigidbody(int index)
    {
        return rigidbodies[index];
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
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            rigidbodies[i].Update(deltaTime);
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
        float e = Mathf.Min(r1.Restitution, r2.Restitution);
        float j = -(1f + e) * Vector2.Dot(relativeVelocity, manifold.Normal);
        j /= (r1.InverseMass + r2.InverseMass);
        if (!r1.IsStatic)
            r1.Velocity += j * r1.InverseMass * manifold.Normal;
        if (!r2.IsStatic)
            r2.Velocity -= j * r2.InverseMass * manifold.Normal;
    }
    
    private void CheckCollide()
    {
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            for (int j = 0; j < rigidbodies.Count; j++)
            {
                if (i == j) continue;
                Manifold result = Manifold.Null;
                var rig1 = rigidbodies[i];
                var rig2 = rigidbodies[j];
                if(rig1.IsStatic && rig2.IsStatic) continue;
                switch (rig1)
                {
                    case MPolygonCollider polygonCollider when rig2 is MPolygonCollider polygonCollider2:
                        result = PhysicsRaycast.PolygonsRaycast(polygonCollider, polygonCollider2);
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
                        result = PhysicsRaycast.CircleVsCircle(circleCollider, circleCollider2);
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
                if(result == Manifold.Null) continue;
                if (!result.R1.IsStatic)
                {
                    result.R1.Move(-result.Normal *
                                   (result.Penetration * result.R1.InverseMass /
                                    (result.R1.InverseMass + result.R2.InverseMass)));
                }

                if (!result.R2.IsStatic)
                {
                    result.R2.Move(result.Normal *
                                   (result.Penetration * result.R2.InverseMass /
                                    (result.R1.InverseMass + result.R2.InverseMass)));
                }
                
                ResolveCollision(result);
            }
        }
    }
}