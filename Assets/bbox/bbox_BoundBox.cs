using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class bbox_BoundBox : MonoBehaviour {
	
	public GameObject cam; 
	public bool colliderBased = false;
	public bool permanent = false; //permanent//onMouseDown
	
	public Color lineColor = new Color(0f,1f, 0.4f,0.1f);

	private Bounds bound;
	
	private Vector3[] corners;
	
	private Vector3[,] lines;
	
	private Quaternion quat;
	
	private Camera mcamera;
	
	private bbox_drawLines cameralines;
	
	private Renderer[] renderers;
	
	private Material[][] Materials;
	
	private Vector3 topFrontLeft;
	private Vector3 topFrontRight;
	private Vector3 topBackLeft;
	private Vector3 topBackRight;
	private Vector3 bottomFrontLeft;
	private Vector3 bottomFrontRight;
	private Vector3 bottomBackLeft;
	private Vector3 bottomBackRight;
	

	void Awake () {	
		renderers = GetComponentsInChildren<Renderer>();
		cameralines = new bbox_drawLines();
		Materials = new Material[renderers.Length][];
		for(int i = 0; i < renderers.Length; i++) {
			Materials[i]= renderers[i].materials;
		}
	}
	
	
	void Start () {
		cameralines = cam.GetComponent<bbox_drawLines>();
		init();
	}
	
	public void init() {
		calculateBounds();
		setPoints();
		setLines();
		cameralines.setOutlines(lines,lineColor);
	}
	
	void Update() {
		calculateBounds();
		setPoints();
		setLines();
		cameralines.setOutlines(lines,lineColor);
	}
	
	void calculateBounds() {
		quat = transform.rotation;
		
		if(colliderBased||!renderers[0].isPartOfStaticBatch){
			//transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,0f,transform.rotation.eulerAngles.z);//
			transform.rotation = Quaternion.Euler(0f,0f,0f);//
		}
		if(colliderBased){
			BoxCollider coll = GetComponent<BoxCollider>();
			if(coll){
				bound = coll.bounds;
			}else{
				Debug.Log("No collider attached");
			}
		}else{
			bound = new Bounds();
			bound = renderers[0].bounds;
			for(int i = 1; i < renderers.Length; i++) {
				bound.Encapsulate(renderers[i].bounds);
			}
		}
		if(colliderBased||!renderers[0].isPartOfStaticBatch){
			transform.rotation = quat;
			//quat = Quaternion.AngleAxis(quat.eulerAngles.y,Vector3.up);//
		}else{
			quat = Quaternion.Euler(0f,0f,0f);
		}
	}
	
	void setPoints() {
	
		Vector3 bc = transform.position + quat *(bound.center - transform.position);

		topFrontRight = bc +  quat *Vector3.Scale(bound.extents, new Vector3(1, 1, 1)); 
		topFrontLeft = bc +  quat *Vector3.Scale(bound.extents, new Vector3(-1, 1, 1)); 
		topBackLeft = bc +  quat *Vector3.Scale(bound.extents, new Vector3(-1, 1, -1));
		topBackRight = bc +  quat *Vector3.Scale(bound.extents, new Vector3(1, 1, -1)); 
		bottomFrontRight = bc +  quat *Vector3.Scale(bound.extents, new Vector3(1, -1, 1)); 
		bottomFrontLeft = bc +  quat *Vector3.Scale(bound.extents, new Vector3(-1, -1, 1)); 
		bottomBackLeft = bc +  quat *Vector3.Scale(bound.extents, new Vector3(-1, -1, -1));
		bottomBackRight = bc +  quat *Vector3.Scale(bound.extents, new Vector3(1, -1, -1)); 
		corners = new Vector3[]{topFrontRight,topFrontLeft,topBackLeft,topBackRight,bottomFrontRight,bottomFrontLeft,bottomBackLeft,bottomBackRight};
		
	}
	
	void setLines() {
		
		int i1;
		int linesCount = 12;

		lines = new Vector3[linesCount,2];
		for (int i=0; i<4; i++) {
			i1 = (i+1)%4;//top rectangle
			lines[i,0] = corners[i];
			lines[i,1] = corners[i1];
			//break;
			i1 = i + 4;//vertical lines
			lines[i+4,0] = corners[i];
			lines[i+4,1] = corners[i1];
			//bottom rectangle
			lines[i+8,0] = corners[i1];
			i1 = 4 + (i+1)%4;
			lines[i+8,1] = corners[i1];
		}
	}
	
	void OnMouseDown() {
		if(permanent) return;
		enabled = !enabled;
	}
	
}
