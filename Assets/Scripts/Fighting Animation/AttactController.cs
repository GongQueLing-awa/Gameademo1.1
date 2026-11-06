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

    private void PlayerAttack(InputAction.CallbackContext obj)
    {
        isAttack = true;
        attactAnimation.PlayerAttack();
        
    }
}
