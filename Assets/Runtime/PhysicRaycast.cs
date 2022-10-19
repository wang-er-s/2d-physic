using UnityEngine;

public static class PhysicRaycast
{
    public static bool AABBRaycast(AABB a, AABB b)
    {
        if (a.Max.x < b.Min.x || a.Min.x > b.Max.x) return false;
        if (a.Max.y < b.Min.y || a.Min.y > b.Max.y) return false;
        return true;
    }

    public static bool CircleRaycast(Circle a, Circle b)
    {
        float sqrDis = a.Transform.Position.SqrDistance(b.Transform.Position);
        if (sqrDis < (a.Radius + b.Radius) * (a.Radius + b.Radius))
        {
            return true;
        }

        return false;
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
        r1.Position -= r1.InverseMass * correction;
        r2.Position += r2.InverseMass * correction;
    }

    public static Manifold CircleVsCircle(Circle c1, Circle c2)
    {
        Vector2 normal = c2.Transform.Position - c1.Transform.Position;
        float r = c1.Radius + c2.Radius;
        r *= r;
        if (normal.sqrMagnitude > r) return Manifold.Null;
        Manifold result = new Manifold();
        float dis = normal.magnitude;
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

    public static Manifold AABBvsAABB(AABB aBox, AABB bBox)
    {
        var normal = bBox.Transform.Position - aBox.Transform.Position;
        float aXExtent = (aBox.Max.x - aBox.Min.x) / 2;
        float bXExtent = (bBox.Max.x - bBox.Min.x) / 2;
        float xOverlap = aXExtent + bXExtent - Mathf.Abs(normal.x);
        if (!(xOverlap > 0)) return Manifold.Null;
        float aYExtent = (aBox.Max.y - aBox.Min.y) / 2;
        float bYExtent = (bBox.Max.y - bBox.Min.y) / 2;
        float yOverlap = aYExtent + bYExtent - Mathf.Abs(normal.y);
        if (!(yOverlap > 0)) return Manifold.Null;
        Manifold result = new Manifold();
        // 取短的一方当作深度
        if (yOverlap < xOverlap)
        {
            if(normal.x < 0)
                result.Normal = Vector2.left;
            else
                result.Normal = Vector2.right;
            result.Penetration = xOverlap;
            return result;
        }
        else
        {
            if(normal.y < 0)
                result.Normal = Vector2.down;
            else
                result.Normal = Vector2.up;
            result.Penetration = yOverlap;
            return result;
        }
    }

    public static Manifold AABBvsCircle(AABB ab, Circle circle)
    {
        Vector2 normal = circle.Transform.Position - ab.Transform.Position;
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
        float radius = circle.Radius;
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