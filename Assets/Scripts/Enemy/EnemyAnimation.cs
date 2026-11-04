using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    public Animator anim;
    public ShootController shootController;
    public bool isAwake=false;
    public bool isDead=false;
    public bool isNear=false;
    
 
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayerNearMe()
    {
        if (!isAwake)
        {
            anim.SetBool("isAwake",true);
            isAwake= true;
        }
    }
    
    public void BeDead()
    {
        anim.SetBool("isDead", true);
        isDead= true;
    }
    
    public void BeHurt()
    {
        anim.SetTrigger("Hurt");
    }
    public void BeNear()
    {
       
        anim.SetBool("isNear", true);
        
        isNear=true;
       
    }
    public void FinishNear()
    {
       
        anim.SetBool("isNear", false);
        isNear = false;
      
    }
   
    public void ShootHeat()//通知爆弹打到了玩家的函数，播放消散动画
    {
        shootController.anim.SetTrigger("isTouch");
    }
}
