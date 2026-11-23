using System;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.UI
{
    /// <summary>
    /// 剧情翻页控制器：挂在 DramaPages 物体上。
    /// 点 Left / Right 按钮切换 Dramapage1~8 和对应的 Text 1~8。
    /// </summary>
    public class DramaPageController : MonoBehaviour
    {
        // 剧情一共 8 页
        private const int pageCount = 8;

        [Tooltip("上一页按钮，留空会自动找子物体 Left")]
        [SerializeField] private Button leftButton;

        [Tooltip("下一页按钮，留空会自动找子物体 Right")]
        [SerializeField] private Button rightButton;

        // 每一页对应的图片和文字组（数组下标 0 就是第 1 页）
        private GameObject[] pages = new GameObject[pageCount];
        private GameObject[] textGroups = new GameObject[pageCount];

        // 当前是第几页，0 表示第 1 页
        private int currentPage = 0;

        // 翻页事件：翻到某一页时通知外面（参数是页码，0 基）
        public event Action<int> pageChanged;

        // 当前页码（0 基），只读给外面看
        public int CurrentPage => currentPage;

        private void Awake()
        {
            FindPagesAndTexts();
            FindButtons();
            MoveArrowsToFront();
        }

        private void OnEnable()
        {
            // 打开书本或切回剧情页签时，重新显示当前页
            AddButtonEvents();
            ShowPage(currentPage);
        }

        private void OnDisable()
        {
            RemoveButtonEvents();
        }

        // 按名字找到 8 页的图片和文字组
        private void FindPagesAndTexts()
        {
            Transform textRoot = transform.Find("TextOfficials");

            for (int i = 0; i < pageCount; i++)
            {
                Transform page = transform.Find("Dramapage" + (i + 1));
                Transform text = null;

                if (textRoot != null)
                {
                    text = textRoot.Find("Text " + (i + 1));
                }

                if (page == null || text == null)
                {
                    Debug.LogError("第 " + (i + 1) + " 页没找到 Dramapage 或 Text，请检查名字");
                    continue;
                }

                pages[i] = page.gameObject;
                textGroups[i] = text.gameObject;
            }
        }

        // 没在 Inspector 里拖按钮时，自动按名字找 Left 和 Right
        private void FindButtons()
        {
            if (leftButton == null)
            {
                Transform left = transform.Find("Left");
                if (left != null) leftButton = left.GetComponent<Button>();
            }

            if (rightButton == null)
            {
                Transform right = transform.Find("Right");
                if (right != null) rightButton = right.GetComponent<Button>();
            }
        }

        // 页面图片比箭头大，会挡住点击，所以把箭头放到最后，保证能点到
        private void MoveArrowsToFront()
        {
            if (leftButton != null)
            {
                leftButton.transform.SetAsLastSibling();
            }

            if (rightButton != null)
            {
                rightButton.transform.SetAsLastSibling();
            }
        }

        private void AddButtonEvents()
        {
            if (leftButton != null)
            {
                leftButton.onClick.AddListener(ShowPreviousPage);
            }

            if (rightButton != null)
            {
                rightButton.onClick.AddListener(ShowNextPage);
            }
        }

        private void RemoveButtonEvents()
        {
            if (leftButton != null)
            {
                leftButton.onClick.RemoveListener(ShowPreviousPage);
            }

            if (rightButton != null)
            {
                rightButton.onClick.RemoveListener(ShowNextPage);
            }
        }

        private void ShowPreviousPage()
        {
            ShowPage(currentPage - 1);
        }

        private void ShowNextPage()
        {
            ShowPage(currentPage + 1);
        }

        // 只显示指定页的图片和文字，其他页全部隐藏
        private void ShowPage(int pageIndex)
        {
            // 把页码限制在 0 ~ 7 之间，点过头也不会出错
            currentPage = Mathf.Clamp(pageIndex, 0, pageCount - 1);

            for (int i = 0; i < pageCount; i++)
            {
                bool isCurrent = (i == currentPage);

                if (pages[i] != null) pages[i].SetActive(isCurrent);
                if (textGroups[i] != null) textGroups[i].SetActive(isCurrent);
            }

            // 第 1 页不能往左翻，第 8 页不能往右翻
            if (leftButton != null) leftButton.interactable = (currentPage > 0);
            if (rightButton != null) rightButton.interactable = (currentPage < pageCount - 1);

            // 通知外面：现在翻到第 currentPage 页了
            pageChanged?.Invoke(currentPage);
        }
    }
}
