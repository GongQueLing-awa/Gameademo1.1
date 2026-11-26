using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
            skillName = "飞翔",
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

    [Tooltip("提示文字显示多少秒后自动隐藏")]
    public float tipShowTime = 2f;

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

        // 开局先把提示文字藏起来
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
            ShowTip(failText, "连接不对，再试一次吧");
            ClearLine();
            return;
        }

        // 对上了，但这个技能以前已经解锁过：也算失败
        if (IsSkillUnlocked(matched.skillName))
        {
            PlayResultOnAll(false);
            ShowTip(repeatText, matched.skillName + " 已经解锁过了");
            ClearLine();
            return;
        }

        // 第一次解锁
        PlayResultOnAll(true);
        unlockedSkills.Add(matched.skillName);
        UnlockSkill(matched.skillName);
        ShowTip(successText, "解锁技能：" + matched.skillName);
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
        if (skillName == "飞翔")
        {
            // 飞翔还没做，先占个位置
            Debug.Log("解锁飞翔技能！（以后在这里写真正会飞的功能）", this);
        }
        else if (skillName == "攻击")
        {
            // 攻击已经写好了，把开关打开就行
            AttactController.attackUnlocked = true;
        }
    }

    private void ShowTip(TMP_Text tip, string message)
    {
        if (tip == null) return;

        tip.text = message;
        tip.gameObject.SetActive(true);

        // 先取消上一次还没执行的隐藏，避免新提示被提前藏掉
        CancelInvoke(nameof(HideAllTips));
        Invoke(nameof(HideAllTips), tipShowTime);
    }

    private void HideAllTips()
    {
        if (successText != null) successText.gameObject.SetActive(false);
        if (failText != null) failText.gameObject.SetActive(false);
        if (repeatText != null) repeatText.gameObject.SetActive(false);
    }
}
