using UnityEngine;

public static class PhysicsRaycast
{
    public static Manifold PolygonsIntersect(MPolygonCollider polygon1, MPolygonCollider polygon2)
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
            axis.Normalize();
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
            axis.Normalize();
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
        
        // 矫正一下法线的方向， 需要跟两个物体的移动方向相符  
        // result.R1 R2顺序，R1 R2根据法线移动的方向 normal的方向需要相互配合来实现正确的效果
        Vector2 twoPolygonDir = polygon1.Position - polygon2.Position;
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
            axis.Normalize();
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
            axis.Normalize();
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
        
        // 矫正一下法线的方向， 需要跟两个物体的移动方向相符  
        // result.R1 R2顺序，R1 R2根据法线移动的方向 normal的方向需要相互配合来实现正确的效果
        Vector2 twoPolygonDir = polygonCollider.Position - circleCollider.Position;
        if (Vector2.Dot(twoPolygonDir, normal) < 0)
            normal = -normal;
        Manifold result = new Manifold();
        result.Normal = normal;
        result.Penetration = depth;
        result.R1 = circleCollider;
        result.R2 = polygonCollider;
        return result;
    }
    
    public static Manifold CircleIntersect(MCircleCollider c1, MCircleCollider c2)
    {
        Vector2 normal = c2.Position - c1.Position;
        float r = c1.Radius + c2.Radius;
        var sqrMag = normal.sqrMagnitude;
        if (sqrMag > r * r) return Manifold.Null;
        Manifold result = new Manifold();
        result.R1 = c1;
        result.R2 = c2;
        result.Contact1 = CalcCircleContactPoint(c1, c2);
        result.ContactCount = 1;
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

    public static Vector2 CalcCircleContactPoint(MCircleCollider c1, MCircleCollider c2)
    {
        var normal = c1.Position - c2.Position;
        normal.Normalize();
        return c2.Position + normal * c2.Radius;
    }

    private static float SqrDistance(this Vector2 selfPos, Vector2 otherPos)
    {
        float num1 = selfPos.x - otherPos.x;
        float num2 = selfPos.y - otherPos.y;
        return (float)(num1 * (double)num1 + num2 * (double)num2);
    }
}