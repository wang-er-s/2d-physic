using System;
using UnityEngine;

public class MCircleCollider : MRigidbody
{
     public readonly float Radius;

     public MCircleCollider(float radius, float mass, float restitution, float friction, bool isStatic) : base(mass,
          restitution, friction, isStatic)
     {
          Radius = radius;
          float area = radius * radius * Mathf.PI;
          if (area < PhysicsWorld.MinBodySize)
          {
               throw new Exception($"area is too small, min area is {PhysicsWorld.MinBodySize}");
          }

          if (area > PhysicsWorld.MaxBodySize)
          {
               throw new Exception($"area is too large, max area is P{PhysicsWorld.MaxBodySize}");
          }

          Inertia = 1f / 2f * Mass * Radius * Radius;
          InverseInertia = 1 / Inertia;
     }

     public override void MoveTo(Vector2 pos)
     {
          base.MoveTo(pos);
          AABBDirty = true;
     }

     public override AABB GetAABB()
     {
          if (AABBDirty)
          {
               AABBCache = new AABB(Position.x - Radius, Position.x + Radius, Position.y - Radius, Position.y + Radius);
               AABBDirty = false;
          }

          return AABBCache;
     }

     public override void ForceRefreshTransform()
     {
     }
}
