using UnityEngine;

public class GhostFollower : MonoBehaviour
{
    public Transform playerTransform;

    [Header("Follow Settings")]
    public float followDistance = 1f;     // Сбоку справа
    public float forwardOffset = 0.1f;      // Чуть сзади
    public float heightOffset = 0.2f;       // Немного выше
    public float followSpeed = 5f;
    public float rotateSpeed = 7f;

    private bool following = false;

    void Update()
    {
        if (!following || playerTransform == null)
            return;

        // --- 1) целевая позиция ---
        Vector3 targetPos =
            playerTransform.position
            + playerTransform.right * followDistance * 1f   // справа
            - playerTransform.forward * forwardOffset       // немного позади
            + Vector3.up * heightOffset;                    // выше

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * followSpeed);

        // --- 2) Смотрим на игрока ---
        Vector3 lookDir = playerTransform.position - transform.position;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(lookDir),
            Time.deltaTime * rotateSpeed
        );
    }

    public void StartFollowing(Transform player)
    {
        playerTransform = player;
        following = true;
    }
}