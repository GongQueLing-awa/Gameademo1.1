//using UnityEngine;

//public class Bullet : MonoBehaviour
//{
//    public float speed = 5f;
//    private Animator anim;
//    private bool alreadyHit;
//    public ShootController shootController;

//    void Start()
//    {
//        anim = GetComponent<Animator>();
//        shootController = GetComponent<ShootController>();
//    }

//    void Update()
//    {
//        if (shootController.yesShoot)
//        {
//            // 子弹一直往右（或往左）飞，每一帧走 speed * 时间
//            // transform.right 方向自动对：敌人朝右子弹就朝右飞
//            transform.position += transform.right * speed * Time.deltaTime;

//            // 飞出屏幕就销毁
//            if (Camera.main != null)
//            {
//                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
//                if (screenPos.x < -50 || screenPos.x > Screen.width + 50 ||
//                    screenPos.y < -50 || screenPos.y > Screen.height + 50)
//                {
//                    Destroy(gameObject);
//                }
//            }
//        }
//    }

//    // 子弹碰到别的Collider2D（因为自己是Trigger，所以触发这个函数）
//    void OnTriggerEnter2D(Collider2D other)
//    {
//        if (alreadyHit) return;

//        // 碰到玩家（玩家在 layer 7）
//        if (other.gameObject.layer == 7)
//        {
//            alreadyHit = true;

//            // 触发你在 Animator 里设好的 isTouch，播放消散动画
//            if (anim != null) anim.SetTrigger("isTouch");

//            // 关掉碰撞体，免得消散过程中又碰一次
//            CircleCollider2D col = GetComponent<CircleCollider2D>();
//            if (col != null) col.enabled = false;

//            // 等 0.3 秒让消散动画播完，再销毁自己
//            Destroy(gameObject, 0.3f);
//        }
//    }
//}
