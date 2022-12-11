using System;
using UnityEngine;

public abstract class MRigidbody
{
     private static int increaseId = 0;
     public int Id { get; }
     protected MRigidbody(float mass, float restitution, float friction ,bool isStatic)
     {
          if (mass < 0) throw new Exception("mass must upper 0");
          Id = ++increaseId;
          IsStatic = isStatic;
          this.Friction = friction;
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
               if (isStatic)
               {
                    InverseMass = 0;
                    this.Mass = 0;
               }
               else
               {
                    this.Mass = mass;
                    this.InverseMass = 1 / mass;
               }
          }
     }

     public bool TransformDirty { get; protected set; }
     public Vector2 Position { get; private set; }
     public Vector2 Velocity;
     public float Angle { get; private set; }
     public float RotateVelocity;
     protected AABB AABBCache;
     public bool AABBDirty { get; protected set; }

     protected Vector2 force;
     
     public readonly bool IsStatic;
     /// <summary>
     /// 质量的反
     /// </summary>
     public readonly float InverseMass;
     public readonly float Mass;

     /// <summary>
     ///  脉冲恢复系数(弹力)
     /// </summary>
     public readonly float Restitution;

     /// <summary>
     /// 惯性
     /// </summary>
     public float Inertia { get; protected set; }
     public float InverseInertia { get; protected set; }
     
     /// <summary>
     /// 摩擦力
     /// </summary>
     public float Friction { get; protected set; }

     internal void Update(float deltaTime, Vector2 gravity)
     {
          if (IsStatic) return;
          // force = mass * acc
          // Vector2 acc = force * InverseMass;
          
          Velocity += gravity * deltaTime;
          // Velocity *= 1 - Friction;
          // Velocity = Vector2.zero;
          Move(deltaTime * Velocity);
          Rotate(RotateVelocity * RotateVelocity);
          
          force = Vector2.zero;
     }

     public abstract AABB GetAABB();
     public abstract void ForceRefreshTransform();

     public void AddForce(Vector2 forceVal)
     {
          this.force = forceVal;
     }

     public void Move(Vector2 offset)
     {
          if (offset == Vector2.zero) return;
          MoveTo(Position + offset);
     }

     public virtual void MoveTo(Vector2 pos)
     {
          Position = pos;
          TransformDirty = true;
     }

     public void Rotate(float angle)
     {
          if (angle == 0) return;
          RotateTo(Angle + angle);
     }

     public virtual void RotateTo(float angle)
     {
          Angle = angle;
          Angle %= 360;
          TransformDirty = true;
     }
     
}