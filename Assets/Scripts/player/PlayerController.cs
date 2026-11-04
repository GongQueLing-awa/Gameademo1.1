using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : MonoBehaviour
{
    public PlayerInputControl inputControl;//隔壁键位输入调一下
    public Vector2 inputDirection;//二维向量
    public float speed;//移动数据工具人
    public float jumpForce;//More More Jump!
    public Rigidbody2D rb;//尝试导入刚体
    public PhysicalCheck physicalCheck;
    public float hurtForce;
    public bool isHurt=false;
    public bool isDead;
    
    private CapsuleCollider2D coll;
    //private AttactController attactController;

    [Header("物理参数")]
    public PhysicsMaterial2D normal;
    public PhysicsMaterial2D wall;
    private void Awake()
    {
        physicalCheck = GetComponent<PhysicalCheck>();
        coll = GetComponent<CapsuleCollider2D>();
        inputControl = new PlayerInputControl();//程序还没开始我就醒力,键位快给我工作
        inputControl.Gameplay.Jump.started += Jump;
        //attactController = GetComponent<AttactController>();

    }

    private void OnEnable()
    {
        inputControl.Enable();//Player你还活着我这个键位就干活
    }

    private void OnDisable()
    {
        inputControl.Disable();//欸你怎么亖了，那我开摆了
    }


    //以下是角色移动与翻转
    private void Update()
    {
     
        inputDirection = inputControl.Gameplay.Move.ReadValue<Vector2>();//把你键位读的数据用二维向量抢过来
        CheckState();

    }
   
private void FixedUpdate()
    {
        if (!isHurt/*&&attactController.isAttack!=true*/)   //MD怎么动不了了
        {
            Move();//稳定地运行一下下面的函数
        }
    }
    private void Move()
    {
        rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime, rb.velocity.y);//读一下键位输入的刚体速度，给我捏一个*比划*这样的二维向量
        int faceDir = (int)transform.localScale.x;//临时设立一个初始化变量，用来翻转角色
        if (inputDirection.x > 0)
        {
            faceDir = 2;//手柄每次移动可能是加0.几的，为了避免人物缩放不合适咱们用if统一框定向左或向右
        }
        if (inputDirection.x < 0)
        {
            faceDir = -2;//同上
        }

        transform.localScale = new Vector3(faceDir, 2, 2);//应用这个翻转
    }
    private void Jump(InputAction.CallbackContext obj)
    {
        if (physicalCheck.isGround)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);//Jump小代码
        }
        
    }
    public void GetHurt(Transform attacker)
    {
        isHurt = true;
        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2((transform.position.x - attacker.position.x), 0);

        rb.AddForce(dir * hurtForce, ForceMode2D.Impulse);
    }
    public void PlayerDead()
    {
        isDead=true;
        inputControl.Gameplay.Disable();
    }
    private void CheckState()
    {
        coll.sharedMaterial = physicalCheck.isGround?normal:wall;
    }


}
