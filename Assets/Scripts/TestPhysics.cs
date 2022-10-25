using System;
using UnityEngine;

public class TestPhysics : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDrawGizmos()
    {
        Vector2 start = new Vector2(0, 0);
        Vector2 end = new Vector2(1, 1);

        Vector2 point = new Vector2(3f, 0.8f);

        Vector2 ab = end - start;
        Vector2 ap = point - start;

        Vector2 closestPoint = Vector2.zero;

        float dot = Vector2.Dot(ab, ap);
        float dDivAb = dot / ab.sqrMagnitude;
        if (dDivAb <= 0)
        {
            closestPoint = start;
        }
        else if (dDivAb >= 1)
        {
            closestPoint = end;
        }
        else
        {
            closestPoint = start + dDivAb * ab;
        }
        // dot = |ab| |ap| cos

        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(point, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(closestPoint, 0.1f);
    }
}
