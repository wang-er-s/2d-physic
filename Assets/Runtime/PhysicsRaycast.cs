using UnityEngine;

public static class PhysicsRaycast
{
    public static Manifold PolygonsRaycast(MPolygonCollider polygon1, MPolygonCollider polygon2)
    {
        // https://blog.csdn.net/yorhomwang/article/details/54869018 分离轴判断多边形和圆形碰撞
        Vector2[] v1 = polygon1.GetVertices();
        Vector2[] v2 = polygon2.GetVertices();
        var normal = Vector2.zero;
        var depth = float.MaxValue;
        // 分离轴判断是否相交
        for (int i = 0; i < v1.Length; i++)
        {
            Vector2 va = v1[i];
            Vector2 vb = v1[(i + 1) % v1.Length];
            Vector2 edge = vb - va;
            Vector2 axis = new Vector2(-edge.y, edge.x);
            ProjectVertices(v1, axis, out var min1, out var max1);
            ProjectVertices(v2, axis, out var min2, out var max2);
            if (min1 > max2 || min2 > max1)
                return Manifold.Null;

            var tmpDepth = Mathf.Min(max1 - min2, max2 - min1);
            if (tmpDepth < depth)
            {
                depth = tmpDepth;
                normal = axis;
            }
        }
        
        for (int i = 0; i < v2.Length; i++)
        {
            Vector2 va = v2[i];
            Vector2 vb = v2[(i + 1) % v2.Length];
            Vector2 edge = vb - va;
            Vector2 axis = new Vector2(-edge.y, edge.x);
            ProjectVertices(v1, axis, out var min1, out var max1);
            ProjectVertices(v2, axis, out var min2, out var max2);
            if (min1 > max2 || min2 > max1)
                return Manifold.Null;
            var tmpDepth = Mathf.Min(max1 - min2, max2 - min1);
            if (tmpDepth < depth)
            {
                depth = tmpDepth;
                normal = axis;
            }
        }
        
        depth /= normal.magnitude;
        normal.Normalize();
        // 矫正一下法线的方向， 需要跟两个物体的移动方向相符  
        // result.R1 R2顺序，R1 R2根据法线移动的方向 normal的方向需要相互配合来实现正确的效果
        Vector2 twoPolygonDir = FindPolygonCenter(v1) - FindPolygonCenter(v2);
        if (Vector2.Dot(twoPolygonDir, normal) < 0)
            normal = -normal;
        Manifold result = new Manifold();
        result.Normal = normal;
        result.Penetration = depth;
        result.R1 = polygon2;
        result.R2 = polygon1;
        return result;
    }

    private static void ProjectVertices(Vector2[] vert, Vector2 axis, out float min, out float max)
    {
        min = float.MaxValue;
        max = float.MinValue;
        for (int i = 0; i < vert.Length; i++)
        {
            var v = vert[i];
            float proj = Vector2.Dot(v, axis);
            if (proj < min)
                min = proj;
            if (proj > max)
                max = proj;
        }
    }

    private static void ProjectCircle(Vector2 axis, Vector2 center, float radius, out float min, out float max)
    {
        Vector2 dir = axis.normalized;
        Vector2 radiusDir = dir * radius;
        Vector2 p1 = center - radiusDir;
        Vector2 p2 = center + radiusDir;

        min = Vector2.Dot(p1, axis);
        max = Vector2.Dot(p2, axis);

        // 不确定axis的方向，有可能是反的
        if (min > max)
        {
            (min, max) = (max, min);
        }
    }

    private static Vector2 FindPolygonCenter(Vector2[] vector2s)
    {
        float totalX = 0;
        float totalY = 0;
        for (int i = 0; i < vector2s.Length; i++)
        {
            totalX += vector2s[i].x;
            totalY += vector2s[i].y;
        }

        return new Vector2(totalX / vector2s.Length, totalY / vector2s.Length);
    }

    public static Manifold PolygonCircleIntersect(MPolygonCollider polygonCollider, MCircleCollider circleCollider)
    {
        Vector2[] v1 = polygonCollider.GetVertices();
        Vector2 minDisCirVert = v1[0];
        float minDisCir = float.MaxValue;
        var normal = Vector2.zero;
        var depth = float.MaxValue;
        // 分离轴判断是否相交
        for (int i = 0; i < v1.Length; i++)
        {
            Vector2 va = v1[i];
            Vector2 vb = v1[(i + 1) % v1.Length];
            Vector2 edge = vb - va;
            Vector2 axis = new Vector2(-edge.y, edge.x);
            ProjectVertices(v1, axis, out var min1, out var max1);
            ProjectCircle(axis, circleCollider.Position, circleCollider.Radius, out var min2, out var max2);
            if (min1 > max2 || min2 > max1)
                return Manifold.Null;

            var tmpDepth = Mathf.Min(max1 - min2, max2 - min1);
            if (tmpDepth < depth)
            {
                depth = tmpDepth;
                normal = axis;
            }

            var sqrDis = va.SqrDistance(circleCollider.Position);
            if (sqrDis < minDisCir)
            {
                minDisCirVert = va;
            }
        }

        {
            Vector2 axis = minDisCirVert - circleCollider.Position;
            ProjectVertices(v1, axis, out var min1, out var max1);
            ProjectCircle(axis, circleCollider.Position, circleCollider.Radius, out var min2, out var max2);
            if (min1 > max2 || min2 > max1)
                return Manifold.Null;

            var tmpDepth = Mathf.Min(max1 - min2, max2 - min1);
            if (tmpDepth < depth)
            {
                depth = tmpDepth;
                normal = axis;
            }
        }
        
        depth /= normal.magnitude;
        normal.Normalize();
        // 矫正一下法线的方向， 需要跟两个物体的移动方向相符  
        // result.R1 R2顺序，R1 R2根据法线移动的方向 normal的方向需要相互配合来实现正确的效果
        Vector2 twoPolygonDir = FindPolygonCenter(v1) - circleCollider.Position;
        if (Vector2.Dot(twoPolygonDir, normal) < 0)
            normal = -normal;
        Manifold result = new Manifold();
        result.Normal = normal;
        result.Penetration = depth;
        result.R1 = circleCollider;
        result.R2 = polygonCollider;
        return result;
    }
    
    
    public static void ResolveCollision(MRigidbody r1, MRigidbody r2)
    {
        Vector2 normal = Normal(r1, r2);
        Vector2 rV = r2.Velocity - r1.Velocity;
        // 法线方向上的速度
        float velAlongNormal = Vector2.Dot(rV, normal);
        if (velAlongNormal > 0) return;
        float restitution = Mathf.Min(r1.Restitution, r2.Restitution);
        // ??
        float j = -(1 + restitution) * velAlongNormal;
        Vector2 impulse = j * normal;

        // 质量越大 对速度的影响越小
        if (r1.InverseMass != 0)
            r1.Velocity -= r1.InverseMass / (r1.InverseMass + r2.InverseMass) * impulse;
        if (r2.InverseMass != 0)
            r2.Velocity += r2.InverseMass / (r1.InverseMass + r2.InverseMass) * impulse;
    }

    private const float CorrectPercent = 0.2f;

    private const float CorrectBuffer = 0.01f;

    /// <summary>
    /// 位置矫正，由于浮点精度问题，穿透的深度是大于脉冲的值的，所以我们需要脉冲之后矫正一下位置
    /// 一般是使用渗透的0.2左右来矫正
    /// </summary>
    /// <param name="r1"></param>
    /// <param name="r2"></param>
    /// <param name="penetrationDepth">渗透深度</param>
    private static void PositionCorrection(MRigidbody r1, MRigidbody r2, float penetrationDepth)
    {
        if(penetrationDepth <= CorrectBuffer) return;
        var normal = Normal(r1, r2);
        Vector2 correction = penetrationDepth / (r1.InverseMass + r2.InverseMass) * CorrectPercent * normal;
        r1.Move(-r1.InverseMass * correction);
        r2.Move(r2.InverseMass * correction);
    }

    public static Manifold CircleVsCircle(MCircleCollider c1, MCircleCollider c2)
    {
        Vector2 normal = c2.Position - c1.Position;
        float r = c1.Radius + c2.Radius;
        var sqrMag = normal.sqrMagnitude;
        if (sqrMag > r * r) return Manifold.Null;
        Manifold result = new Manifold();
        result.R1 = c1;
        result.R2 = c2;
        float dis = Mathf.Sqrt(sqrMag);
        // 如果两个圆之间的距离不为0
        if (dis != 0)
        {
            result.Penetration = r - dis;
            // normal / dis 是求normal.normalized
            result.Normal = normal / dis;
            return result;
        }
        else
        {
            // 位置相同 随机选，但是要保持一致
            result.Penetration = c1.Radius;
            result.Normal = Vector2.left;
            return result;
        }
    }

    public static Manifold AABBvsCircle(MBoxCollider ab, MCircleCollider mCircleCollider)
    {
        Vector2 normal = mCircleCollider.Position - ab.Position;
        // 矩形上距离圆形最近的点
        Vector2 closest = normal;

        float xExtent = (ab.Max.x - ab.Min.x) / 2;
        float yExtent = (ab.Max.y - ab.Min.y) / 2;

        closest.x = Mathf.Clamp(closest.x, -xExtent, xExtent);
        closest.y = Mathf.Clamp(closest.y, -yExtent, yExtent);

        bool inside = false;
        if (normal == closest)
        {
            inside = true;
            if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
            {
                if (closest.x > 0)
                    closest.x = xExtent;
                else
                    closest.x = -xExtent;
            }
            else
            {
                if (closest.y > 0)
                    closest.y = yExtent;
                else
                    closest.y = -yExtent;
            }
        }

        normal -= closest;
        float dis = normal.sqrMagnitude;
        float radius = mCircleCollider.Radius;
        if (dis > radius * radius && !inside) return Manifold.Null;

        Manifold result = new Manifold();
        dis = Mathf.Sqrt(dis);
        if (inside)
        {
            result.Normal = -normal;
            result.Penetration = radius - dis;
        }
        else
        {
            result.Normal = normal;
            result.Penetration = radius - dis;
        }

        return result;
    }
    
    private static Vector2 Normal(MRigidbody r1, MRigidbody r2)
    {
        return Vector2.one;
    }

    public static float SqrDistance(this Vector2 selfPos, Vector2 otherPos)
    {
        float num1 = selfPos.x - otherPos.x;
        float num2 = selfPos.y - otherPos.y;
        return (float)(num1 * (double)num1 + num2 * (double)num2);
    }
}