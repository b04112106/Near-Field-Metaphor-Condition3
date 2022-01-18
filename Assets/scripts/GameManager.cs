using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using System.Text;
using System.IO;
using UnityEngine.UI;
using HighlightPlus;

public class GameManager : MonoBehaviour
{
    public GameObject smallObj, smallObjWire, bigObj, bigObjWire, tunnel, pickCube, tunnelCube;
    public GameObject [] pickPlace;
    public Text text;
    public int ID, condition;
    private bool start = false;
    private GameObject rightController, s, w;
    private int mode = 0; // 0:T+R(docking), 1:T+S(docking), 2:T+R+S(docking), 3:pick&place, 4:tunnel
    private int size = 0; // 0:small, 1:medium
    private int dim = 1; // 1:1-dim, 2:2-dim, 3:3-dim
    private int trial = 0;
    private int task = 0;
    private float attempt = 0, collNum = 0, pathLen = 0, cScaleError = 0;
    private float xInit = 0f, yInit = 1.5f, zInit = 4f;
    private float xMax = 2f, xMin = -2f, yMax = 2.5f, yMin = 0.5f, zMax = 3f, zMin = 5f, sMin = 0.5f, sMax = 2.5f;
    private float bxMax = 3f, bxMin = -3f, byMax = 2.5f, byMin = 0.5f, bzMax = 3f, bzMin = 5f;
    private bool click = true;
    private float time_f = 0f;
    private Vector3 totalAngular;
    private Vector3 oriPos, oriScale, oriAngular;
    private List<string[]> rowData = new List<string[]>();
    private int num = 98;
    // Start is called before the first frame update
    void Start()
    {
        rightController = GameObject.Find("RightControllerScriptAlias");
        tunnel.SetActive(false);
        tunnelCube.SetActive(false);
        pickCube.SetActive(false);
        for(int i=0; i<8; i++)
            pickPlace[i].SetActive(false);
        // mode:0, size:0, dim:1, trial:1
        // s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
        // w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), yInit, zInit), smallObjWire.transform.rotation);
        // w.transform.Rotate(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
        // trial++;
        CreateObject();
        // Creating First row of titles manually..
        string[] rowDataTemp = new string[19]{"PID", "condition", "case", "task", "trial", "size", "distance(cm)", "Angular error X", "Angular error Y", "Angular error Z",
                                              "volume difference(cm^3)", "time(sec.)", "number of attempt", "number of collision", "path length(cm)", "total angular rotation X",
                                              "total angular rotation Y", "total angular rotation Z", "continous delta volume error(m^3)"};
        rowData.Add(rowDataTemp);
        Resources.Load<HighlightProfile>("ObjectSelect").outline = 0;
    }
    public void release(float num)
    {
        attempt += num;
    }
    public void setStart()
    {
        start = true;
    }
    public int getMode()
    {
        return mode;
    }
    private void CreateObject()
    {
        
        if(mode == 0) // pick & place
        {
            int ID = Random.Range(0, 7);
            s = Instantiate(pickCube, pickCube.transform.position, pickCube.transform.rotation);
            w = Instantiate(pickPlace[ID], pickPlace[ID].transform.position, pickPlace[ID].transform.rotation);
            s.SetActive(true);
            w.SetActive(true);
            if(task == 1)
            {
                s.transform.Rotate(0,30,0);
                w.transform.Rotate(0,30,0);
            }
            else if(task == 2)
            {
                s.transform.Rotate(0,-30,0);
                w.transform.Rotate(0,-30,0);
            }
            if(size == 0)
            {
                float f = Random.Range(0.4f, 0.9f);
                w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(f, f, 1));
                w.transform.Translate(-0.3f,0,0);
            }
            else if(size == 1)
            {
                s.transform.localScale = new Vector3(1f,1f,1f);
                float f = Random.Range(1, 1.8f);
                w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(f, f, 1));
                w.transform.Translate(0,0,-0.3f);
                if(task == 2)
                    dim = 3;
            }
        }
        else if(mode == 1) // T+R (docking)
        {
            if(dim == 1)
            {
                if(task == 0) // x-axis
                {
                    if(size == 0)
                    {
                        s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                        w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), yInit, zInit), smallObjWire.transform.rotation);
                        w.transform.Rotate(Random.Range(0f, 360f), 0, 0);
                    }
                    else if(size == 1)
                    {
                        s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                        w = Instantiate(bigObjWire, new Vector3(Random.Range(bxMin, bxMax), yInit, zInit), bigObjWire.transform.rotation);
                        w.transform.Rotate(Random.Range(0f, 360f), 0, 0);
                    }
                }
                else if(task == 1) // y-axis
                {
                    if(size == 0)
                    {
                        s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                        w = Instantiate(smallObjWire, new Vector3(xInit, Random.Range(yMin, yMax), zInit), smallObjWire.transform.rotation);
                        w.transform.Rotate(0, Random.Range(0f, 360f), 0);
                    }
                    else if(size == 1)
                    {
                        s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                        w = Instantiate(bigObjWire, new Vector3(xInit, Random.Range(byMin, byMax), zInit), bigObjWire.transform.rotation);
                        w.transform.Rotate(0, Random.Range(0f, 360f), 0);
                    }
                }
                else if(task == 2) // z-axis
                {
                    if(size == 0)
                    {
                        s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                        w = Instantiate(smallObjWire, new Vector3(xInit, yInit, Random.Range(zMin, zMax)), smallObjWire.transform.rotation);
                        w.transform.Rotate(0, 0, Random.Range(0f, 360f));
                    }
                    else if(size == 1)
                    {
                        s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                        w = Instantiate(bigObjWire, new Vector3(xInit, yInit, Random.Range(bzMin, bzMax)), bigObjWire.transform.rotation);
                        w.transform.Rotate(0, 0, Random.Range(0f, 360f));
                    }
                }
            }
            else if(dim == 2)
            {
                if(task == 0) // x-y plane
                {
                    if(size == 0)
                    {
                        s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                        w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), zInit), smallObjWire.transform.rotation);
                        w.transform.Rotate(Random.Range(0f, 360f), 0, 0);
                    }
                    else if(size == 1)
                    {
                        s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                        w = Instantiate(bigObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), zInit), bigObjWire.transform.rotation);
                        w.transform.Rotate(Random.Range(0f, 360f), 0, 0);
                    }
                }
                else if(task == 1) // y-z plane
                {
                    if(size == 0)
                    {
                        s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                        w = Instantiate(smallObjWire, new Vector3(xInit, Random.Range(yMin, yMax), Random.Range(zMin, zMax)), smallObjWire.transform.rotation);
                        w.transform.Rotate(0, Random.Range(0f, 360f), 0);
                    }
                    else if(size == 1)
                    {
                        s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                        w = Instantiate(bigObjWire, new Vector3(xInit, Random.Range(yMin, yMax), Random.Range(zMin, zMax)), bigObjWire.transform.rotation);
                        w.transform.Rotate(0, Random.Range(0f, 360f), 0);
                    }
                }
                else if(task == 2) // x-z plane
                {
                    if(size == 0)
                    {
                        s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                        w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), yInit, Random.Range(zMin, zMax)), smallObjWire.transform.rotation);
                        w.transform.Rotate(0, 0, Random.Range(0f, 360f));
                    }
                    else if(size == 1)
                    {
                        s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                        w = Instantiate(bigObjWire, new Vector3(Random.Range(xMin, xMax), yInit, Random.Range(zMin, zMax)), bigObjWire.transform.rotation);
                        w.transform.Rotate(0, 0, Random.Range(0f, 360f));
                    }
                }
            }
            else if(dim == 3) // x-y-z
            {
                if(size == 0)
                {
                    s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                    w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), smallObjWire.transform.rotation);
                }
                else if(size == 1)
                {
                    s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                    w = Instantiate(bigObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), bigObjWire.transform.rotation);
                }
                if(task == 0)
                    w.transform.Rotate(Random.Range(0f, 360f), 0, 0);
                else if(task == 1)
                    w.transform.Rotate(0, Random.Range(0f, 360f), 0);
                else if(task == 2)
                    w.transform.Rotate(0, 0, Random.Range(0f, 360f));
            }
        }
        else if(mode == 2) // T+S (docking)
        {
            if(dim == 1)
            {
                if(task == 0) // X
                {
                    if(size == 0)
                    {
                        s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                        w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), smallObjWire.transform.rotation);
                        w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(Random.Range(sMin, sMax), 1, 1));
                    }
                    else if(size == 1)
                    {
                        s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                        w = Instantiate(bigObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), bigObjWire.transform.rotation);
                        w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(Random.Range(sMin, sMax), 1, 1));
                    }
                }
                else if(task == 1) // Y
                {
                    if(size == 0)
                    {
                        s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                        w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), smallObjWire.transform.rotation);
                        w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(1, Random.Range(sMin, sMax), 1));
                    }
                    else if(size == 1)
                    {
                        s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                        w = Instantiate(bigObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), bigObjWire.transform.rotation);
                        w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(1, Random.Range(sMin, sMax), 1));
                    }
                }
                else if(task == 2) // Z
                {
                    if(size == 0)
                    {
                        s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                        w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), smallObjWire.transform.rotation);
                        w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(1, 1, Random.Range(sMin, sMax)));
                    }
                    else if(size == 1)
                    {
                        s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                        w = Instantiate(bigObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), bigObjWire.transform.rotation);
                        w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(1, 1, Random.Range(sMin, sMax)));
                    }
                }
            }
            else if(dim == 2)
            {
                if(task == 0) // X-Y
                {
                    if(size == 0)
                    {
                        s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                        w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), smallObjWire.transform.rotation);
                        w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(Random.Range(sMin, sMax), Random.Range(sMin, sMax), 1));
                    }
                    else if(size == 1)
                    {
                        s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                        w = Instantiate(bigObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), bigObjWire.transform.rotation);
                        w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(Random.Range(sMin, sMax), Random.Range(sMin, sMax), 1));
                    }
                }
                else if(task == 1) // Y-Z
                {
                    if(size == 0)
                    {
                        s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                        w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), smallObjWire.transform.rotation);
                        w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(1, Random.Range(sMin, sMax), Random.Range(sMin, sMax)));
                    }
                    else if(size == 1)
                    {
                        s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                        w = Instantiate(bigObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), bigObjWire.transform.rotation);
                        w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(1, Random.Range(sMin, sMax), Random.Range(sMin, sMax)));
                    }
                }
                else if(task == 2) // Z
                {
                    if(size == 0)
                    {
                        s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                        w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), smallObjWire.transform.rotation);
                        w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(Random.Range(sMin, sMax), 1, Random.Range(sMin, sMax)));
                    }
                    else if(size == 1)
                    {
                        s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                        w = Instantiate(bigObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), bigObjWire.transform.rotation);
                        w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(Random.Range(sMin, sMax), 1, Random.Range(sMin, sMax)));
                    }
                }
            }
            else if(dim == 3)
            {
                if(size == 0)
                {
                    s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                    w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), smallObjWire.transform.rotation);
                    w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(Random.Range(sMin, sMax), Random.Range(sMin, sMax), Random.Range(sMin, sMax)));
                }
                else if(size == 1)
                {
                    s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                    w = Instantiate(bigObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), bigObjWire.transform.rotation);
                    w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(Random.Range(sMin, sMax), Random.Range(sMin, sMax), Random.Range(sMin, sMax)));
                }
            }
        }
        else if(mode == 3) // T+R+S (docking)
        {
            if(size == 0)
            {
                s = Instantiate(smallObj, new Vector3(xInit, yInit, zInit), smallObj.transform.rotation);
                w = Instantiate(smallObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), smallObjWire.transform.rotation);
                w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(Random.Range(sMin, sMax), Random.Range(sMin, sMax), Random.Range(sMin, sMax)));
            }
            else if(size == 1)
            {
                s = Instantiate(bigObj, new Vector3(xInit, yInit, zInit), bigObj.transform.rotation);
                w = Instantiate(bigObjWire, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), bigObjWire.transform.rotation);
                w.transform.localScale = Vector3.Scale(w.transform.localScale, new Vector3(Random.Range(sMin, sMax), Random.Range(sMin, sMax), Random.Range(sMin, sMax)));
                if(task == 2)
                    dim = 3;
            }
            if(task == 0)
                w.transform.Rotate(Random.Range(0f, 360f), 0, 0);
            else if(task == 1)
                w.transform.Rotate(0, Random.Range(0f, 360f), 0);
            else if(task == 2)
                w.transform.Rotate(0, 0, Random.Range(0f, 360f));
        }
        else if(mode == 4) // tunnel
        {
            s = Instantiate(tunnelCube, tunnelCube.transform.position, tunnelCube.transform.rotation);
            w = Instantiate(tunnel, tunnel.transform.position, tunnel.transform.rotation);
            s.SetActive(true);
            w.SetActive(true);
            Resources.Load<HighlightProfile>("ObjectSelect").outline = 1;
            if(trial == 1)
                s.transform.Translate(2, 0, 0);
            if(size == 1)
                s.transform.localScale *= 2;
            if(task == 1)
            {
                var f = Random.Range(-30, 30);
                s.transform.Rotate(0, f, 0);
                w.transform.Rotate(0, f, 0);
            }
        }
        oriPos = s.transform.position;
        oriScale = s.transform.lossyScale;
        oriAngular = s.transform.localEulerAngles;
    }
    private void timer()
    {
        time_f += Time.deltaTime;
    }
    private void calDiff()
    {
        pathLen += Vector3.Distance(s.transform.position, oriPos);
        if(Mathf.Abs(s.transform.lossyScale.x*s.transform.lossyScale.y*s.transform.lossyScale.z - oriScale.x*oriScale.y*oriScale.z) > 0.001)
            cScaleError += Mathf.Abs(s.transform.lossyScale.x*s.transform.lossyScale.y*s.transform.lossyScale.z - oriScale.x*oriScale.y*oriScale.z);
        
        oriPos = s.transform.position;
        oriScale = s.transform.lossyScale;
        
    }
    public void calRotation()
    {
        var v = new Vector3(Mathf.Abs(s.transform.localEulerAngles.x - oriAngular.x), Mathf.Abs(s.transform.localEulerAngles.y - oriAngular.y), Mathf.Abs(s.transform.localEulerAngles.z - oriAngular.z));
        totalAngular += v;
        oriAngular = s.transform.localEulerAngles;
    }
    
    public void finishOneTrial()
    {
        // string[] rowDataTemp = new string[19]{"PID", "condition", "case", "task", "trial", "size", "distance(cm)", "Angular error X", "Angular error Y", "Angular error Z",
        //                                       "volume difference(cm^3)", "time(sec.)", "number of attempt", "number of collision", "path length(cm)", "total angular rotation X",
        //                                       "total angular rotation Y", "total angular rotation Z", "continous delta volume error(cm^3)"};
        // save data to rowData
        string[] rowDataTemp = new string[19];
        rowDataTemp[0] = ID.ToString();
        rowDataTemp[1] = condition.ToString();
        if(mode == 0)
            rowDataTemp[2] = "pick and place: 3-dim T + 1-dim R + 3-dim S";
        else if(mode == 1)
        {
            if(dim == 1)
                rowDataTemp[2] = "docking: 1-dim T + 1-dim R";
            else if(dim == 2)
                rowDataTemp[2] = "docking: 2-dim T + 1-dim R";
            else if(dim == 3)
                rowDataTemp[2] = "docking: 3-dim T + 1-dim R";
        }
        else if(mode == 2)
        {
            if(dim == 1)
                rowDataTemp[2] = "docking: 3-dim T + 1-dim S";
            else if(dim == 2)
                rowDataTemp[2] = "docking: 3-dim T + 2-dim S";
            else if(dim == 3)
                rowDataTemp[2] = "docking: 3-dim T + 3-dim S";
        }
        else if(mode == 3)
            rowDataTemp[2] = "docking: 3-dim T + 3-dim R + 3-dim S";
        else if(mode == 4)
            rowDataTemp[2] = "tunnel: 3-dim T + 1-dim R + 3-dim S";
        rowDataTemp[3] = (task + 1).ToString();
        rowDataTemp[4] = (trial + 1).ToString();
        rowDataTemp[5] = (size == 0) ? "small" : "big";
        if(mode == 1 || mode == 2 || mode == 3)
        {
            rowDataTemp[6] = (Vector3.Distance(s.transform.position, w.transform.position)*100f).ToString();
            rowDataTemp[7] = (s.transform.localEulerAngles - w.transform.localEulerAngles)[0].ToString();
            rowDataTemp[8] = (s.transform.localEulerAngles - w.transform.localEulerAngles)[1].ToString();
            rowDataTemp[9] = (s.transform.localEulerAngles - w.transform.localEulerAngles)[2].ToString();
            rowDataTemp[10] = ((s.transform.lossyScale.x*s.transform.lossyScale.y*s.transform.lossyScale.z - w.transform.lossyScale.x*w.transform.lossyScale.y*w.transform.lossyScale.z)*1000000f).ToString();
        }
        rowDataTemp[11] = time_f.ToString();
        rowDataTemp[14] = pathLen.ToString();
        if(mode == 0 || mode == 4)
        {
            rowDataTemp[12] = attempt.ToString();
            rowDataTemp[13] = collNum.ToString();
            rowDataTemp[15] = totalAngular[0].ToString();
            rowDataTemp[16] = totalAngular[1].ToString();
            rowDataTemp[17] = totalAngular[2].ToString();
            rowDataTemp[18] = cScaleError.ToString();
        }

        rowData.Add(rowDataTemp);
        
        if(mode == 4 && size == 0 && task == 2)
        {
            Save();
            s.GetComponent<PersonalSpace>().clean();
            Destroy(s);
            Destroy(w);
        }
        else
        {
            // reset and create new trial
            s.GetComponent<PersonalSpace>().clean();
            Destroy(s);
            Destroy(w);
            CreateObject();
            resetValue();
        }
        trial++;
        if(trial == 2)
        {
            size++;
            trial = 0;
        }
        if(size == 2)
        {
            task++;
            size = 0;
        }
        if(task == 3)
        {
            dim++;
            task = 0;
        }
        if(dim == 4)
        {
            mode++;
            dim = 1;
        }
        num -= 1;
        Debug.Log("Remain: " + num + " trials.");
    }
    public void addColl()
    {
        collNum += 1;
    }
    private void Save()
    {
        string[][] output = new string[rowData.Count][];

        for(int i = 0; i < output.Length; i++){
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();
        
        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));
        
        
        string filePath = getPath();

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }

    // Following method is used to retrive the relative path as device platform
    private string getPath(){
        #if UNITY_EDITOR
        return Application.dataPath +"/CSV/"+"Saved_data_"+ID.ToString()+".csv";
        #elif UNITY_ANDROID
        return Application.persistentDataPath+"Saved_data.csv";
        #elif UNITY_IPHONE
        return Application.persistentDataPath+"/"+"Saved_data.csv";
        #else
        return Application.dataPath +"/"+"Saved_data.csv";
        #endif
    }
    private void resetValue()
    {
        click = false;
        // Debug.Log(time_f);
        // reset timer
        start = false;
        time_f = 0f;
        // reset attempt number;
        // Debug.Log(attempt);
        attempt = 0;
        collNum = 0;
        pathLen = 0;
        totalAngular = Vector3.zero;
        cScaleError = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(start)
            timer();
        if(s != null)
            calDiff();
        if(GameObject.FindWithTag("CopyOfObject"))
        {
            if(GameObject.FindWithTag("CopyOfObject").GetComponent<ControlObject>())
            {
                Destroy(GameObject.FindWithTag("CopyOfObject").GetComponent<ControlObject>());
                if(GameObject.FindWithTag("CopyOfObject").transform.childCount == 8)
                {
                    for(int i=0; i<8; i++)
                        Destroy(GameObject.FindWithTag("CopyOfObject").transform.GetChild(i).gameObject);
                }
                click = true;
            }
        }
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            Destroy(s);
            Destroy(w);
            CreateObject();
        }
        if(Input.GetKeyDown(KeyCode.Keypad0))
        {
            mode = 0; trial = 0; size = 0; task = 0; dim = 1;
            Destroy(s);
            Destroy(w);
            CreateObject();
        }
        else if(Input.GetKeyDown(KeyCode.Keypad1))
        {
            mode = 1; trial = 0; size = 0; task = 0; dim = 1;
            Destroy(s);
            Destroy(w);
            CreateObject();
        }
        else if(Input.GetKeyDown(KeyCode.Keypad2))
        {
            mode = 2; trial = 0; size = 0; task = 0; dim = 1;
            Destroy(s);
            Destroy(w);
            CreateObject();
        }
        else if(Input.GetKeyDown(KeyCode.Keypad3))
        {
            mode = 3; trial = 0; size = 0; task = 0; dim = 3;
            Destroy(s);
            Destroy(w);
            CreateObject();
        }
        else if(Input.GetKeyDown(KeyCode.Keypad4))
        {
            mode = 4; trial = 0; size = 0; task = 0; dim = 1;
            Destroy(s);
            Destroy(w);
            CreateObject();
        }
        // change text
        if(mode == 1 || mode == 2 || mode == 3)
            text.GetComponent<Text>().text = "請將方塊放進框框中，並且將8個角落的顏色對應正確\n若覺得放好後，按下下方的按紐\n↓";
        else if(mode == 4)
            text.GetComponent<Text>().text = "請試著讓方塊通過管子\n到達出口後，請按下下方的按紐\n↓";
    }
}
