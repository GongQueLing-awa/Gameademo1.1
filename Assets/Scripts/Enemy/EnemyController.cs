using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    Rigidbody2D rb;
    public Transform playerTransform;
    public EnemyAnimation enemyAnimation;
   
    
    [Header("基本参数")]
    public float normalSpeed;
    public float chaseSpped;
    public float detectionRange = 0.09f;

    private void Awake()
    {
        
        GameObject playerObj = GameObject.Find("player");
        if(playerObj != null)
        {
            playerTransform= playerObj.transform;
        }
        else
        {
            Debug.LogWarning("未找到player！");
        }




        enemyAnimation=GetComponent<EnemyAnimation>();
       
        rb = GetComponent<Rigidbody2D>();
       
    }

    private void Update()
    {
        if (playerTransform != null) {
            float distance=Vector2.Distance(playerTransform.position, transform.position);
            if (distance< detectionRange)
            {
                enemyAnimation.PlayerNearMe();
            }
        }
      
       
    }
    public void PlayerDead()
    {
        enemyAnimation.BeDead();
        //inputControl.Gameplay.Disable();后面咱再研究移动的事
    }
}
