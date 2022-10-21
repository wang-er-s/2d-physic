using UnityEngine;

public struct MTransform
{
    public readonly float PosX;
    public readonly float PosY;
    public readonly float Sin;
    public readonly float Cos;

    public static readonly MTransform Zero = new MTransform(0, 0, 0);

    public MTransform(float posX, float posY, float angle)
    {
        PosX = posX;
        PosY = posY;
        Sin = Mathf.Sin(angle * Mathf.Deg2Rad);
        Cos = Mathf.Cos(angle * Mathf.Deg2Rad);
    }

    public MTransform(Vector2 pos, float angle) : this(pos.x, pos.y, angle)
    {
    }


}
