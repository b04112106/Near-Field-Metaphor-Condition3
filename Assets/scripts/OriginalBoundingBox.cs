using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
public class OriginalBoundingBox : MonoBehaviour
{
    // public GameObject Edge;
    private Vector3 originalLocalScale;
    private float origianlRadius;
    public bool doUnTouch = true;
    // Start is called before the first frame update
    void Start()
    {
        originalLocalScale = transform.localScale;
        origianlRadius = GetComponent<CapsuleCollider>().radius;
    }

    public void OnTouch()
    {
        // Debug.Log("Touch");
        originalLocalScale = transform.localScale;
        origianlRadius = GetComponent<CapsuleCollider>().radius;
        transform.localScale = new Vector3(originalLocalScale.x*2f, originalLocalScale.y, originalLocalScale.z*2f);
        GetComponent<DoManipulation>().objectToHL.transform.localScale = transform.localScale;
        GetComponent<CapsuleCollider>().radius = origianlRadius / 1.2f;
    }
    public void OnUnTouch()
    {
        
        // Debug.Log(transform.localScale.x);
        if(doUnTouch)
        {
            transform.localScale = originalLocalScale;
            GetComponent<DoManipulation>().objectToHL.transform.localScale = transform.localScale;
        }
        else
        {    
            doUnTouch = true;
        }
        GetComponent<CapsuleCollider>().radius = origianlRadius;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
