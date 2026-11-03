using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
public class Character : MonoBehaviour
{
    [Header("基本属性")]
    public float maxHealth;
    public float currentHealth;
    [Header("无敌属性")]
    public float wudiTime;
    private float wudiCounter;
    public bool wudiIf;
    [Header("受伤属性")]
    public UnityEvent<Transform> OnTakeDamage;
    public UnityEvent OnDie;

    //private SpriteRenderer spriteRenderer;
    //private void Awake()
    //{
    //    spriteRenderer= GetComponent<SpriteRenderer>();//加到这了
    //}

    private void Update()
    {
        if (wudiIf)
        {
            wudiCounter -= Time.deltaTime;
            //if (Time.time % 0.2f > 0.1f) {
            //    spriteRenderer.color = new Color(1f, 1f, 1f, 0.2f);

            //}
            //else
            //{
            //    spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            //}



            //下面执行受伤

            if (wudiCounter <= 0)
            {
                wudiIf = false;
                //spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }
    public void TakeDamage(Attack attacker)
    {
        if (wudiIf)
        {
            return;
        }
        if (currentHealth - attacker.damage > 0)
        {
            currentHealth -= attacker.damage;
            
            Triggerwudi();
            
            //执行受伤
            OnTakeDamage?.Invoke(attacker.transform);
        }
        else
        {
            currentHealth = 0;
            //西内！！！
            OnDie?.Invoke();
        }
    }
    public void Triggerwudi()
    {
        if (!wudiIf)
        {
            wudiIf= true;
            wudiCounter = wudiTime;
        }
    }
}
