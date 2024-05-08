using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class PoseData{

    public List<List<double>> Aroot {get;set;}
    public List<List<List<List<double>>>> Pose {get;set;}
}

[System.Serializable]
public class Mapper : MonoBehaviour
{

    public Transform baseRoot;
    public Transform[] bones;
    public PoseData poseData;
    public float move_scale = 1.0f;
    public string json_string;

    public bool ready = false;
    private bool isPlaying = false;

    private int currentFrame = 0;

    private float frameTime = 0;
    public float frameRate = 30;

    private float initY = 0;
    // Start is called before the first frame update
    void Start()
    {
        //  log the bones name
        if (bones == null || bones.Length == 0) {
                Debug.LogError("Bones array is not set up!");
            } else {
                for (int i = 0; i < bones.Length; i++) {
                    if (bones[i] == null)
                        Debug.LogError("A bone is missing in the bones array at index " + i + "!");
                    else
                        Debug.Log("Bone " + bones[i].name + " at index " + i + " is correctly assigned.");
                }
            }
        initY = baseRoot.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(isPlaying)
        {
            frameTime += Time.deltaTime;
            if(frameTime >= 1.0f / frameRate)
            {

                DrawFrame(currentFrame);
                currentFrame++;      
                frameTime = 0;
                if (currentFrame >= poseData.Pose.Count)
                {
                
                Transform characterTF = this.transform;
                Vector3 newPosition = new Vector3(characterTF.position.x, initY, characterTF.position.z);
                characterTF.position = newPosition;
                
                isPlaying = false;
                currentFrame = 0;
                }

            }

        }

    }

    public void OnMotionReceived(string data)
    {
        Debug.Log("[Mapper] Received motion:  " + data);
        PraseJson(data);
        isPlaying = true;
    }
    
    // public void praseJson(string json)
    // {
    //     poseData = JsonUtility.FromJson<PoseData>(json);

    //     Debug.Log(poseData.pose);

    // }
    public void PraseJson(string json)
    {
        try
        {
            json_string = json;
            // 检查字符串是否以数组标记开始和结束
            if (json.StartsWith("[") && json.EndsWith("]"))
            {
                // 去掉字符串的第一个和最后一个字符
                json = json.Substring(1, json.Length - 2);
            }
            json_string = json;
            poseData = JsonConvert.DeserializeObject<PoseData>(json_string);
            Debug.Log(poseData.Pose[0][0][0][0]);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to parse JSON with Newtonsoft: " + ex.Message);
        }
    }

    public void DrawFrame(int frame)
    {
        // 根据帧数调整骨骼旋转
        //  frame是int类型，表示当前帧数，作为PoseData.Pose的第一个索引
        // PoseData.Pose是一个四维数组，第一个维度是帧数，第二个维度是关节点数量，第三、四维度组成旋转矩阵
        // 通过PoseData.Pose[frame]获取当前帧的所有关节点数据

        // 关节点只有22个，按照transform Bone的顺序以此对应，不足的先不处理，因为是没有设置完全，可以跳过
        // 应用旋转
        if (poseData == null || poseData.Pose == null || frame >= poseData.Pose.Count)
        {
            Debug.LogWarning("Pose data is not available or frame index is out of range.");
            return;
        }
        // Vector3 foot1Position = Vector3.zero;
        // Vector3 foot2Position = Vector3.zero;
        for(int i = 0; i<22 && i<poseData.Pose[frame].Count; i++)
        {
            // 通过PoseData.Pose[frame][i]获取当前帧的第i个关节点的旋转矩阵
            // 旋转矩阵是一个3x3的矩阵，可以
            var matrixData = poseData.Pose[frame][i];
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetRow(0, new Vector4((float)matrixData[0][0], (float)matrixData[0][1], (float)matrixData[0][2], 0));
            matrix.SetRow(1, new Vector4((float)matrixData[1][0], (float)matrixData[1][1], (float)matrixData[1][2], 0));
            matrix.SetRow(2, new Vector4((float)matrixData[2][0], (float)matrixData[2][1], (float)matrixData[2][2], 0));
            matrix.SetRow(3, new Vector4(0, 0, 0, 1));
            Debug.Log("roting bones " + i );

            Quaternion rotation = Quaternion.LookRotation(
                matrix.GetColumn(2),
                matrix.GetColumn(1)
            );

            bones[i].localRotation = rotation;

            // if (i == 10) foot1Position = bones[i].position;
            // if (i == 11) foot2Position = bones[i].position;

        
        }
        Vector3 move = new Vector3((float)poseData.Aroot[frame][0], (float)poseData.Aroot[frame][1], (float)poseData.Aroot[frame][2]);
        baseRoot.localPosition += move * move_scale;

    }
    public void RepeatMotion()
    {
        isPlaying = true;
        currentFrame = 0;
    }
}
