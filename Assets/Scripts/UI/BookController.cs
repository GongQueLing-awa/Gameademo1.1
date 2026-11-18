using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MyGame.UI
{
    /// <summary>
    /// 管理书本 UI 的交互逻辑，包括进入/退出书本状态、游戏暂停、动画播放等。
    /// </summary>
    public class BookController : MonoBehaviour
    {
        [Header("UI 元素引用")]
        [Tooltip("右上角的小书本图标按钮")]
        [SerializeField] private Button m_bookIcon;

        [Tooltip("画面中心的大书本容器对象")]
        [SerializeField] private GameObject m_largeBook;

        [Tooltip("点击关闭的半透明背景遮罩按钮")]
        [SerializeField] private Button m_backgroundOverlay;

        [Tooltip("提示文字：'点击任意空白处关闭书籍'")]
        [SerializeField] private TextMeshProUGUI m_closeHintText;

        [Header("外部引用")]
        [Tooltip("玩家控制器引用，用于暂停输入")]
        [SerializeField] private PlayerController m_playerController;

        [Header("动画配置")]
        [Tooltip("大书本上的 Animator 组件")]
        [SerializeField] private Animator m_bookAnimator;

        // 常量定义，避免魔术字符串
        private static readonly int k_isBookParam = Animator.StringToHash("isBook");
        private const float k_animationTransitionDelay = 0.5f;

        #region MonoBehaviour Methods

        private void OnEnable()
        {
            // 订阅按钮点击事件
            if (m_bookIcon != null)
            {
                m_bookIcon.onClick.AddListener(HandleOpenBook);
            }

            if (m_backgroundOverlay != null)
            {
                m_backgroundOverlay.onClick.AddListener(HandleCloseBook);
            }
        }

        private void OnDisable()
        {
            // 取消订阅，防止内存泄漏
            if (m_bookIcon != null)
            {
                m_bookIcon.onClick.RemoveListener(HandleOpenBook);
            }

            if (m_backgroundOverlay != null)
            {
                m_backgroundOverlay.onClick.RemoveListener(HandleCloseBook);
            }
        }

        private void Start()
        {
            // 初始状态设置
            InitializeUI();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 初始化 UI 状态，确保开始时大书是隐藏的
        /// </summary>
        private void InitializeUI()
        {
            if (m_largeBook != null)
            {
                m_largeBook.SetActive(false);
            }

            if (m_backgroundOverlay != null)
            {
                m_backgroundOverlay.gameObject.SetActive(false);
            }

            if (m_closeHintText != null)
            {
                m_closeHintText.gameObject.SetActive(false);
            }

            if (m_bookIcon != null)
            {
                m_bookIcon.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 处理打开书本的逻辑
        /// </summary>
        private void HandleOpenBook()
        {
            StartCoroutine(OpenBookCo());
        }

        private IEnumerator OpenBookCo()
        {
            // 1. 隐藏小图标
            if (m_bookIcon != null)
            {
                m_bookIcon.gameObject.SetActive(false);
            }

            // 2. 显示大书、背景和提示文字
            if (m_largeBook != null)
            {
                m_largeBook.SetActive(true);
            }

            if (m_backgroundOverlay != null)
            {
                m_backgroundOverlay.gameObject.SetActive(true);
            }

            if (m_closeHintText != null)
            {
                m_closeHintText.gameObject.SetActive(true);
                m_closeHintText.text = "点击任意空白处关闭书籍";
            }

            // 3. 暂停游戏进程并禁用输入
            Time.timeScale = 0f;
            if (m_playerController != null && m_playerController.inputControl != null)
            {
                m_playerController.inputControl.Disable();
            }

            // --- 核心修复：确保 Animator 在暂停状态下能更新 ---
            if (m_bookAnimator != null)
            {
                m_bookAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                
                // 等待一帧让 Animator 在激活状态下完成内部初始化
                yield return null;

                Debug.Log($"[BookController] 设置 isBook 为 true. UpdateMode: {m_bookAnimator.updateMode}");
                m_bookAnimator.SetBool("isBook", true);
            }
        }

        /// <summary>
        /// 处理关闭书本的逻辑
        /// </summary>
        private void HandleCloseBook()
        {
            StartCoroutine(CloseBookCo());
        }

        private IEnumerator CloseBookCo()
        {
            // 1. 播放退出动画 "Bookout"
            if (m_bookAnimator != null)
            {
                m_bookAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                Debug.Log("[BookController] 设置 isBook 为 false");
                m_bookAnimator.SetBool("isBook", false);
            }

            // 2. 等待动画播放完成
            // 注意：由于游戏已暂停 (Time.timeScale = 0)，必须使用 WaitForSecondsRealtime
            yield return new WaitForSecondsRealtime(k_animationTransitionDelay);

            // 3. 安全检查：如果对象已被销毁或禁用，则不再继续
            if (this == null || !isActiveAndEnabled)
            {
                yield break;
            }

            // 4. 关闭大书、背景和提示文字
            if (m_largeBook != null)
            {
                m_largeBook.SetActive(false);
            }

            if (m_backgroundOverlay != null)
            {
                m_backgroundOverlay.gameObject.SetActive(false);
            }

            if (m_closeHintText != null)
            {
                m_closeHintText.gameObject.SetActive(false);
            }

            // 5. 恢复显示小图标并启用输入
            if (m_bookIcon != null)
            {
                m_bookIcon.gameObject.SetActive(true);
            }

            if (m_playerController != null && m_playerController.inputControl != null)
            {
                m_playerController.inputControl.Enable();
            }

            // 6. 恢复游戏进程
            Time.timeScale = 1f;
        }

        #endregion
    }
}
