using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.UI;

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

            // 怪物苏醒（播 EnemyAwake）时，解锁书里的"怪物"词条
            MyGame.UI.DramaInputManager.UnlockWord("guaiwu");
        }
    }
    
    public void BeDead()
    {
        anim.SetBool("isDead", true);
        isDead= true;

        // 怪物被杀死时，解锁书里的"消失"词条
        MyGame.UI.DramaInputManager.UnlockWord("xiaoshi");
    }
    
    public void BeHurt()
    {
        anim.SetTrigger("Hurt");
    }
    public void BeNear()
    {
       
        anim.SetBool("isNear",true);
        
        isNear=true;
       
    }
    public void FinishNear()
    {
        anim.SetBool("isNear", false);
        isNear = false;
      
    }
   
    
}
