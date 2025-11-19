using UnityEngine;

public class GhostFollower : MonoBehaviour
{
    public Transform playerTransform;

    [Header("Follow Settings")]
    public float followDistance = 1f;
    public float followHeight = 0.3f;
    public float followSpeed = 5f;
    public float rotateSpeed = 7f;

    private bool following = false;

    void Update()
    {
        if (!following || playerTransform == null) return;

        // Позиция сбоку игрока
        Vector3 targetPos =
            playerTransform.position
            - playerTransform.right * followDistance
            + Vector3.up * followHeight;

        // Плавное движение
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

        // Плавный поворот на игрока
        Vector3 lookDir = playerTransform.position - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * rotateSpeed);
    }

    // ← Исправленный метод: игрок передаётся НЕ обязательно
    public void StartFollowing(Transform player = null)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        playerTransform = player;
        following = true;
    }
}