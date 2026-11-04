using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//朋友现在还有两个问题需要解决
//第一个，isNear布尔值改trigger的问题
//第二个，shoot到时候直接写一个publictransform把shoot拖进来就行的话要不把shoot身上的shootcontroller给ban了，不不不你直接把shootAnimator拖进smallEnemy的ShootController里就行了朋友
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
    public void ShootFire()//通知爆弹移动出枪的函数，到时候给搬家搬到ShootController里面，记得在EnemyCotroller挂的事件Shoot上挂这个Fire公共函数
    {
        Debug.LogWarning("testtest");
    }
}
