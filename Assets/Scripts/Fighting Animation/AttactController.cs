using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttactController : MonoBehaviour
{
    private AttactAnimation attactAnimation;
    public bool isAttack;

    public PlayerInputControl inputControl;
    private void Awake()
    {
        attactAnimation = GetComponent<AttactAnimation>();
        inputControl = new PlayerInputControl();
        inputControl.Gameplay.Attack.started += PlayerAttack;
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

        inputControl.Gameplay.Attack.started -= PlayerAttack;
        inputControl.Dispose();
    }

    private void PlayerAttack(InputAction.CallbackContext obj)
    {
        isAttack = true;
        attactAnimation.PlayerAttack();
        
    }
}
