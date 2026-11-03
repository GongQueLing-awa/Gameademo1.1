using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttactAnimation : MonoBehaviour
{
    private Animator anim;
    private AttactController attactController;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        attactController= GetComponent<AttactController>();
    }
    public void PlayerAttack()
    {
        anim.SetTrigger("Attack");
    }
    private void Update()
    {
        SetAnimation();
    }
    public void SetAnimation()
    {
        anim.SetBool("isAttack", attactController.isAttack);
    }
}
