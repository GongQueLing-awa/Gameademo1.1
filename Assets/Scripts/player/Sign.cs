using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sign : MonoBehaviour
{
    private const string k_interactableTag = "interactable";
    private const string k_isTabletParam = "isTablet";

    public PlayerInputControl inputControl;
    public GameObject signSprite;
    public Transform playerTransform;

    private Animator m_anim;
    private SpriteRenderer m_signRenderer;
    private IInteractable m_targetItem;
    // 记录当前还在触发器里的 interactable 碰撞体，
    // 这样其中一个离开时不会把另一个仍在范围内的目标错误清掉
    private readonly HashSet<Collider2D> m_overlaps = new();

    private void Awake()
    {
        inputControl = new PlayerInputControl();
        m_anim = signSprite.GetComponent<Animator>();
        m_signRenderer = signSprite.GetComponent<SpriteRenderer>();
        inputControl.Gameplay.Confirm.started += PlayerConfirm;
        inputControl.Enable();
    }

    private void OnDestroy()
    {
        inputControl.Gameplay.Confirm.started -= PlayerConfirm;
        inputControl.Dispose();
    }

    private void Update()
    {
        m_signRenderer.enabled = m_targetItem != null;

        float direction = playerTransform.localScale.x > 0 ? 1f : -1f;
        signSprite.transform.localScale = new Vector3(0.5f * direction, 0.5f, 0.5f);
    }

    private void PlayerConfirm(InputAction.CallbackContext obj)
    {
        // 目标只可能来自 RefreshTarget，按下时直接调用即可
        if (m_targetItem == null) return;

        m_targetItem.TriggerAction();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(k_interactableTag)) return;

        if (m_overlaps.Add(other))
        {
            RefreshTarget();
        }
    }

    // 每帧兜底：物体在玩家进入之后才补上 IInteractable 的情况也能及时拿到引用
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(k_interactableTag)) return;

        if (m_overlaps.Add(other) || m_targetItem == null)
        {
            RefreshTarget();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(k_interactableTag)) return;

        if (m_overlaps.Remove(other))
        {
            RefreshTarget();
        }
    }

    // 在仍在范围内的可交互物里选最近的一个作为当前目标
    private void RefreshTarget()
    {
        // 被销毁的碰撞体会变成 null，顺便清出集合
        m_overlaps.RemoveWhere(collider => collider == null);

        IInteractable nearest = null;
        float nearestSqrDistance = float.MaxValue;
        Vector2 origin = transform.position;

        foreach (Collider2D collider in m_overlaps)
        {
            // 交互组件可能挂在碰撞体的父物体上，所以往上找
            IInteractable interactable = collider.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float sqrDistance = ((Vector2)collider.transform.position - origin).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance) continue;

            nearestSqrDistance = sqrDistance;
            nearest = interactable;
        }

        SetTarget(nearest);
    }

    private void SetTarget(IInteractable interactable)
    {
        if (ReferenceEquals(interactable, m_targetItem)) return;

        m_targetItem = interactable;
        m_anim.SetBool(k_isTabletParam, interactable != null);
    }
}
