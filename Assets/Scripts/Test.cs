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
        GenBox();
    }

    private void Start()
    {
    }

    private List<MBoxCollider> boxColliders = new();

    private void Update()
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
       // selfRigidbody.Move(new Vector2(h, v) * (Time.deltaTime * 3));

        foreach (var valueTuple in rig2Trans)
        {
            valueTuple.Item2.position = new Vector3(valueTuple.Item1.Position.x, 0, valueTuple.Item1.Position.y);
        }
        
        world.Update();
        foreach (var boxCollider in boxColliders)
        {
            boxCollider.Rotate(Time.deltaTime * 10);
            var vertices = boxCollider.GetVertices();
            for (var i = 0; i < boxCollider.trangles.Length; i += 3)
            {
                Vector3 p1 = new Vector3(vertices[boxCollider.trangles[i]].x, 0, vertices[boxCollider.trangles[i]].y);
                Vector3 p2 = new Vector3(vertices[boxCollider.trangles[i+1]].x, 0, vertices[boxCollider.trangles[i+1]].y);
                Vector3 p3 = new Vector3(vertices[boxCollider.trangles[i+2]].x, 0, vertices[boxCollider.trangles[i+2]].y);
                Debug.DrawLine(p1, p2);
                Debug.DrawLine(p3, p2);
                Debug.DrawLine(p3, p1);
            }
        }
    }

    void DrawBox(Vector2 pos, Vector2 scale)
    {
        
    }

    private void GenBox()
    {
        for (int i = 0; i < count; i++)
        {
            MBoxCollider boxCollider = new MBoxCollider(Vector2.one * 2, 2, 1, false);
            boxCollider.MoveTo(new Vector2(Random.Range(Min.x, Max.x), Random.Range(Min.y, Max.y)));
            boxColliders.Add(boxCollider);
        }
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