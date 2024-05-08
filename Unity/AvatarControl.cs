using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

using Newtonsoft.Json;
[Serializable]
public class JointData
{
    public List<List<List<double>>> A { get; set; }

    public void Zoom(float scale)
    {
        foreach(List<List<double>> subArray2D in A)
        {
            foreach(List<double> subArray1D in subArray2D )
            {
                for(int i = 0; i < subArray1D.Count; i++)
                {
                    subArray1D[i] *= scale;
                }
            }
        }
    }
}
public class AvatarControl : MonoBehaviour
{

    public TextAsset jsonFile; // 将JSON文件拖拽到这个字段中
    public GameObject jointPrefab; // 骨骼的预制体
    public int currentFrame = 0;
    private JointData jointData;
    public int FrameRate = 25;
    private float frameTime;
    private float timeSinceLastFrame;
    public float Zoom = 100f;
    void Start()
    {

        if (jsonFile == null)
        {
            Debug.LogError("JSON file is not assigned.");
            return;
        }

        string jsonDataText = jsonFile.text; // 直接从TextAsset获取文本内容
        Debug.Log(jsonDataText);
        // JointData jointDataa = JsonUtility.FromJson<JointData>(jsonDataText);
        JointData jointDataa = JsonConvert.DeserializeObject<JointData>(jsonDataText);
        if (jointDataa == null)
        {
            Debug.LogError("Failed to parse JSON data ");
            return;
        }
        else if(jointDataa.A == null)
        {
            Debug.LogError("Joints array is null.");
            return;
        }
        jointDataa.Zoom(Zoom);
        jointData = jointDataa;
        
    }

    //  接收一个Frame索引，绘制这一帧的骨骼
    // 针对每一帧，去jointData.A中找到对应的数据，然后绘制
    //  创建关节点数量的GameObject，作为子物体

    private void DrawFrame(int frame)
    {
        ClearFrame();  // 清除当前所有关节点
        if (frame >= jointData.A.Count)
        {
            Debug.LogError("Frame index out of range.");
            return;
        }
        List<List<double>> frameData = jointData.A[frame];
        for (int i = 0; i < frameData.Count; i++)
        {
            List<double> joint = frameData[i];
            Vector3 position = new Vector3((float)joint[0], (float)joint[1], (float)joint[2]);
            
            position += transform.position;
            GameObject jointObject = Instantiate(jointPrefab, position, Quaternion.identity, this.transform); // 创建一个骨骼
            

        }
    }
    private void DrawLineBetweenJoints(Vector3 start, Vector3 end)
    {
        GameObject lineObject = new GameObject("Line");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        
        // 设置线的材质和宽度
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        
        // 设置线的起点和终点
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
    private void ClearFrame()
    {
        // 删除所有子骨骼节点
        while (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            child.SetParent(null); // Optional: Unparent the child first
            Destroy(child.gameObject);
        }

    }
    void Update()
    {
    // 换算帧数，以framerate的速率正确draw Frame
        timeSinceLastFrame += Time.deltaTime;
        frameTime = 1f/FrameRate;
        if (timeSinceLastFrame >= frameTime)
        {
            currentFrame++;
            if (currentFrame >= jointData.A.Count)
            {
                currentFrame = 0;  // Optionally loop the animation
            }
            DrawFrame(currentFrame);
            timeSinceLastFrame -= frameTime;
        }
    }
}