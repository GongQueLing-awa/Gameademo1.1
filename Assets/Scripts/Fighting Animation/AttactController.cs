using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttactController : MonoBehaviour
{
    private AttactAnimation attactAnimation;
    public bool isAttack;

    public PlayerInputControl inputControl;//隔壁键位输入调一下
    private void Awake()
    {
        attactAnimation = GetComponent<AttactAnimation>();
        inputControl = new PlayerInputControl();//程序还没开始我就醒力,键位快给我工作
        inputControl.Gameplay.Attack.started += PlayerAttack;
    }
    private void OnEnable()
    {
        inputControl.Enable();//Player你还活着我这个键位就干活
    }

    private void OnDisable()
    {
        inputControl.Disable();//欸你怎么亖了，那我开摆了
    }

    private void PlayerAttack(InputAction.CallbackContext obj)
    {
        isAttack = true;
        attactAnimation.PlayerAttack();
        
    }
}
