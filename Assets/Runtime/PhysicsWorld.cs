using System.Collections.Generic;
using UnityEngine;

public sealed class PhysicsWorld
{
    public const float MinBodySize = 0.01f * 0.01f;
    public const float MaxBodySize = 64f * 64f;

    private List<MRigidbody> rigidbodies = new();
    private List<MCircleCollider> circleColliders = new();
    private List<MBoxCollider> boxColliders = new();

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

    public void Update()
    {
        foreach (var mCircleCollider in circleColliders)
        {
            foreach (var circleCollider in circleColliders)
            {
                if(mCircleCollider == circleCollider) continue;
                Manifold result = PhysicsRaycast.CircleVsCircle(mCircleCollider, circleCollider);
                if(result == Manifold.Null) continue;
                result.R1.Move(-result.Normal *
                               (result.Penetration * result.R1.InverseMass /
                                (result.R1.InverseMass + result.R2.InverseMass)));
                result.R2.Move(result.Normal *
                               (result.Penetration * result.R2.InverseMass /
                                (result.R1.InverseMass + result.R2.InverseMass)));
            }
        }

        foreach (var box1 in boxColliders)
        {
            foreach (var box2 in boxColliders)
            {
                if(box1 == box2) continue;
                Manifold result = PhysicsRaycast.PolygonsRaycast(box1, box2);
                if(result == Manifold.Null) continue;
                result.R1.Move(-result.Normal *
                               (result.Penetration * result.R1.InverseMass /
                                (result.R1.InverseMass + result.R2.InverseMass)));
                result.R2.Move(result.Normal *
                               (result.Penetration * result.R2.InverseMass /
                                (result.R1.InverseMass + result.R2.InverseMass)));
            }
        }

        foreach (var mBoxCollider in boxColliders)
        {
            foreach (var mCircleCollider in circleColliders)
            {
                Manifold result = PhysicsRaycast.PolygonCircleIntersect(mBoxCollider, mCircleCollider);
                if (result == Manifold.Null) continue;
                result.R1.Move(-result.Normal *
                               (result.Penetration * result.R1.InverseMass /
                                (result.R1.InverseMass + result.R2.InverseMass)));
                result.R2.Move(result.Normal *
                               (result.Penetration * result.R2.InverseMass /
                                (result.R1.InverseMass + result.R2.InverseMass)));
            }
        }
    }
}