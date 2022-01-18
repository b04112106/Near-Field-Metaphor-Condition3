using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision_color_sound : MonoBehaviour
{
    public Material collide_material;
    public AudioSource collide_sound;

    private Material mr;
    // Start is called before the first frame update
    void Start()
    {
        mr = this.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        //this.GetComponent<MeshRenderer>().material = mr;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("I'm " + this.name + ". I've been collided.");
        collide_sound.Play(0);
        this.GetComponent<MeshRenderer>().material = collide_material;
    }
    private void OnCollisionExit(Collision collision)
    {
        this.GetComponent<MeshRenderer>().material = mr;
    }
}
