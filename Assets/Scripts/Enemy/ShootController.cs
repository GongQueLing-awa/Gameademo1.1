//using System.Collections;
//using UnityEngine;

//public class ShootController : MonoBehaviour
//{
//    public Animator anim;
//    public CircleCollider2D circleCollider;

//    [Header("Shoot 飞行参数")]
//    public GameObject shootPrefab;
//    public Transform shootOrigin;
//    public float bulletSpeed ; // 飞行速度（单位/秒）
//    public float maxDistance = 30f; // 没有相机时的兜底飞行距离

//    private Camera mainCamera;

//    private void Awake()
//    {
//        circleCollider = GetComponent<CircleCollider2D>();
//        mainCamera = Camera.main;
//    }

//    // 动画事件 ShootFire：smallenemy 战斗动画播放到开枪那一帧时调用
//    public void ShootFire()
//    {
//        if (shootPrefab == null)
//        {
//            return;
//        }

//        // 克隆动画里挂着的 shoot 作为独立飞行体。
//        // 原对象继续留在 smallenemy 的动画层级里，所以销毁子弹不会影响战斗动画；
//        // 每次开枪都是新的克隆体，因此可以同时存在多颗子弹。
//        GameObject bullet = Instantiate(shootPrefab);
//        bullet.transform.SetParent(null, true); // 脱离 smallenemy，保留世界缩放
//        bullet.SetActive(true);                 // shoot 在动画里可能是 inactive 的

//        // 从枪口（shoot 当前所在位置）飞出
//        Transform origin = shootOrigin != null ? shootOrigin : shootPrefab.transform;
//        bullet.transform.position = origin.position;

//        // shoot 的 worldScale.x 与 smallenemy 的 localScale.x 相反：
//        // 敌人朝右时 localScale.x = -1，shoot 的 worldScale.x = +1，视觉上同样朝右。
//        // 所以飞行方向取敌人的视觉朝向（-localScale.x），不能直接拿 shoot.localScale.x。
//        int dir = transform.localScale.x > 0f ? -1 : 1;

//        StartCoroutine(MoveBullet(bullet, dir));
//    }

//    // 每颗子弹一个协程：固定速度、固定方向，互不干扰
//    private IEnumerator MoveBullet(GameObject bullet, int dir)
//    {
//        Vector3 startPosition = bullet.transform.position;

//        while (bullet != null)
//        {
//            // 沿世界 X 轴水平飞行，不受 smallenemy 翻转缩放的影响
//            bullet.transform.position += Vector3.right * (dir * bulletSpeed * Time.deltaTime);

//            if (IsCompletelyOutOfView(bullet) ||
//                (mainCamera == null &&
//                 Vector3.Distance(bullet.transform.position, startPosition) >= maxDistance))
//            {
//                // 只销毁飞行用的克隆体；smallenemy 动画里挂着的 shoot 原对象保持不动
//                Destroy(bullet);
//                yield break;
//            }

//            yield return null;
//        }
//    }

//    // 判断 shoot 是否已经完全离开玩家视野（相机画面）
//    private bool IsCompletelyOutOfView(GameObject bullet)
//    {
//        if (mainCamera == null)
//        {
//            return false;
//        }

//        Renderer bulletRenderer = bullet.GetComponent<Renderer>();
//        if (bulletRenderer == null)
//        {
//            return false;
//        }

//        Bounds bounds = bulletRenderer.bounds;
//        Vector3 screenMin = mainCamera.WorldToScreenPoint(bounds.min);
//        Vector3 screenMax = mainCamera.WorldToScreenPoint(bounds.max);

//        // 飞到相机背后
//        if (screenMin.z < 0f || screenMax.z < 0f)
//        {
//            return true;
//        }

//        float left = Mathf.Min(screenMin.x, screenMax.x);
//        float right = Mathf.Max(screenMin.x, screenMax.x);
//        float bottom = Mathf.Min(screenMin.y, screenMax.y);
//        float top = Mathf.Max(screenMin.y, screenMax.y);

//        // 整个 sprite 都越过某一条屏幕边界，才算“完全”飞出视野
//        return right < 0f || left > mainCamera.pixelWidth
//            || top < 0f || bottom > mainCamera.pixelHeight;
//    }
//}




using UnityEngine;
//1.子弹初始坐标跟随shoot
//2.只变X轴
//3.不要动画一波完就消失

public class ShootController : MonoBehaviour
{

    private EnemyController enemyController;

    [Header("发射设置")]
    public Transform shootOrigin; // 拖拽子物体 shootClone 到这里
    public float moveSpeed = 5f;  // 子弹移动速度
    private bool alreadyHit;
    public Animator anim;
    private Vector3 direction;

   [Header("活动范围设置 (X轴和Y轴的最大最小值)")]
    public float minX = -90f;     // 左边界
    public float maxX = 90f;      // 右边界
    public float minY = -5f;      // 下边界
    public float maxY = 5f;       // 上边界

    [Header("引用")]
    public Transform player;      // 拖拽玩家到这里

    private bool isRunning = false; // 标记子弹是否正在跑动

   void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }


    public void ShootFire()
    {
        alreadyHit = false;
        if (shootOrigin == null || player == null) return;
        if (transform.Find("shoot") != null) { 
            shootOrigin.position= transform.Find("shoot").position;
        }

        //  激活子物体（确保它是可见的）
        shootOrigin.gameObject.SetActive(true);



        // 计算从子弹指向玩家的方向
        
        isRunning = true;
        direction = (player.position - shootOrigin.position).normalized;
        int dir = transform.localScale.x > 0f ? -1 : 1;
    }

    void Update()
    {
        if (isRunning)
        {
            Moveto();
            CheckRange();
        }
    }

   
    void Moveto()
    {
        //// 计算从子弹指向玩家的方向
        //Vector3 direction = (player.position - shootOrigin.position).normalized;

        // 移动子物体
        float moveX = direction.x * moveSpeed * Time.deltaTime;
        shootOrigin.position += new Vector3(moveX, 0, 0);
    }

    
    void CheckRange()
    {
        Vector3 currentPos = shootOrigin.position;

        // 只要X或Y坐标超出了你设定的最大/最小值，就算“出界”
        if (currentPos.x < minX || currentPos.x > maxX ||
            currentPos.y < minY || currentPos.y > maxY)
        {
           
            // 停止跑动
            isRunning = false;
            // 刷新回原位
            // 隐藏它，等待下一次发射
            shootOrigin.gameObject.SetActive(false);
            if (transform.Find("shoot") != null)
                shootOrigin.position = transform.Find("shoot").position;
            isRunning = false;
        }
    }


    public void imTrying(Collider2D other)
    {
        if (alreadyHit) return;

        // 碰到玩家（玩家在 layer 7）
        if (other.gameObject.layer == 7)
        {
            alreadyHit = true;
            // 触发你在 Animator 里设好的 isTouch，播放消散动画
            if (anim != null) anim.SetTrigger("isTouch");

            // 关掉碰撞体，免得消散过程中又碰一次
            // CircleCollider2D col = GetComponent<CircleCollider2D>();
            // if (col != null) col.enabled = false;

            isRunning = false;
            // 隐藏它，等待下一次发射
            shootOrigin.gameObject.SetActive(false);

            if (transform.Find("shoot") != null)
                shootOrigin.position = transform.Find("shoot").position;

            isRunning = false;

        }
    }
}