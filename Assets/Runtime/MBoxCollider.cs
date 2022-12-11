using System;
using UnityEngine;

public class MBoxCollider :  MPolygonCollider
{
     public Vector2 Min => Position - range;
     public Vector2 Max => Position + range;
     private readonly Vector2 range;


     public MBoxCollider(Vector2 range, float mass, float restitution, float friction, bool isStatic) : base(mass,
          restitution, friction, isStatic)
     {
          this.range = range;
          float area = range.x * range.y;
          if (area < PhysicsWorld.MinBodySize)
          {
               throw new Exception($"area is too small, min area is {PhysicsWorld.MinBodySize}");
          }

          if (area > PhysicsWorld.MaxBodySize)
          {
               throw new Exception($"area is too large, max area is P{PhysicsWorld.MaxBodySize}");
          }

          var tmpVertexes = new Vector2[4];
          float left = -range.x / 2;
          float right = left + range.x;
          float bottom = -range.y / 2;
          float top = bottom + range.y;

          tmpVertexes[0] = new Vector2(left, top);
          tmpVertexes[1] = new Vector2(right, top);
          tmpVertexes[2] = new Vector2(right, bottom);
          tmpVertexes[3] = new Vector2(left, bottom);

          SetVertexAndTriangles(tmpVertexes);
          Inertia = (1f / 12) * Mass * (range.x * range.x + range.y * range.y);
          if (Inertia == 0)
          {
               InverseInertia = 0;
          }
          else
          {
               InverseInertia = 1 / Inertia;
          }
     }
}
