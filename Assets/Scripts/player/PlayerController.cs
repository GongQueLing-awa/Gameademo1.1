using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;


public class PlayerController : MonoBehaviour
{
    public PlayerInputControl inputControl;
    public Vector2 inputDirection;
    public float speed;
    public float jumpForce;//More More Jump!
    public Rigidbody2D rb;
    public PhysicalCheck physicalCheck;
    public float hurtForce;
    public bool isHurt=false;
    public bool isDead;
    public AudioClip jumpAudio;
    public PlayAudioEventSO fxAudioEvent;

    private CapsuleCollider2D coll;
   

    [Header("物理参数")]
    public PhysicsMaterial2D normal;
    public PhysicsMaterial2D wall;
    private void Awake()
    {
        physicalCheck = GetComponent<PhysicalCheck>();
        coll = GetComponent<CapsuleCollider2D>();
        inputControl = new PlayerInputControl();
        inputControl.Gameplay.Jump.started += Jump;
        

    }

    private void OnEnable()
    {
        inputControl.Enable();
    }

    private void OnDisable()
    {
        inputControl.Disable();
    }

    private void OnDestroy()
    {
        // 退订并释放自己创建输入实例，避免残留订阅和原生资源
        if (inputControl == null)
        {
            return;
        }

        inputControl.Gameplay.Jump.started -= Jump;
        inputControl.Dispose();
    }


    //以下是角色移动与翻转
    private void Update()
    {
     
        inputDirection = inputControl.Gameplay.Move.ReadValue<Vector2>();
        CheckState();

    }
   
private void FixedUpdate()
    {
        if (!isHurt/*&&attactController.isAttack!=true*/)   
        {
            Move();
        }
    }
    private void Move()
    {
        rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime, rb.velocity.y);
        int faceDir = (int)transform.localScale.x;
        if (inputDirection.x > 0)
        {
            faceDir = 2;
        }
        if (inputDirection.x < 0)
        {
            faceDir = -2;
        }

        transform.localScale = new Vector3(faceDir, 2, 2);
    }
    private void Jump(InputAction.CallbackContext obj)
    {
        if (physicalCheck.isGround)
        {

            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);

            // 统一走 FX 事件通道，由 AudioManager 负责真正播放
            if (fxAudioEvent != null && jumpAudio != null)
            {
                fxAudioEvent.RaiseEvent(jumpAudio);
            }

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
