using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitboxController : MonoBehaviour

{  
   public PolygonCollider2D hitbox;
    
   
   public void TurnOnHitbox()   {   
        if (hitbox != null)
        {    
            hitbox.enabled = true; // 开启碰撞判定
         }
    } 
      public void TurnOffHitbox(){  if (hitbox != null)
        {  
            hitbox.enabled = false; // 关闭碰撞判定
           }
    }
}

