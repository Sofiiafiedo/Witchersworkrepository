using UnityEngine;
using TMPro;
using BNG;
using System.Collections;

public class ChestController : MonoBehaviour
{
    [Header("Chest Parts")]
    public Transform lid;               // Крышка сундука
    public float openAngle = -100f;     // Угол открытия (можно подстроить)
    public float openSpeed = 2f;        // Скорость открытия

    [Header("Key Reference")]
    public Grabbable key;               // Ключ из сцены (с Grabbable)

    [Header("UI")]
    public TextMeshProUGUI message;     // Табличка над сундуком

    [Header("Sound")]
    public AudioSource chestAudio;      // Источник звука
    public AudioClip openSound;         // Звук открытия сундука

    private bool playerNear = false;
    private bool chestOpened = false;

    void Update()
    {
        if (chestOpened) return;

        if (playerNear)
        {
            if (key != null && key.BeingHeld)
            {
                message.text = "Press A to open the chest";

                if (InputBridge.Instance.AButtonDown)
                {
                    StartCoroutine(OpenChest());
                }
            }
            else
            {
                message.text = "Closed";
            }
        }
        else
        {
            message.text = "";
        }
    }

    private IEnumerator OpenChest()
    {
        chestOpened = true;
        message.text = "";

        // Проигрываем звук
        if (chestAudio != null && openSound != null)
        {
            chestAudio.PlayOneShot(openSound);
        }

        // Удаляем ключ
        if (key != null)
        {
            Destroy(key.gameObject);
        }

        // Анимация открытия крышки
        Quaternion startRot = lid.localRotation;
        Quaternion endRot = Quaternion.Euler(openAngle, 0, 0); // по X обычно открывается

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            lid.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            message.text = "";
        }
    }
}
