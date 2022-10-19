using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Test : UnityEngine.MonoBehaviour
{
    public int count;
    public Vector2 Min;
    public Vector2 Max;
    public Pinball Go;

    private List<Pinball> gos = new List<Pinball>();

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        Gen();
    }

    private void Update()
    {
        for (int i = 0; i < gos.Count; i++)
        {
            gos[i].OnUpdate();
        } 
    }

    private void OnGUI()
    {
        var t = GUI.TextField(new Rect(100, 100, 100, 50), count.ToString());
        try
        {
            count = Int32.Parse(t);
        }
        catch (Exception e)
        {
        }

        if (GUI.Button(new Rect(100, 200, 100, 50), "生成"))
        {
            Gen();
        }
    }

    private void Gen()
    {
        foreach (var go in gos)
        {
            Destroy(go.gameObject);
        }
        gos.Clear();
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(Random.Range(Min.x, Max.x), 0, Random.Range(Min.y, Max.y));
            Quaternion rota = Quaternion.Euler(new Vector3(0, Random.Range(0, 360f), 0));
            gos.Add(Instantiate(Go, pos, rota, null));
        }
    }
}