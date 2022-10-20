using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Test : UnityEngine.MonoBehaviour
{
    public int count;
    public Vector2 Min;
    public Vector2 Max;
    public SphereCollider Self;
    public SphereCollider SphereCollider;

    private PhysicsWorld world;

    private List<ValueTuple<MRigidbody, Transform>> rig2Trans = new();

    private MRigidbody selfRigidbody;

    private void Awake()
    {
        world = new PhysicsWorld();
        Gen();
    }

    private void Start()
    {
    }

    private void Update()
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        selfRigidbody.Position += new Vector2(h, v) * (Time.deltaTime * 3);

        foreach (var valueTuple in rig2Trans)
        {
            valueTuple.Item2.position = new Vector3(valueTuple.Item1.Position.x, 0, valueTuple.Item1.Position.y);
        }
        
        world.Update();
    }

    private void Gen()
    {
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(SphereCollider);
            go.transform.position = new Vector3(Random.Range(Min.x, Max.x), 0, Random.Range(Min.y, Max.y));
            var render = go.GetComponent<Renderer>();
            render.sharedMaterial = new Material(render.sharedMaterial);
            render.sharedMaterial.color = Random.ColorHSV();
            var rig = world.AddRigidbody(go);
            rig2Trans.Add((rig, go.transform));
        }
        var rig2 = world.AddRigidbody(Self);
        selfRigidbody = rig2;
        rig2Trans.Add((rig2, transform));
    }
}