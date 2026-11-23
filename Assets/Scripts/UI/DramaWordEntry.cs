using UnityEngine;

namespace MyGame.UI
{
    /// <summary>
    /// 剧情词条：只记录"解锁状态"。
    /// 显示/隐藏不再由这个脚本管，直接用物体自身的 SetActive
    /// （就是 Inspector 左上角物体名字旁边的那个勾选框）。
    /// </summary>
    public class DramaWordEntry : MonoBehaviour
    {
        [Tooltip("勾选表示这个词条已解锁；只有已解锁的词条，输入正确答案后才会显示出来")]
        public bool unlocked;
    }
}
