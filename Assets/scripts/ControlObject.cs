using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }
    private void CreateSphere(Vector3 pos, Color color, float duration = 0.02f)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        sphere.transform.position = pos;
        sphere.GetComponent<MeshRenderer>().material.color = color;
        GameObject.Destroy(sphere, duration);
    }
    private void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.02f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(myLine, duration);
    }
    // Update is called once per frame
    void Update()
    {
        for(int i=0; i<8; i++)
        {
            if(gameObject.layer == 8) // small
                transform.GetChild(i).transform.localScale = new Vector3(0.04f/transform.lossyScale.x, 0.04f/transform.lossyScale.y, 0.04f/transform.lossyScale.z);
            else if(gameObject.layer == 9) // big
                transform.GetChild(i).transform.localScale = new Vector3(0.08f/transform.lossyScale.x, 0.08f/transform.lossyScale.y, 0.08f/transform.lossyScale.z);
        }
        if(tag == "wireFrame")
        {
            for(int i=0; i<4; i++)
            {
                DrawLine(transform.GetChild(i).position, transform.GetChild((i+1)%4).position, Color.white);
                DrawLine(transform.GetChild(i+4).position, transform.GetChild((i+1)%4 + 4).position, Color.white);
                DrawLine(transform.GetChild(i).position, transform.GetChild((i)%4 + 4).position, Color.white);
            }
        }
    }
}
