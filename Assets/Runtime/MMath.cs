using UnityEngine;

public static class MMath
{
    public static Vector2 Transform(this MTransform self, Vector2 pos)
    {
        float x = self.Cos * pos.x - self.Sin * pos.y;
        float y = self.Sin * pos.x + self.Cos * pos.y;

        return new Vector2(x + self.PosX, y + self.PosY);
    }
    
    public static float SqrDistance(this Vector2 selfPos, Vector2 otherPos)
    {
        float num1 = selfPos.x - otherPos.x;
        float num2 = selfPos.y - otherPos.y;
        return (float)(num1 * (double)num1 + num2 * (double)num2);
    }

    public static bool NearlyEqual(this Vector2 v1, Vector2 v2)
    {
        return v1.SqrDistance(v2) < 0.001f * 0.001f;
    }

    public static bool NearlyEqual(this float f1, float f2)
    {
        return Mathf.Abs(f1 - f2) < 0.001f;
    }

    public static float Cross(this Vector2 v1, Vector2 v2)
    {
        return v1.x * v2.y - v1.y * v2.x;
    }
}