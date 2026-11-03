using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    public Animator anim;
    public bool isAwake=false;
    public bool isDead;
 
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayerNearMe()
    {
        if (!isAwake)
        {
            anim.SetBool("isAwake",true);

        }
    }
    public void BeDead()
    {
        anim.SetBool("isDead", true);
    }
    
    public void BeHurt()
    {
        anim.SetTrigger("Hurt");
    }

}
