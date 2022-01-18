using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextChanger : MonoBehaviour
{
    public Text showing;
    public GameObject l1,l2;
    public GameObject M_rectangle;
    public GameObject M_cube;

    public GameObject m_cube;

    public GameObject table_hole;
    public GameObject Scaling_task;

    private uint table, scaling;
    private void Start()
    {
        M_rectangle.SetActive(false);
        M_cube.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
        table = table_hole.GetComponent<hole_trigger>().count;
        scaling = Scaling_task.GetComponent<Scaling>().count;
        if(table == 3 || table == 4 || table == 7 || table == 8)
        {
            showing.text = "請用「移動」抓著「橘色」的邊把方塊移動到黑色圓形上\nPlease move the cube onto the black circle by grabbing orange edge of the cube.";
        }
        else
        if(table == 2 || table == 5 || table == 6 || table == 9)
        {
            showing.text = "請用「移動」抓著「綠色」的面把方塊移動到黑色圓形上\nPlease move the cube onto the black circle by grabbing green face of the cube.";
        }
        else
        if(l1.activeSelf || l2.activeSelf)
        {
            //Debug.Log("?!");
            showing.text = "請用「移動」抓著「藍色」的物體本身將方塊放至發光點\nPlease put the cube into the light point by grabbing the blue object.";
        }else
        if(scaling == 1 || scaling == 2)
        {
            showing.text = "請用「縮放」抓著「綠色」的面將長方形拉伸至模型大小\nPlease scale the rectangle until both of two teapots are in the same size by grabbing green face.";
        }else
        if (scaling == 3 || scaling == 4)
        {
            showing.text = "請用「縮放」抓著「橘色」的邊將長方形拉伸至模型大小\nPlease scale the rectangle until both of two teapots are in the same size by grabbing orange edge.";
        }
        else
        if (scaling == 5)
        {
            showing.text = "請用「縮放」抓著「紅色」的點將長方形拉伸至模型大小\nPlease scale the rectangle until both of two teapots are in the same size by grabbing red point.";
        }
        else
        if (M_cube.activeSelf)
        {
            showing.text = "請旋轉後將方塊放入牆壁的凹槽中\nPlease put the cube in the hole on the wall.";
        }
        else
        {
            showing.text = "完成練習階段，請告知工作人員\nFinish training phase, please infrom staffs";
        }
    }
}
