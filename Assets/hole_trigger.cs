using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using VRTK;

public class hole_trigger : MonoBehaviour
{
    public Transform points;
    public Transform cube;
    private Vector3 ori;
    public uint count = 1;

    public GameObject next;

    public GameObject rightHand;

    public GameObject translationWall;

    // Start is called before the first frame update
    void Start()
    {
        this.transform.position = points.Find("point0").position;
    }
    private void Update()
    {
        //Debug.Log(count);
    }
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "ManipulatedCube0")
        {
            if (points.Find("point" + count.ToString()) != null)
            {
                if (count == 6)
                {
                    translationWall.SetActive(true);
                    this.transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
                }
                else if(count == 7)
                {
                    this.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                }
                this.transform.position = points.Find("point" + count.ToString()).position;
                count++;
            }
            else
            {
                count++;
                this.gameObject.SetActive(false);
                next.SetActive(true);
            }
        }
        if(this.gameObject.name == "Cube_trigger1")
        {
            if(collider.gameObject.name == "ManipulatedCube")
            {
                next.SetActive(true);
                cube.gameObject.SetActive(false);
                collider.GetComponent<PersonalSpace>().clean();
                clean();
                collider.transform.localScale = 2 * collider.transform.localScale;
                collider.transform.position += new Vector3(-1f,0f,-1f);
                collider.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                collider.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                points.gameObject.SetActive(false);
            }
        }
        if (this.gameObject.name == "Cube_trigger2")
        {
            if (collider.gameObject.name == "ManipulatedCube")
            {
                collider.GetComponent<PersonalSpace>().clean();
                clean();
                points.gameObject.SetActive(false);
                next.SetActive(false);
                cube.gameObject.SetActive(false);
            }
        }
    }
    private void clean()
    {
        rightHand.GetComponent<VRTK_Pointer>().enabled = false;
        rightHand.GetComponent<VRTK_Pointer>().activateOnEnable = true;
        rightHand.GetComponent<VRTK_Pointer>().enabled = true;
        if (GameObject.Find("ManipulatedCube(Clone)"))
            Destroy(GameObject.Find("ManipulatedCube(Clone)"));
        if (GameObject.Find("ManipulatedCube0(Clone)"))
            Destroy(GameObject.Find("ManipulatedCube0(Clone)"));
        if (GameObject.Find("ManipulatedRectangle(Clone)"))
            Destroy(GameObject.Find("ManipulatedRectangle(Clone)"));
        if (GameObject.Find("bounding box 1(Clone)"))
            Destroy(GameObject.Find("bounding box 1(Clone)"));
        for (int i = 0; i < 6; i++)
        {
            if (GameObject.Find("Face " + i.ToString()))
                Destroy(GameObject.Find("Face " + i.ToString()));
        }
        for (int i = 0; i < 8; i++)
        {
            if (GameObject.Find("Corner " + i.ToString()))
                Destroy(GameObject.Find("Corner " + i.ToString()));
        }
        for (int i = 0; i < 12; i++)
        {
            if (GameObject.Find("SolidEdge " + i.ToString()))
                Destroy(GameObject.Find("SolidEdge " + i.ToString()));
        }
        if (GameObject.Find("HandleY"))
            Destroy(GameObject.Find("HandleY"));
        if (GameObject.Find("HandleY"))
            Destroy(GameObject.Find("HandleY"));
        if (GameObject.Find("HandleX"))
            Destroy(GameObject.Find("HandleX"));
        if (GameObject.Find("HandleX"))
            Destroy(GameObject.Find("HandleX"));
        if (GameObject.Find("HandleZ"))
            Destroy(GameObject.Find("HandleZ"));
        if (GameObject.Find("HandleZ"))
            Destroy(GameObject.Find("HandleZ"));
        if (GameObject.Find("empty"))
            Destroy(GameObject.Find("empty"));
        if (GameObject.Find("empty1"))
            Destroy(GameObject.Find("empty1"));
        if (GameObject.Find("coolObject"))
            Destroy(GameObject.Find("coolObject"));
        if (GameObject.Find("coolObject1"))
            Destroy(GameObject.Find("coolObject1"));
    }
}
