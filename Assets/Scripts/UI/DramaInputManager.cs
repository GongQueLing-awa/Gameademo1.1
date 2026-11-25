using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MyGame.UI
{
    /// <summary>
    /// 剧情输入管理器：挂在 DramaPages 物体上。
    /// 负责弹出输入框、回车判定答案、显示已解锁的词条、播放音效。
    /// 所有按钮共用同一个输入框，当前为哪个按钮输入，记在 currentButton 里。
    /// </summary>
    public class DramaInputManager : MonoBehaviour
    {
        [Tooltip("玩家输入答案用的输入框（所有按钮共用这一个）")]
        public TMP_InputField inputBox;

        [Tooltip("答案正确且词条已解锁时播放的音效")]
        public AudioClip correctSound;

        [Tooltip("音效事件通道（拖入 FX AudioEventSO）")]
        public PlayAudioEventSO fxAudioEvent;

        [Tooltip("失败文本模板：代码会按它给每个按钮各复制一份，模板自己不会显示")]
        public TMP_Text failText;

        [Tooltip("失败文本用的字体（拖入 ark-pixel-12px 的 SDF 字体）")]
        public TMP_FontAsset failFont;

        [Tooltip("提示框面板（圆角半透明黑底），放在 Canvas 下")]
        public RectTransform tooltipPanel;

        [Tooltip("提示框里的文字，显示已解锁的词条")]
        public TMP_Text tooltipText;

        [Tooltip("提示框相对鼠标的偏移，默认出现在鼠标下方一点")]
        public Vector2 tooltipOffset = new Vector2(0f, -40f);

        // 当前正在为哪个按钮输入，没输入时是 null
        private DramaInputButton currentButton;

        // 当前鼠标停在哪个按钮上（用来跟鼠标显示提示框），不在按钮上时是 null
        private DramaInputButton hoveredButton;

        // 翻页控制器，用来订阅翻页事件
        private DramaPageController pageController;

        // 待判定列表：答案输对了但当时词条没解锁，就先记在这里，
        // 等翻到对应页（且词条已解锁）时再补判定
        private List<DramaInputButton> pendingButtons = new List<DramaInputButton>();

      
        // 这样 A 按钮的失败文本不会被 B 按钮的操作清掉
        private Dictionary<DramaInputButton, TMP_Text> buttonFailTexts
            = new Dictionary<DramaInputButton, TMP_Text>();

        private void Awake()
        {
            if (inputBox != null)
            {
                // 在输入框里按下回车时，执行 Judge 方法
                inputBox.onSubmit.AddListener(Judge);
            }

            // 统一失败文本的字体、字号、颜色，并让它不拦截点击（不会挡住下面的按钮）
            if (failText != null)
            {
                failText.raycastTarget = false;
                failText.fontSize = 24f;
                failText.color = new Color32(0x84, 0x4A, 0x47, 0xFF); // 色号 844A47
                if (failFont != null) failText.font = failFont;
            }

            // 提示框文字统一成 24 号白色、居中
            if (tooltipText != null)
            {
                tooltipText.fontSize = 24f;
                tooltipText.color = Color.white;
                tooltipText.alignment = TextAlignmentOptions.Center;
                tooltipText.raycastTarget = false; // 文字也不拦截鼠标
            }

            // 提示框面板也不拦截鼠标，否则它挡在按钮上会让"进入/离开"反复触发
            if (tooltipPanel != null)
            {
                Image panelImage = tooltipPanel.GetComponent<Image>();
                if (panelImage != null) panelImage.raycastTarget = false;
            }

            // 拿到翻页控制器（订阅放到 OnEnable，确保书本每次打开都能重新订阅）
            pageController = GetComponent<DramaPageController>();

            // 进场先按"同拼音共享解锁"规则同步一次
            SyncUnlockByPinyin();
        }

        private void OnEnable()
        {
            // 订阅翻页事件，翻到某页时看看有没有待判定的词条
            if (pageController != null)
            {
                pageController.pageChanged += OnPageChanged;

                // 书本刚打开（或切回剧情页签）时，立刻检查当前页有没有待判定词条
                OnPageChanged(pageController.CurrentPage);
            }
        }

        private void Start()
        {
            CloseInput(); // 一开始输入框是收起来的
            if (failText != null) failText.gameObject.SetActive(false);
            if (tooltipPanel != null) tooltipPanel.gameObject.SetActive(false);
        }

        // 只做安全兜底：提示框显示期间，鼠标停着的按钮被隐藏或销毁了就收起
        private void Update()
        {
            if (tooltipPanel == null) return;
            if (!tooltipPanel.gameObject.activeSelf) return;

            if (hoveredButton == null || !hoveredButton.gameObject.activeInHierarchy)
            {
                HideTooltip();
            }
        }

        private void OnDisable()
        {
            if (pageController != null)
            {
                pageController.pageChanged -= OnPageChanged;
            }
        }

        // 按钮被点击时调用（由 DramaInputButton 传进来）：打开输入框
        public void OpenInput(DramaInputButton button)
        {
            if (inputBox == null) return;

            // 重新输入时只清掉这个按钮自己的待判定记录，
            // 其他按钮已经显示的失败文本保持原样不动
            pendingButtons.Remove(button);

            currentButton = button;
            inputBox.gameObject.SetActive(true); // 显示输入框
            inputBox.text = "";                  // 清掉上一次写的内容
            inputBox.ActivateInputField();       // 聚焦输入框，让玩家直接打字
        }

        // 鼠标进入按钮时调用（由 DramaInputButton 传进来）：显示提示框
        public void OpenTooltip(DramaInputButton button, Vector2 screenPos)
        {
            if (tooltipPanel == null || button == null) return;

            hoveredButton = button;

            if (tooltipText != null)
            {
                tooltipText.text = BuildTooltipText();
            }

            tooltipPanel.gameObject.SetActive(true);
            FollowMouse(screenPos); // 立刻摆到鼠标位置，避免第一帧闪在别处
        }

        // 鼠标在按钮上移动时调用：跟着鼠标走
        public void MoveTooltip(DramaInputButton button, Vector2 screenPos)
        {
            if (hoveredButton != button) return;
            if (tooltipPanel == null || !tooltipPanel.gameObject.activeSelf) return;

            FollowMouse(screenPos);
        }

        // 鼠标离开按钮时调用：离开的是当前这个按钮才收起
        public void CloseTooltip(DramaInputButton button)
        {
            if (hoveredButton != button) return;
            HideTooltip();
        }

        // 收起提示框
        private void HideTooltip()
        {
            hoveredButton = null;
            if (tooltipPanel != null)
            {
                tooltipPanel.gameObject.SetActive(false);
            }
        }

        // 让提示框跟着鼠标走；screenPos 是鼠标的屏幕坐标（由按钮的指针事件传进来）
        private void FollowMouse(Vector2 screenPos)
        {
            RectTransform parentRect = tooltipPanel.parent as RectTransform;
            if (parentRect == null) return;

            // 屏幕坐标 -> 提示框父级里的局部坐标；本项目是 Overlay 画布，相机传 null 就行
            Camera cam = null;
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect, screenPos, cam, out localPoint))
            {
                tooltipPanel.anchoredPosition = localPoint + tooltipOffset;
            }
        }

        // 规则：同拼音的词条共享解锁状态。
        // 只要其中有一个解锁了，所有同拼音词条都算解锁（跨页也算）。
        private void SyncUnlockByPinyin()
        {
            Transform textRoot = transform.Find("TextOfficials");
            if (textRoot == null) return;

            // 第一遍：从"已解锁"的词条出发
            foreach (Transform group in textRoot)
            {
                foreach (Transform child in group)
                {
                    DramaWordEntry word = child.GetComponent<DramaWordEntry>();
                    if (word == null || !word.unlocked) continue;

                    string pinyin = GetPinyin(child.name);

                    // 第二遍：把所有页里同拼音的词条都设为解锁
                    foreach (Transform otherGroup in textRoot)
                    {
                        foreach (Transform other in otherGroup)
                        {
                            DramaWordEntry otherWord = other.GetComponent<DramaWordEntry>();
                            if (otherWord == null) continue;
                            if (GetPinyin(other.name) != pinyin) continue;

                            otherWord.unlocked = true;
                        }
                    }
                }
            }
        }

        // 拼提示框文字：第一行标题，第二行是整本书里所有已解锁词条（中文，逗号隔开）。
        // 不管鼠标指着哪个按钮，内容都一样，平等列出全部已解锁词条。
        private string BuildTooltipText()
        {
            // 显示前同步一次，保证提示框里的解锁列表是最新的
            SyncUnlockByPinyin();

            Transform textRoot = transform.Find("TextOfficials");

            List<string> names = new List<string>();

            if (textRoot != null)
            {
                // 遍历 Text 1 ~ Text 8 这 8 个页组
                foreach (Transform group in textRoot)
                {
                    // 再遍历这一页里的所有子物体，挑出词条
                    foreach (Transform child in group)
                    {
                        if (child.name.ToLower().Contains("button")) continue; // 按钮跳过

                        DramaWordEntry word = child.GetComponent<DramaWordEntry>();
                        if (word == null || !word.unlocked) continue; // 没挂组件或没解锁，跳过

                        TMP_Text label = child.GetComponent<TMP_Text>();
                        if (label == null) continue;

                        // 同一个中文只列一次，避免出现"神，神"
                        if (!names.Contains(label.text))
                        {
                            names.Add(label.text);
                        }
                    }
                }
            }

            string list = (names.Count > 0) ? string.Join("，", names) : "无";
            return "已解锁的词条有：\n" + list;
        }

        // 收起输入框，结束本次输入
        private void CloseInput()
        {
            currentButton = null;
            if (inputBox != null)
            {
                inputBox.gameObject.SetActive(false);
            }
        }

        // 回车后进入判定：答案对不对、词条有没有解锁
        private void Judge(string input)
        {
            if (currentButton == null || inputBox == null) return;

            DramaInputButton button = currentButton;

            // 判定前先同步一次解锁状态（方便你在 Inspector 里调试勾选）
            SyncUnlockByPinyin();

            string pinyin = GetPinyin(button.name);
            string answer = GetAnswer(pinyin);
            bool isCorrect = (answer != "" && input.Trim() == answer);

            if (isCorrect)
            {
                // 答案对：尝试显示已解锁的词条
                if (RevealWords(button))
                {
                    pendingButtons.Remove(button); // 成功了，不再待判定
                    HandleShuPair(button);         // 第 4 页两个"书"联动
                    Succeed(button);
                    return;
                }

                // 答案对但词条还没解锁：记入待判定，等翻到这页且解锁后再判
                if (!pendingButtons.Contains(button))
                {
                    pendingButtons.Add(button);
                }
            }

            // 走到这里 = 判定失败（答案错，或答案对但未解锁）：
            // 把玩家输入的字显示在按钮下方，然后退出输入模式
            ShowFailText(button, input);
            CloseInput();
        }

        // 翻页时调用：检查这一页有没有待判定的词条，有就补判定
        private void OnPageChanged(int pageIndex)
        {
            // 翻页时也同步一次，保证新页面用的是最新解锁状态
            SyncUnlockByPinyin();

            if (pendingButtons.Count == 0) return;

            Transform textRoot = transform.Find("TextOfficials");
            if (textRoot == null) return;

            Transform group = textRoot.Find("Text " + (pageIndex + 1));
            if (group == null) return;

            // 从后往前遍历，方便边遍历边移除
            for (int i = pendingButtons.Count - 1; i >= 0; i--)
            {
                DramaInputButton btn = pendingButtons[i];

                // 收敛：按钮对象可能已被销毁，直接清掉
                if (btn == null)
                {
                    pendingButtons.RemoveAt(i);
                    continue;
                }

                // 不在当前这一页，跳过
                if (btn.transform.parent != group) continue;

                // 补判定：词条现在解锁了就显示
                if (RevealWords(btn))
                {
                    pendingButtons.RemoveAt(i);
                    HandleShuPair(btn); // 第 4 页两个"书"联动
                    Succeed(btn);
                }
            }
        }

        // 把按钮所在 Text N 下、同拼音且已解锁的词条显示出来；返回是否有词条被显示
        private bool RevealWords(DramaInputButton button)
        {
            if (button == null) return false;

            string pinyin = GetPinyin(button.name);
            Transform group = button.transform.parent;
            bool anyShown = false;

            foreach (Transform child in group)
            {
                DramaWordEntry word = child.GetComponent<DramaWordEntry>();
                if (word == null) continue;                    // 不是词条（比如按钮），跳过
                if (GetPinyin(child.name) != pinyin) continue; // 拼音不同的词条，跳过
                if (!word.unlocked) continue;                  // 没解锁的词条，不作任何反应

                child.gameObject.SetActive(true); // 显示这个词条
                anyShown = true;
            }

            return anyShown;
        }

        // 第 4 页特殊规则：两个"书"是一对。
        // 任意一个"书"判定成功后，另一个也无条件一起成功（不管它原来解锁没解锁、输入框里是什么）。
        private void HandleShuPair(DramaInputButton button)
        {
            if (GetPinyin(button.name) != "shu") return;      // 不是"书"，不管
            if (button.transform.parent.name != "Text 4") return; // 不是第 4 页，不管

            Transform group = button.transform.parent;

            foreach (Transform child in group)
            {
                if (child.name.ToLower().Contains("button")) continue; // 按钮跳过
                if (GetPinyin(child.name) != "shu") continue;          // 只处理"书"

                // 直接把这个词条显示出来（不看它的 unlocked）
                child.gameObject.SetActive(true);

                // 它对应的按钮也当作已经成功：清掉待判定记录和失败文本
                DramaInputButton pairButton = FindButtonOfWord(child);
                if (pairButton != null)
                {
                    pendingButtons.Remove(pairButton);
                    HideFailText(pairButton);
                }
            }
        }

        // 找一个词条对应的按钮：同拼音、离它最近的按钮
        private DramaInputButton FindButtonOfWord(Transform word)
        {
            string pinyin = GetPinyin(word.name);
            Transform group = word.parent;
            RectTransform wordRect = word.GetComponent<RectTransform>();

            DramaInputButton best = null;
            float bestDist = float.MaxValue;

            foreach (Transform child in group)
            {
                if (!child.name.ToLower().Contains("button")) continue; // 只要按钮
                if (GetPinyin(child.name) != pinyin) continue;          // 拼音不同跳过

                DramaInputButton btn = child.GetComponent<DramaInputButton>();
                if (btn == null) continue;

                RectTransform btnRect = child.GetComponent<RectTransform>();
                float dx = btnRect.anchoredPosition.x - wordRect.anchoredPosition.x;
                float dy = btnRect.anchoredPosition.y - wordRect.anchoredPosition.y;
                float dist = dx * dx + dy * dy;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = btn;
                }
            }

            return best;
        }

        // 判定成功后的统一动作：播音效、收起这个按钮自己的失败文本、清输入框、收起输入框
        private void Succeed(DramaInputButton button)
        {
            // 统一走 FX 事件通道，由 AudioManager 负责真正播放
            if (fxAudioEvent != null && correctSound != null)
            {
                fxAudioEvent.RaiseEvent(correctSound);
            }

            HideFailText(button);
            if (inputBox != null) inputBox.text = "";
            CloseInput();
        }

        // 判定失败：把玩家输入的字符串放到对应词条的下划线空位，并显示出来
        private void ShowFailText(DramaInputButton button, string text)
        {
            if (button == null) return;

            // 拿到这个按钮专属的失败文本（第一次用到时复制一份模板）
            TMP_Text myText = GetFailText(button);
            if (myText == null) return;

            Transform group = button.transform.parent;
            RectTransform groupRect = group as RectTransform;

            // 找到这个按钮对应的词条（同拼音、距离最近），它的位置就是下划线空位；找不到就用按钮自己
            Transform word = FindNearestWord(button);
            RectTransform srcRect = (word != null)
                ? word.GetComponent<RectTransform>()
                : button.GetComponent<RectTransform>();

            RectTransform failRect = myText.GetComponent<RectTransform>();

            // 固定成居中锚点 + 居中中心，再强制文字居中，消除对齐基准不同带来的偏移
            failRect.anchorMin = new Vector2(0.5f, 0.5f);
            failRect.anchorMax = new Vector2(0.5f, 0.5f);
            failRect.pivot = new Vector2(0.5f, 0.5f);
            myText.alignment = TextAlignmentOptions.Center;

            // 用 Unity 标准坐标转换：取源矩形的世界中心，写成父级下的 anchoredPosition。
            // 不直接用 position / anchoredPosition 的数值，避开父级缩放与原点差异。
            failRect.anchoredPosition = WorldCenterToAnchored(srcRect, groupRect);

            // 图层先后：放到最后一个兄弟，让它画在最上面（能看见字）。
            failRect.SetAsLastSibling();

            myText.text = text;
            myText.gameObject.SetActive(true);
        }

        // 拿到某个按钮专属的失败文本；第一次用到时按模板复制（克隆）一份
        private TMP_Text GetFailText(DramaInputButton button)
        {
            TMP_Text myText;

            // 已经给这个按钮建过文本，就直接复用
            if (buttonFailTexts.TryGetValue(button, out myText) && myText != null)
            {
                return myText;
            }

            if (failText == null) return null; // 没设模板，没法复制

            // 复制一份模板，挂到按钮所在的 Text N 下，这样翻页时跟着那页一起显隐
            myText = Instantiate(failText, button.transform.parent, false);
            myText.gameObject.SetActive(false); // 先藏起来，等设好文字再显示
            myText.raycastTarget = false;       // 不拦截点击

            buttonFailTexts[button] = myText;
            return myText;
        }

        // 只收起某个按钮自己的失败文本，不影响别的按钮
        private void HideFailText(DramaInputButton button)
        {
            TMP_Text myText;
            if (button != null
                && buttonFailTexts.TryGetValue(button, out myText)
                && myText != null)
            {
                myText.gameObject.SetActive(false);
            }
        }

        // 把源矩形的几何中心（世界坐标）换算成目标父级下的 anchoredPosition
        private Vector2 WorldCenterToAnchored(RectTransform src, RectTransform parentRect)
        {
            // GetWorldCorners 返回世界空间的四个角：[0]左下 [2]右上，中点就是矩形中心（不受 pivot 影响）
            Vector3[] corners = new Vector3[4];
            src.GetWorldCorners(corners);
            Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

            // Overlay Canvas 不需要相机，传 null
            Camera cam = null;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);

            // 屏幕坐标 -> 目标父级矩形内的局部坐标；居中锚点下它就等于 anchoredPosition
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, cam, out localPoint);
            return localPoint;
        }

        // 在按钮所在的 Text N 下，找同拼音且离按钮最近的词条（它就是这个按钮对应的下划线空位）。
        // 只靠名字判断（排除名字带 button 的），不依赖是否挂了 DramaWordEntry 组件。
        private Transform FindNearestWord(DramaInputButton button)
        {
            string pinyin = GetPinyin(button.name);
            Transform group = button.transform.parent;
            RectTransform buttonRect = button.GetComponent<RectTransform>();

            Transform best = null;
            float bestDist = float.MaxValue;

            foreach (Transform child in group)
            {
                // 名字带 button 的是按钮，跳过；只在词条里找
                if (child.name.ToLower().Contains("button")) continue;
                if (GetPinyin(child.name) != pinyin) continue; // 拼音不同，跳过

                RectTransform wordRect = child.GetComponent<RectTransform>();
                float dx = wordRect.anchoredPosition.x - buttonRect.anchoredPosition.x;
                float dy = wordRect.anchoredPosition.y - buttonRect.anchoredPosition.y;
                float dist = dx * dx + dy * dy;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = child;
                }
            }

            return best;
        }

        // 从物体名字里提取拼音词缀，统一转小写，所以大小写不同也能对上：
        // "ShenButton" -> "shen"；"Text shen (1)" -> "shen"；"ShuButton (2)" -> "shu"
        private string GetPinyin(string objectName)
        {
            string s = objectName.ToLower();
            s = s.Replace("text", "");
            s = s.Replace("button", "");

            // 去掉 "(1)" 这种编号括号
            int bracket = s.IndexOf('(');
            if (bracket >= 0)
            {
                s = s.Substring(0, bracket);
            }

            return s.Trim(); // 去掉首尾空格
        }

        // 拼音词缀对应的标准答案
        private string GetAnswer(string pinyin)
        {
            switch (pinyin)
            {
                case "shen": return "神";
                case "moli": return "魔力";
                case "renlei": return "人类";
                case "xiaoshi": return "消失";
                case "meiyou": return "没有";
                case "zhandou": return "战斗";
                case "huifu": return "恢复";
                case "shu": return "书";
                case "fangshi": return "方式";
                case "tanxianzhe": return "探险者";
                case "guaiwu": return "怪物";
                case "goutong": return "沟通";
                case "chouhen": return "仇恨";
                case "heping": return "和平";
                case "kewang": return "渴望";
                default: return "";
            }
        }
    }
}
