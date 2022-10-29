using System.Collections.Generic;
using UnityEngine;

public class CombineCollider : MRigidbody
{
    public CombineCollider(float mass, float restitution, float friction, bool isStatic) : base(mass, restitution,
        friction, isStatic)
    {
    }

    private List<MRigidbody> subRigidbodies = new();
    private List<(Vector2 pos, float angle)> subRigidbodiesOriginPosAndAngle = new();
    
    public void AddRigidbody(params MRigidbody[] rigidbodies)
    {
        subRigidbodies.AddRange(rigidbodies);
        foreach (var rigidbody in rigidbodies)
        {
            subRigidbodiesOriginPosAndAngle.Add((rigidbody.Position, rigidbody.Angle));
        }
    }

    public IReadOnlyList<MRigidbody> GetRigidbodies()
    {
        if (TransformDirty)
        {
            for (int i = 0; i < subRigidbodies.Count; i++)
            {
                var rig = subRigidbodies[i];
                var originPosAngle = subRigidbodiesOriginPosAndAngle[i];
                rig.RotateTo(originPosAngle.angle + Angle);
                rig.MoveTo(Position + originPosAngle.pos);
            }
        }

        return subRigidbodies;
    }

    public override AABB GetAABB()
    {
        return new AABB();
    }

    public override void ForceRefreshTransform()
    {
        foreach (var subRigidbody in subRigidbodies)
        {
           subRigidbody.ForceRefreshTransform();
        } 
    }
}