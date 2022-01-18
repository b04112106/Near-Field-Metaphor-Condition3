using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameManager_trainingPhase : MonoBehaviour
{
    private int input = 0;
    private int mode = -2;

    private int trans_count;
    private int scale_count;
    private int rotate_count;
    public GameObject object_parent;

    public GameObject TranslationCube;
    public GameObject ScalingBrick;
    public GameObject RotationCube;

    public GameObject table;

    public GameObject translation_points;
    public GameObject hole;
    public GameObject TranslationWall;
    public GameObject Light1;
    public GameObject Light2;

    public GameObject scaling_models;

    public GameObject WallWithHole1;
    public GameObject WallWithHole2;

    public Text helperText;

    private GameObject manipulatedObject;
    private GameObject destination;
    private GameObject points;

    private GameObject TW;
    private GameObject L1;
    private GameObject L2;
    // Start is called before the first frame update
    void Start()
    {
        TranslationCube.SetActive(false);
        ScalingBrick.SetActive(false);
        RotationCube.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {   //0~9 ,10 for up ,11 for down ,12 for right ,13 for left
        if (Input.GetKeyDown(KeyCode.Keypad0) && input == -2)
            input = 0;
        if (Input.GetKeyDown(KeyCode.Keypad1) && input == -2)
            input = 1;
        if (Input.GetKeyDown(KeyCode.Keypad2) && input == -2)
            input = 2;
        if (Input.GetKeyDown(KeyCode.RightArrow) && input == -2)
            input = 13;
        if (Input.GetKeyDown(KeyCode.LeftArrow) && input == -2)
            input = 12;
        if (Input.GetKeyUp(KeyCode.Keypad0))
            input = -2;
        if (Input.GetKeyUp(KeyCode.Keypad1))
            input = -2;
        if (Input.GetKeyUp(KeyCode.Keypad2))
            input = -2;
        if (Input.GetKeyUp(KeyCode.RightArrow))
            input = -2;
        if (Input.GetKeyUp(KeyCode.LeftArrow))
            input = -2;
        switch (input)
        {
            case 0:
                // start translation
                mode = 0;
                Debug.Log("Changing to translation");
                clean();
                manipulatedObject = Instantiate(TranslationCube, object_parent.transform, false);
                destination = Instantiate(hole, object_parent.transform, false);
                points = Instantiate(translation_points, object_parent.transform, false);
                TW = Instantiate(TranslationWall, object_parent.transform, false);
                manipulatedObject.SetActive(true);
                destination.SetActive(true);
                points.SetActive(true);
                TW.SetActive(true);

                trans_count = 1;
                destination.transform.position = points.transform.Find("point" + trans_count.ToString()).transform.position;

                input = -1;
                break;
            case 1:
                // start scaling
                mode = 1;
                Debug.Log("Changing to scaling");
                clean();
                manipulatedObject = Instantiate(ScalingBrick, object_parent.transform, false);
                destination = Instantiate(scaling_models, object_parent.transform, false);
                manipulatedObject.SetActive(true);
                destination.SetActive(true);
                destination.transform.Find("Target0").gameObject.SetActive(true);
                scale_count = 0;
                input = -1;
                break;
            case 2:
                // start rotation
                mode = 2;
                Debug.Log("Changing to rotation");
                clean();
                manipulatedObject = Instantiate(RotationCube, object_parent.transform, false);
                destination = Instantiate(WallWithHole1, object_parent.transform, false);
                manipulatedObject.SetActive(true);
                destination.SetActive(true);
                rotate_count = 1;
                input = -1;
                break;
        }
        switch(mode)
        {
            case 0:
                // Translation mode
                if(input == 13)
                {
                    trans_count++;
                    if (points.transform.Find("point" + trans_count.ToString()))
                    {
                        destination.transform.position = points.transform.Find("point" + trans_count.ToString()).transform.position;
                        destination.transform.rotation = points.transform.Find("point" + trans_count.ToString()).transform.rotation;
                    }
                    else if(trans_count == 9)
                    {
                        destination.SetActive(false);
                        L1 = Instantiate(Light1, object_parent.transform, false);
                        L1.SetActive(true);
                    }
                    else if(trans_count == 10)
                    {
                        //L1.GetComponent<PersonalSpace>().clean();
                        Destroy(L1);
                        L2 = Instantiate(Light2, object_parent.transform, false);
                        L2.SetActive(true);
                    }
                    else
                        trans_count--;
                    input = -1;
                }
                if (input == 12)
                {
                    trans_count--;
                    if (points.transform.Find("point" + trans_count.ToString()))
                    {
                        //L1.GetComponent<PersonalSpace>().clean();
                        Destroy(L1);
                        destination.SetActive(true);
                        destination.transform.position = points.transform.Find("point" + trans_count.ToString()).transform.position;
                        destination.transform.rotation = points.transform.Find("point" + trans_count.ToString()).transform.rotation;
                    }
                    else if (trans_count == 9)
                    {
                        //L2.GetComponent<PersonalSpace>().clean();
                        Destroy(L2);
                        L1 = Instantiate(Light1, object_parent.transform, false);
                        L1.SetActive(true);
                    }
                    else
                        trans_count++;
                    input = -1;
                }
                //todo text
                switch(trans_count)
                {
                    case 1:
                        helperText.text = "請用「移動」抓著「綠色」的面把方塊移動到黑色圓形上\n(即一維移動)";
                        break;
                    case 2:
                        helperText.text = "請用「移動」抓著「橘色」的邊把方塊移動到黑色圓形上\n(即二維移動)";
                        break;
                    case 3:
                        helperText.text = "請用「移動」抓著「橘色」的邊把方塊移動到黑色圓形上\n(即二維移動)";
                        break;
                    case 4:
                        helperText.text = "請用「移動」抓著「綠色」的面把方塊移動到黑色圓形上\n(即一維移動)";
                        break;
                    case 5:
                        helperText.text = "請用「移動」抓著「綠色」的面把方塊移動到黑色圓形上\n(即一維移動)";
                        break;
                    case 6:
                        helperText.text = "請用「移動」抓著「橘色」的邊把方塊移動到黑色圓形上\n(即二維移動)";
                        break;
                    case 7:
                        helperText.text = "請用「移動」抓著「橘色」的邊把方塊移動到黑色圓形上\n(即二維移動)";
                        break;
                    case 8:
                        helperText.text = "請用「移動」抓著「綠色」的面把方塊移動到黑色圓形上\n(即一維移動)";
                        break;
                    case 9:
                        helperText.text = "請用「移動」抓著「藍色」的物體複製品將方塊放至發光點\n(即三維移動)";
                        break;
                    case 10:
                        helperText.text = "請用「移動」抓著「藍色」的物體複製品將方塊放至發光點\n(即三維移動)";
                        break;
                }
                break;
            case 1:
                // Scaling mode
                if (input == 13)
                {
                    scale_count++;
                    if (destination.transform.Find("Target" + scale_count.ToString()))
                    {
                        manipulatedObject.GetComponent<PersonalSpace>().clean();
                        Destroy(manipulatedObject);
                        manipulatedObject = Instantiate(ScalingBrick, object_parent.transform, false);
                        manipulatedObject.SetActive(true);
                        destination.transform.Find("Target" + (scale_count - 1).ToString()).gameObject.SetActive(false);
                        destination.transform.Find("Target" + scale_count.ToString()).gameObject.SetActive(true);
                    }
                    else
                        scale_count--;
                    input = -1;
                }
                if (input == 12)
                {
                    scale_count--;
                    if (destination.transform.Find("Target" + scale_count.ToString()))
                    {
                        manipulatedObject.GetComponent<PersonalSpace>().clean();
                        Destroy(manipulatedObject);
                        manipulatedObject = Instantiate(ScalingBrick, object_parent.transform, false);
                        manipulatedObject.SetActive(true);
                        destination.transform.Find("Target" + (scale_count + 1).ToString()).gameObject.SetActive(false);
                        destination.transform.Find("Target" + scale_count.ToString()).gameObject.SetActive(true);
                    }
                    else
                        scale_count++;
                    input = -1;
                }
                //todo text
                switch(scale_count)
                {
                    case 0:
                        helperText.text = "請用「縮放」抓著「綠色」的面將長方形拉伸至模型大小(即一維縮放)";
                        break;
                    case 1:
                        helperText.text = "請用「縮放」抓著「綠色」的面將長方形拉伸至模型大小(即一維縮放)";
                        break;
                    case 2:
                        helperText.text = "請用「縮放」抓著「橘色」的邊將長方形拉伸至模型大小(即二維縮放)";
                        break;
                    case 3:
                        helperText.text = "請用「縮放」抓著「橘色」的邊將長方形拉伸至模型大小(即二維縮放)";
                        break;
                    case 4:
                        helperText.text = "請用「縮放」抓著「紅色」的端點將長方形拉伸至模型大小(即三維縮放)";
                        break;
                }
                break;
            case 2:
                // Rotation mode
                if (input == 13)
                {
                    if (rotate_count == 2)
                    {
                        input = -1;
                    }
                    else
                    {
                        clean();
                        manipulatedObject = Instantiate(RotationCube, object_parent.transform, false);
                        destination = Instantiate(WallWithHole2, object_parent.transform, false);
                        manipulatedObject.SetActive(true);
                        destination.SetActive(true);
                        rotate_count = 2;
                    }
                }
                if (input == 12)
                {
                    if (rotate_count == 1)
                    {
                        input = -1;
                    }
                    else
                    {
                        clean();
                        manipulatedObject = Instantiate(RotationCube, object_parent.transform, false);
                        destination = Instantiate(WallWithHole1, object_parent.transform, false);
                        manipulatedObject.SetActive(true);
                        destination.SetActive(true);
                        rotate_count = 1;
                    }
                }
                //todo text
                switch(rotate_count)
                {
                    case 1:
                        helperText.text = "請「旋轉」後將方塊放入牆壁的凹槽中";
                        break;
                    case 2:
                        helperText.text = "請「旋轉」後將方塊放入牆壁的凹槽中";
                        break;
                }
                break;
        }
        
    }
    private void clean()
    {
        if (manipulatedObject)
            manipulatedObject.GetComponent<PersonalSpace>().clean();
        
        Destroy(manipulatedObject);
        Destroy(destination);
        Destroy(points);
        Destroy(TW);
        Destroy(L1);
        Destroy(L2);
    }
}
