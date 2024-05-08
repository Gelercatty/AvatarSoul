using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

//创建一个Transform类型的量CameraRotation用来保存实现相机旋转
    public Transform CameraRotation;

   //定义两个私有类型的量Mouse_X,Mouse_Y分别接收鼠标向各个方向滑动的值
    private float Mouse_X;
    private float Mouse_Y;
    public UImanager UI;
   //鼠标灵敏度
    public float MouseSensitivity;
   
   //定义一个浮点类型的量，记录绕X轴旋转的角度
    public float xRotation;
    
   //放在Updata里面每一帧都会执行，导致不能够保存前一时刻的值 
    void Update()
    {
        if (UI.isInputFieldActive == true)
        {
            return;
        }
        //获取鼠标左右移动的量乘上灵敏度的值
        Mouse_X = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        //获取鼠标上下移动的量乘上灵敏度的值
        Mouse_Y = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

        xRotation = xRotation - Mouse_Y;
        //xRotation值为正时，屏幕下移，当xRotation值为负时，屏幕上移
        //当鼠标向上滑动，Mouse_Y值为正,xRotation-Mouse_Y的值为负,xRotation总的值为负，屏幕视角向上滑动
        //当鼠标向下滑动，Mouse_Y值为负,xRotation-Mouse_Y的值为正,xRotation总的值为正，屏幕视角向下滑动
        //简单来说就是要控制鼠标滑动的方向与屏幕移动的方向要相同

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        //限制 value的值在-90f,90f之间,如果xRotation大于90f,则返回90f,如果xRotation小于-90f,则返回-90f,否者返回xRotation;

        //相机左右旋转时，是以Y轴为中心旋转的，上下旋转时，是以X轴为中心旋转的
        CameraRotation.Rotate(Vector3.up * Mouse_X);
        //Vector3.up相当于Vector3(0,1,0),CameraRotation.Rotate(Vector3.up * Mouse_X)相当于使CameraRotation对象绕y轴旋转Mouse_X个单位
        //即相机左右旋转时，是以Y轴为中心旋转的，此时Mouse_X控制着值的大小

        //相机在上下旋转移动时，相机方向不会随着移动，类似于低头和抬头，左右移动时，相机方向会随着向左向右移动，类似于向左向右看
        //所以在控制相机向左向右旋转时，要保证和父物体一起转动
        this.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        //this.transform指这个CameraRotation的位置,localRotation指的是旋转轴
        //transform.localRotation = Quaternion.Eular(x,y,z)控制旋转的时候，按照X-Y-Z轴的旋转顺规
        //即以围绕X轴旋转x度，围绕Y轴旋转y度，围绕Z轴旋转z度
        //且绕轴旋转的坐标轴是父节点本地坐标系的坐标轴
    

}
}
