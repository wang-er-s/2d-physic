using NUnit.Framework;
using UnityEngine;

public class EditorTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestRaycast()
    {
        MCircleCollider c1 = new MCircleCollider(1, 1, 1, false);
        c1.Position = Vector2.zero;
        MCircleCollider c2 = new MCircleCollider(0.5f, 1, 1, false);
        c2.Position = Vector2.one;
        Assert.IsTrue(PhysicsRaycast.CircleVsCircle(c1, c2) != Manifold.Null);

        c1 = new MCircleCollider(0.5f, 1, 1, false);
        c1.Position = Vector2.zero;
        c2 = new MCircleCollider(0.5f, 1, 1, false);
        c2.Position = Vector2.one;
        Assert.IsFalse(PhysicsRaycast.CircleVsCircle(c1, c2) != Manifold.Null);

        MBoxCollider a = new MBoxCollider(Vector2.one * 0.5f, 1, 1, false);
        MBoxCollider b = new MBoxCollider(Vector2.one * 0.25f,1, 1, false);
        b.Position = Vector2.one * 0.75f;
        Assert.IsTrue(PhysicsRaycast.AABBRaycast(a,b));

        a = new MBoxCollider(Vector2.one * 0.5f, 1, 1, false);
        b = new MBoxCollider(Vector2.one * 0.45f, 1, 1, false);
        b.Position = Vector2.one * 1.5f;
        Assert.IsFalse(PhysicsRaycast.AABBRaycast(a,b));
    }

    [Test]
    public void TestCircleVsCircle()
    {
        MCircleCollider c1 = new MCircleCollider(0.5f, 1, 1, false);
        c1.Position = new Vector2(0, 0.1f);
        MCircleCollider c2 = new MCircleCollider(0.5f, 1, 1, false);
        c2.Position = new Vector2(0,1);

        var result = PhysicsRaycast.CircleVsCircle(c1, c2);
        Debug.Log(result.Normal);
        Debug.Log(result.Penetration);
    }
}
