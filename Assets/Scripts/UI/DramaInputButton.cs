using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MyGame.UI
{
    /// <summary>
    /// 剧情输入按钮：挂在 ShenButton 这类带 Button 后缀的物体上。
    /// 它只管两件事：被点击时告诉输入管理器"轮到我了"；
    /// 鼠标放上来/离开时，让管理器显示/隐藏提示框。
    /// </summary>
    public class DramaInputButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        private void Awake()
        {
            // 拿到自己身上的 Button 组件，挂上点击后要执行的方法
            GetComponent<Button>().onClick.AddListener(Clicked);
        }

        private void Clicked()
        {
            // 往父辈物体找输入管理器（它挂在 DramaPages 上）
            DramaInputManager manager = GetComponentInParent<DramaInputManager>();
            if (manager != null)
            {
                // 把自己交出去，管理器就知道这次是为哪个按钮输入
                manager.OpenInput(this);
            }
        }

        // 鼠标进入按钮范围时自动调用
        public void OnPointerEnter(PointerEventData eventData)
        {
            DramaInputManager manager = GetComponentInParent<DramaInputManager>();
            if (manager != null)
            {
                // 把鼠标当前屏幕坐标一起传过去，提示框第一帧就能出现在正确位置
                manager.OpenTooltip(this, eventData.position);
            }
        }

        // 鼠标在按钮上移动时自动调用，用来让提示框跟着鼠标走
        public void OnPointerMove(PointerEventData eventData)
        {
            DramaInputManager manager = GetComponentInParent<DramaInputManager>();
            if (manager != null)
            {
                manager.MoveTooltip(this, eventData.position);
            }
        }

        // 鼠标离开按钮范围时自动调用
        public void OnPointerExit(PointerEventData eventData)
        {
            DramaInputManager manager = GetComponentInParent<DramaInputManager>();
            if (manager != null)
            {
                manager.CloseTooltip(this);
            }
        }
    }
}
