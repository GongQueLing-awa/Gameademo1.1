using UnityEngine;


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
    public float minX=-45f ;     // 左边界
    public float maxX=45f  ;      // 右边界
    public float minY=-5f ;      // 下边界
    public float maxY=5f;       // 上边界

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

        shootOrigin.localScale = new Vector3(-Mathf.Abs(shootOrigin.localScale.x), Mathf.Abs(shootOrigin.localScale.y), Mathf.Abs(shootOrigin.localScale.z));
        //  激活子物体（确保它是可见的）
        shootOrigin.gameObject.SetActive(true);



        // 计算从子弹指向玩家的方向
        
        isRunning = true;
        direction = new Vector3(transform.localScale.x>0f?-1f:1f,0f,0f);
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
            Debug.Log("stop");
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
            

            // 关掉碰撞体，免得消散过程中又碰一次
            // CircleCollider2D col = GetComponent<CircleCollider2D>();
            // if (col != null) col.enabled = false;

            isRunning = false;
            // 隐藏它，等待下一次发射
            shootOrigin.gameObject.SetActive(false);

            if (transform.Find("shoot") != null)
                shootOrigin.position = transform.Find("shoot").position;
            Debug.Log("stop");
            isRunning = false;

        }
    }
}