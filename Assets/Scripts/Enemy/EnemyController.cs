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
    public UnityEvent Shoot;
    [Header("基本参数")]
    public float normalSpeed;
    public float detectionRange = 8f;
    private float distanceNear = 3.5f;
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
            if (distance< detectionRange&&!enemyAnimation.isAwake)
            {
                enemyAnimation.PlayerNearMe();
               
            }
            if (enemyAnimation.isAwake)
            {
                if (distance < distanceNear)
                {
                    enemyAnimation.BeNear();

                }
                if (!enemyAnimation.isNear && !enemyAnimation.isDead)//bro、怎么加hurt条件不能动比较体面，，，
                {
                    MoveToPlayer();
                }

                
            }
        }
      
       
    }
    public void EnemyDead()
    {
        enemyAnimation.BeDead();
        //inputControl.Gameplay.Disable();后面咱再研究移动的事
    }
    public void MoveToPlayer()
    {
        FacePlayer();
        Vector2 direction=(playerTransform.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, normalSpeed * Time.deltaTime);//记得问问理解
    }
    public void FacePlayer()
    {
        float testDistance = playerTransform.position.x - transform.position.x;
        int faceDir = (int)transform.localScale.x;//临时设立一个初始化变量，用来翻转角色
        if (testDistance > 0)
        {
            faceDir = 1;//手柄每次移动可能是加0.几的，为了避免人物缩放不合适咱们用if统一框定向左或向右
        }
        if (testDistance < 0)
        {
            faceDir = -1;//同上
        }

        transform.localScale = new Vector3(-faceDir, 1, 1);//应用这个翻转
    }
}
