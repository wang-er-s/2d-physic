using System;
using NUnit.Framework;
using UnityEngine;

public class EditorTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestRaycast()
    {
        MCircleCollider c1 = new MCircleCollider(0.5f, 1, 1, false);
        c1.MoveTo(Vector2.zero);
        MCircleCollider c2 = new MCircleCollider(0.5f, 1, 1, false);
        c2.MoveTo(new Vector2(0, 0.9f));
        Manifold result = PhysicsRaycast.CircleVsCircle(c1, c2);
        Assert.IsTrue(result.Normal == Vector2.up);
        Assert.IsTrue(Math.Abs(result.Penetration - 0.1f) < 0.001f);

        c1 = new MCircleCollider(0.5f, 1, 1, false);
        c1.MoveTo(Vector2.zero);
        c2 = new MCircleCollider(0.5f, 1, 1, false);
        c2.MoveTo(new Vector2(0, 1.1f));
        Assert.IsTrue(PhysicsRaycast.CircleVsCircle(c1, c2) == Manifold.Null);

        MBoxCollider a = new MBoxCollider(Vector2.one, 1, 1, false);
        MBoxCollider b = new MBoxCollider(Vector2.one,1, 1, false);
        b.MoveTo(new Vector2(1,1.1f));
        Assert.IsTrue(PhysicsRaycast.PolygonsRaycast(a, b) == Manifold.Null);

        a = new MBoxCollider(Vector2.one, 1, 1, false);
        b = new MBoxCollider(Vector2.one, 1, 1, false);
        b.MoveTo(new Vector2(0, 0.9f));
        result = PhysicsRaycast.PolygonsRaycast(a, b);
        Assert.IsTrue(result.Normal == Vector2.down);
        Assert.IsTrue(Math.Abs(result.Penetration - 0.1f) < 0.001f);
    }

    [Test]
    public void TestCircleVsCircle()
    {
        MCircleCollider c1 = new MCircleCollider(0.5f, 1, 1, false);
        c1.MoveTo(new Vector2(0, 0.1f));
        MCircleCollider c2 = new MCircleCollider(0.5f, 1, 1, false);
        c2.MoveTo(new Vector2(0, 1));

        var result = PhysicsRaycast.CircleVsCircle(c1, c2);
        Debug.Log(result.Normal);
        Debug.Log(result.Penetration);
    }
}
