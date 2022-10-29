using System;
using UnityEngine;

/// <summary>
/// 碰撞区域
/// </summary>
public struct Manifold : IEquatable<Manifold>
{
     public static Manifold Null = new Manifold() { Penetration = -1 };
     public  MRigidbody R1;
     public  MRigidbody R2;

     /// 渗透深
     public  float Penetration;

     public  Vector2 Normal;

     public  Vector2 Contact1;
     public  Vector2 Contact2;
     public  int ContactCount;

     public Manifold(MRigidbody r1, MRigidbody r2, float penetration, Vector2 normal, Vector2 contact1, Vector2 contact2, int contactCount)
     {
          R1 = r1;
          R2 = r2;
          Penetration = penetration;
          Normal = normal;
          Contact1 = contact1;
          Contact2 = contact2;
          ContactCount = contactCount;
     }

     public override bool Equals(object obj)
     {
          return obj is Manifold other && Equals(other);
     }

     public override int GetHashCode()
     {
          return HashCode.Combine(R1, R2, Penetration, Normal, Contact1, Contact2, ContactCount);
     }

     public static bool operator ==(Manifold m1, Manifold m2)
     {
          return m1.Equals(m2);
     }

     public static bool operator !=(Manifold m1, Manifold m2)
     {
          return !(m1 == m2);
     }

     public bool Equals(Manifold other)
     {
          return Equals(R1, other.R1) && Equals(R2, other.R2) && Penetration.Equals(other.Penetration) &&
                 Normal.Equals(other.Normal) && Contact1.Equals(other.Contact1) && Contact2.Equals(other.Contact2) &&
                 ContactCount == other.ContactCount;
     }
}
