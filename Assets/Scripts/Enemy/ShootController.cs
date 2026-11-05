using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//shoot到时候直接写一个publictransform把shoot拖进来就行的话要不把shoot身上的shootcontroller给ban了，不不不你直接把shootAnimator拖进smallEnemy的ShootController里就行了朋友
//shoot的检测整普通检测就行，Shoot又不掉血没必要挂Character
public class ShootController : MonoBehaviour
{
    public Animator anim;
    public CircleCollider2D circleCollider;

    public GameObject shootPrefab;
    public Transform shootOrigin;
    public float bulletSpeed = 20f; // 飞行速度（像素/秒）
    public float maxDistance = 30f; // 飞行最远距离

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();

    }

    public void ShootFire()//通知爆弹移动出枪的函数，记得在EnemyCotroller挂的事件Shoot上挂这个Fire公共函数
    {
        anim.SetTrigger("shoot");
        

       

     }
}






























//if (shootPrefab == null) return;
// 实例化子弹（在敌人当前的位置生成）

//    GameObject bullet = Instantiate(shootPrefab, shootOrigin.position, shootOrigin.rotation);
//    bullet.layer = this.gameObject.layer;

//    // 判断发射方向 (根据敌人的朝向)
//    int direction = transform.localScale.x >= 0 ? -1 : 1;

//    // 启动协程，让子弹动起来
//    StartCoroutine(MoveBullet(bullet, direction));
//}

//// 负责让子弹移动并定时销毁
//private IEnumerator MoveBullet(GameObject bullet, int direction)
//{

//    Vector3 startPos = bullet.transform.position;

//    while (bullet != null)
//    {
//        // 1. 计算每帧应该移动的距离 = 速度 * 时间(每秒帧数平滑) * 方向
//        bullet.transform.Translate(Vector3.right * bulletSpeed * Time.deltaTime * direction);

//        // 2. 检查是否飞出了最大距离
//        if (Vector3.Distance(bullet.transform.position, startPos) > maxDistance)
//        {
//            Destroy(bullet); // 删除子弹
//            yield break;     // 退出协程，释放内存
//        }

//        yield return null; // 等待下一帧

//        Debug.Log("【第一步】ShootFire 函数被成功调用！");

//        // 1. 检查预制体是否为空
//        if (shootPrefab == null)
//        {
//            Debug.LogError("【失败原因】shootPrefab 为空！你没有在 Inspector 里拖入子弹预制体！");
//            return;
//        }

//        // 2. 检查发射原点是否为空
//        if (shootOrigin == null)
//        {
//            Debug.LogError("【失败原因】shootOrigin 为空！你没有把子物体 Shoot 拖到 Shoot Origin 槽位里！");
//            return;
//        }

//        Debug.Log("【第二步】预制体和发射原点检查通过，准备生成子弹...");

//        // 3. 生成子弹
//        GameObject bullet = Instantiate(shootPrefab, shootOrigin.position, shootOrigin.rotation);

//        // 4. 打印生成结果
//        if (bullet != null)
//        {
//            bullet.layer = this.gameObject.layer;
//            Debug.Log("【第三步】子弹生成成功！名称: " + bullet.name + " 位置: " + bullet.transform.position);

//            int direction = transform.localScale.x >= 0 ? 1 : -1;
//            StartCoroutine(MoveBullet(bullet, direction));
//        }
//        else
//        {
//            Debug.LogError("【致命错误】Instantiate 返回了空值！请检查你的 bulletPrefab 是否是真正的 Prefab！");
//        }








