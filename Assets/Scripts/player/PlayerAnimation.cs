using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private PhysicalCheck PhysicalCheck;
    private PlayerController playController;
    private void Awake()
    {
        anim= GetComponent<Animator>();
        rb= GetComponent<Rigidbody2D>();
        PhysicalCheck = GetComponent<PhysicalCheck>();
        playController= GetComponent<PlayerController>();
    }
    void Update()
    {
        SetAnimation();
    }
    private void SetAnimation()
    {
        anim.SetFloat("velocityx", Mathf.Abs(rb.velocity.x));
        anim.SetFloat("velocityy", Mathf.Abs(rb.velocity.y));
        anim.SetBool("isGround", PhysicalCheck.isGround);
        anim.SetBool("isDead", playController.isDead);
    }
    public void PlayHurt()
    {
        anim.SetTrigger("Hurt");
    }
}
