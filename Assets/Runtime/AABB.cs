using UnityEngine;

public struct AABB
{
    public Vector2 Min;
    public Vector2 Max;
     
    public AABB(Vector2 min, Vector2 max)
    {
        Min = min;
        Max = max;
    }

    public AABB(float minx,float maxX, float minY, float maxY)
    {
        Min = new Vector2(minx, minY);
        Max = new Vector2(maxX, maxY);
    }
}