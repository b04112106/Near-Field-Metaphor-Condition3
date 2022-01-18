using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using HighlightPlus;
using UnityEngine.SceneManagement;

public class RotateHandle : MonoBehaviour
{
    public GameObject BoundingBox;
    public GameObject RotateContainer;
    public AudioSource audioSource;
    public GameObject [] Face;
    public GameObject [] Edge;
    public GameObject [] SolidEdge;
    private GameObject leftHand, rightHand;
    private bool grab = true;
    private bool isTouching;
    private bool isNearTouching;
    private bool isUsing;
    private GameObject Base;
    private Color baseOriginalColor, handleOriginalColor;
    private Vector3 [] edgeOriginalLocalPos;
    private GameObject coolObject;
    private GameObject rotateContainerChild;
    private bool flash = false, increase = true;
    private Color edgeHLColor, oriColor, handleHLColor;
    private float minFlashAlpha = 0.05f, maxFlashAlpha = 0.5f, touchAlpha = 0.6f, grabAlpha = 0.9f;
    private GameObject newParent, oriParent, selectedObject, GM;
    private Vector3 oriSize;
    private float oriBoundMinSize = 0.0f;
    private bool firstPlay = true;
    // Start is called before the first frame update
    void Start()
    {
        leftHand = GameObject.Find("LeftControllerScriptAlias");
        rightHand = GameObject.Find("RightControllerScriptAlias");
        GetComponent<VRTK_InteractableObject>().usingState = 0;
        GetComponent<MeshRenderer>().enabled = false;
        isTouching = false;
        isNearTouching = false;
        isUsing = false;
        Base = this.gameObject.transform.parent.GetChild(1).gameObject;
        Base.GetComponent<MeshRenderer>().enabled = false;
        baseOriginalColor = Base.GetComponent<MeshRenderer>().material.color;
        handleOriginalColor = GetComponent<MeshRenderer>().material.color;
        // initial edge local position
        edgeOriginalLocalPos = new Vector3 [4];
        for(int i=0; i<4; i++)
        {
            edgeOriginalLocalPos[i] = Edge[i].transform.localPosition;
        }
        // create empty object
        if(!GameObject.Find("coolObject"))
            coolObject = new GameObject("coolObject");
        else
            coolObject = GameObject.Find("coolObject");

        oriColor = new Color(0.9716981f, 0.7230207f, 0.3345942f, minFlashAlpha);
        edgeHLColor = oriColor;
        handleHLColor = handleOriginalColor;
        selectedObject = GameObject.FindWithTag("SelectedObject");
        if(selectedObject.transform.parent != null)
            oriParent = selectedObject.transform.parent.gameObject;
        else
            oriParent = null;
        if(GameObject.Find("newParent"))
            newParent = GameObject.Find("newParent");
        else
            newParent = new GameObject("newParent");
        if(name == "HandleY")
        {
            rotateContainerChild = GameObject.Find("[VRTK][AUTOGEN][RotateY][Controllable][ArtificialBased][RotatorContainer]");
        } 
        else if(name == "HandleX")
        {
            rotateContainerChild = GameObject.Find("[VRTK][AUTOGEN][RotateX][Controllable][ArtificialBased][RotatorContainer]");
        } 
        else if(name == "HandleZ")
        {
            rotateContainerChild = GameObject.Find("[VRTK][AUTOGEN][RotateZ][Controllable][ArtificialBased][RotatorContainer]");
        }
        rotateContainerChild.GetComponent<VRTK_InteractableObject>().grabOverrideButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
        oriBoundMinSize = Mathf.Min(BoundingBox.transform.localScale.x, BoundingBox.transform.localScale.y, BoundingBox.transform.localScale.z);
        oriSize = transform.parent.localScale;
        if(SceneManager.GetActiveScene().name == "Testing")
            GM = GameObject.Find("GameManager");
    }
    public void onTouch()
    {
        isTouching = true;
        if(!isUsing)
        {
            GetComponent<MeshRenderer>().material.color = new Color(handleOriginalColor.r, handleOriginalColor.g, handleOriginalColor.b, touchAlpha);
            Base.GetComponent<MeshRenderer>().material.color = new Color(baseOriginalColor.r, baseOriginalColor.g, baseOriginalColor.b, touchAlpha);
            GetComponent<MeshRenderer>().enabled = true;
            Base.GetComponent<MeshRenderer>().enabled = true;
            transform.parent.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("HandleTouch"));
            transform.parent.GetComponent<HighlightEffect>().outlineColor = GetComponent<MeshRenderer>().material.color;
            transform.parent.GetComponent<HighlightEffect>().outlineColor.a = 1;
            transform.parent.GetComponent<HighlightEffect>().highlighted = true;
        }
    }
    public void onUnTouch()
    {
        isTouching = false;
        if(!isUsing && (!SolidEdge[0].GetComponent<DoManipulation>().isPressing && !SolidEdge[1].GetComponent<DoManipulation>().isPressing && !SolidEdge[2].GetComponent<DoManipulation>().isPressing && !SolidEdge[3].GetComponent<DoManipulation>().isPressing))
        {    
            GetComponent<MeshRenderer>().material.color = handleOriginalColor;
            Base.GetComponent<MeshRenderer>().material.color = baseOriginalColor;
            GetComponent<MeshRenderer>().enabled = false;
            Base.GetComponent<MeshRenderer>().enabled = false;
            transform.parent.GetComponent<HighlightEffect>().highlighted = false;
        }
    }
    public void NearTouch()
    {
        isNearTouching = true;
    }
    public void NearUnTouch()
    {
        isNearTouching = false;
        if(!isUsing)
        {
            handleHLColor = handleOriginalColor;
            increase = true;
            GetComponent<MeshRenderer>().enabled = false;
            Base.GetComponent<MeshRenderer>().enabled = false;
            transform.parent.GetComponent<HighlightEffect>().highlighted = false;
        }
    }
    public void PressTrigger()
    {
        GetComponent<VRTK_InteractableObject>().usingState = 1;
        GetComponent<CapsuleCollider>().radius = 10f;
        GetComponent<MeshRenderer>().material.color = new Color(handleOriginalColor.r, handleOriginalColor.g, handleOriginalColor.b, grabAlpha);
        Base.GetComponent<MeshRenderer>().material.color = new Color(baseOriginalColor.r, baseOriginalColor.g, baseOriginalColor.b, grabAlpha);
        coolObject.transform.position = BoundingBox.transform.position;
        rotateContainerChild.transform.parent = coolObject.transform;
        rotateContainerChild.transform.rotation = BoundingBox.transform.rotation;
        // if one of solid edges is pressing by right controller, force release the target
        if(SolidEdge[0].GetComponent<DoManipulation>().isPressing || SolidEdge[1].GetComponent<DoManipulation>().isPressing || SolidEdge[2].GetComponent<DoManipulation>().isPressing || SolidEdge[3].GetComponent<DoManipulation>().isPressing)
        {
            rightHand.GetComponent<VRTK_InteractGrab>().ForceRelease();
            grab = false;
        }
        // disable solid edge, enable edge
        for(int i=0; i<4; i++)
        {
            SolidEdge[i].SetActive(false);
            Edge[i].SetActive(true);
            Edge[i].transform.position = SolidEdge[i].GetComponent<DoManipulation>().objectToHL.transform.position;
            Edge[i].transform.localScale = SolidEdge[i].transform.localScale;
            SolidEdge[i].GetComponent<DoManipulation>().objectToHL.SetActive(false);
            // load edge opposite profile
            Edge[i].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("EdgeOpposite"));
            Edge[i].GetComponent<HighlightEffect>().highlighted = true;
        }
        // disable face, edge, corner and copy of object
        for(int i=0; i<BoundingBox.transform.GetChild(0).childCount; i++) // face
        {
            BoundingBox.transform.GetChild(0).GetChild(i).GetComponent<BoxCollider>().enabled = false;
            BoundingBox.transform.GetChild(0).GetChild(i).GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = false;
        }
        for(int i=0; i<BoundingBox.transform.GetChild(2).childCount; i++) // corner
        {
            BoundingBox.transform.GetChild(2).GetChild(i).GetComponent<SphereCollider>().enabled = false;
            BoundingBox.transform.GetChild(2).GetChild(i).GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = false;
        }
        for(int i=0; i<BoundingBox.transform.GetChild(4).childCount; i++) // edge, edge still show the mesh renderer
        {
            BoundingBox.transform.GetChild(4).GetChild(i).GetComponent<CapsuleCollider>().enabled = false;
        }
        GameObject.FindGameObjectWithTag("CopyOfObject").GetComponent<BoxCollider>().enabled = false;
        // rightHand.GetComponent<VRTK_Pointer>().enabled = false;
        isUsing = true;
        transform.parent.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("HandleSelect"));
        transform.parent.GetComponent<HighlightEffect>().glowHQColor  = GetComponent<MeshRenderer>().material.color;
        transform.parent.GetComponent<HighlightEffect>().glowHQColor.a = 1;
        transform.parent.GetComponent<HighlightEffect>().innerGlowColor  = GetComponent<MeshRenderer>().material.color;
        transform.parent.GetComponent<HighlightEffect>().innerGlowColor.a = 1;
        audioSource.PlayOneShot(Resources.Load<AudioClip>("PressAxis"));
        // // set selected object's parent to newParent
        // newParent.transform.position = selectedObject.transform.position;
        // newParent.transform.rotation = selectedObject.transform.rotation;
        // selectedObject.transform.parent = newParent.transform;
        // selectedObject.transform.localPosition = -selectedObject.GetComponent<BoxCollider>().center;
        // newParent.transform.Translate(-selectedObject.transform.localPosition);
    }
    public void ReleaseTrigger()
    {
        GetComponent<VRTK_InteractableObject>().usingState = 0;
        GetComponent<CapsuleCollider>().radius = 3f;
        isUsing = false;  
        rotateContainerChild.transform.parent = RotateContainer.transform;
        // rotateContainerChild.transform.localPosition = Vector3.zero;
        GetComponent<MeshRenderer>().material.color = new Color(handleOriginalColor.r, handleOriginalColor.g, handleOriginalColor.b, touchAlpha);
        Base.GetComponent<MeshRenderer>().material.color = new Color(baseOriginalColor.r, baseOriginalColor.g, baseOriginalColor.b, touchAlpha);
        // disable edge, enable solid edge
        for(int i=0; i<4; i++){
            Edge[i].SetActive(false);
            SolidEdge[i].SetActive(true);
            SolidEdge[i].GetComponent<DoManipulation>().objectToHL.SetActive(true);
            // turn off highlight
            Edge[i].GetComponent<HighlightEffect>().highlighted = false;
        }
        // set edge local position to original
        for(int i=0; i<4; i++)
        {
            Edge[i].transform.position = SolidEdge[i].GetComponent<DoManipulation>().objectToHL.transform.position;
        }
        // enable face, edge and corner
        for(int i=0; i<BoundingBox.transform.GetChild(0).childCount; i++) // face
        {
            BoundingBox.transform.GetChild(0).GetChild(i).GetComponent<BoxCollider>().enabled = true;
            BoundingBox.transform.GetChild(0).GetChild(i).GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = true;
        }
        for(int i=0; i<BoundingBox.transform.GetChild(2).childCount; i++) // corner
        {
            BoundingBox.transform.GetChild(2).GetChild(i).GetComponent<SphereCollider>().enabled = true;
            BoundingBox.transform.GetChild(2).GetChild(i).GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = true;
        }
        for(int i=0; i<BoundingBox.transform.GetChild(4).childCount; i++) // edge
        {
            BoundingBox.transform.GetChild(4).GetChild(i).GetComponent<CapsuleCollider>().enabled = true;
        }
        GameObject.FindGameObjectWithTag("CopyOfObject").GetComponent<BoxCollider>().enabled = true;
        // rightHand.GetComponent<VRTK_Pointer>().enabled = true;
        transform.parent.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("HandleTouch"));
        transform.parent.GetComponent<HighlightEffect>().outlineColor = GetComponent<MeshRenderer>().material.color;
        transform.parent.GetComponent<HighlightEffect>().outlineColor.a = 1;
        // selectedObject.transform.parent = oriParent.transform;
    }
    public bool getUsingState()
    {
        return isUsing;
    }
    private void flashingEdge()
    {
        if(increase && edgeHLColor.a <= maxFlashAlpha)
        {
            for(int i=0; i<4; i++)
                Edge[i].GetComponent<MeshRenderer>().material.color = edgeHLColor;
            edgeHLColor.a += 0.01f;
        }
        else if(!increase && edgeHLColor.a >= minFlashAlpha)
        {
            for(int i=0; i<4; i++)
                Edge[i].GetComponent<MeshRenderer>().material.color = edgeHLColor;
            edgeHLColor.a -= 0.01f;
        }
        if(edgeHLColor.a < minFlashAlpha || edgeHLColor.a > maxFlashAlpha)
        {
            increase = !increase;
        }
    }
    private void flashingHandle()
    {
        if(increase && handleHLColor.a <= maxFlashAlpha)
        {
            GetComponent<MeshRenderer>().material.color = handleHLColor;
            Base.GetComponent<MeshRenderer>().material.color = handleHLColor;
            handleHLColor.a += 0.01f;
        }
        else if(!increase && handleHLColor.a >= minFlashAlpha)
        {
            GetComponent<MeshRenderer>().material.color = handleHLColor;
            Base.GetComponent<MeshRenderer>().material.color = handleHLColor;
            handleHLColor.a -= 0.01f;
        }
        if(handleHLColor.a < minFlashAlpha || handleHLColor.a > maxFlashAlpha)
        {
            increase = !increase;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(isUsing)
        {
            if(!grab)
                rightHand.GetComponent<VRTK_InteractGrab>().AttemptGrab();
            if(rotateContainerChild.GetComponent<VRTK_InteractableObject>().IsGrabbed())
                grab = true;
            
            if(rotateContainerChild.GetComponent<VRTK_InteractableObject>().IsTouched() && rotateContainerChild.GetComponent<VRTK_InteractableObject>().IsGrabbed())
            {
                BoundingBox.transform.rotation = rotateContainerChild.transform.rotation;
                // newParent.transform.rotation = BoundingBox.transform.rotation;
                // grab edge 
                flash = false;
                increase = true;
                edgeHLColor = oriColor;
                for(int i=0; i<4; i++)
                {
                    Edge[i].GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, grabAlpha);
                    Edge[i].transform.localScale = new Vector3(SolidEdge[i].transform.localScale.x * 2f, SolidEdge[i].transform.localScale.y, SolidEdge[i].transform.localScale.z * 2f);
                    Edge[i].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("EdgeGrab"));
                    if(SceneManager.GetActiveScene().name == "Testing")
                        GM.GetComponent<GameManager>().calRotation();
                }
                if(firstPlay)
                {
                    audioSource.PlayOneShot(Resources.Load<AudioClip>("Grab"));
                    if(SceneManager.GetActiveScene().name == "Testing")
                        GM.GetComponent<GameManager>().release(0.5f);
                    firstPlay = false;
                }
            }
            else if(rotateContainerChild.GetComponent<VRTK_InteractableObject>().IsTouched())
            {
                // touch edge 
                flash = false;
                increase = true;
                edgeHLColor = oriColor;
                for(int i=0; i<4; i++)
                {
                    Edge[i].GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, touchAlpha);
                    Edge[i].transform.localScale = new Vector3(SolidEdge[i].transform.localScale.x * 2f, SolidEdge[i].transform.localScale.y, SolidEdge[i].transform.localScale.z * 2f);
                    Edge[i].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("EdgeTouch"));
                    // Edge[i].GetComponent<HighlightEffect>().highlighted = true;
                }
                firstPlay = true;
            }
            else
            {
                flash = true;
                for(int i=0; i<4; i++)
                {
                    Edge[i].transform.localScale = SolidEdge[i].transform.localScale;
                    Edge[i].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("EdgeOpposite"));
                }
                firstPlay = true;
            }
            if(flash)
                flashingEdge();
            if(rotateContainerChild.transform.rotation != BoundingBox.transform.rotation)
                rotateContainerChild.transform.rotation = BoundingBox.transform.rotation;
        }
        else
        {
            if(isNearTouching)
            {
                if((SolidEdge[0].GetComponent<DoManipulation>().isPressing && !SolidEdge[2].GetComponent<DoManipulation>().isPressing && SolidEdge[0].GetComponent<VRTK_InteractableObject>().GetGrabbingObject().name != leftHand.name) || 
                   (SolidEdge[1].GetComponent<DoManipulation>().isPressing && !SolidEdge[3].GetComponent<DoManipulation>().isPressing && SolidEdge[1].GetComponent<VRTK_InteractableObject>().GetGrabbingObject().name != leftHand.name) || 
                   (SolidEdge[2].GetComponent<DoManipulation>().isPressing && !SolidEdge[0].GetComponent<DoManipulation>().isPressing && SolidEdge[2].GetComponent<VRTK_InteractableObject>().GetGrabbingObject().name != leftHand.name) ||
                   (SolidEdge[3].GetComponent<DoManipulation>().isPressing && !SolidEdge[1].GetComponent<DoManipulation>().isPressing && SolidEdge[3].GetComponent<VRTK_InteractableObject>().GetGrabbingObject().name != leftHand.name))
                {
                    // 若右手抓edge時，左手已經neartouch且沒抓edge，則flashing
                    GetComponent<MeshRenderer>().enabled = true;
                    Base.GetComponent<MeshRenderer>().enabled = true;
                    transform.parent.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("HandleOpposite"));
                    transform.parent.GetComponent<HighlightEffect>().glowHQColor  = GetComponent<MeshRenderer>().material.color;
                    transform.parent.GetComponent<HighlightEffect>().glowHQColor.a = 1;
                    transform.parent.GetComponent<HighlightEffect>().highlighted = true;
                    flashingHandle();
                }
                else
                {
                    GetComponent<MeshRenderer>().enabled = false;
                    Base.GetComponent<MeshRenderer>().enabled = false;
                    transform.parent.GetComponent<HighlightEffect>().highlighted = false;
                }
            }
            rotateContainerChild.transform.parent = RotateContainer.transform;
            // rotateContainerChild.transform.localPosition = Vector3.zero;
        }
        for(int i=0; i<4; i++)
            Edge[i].transform.position = SolidEdge[i].GetComponent<DoManipulation>().objectToHL.transform.position;
        // for(int i=0; i<BoundingBox.transform.GetChild(5).childCount; i++)
        // {
        //     if(BoundingBox.transform.GetChild(5).GetChild(i).transform.localPosition != Vector3.zero)
        //         BoundingBox.transform.GetChild(5).GetChild(i).transform.localPosition = Vector3.zero;
        //         //Destroy(BoundingBox.transform.GetChild(5).GetChild(i).gameObject);
        // }
        // var min = Mathf.Min(BoundingBox.transform.localScale.x, BoundingBox.transform.localScale.y, BoundingBox.transform.localScale.z);
        // transform.parent.localScale = oriSize * (min / oriBoundMinSize);
    }
}
