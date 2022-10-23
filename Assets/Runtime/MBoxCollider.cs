using System;
using UnityEngine;

public class MBoxCollider :  MPolygonCollider
{
     public Vector2 Min => Position - range;
     public Vector2 Max => Position + range;
     private readonly Vector2 range;


     public MBoxCollider(Vector2 range, float mass, float restitution, bool isStatic) : base(mass, restitution, isStatic)
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

          baseVectories = new Vector2[4];
          vectories = new Vector2[4];
          float left = -range.x / 2;
          float right = left + range.x;
          float bottom = -range.y / 2;
          float top = bottom + range.y;

          baseVectories[0] = new Vector2(left, top);
          baseVectories[1] = new Vector2(right, top);
          baseVectories[2] = new Vector2(right, bottom);
          baseVectories[3] = new Vector2(left, bottom);

          Array.Copy(baseVectories, vectories, vectories.Length);

          trangles = new[] { 0, 1, 2, 0, 2, 3 };
     }
}

public class MPolygonCollider : MRigidbody
{
     protected Vector2[] baseVectories;
     protected Vector2[] vectories;
     public int[] trangles;
     protected MPolygonCollider(float mass, float restitution, bool isStatic) : base(mass, restitution, isStatic)
     {
     }
     
     public Vector2[] GetVertices()
     {
          if (TransformDirty)
          {
               MTransform transform = new MTransform(Position, Rotation);
               for (int i = 0; i < baseVectories.Length; i++)
               {
                    vectories[i] = transform.Transform(baseVectories[i]);
               }
               TransformDirty = false;
          }

          return vectories;
     }

     public override void MoveTo(Vector2 pos)
     {
          base.MoveTo(pos);
          AABBDirty = true;
     }

     public override void RotateTo(float angle)
     {
          base.RotateTo(angle);
          AABBDirty = true;
     }

     public override AABB GetAABB()
     {
          if (AABBDirty)
          {
               float minX = float.MaxValue;
               float maxX = float.MinValue;
               float minY = float.MaxValue;
               float maxY = float.MinValue;

               var vertices = GetVertices();
               for (int i = 0; i < vertices.Length; i++)
               {
                    var vert = vertices[i];
                    if (vert.x < minX)
                         minX = vert.x;
                    if (vert.x > maxX)
                         maxX = vert.x;
                    if (vert.y < minY)
                         minY = vert.y;
                    if (vert.y > maxY)
                         maxY = vert.y;
               }
               AABBCache = new AABB(minX, maxX, minY, maxY);
               AABBDirty = false;
          }

          return AABBCache;
     }
}

public class MCircleCollider : MRigidbody
{
     public readonly float Radius;

     public MCircleCollider(float radius,float mass, float restitution, bool isStatic) : base(mass, restitution, isStatic)
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
}

/// <summary>
/// 碰撞区域
/// </summary>
public struct Manifold : IEquatable<Manifold>
{
     public static Manifold Null = new Manifold() { Penetration = -1 };
     public MRigidbody R1;
     public MRigidbody R2;

     /// 渗透深度
     public float Penetration;

     public Vector2 Normal;

     public bool Equals(Manifold other)
     {
          return Equals(R1, other.R1) && Equals(R2, other.R2) && Penetration.Equals(other.Penetration) && Normal.Equals(other.Normal);
     }

     public override bool Equals(object obj)
     {
          return obj is Manifold other && Equals(other);
     }

     public override int GetHashCode()
     {
          return HashCode.Combine(R1, R2, Penetration, Normal);
     }

     public static bool operator ==(Manifold m1, Manifold m2)
     {
          return m1.Equals(m2);
     }

     public static bool operator !=(Manifold m1, Manifold m2)
     {
          return !(m1 == m2);
     }
}

public abstract class MRigidbody
{
     protected MRigidbody(float mass, float restitution,bool isStatic)
     {
          if (mass < 0) throw new Exception("mass must upper 0");
          IsStatic = isStatic;
          force = Vector2.zero;
          TransformDirty = true;
          AABBDirty = true;
          this.Restitution = Mathf.Clamp(restitution, 0, 1);
          if (mass == 0)
          {
               throw new Exception("mass can not be zero");
          }
          else
          {
               this.InverseMass = 1 / mass;
               if (isStatic)
                   InverseMass = 0;
          }
     }

     public bool TransformDirty { get; protected set; }
     public Vector2 Position { get; private set; }
     public Vector2 Velocity;
     public float Rotation { get; private set; }
     public float RotationVelocity;
     protected AABB AABBCache;
     public bool AABBDirty { get; protected set; }

     protected Vector2 force;
     
     public readonly bool IsStatic;
     /// <summary>
     /// 质量的反
     /// </summary>
     public readonly float InverseMass;

     /// <summary>
     ///  脉冲恢复弹力
     /// </summary>
     public readonly float Restitution;

     internal void Update(float deltaTime, Vector2 gravity)
     {
          if (IsStatic) return;
          // force = mass * acc
          // Vector2 acc = force * InverseMass;
          Velocity += gravity * deltaTime;
          Move(deltaTime * Velocity);
          Rotate(RotationVelocity * RotationVelocity);
          
          force = Vector2.zero;
     }

     public abstract AABB GetAABB();

     public void AddForce(Vector2 forceVal)
     {
          this.force = forceVal;
     }

     public void Move(Vector2 offset)
     {
          MoveTo(Position + offset);
     }

     public virtual void MoveTo(Vector2 pos)
     {
          Position = pos;
          TransformDirty = true;
     }

     public void Rotate(float angle)
     {
         RotateTo(Rotation + angle); 
     }

     public virtual void RotateTo(float angle)
     {
          Rotation = angle;
          Rotation %= 360;
          TransformDirty = true;
     }
}

public struct AABB
{
     public Vector2 Min;
     public Vector2 Max;
     
     public AABB(Vector2 min, Vector2 max)
     {
          Min = min;
          Max = max;
     }

     public AABB(float minx,float maxX, float minY, float maxY)
     {
          Min = new Vector2(minx, minY);
          Max = new Vector2(maxX, maxY);
     }
}