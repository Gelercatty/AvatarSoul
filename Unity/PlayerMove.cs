using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
     // Update is called once per frame
    public float MoveSpeed;
    //用户在PlayerMove脚本界面可以对其进行调整
    //（用public定义，脚本外可以修改，并以脚本外修改的为主）
    //（如果是private类型则只能在脚本修改）
    
    //定义两个私有类型的浮点型数据horizontal和vertical来记录A、D键和
    //W、S键的数据，也可以理解为记录水平轴和竖直轴的数据，即X轴和Y轴 
    private float horizontal;
    private float vertical;
    
    //定义一个重力gravity 
    private float gravity = 9.8f;
    //定义一个起跳速度
    public float JumpSpeed = 15f;
    //定义一个公有类型的CharacterController，将其命名为PlayerController
    //用于实例化对象
    public UImanager UI;
    public CharacterController PlayerController;
    
    private Vector3 initPosition;
    void Start()
    {
        initPosition = transform.position; // position 是一个 Vector3 类型的变量，记录了物体在世界坐标系中的位置
        Debug.Log("initPosition: " + initPosition);
    }
    //定义一个Vector3
    //Vector3向量既可以用来表示位置，也可以用来表示方向
    //在立体三维坐标系中,分别取与x轴、y轴,z轴方向相同的3个单位向量i，j, k作为一组基底。
    //若a为该坐标系内的任意向量，以坐标原点O为起点作向量OP=a。由空间基本定理知，有且只有一组实数（x，y, z）
    //使得 a=向量OP=xi+yj+zk，因此把实数对（x，y, z）叫做向量a的坐标
    //记作a=（x，y, z）。这就是向量a的坐标表示。其中（x，y, z）,也就是点P的坐标。向量OP称为点P的位置向量。
    Vector3 Player_Move;
    //如果还是不理解的话可以去CSDN上详细了解，这里就不做过多说明了

    //放在Updata里面每一帧都会执行，导致不能够保存前一时刻的值   
    void Update()
    {
        if (UI.isInputFieldActive == true)
        {
            return;
        }
        //判断PlayerController是否在地面上，如果不是在地面上就不能够前后左右移动，也不能够起跳
        if(PlayerController.isGrounded)
        {
        //Input.GetAxis("Horizontal")为获取X(横轴)轴方向的值给horizontal
            horizontal = Input.GetAxis("Horizontal");
        //Input.GetAxis("Vertical")为获取Z(纵轴)轴方向的值给Vertical
            vertical = Input.GetAxis("Vertical");
            
        //transform为本物体所在位置，forward是向前的一个分量
        //transform.forward * vertical为物体向前的方向乘获取的Z轴的值，即物体向前移动的量，如果vertical为负数，则物体后退
        //transform.right * horizontal为物体向右的方向乘获取的X轴的值，即物体向右移动的量，如果horizon为负数，则物体向左
        //两者相加后最后面乘移动数度，赋值给Vector3类型的Player_Move即为物体实际运动的方向           
            Player_Move = (transform.forward * vertical + transform.right * horizontal) * MoveSpeed;

            //判断玩家是否按下空格键
            if (Input.GetAxis("Jump")==1)
            {
                // log the signal
                Debug.Log("Jump");
            //按下空格键，给其竖直方向添加一个向上的数度，使其跳起
            //Player_Move.y相当于Player_Move下的Vector3(0,1,0)
                Player_Move.y = Player_Move.y + JumpSpeed;
            }
        }
        
        //模拟重力下降的效果，让向上的量减去重力下降的量
        //变量deltaTime表示为unity本地变量，作为Time类中的数据在各帧中被更新，在各帧中，该变量显示了距上一帧所经历的时间值（以秒计算）。 
        //这个变量的优点： 使用这个函数他会与你的游戏帧速率无关 放在Update（）函数中的代码是以帧来执行的
        //我们需要将移动的物体乘以秒来执行，而乘以deltaTime其就可以实现 
        //例如：想让一个游戏对象向前以每秒10m移动的话 用你的速度10乘Time.deltaTime  他表示每秒移动的距离为10m而不是每帧10m      
        Player_Move.y = Player_Move.y - gravity * Time.deltaTime;
        
        //PlayerController下的.Move为实现物体运动的函数
        //Move()括号内放入一个Vector3类型的量，本例中为Player_Move
        PlayerController.Move(Player_Move*Time.deltaTime);




        // 如果掉下去了，就重置位置
        if (transform.position.y < -10)
        {
            transform.position = initPosition;
        }
    }

                        
}
