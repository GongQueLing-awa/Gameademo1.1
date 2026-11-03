using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    public Animator anim;
    public bool isAwake=false;
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



}
