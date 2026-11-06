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

    
    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChange?.Invoke(this);
    }
    public UnityEvent<Character> OnHealthChange;
    private void Update()
    {
        if (wudiIf)
        {
            wudiCounter -= Time.deltaTime;
            



            //下面执行受伤

            if (wudiCounter <= 0)
            {
                wudiIf = false;
               
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


        OnHealthChange?.Invoke(this);

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
