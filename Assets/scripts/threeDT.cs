using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using IndiePixel.VR;
using HighlightPlus;
using UnityEngine.SceneManagement;

public class threeDT : MonoBehaviour
{
    private GameObject [] Edge, SolidEdge;
    private HighlightProfile copyGrab, copyTouch;
    private HighlightEffect effect;
    private GameObject BoundingBox;
    private AudioSource audioSource;
    private Color original;
    private Color objectOriginal;
    private bool press = false;
    private GameObject SelectedObject;
    private float ScaleCoefficient = 0.0f;
    private Vector3 originalPosition;
    private Vector3 originalLocalPosition;
    private Quaternion originalRotation;

    #region condition 2
    private GameObject menu;
    private Vector3 originalScale;
    private GameObject rightHand;
    private GameObject GM;
    int flag = 1;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        SelectedObject = GameObject.FindWithTag("SelectedObject");
        if(SelectedObject)
            ScaleCoefficient = SelectedObject.GetComponent<PersonalSpace>().ScaleCoefficient;
        originalPosition = transform.position;
        originalLocalPosition = transform.localPosition;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        Edge = new GameObject [12];
        SolidEdge = new GameObject [12];
        copyGrab = Resources.Load<HighlightProfile>("CopyGrab");
        copyTouch = Resources.Load<HighlightProfile>("CopyTouch");
        if(tag == "CopyOfObject")
        {
            effect = GetComponent<HighlightEffect>();
            effect.ProfileLoad(copyTouch);
        }
        rightHand = GameObject.Find("RightControllerScriptAlias");
        #region condition 2
        menu = GameObject.Find("RadialMenu_Canvas");
        #endregion
        if(GameObject.Find("audioSource"))
            audioSource = GameObject.Find("audioSource").GetComponent<AudioSource>();
        else
        {
            new GameObject("audioSource").AddComponent<AudioSource>();
            audioSource = GameObject.Find("audioSource").GetComponent<AudioSource>();
        }
        if(SceneManager.GetActiveScene().name == "Testing")
            GM = GameObject.Find("GameManager");
    }
    public void Touch()
    {
        if(tag == "CopyOfObject")
        {
            effect.ProfileLoad(copyTouch);
            effect.highlighted = true;
        }
    }
    public void UnTouch()
    {
        if(tag == "CopyOfObject")
            effect.highlighted = false;
    }
    public void pressGrip()
    {
        originalPosition = this.gameObject.transform.position;
        originalRotation = this.gameObject.transform.rotation;
        press = true;
        BoundingBox = this.gameObject.transform.parent.gameObject;
        original = BoundingBox.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color;
        for(int i=0; i<12; i++){
            Edge[i].SetActive(true);
            Edge[i].transform.localScale = SolidEdge[i].transform.localScale;
            SolidEdge[i].SetActive(false);
        }
        // change highlight profile to copyGrab
        effect.ProfileLoad(copyGrab);
        effect.highlighted = true;
        // rightHand.GetComponent<VRTK_Pointer>().enabled = false;
        audioSource.PlayOneShot(Resources.Load<AudioClip>("Grab"));
    }
    public void ReleaseGrip()
    {
        press = false;
        // unhighlight all edges and corners
        for(int i=0; i<12; i++){
            Edge[i].GetComponent<MeshRenderer>().material.color = original;
            Edge[i].SetActive(false);
            SolidEdge[i].SetActive(true);
        }
        transform.localPosition = originalLocalPosition;
        transform.rotation = originalRotation;
        // change highlight profile to copyTouch
        effect.ProfileLoad(copyTouch);
        // rightHand.GetComponent<VRTK_Pointer>().enabled = true;
        if(SceneManager.GetActiveScene().name == "Testing")
            GM.GetComponent<GameManager>().release(1f);
    }
    private float setValue(char ch)
    {
        float max = 0.0f;
        //maxX = 0.0f; maxY = 0.0f; maxZ = 0.0f;
        int dir = maxDir(ch);
        if(dir == 1)
            max = this.gameObject.transform.position.x - originalPosition.x;
        else if(dir == 2)
            max = this.gameObject.transform.position.y - originalPosition.y;
        else if(dir == 3)
            max = this.gameObject.transform.position.z - originalPosition.z;
        max *= flag;
        return max;
    }
    private int maxDir(char ch)
    {
        float max = -Mathf.Infinity;
        int maxID = 0;
        flag = 1;
        for(int i=0; i<3; i++)
        {
            if(ch == 'X')
            {
                if( Mathf.Abs(this.gameObject.transform.right[i]) > max)
                {
                    max = Mathf.Abs(this.gameObject.transform.right[i]);
                    maxID = i + 1;
                    if(this.gameObject.transform.right[i] < 0)
                        flag = -1;
                    else 
                        flag = 1;
                }
            }
            else if(ch == 'Y')
            {
                if( Mathf.Abs(this.gameObject.transform.up[i]) > max)
                {
                    max = Mathf.Abs(this.gameObject.transform.up[i]);
                    maxID = i + 1;
                    if(this.gameObject.transform.up[i] < 0)
                        flag = -1;
                    else 
                        flag = 1;
                }
            }
            else if(ch == 'Z')
            {
                if( Mathf.Abs(this.gameObject.transform.forward[i]) > max)
                {
                    max = Mathf.Abs(this.gameObject.transform.forward[i]);
                    maxID = i + 1;
                    if(this.gameObject.transform.forward[i] < 0)
                        flag = -1;
                    else 
                        flag = 1;
                }
            }
        }
        return maxID;
    }
    // Update is called once per frame
    void Update()
    {
        // move object
        if(press){
            this.gameObject.transform.rotation = originalRotation;
            var tmpPos = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z);
            this.gameObject.transform.position = tmpPos;
            var diff = new Vector3(this.gameObject.transform.position.x - originalPosition.x, this.gameObject.transform.position.y - originalPosition.y, this.gameObject.transform.position.z - originalPosition.z);
            BoundingBox.transform.Translate(diff, Space.World);
            SelectedObject.transform.Translate(diff / ScaleCoefficient, Space.World);
            originalPosition = tmpPos;
        }
        // after bounding box appear, assign edge and solid edge
        if(GameObject.Find("SolidEdge 0") && tag == "CopyOfObject" && SolidEdge[0] == null)
        {
            BoundingBox = transform.parent.gameObject;
            for(int i=0; i<12; i++)
            {
                Edge[i] = BoundingBox.transform.GetChild(1).GetChild(i).gameObject;
                SolidEdge[i] = GameObject.Find("SolidEdge " + i.ToString());
            }  
        }
    }
}
