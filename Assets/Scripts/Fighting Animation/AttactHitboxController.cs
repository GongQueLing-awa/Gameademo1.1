using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitboxController : MonoBehaviour

{  
   public PolygonCollider2D hitbox;
    
   
   public void TurnOnHitbox()   {   
        if (hitbox != null)
        {    
            hitbox.enabled = true; 
         }
    } 
      public void TurnOffHitbox(){  if (hitbox != null)
        {  
            hitbox.enabled = false; 
           }
    }
}

