# AvatarSoul
这个文件夹内存放Avatar实现的关键源代码及素材。
暂不提供部署，仅为学习交流。
MotionGPT: https://github.com/OpenMotionLab/MotionGPT
GPT-SoViT: https://github.com/RVC-Boss/GPT-SoVITS
## ChatGPT
这个文件夹存放着转发Openai接口的程序
- aichat.py   
## mgpt
- api.py MotionGPT的接口，以及部分Mapper的实现
- api_tools.py 可视化旋转矩阵的工具
- web_rendered 部分生成数据直接plot的结果
## Unity
unity端的关键代码
📦Unity
 ┣ 📜AvatarControl.cs             将节点直接可视化          
 ┣ 📜CameraController.cs          玩家视角控制
 ┣ 📜HTTP.cs                      HTTP请求类的实现
 ┣ 📜Manager.cs                   Manager模块
 ┣ 📜Mapper.cs                    Mapper模块的后半部分实现
 ┣ 📜PlayerMove.cs                玩家控制
 ┣ 📜socketV2.cs                  WebSocket，和MotionGPT进行Socket通信
 ┗ 📜UImanager.cs                 UI界面
