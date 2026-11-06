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
    }
    

    public void ShootFire()
    {
        
        alreadyHit = false;
        if (shootOrigin == null || player == null) return;
        if (transform.Find("shoot") != null) { 
            shootOrigin.position= transform.Find("shoot").position;
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