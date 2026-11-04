using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//shoot到时候直接写一个publictransform把shoot拖进来就行的话要不把shoot身上的shootcontroller给ban了，不不不你直接把shootAnimator拖进smallEnemy的ShootController里就行了朋友
public class ShootController : MonoBehaviour
{
    public Animator anim;
    private void Awake()
    {
        anim= GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ShootFire()//通知爆弹移动出枪的函数，记得在EnemyCotroller挂的事件Shoot上挂这个Fire公共函数
    {
        //Debug.LogWarning("testtest");
       
    }
}
