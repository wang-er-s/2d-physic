using UnityEngine;

public static class MMath
{
    public static Vector2 Transform(this MTransform self, Vector2 pos)
    {
        float x = self.Cos * pos.x - self.Sin * pos.y;
        float y = self.Sin * pos.x + self.Cos * pos.y;

        return new Vector2(x + self.PosX, y + self.PosY);
    }
}