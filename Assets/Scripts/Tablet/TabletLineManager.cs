using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using MyGame.UI;

// 一个技能的答案：技能名 + 需要连出来的边
[Serializable]
public class TabletAnswer
{
    [Tooltip("技能名，比如 飞翔、攻击")]
    public string skillName;

    [Tooltip("需要连出来的边，每条写成 名字-名字，比如 Tabletsmall-Tabletbig")]
    public string[] edges;
}

public class TabletLineManager : MonoBehaviour
{
    // 全场景只有一份，石碑通过它找到管理器
    public static TabletLineManager instance;

    [Header("引用")]
    [Tooltip("玩家身上的 Lightpoint，线的最后一端")]
    public Transform playerLight;

    [Header("答案（可以在这里继续加新技能）")]
    [Tooltip("每个技能一组答案，判定时按顺序找第一个完全符合的")]
    public List<TabletAnswer> answers = new List<TabletAnswer>()
    {
        new TabletAnswer()
        {
            skillName = "飞行",
            edges = new string[]
            {
                "Tabletsmall-Tabletbig",
                "Tabletbig-Tablettiny",
                "Tablettiny-Tabletsmall"
            }
        },
        new TabletAnswer()
        {
            skillName = "攻击",
            edges = new string[]
            {
                "Tabletsmall-Tabletbig",
                "Tabletbig-Tablettiny",
                "Tablettiny-Tabletsmall",
                "Tablettiny-Tabletbig1",
                "Tabletbig1-Tablettiny1",
                "Tablettiny1-Tabletsmall1"
            }
        }
    };

    [Header("UI 提示（把文本拖进来，留空就不显示）")]
    public TMP_Text successText;
    public TMP_Text failText;
    public TMP_Text repeatText;

    [Tooltip("词条解锁提示 TextWords，和上面几个用同一套淡入淡出")]
    public TMP_Text wordsText;

    [Tooltip("提示文字停留多少秒后开始淡出")]
    public float tipShowTime = 2f;

    [Tooltip("淡入淡出各花多少秒")]
    public float fadeTime = 0.3f;

    // 技能名在 UI 里显示的颜色（青色 #53CEE7）
    private const string k_skillNameColor = "53CEE7";

    // 三个提示的原始模板文字。Awake 里从 TMP_Text.text 读出来存着，
    // 每次显示都从模板出发替换 wordsPoint，避免上一次替换后的富文本残留
    private string successTemplate;
    private string failTemplate;
    private string repeatTemplate;
    private string wordsTemplate;

    // 当前正在跑的那个淡入淡出协程。同时只应该有一个提示在显示
    private Coroutine currentFadeCo;

    // 词条提示的队列：同一刻解锁多个词条时排队，一个一个放，不叠在一起
    private Queue<string> wordTipQueue = new Queue<string>();

    // 正在跑"词条提示连播"的那个协程，空表示现在没在放
    private Coroutine wordTipsCo;

    [Header("线的样子")]
    public float lineWidth = 0.08f;

    [Tooltip("线的排序层，默认 bottom")]
    public string lineSortingLayer = "bottom";

    [Tooltip("同一层里的顺序，数字越大越靠前")]
    public int lineSortingOrder = 1;

    [Tooltip("线的材质。留空会自动用 2D 精灵默认材质")]
    public Material lineMaterial;

    // 已经按过的石碑，按按下的顺序存
    private List<TabletTouch> pressedTablets = new List<TabletTouch>();

    // 已经连出来的边，比如 "Tabletsmall-Tabletbig"
    private List<string> drawnEdges = new List<string>();

    private LineRenderer beam;

    // 已经解锁过的技能。静态的，切换场景也不会丢
    public static List<string> unlockedSkills = new List<string>();

    public static bool IsSkillUnlocked(string skillName)
    {
        return unlockedSkills.Contains(skillName);
    }

    private void Awake()
    {
        instance = this;

        // 没拖玩家光点就自动找玩家，再找它下面的 Lightpoint
        if (playerLight == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();

            if (player != null)
            {
                playerLight = player.transform.Find("Lightpoint");
            }
        }

        if (playerLight == null)
        {
            Debug.LogError("没找到玩家 Lightpoint，请把玩家身上的 Lightpoint 拖到 playerLight 上", this);
        }

        CreateBeam();

        // 把 Inspector 里填好的原文案存成模板，以后每次替换都从模板出发
        // 这样即使上一次替换后的富文本残留在 tip.text 里，也不会污染下一次
        successTemplate = successText != null ? successText.text : "";
        failTemplate = failText != null ? failText.text : "";
        repeatTemplate = repeatText != null ? repeatText.text : "";
        wordsTemplate = wordsText != null ? wordsText.text : "";

        // 兼容旧存档：老版本解锁过的 "飞翔" 统一改叫 "飞行"
        if (unlockedSkills.Remove("飞翔"))
        {
            unlockedSkills.Add("飞行");
        }

        // 开局先把提示文字藏起来（同时重置 alpha，免得下次淡入时继承上一次的透明度）
        HideAllTips();
    }

    private void OnDestroy()
    {
        // 切场景销毁管理器时清掉静态引用，避免指向已经消失的对象
        if (instance == this)
        {
            instance = null;
        }
    }

    private void CreateBeam()
    {
        // 新建一个空物体专门放线。
        // 不挂在玩家下面，因为玩家会被翻转/缩放，线会跟着变形。
        GameObject beamObject = new GameObject("TabletBeam");

        beam = beamObject.AddComponent<LineRenderer>();
        beam.positionCount = 0;
        beam.useWorldSpace = true;
        beam.startWidth = lineWidth;
        beam.endWidth = lineWidth;
        beam.startColor = Color.white;
        beam.endColor = Color.white;
        beam.sortingLayerName = lineSortingLayer;
        beam.sortingOrder = lineSortingOrder;

        // 有拖材质就用拖的，没有就用 2D 精灵默认材质
        if (lineMaterial != null)
        {
            beam.material = lineMaterial;
        }
        else
        {
            beam.material = new Material(Shader.Find("Sprites/Default"));
        }

        beam.enabled = false;
    }

    private void Update()
    {
        if (pressedTablets.Count > 0)
        {
            RefreshBeam();
        }

        // 回车判定。用新输入系统的 Keyboard.current 直接读，不用改输入资源
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Judge();
        }
    }

    // 石碑被按 E 激活时，TabletTouch 会调用这里
    public void TabletPressed(TabletTouch tablet)
    {
        if (tablet == null) return;

        if (string.IsNullOrEmpty(tablet.tabletName))
        {
            Debug.LogWarning("这块石碑没填 tabletName，先跳过", tablet);
            return;
        }

        // 同一块石碑连续按两次就忽略，避免画出长度 0 的边。
        // 这里按名字判断，父物体和子物体都挂了 TabletTouch 也能去重。
        if (pressedTablets.Count > 0)
        {
            TabletTouch lastTablet = pressedTablets[pressedTablets.Count - 1];

            if (lastTablet.tabletName == tablet.tabletName)
            {
                return;
            }
        }

        bool isFirstOne = pressedTablets.Count == 0;

        // 如果这块碑上一轮成功过，先关掉 HaveAcceed，才能重新点亮
        tablet.WakeUp();

        if (isFirstOne)
        {
            // 只有第一块碑 isFirst 为真
            tablet.SetFirst(true);
        }
        else
        {
            tablet.SetFirst(false);

            // 把上一块碑和这块碑连成一条边
            TabletTouch lastTablet = pressedTablets[pressedTablets.Count - 1];
            drawnEdges.Add(MakeEdgeName(lastTablet.tabletName, tablet.tabletName));
        }

        pressedTablets.Add(tablet);
        RefreshBeam();
    }

    // 刷新线：所有已按石碑 + 玩家，最后一个点永远是玩家
    private void RefreshBeam()
    {
        if (playerLight == null) return;

        beam.enabled = true;
        beam.positionCount = pressedTablets.Count + 1;

        for (int i = 0; i < pressedTablets.Count; i++)
        {
            beam.SetPosition(i, GetTabletLightPos(pressedTablets[i]));
        }

        // 最后一段：当前石碑 -> 玩家，每帧跟着玩家动
        beam.SetPosition(pressedTablets.Count, playerLight.position);
    }

    private Vector3 GetTabletLightPos(TabletTouch tablet)
    {
        // 没拖 Light 就用石碑自己的位置，至少不会报错
        if (tablet.lightPoint != null)
        {
            return tablet.lightPoint.position;
        }

        return tablet.transform.position;
    }

    // 把两个名字整理成一条边，保证 大-小 和 小-大 是同一个字符串
    private string MakeEdgeName(string a, string b)
    {
        a = a.Trim();
        b = b.Trim();

        if (string.CompareOrdinal(a, b) > 0)
        {
            return b + "-" + a;
        }

        return a + "-" + b;
    }

    private void Judge()
    {
        // 还没连线就不判定
        if (drawnEdges.Count == 0) return;

        // 找第一个和连线完全一样的答案
        TabletAnswer matched = null;

        foreach (TabletAnswer answer in answers)
        {
            if (IsSameAsAnswer(answer))
            {
                matched = answer;
                break;
            }
        }

        // 没对上任何答案：失败
        if (matched == null)
        {
            PlayResultOnAll(false);
            ShowTip(failText, null); // 失败提示里没技能名，传 null 就保留原文案
            ClearLine();
            return;
        }

        // 对上了，但这个技能以前已经解锁过：也算失败
        if (IsSkillUnlocked(matched.skillName))
        {
            PlayResultOnAll(false);
            ShowTip(repeatText, matched.skillName);
            ClearLine();
            return;
        }

        // 第一次解锁
        PlayResultOnAll(true);
        unlockedSkills.Add(matched.skillName);
        UnlockSkill(matched.skillName);
        ShowTip(successText, matched.skillName);
        ClearLine();
    }

    private bool IsSameAsAnswer(TabletAnswer answer)
    {
        if (answer.edges == null) return false;
        if (answer.edges.Length != drawnEdges.Count) return false;

        foreach (string need in answer.edges)
        {
            string[] names = need.Split('-');

            if (names.Length != 2)
            {
                Debug.LogWarning($"答案里的边写错了：{need}，应该写成 名字-名字", this);
                return false;
            }

            string fixedEdge = MakeEdgeName(names[0], names[1]);

            if (!drawnEdges.Contains(fixedEdge))
            {
                return false;
            }
        }

        return true;
    }

    // 让所有连过的石碑播结果动画。闭合三角时第一块碑会出现两次，这里去重
    private void PlayResultOnAll(bool success)
    {
        List<TabletTouch> done = new List<TabletTouch>();

        foreach (TabletTouch tablet in pressedTablets)
        {
            if (done.Contains(tablet)) continue;
            done.Add(tablet);

            if (success)
            {
                tablet.PlaySuccess();
            }
            else
            {
                tablet.PlayFail();
            }
        }
    }

    private void ClearLine()
    {
        foreach (TabletTouch tablet in pressedTablets)
        {
            tablet.ClearFirst();
        }

        pressedTablets.Clear();
        drawnEdges.Clear();
        beam.positionCount = 0;
        beam.enabled = false;
    }

    private void UnlockSkill(string skillName)
    {
        if (skillName == "飞行" || skillName == "飞翔")
        {
            // 飞行还没做，先占个位置
            Debug.Log("解锁飞行技能！（以后在这里写真正会飞的功能）", this);
        }
        else if (skillName == "攻击")
        {
            // 攻击已经写好了，把开关打开就行
            AttactController.attackUnlocked = true;

            // 解锁战斗技能的同时，解锁书里的"战斗"词条
            MyGame.UI.DramaInputManager.UnlockWord("zhandou");
        }
    }

    // 词条解锁提示：给 DramaInputManager 调的公开入口。
    // 同一刻来多个就排队，前一个淡入淡出播完，才播下一个
    public void ShowWordsTip(string wordName)
    {
        if (wordsText == null) return;

        wordTipQueue.Enqueue(wordName);

        if (wordTipsCo == null)
        {
            wordTipsCo = StartCoroutine(ShowWordTipsCo());
        }
    }

    // 依次播队列里的词条提示
    private IEnumerator ShowWordTipsCo()
    {
        while (wordTipQueue.Count > 0)
        {
            string wordName = wordTipQueue.Dequeue();

            // 把模板里的 wordsPoint 换成带颜色 #53CEE7 的中文词条名
            wordsText.text = wordsTemplate.Replace(
                "wordsPoint",
                $"<color=#{k_skillNameColor}>{wordName}</color>");

            // 播一次完整的淡入淡出；yield return 另一个协程 = 等它跑完再往下走
            yield return FadeTipCo(wordsText);
        }

        wordTipsCo = null;
    }

    // skillName 为 null = 失败提示，直接用原文案；
    // 否则把原文案里的 wordsPoint 替换成带青色的技能名（TMP 富文本）
    private void ShowTip(TMP_Text tip, string skillName)
    {
        if (tip == null) return;

        string template = GetTemplateFor(tip);
        string display = template;

        if (!string.IsNullOrEmpty(skillName))
        {
            display = display.Replace(
                "wordsPoint",
                $"<color=#{k_skillNameColor}>{skillName}</color>");
        }

        tip.text = display;

        // 上一个提示还没淡出完的话先停掉，免得重叠
        if (currentFadeCo != null) StopCoroutine(currentFadeCo);
        currentFadeCo = StartCoroutine(FadeTipCo(tip));
    }

    private string GetTemplateFor(TMP_Text tip)
    {
        if (tip == successText) return successTemplate;
        if (tip == failText) return failTemplate;
        if (tip == repeatText) return repeatTemplate;
        if (tip == wordsText) return wordsTemplate;
        return tip.text;
    }

    // 协程：先淡入 → 停留 → 淡出，然后自动隐藏
    // 一句话理解协程：就是一个能"暂停"的函数，yield return null 等下一帧再跑
    private IEnumerator FadeTipCo(TMP_Text tip)
    {
        SetAlpha(tip, 0f);
        tip.gameObject.SetActive(true);

        // 淡入：从 0 跑到 1
        // 用 unscaledDeltaTime / WaitForSecondsRealtime，开书暂停 (timeScale = 0) 时提示也能正常播
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(tip, Mathf.Clamp01(t / fadeTime));
            yield return null; // 等下一帧
        }

        // 停留不动（同样用真实时间，暂停时也会计时）
        yield return new WaitForSecondsRealtime(tipShowTime);

        // 淡出：从 1 跑到 0
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(tip, 1f - Mathf.Clamp01(t / fadeTime));
            yield return null;
        }

        tip.gameObject.SetActive(false);
        SetAlpha(tip, 1f); // 重置 alpha，下次淡入从 0 覆盖就行

        if (currentFadeCo != null) currentFadeCo = null;
    }

    private void SetAlpha(TMP_Text tip, float a)
    {
        Color c = tip.color;
        c.a = a;
        tip.color = c;
    }

    private void HideAllTips()
    {
        SetTipInvisible(successText);
        SetTipInvisible(failText);
        SetTipInvisible(repeatText);
        SetTipInvisible(wordsText);
    }

    private void SetTipInvisible(TMP_Text tip)
    {
        if (tip == null) return;

        Color c = tip.color;
        c.a = 1f; // 重置 alpha，避免下次 SetActive(true) 时继承透明状态
        tip.color = c;
        tip.gameObject.SetActive(false);
    }
}
