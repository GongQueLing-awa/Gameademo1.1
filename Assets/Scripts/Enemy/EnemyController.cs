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
    public bool coolover;
    public UnityEvent Shoot;
    [Header("基本参数")]
    public float normalSpeed;
    public float detectionRange = 8f;
    public float distanceNear = 3.5f;
    private float lastAttackTime = 0f;
    public float attackCooldown;
    private void Awake()
    {

        GameObject playerObj = GameObject.Find("player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("未找到player！");
        }




        enemyAnimation = GetComponent<EnemyAnimation>();

        rb = GetComponent<Rigidbody2D>();

    }

    private void Update()//请记得认真阅读以下文章
    {
        if (playerTransform != null)
        {
            float distance = Vector2.Distance(playerTransform.position, transform.position);
            if (distance < detectionRange && !enemyAnimation.isAwake)
            {
                enemyAnimation.PlayerNearMe();

            }
            if (enemyAnimation.isAwake)
            {

                float timeSinceLastAttack = Time.time - lastAttackTime;
                if (distance < distanceNear && timeSinceLastAttack > attackCooldown)
                {
                    enemyAnimation.BeNear();
                    lastAttackTime = Time.time;
                    coolover = true;
                }
                else if (!enemyAnimation.isNear && !enemyAnimation.isDead/*&&timeSinceLastAttack<attackCooldown*/)//刚刚改了这里
                {
                    MoveToPlayer();
                }
                if(timeSinceLastAttack < attackCooldown)
                {
                    coolover=false;
                }


            }
        }


    }
    public void EnemyDead()
    {
        enemyAnimation.BeDead();

    }
    public void MoveToPlayer()
    {
        FacePlayer();
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, normalSpeed * Time.deltaTime);//记得问问理解
    }
    public void FacePlayer()
    {
        float testDistance = playerTransform.position.x - transform.position.x;
        int faceDir = (int)transform.localScale.x;//临时设立一个初始化变量，用来翻转角色
        if (testDistance > 0)
        {
            faceDir = 1;

        }
         if (testDistance < 0)
         {
            faceDir = -1;
         }

            transform.localScale = new Vector3(-faceDir, 1, 1);//应用这个翻转
        
    }
}
