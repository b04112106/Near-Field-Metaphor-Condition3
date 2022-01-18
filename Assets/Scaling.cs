using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Scaling : MonoBehaviour
{
    public GameObject source;
    public GameObject next;
    public GameObject transwall;
    public GameObject Cube_trigger;
    private Transform target;
    private Vector3 scl;
    public uint count = 0;


    public GameObject ManipulatedCube;

    public GameObject rightHand;
    // Start is called before the first frame update
    void Start()
    {
        transwall.SetActive(false);
        ManipulatedCube.SetActive(false);
        scl = source.transform.localScale;
        this.transform.Find("Target" + count.ToString()).gameObject.SetActive(true);
        target = this.transform.Find("Target" + count.ToString()).Find("default");
        count++;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(count);
        
        if (target.localScale.x / source.transform.localScale.x < 1.1f && target.localScale.x / source.transform.localScale.x > 0.9f &&
            target.localScale.y / source.transform.localScale.y < 1.1f && target.localScale.y / source.transform.localScale.y > 0.9f &&
            target.localScale.z / source.transform.localScale.z < 1.1f && target.localScale.z / source.transform.localScale.z > 0.9f)
        {
            Debug.Log("Meet the scaling of Target " + (count - 1).ToString());
            source.transform.localScale = scl;
            Destroy(this.transform.Find("Target" + (count - 1).ToString()).gameObject);
            if (this.transform.Find("Target" + count.ToString()) != null)
            {
                this.transform.Find("Target" + count.ToString()).gameObject.SetActive(true);
                target = this.transform.Find("Target" + count.ToString()).Find("default");
                count++;
                source.GetComponent<PersonalSpace>().clean();
                clean();
            }
            else
            {
                count++;
                ManipulatedCube.SetActive(true);
                next.SetActive(true);
                Cube_trigger.SetActive(true);
                source.GetComponent<PersonalSpace>().clean();
                clean();
                source.SetActive(false);
                this.gameObject.SetActive(false);
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
            if (GameObject.Find("Face "+i.ToString()))
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
