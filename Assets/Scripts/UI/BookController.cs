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
        private enum BookState
        {
            Drama,
            Magic,
            Setting
        }

        [Header("UI 元素引用")]
        [Tooltip("右上角的小书本图标按钮")]
        [SerializeField] private Button bookIcon;

        [Tooltip("画面中心的大书本容器对象")]
        [SerializeField] private GameObject largeBook;

        [Tooltip("点击关闭的半透明背景遮罩按钮")]
        [SerializeField] private Button backgroundOverlay;

        [Tooltip("提示文字：'点击任意空白处关闭书籍'")]
        [SerializeField] private TextMeshProUGUI closeHintText;

        [Header("页签按钮 (Tags)")]
        [SerializeField] private Button tagDrama;
        [SerializeField] private Button tagMagic;
        [SerializeField] private Button tagSetting;

        [Header("内容页面 (Pages)")]
        [SerializeField] private GameObject dramaPages;
        [SerializeField] private GameObject magicPages;
        [SerializeField] private GameObject settingPages;

        [Header("层级参考")]
        [Tooltip("层级参考对象：Papers2")]
        [SerializeField] private Transform papers2;

        [Header("外部引用")]
        [Tooltip("玩家控制器引用，用于暂停输入")]
        [SerializeField] private PlayerController playerController;

        [Header("动画配置")]
        [Tooltip("大书本上的 Animator 组件")]
        [SerializeField] private Animator bookAnimator;

        // 动画参数名的哈希值，用来告诉动画"打开/关闭"
        private static readonly int isBookHash = Animator.StringToHash("isBook");

        // 关闭动画大约要播多久（秒）
        private const float animationDelay = 0.45f;

        private BookState lastState = BookState.Drama;
        private GameObject[] allPages;
        private Button[] allTags;
        private int[] originalTagIndices;

        #region MonoBehaviour Methods

        private void Awake()
        {
            // 把三个页面和三个页签放进数组，方便后面循环处理
            allPages = new GameObject[] { dramaPages, magicPages, settingPages };
            allTags = new Button[] { tagDrama, tagMagic, tagSetting };

            // 记录三个页签一开始在层级里的位置，关闭书本时好还原
            originalTagIndices = new int[allTags.Length];
            for (int i = 0; i < allTags.Length; i++)
            {
                if (allTags[i] != null)
                {
                    originalTagIndices[i] = allTags[i].transform.GetSiblingIndex();
                }
            }
        }

        private void OnEnable()
        {
            // 订阅按钮点击事件
            if (bookIcon != null)
            {
                bookIcon.onClick.AddListener(HandleOpenBook);
            }

            if (backgroundOverlay != null)
            {
                backgroundOverlay.onClick.AddListener(HandleCloseBook);
            }

            // 订阅页签点击事件
            if (tagDrama != null) tagDrama.onClick.AddListener(() => SwitchState(BookState.Drama));
            if (tagMagic != null) tagMagic.onClick.AddListener(() => SwitchState(BookState.Magic));
            if (tagSetting != null) tagSetting.onClick.AddListener(() => SwitchState(BookState.Setting));
        }

        private void OnDisable()
        {
            // 取消订阅，防止内存泄漏
            if (bookIcon != null)
            {
                bookIcon.onClick.RemoveListener(HandleOpenBook);
            }

            if (backgroundOverlay != null)
            {
                backgroundOverlay.onClick.RemoveListener(HandleCloseBook);
            }

            if (tagDrama != null) tagDrama.onClick.RemoveAllListeners();
            if (tagMagic != null) tagMagic.onClick.RemoveAllListeners();
            if (tagSetting != null) tagSetting.onClick.RemoveAllListeners();
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
            if (largeBook != null)
            {
                largeBook.SetActive(false);
            }

            if (backgroundOverlay != null)
            {
                backgroundOverlay.gameObject.SetActive(false);
            }

            if (closeHintText != null)
            {
                closeHintText.gameObject.SetActive(false);
            }

            if (bookIcon != null)
            {
                bookIcon.gameObject.SetActive(true);
            }

            // 初始时根据记录的状态设置页面（默认为 Drama）
            UpdatePageVisibility();
        }

        /// <summary>
        /// 切换书本状态并更新页面显示
        /// </summary>
        private void SwitchState(BookState newState)
        {
            lastState = newState;
            UpdatePageVisibility();
        }

        /// <summary>
        /// 根据当前 lastState 统一更新所有页面的显示/隐藏，并调整页签层级与交互
        /// </summary>
        private void UpdatePageVisibility()
        {
            if (papers2 == null || allTags == null) return;

            int activeIndex = (int)lastState;

            // 1. 处理所有页签的交互和页面显示
            for (int i = 0; i < allTags.Length; i++)
            {
                if (allTags[i] == null) continue;

                bool isActive = (i == activeIndex);

                // 设置交互状态：当前激活的 Tag 无法被点击
                allTags[i].interactable = !isActive;

                // 更新页面内容显示
                if (allPages[i] != null)
                {
                    allPages[i].SetActive(isActive);
                }

                // 2. 层级排序准备：先将所有非激活 Tag 移到最顶层区域
                if (!isActive)
                {
                    allTags[i].transform.SetAsFirstSibling();
                }
            }

            // 3. 将 Papers2 设置为第二层（从上往下数）
            // 先将其设为最顶层，此时它在所有非激活 Tag 之上
            papers2.SetAsLastSibling();

            // 获取当前总子物体数，将其索引减 1，使其变为从上往下数第二层
            int totalChildren = papers2.parent.childCount;
            papers2.transform.SetSiblingIndex(totalChildren - 2);

            // 4. 将当前激活的 Tag 设置为最底层
            if (allTags[activeIndex] != null)
            {
                allTags[activeIndex].transform.SetAsLastSibling();
            }

            // 最终视觉层级（从上到下）：非激活Tags -> Papers2 (第二层) -> ... -> 激活Tag (最底层)
        }

        /// <summary>
        /// 恢复页签到原始层级和交互状态
        /// </summary>
        private void ResetTagsToOriginal()
        {
            if (allTags == null) return;

            for (int i = 0; i < allTags.Length; i++)
            {
                if (allTags[i] != null)
                {
                    allTags[i].interactable = true;
                    allTags[i].transform.SetSiblingIndex(originalTagIndices[i]);
                }
            }
        }

        /// <summary>
        /// 处理打开书本的逻辑
        /// </summary>
        private void HandleOpenBook()
        {
            // 每次打开时，先确保显示的是上一次退出时的页面
            UpdatePageVisibility();
            StartCoroutine(OpenBookCo());
        }

        private IEnumerator OpenBookCo()
        {
            // 1. 隐藏小图标
            if (bookIcon != null)
            {
                bookIcon.gameObject.SetActive(false);
            }

            // 2. 显示大书、背景和提示文字
            if (largeBook != null)
            {
                largeBook.SetActive(true);
            }

            if (backgroundOverlay != null)
            {
                backgroundOverlay.gameObject.SetActive(true);
            }

            if (closeHintText != null)
            {
                closeHintText.gameObject.SetActive(true);
                closeHintText.text = "*点击任意空白处关闭书籍*";
            }

            // 3. 暂停游戏进程并禁用输入
            Time.timeScale = 0f;
            if (playerController != null && playerController.inputControl != null)
            {
                playerController.inputControl.Disable();
            }

            // --- 核心修复：确保 Animator 在暂停状态下能更新 ---
            if (bookAnimator != null)
            {
                bookAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

                // 等待一帧让 Animator 在激活状态下完成内部初始化
                yield return null;

                bookAnimator.SetBool("isBook", true);
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
            if (bookAnimator != null)
            {
                bookAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                bookAnimator.SetBool("isBook", false);
            }

            // 2. 等待动画播放完成
            // 注意：由于游戏已暂停 (Time.timeScale = 0)，必须使用 WaitForSecondsRealtime
            yield return new WaitForSecondsRealtime(animationDelay);

            // 3. 安全检查：如果对象已被销毁或禁用，则不再继续
            if (this == null || !isActiveAndEnabled)
            {
                yield break;
            }

            // 4. 关闭大书、背景和提示文字
            if (largeBook != null)
            {
                largeBook.SetActive(false);
            }

            if (backgroundOverlay != null)
            {
                backgroundOverlay.gameObject.SetActive(false);
            }

            if (closeHintText != null)
            {
                closeHintText.gameObject.SetActive(false);
            }

            // 5. 恢复页签层级和交互状态
            ResetTagsToOriginal();

            // 6. 恢复显示小图标并启用输入
            if (bookIcon != null)
            {
                bookIcon.gameObject.SetActive(true);
            }

            if (playerController != null && playerController.inputControl != null)
            {
                playerController.inputControl.Enable();
            }

            // 6. 恢复游戏进程
            Time.timeScale = 1f;
        }

        #endregion
    }
}
