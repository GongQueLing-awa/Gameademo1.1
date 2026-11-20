using UnityEngine;


public class ShootController : MonoBehaviour
{

    private EnemyController enemyController;

    [Header("发射设置")]
    public Transform shootOrigin; // 拖拽子物体 shootClone 到这里
    public float moveSpeed = 5f;  
    private bool alreadyHit;
    public Animator anim;
    private Vector3 direction;

   [Header("活动范围设置 (X轴和Y轴的最大最小值)")]
    public float minX=-45f ;     
    public float maxX=45f  ;      
    public float minY=-5f ;      
    public float maxY=5f;       

    [Header("引用")]
    public Transform player;      

    private bool isRunning = false; 

   void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        
        // 如果 Inspector 中没有分配 player，尝试自动查找
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }
    
    public void ShootFire()
    {
        alreadyHit = false;

        // 检查关键引用
        if (shootOrigin == null)
        {
            Debug.LogError($"[ShootController] 错误: {gameObject.name} 的 shootOrigin 未分配！");
            return;
        }
        
        if (player == null)
        {
            // 再次尝试查找，以防 Awake 时没找到
            GameObject playerObj = GameObject.Find("player");
            if (playerObj != null) player = playerObj.transform;
            
            if (player == null)
            {
                Debug.LogError($"[ShootController] 错误: 找不到名为 'player' 的物体！");
                return;
            }
        }

        // 查找枪口位置
        Transform muzzle = transform.Find("shoot");
        if (muzzle != null) 
        { 
            shootOrigin.position = muzzle.position;
        }
        else
        {
            Debug.LogWarning($"[ShootController] 警告: 在 {gameObject.name} 下找不到名为 'shoot' 的子物体！");
        }

        shootOrigin.localScale = new Vector3(transform.localScale.x > 0f ? -1f : 1f, Mathf.Abs(shootOrigin.localScale.y), Mathf.Abs(shootOrigin.localScale.z));
       
        shootOrigin.gameObject.SetActive(true);



      
        
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
       
        float moveX = direction.x * moveSpeed * Time.deltaTime;
        shootOrigin.position += new Vector3(moveX, 0, 0);
    }

    
    void CheckRange()
    {
        Vector3 currentPos = shootOrigin.position;

        
        if (currentPos.x < minX || currentPos.x > maxX ||
            currentPos.y < minY || currentPos.y > maxY)
        {
           
           
            isRunning = false;
            Debug.Log("stop");
            
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

            other.GetComponent<Character>()?.TakeDamage(GetComponent<Attack>());

            

            isRunning = false;
           
            shootOrigin.gameObject.SetActive(false);

            if (transform.Find("shoot") != null)
                shootOrigin.position = transform.Find("shoot").position;
            Debug.Log("stop");
            isRunning = false;

        }
    }
}