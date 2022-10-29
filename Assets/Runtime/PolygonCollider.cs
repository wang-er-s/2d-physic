using UnityEngine;

public class MPolygonCollider : MRigidbody
{
     protected Vector2[] BaseVertexes;
     protected Vector2[] vertexes;

     public MPolygonCollider(float mass, float restitution, float friction, bool isStatic) : base(mass, restitution,
          friction, isStatic)
     {
     }

     public void SetVertexAndTriangles(Vector2[] vertexes)
     {
          this.BaseVertexes = vertexes;
          this.vertexes = new Vector2[BaseVertexes.Length];
     }
     
     public Vector2[] GetVertices()
     {
          if (TransformDirty)
          {
               MTransform transform = new MTransform(Position, Angle);
               for (int i = 0; i < BaseVertexes.Length; i++)
               {
                    vertexes[i] = transform.Transform(BaseVertexes[i]);
               }
               TransformDirty = false;
          }

          return vertexes;
     }

     public override void MoveTo(Vector2 pos)
     {
          base.MoveTo(pos);
          AABBDirty = true;
     }

     public override void RotateTo(float angle)
     {
          base.RotateTo(angle);
          AABBDirty = true;
     }

     public override AABB GetAABB()
     {
          if (AABBDirty)
          {
               float minX = float.MaxValue;
               float maxX = float.MinValue;
               float minY = float.MaxValue;
               float maxY = float.MinValue;

               var vertices = GetVertices();
               for (int i = 0; i < vertices.Length; i++)
               {
                    var vert = vertices[i];
                    if (vert.x < minX)
                         minX = vert.x;
                    if (vert.x > maxX)
                         maxX = vert.x;
                    if (vert.y < minY)
                         minY = vert.y;
                    if (vert.y > maxY)
                         maxY = vert.y;
               }
               AABBCache = new AABB(minX, maxX, minY, maxY);
               AABBDirty = false;
          }

          return AABBCache;
     }

     public override void ForceRefreshTransform()
     {
          GetVertices();
     }
}
