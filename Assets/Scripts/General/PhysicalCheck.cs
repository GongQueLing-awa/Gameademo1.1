using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalCheck : MonoBehaviour
{
    [Header ("检测参数")]
    public float checkRaduis;
    public LayerMask groundLayer;
    public Vector2 bottomOffset;
    [Header("人物状态")]
    public bool isGround;
    void Update()
    {
        Check();
    }
    public void Check()
    {
        isGround = Physics2D.OverlapCircle((Vector2)transform.position+bottomOffset,checkRaduis, groundLayer);
    }
    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere ((Vector2)transform.position + bottomOffset, checkRaduis);
    }
}
