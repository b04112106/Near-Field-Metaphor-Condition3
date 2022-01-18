using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.Controllables;
using IndiePixel.VR;
using HighlightPlus;
using UnityEngine.SceneManagement;
public class DoManipulation : MonoBehaviour
{
    public GameObject BoundingBox;
    public GameObject oppsiteObject, objectToHL;
    public Material objectM;
    public GameObject [] resizeFace;
    public GameObject [] resizeEdge;
    public GameObject [] resizeCorner;
    private GameObject leftHand, rightHand;
    private GameObject RotateHandler;
    private GameObject SelectedObject;
    private bool LeftHandUp = false, changeParent = true;
    private bool isTranslation = false, isRotation = false, isScaling = false;
    private GameObject originalParent;
    private Quaternion originalRotation;
    private Vector3 originalPosition, originalLocalPosition, originalLocalScale;
    private Color oriColor, hlColor;
    private bool increase = true; // true:increase alpha, false:decrease alpha
    private Vector3 leftHandOriPos;
    private float ScaleCoefficient = 0.0f;
    private int flag = 0;
    private GameObject empty;
    private GameObject empty1;
    private char reCh = ' ';
    private float reF = 0.0f;
    private int sclaingMode = 0; // -1:default, 0:uniform, 1:anchored
    private int manipulationMode = 0; // 0:default, 1:TX, 2:TY, 3:TZ, 4:RX, 5:RY, 6:RZ, 7:SX, 8:SY, 9:SZ
                                      //            10:TXY, 11:TYZ, 12:TXZ, 13:SXY, 14:SYZ, 15:SXZ
                                      //            16:TXYZ, 17SXYZ
    private int mode = 1; // -1:default, 0:gesture, 1:grab oppsite
    // Start is called before the first frame update
    public bool isPressing = false, isNearTouching = false;
    public float velocity = 0.0f;
    private float oriBoundMinSize = 0.0f, oriCollSize = 0.0f;
    private Vector3 velOriPosition; // used to calculate velocity, same as originalPosition
    private float oriLength; // the length between this object and opposite object
    public GameObject attachController; // is right or left controller attach on this object
    private GameObject menu;
    private float minFlashAlpha = 0.0f, maxFlashAlpha = 0.4f, touchAlpha = 0.5f, grabAlpha = 0.8f;
    private HighlightEffect effect;
    private GameObject GM;
    void Start()
    {
        originalLocalPosition = this.gameObject.transform.localPosition;
        originalLocalScale = this.gameObject.transform.localScale;
        SelectedObject = GameObject.FindWithTag("SelectedObject");
        ScaleCoefficient = SelectedObject.GetComponent<PersonalSpace>().ScaleCoefficient;
        RotateHandler = GameObject.Find("RotateHandler");
        leftHand = GameObject.Find("LeftControllerScriptAlias");
        rightHand = GameObject.Find("RightControllerScriptAlias");
        leftHandOriPos = leftHand.transform.position;
        if(!GameObject.Find("empty"))
            empty = new GameObject("empty");
        else    
            empty = GameObject.Find("empty");
        if(!GameObject.Find("empty1"))
            empty1 = new GameObject("empty1");
        else    
            empty1 = GameObject.Find("empty1");
        if(SelectedObject.transform.parent != null)
            originalParent = SelectedObject.transform.parent.gameObject;
        menu = GameObject.Find("RadialMenu_Canvas");
        velOriPosition = transform.position;
        // set ignored collider as cooy of object
        if(tag == "Corner" || tag == "Face")
            GetComponent<VRTK_InteractableObject>().ignoredColliders[0] = GameObject.FindGameObjectWithTag("CopyOfObject").GetComponent<Collider>();
        oriColor = objectM.color;
        hlColor = oriColor;
        effect = objectToHL.GetComponent<HighlightEffect>();
        oriBoundMinSize = Mathf.Min(BoundingBox.transform.localScale.x, BoundingBox.transform.localScale.y, BoundingBox.transform.localScale.z);
        if(tag == "Face")
            oriCollSize = Mathf.Max(GetComponent<BoxCollider>().size.x, GetComponent<BoxCollider>().size.y, GetComponent<BoxCollider>().size.z);
        else if(tag == "Corner")
            oriCollSize = GetComponent<SphereCollider>().radius;
        else if(tag == "Edge")
            oriCollSize = GetComponent<CapsuleCollider>().radius;
        if(SceneManager.GetActiveScene().name == "Testing")
            GM = GameObject.Find("GameManager");
    }
    public void Touch()
    {
        // set material color & HL effect
        if(tag == "Face")
        {
            objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, touchAlpha);
            effect.ProfileLoad(Resources.Load<HighlightProfile>("FaceTouch"));
        }
        else if(tag == "Edge")
        {
            objectToHL.GetComponent<MeshRenderer>().material.color = new Color(0.9716981f, 0.7230207f, 0.3345942f, touchAlpha);
            effect.ProfileLoad(Resources.Load<HighlightProfile>("EdgeTouch"));
        }
        else if(tag == "Corner")
        {
            objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, touchAlpha);
            effect.ProfileLoad(Resources.Load<HighlightProfile>("CornerTouch"));
        }
        effect.highlighted = true;
        // set which controller can interact with target
        if(GetComponent<VRTK_InteractableObject>().GetTouchingObjects()[0].name == "RightControllerScriptAlias")
        {
            GetComponent<VRTK_InteractableObject>().allowedTouchControllers = VRTK_InteractableObject.AllowedController.RightOnly;
            GetComponent<VRTK_InteractableObject>().allowedNearTouchControllers = VRTK_InteractableObject.AllowedController.RightOnly;
        }
        else if(GetComponent<VRTK_InteractableObject>().GetTouchingObjects()[0].name == "LeftControllerScriptAlias")
        {
            GetComponent<VRTK_InteractableObject>().allowedTouchControllers = VRTK_InteractableObject.AllowedController.LeftOnly;
            GetComponent<VRTK_InteractableObject>().allowedNearTouchControllers = VRTK_InteractableObject.AllowedController.LeftOnly;
        }
        // rightHand.GetComponent<VRTK_Pointer>().enabled = false;
    }
    public void UnTouch()
    {
        GetComponent<VRTK_InteractableObject>().ResetIgnoredColliders();
        objectToHL.GetComponent<MeshRenderer>().material.color = oriColor;
        effect.highlighted = false;
        GetComponent<VRTK_InteractableObject>().allowedTouchControllers = VRTK_InteractableObject.AllowedController.Both;
        GetComponent<VRTK_InteractableObject>().allowedNearTouchControllers = VRTK_InteractableObject.AllowedController.Both;
        if(tag == "Corner" && oppsiteObject.GetComponent<DoManipulation>().isPressing)
        {
            objectToHL.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("CornerOpposite"));
            hlColor = oriColor;
            objectToHL.GetComponent<HighlightEffect>().highlighted = true;
            objectToHL.GetComponent<MeshRenderer>().material.color = hlColor;
        }
    }
    public void NearTouch()
    {
        isNearTouching = true;
        if(oppsiteObject.GetComponent<DoManipulation>().isPressing)
        {
            increase = true;
            if(tag == "Face")
            {
                effect.ProfileLoad(Resources.Load<HighlightProfile>("FaceOpposite"));
                hlColor = oriColor;
            }
            else if(tag == "Edge")
            {
                effect.ProfileLoad(Resources.Load<HighlightProfile>("EdgeOpposite"));
                hlColor = new Color(0.9716981f, 0.7230207f, 0.3345942f, minFlashAlpha);
            }
            effect.highlighted = true;
            objectToHL.GetComponent<MeshRenderer>().material.color = hlColor;
        }
        // rightHand.GetComponent<VRTK_Pointer>().enabled = false;
    }
    public void UnNearTouch()
    {
        isNearTouching = false;
        if(tag != "Corner")
        {
            hlColor = oriColor;
            objectToHL.GetComponent<MeshRenderer>().material.color = oriColor;
            effect.highlighted = false;
        }
        // rightHand.GetComponent<VRTK_Pointer>().enabled = true;
    }
    private void FlashingObject()
    {
        if(isNearTouching && !isPressing && oppsiteObject.GetComponent<DoManipulation>().isPressing)
        {    
            if(increase && hlColor.a <= maxFlashAlpha)
            {
                objectToHL.GetComponent<MeshRenderer>().material.color = hlColor;
                hlColor.a += 0.01f;
            }
            else if(!increase && hlColor.a >= minFlashAlpha)
            {
                objectToHL.GetComponent<MeshRenderer>().material.color = hlColor;
                hlColor.a -= 0.01f;
            }
            if(hlColor.a < minFlashAlpha || hlColor.a > maxFlashAlpha)
            {
                increase = !increase;
            }
        }
        else if(isNearTouching && !isPressing && !oppsiteObject.GetComponent<DoManipulation>().isPressing)
        {
            hlColor = oriColor;
            objectToHL.GetComponent<MeshRenderer>().material.color = oriColor;
            effect.highlighted = false;
        }
    }
    public void PressGrip()
    {
        isPressing = true;
        // if opposite object is neartouching, then flash opposite object
        if(oppsiteObject.GetComponent<DoManipulation>().isNearTouching)
        {
            // if neartouch controller and grab controller is the same, and another controller didn't neartouch, then don't need to highlight
            if(oppsiteObject.GetComponent<VRTK_InteractableObject>().GetNearTouchingObjects()[0] == GetComponent<VRTK_InteractableObject>().GetGrabbingObject() && oppsiteObject.GetComponent<VRTK_InteractableObject>().GetNearTouchingObjects().Count == 1)
                oppsiteObject.GetComponent<DoManipulation>().isNearTouching = false;
            else
                oppsiteObject.GetComponent<DoManipulation>().NearTouch();
        }
        if(tag == "Face" || tag == "Corner")
        {
            for(int i=0; i<6; i++)
            {
                RotateHandler.transform.GetChild(i).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
            }
        }
        else if(tag == "Edge")
        {
            if(GetComponent<VRTK_InteractableObject>().GetGrabbingObject().name == "LeftControllerScriptAlias")
            {
                for(int i=0; i<6; i++)
                {
                    RotateHandler.transform.GetChild(i).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                    RotateHandler.transform.GetChild(i).GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                    RotateHandler.transform.GetChild(i).GetChild(1).GetComponent<MeshRenderer>().enabled = false;
                }
            }
            else
            {
                if(name == "SolidEdge 0" || name == "SolidEdge 2" || name == "SolidEdge 8" || name == "SolidEdge 10")
                {
                    RotateHandler.transform.GetChild(0).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                    RotateHandler.transform.GetChild(1).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                    RotateHandler.transform.GetChild(2).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                    RotateHandler.transform.GetChild(3).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                }
                else if(name == "SolidEdge 1" || name == "SolidEdge 3" || name == "SolidEdge 9" || name == "SolidEdge 11")
                {
                    RotateHandler.transform.GetChild(0).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                    RotateHandler.transform.GetChild(1).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                    RotateHandler.transform.GetChild(4).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                    RotateHandler.transform.GetChild(5).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                }
                else if(name == "SolidEdge 4" || name == "SolidEdge 5" || name == "SolidEdge 6" || name == "SolidEdge 7")
                {
                    RotateHandler.transform.GetChild(2).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                    RotateHandler.transform.GetChild(3).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                    RotateHandler.transform.GetChild(4).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                    RotateHandler.transform.GetChild(5).GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
                }
            }
        }
        oriLength = Mathf.Abs((transform.position - oppsiteObject.transform.position).magnitude);
        originalPosition = this.gameObject.transform.position;
        originalRotation = this.gameObject.transform.rotation;
        originalLocalPosition = this.gameObject.transform.localPosition;
        reF = 1f;
        if(mode == 0)
        {
            if(!LeftHandUp) // do translation
            {
                isTranslation = true;
                // change near touch highlight color
                if(this.gameObject.tag == "Face") // 1-dim
                {
                    objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, grabAlpha);
                    int index = 5; // number index
                    if(this.gameObject.name[index] == '1' || this.gameObject.name[index] == '3')
                    { // X-axis
                        manipulationMode = 1;
                    }
                    else if(this.gameObject.name[index] == '0' || this.gameObject.name[index] == '5')
                    { // Y-axis
                        manipulationMode = 2;
                    }
                    else if(this.gameObject.name[index] == '2' || this.gameObject.name[index] == '4')
                    { // Z-axis
                        manipulationMode = 3;
                    }
                }
                else if(this.gameObject.tag == "Edge") // 2-dim
                {
                    objectToHL.GetComponent<MeshRenderer>().material.color = new Color(0.9716981f, 0.7230207f, 0.3345942f, grabAlpha);
                    int index = 10; // number index
                    if(this.gameObject.name[index] == '0' || this.gameObject.name[index] == '2' || this.gameObject.name[index] == '8' || this.gameObject.name.Substring(index) == "10")
                    { // X-Y plane
                        manipulationMode = 10;
                    }
                    else if(this.gameObject.name[index] == '1' || this.gameObject.name[index] == '3' || this.gameObject.name[index] == '9' || this.gameObject.name.Substring(index) == "11")
                    { // Y-Z plane
                        manipulationMode = 11;
                    }
                    else if(this.gameObject.name[index] == '4' || this.gameObject.name[index] == '5' || this.gameObject.name[index] == '6' || this.gameObject.name[index] == '7')
                    { // X-Z plane
                        manipulationMode = 12;
                    }
                }
                else if(this.gameObject.tag == "CopyOfObject") // 3-dim
                {
                    manipulationMode = 16;
                    
                }
            } 
            else // do scaling
            {
                isScaling = true;
                // change near touch highlight color
                if(this.gameObject.tag == "Face") // 1-dim
                {
                    objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, grabAlpha);
                    int index = 5; // number index
                    if(this.gameObject.name[index] == '1' || this.gameObject.name[index] == '3')
                    { // X-axis
                        manipulationMode = 7;
                    }
                    else if(this.gameObject.name[index] == '0' || this.gameObject.name[index] == '5')
                    { // Y-axis
                        manipulationMode = 8;
                    }
                    else if(this.gameObject.name[index] == '2' || this.gameObject.name[index] == '4')
                    { // Z-axis
                        manipulationMode = 9;
                    }
                }
                else if(this.gameObject.tag == "Edge")
                {
                    objectToHL.GetComponent<MeshRenderer>().material.color = new Color(0.9716981f, 0.7230207f, 0.3345942f, grabAlpha);
                    int index = 10; // number index
                    if(this.gameObject.name[index] == '0' || this.gameObject.name[index] == '2' || this.gameObject.name[index] == '8' || this.gameObject.name.Substring(index) == "10")
                    { // X-Y plane
                        manipulationMode = 13;
                    }
                    else if(this.gameObject.name[index] == '1' || this.gameObject.name[index] == '3' || this.gameObject.name[index] == '9' || this.gameObject.name.Substring(index) == "11")
                    { // Y-Z plane
                        manipulationMode = 14;
                    }
                    else if(this.gameObject.name[index] == '4' || this.gameObject.name[index] == '5' || this.gameObject.name[index] == '6' || this.gameObject.name[index] == '7')
                    { // X-Z plane
                        manipulationMode = 15;
                    }
                }
                else if(this.gameObject.tag == "Corner")
                {
                    objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, grabAlpha);
                    manipulationMode = 17;
                }
            }
        }
        // rightHand.GetComponent<VRTK_Pointer>().enabled = false;
        if(tag == "Corner")
        {
            oppsiteObject.GetComponent<DoManipulation>().objectToHL.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("CornerOpposite"));
            hlColor = oriColor;
            oppsiteObject.GetComponent<DoManipulation>().objectToHL.GetComponent<HighlightEffect>().highlighted = true;
            oppsiteObject.GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().material.color = hlColor;
        }
    }
    public void RleaseGrip()
    {
        isPressing = false;
        manipulationMode = 0;
        // clean to original color
        objectToHL.GetComponent<MeshRenderer>().material.color = oriColor;
        //adjust local position
        transform.localPosition = originalLocalPosition;
        transform.rotation = originalRotation;
        if(isTranslation)
        {
            if(mode == 0)
                this.gameObject.transform.localScale = originalLocalScale;
            else if(mode == 1)
            {
                if(reCh == 'X')
                    transform.localScale = new Vector3(transform.localScale.x/reF, transform.localScale.y, transform.localScale.z);
                else if(reCh == 'Y')
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y/reF, transform.localScale.z);
                else if(reCh == 'Z')
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z/reF);
                else if(reCh == 'I')
                {
                    transform.localScale = new Vector3(transform.localScale.x/reF/2f, transform.localScale.y, transform.localScale.z/reF/2f);
                    GetComponent<OriginalBoundingBox>().doUnTouch = false;
                }
                else if(reCh == 'J')
                {
                    transform.localScale = new Vector3(transform.localScale.x/reF/2f, transform.localScale.y, transform.localScale.z/reF/2f);
                    GetComponent<OriginalBoundingBox>().doUnTouch = false;
                }
                else if(reCh == 'K')
                {
                    transform.localScale = new Vector3(transform.localScale.x/reF/2f, transform.localScale.y, transform.localScale.z/reF/2f);
                    GetComponent<OriginalBoundingBox>().doUnTouch = false;
                }
                else if(reCh == 'L')
                    transform.localScale = new Vector3(transform.localScale.x/reF, transform.localScale.y/reF, transform.localScale.z/reF);
                originalLocalScale = transform.localScale;
                if(tag == "Edge")
                    objectToHL.transform.localScale = transform.localScale;
            }
        }
        else if (isScaling)
        {
            if(reCh == 'X')
                transform.localScale = new Vector3(transform.localScale.x/reF, transform.localScale.y, transform.localScale.z);
            else if(reCh == 'Y')
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y/reF, transform.localScale.z);
            else if(reCh == 'Z')
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z/reF);
            else if(reCh == 'I')
            {
                transform.localScale = new Vector3(transform.localScale.x/reF/2f, transform.localScale.y, transform.localScale.z/reF/2f);
                GetComponent<OriginalBoundingBox>().doUnTouch = false;
            }
            else if(reCh == 'J')
            {
                transform.localScale = new Vector3(transform.localScale.x/reF/2f, transform.localScale.y, transform.localScale.z/reF/2f);
                GetComponent<OriginalBoundingBox>().doUnTouch = false;
            }
            else if(reCh == 'K')
            {
                transform.localScale = new Vector3(transform.localScale.x/reF/2f, transform.localScale.y, transform.localScale.z/reF/2f);
                GetComponent<OriginalBoundingBox>().doUnTouch = false;
            }
            else if(reCh == 'L')
                transform.localScale = new Vector3(transform.localScale.x/reF, transform.localScale.y/reF, transform.localScale.z/reF);
            originalLocalScale = transform.localScale;
            if(tag == "Edge")
                objectToHL.transform.localScale = transform.localScale;
        }
        isTranslation = false;
        isRotation = false;
        isScaling = false;
        changeParent = true;
        BoundingBox.transform.parent = null;
        if(originalParent != null)
            SelectedObject.transform.parent = originalParent.transform;
        else
            SelectedObject.transform.parent = null;
        attachController = null;
        if(!oppsiteObject.GetComponent<DoManipulation>().isPressing)
        {
            for(int i=0; i<BoundingBox.transform.GetChild(0).childCount; i++)
            {
                BoundingBox.transform.GetChild(0).GetChild(i).GetComponent<BoxCollider>().enabled = true;
                BoundingBox.transform.GetChild(0).GetChild(i).GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = true;
            }
            for(int i=0; i<BoundingBox.transform.GetChild(2).childCount; i++)
            {
                BoundingBox.transform.GetChild(2).GetChild(i).GetComponent<SphereCollider>().enabled = true;
                BoundingBox.transform.GetChild(2).GetChild(i).GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = true;
            }
            for(int i=0; i<BoundingBox.transform.GetChild(4).childCount; i++)
            {
                BoundingBox.transform.GetChild(4).GetChild(i).GetComponent<CapsuleCollider>().enabled = true;
            }
            GameObject.FindGameObjectWithTag("CopyOfObject").GetComponent<BoxCollider>().enabled = true;
            if(tag == "Face" || tag == "Corner")
            {
                for(int i=0; i<6; i++)
                {
                    RotateHandler.transform.GetChild(i).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                }
            }
            else if(tag == "Edge")
            {
                RotateHandler.transform.GetChild(0).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                RotateHandler.transform.GetChild(1).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                RotateHandler.transform.GetChild(2).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                RotateHandler.transform.GetChild(3).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                RotateHandler.transform.GetChild(4).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                RotateHandler.transform.GetChild(5).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
            }
            if(tag == "Corner")
            {
                objectToHL.GetComponent<HighlightEffect>().highlighted = false;
                oppsiteObject.GetComponent<DoManipulation>().objectToHL.GetComponent<HighlightEffect>().highlighted = false;
            }
        }
        else
        {
            if(oppsiteObject.GetComponent<VRTK_InteractableObject>().GetGrabbingObject().name == "RightControllerScriptAlias")
            {
                if(name == "SolidEdge 0" || name == "SolidEdge 2" || name == "SolidEdge 8" || name == "SolidEdge 10")
                {
                    RotateHandler.transform.GetChild(4).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                    RotateHandler.transform.GetChild(5).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                }
                else if(name == "SolidEdge 1" || name == "SolidEdge 3" || name == "SolidEdge 9" || name == "SolidEdge 11")
                {
                    RotateHandler.transform.GetChild(0).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                    RotateHandler.transform.GetChild(1).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                    RotateHandler.transform.GetChild(2).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                    RotateHandler.transform.GetChild(3).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                }
                else if(name == "SolidEdge 4" || name == "SolidEdge 5" || name == "SolidEdge 6" || name == "SolidEdge 7")
                {
                    RotateHandler.transform.GetChild(0).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                    RotateHandler.transform.GetChild(1).GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
                }
            }
        }
        // both controller can interact with the bounding box
        GetComponent<VRTK_InteractableObject>().allowedNearTouchControllers = VRTK_InteractableObject.AllowedController.Both;
        GetComponent<VRTK_InteractableObject>().allowedTouchControllers = VRTK_InteractableObject.AllowedController.Both;
        GetComponent<VRTK_InteractableObject>().allowedGrabControllers = VRTK_InteractableObject.AllowedController.Both;
        if(SceneManager.GetActiveScene().name == "Testing")
            GM.GetComponent<GameManager>().release(1f);
    }
    private void DoTranslation()
    {
        transform.rotation = originalRotation;
        Vector3 v = transform.position - originalPosition; // controller movement
        Vector3 u = Vector3.zero; // projection vector

        if(manipulationMode == 1) // TX
        {
            u = Vector3.Project(v, BoundingBox.transform.right);
        }
        else if(manipulationMode == 2) // TY
        {
            u = Vector3.Project(v, BoundingBox.transform.up);
        }
        else if(manipulationMode == 3) // TZ
        {
            u = Vector3.Project(v, BoundingBox.transform.forward);
        }
        else if(manipulationMode == 10) // TXY
        {
            u = Vector3.ProjectOnPlane(v, BoundingBox.transform.forward);
        }
        else if(manipulationMode == 11) // TYZ
        {
            u = Vector3.ProjectOnPlane(v, BoundingBox.transform.right);
        }
        else if(manipulationMode == 12) // TXZ
        {
            u = Vector3.ProjectOnPlane(v, BoundingBox.transform.up);
        }
        BoundingBox.transform.Translate(u, Space.World);
        SelectedObject.transform.Translate(u / ScaleCoefficient, Space.World);
        transform.position = originalPosition;
        transform.Translate(u, Space.World);
        originalPosition = transform.position;
    }
    private void DoScaling()
    {
        this.gameObject.transform.rotation = originalRotation;
        Vector3 v = this.gameObject.transform.position - originalPosition; // controller translation vector
        Vector3 u = Vector3.zero; // v project on dir axis
        Vector3 factor = Vector3.zero; // sclaing factor
        Vector3 t = Vector3.zero; // the distance vector
        if(sclaingMode == 1)
            t = originalPosition - empty.transform.position;
        else if(sclaingMode == 0)
            t = originalPosition - BoundingBox.transform.position;
        float len = t.magnitude;
        reCh = ' ';
        reF = 0.0f;

        if(manipulationMode == 7) // SX
        {
            u = Vector3.Project(v, transform.right);
            if(name == "Face 1")
            {
                if(Vector3.Dot(v, transform.right) > 0 )
                    factor = new Vector3(u.magnitude/len + 1, 1, 1);
                else if(Vector3.Dot(v, transform.right) < 0 )
                    factor = new Vector3(-u.magnitude/len + 1, 1, 1);
            }
            else if(name == "Face 3")
            {                
                if(Vector3.Dot(v, transform.right) > 0 )
                    factor = new Vector3(-u.magnitude/len + 1, 1, 1);
                else if(Vector3.Dot(v, transform.right) < 0 )
                    factor = new Vector3(u.magnitude/len + 1, 1, 1);
            }
            t *= factor.x;
            reCh = 'X';
            reF = factor.x;
        }
        else if(manipulationMode == 8) // SY
        {
            u = Vector3.Project(v, transform.up);
            if(name == "Face 0")
            {
                if(Vector3.Dot(v, transform.up) > 0 )
                    factor = new Vector3(1, u.magnitude/len + 1, 1);
                else if(Vector3.Dot(v, transform.up) < 0 )
                    factor = new Vector3(1, -u.magnitude/len + 1, 1);
            }
            else if(name == "Face 5")
            {                
                if(Vector3.Dot(v, transform.up) > 0 )
                    factor = new Vector3(1, -u.magnitude/len + 1, 1);
                else if(Vector3.Dot(v, transform.up) < 0 )
                    factor = new Vector3(1, u.magnitude/len + 1, 1);
            }
            t *= factor.y;
            reCh = 'Y';
            reF = factor.y;
        }
        else if(manipulationMode == 9) // SZ
        {
            u = Vector3.Project(v, transform.forward);
            if(name == "Face 4")
            {
                if(Vector3.Dot(v, transform.forward) > 0 )
                    factor = new Vector3(1, 1, u.magnitude/len + 1);
                else if(Vector3.Dot(v, transform.forward) < 0 )
                    factor = new Vector3(1, 1, -u.magnitude/len + 1);
            }
            else if(name == "Face 2")
            {                
                if(Vector3.Dot(v, transform.forward) > 0 )
                    factor = new Vector3(1, 1, -u.magnitude/len + 1);
                else if(Vector3.Dot(v, transform.forward) < 0 )
                    factor = new Vector3(1, 1, u.magnitude/len + 1);
            }
            t *= factor.z;
            reCh = 'Z';
            reF = factor.z;
        }
        else if(manipulationMode == 13) // SXY
        {
            Vector3 tmpv = Vector3.zero;
            if(name == "SolidEdge 0")
                tmpv = transform.right - transform.forward;
            else if(name == "SolidEdge 2")
                tmpv = -transform.right - transform.forward;
            else if(name == "SolidEdge 8")
                tmpv = transform.right + transform.forward;
            else if(name == "SolidEdge 10")
                tmpv = -transform.right + transform.forward;
            u = Vector3.Project(v, tmpv);
            if(Vector3.Dot(v, tmpv) > 0)
                factor = new Vector3(u.magnitude/len + 1, u.magnitude/len + 1, 1);
            else if(Vector3.Dot(v, tmpv) < 0)
                factor = new Vector3(-u.magnitude/len + 1, -u.magnitude/len + 1, 1);
            t *= factor.x;
            reCh = 'I';
            reF = factor.x;
        }
        else if(manipulationMode == 14) // SYZ
        {
            Vector3 tmpv = Vector3.zero;
            if(name == "SolidEdge 1")
                tmpv = transform.right - transform.forward;
            else if(name == "SolidEdge 3")
                tmpv = -transform.right - transform.forward;
            else if(name == "SolidEdge 9")
                tmpv = transform.right + transform.forward;
            else if(name == "SolidEdge 11")
                tmpv = -transform.right + transform.forward;
            u = Vector3.Project(v, tmpv);
            if(Vector3.Dot(v, tmpv) > 0)
                factor = new Vector3(1, u.magnitude/len + 1, u.magnitude/len + 1);
            else if(Vector3.Dot(v, tmpv) < 0)
                factor = new Vector3(1, -u.magnitude/len + 1, -u.magnitude/len + 1);
            t *= factor.y;
            reCh = 'J';
            reF = factor.y;
        }
        else if(manipulationMode == 15) // SXZ
        {
            Vector3 tmpv = Vector3.zero;
            if(name == "SolidEdge 4")
                tmpv = transform.right - transform.forward;
            else if(name == "SolidEdge 5")
                tmpv = -transform.right - transform.forward;
            else if(name == "SolidEdge 6")
                tmpv = -transform.right + transform.forward;
            else if(name == "SolidEdge 7")
                tmpv = transform.right + transform.forward;
            u = Vector3.Project(v, tmpv);
            if(Vector3.Dot(v, tmpv) > 0)
                factor = new Vector3(u.magnitude/len + 1, 1, u.magnitude/len + 1);
            else if(Vector3.Dot(v, tmpv) < 0)
                factor = new Vector3(-u.magnitude/len + 1, 1, -u.magnitude/len + 1);
            t *= factor.x;
            reCh = 'K';
            reF = factor.x;
        }
        else if(manipulationMode == 17) // SXYZ
        {
            Vector3 tmpv = Vector3.zero;
            tmpv = transform.right + transform.forward + transform.up;
            u = Vector3.Project(v, tmpv);
            if(Vector3.Dot(v, tmpv) > 0)
                factor = new Vector3(u.magnitude/len + 1, u.magnitude/len + 1, u.magnitude/len + 1);
            else if(Vector3.Dot(v, tmpv) < 0)
                factor = new Vector3(-u.magnitude/len + 1, -u.magnitude/len + 1, -u.magnitude/len + 1);
            t *= factor.x;
            reCh = 'L';
            reF = factor.x;
        }

        // change bounding box & selected object size, and move the target
        if(sclaingMode == 0) // do uniform scaling
        {
            if(factor.x != Mathf.Infinity && factor.x != -Mathf.Infinity && factor.x != 0 && factor.y != Mathf.Infinity && factor.y != -Mathf.Infinity && factor.y != 0 && factor.z != Mathf.Infinity && factor.z != -Mathf.Infinity && factor.z != 0)
            {
                BoundingBox.transform.localScale = new Vector3(BoundingBox.transform.localScale.x * factor.x, BoundingBox.transform.localScale.y * factor.y, BoundingBox.transform.localScale.z * factor.z);
                // 下面這行是根據statue而調整，日後需要修正
                SelectedObject.transform.localScale = new Vector3(SelectedObject.transform.localScale.x * factor.x, SelectedObject.transform.localScale.y * factor.y, SelectedObject.transform.localScale.z * factor.z);
                if(BoundingBox.transform.localScale.x < 0 || BoundingBox.transform.localScale.y < 0 || BoundingBox.transform.localScale.z < 0)
                {
                    BoundingBox.transform.localScale = new Vector3(BoundingBox.transform.localScale.x / factor.x, BoundingBox.transform.localScale.y / factor.y, BoundingBox.transform.localScale.z / factor.z);
                    SelectedObject.transform.localScale = new Vector3(SelectedObject.transform.localScale.x / factor.x, SelectedObject.transform.localScale.y / factor.y, SelectedObject.transform.localScale.z / factor.z);
                    return;
                }
                this.gameObject.transform.position = originalPosition;
                this.gameObject.transform.position = BoundingBox.transform.position + t;
                originalPosition = this.gameObject.transform.position;
                resize(reCh, reF);
            }
        }
        else if(sclaingMode == 1) // do anchored scaling
        {
            if(factor.x != Mathf.Infinity && factor.x != -Mathf.Infinity && factor.x != 0 && factor.y != Mathf.Infinity && factor.y != -Mathf.Infinity && factor.y != 0 && factor.z != Mathf.Infinity && factor.z != -Mathf.Infinity && factor.z != 0)
            {
                empty.transform.localScale = new Vector3(empty.transform.localScale.x * factor.x, empty.transform.localScale.y * factor.y, empty.transform.localScale.z * factor.z);
                empty1.transform.localScale = new Vector3(empty1.transform.localScale.x * factor.x, empty1.transform.localScale.y * factor.z, empty1.transform.localScale.z * factor.y);
                if(empty.transform.localScale.x < 0 || empty.transform.localScale.y < 0 || empty.transform.localScale.z < 0)
                {
                    empty.transform.localScale = new Vector3(empty.transform.localScale.x / factor.x, empty.transform.localScale.y / factor.y, empty.transform.localScale.z / factor.z);
                    empty1.transform.localScale = new Vector3(empty1.transform.localScale.x / factor.x, empty1.transform.localScale.y / factor.z, empty1.transform.localScale.z / factor.y);
                    return;
                }
                this.gameObject.transform.position = originalPosition;
                this.gameObject.transform.position = empty.transform.position + t;
                originalPosition = this.gameObject.transform.position;
                resize(reCh, reF);
            }
        }

    }
    private void DoRotation()
    {
        // use VRTK_ArtificialRotator to do rotation
        // rotate handle-bar event control in RotateHandle.cs
    }
    private void resize(char ch, float f)
    {
        if(ch == 'X')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                if(mode == 1)
                {
                    objectToHL.transform.localScale = new Vector3(objectToHL.transform.localScale.x/f, objectToHL.transform.localScale.y, objectToHL.transform.localScale.z);
                    var obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                    obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y, obb.transform.localScale.z);
                    continue;
                }
                resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x/f, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                if(resizeCorner[i].name == "Corner 1" ||　resizeCorner[i].name == "Corner 3" || resizeCorner[i].name == "Corner 4" || resizeCorner[i].name == "Corner 6")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y, resizeCorner[i].transform.localScale.z);
                else if(resizeCorner[i].name == "Corner 0" ||　resizeCorner[i].name == "Corner 2" || resizeCorner[i].name == "Corner 5" || resizeCorner[i].name == "Corner 7")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x, resizeCorner[i].transform.localScale.y, resizeCorner[i].transform.localScale.z/f);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
            for(int i=0; i<6; i++)
            {
                GameObject resizeBar = RotateHandler.transform.GetChild(i).gameObject;
                if(resizeBar.name == "HandleY" || resizeBar.name == "HandleZ")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x/f, resizeBar.transform.localScale.y, resizeBar.transform.localScale.z);
                else if(resizeBar.name == "HandleX")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x, resizeBar.transform.localScale.y/f, resizeBar.transform.localScale.z);
            }
        }
        else if(ch == 'Y')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                if(mode == 1)
                {
                    objectToHL.transform.localScale = new Vector3(objectToHL.transform.localScale.x, objectToHL.transform.localScale.y/f, objectToHL.transform.localScale.z);
                    var obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                    obb.transform.localScale = new Vector3(obb.transform.localScale.x, obb.transform.localScale.y/f, obb.transform.localScale.z);
                    continue;
                }
                resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y/f, resizeFace[i].transform.localScale.z);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
            for(int i=0; i<6; i++)
            {
                GameObject resizeBar = RotateHandler.transform.GetChild(i).gameObject;
                if(resizeBar.name == "HandleY")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x, resizeBar.transform.localScale.y/f, resizeBar.transform.localScale.z);
                else if(resizeBar.name == "HandleX")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x/f, resizeBar.transform.localScale.y, resizeBar.transform.localScale.z);
                else if(resizeBar.name == "HandleZ")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x, resizeBar.transform.localScale.y, resizeBar.transform.localScale.z/f);
            }
        }
        else if(ch == 'Z')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                if(mode == 1)
                {
                    objectToHL.transform.localScale = new Vector3(objectToHL.transform.localScale.x, objectToHL.transform.localScale.y, objectToHL.transform.localScale.z/f);
                    var obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                    obb.transform.localScale = new Vector3(obb.transform.localScale.x, obb.transform.localScale.y, obb.transform.localScale.z/f);
                    continue;
                }
                resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z/f);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                if(resizeEdge[i].name == "SolidEdge 1" || resizeEdge[i].name == "SolidEdge 3" || resizeEdge[i].name == "SolidEdge 9" || resizeEdge[i].name == "SolidEdge 11")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z);
                else if(resizeEdge[i].name == "SolidEdge 4" || resizeEdge[i].name == "SolidEdge 5" || resizeEdge[i].name == "SolidEdge 6" || resizeEdge[i].name == "SolidEdge 7")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                if(resizeCorner[i].name == "Corner 1" ||　resizeCorner[i].name == "Corner 3" || resizeCorner[i].name == "Corner 4" || resizeCorner[i].name == "Corner 6")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x, resizeCorner[i].transform.localScale.y, resizeCorner[i].transform.localScale.z/f);
                else if(resizeCorner[i].name == "Corner 0" ||　resizeCorner[i].name == "Corner 2" || resizeCorner[i].name == "Corner 5" || resizeCorner[i].name == "Corner 7")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y, resizeCorner[i].transform.localScale.z);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
            for(int i=0; i<6; i++)
            {
                GameObject resizeBar = RotateHandler.transform.GetChild(i).gameObject;
                if(resizeBar.name == "HandleY")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x, resizeBar.transform.localScale.y, resizeBar.transform.localScale.z/f);
                else if(resizeBar.name == "HandleX")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x, resizeBar.transform.localScale.y, resizeBar.transform.localScale.z/f);
                else if(resizeBar.name == "HandleZ")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x, resizeBar.transform.localScale.y/f, resizeBar.transform.localScale.z);
            }
        }
        else if(ch == 'I')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                if(resizeFace[i].name == "Face 1" || resizeFace[i].name == "Face 3")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x/f, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z);
                else if(resizeFace[i].name == "Face 0" || resizeFace[i].name == "Face 5")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y/f, resizeFace[i].transform.localScale.z);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                if(mode == 1 && resizeEdge[i].name == oppsiteObject.name)
                {
                    var obb = objectToHL;
                    if(name == "SolidEdge 0" || name == "SolidEdge 2" || name == "SolidEdge 8" || name == "SolidEdge 10")
                    {
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y, obb.transform.localScale.z/f);
                        obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y, obb.transform.localScale.z/f);
                    }
                    else if(name == "SolidEdge 1" || name == "SolidEdge 3" || name == "SolidEdge 9" || name == "SolidEdge 11")
                    {
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x, obb.transform.localScale.y, obb.transform.localScale.z/f);
                        obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x, obb.transform.localScale.y, obb.transform.localScale.z/f);
                    }
                    else if(name == "SolidEdge 4" || name == "SolidEdge 5" || name == "SolidEdge 6" || name == "SolidEdge 7")
                    {
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y/f, obb.transform.localScale.z);
                        obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y/f, obb.transform.localScale.z);
                    }
                    continue;
                }
                if(resizeEdge[i].name == "SolidEdge 0" || resizeEdge[i].name == "SolidEdge 2" || resizeEdge[i].name == "SolidEdge 8" || resizeEdge[i].name == "SolidEdge 10")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                else if(resizeEdge[i].name == "SolidEdge 1" || resizeEdge[i].name == "SolidEdge 3" || resizeEdge[i].name == "SolidEdge 9" || resizeEdge[i].name == "SolidEdge 11")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                else if(resizeEdge[i].name == "SolidEdge 4" || resizeEdge[i].name == "SolidEdge 5" || resizeEdge[i].name == "SolidEdge 6" || resizeEdge[i].name == "SolidEdge 7")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                if(resizeCorner[i].name == "Corner 1" ||　resizeCorner[i].name == "Corner 3" || resizeCorner[i].name == "Corner 4" || resizeCorner[i].name == "Corner 6")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z);
                else if(resizeCorner[i].name == "Corner 0" ||　resizeCorner[i].name == "Corner 2" || resizeCorner[i].name == "Corner 5" || resizeCorner[i].name == "Corner 7")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z/f);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
            for(int i=0; i<6; i++)
            {
                GameObject resizeBar = RotateHandler.transform.GetChild(i).gameObject;
                if(resizeBar.name == "HandleY")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x/f, resizeBar.transform.localScale.y/f, resizeBar.transform.localScale.z);
                else if(resizeBar.name == "HandleX")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x/f, resizeBar.transform.localScale.y/f, resizeBar.transform.localScale.z);
                else if(resizeBar.name == "HandleZ")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x/f, resizeBar.transform.localScale.y, resizeBar.transform.localScale.z/f);
            }
        }
        else if(ch == 'J')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                if(resizeFace[i].name == "Face 2" || resizeFace[i].name == "Face 4")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z/f);
                else if(resizeFace[i].name == "Face 0" || resizeFace[i].name == "Face 5")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y/f, resizeFace[i].transform.localScale.z);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                if(mode == 1 && resizeEdge[i].name == oppsiteObject.name)
                {
                    var obb = objectToHL;
                    if(name == "SolidEdge 0" || name == "SolidEdge 2" || name == "SolidEdge 8" || name == "SolidEdge 10")
                    {
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x, obb.transform.localScale.y, obb.transform.localScale.z/f);
                        obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x, obb.transform.localScale.y, obb.transform.localScale.z/f);
                    }
                    else if(name == "SolidEdge 1" || name == "SolidEdge 3" || name == "SolidEdge 9" || name == "SolidEdge 11")
                    {
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y, obb.transform.localScale.z/f);
                        obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y, obb.transform.localScale.z/f);
                    }
                    else if(name == "SolidEdge 4" || name == "SolidEdge 5" || name == "SolidEdge 6" || name == "SolidEdge 7")
                    {
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x, obb.transform.localScale.y, obb.transform.localScale.z/f);
                        obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x, obb.transform.localScale.y, obb.transform.localScale.z/f);
                    }
                    continue;
                }
                if(resizeEdge[i].name == "SolidEdge 0" || resizeEdge[i].name == "SolidEdge 2" || resizeEdge[i].name == "SolidEdge 8" || resizeEdge[i].name == "SolidEdge 10")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                else if(resizeEdge[i].name == "SolidEdge 1" || resizeEdge[i].name == "SolidEdge 3" || resizeEdge[i].name == "SolidEdge 9" || resizeEdge[i].name == "SolidEdge 11")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                else if(resizeEdge[i].name == "SolidEdge 4" || resizeEdge[i].name == "SolidEdge 5" || resizeEdge[i].name == "SolidEdge 6" || resizeEdge[i].name == "SolidEdge 7")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                if(resizeCorner[i].name == "Corner 1" ||　resizeCorner[i].name == "Corner 3" || resizeCorner[i].name == "Corner 4" || resizeCorner[i].name == "Corner 6")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z/f);
                else if(resizeCorner[i].name == "Corner 0" ||　resizeCorner[i].name == "Corner 2" || resizeCorner[i].name == "Corner 5" || resizeCorner[i].name == "Corner 7")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
            for(int i=0; i<6; i++)
            {
                GameObject resizeBar = RotateHandler.transform.GetChild(i).gameObject;
                if(resizeBar.name == "HandleY")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x, resizeBar.transform.localScale.y/f, resizeBar.transform.localScale.z/f);
                else if(resizeBar.name == "HandleX")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x/f, resizeBar.transform.localScale.y, resizeBar.transform.localScale.z/f);
                else if(resizeBar.name == "HandleZ")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x, resizeBar.transform.localScale.y/f, resizeBar.transform.localScale.z/f);
            }
        }
        else if(ch == 'K')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                if(resizeFace[i].name == "Face 2" || resizeFace[i].name == "Face 4")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z/f);
                else if(resizeFace[i].name == "Face 1" || resizeFace[i].name == "Face 3")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x/f, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                if(mode == 1 && resizeEdge[i].name == oppsiteObject.name)
                {
                    var obb = objectToHL;
                    if(name == "SolidEdge 0" || name == "SolidEdge 2" || name == "SolidEdge 8" || name == "SolidEdge 10")
                    {
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y, obb.transform.localScale.z);
                        obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y, obb.transform.localScale.z);
                    }
                    else if(name == "SolidEdge 1" || name == "SolidEdge 3" || name == "SolidEdge 9" || name == "SolidEdge 11")
                    {
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y, obb.transform.localScale.z);
                        obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y, obb.transform.localScale.z);
                    }
                    else if(name == "SolidEdge 4" || name == "SolidEdge 5" || name == "SolidEdge 6" || name == "SolidEdge 7")
                    {
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y, obb.transform.localScale.z/f);
                        obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y, obb.transform.localScale.z/f);
                    }
                    continue;
                }
                if(resizeEdge[i].name == "SolidEdge 0" || resizeEdge[i].name == "SolidEdge 2" || resizeEdge[i].name == "SolidEdge 8" || resizeEdge[i].name == "SolidEdge 10")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z);
                else if(resizeEdge[i].name == "SolidEdge 1" || resizeEdge[i].name == "SolidEdge 3" || resizeEdge[i].name == "SolidEdge 9" || resizeEdge[i].name == "SolidEdge 11")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z);
                else if(resizeEdge[i].name == "SolidEdge 4" || resizeEdge[i].name == "SolidEdge 5" || resizeEdge[i].name == "SolidEdge 6" || resizeEdge[i].name == "SolidEdge 7")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                if(resizeCorner[i].name == "Corner 1" ||　resizeCorner[i].name == "Corner 3" || resizeCorner[i].name == "Corner 4" || resizeCorner[i].name == "Corner 6")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y, resizeCorner[i].transform.localScale.z/f);
                else if(resizeCorner[i].name == "Corner 0" ||　resizeCorner[i].name == "Corner 2" || resizeCorner[i].name == "Corner 5" || resizeCorner[i].name == "Corner 7")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y, resizeCorner[i].transform.localScale.z/f);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
            for(int i=0; i<6; i++)
            {
                GameObject resizeBar = RotateHandler.transform.GetChild(i).gameObject;
                if(resizeBar.name == "HandleY")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x/f, resizeBar.transform.localScale.y, resizeBar.transform.localScale.z/f);
                else if(resizeBar.name == "HandleX")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x, resizeBar.transform.localScale.y/f, resizeBar.transform.localScale.z/f);
                else if(resizeBar.name == "HandleZ")
                    resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x/f, resizeBar.transform.localScale.y/f, resizeBar.transform.localScale.z);
            }
        }
        else if(ch == 'L')
        {
            for(int i=0; i<resizeFace.Length; i++)
            {
                if(resizeFace[i].name == "Face 2" || resizeFace[i].name == "Face 4")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z/f);
                else if(resizeFace[i].name == "Face 1" || resizeFace[i].name == "Face 3")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x/f, resizeFace[i].transform.localScale.y, resizeFace[i].transform.localScale.z);
                else if(resizeFace[i].name == "Face 0" || resizeFace[i].name == "Face 5")
                    resizeFace[i].transform.localScale = new Vector3(resizeFace[i].transform.localScale.x, resizeFace[i].transform.localScale.y/f, resizeFace[i].transform.localScale.z);
                var ob = resizeFace[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeFace[i].transform.localScale;
            }
            for(int i=0; i<resizeEdge.Length; i++)
            {
                if(resizeEdge[i].name == "SolidEdge 0" || resizeEdge[i].name == "SolidEdge 2" || resizeEdge[i].name == "SolidEdge 8" || resizeEdge[i].name == "SolidEdge 10")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                else if(resizeEdge[i].name == "SolidEdge 1" || resizeEdge[i].name == "SolidEdge 3" || resizeEdge[i].name == "SolidEdge 9" || resizeEdge[i].name == "SolidEdge 11")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                else if(resizeEdge[i].name == "SolidEdge 4" || resizeEdge[i].name == "SolidEdge 5" || resizeEdge[i].name == "SolidEdge 6" || resizeEdge[i].name == "SolidEdge 7")
                    resizeEdge[i].transform.localScale = new Vector3(resizeEdge[i].transform.localScale.x/f, resizeEdge[i].transform.localScale.y, resizeEdge[i].transform.localScale.z/f);
                var ob = resizeEdge[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeEdge[i].transform.localScale;
            }
            for(int i=0; i<resizeCorner.Length; i++)
            {
                if(mode == 1 && resizeCorner[i].name == oppsiteObject.name)
                {
                    var obb = objectToHL;
                    if(name == "Corner 1" || name == "Corner 3" || name == "Corner 4" || name == "Corner 6")
                    {
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y/f, obb.transform.localScale.z/f);
                        obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y/f, obb.transform.localScale.z/f);
                    }
                    else if(name == "Corner 0" || name == "Corner 2" || name == "Corner 5" || name == "Corner 7")
                    {
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y/f, obb.transform.localScale.z/f);
                        obb = oppsiteObject.GetComponent<DoManipulation>().objectToHL;
                        obb.transform.localScale = new Vector3(obb.transform.localScale.x/f, obb.transform.localScale.y/f, obb.transform.localScale.z/f);
                    }
                    continue;
                }
                if(resizeCorner[i].name == "Corner 1" ||　resizeCorner[i].name == "Corner 3" || resizeCorner[i].name == "Corner 4" || resizeCorner[i].name == "Corner 6")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z/f);
                else if(resizeCorner[i].name == "Corner 0" ||　resizeCorner[i].name == "Corner 2" || resizeCorner[i].name == "Corner 5" || resizeCorner[i].name == "Corner 7")
                    resizeCorner[i].transform.localScale = new Vector3(resizeCorner[i].transform.localScale.x/f, resizeCorner[i].transform.localScale.y/f, resizeCorner[i].transform.localScale.z/f);
                var ob = resizeCorner[i].transform.GetComponent<DoManipulation>().objectToHL;
                ob.transform.localScale = resizeCorner[i].transform.localScale;
            }
            for(int i=0; i<6; i++)
            {
                GameObject resizeBar = RotateHandler.transform.GetChild(i).gameObject;
                resizeBar.transform.localScale = new Vector3(resizeBar.transform.localScale.x/f, resizeBar.transform.localScale.y/f, resizeBar.transform.localScale.z/f);
            }
        }
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
    private void CalVelocity()
    {
        // v = distance(m) / timestep(sec)
        // default timestep = 0.02 sec
        velocity = (transform.position - velOriPosition).magnitude / 0.02f;
        velOriPosition = transform.position;
    }
    private void DoModeTwoScaling()
    {
        transform.rotation = originalRotation;
        Vector3 newPos = Vector3.zero; // this object's new position
        Vector3 newOppoPos = Vector3.zero; // opposite's new position
        Vector3 newCenter = Vector3.zero; // new cneter of the two target
        Vector3 diff = Vector3.zero; // Bounding box movement;
        float newLength = 0.0f;
        Vector3 factor = Vector3.zero; // scaling factor
        reCh = ' ';
        reF = 0.0f;
        if(manipulationMode == 7) // SX
        {
            newPos = Vector3.Project(transform.position - BoundingBox.transform.position, BoundingBox.transform.right) + BoundingBox.transform.position;
            newOppoPos = Vector3.Project(oppsiteObject.transform.position - BoundingBox.transform.position, BoundingBox.transform.right) + BoundingBox.transform.position;
            newLength = Mathf.Abs((newPos - newOppoPos).magnitude);
            factor =  new Vector3(newLength / oriLength, 1, 1);
            newCenter = (newPos + newOppoPos) / 2f;
            reCh = 'X';
            reF = factor.x;
        }
        else if(manipulationMode == 8) // SY
        {
            newPos = Vector3.Project(transform.position - BoundingBox.transform.position, BoundingBox.transform.up) + BoundingBox.transform.position;
            newOppoPos = Vector3.Project(oppsiteObject.transform.position - BoundingBox.transform.position, BoundingBox.transform.up) + BoundingBox.transform.position;
            newLength = Mathf.Abs((newPos - newOppoPos).magnitude);
            factor =  new Vector3(1, newLength / oriLength, 1);
            newCenter = (newPos + newOppoPos) / 2f;
            reCh = 'Y';
            reF = factor.y;
        }
        else if(manipulationMode == 9) // SZ
        {
            newPos = Vector3.Project(transform.position - BoundingBox.transform.position, BoundingBox.transform.forward) + BoundingBox.transform.position;
            newOppoPos = Vector3.Project(oppsiteObject.transform.position - BoundingBox.transform.position, BoundingBox.transform.forward) + BoundingBox.transform.position;
            newLength = Mathf.Abs((newPos - newOppoPos).magnitude);
            factor =  new Vector3(1, 1, newLength / oriLength);
            newCenter = (newPos + newOppoPos) / 2f;
            reCh = 'Z';
            reF = factor.z;
        }
        else if(manipulationMode == 13) // SXY
        {
            newPos = Vector3.Project(transform.position - BoundingBox.transform.position, originalPosition - BoundingBox.transform.position) + BoundingBox.transform.position;
            newOppoPos = Vector3.Project(oppsiteObject.transform.position - BoundingBox.transform.position, originalPosition - BoundingBox.transform.position) + BoundingBox.transform.position;
            newLength = Mathf.Abs((newPos - newOppoPos).magnitude);
            factor =  new Vector3(newLength / oriLength, newLength / oriLength, 1);
            newCenter = (newPos + newOppoPos) / 2f;
            reCh = 'I';
            reF = factor.x;
        }
        else if(manipulationMode == 14) // SYZ
        {
            newPos = Vector3.Project(transform.position - BoundingBox.transform.position, originalPosition - BoundingBox.transform.position) + BoundingBox.transform.position;
            newOppoPos = Vector3.Project(oppsiteObject.transform.position - BoundingBox.transform.position, originalPosition - BoundingBox.transform.position) + BoundingBox.transform.position;
            newLength = Mathf.Abs((newPos - newOppoPos).magnitude);
            factor =  new Vector3(1, newLength / oriLength, newLength / oriLength);
            newCenter = (newPos + newOppoPos) / 2f;
            reCh = 'J';
            reF = factor.y;
        }
        else if(manipulationMode == 15) // SXZ
        {
            newPos = Vector3.Project(transform.position - BoundingBox.transform.position, originalPosition - BoundingBox.transform.position) + BoundingBox.transform.position;
            newOppoPos = Vector3.Project(oppsiteObject.transform.position - BoundingBox.transform.position, originalPosition - BoundingBox.transform.position) + BoundingBox.transform.position;
            newLength = Mathf.Abs((newPos - newOppoPos).magnitude);
            factor =  new Vector3(newLength / oriLength, 1, newLength / oriLength);
            newCenter = (newPos + newOppoPos) / 2f;
            reCh = 'K';
            reF = factor.x;
        }
        else if(manipulationMode == 17) // SXYZ
        {
            newPos = Vector3.Project(transform.position - BoundingBox.transform.position, originalPosition - BoundingBox.transform.position) + BoundingBox.transform.position;
            newOppoPos = Vector3.Project(oppsiteObject.transform.position - BoundingBox.transform.position, originalPosition - BoundingBox.transform.position) + BoundingBox.transform.position;
            newLength = Mathf.Abs((newPos - newOppoPos).magnitude);
            factor =  new Vector3(newLength / oriLength, newLength / oriLength, newLength / oriLength);
            newCenter = (newPos + newOppoPos) / 2f;
            reCh = 'L';
            reF = factor.x;
        }
        if(factor.x != Mathf.Infinity && factor.x != -Mathf.Infinity && factor.x != 0 && factor.y != Mathf.Infinity && factor.y != -Mathf.Infinity && factor.y != 0 && factor.z != Mathf.Infinity && factor.z != -Mathf.Infinity && factor.z != 0)
        {
            
            if(name == "Face 0" || name == "Face 1" || name == "Face 2" || name == "SolidEdge 0" || name == "SolidEdge 1" || name == "SolidEdge 2" || name == "SolidEdge 3"|| name == "SolidEdge 4" || name == "SolidEdge 5" || name == "Corner 0" || name == "Corner 1" || name == "Corner 2" || name == "Corner 3")
            {
                SelectedObject.transform.Translate((newCenter - BoundingBox.transform.position)/ScaleCoefficient, Space.World);
                BoundingBox.transform.position = newCenter;
                BoundingBox.transform.localScale = Vector3.Scale(BoundingBox.transform.localScale, factor);
                SelectedObject.transform.localScale = Vector3.Scale(SelectedObject.transform.localScale, factor);
                resize(reCh, reF);
            }
            // else if(velocity > 0.1f )
            // {
            //     SelectedObject.transform.Translate((newCenter - BoundingBox.transform.position)/ScaleCoefficient, Space.World);
            //     BoundingBox.transform.position = newCenter;
            //     BoundingBox.transform.localScale = Vector3.Scale(BoundingBox.transform.localScale, factor);
            //     SelectedObject.transform.localScale = Vector3.Scale(SelectedObject.transform.localScale, factor);
            //     resize(reCh, reF);
            // }
            transform.position = objectToHL.transform.position;
            originalPosition = transform.position;
            oriLength = newLength;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(isTranslation)
        {
            DoTranslation();
        }
        else if(isRotation)
        {
            DoRotation();
        }
        else if(isScaling)
        {
            if(mode == 0)
            {
                // acnhored scaling
                if(sclaingMode == 1 && changeParent)
                {
                    empty.transform.position = oppsiteObject.transform.position;
                    empty.transform.localScale = new Vector3(1,1,1);
                    empty.transform.rotation = BoundingBox.transform.rotation;
                    BoundingBox.transform.parent = empty.transform;
                    empty1.transform.position = SelectedObject.transform.position + (oppsiteObject.transform.position - BoundingBox.transform.position) / ScaleCoefficient;
                    empty1.transform.localScale = new Vector3(1,1,1);
                    empty1.transform.rotation = SelectedObject.transform.rotation;
                    SelectedObject.transform.parent = empty1.transform;
                    changeParent = false;
                }
                // uniform scaling
                else if(sclaingMode == 0)
                {
                    if(originalParent != null)
                        SelectedObject.transform.parent = originalParent.transform;
                    else
                        SelectedObject.transform.parent = null;
                    BoundingBox.transform.parent = null;
                    changeParent = true;
                }
                DoScaling();
                // resize FaceHL, SolidEdgeHL, CornerHL
                if(tag == "Face")
                    objectToHL.transform.localScale = oppsiteObject.GetComponent<DoManipulation>().objectToHL.transform.localScale;
                else if(tag == "Edge")
                    objectToHL.transform.localScale = new Vector3(oppsiteObject.transform.localScale.x*2f, oppsiteObject.transform.localScale.y, oppsiteObject.transform.localScale.z*2f);
                else if(tag == "Corner")
                    objectToHL.transform.localScale = resizeCorner[0].GetComponent<DoManipulation>().objectToHL.transform.localScale;
            }
            else
            {
                // do mode two scaling
                DoModeTwoScaling();
            }
        }
        
        if(mode == 0) // gesture mode
        {
            // set LeftHandUp value, if user height = 1.7m
            if(leftHand.transform.position.y >= 1.2f)
            {    
                LeftHandUp = true;
            }
            else
            {
                LeftHandUp = false;
            }
            // set scaling Mode, 0:uniform, 1:anchored
            if(Mathf.Abs((leftHand.transform.position - leftHandOriPos).magnitude) < 0.002)
            {
                sclaingMode = 1;
            }
            else
            {
                sclaingMode = 0;
            }
            leftHandOriPos = leftHand.transform.position;
            // when rotating, other target disable
            if( RotateHandler.transform.GetChild(0).GetChild(0).GetComponent<RotateHandle>().getUsingState() || 
                RotateHandler.transform.GetChild(1).GetChild(0).GetComponent<RotateHandle>().getUsingState() || 
                RotateHandler.transform.GetChild(2).GetChild(0).GetComponent<RotateHandle>().getUsingState() ||
                RotateHandler.transform.GetChild(3).GetChild(0).GetComponent<RotateHandle>().getUsingState() || 
                RotateHandler.transform.GetChild(4).GetChild(0).GetComponent<RotateHandle>().getUsingState() || 
                RotateHandler.transform.GetChild(5).GetChild(0).GetComponent<RotateHandle>().getUsingState())
            {
                if(tag == "Face")
                {
                    GetComponent<BoxCollider>().enabled = false;
                    objectToHL.GetComponent<MeshRenderer>().enabled = false;
                }
                else if(tag == "Edge")
                    GetComponent<CapsuleCollider>().enabled = false;
                else if(tag == "Corner")
                {
                    GetComponent<SphereCollider>().enabled = false;
                    objectToHL.GetComponent<MeshRenderer>().enabled = false;
                }
                GameObject.FindGameObjectWithTag("CopyOfObject").GetComponent<BoxCollider>().enabled = false;
            }
            else
            {
                if(tag == "Face")
                {
                    GetComponent<BoxCollider>().enabled = true;
                    objectToHL.GetComponent<MeshRenderer>().enabled = true;
                }
                else if(tag == "Edge")
                    GetComponent<CapsuleCollider>().enabled = true;
                if(LeftHandUp)
                {
                    // when left hand up, corner enable, CopyOfObject disable
                    if(tag == "Corner")
                    {
                        GetComponent<SphereCollider>().enabled = true;
                        objectToHL.GetComponent<MeshRenderer>().enabled = true;
                    }
                    GameObject.FindGameObjectWithTag("CopyOfObject").GetComponent<BoxCollider>().enabled = false;
                }
                else
                {
                    // when left hand down, corner disable, CopyOfObject enable
                    if(tag == "Corner")
                    {
                        GetComponent<SphereCollider>().enabled = false;
                        objectToHL.GetComponent<MeshRenderer>().enabled = false;
                    }
                    GameObject.FindGameObjectWithTag("CopyOfObject").GetComponent<BoxCollider>().enabled = true;
                }
            }
            // only right controller can interact with the bounding box
            GetComponent<VRTK_InteractableObject>().allowedNearTouchControllers = VRTK_InteractableObject.AllowedController.RightOnly;
            GetComponent<VRTK_InteractableObject>().allowedTouchControllers = VRTK_InteractableObject.AllowedController.RightOnly;
            GetComponent<VRTK_InteractableObject>().allowedGrabControllers = VRTK_InteractableObject.AllowedController.RightOnly;
        }
        else if(mode == 1) // non-gesture mode
        {
            FlashingObject();
            if(isPressing && !oppsiteObject.GetComponent<DoManipulation>().isPressing) // do translation
            {
                isTranslation = true;
                isScaling = false;
                if(this.gameObject.tag == "Face") // 1-dim
                {
                    // change highlight color
                    objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, grabAlpha);
                    effect.ProfileLoad(Resources.Load<HighlightProfile>("FaceGrab"));
                    int index = 5; // number index
                    if(this.gameObject.name[index] == '1' || this.gameObject.name[index] == '3')
                    { // X-axis
                        manipulationMode = 1;
                    }
                    else if(this.gameObject.name[index] == '0' || this.gameObject.name[index] == '5')
                    { // Y-axis
                        manipulationMode = 2;
                    }
                    else if(this.gameObject.name[index] == '2' || this.gameObject.name[index] == '4')
                    { // Z-axis
                        manipulationMode = 3;
                    }
                }
                else if(this.gameObject.tag == "Edge") // 2-dim
                {
                    objectToHL.GetComponent<MeshRenderer>().material.color = new Color(0.9716981f, 0.7230207f, 0.3345942f, grabAlpha);
                    effect.ProfileLoad(Resources.Load<HighlightProfile>("EdgeGrab"));
                    int index = 10; // number index
                    if(this.gameObject.name[index] == '0' || this.gameObject.name[index] == '2' || this.gameObject.name[index] == '8' || this.gameObject.name.Substring(index) == "10")
                    { // X-Y plane
                        manipulationMode = 10;
                    }
                    else if(this.gameObject.name[index] == '1' || this.gameObject.name[index] == '3' || this.gameObject.name[index] == '9' || this.gameObject.name.Substring(index) == "11")
                    { // Y-Z plane
                        manipulationMode = 11;
                    }
                    else if(this.gameObject.name[index] == '4' || this.gameObject.name[index] == '5' || this.gameObject.name[index] == '6' || this.gameObject.name[index] == '7')
                    { // X-Z plane
                        manipulationMode = 12;
                    }
                }
                else if(this.gameObject.tag == "Corner")
                {
                    objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, grabAlpha);
                    effect.ProfileLoad(Resources.Load<HighlightProfile>("CornerGrab"));
                }
                else if(this.gameObject.tag == "CopyOfObject") // 3-dim
                {
                    manipulationMode = 16;
                }
            }
            else if(isPressing && oppsiteObject.GetComponent<DoManipulation>().isPressing) // do scaling
            {
                isScaling = true;
                isTranslation = false;
                if(this.gameObject.tag == "Face") // 1-dim
                {
                    // change highlight color
                    objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, grabAlpha);
                    effect.ProfileLoad(Resources.Load<HighlightProfile>("FaceGrab"));
                    int index = 5; // number index
                    if(this.gameObject.name[index] == '1' || this.gameObject.name[index] == '3')
                    { // X-axis
                        manipulationMode = 7;
                    }
                    else if(this.gameObject.name[index] == '0' || this.gameObject.name[index] == '5')
                    { // Y-axis
                        manipulationMode = 8;
                    }
                    else if(this.gameObject.name[index] == '2' || this.gameObject.name[index] == '4')
                    { // Z-axis
                        manipulationMode = 9;
                    }
                }
                else if(this.gameObject.tag == "Edge")
                {
                    objectToHL.GetComponent<MeshRenderer>().material.color = new Color(0.9716981f, 0.7230207f, 0.3345942f, grabAlpha);
                    effect.ProfileLoad(Resources.Load<HighlightProfile>("EdgeGrab"));
                    int index = 10; // number index
                    if(this.gameObject.name[index] == '0' || this.gameObject.name[index] == '2' || this.gameObject.name[index] == '8' || this.gameObject.name.Substring(index) == "10")
                    { // X-Y plane
                        manipulationMode = 13;
                    }
                    else if(this.gameObject.name[index] == '1' || this.gameObject.name[index] == '3' || this.gameObject.name[index] == '9' || this.gameObject.name.Substring(index) == "11")
                    { // Y-Z plane
                        manipulationMode = 14;
                    }
                    else if(this.gameObject.name[index] == '4' || this.gameObject.name[index] == '5' || this.gameObject.name[index] == '6' || this.gameObject.name[index] == '7')
                    { // X-Z plane
                        manipulationMode = 15;
                    }
                }
                else if(this.gameObject.tag == "Corner")
                {
                    objectToHL.GetComponent<MeshRenderer>().material.color = new Color(oriColor.r, oriColor.g, oriColor.b, grabAlpha);
                    manipulationMode = 17;
                    effect.ProfileLoad(Resources.Load<HighlightProfile>("CornerGrab"));
                }
            }
            if(isPressing && !oppsiteObject.GetComponent<DoManipulation>().isPressing)
            {
                // set attachController
                attachController = transform.parent.parent.parent.parent.gameObject;
                // disable other target when pressing, except the opposite object
                for(int i=0; i<BoundingBox.transform.GetChild(0).childCount; i++)
                {
                    if(BoundingBox.transform.GetChild(0).GetChild(i).name == oppsiteObject.name)
                        continue;
                    BoundingBox.transform.GetChild(0).GetChild(i).GetComponent<BoxCollider>().enabled = false;
                    BoundingBox.transform.GetChild(0).GetChild(i).GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = false;
                }
                for(int i=0; i<BoundingBox.transform.GetChild(2).childCount; i++)
                {
                    if(BoundingBox.transform.GetChild(2).GetChild(i).name == oppsiteObject.name)
                        continue;
                    BoundingBox.transform.GetChild(2).GetChild(i).GetComponent<SphereCollider>().enabled = false;
                    BoundingBox.transform.GetChild(2).GetChild(i).GetComponent<DoManipulation>().objectToHL.GetComponent<MeshRenderer>().enabled = false;
                }
                for(int i=0; i<BoundingBox.transform.GetChild(4).childCount; i++)
                {
                    if(BoundingBox.transform.GetChild(4).GetChild(i).name == oppsiteObject.name)
                        continue;
                    BoundingBox.transform.GetChild(4).GetChild(i).GetComponent<CapsuleCollider>().enabled = false;
                }
                GameObject.FindGameObjectWithTag("CopyOfObject").GetComponent<BoxCollider>().enabled = false;
                if(attachController.name == "Controller (right)")
                {
                    GetComponent<VRTK_InteractableObject>().allowedNearTouchControllers = VRTK_InteractableObject.AllowedController.RightOnly;
                    GetComponent<VRTK_InteractableObject>().allowedTouchControllers = VRTK_InteractableObject.AllowedController.RightOnly;
                    GetComponent<VRTK_InteractableObject>().allowedGrabControllers = VRTK_InteractableObject.AllowedController.RightOnly;
                }
                else if(attachController.name == "Controller (left)")
                {
                    GetComponent<VRTK_InteractableObject>().allowedNearTouchControllers = VRTK_InteractableObject.AllowedController.LeftOnly;
                    GetComponent<VRTK_InteractableObject>().allowedTouchControllers = VRTK_InteractableObject.AllowedController.LeftOnly;
                    GetComponent<VRTK_InteractableObject>().allowedGrabControllers = VRTK_InteractableObject.AllowedController.LeftOnly;
                }
            }
            if(oppsiteObject.GetComponent<DoManipulation>().isPressing && !isPressing)
            {
                if(oppsiteObject.GetComponent<DoManipulation>().attachController != null)
                {
                    if(oppsiteObject.GetComponent<DoManipulation>().attachController.name == "Controller (right)")
                    {
                        GetComponent<VRTK_InteractableObject>().allowedNearTouchControllers = VRTK_InteractableObject.AllowedController.LeftOnly;
                    }
                    else if(oppsiteObject.GetComponent<DoManipulation>().attachController.name == "Controller (left)")
                    {
                        GetComponent<VRTK_InteractableObject>().allowedNearTouchControllers = VRTK_InteractableObject.AllowedController.RightOnly;
                    }
                }
            }
            // adjust primitive collider size
            if(tag == "Face")
            {
                var min = Mathf.Min(BoundingBox.transform.localScale.x, BoundingBox.transform.localScale.y, BoundingBox.transform.localScale.z);
                if(name == "Face 0" || name == "Face 5")
                    GetComponent<BoxCollider>().size = new Vector3(0.8f, oriCollSize * min/oriBoundMinSize, 0.8f);
                else if(name == "Face 1" || name == "Face 3")
                    GetComponent<BoxCollider>().size = new Vector3(oriCollSize * min/oriBoundMinSize, 0.8f, 0.8f);
                else if(name == "Face 2" || name == "Face 4")
                    GetComponent<BoxCollider>().size = new Vector3(0.8f, 0.8f, oriCollSize * min/oriBoundMinSize);
            }
            else if(tag == "Corner")
            {
                var min = Mathf.Min(BoundingBox.transform.localScale.x, BoundingBox.transform.localScale.y, BoundingBox.transform.localScale.z);
                GetComponent<SphereCollider>().radius = oriCollSize * (min / oriBoundMinSize);
            }
            else if(tag == "Edge")
            {
                var min = Mathf.Min(BoundingBox.transform.localScale.x, BoundingBox.transform.localScale.y, BoundingBox.transform.localScale.z);
                GetComponent<CapsuleCollider>().radius = oriCollSize * (min / oriBoundMinSize);
            }
        }
        
        // if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 0)
        // {
        //     mode = 0;
        // }
        // else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7)
        // {
        //     mode = 1;
        // }
        // else
        // {
        //     mode = -1;
        //     GetComponent<Collider>().enabled = false;
        // }
        
        
    }
}
