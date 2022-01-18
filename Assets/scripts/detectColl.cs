using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class detectColl : MonoBehaviour
{
    private GameObject GM;
    // Start is called before the first frame update
    void Start()
    {
        GM = GameObject.Find("GameManager");
    }

    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun
    /// touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionEnter(Collision other)
    {
        GM.GetComponent<GameManager>().addColl();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
