using System;
using UnityEngine;

public class MTransform
{
     public Vector2 Position;
}

public class MCollider
{
     public MTransform Transform { get; }
     public MRigidbody Rigidbody { get; }

     public MCollider(MRigidbody rigidbody)
     {
          this.Rigidbody = rigidbody;
          Transform = new MTransform();
     } 
}

public class AABB : MCollider
{

     public Vector2 Min => Transform.Position - range;
     public Vector2 Max => Transform.Position + range;
     private readonly Vector2 range;

     public AABB(Vector2 range, MRigidbody rigidbody = null) : base(rigidbody)
     {
          this.range = range;
     }
}

public class Circle : MCollider
{
     public float Radius;

     public Circle(float radius,MRigidbody rigidbody = null) : base(rigidbody)
     {
          this.Radius = radius;
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
}

public class MRigidbody
{
     public MRigidbody(float mass, float restitution)
     {
          if (mass < 0) throw new Exception("mass must upper 0");
          this.Restitution = Mathf.Clamp(restitution, 0, 1);
          if (mass == 0)
          {
               this.InverseMass = 0;
          }
          else
          {
               this.InverseMass = 1 / mass;
          }
     }

     public Vector2 Position;
     
     public Vector2 Velocity;
     /// <summary>
     /// 质量的反
     /// </summary>
     public float InverseMass { get; }
     /// <summary>
     ///  弹力
     /// </summary>
     public float Restitution { get; }
}