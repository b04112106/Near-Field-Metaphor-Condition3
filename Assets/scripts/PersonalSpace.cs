using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using HighlightPlus;
using UnityEngine.SceneManagement;

public class PersonalSpace : MonoBehaviour
{
    public Camera Cam;
    public GameObject BoundingBox;
    private GameObject CopyOfObject;
    private AudioSource audioSource;
    private float UserHeight = 1.75f; // assume user 175cm
    private float UserHandSize = 0.0f;
    public float ScaleCoefficient = 0.0f;
    private List<GameObject> context;
    private List<GameObject> contextInPersonal;
    public Material [] original;
    private HighlightEffect effect;
    private HighlightProfile objectSelect;
    private GameObject rightHand, leftHand;
    private GameObject bound, GM;
    
    

    // Start is called before the first frame update
    void Start()
    {
        context = new List<GameObject>();
        contextInPersonal = new List<GameObject>();
        effect = GetComponent<HighlightEffect>();
        effect.ProfileLoad(Resources.Load<HighlightProfile>("ObjectTouch"));
        objectSelect = Resources.Load<HighlightProfile>("ObjectSelect");
        rightHand = GameObject.Find("RightControllerScriptAlias");
        leftHand = GameObject.Find("LeftControllerScriptAlias");
        UserHandSize = UserHeight * 0.1f;
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
        if(this.gameObject.GetComponent<VRTK_InteractableObject>().isUsable)
            effect.highlighted = true;
    }
    public void UnTouch()
    {
        if(this.gameObject.GetComponent<VRTK_InteractableObject>().isUsable)
            effect.highlighted = false;
    }
    public void PressTrigger()
    {
        if(GetComponent<VRTK_InteractableObject>().isUsable){
            // add tag
            tag = "SelectedObject";
            // turn off highlight and trigger HitFX effect
            effect.highlighted = false;
            effect.overlay = 0;
            effect.HitFX(Color.white, 0.15f, 0.8f);
            // create copy of the selected object in personal space
            CreateCopy();
            // create context
            //CreateContext();
            // create bounding box of the copy
            CreateBoundingBox();
            // load ObjectSelect profile
            effect.ProfileLoad(objectSelect);
            effect.highlighted = true;
            // rightHand.GetComponent<VRTK_Pointer>().activateOnEnable = false;
            rightHand.GetComponent<VRTK_Pointer>().enabled = false;
            audioSource.PlayOneShot(Resources.Load<AudioClip>("Select"));
            if(SceneManager.GetActiveScene().name == "Testing")
                GM.GetComponent<GameManager>().setStart();
        }
        // set isUsable to false, so object can not select again
        GetComponent<VRTK_InteractableObject>().isUsable = false;
    }
    private void CreateCopy()
    {
        var distance = 0.5f; //distance from user
        CopyOfObject = Instantiate(this.gameObject, Cam.transform.position + Cam.transform.forward * distance - new Vector3(0,0.1f,0), this.gameObject.transform.rotation);
        CopyOfObject.GetComponent<VRTK_InteractableObject>().isUsable = false;
        var maxValue = 0.0f; //maximum of object.size
        maxValue = Mathf.Max(CopyOfObject.GetComponent<MeshFilter>().mesh.bounds.size.x * CopyOfObject.transform.localScale.x,
                                CopyOfObject.GetComponent<MeshFilter>().mesh.bounds.size.y * CopyOfObject.transform.localScale.y, 
                                CopyOfObject.GetComponent<MeshFilter>().mesh.bounds.size.z * CopyOfObject.transform.localScale.z);
        maxValue = (CopyOfObject.GetComponent<MeshFilter>().mesh.bounds.size.x * CopyOfObject.transform.localScale.x+
                    CopyOfObject.GetComponent<MeshFilter>().mesh.bounds.size.y * CopyOfObject.transform.localScale.y+ 
                    CopyOfObject.GetComponent<MeshFilter>().mesh.bounds.size.z * CopyOfObject.transform.localScale.z) / 3f;
        ScaleCoefficient = UserHandSize / maxValue; //scale coefficient between personal and object space
        CopyOfObject.transform.localScale *= ScaleCoefficient;
        CopyOfObject.GetComponent<MeshRenderer>().material.color = new Color(CopyOfObject.GetComponent<MeshRenderer>().material.color.r, 
                                                                             CopyOfObject.GetComponent<MeshRenderer>().material.color.g, 
                                                                             CopyOfObject.GetComponent<MeshRenderer>().material.color.b, 
                                                                             0.8f);
        CopyOfObject.tag = "CopyOfObject";
        for(int i=0; i<original.Length; i++)
            CopyOfObject.GetComponent<MeshRenderer>().materials[i] = original[i];
        CopyOfObject.GetComponent<VRTK_InteractableObject>().isGrabbable = true;
        Destroy(CopyOfObject.GetComponent<PersonalSpace>());
        if(CopyOfObject.GetComponent<Rigidbody>())
            Destroy(CopyOfObject.GetComponent<Rigidbody>());
    }
    private void CreateContext()
    {   
        var ContextRange = 2.0f; // sphere radius
        Collider[] hitColliders = Physics.OverlapSphere(this.gameObject.transform.position, 0.5f * ContextRange);
        int i = 0;
        while (i < hitColliders.Length)
        {
            if(hitColliders[i].name == this.gameObject.name || hitColliders[i].name == "Floor"){
                i++;
                continue;
            }
            var dis = hitColliders[i].transform.position - this.gameObject.transform.position; // distace of context object and selected object in object space
            var obj = Instantiate(hitColliders[i].gameObject, CopyOfObject.transform.position + dis * ScaleCoefficient, hitColliders[i].transform.rotation);
            //obj.transform.Rotate(new Vector3(0, Cam.transform.eulerAngles.y, 0), Space.World);
            obj.transform.localScale = obj.transform.localScale * ScaleCoefficient;
            /*obj.GetComponent<MeshRenderer>().material.color = new Color(obj.GetComponent<MeshRenderer>().material.color.r, 
                                                                        obj.GetComponent<MeshRenderer>().material.color.g, 
                                                                        obj.GetComponent<MeshRenderer>().material.color.b,
                                                                        0.9f);*/
            //Debug.Log(hitColliders[i].name);
            context.Add(hitColliders[i].gameObject);
            // context.Add(obj);
            contextInPersonal.Add(obj);
            // if context has Collider conpoment, then disable
            if(obj.GetComponent<BoxCollider>())
                obj.GetComponent<BoxCollider>().enabled = false;
            i++;
        }
        if(GameObject.Find("[VRTK][AUTOGEN][RightControllerScriptAlias][StraightPointerRenderer_Cursor](Clone)"))
            GameObject.Find("[VRTK][AUTOGEN][RightControllerScriptAlias][StraightPointerRenderer_Cursor](Clone)").SetActive(false);
        if(GameObject.Find("[VRTK][AUTOGEN][RightControllerScriptAlias][StraightPointerRenderer_Tracer](Clone)"))
            GameObject.Find("[VRTK][AUTOGEN][RightControllerScriptAlias][StraightPointerRenderer_Tracer](Clone)").SetActive(false);
        if(GameObject.Find("[VRTK][AUTOGEN][RightControllerScriptAlias][BasePointerRenderer_ObjectInteractor_Collider](Clone)"))
            GameObject.Find("[VRTK][AUTOGEN][RightControllerScriptAlias][BasePointerRenderer_ObjectInteractor_Collider](Clone)").SetActive(false);
    }
    private void CreateBoundingBox()
    {
        bound = Instantiate(BoundingBox, CopyOfObject.transform.position, CopyOfObject.transform.rotation);
        bound.transform.localScale = Vector3.Scale(CopyOfObject.GetComponent<MeshFilter>().mesh.bounds.size, CopyOfObject.transform.localScale);
        // var tmpV = new Vector3(1f/transform.localScale.x, 1f/transform.localScale.y, 1f/transform.localScale.z);
        // bound.transform.Translate(Vector3.Scale(CopyOfObject.GetComponent<BoxCollider>().center, Vector3.Scale(CopyOfObject.transform.localScale, tmpV)));
        bound.transform.localScale *= 1.4f;
        CopyOfObject.transform.parent = bound.transform;
        Vector3 factor = bound.transform.localScale;
        float oriScale = UserHandSize; // bounding box original size
        // adjust face size
        bound.transform.GetChild(0).GetChild(0).localScale = new Vector3(bound.transform.GetChild(0).GetChild(0).localScale.x, bound.transform.GetChild(0).GetChild(0).localScale.y/factor.y*oriScale, bound.transform.GetChild(0).GetChild(0).localScale.z);
        bound.transform.GetChild(0).GetChild(5).localScale = bound.transform.GetChild(0).GetChild(0).localScale;
        bound.transform.GetChild(0).GetChild(1).localScale = new Vector3(bound.transform.GetChild(0).GetChild(1).localScale.x/factor.x*oriScale, bound.transform.GetChild(0).GetChild(1).localScale.y, bound.transform.GetChild(0).GetChild(1).localScale.z);
        bound.transform.GetChild(0).GetChild(3).localScale = bound.transform.GetChild(0).GetChild(1).localScale;
        bound.transform.GetChild(0).GetChild(2).localScale = new Vector3(bound.transform.GetChild(0).GetChild(2).localScale.x, bound.transform.GetChild(0).GetChild(2).localScale.y, bound.transform.GetChild(0).GetChild(2).localScale.z/factor.z*oriScale);
        bound.transform.GetChild(0).GetChild(4).localScale = bound.transform.GetChild(0).GetChild(2).localScale;
         
        // // adjust edge size
        // for(int i=0; i<4; i++)
        //     bound.transform.GetChild(1).GetChild(i).localScale = new Vector3(bound.transform.GetChild(1).GetChild(i).localScale.x, bound.transform.GetChild(1).GetChild(i).localScale.y, bound.transform.GetChild(1).GetChild(i).localScale.z * factor);
        // for(int i=8; i<12; i++)
        //     bound.transform.GetChild(1).GetChild(i).localScale = new Vector3(bound.transform.GetChild(1).GetChild(i).localScale.x, bound.transform.GetChild(1).GetChild(i).localScale.y, bound.transform.GetChild(1).GetChild(i).localScale.z * factor);
        
        // // adjust corner size
        for(int i=0; i<8; i++)
        {
            if(bound.transform.GetChild(2).GetChild(i).name == "Corner 1" || bound.transform.GetChild(2).GetChild(i).name == "Corner 3" || bound.transform.GetChild(2).GetChild(i).name == "Corner 4" || bound.transform.GetChild(2).GetChild(i).name == "Corner 6")
                bound.transform.GetChild(2).GetChild(i).localScale = new Vector3(bound.transform.GetChild(2).GetChild(i).localScale.x/factor.x, bound.transform.GetChild(2).GetChild(i).localScale.y/factor.y, bound.transform.GetChild(2).GetChild(i).localScale.z/factor.z);
            else if(bound.transform.GetChild(2).GetChild(i).name == "Corner 0" || bound.transform.GetChild(2).GetChild(i).name == "Corner 2" || bound.transform.GetChild(2).GetChild(i).name == "Corner 5" || bound.transform.GetChild(2).GetChild(i).name == "Corner 7")
                bound.transform.GetChild(2).GetChild(i).localScale = new Vector3(bound.transform.GetChild(2).GetChild(i).localScale.x/factor.z, bound.transform.GetChild(2).GetChild(i).localScale.y/factor.y, bound.transform.GetChild(2).GetChild(i).localScale.z/factor.x); 
            bound.transform.GetChild(2).GetChild(i).localScale *= oriScale;
        }
            
        // adjust rotate handle bar size
        for(int i=0; i<2; i++)
        {
            bound.transform.GetChild(3).GetChild(i).localScale = new Vector3(bound.transform.GetChild(3).GetChild(i).localScale.x/factor.x, bound.transform.GetChild(3).GetChild(i).localScale.y/factor.y, bound.transform.GetChild(3).GetChild(i).localScale.z/factor.z);
            bound.transform.GetChild(3).GetChild(i).localScale *= oriScale;
        }
        for(int i=2; i<4; i++)
        {
            bound.transform.GetChild(3).GetChild(i).localScale = new Vector3(bound.transform.GetChild(3).GetChild(i).localScale.x/factor.y, bound.transform.GetChild(3).GetChild(i).localScale.y/factor.x, bound.transform.GetChild(3).GetChild(i).localScale.z/factor.z);
            bound.transform.GetChild(3).GetChild(i).localScale *= oriScale;
        }
        for(int i=4; i<6; i++)
        {
            bound.transform.GetChild(3).GetChild(i).localScale = new Vector3(bound.transform.GetChild(3).GetChild(i).localScale.x/factor.x, bound.transform.GetChild(3).GetChild(i).localScale.y/factor.z, bound.transform.GetChild(3).GetChild(i).localScale.z/factor.y);
            bound.transform.GetChild(3).GetChild(i).localScale *= oriScale;
        }

        // // adjust solid edge size
        for(int i=0; i<12; i++)
        {
            if(bound.transform.GetChild(4).GetChild(i).name == "SolidEdge 0" || bound.transform.GetChild(4).GetChild(i).name == "SolidEdge 2" || bound.transform.GetChild(4).GetChild(i).name == "SolidEdge 8" || bound.transform.GetChild(4).GetChild(i).name == "SolidEdge 10")
                bound.transform.GetChild(4).GetChild(i).localScale = new Vector3(bound.transform.GetChild(4).GetChild(i).localScale.x/factor.x*oriScale, bound.transform.GetChild(4).GetChild(i).localScale.y, bound.transform.GetChild(4).GetChild(i).localScale.z/factor.y*oriScale);
            else if(bound.transform.GetChild(4).GetChild(i).name == "SolidEdge 1" || bound.transform.GetChild(4).GetChild(i).name == "SolidEdge 3" || bound.transform.GetChild(4).GetChild(i).name == "SolidEdge 9" || bound.transform.GetChild(4).GetChild(i).name == "SolidEdge 11")
                bound.transform.GetChild(4).GetChild(i).localScale = new Vector3(bound.transform.GetChild(4).GetChild(i).localScale.x/factor.z*oriScale, bound.transform.GetChild(4).GetChild(i).localScale.y, bound.transform.GetChild(4).GetChild(i).localScale.z/factor.y*oriScale);
            else if(bound.transform.GetChild(4).GetChild(i).name == "SolidEdge 4" || bound.transform.GetChild(4).GetChild(i).name == "SolidEdge 5" || bound.transform.GetChild(4).GetChild(i).name == "SolidEdge 6" || bound.transform.GetChild(4).GetChild(i).name == "SolidEdge 7")
                bound.transform.GetChild(4).GetChild(i).localScale = new Vector3(bound.transform.GetChild(4).GetChild(i).localScale.x/factor.x*oriScale, bound.transform.GetChild(4).GetChild(i).localScale.y, bound.transform.GetChild(4).GetChild(i).localScale.z/factor.z*oriScale);
            
        }

        // // adjust rotate container size
        for(int i=0; i<4; i++)
            bound.transform.GetChild(5).GetChild(0).GetChild(0).GetChild(i).localScale = new Vector3(bound.transform.GetChild(5).GetChild(0).GetChild(0).GetChild(i).localScale.x/factor.x*oriScale, bound.transform.GetChild(5).GetChild(0).GetChild(0).GetChild(i).localScale.y, bound.transform.GetChild(5).GetChild(0).GetChild(0).GetChild(i).localScale.z/factor.z*oriScale);
        for(int i=0; i<4; i++)
            bound.transform.GetChild(5).GetChild(1).GetChild(0).GetChild(i).localScale = new Vector3(bound.transform.GetChild(5).GetChild(1).GetChild(0).GetChild(i).localScale.x/factor.z*oriScale, bound.transform.GetChild(5).GetChild(1).GetChild(0).GetChild(i).localScale.y, bound.transform.GetChild(5).GetChild(1).GetChild(0).GetChild(i).localScale.z/factor.y*oriScale);
        for(int i=0; i<4; i++)
            bound.transform.GetChild(5).GetChild(2).GetChild(0).GetChild(i).localScale = new Vector3(bound.transform.GetChild(5).GetChild(2).GetChild(0).GetChild(i).localScale.x/factor.x*oriScale, bound.transform.GetChild(5).GetChild(2).GetChild(0).GetChild(i).localScale.y, bound.transform.GetChild(5).GetChild(2).GetChild(0).GetChild(i).localScale.z/factor.y*oriScale);
        
        // adjust faceHL size
        for(int i=0; i<6; i++)
        {
            bound.transform.GetChild(6).GetChild(i).localScale = bound.transform.GetChild(0).GetChild(i).localScale;
        }

        // // adjust SolidEdgeHL size
        for(int i=0; i<12; i++)
        {
            bound.transform.GetChild(7).GetChild(i).localScale = bound.transform.GetChild(4).GetChild(i).localScale;
        }

        // // adjust CornerHL size
        for(int i=0; i<8; i++)
        {
            bound.transform.GetChild(8).GetChild(i).localScale = bound.transform.GetChild(2).GetChild(i).localScale;
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        if(tag == "SelectedObject" && CopyOfObject != null)
        {
            transform.rotation = CopyOfObject.transform.rotation;
            // cancle the copy and its context by grip click
            if(rightHand.GetComponent<VRTK_ControllerEvents>().gripClicked)
            {
                Destroy(bound);
                Destroy(GameObject.Find("coolObject"));
                Destroy(GameObject.Find("coolObject1"));
                if(contextInPersonal.Count != 0)
                {
                    for(int i=0; i<contextInPersonal.Count; i++)
                        Destroy(contextInPersonal[i]);
                    contextInPersonal = new List<GameObject>();
                }
                tag = "Untagged";
                effect.highlighted = false;
                effect.ProfileLoad(Resources.Load<HighlightProfile>("ObjectTouch"));
                GetComponent<VRTK_InteractableObject>().isUsable = true;
                // rightHand.GetComponent<VRTK_Pointer>().enabled = false;
                // rightHand.GetComponent<VRTK_Pointer>().activateOnEnable = true;
                rightHand.GetComponent<VRTK_Pointer>().enabled = true;
                audioSource.PlayOneShot(Resources.Load<AudioClip>("Deselect"));
            }
        }
        // 若場景中有被選擇的物件且不是自己，則關閉 isUsable
        if(GameObject.FindWithTag("SelectedObject") && tag != "SelectedObject")
        {
            GetComponent<VRTK_InteractableObject>().isUsable = false;
        }
        else if(!GameObject.FindWithTag("SelectedObject"))
        {
            GetComponent<VRTK_InteractableObject>().isUsable = true;
        }
    }

    public void clean()
    {
        Destroy(bound);
        Destroy(GameObject.Find("coolObject"));
        Destroy(GameObject.Find("coolObject1"));
        if(contextInPersonal.Count != 0)
        {
            for(int i=0; i<contextInPersonal.Count; i++)
                Destroy(contextInPersonal[i]);
            contextInPersonal = new List<GameObject>();
        }
        tag = "Untagged";
        effect.highlighted = false;
        effect.ProfileLoad(Resources.Load<HighlightProfile>("ObjectTouch"));
        GetComponent<VRTK_InteractableObject>().isUsable = true;
        // rightHand.GetComponent<VRTK_Pointer>().enabled = false;
        // rightHand.GetComponent<VRTK_Pointer>().activateOnEnable = true;
        rightHand.GetComponent<VRTK_Pointer>().enabled = true;
        audioSource.PlayOneShot(Resources.Load<AudioClip>("Deselect"));
    }
}
