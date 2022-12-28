using System.Collections.Generic;
using UnityEngine;

public sealed class PhysicsWorld
{
    public const float MinBodySize = 0.01f * 0.01f;
    public const float MaxBodySize = 64f * 64f;

    public QuadTree quadTree;

    private List<MRigidbody> rigidbodies = new();
    private Vector2 gravity;

    public int RigidbodyCount => rigidbodies.Count;

    public PhysicsWorld(float width,float height)
    {
        gravity = new Vector2(0, -9.81f);
        quadTree = new QuadTree(new Rect(-width / 2, -height / 2, width, height),maxBodiesPerNode:5, maxLevel: 10);
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
        rigidbody.MoveTo(new Vector2(collider.transform.position.x, collider.transform.position.z));
        rigidbodies.Add(rigidbody);
        return rigidbody;
    }

    public void AddRigidbody(MRigidbody rigidbody)
    {
        rigidbodies.Add(rigidbody);
        quadTree.AddBody(rigidbody);
        rigidbody.OnPositionChanged += mRigidbody =>
        {
            quadTree.UpdateBody(mRigidbody);
        };
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

        CheckCollideQuad();
    }

    private void ResolveCollision(in Manifold manifold)
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
        // 相对速度在碰撞方向的分量
        float j = -(1f + e) * Vector2.Dot(relativeVelocity, manifold.Normal);
        j /= (r1.InverseMass + r2.InverseMass);
        if (!r1.IsStatic)
            r1.Velocity += j * r1.InverseMass * manifold.Normal;
        if (!r2.IsStatic)
            r2.Velocity -= j * r2.InverseMass * manifold.Normal;
    }
    
    public List<Manifold> contactManifolds = new();

    private void CheckCollideQuad()
    {
        int count = 0;
        contactManifolds.Clear();
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            var rig1 = rigidbodies[i];
            if(rig1.IsStatic) continue;
            var aroundBodies = quadTree.GetBodies(rig1);
            if(aroundBodies.Count <= 1) continue;
            AABB r1aabb = rig1.GetAABB();
            foreach (var rig2 in aroundBodies)
            {
                if(rig2 == rig1) continue;
                if (rig1.IsStatic && rig2.IsStatic) continue;
                Manifold result = Manifold.Null;
                var r2aabb = rig2.GetAABB();
                if (!PhysicsRaycast.AABBIntersect(r1aabb, r2aabb)) continue;
                bool hasContact = false;
                for (int j = 0; j < contactManifolds.Count; j++)
                {
                    var manifold = contactManifolds[j];
                    if ((manifold.R2 == rig1 && manifold.R1 == rig2) || (manifold.R1 == rig1 && manifold.R2 == rig2))
                    {
                        hasContact = true;
                        break;
                    }
                }
                if(hasContact) continue;
                count++;
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
                }

                if (result == Manifold.Null) continue;


                var offset = -result.Normal * (result.Penetration * result.R1.InverseMass /
                                               (result.R1.InverseMass + result.R2.InverseMass));
                result.R1.Move(offset);
                // Debug.Log($"id={result.R1.Id} move:{offset.y} pos={result.R1.Position.y}");

                offset = result.Normal * (result.Penetration * result.R2.InverseMass /
                                          (result.R1.InverseMass + result.R2.InverseMass));
                result.R2.Move(offset);
                // Debug.Log($"id={result.R2.Id} move:{offset.y} pos={result.R2.Position.y}");

                contactManifolds.Add(result);
            }
        }
        Debug.Log(count);
        foreach (var manifold in contactManifolds)
        {
            ResolveCollision(manifold);
        }
    }
    
    private void CheckCollide()
    {
        int count = 0;
        contactManifolds.Clear();
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            var rig1 = rigidbodies[i];
            AABB r1aabb = rig1.GetAABB();
            for (int j = i + 1; j < rigidbodies.Count; j++)
            {
                Manifold result = Manifold.Null;
                var rig2 = rigidbodies[j];
                var r2aabb = rig2.GetAABB();
                if (rig1.IsStatic && rig2.IsStatic) continue;
                if (!PhysicsRaycast.AABBIntersect(r1aabb, r2aabb)) continue;
                count++;
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
                }

                if (result == Manifold.Null) continue;


                var offset = -result.Normal * (result.Penetration * result.R1.InverseMass /
                                               (result.R1.InverseMass + result.R2.InverseMass));
                result.R1.Move(offset);
                // Debug.Log($"id={result.R1.Id} move:{offset.y} pos={result.R1.Position.y}");

                offset = result.Normal * (result.Penetration * result.R2.InverseMass /
                                          (result.R1.InverseMass + result.R2.InverseMass));
                result.R2.Move(offset);
                // Debug.Log($"id={result.R2.Id} move:{offset.y} pos={result.R2.Position.y}");

                contactManifolds.Add(result);
            }
        }

        Debug.Log(count);
        foreach (var manifold in contactManifolds)
        {
            ResolveCollision(manifold);
        }
    }
}