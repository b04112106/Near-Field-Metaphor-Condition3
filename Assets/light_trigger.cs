using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class light_trigger : MonoBehaviour
{
    public GameObject father;

    public GameObject next;
    public GameObject next_trigger;

    public GameObject rightHand;
    // Start is called before the first frame update
    private void Start()
    {
        next.SetActive(false);
        next_trigger.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "ManipulatedCube0")
        {
            if (this.gameObject.name == "l1_trigger")
            {
                next.SetActive(true);
                next_trigger.SetActive(true);
                father.SetActive(false);
            }
            else
            {
                other.gameObject.GetComponent<PersonalSpace>().clean();
                clean();
                Destroy(other.gameObject);
                next.SetActive(true);
                next_trigger.SetActive(true);
                father.SetActive(false);
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
