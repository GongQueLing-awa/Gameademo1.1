using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl : MonoBehaviour
{
    private CinemachineConfiner2D confiner2D;
    public CinemachineImpulseSource impulseSource;
    public VoidEventSO cameraShakeEvent;


    private void OnEnable()
    {
        cameraShakeEvent.OnEventRaised += ShakeCamera;
    }

    private void OnDisable()
    {
        cameraShakeEvent.OnEventRaised -= ShakeCamera;
    }
    
    private void ShakeCamera()
    {
        impulseSource.GenerateImpulse();
    }

    private void Awake()
    {
        confiner2D = GetComponent<CinemachineConfiner2D>();
    }

    //TODO: 以后可以考虑在场景切换时调用这个函数，来更新相机的边界
    private void Start()
    {
        GetNewCameraBounds();
    }

    private void GetNewCameraBounds()
    {
        var obj=GameObject.FindGameObjectWithTag("Bounds");
        if (obj == null) return;
        confiner2D.m_BoundingShape2D = obj.GetComponent<Collider2D>();

        confiner2D.InvalidateCache();

    }





}
