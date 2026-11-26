using UnityEngine;

public class TabletTouch : MonoBehaviour, IInteractable
{
    [Header("这块石碑的信息")]
    [Tooltip("答案里用的名字，比如 Tabletsmall。答案里的边必须用这个名字")]
    public string tabletName;

    [Tooltip("石碑亮光 Light 的中心点，把 Light 物体拖进来")]
    public Transform lightPoint;

    [Tooltip("石碑的 Animator。留空会自动往自己或父物体上找")]
    public Animator tabletAnim;

    [Tooltip("是不是这一轮第一块被按下的石碑，方便在 Inspector 里观察")]
    public bool isFirst;

    [Tooltip("成功动画开始多少秒后，把 HaveAcceed 关回去")]
    public float haveAcceedResetTime = 0.5f;

    private void Awake()
    {
        // 动画器可能挂在父物体上，自己身上没有就往上找
        if (tabletAnim == null)
        {
            tabletAnim = GetComponentInParent<Animator>();
        }

        if (tabletAnim == null)
        {
            Debug.LogError($"[{name}] 没找到 Animator，请把 Animator 拖到 tabletAnim 上", this);
        }
    }

    public void TriggerAction()
    {
        // 场景里没放管理器就什么都不做，避免空引用
        if (TabletLineManager.instance == null)
        {
            Debug.LogWarning("场景里没有 TabletLineManager，石碑没法连线", this);
            return;
        }

        TabletLineManager.instance.TabletPressed(this);
    }

    // 点亮这块石碑
    public void WakeUp()
    {
        if (tabletAnim == null) return;

        // HaveAcceed 只用来播一次成功动画，点亮前先关掉，
        // 不然会一直停在上一次的成功状态里
        tabletAnim.SetBool("HaveAcceed", false);
        tabletAnim.SetBool("isAwake", true);
    }

    public void SetFirst(bool value)
    {
        isFirst = value;

        if (tabletAnim != null)
        {
            tabletAnim.SetBool("isFirst", value);
        }
    }

    // 判定成功：播一次成功动画，然后把 HaveAcceed 关回去
    public void PlaySuccess()
    {
        if (tabletAnim == null) return;

        tabletAnim.SetBool("isAwake", false);
        tabletAnim.SetBool("HaveAcceed", true);

        // 先取消上一次还没执行的复位，避免连按时被旧的计时器提前关掉
        CancelInvoke(nameof(ResetHaveAcceed));
        // 等动画进入成功状态后再关掉，避免以后一直重复播成功动画
        Invoke(nameof(ResetHaveAcceed), haveAcceedResetTime);
    }

    private void ResetHaveAcceed()
    {
        if (tabletAnim == null) return;
        tabletAnim.SetBool("HaveAcceed", false);
    }

    // 判定失败：只关掉点亮状态
    public void PlayFail()
    {
        if (tabletAnim == null) return;

        // 如果上一次的成功复位还没执行，先取消，
        // 不然过一会儿 HaveAcceed 又变回 true，会误播成功动画
        CancelInvoke(nameof(ResetHaveAcceed));
        tabletAnim.SetBool("HaveAcceed", false);
        tabletAnim.SetBool("isAwake", false);
    }

    public void ClearFirst()
    {
        SetFirst(false);
    }
}
