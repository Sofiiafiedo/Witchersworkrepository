using UnityEngine;
using TMPro;
using BNG;   // Для Grabbable и VR-кнопок

public class GateController : MonoBehaviour
{
    [Header("Gate Parts")]
    public Transform leftGate;
    public Transform rightGate;
    public float openAngle = 90f;
    public float openSpeed = 2f;

    [Header("UI")]
    public TextMeshProUGUI message;

    [Header("Key Reference")]
    public Grabbable key; // Проверяем, держит ли игрок ключ

    [Header("Sound")]
    public AudioSource gateAudio;
    public AudioClip openSound;

    private bool playerNear = false;
    private bool gateOpened = false;

    void Update()
    {
        if (gateOpened) return;

        if (playerNear)
        {
            // Ключ в руке?
            if (key != null && key.BeingHeld)
            {
                message.text = "Press A to open the gates";

                // Нажата кнопка A (BNG)
                if (InputBridge.Instance.AButtonDown)
                {
                    StartCoroutine(OpenGate());
                }
            }
            else
            {
                message.text = "You need a key";
            }
        }
        else
        {
            message.text = "";
        }
    }

    private System.Collections.IEnumerator OpenGate()
    {
        gateOpened = true;
        message.text = "";

        // Звук открытия
        if (gateAudio != null && openSound != null)
        {
            gateAudio.PlayOneShot(openSound);
        }

        Quaternion leftStart = leftGate.localRotation;
        Quaternion rightStart = rightGate.localRotation;

        Quaternion leftTarget = Quaternion.Euler(0, -openAngle, 0);
        Quaternion rightTarget = Quaternion.Euler(0, openAngle, 0);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;

            leftGate.localRotation = Quaternion.Slerp(leftStart, leftTarget, t);
            rightGate.localRotation = Quaternion.Slerp(rightStart, rightTarget, t);

            yield return null;
        }

        // Удаляем ключ после открытия
        Destroy(key.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))   // Вот так! Без PlayerController
        {
            playerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
        }
    }
}