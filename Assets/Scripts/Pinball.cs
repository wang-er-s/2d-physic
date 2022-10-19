using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinball : MonoBehaviour
{
    public float speed = 5;
    private int mask ;
    private int updateFrame;
    private static int TotalBatch = 6;
    void Start()
    {
        mask = LayerMask.GetMask("Wall");
        updateFrame = Random.Range(0, TotalBatch);
    }

    // Update is called once per frame
    public void OnUpdate()
    {
        var pos = transform.position;
        if (Time.frameCount % TotalBatch == updateFrame)
        {
            if (Physics.Linecast(pos, pos + transform.forward * 1.5f, out var hitInfo, mask))
            {
                transform.forward = Vector3.Reflect(transform.forward, hitInfo.normal);
            }
        }

        transform.position = pos + transform.forward * (speed * Time.deltaTime);
    }
}
