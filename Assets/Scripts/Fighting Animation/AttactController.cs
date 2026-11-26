using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MyGame.UI;

public class AttactController : MonoBehaviour
{
    private AttactAnimation attactAnimation;
    public bool isAttack;

    // 攻击技能是否已经通过石碑谜题解锁。默认 false = 锁住
    public static bool attackUnlocked = false;

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
        // 玩家正在剧情里打字时，忽略攻击键
        if (DramaInputManager.isTyping) return;

        // 石碑谜题还没解锁攻击技能时，按攻击键没反应
        if (!attackUnlocked) return;

        isAttack = true;
        attactAnimation.PlayerAttack();
        
    }
}
