using System;
using UnityEngine;


public class MBoxCollider : MRigidbody 
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

public class MRigidbody
{
     protected MRigidbody(float mass, float restitution,bool isStatic)
     {
          if (mass < 0) throw new Exception("mass must upper 0");
          IsStatic = isStatic;
          this.Restitution = Mathf.Clamp(restitution, 0, 1);
          if (mass == 0)
          {
               throw new Exception("mass can not be zero");
          }
          else
          {
               this.InverseMass = 1 / mass;
               this.Mass = mass;
          }
     }

     public Vector2 Position;
     
     public Vector2 Velocity;

     public float Rotation;

     public float RotationVelocity;

     public readonly bool IsStatic;

     /// <summary>
     /// 质量的反
     /// </summary>
     public readonly float InverseMass;

     public readonly float Mass;
     
     /// <summary>
     ///  弹力
     /// </summary>
     public readonly float Restitution;
}