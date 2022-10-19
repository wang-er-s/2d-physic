using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EditorTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestRaycast()
    {
        Circle c1 = new Circle(1);
        c1.Transform.Position = Vector2.zero;
        Circle c2 = new Circle(0.5f);
        c2.Transform.Position = Vector2.one;
        Assert.IsTrue(PhysicRaycast.CircleRaycast(c1, c2));

        c1 = new Circle(0.5f);
        c1.Transform.Position = Vector2.zero;
        c2 = new Circle(0.5f);
        c2.Transform.Position = Vector2.one;
        Assert.IsFalse(PhysicRaycast.CircleRaycast(c1, c2));

        AABB a = new AABB(Vector2.one * 0.5f);
        AABB b = new AABB(Vector2.one * 0.25f);
        b.Transform.Position = Vector2.one * 0.75f;
        Assert.IsTrue(PhysicRaycast.AABBRaycast(a,b));

        a = new AABB(Vector2.one * 0.5f);
        b = new AABB(Vector2.one * 0.45f);
        b.Transform.Position = Vector2.one * 1.5f;
        Assert.IsFalse(PhysicRaycast.AABBRaycast(a,b));
    }
}
