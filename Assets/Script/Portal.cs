using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Transform destination; // 텔레포트 할 목적지
    [SerializeField] private PolygonCollider2D nextRoomConfiner; // 갈아끼울 울타리(다음 방)
    [SerializeField] private CinemachineCamera virtualCamera; // 시네머신 카메라
    [SerializeField] private CinemachineConfiner2D cameraConfiner; // 시네머신 컨파이너

    [SerializeField] private float portalCooldown = 0.2f; // 이동 쿨타임
    [SerializeField] private static float lastPortalTime = 0; // 마지막 이동시간



    void Awake()
    {   
        if (cameraConfiner == null)
        {
            Debug.Log("카메라에 컨파이너 안달림.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Time.time > lastPortalTime + portalCooldown) // 닿은게 플레이어일때 && 쿨타임 지났을때
        {
            lastPortalTime = Time.time;
            Vector3 delta = destination.position - collision.transform.position; // 델타
            collision.transform.position = destination.position; // Player 이동

            // 시네머신한테 delta만큼 워프했다고 알려줌
            virtualCamera.OnTargetObjectWarped(collision.transform, delta);

            cameraConfiner.BoundingShape2D = nextRoomConfiner; // 새로 이동한 방에 카메라 이동
            cameraConfiner.InvalidateBoundingShapeCache(); // 다시 계산 함으로써 데이터 초기화, 이전 방의 캐시값 제거

        }
    }
}
