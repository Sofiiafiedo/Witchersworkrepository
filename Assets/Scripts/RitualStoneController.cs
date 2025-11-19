using UnityEngine;
using TMPro;
using BNG;
using System.Collections;

public class RitualStoneController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI message;
    public TextMeshProUGUI ghostMessage;

    [Header("Ritual")]
    public Grabbable potion;
    public GameObject portalEffect;
    public GameObject ghost;
    public AudioSource ritualAudio;

    [Header("Fade Controllers")]
    public FadeFX fadePortal;
    public FadeFX fadeGhost;

    [Header("Texts")]
    [TextArea] public string textNoPotion = "The stone rejects you. The time has not yet come...";
    [TextArea] public string textHasPotion = "Press A to summon the spirits";
    [TextArea]
    public string ghostText =
        "Your training is finished — you are a real witch now. As your familiar, tied to your heartbeat and your magic, " +
        "I will follow you wherever your adventures take you.\n\nPress A to continue";

    private bool playerNear = false;
    private bool ritualStarted = false;
    private bool ghostReady = false;

    void Update()
    {
        if (ritualStarted) return;

        if (!playerNear)
        {
            message.text = "";
            return;
        }

        if (potion != null && potion.BeingHeld)
        {
            message.text = textHasPotion;

            if (InputBridge.Instance.AButtonDown)
                StartCoroutine(RitualSequence());
        }
        else
        {
            message.text = textNoPotion;
        }
    }

    private IEnumerator RitualSequence()
    {
        ritualStarted = true;
        message.text = "";

        // 1️⃣ Звук играет ровно 1 секунду
        ritualAudio.Play();
        yield return new WaitForSeconds(1f);

        // 2️⃣ Портал появляется плавно 2 сек
        portalEffect.SetActive(true);
        yield return fadePortal.FadeIn(2f);

        // 3️⃣ Через 1 сек после появления портала — появляется призрак (2 сек)
        yield return new WaitForSeconds(1f);

        ghost.SetActive(true);
        yield return fadeGhost.FadeIn(2f);

        // 4️⃣ Портал и звук держатся 4 секунды
        yield return new WaitForSeconds(2f);

        // 5️⃣ Портал исчезает за 2 сек. Звук выключается после исчезновения.
        yield return fadePortal.FadeOut(2f);
        ritualAudio.Stop();
        portalEffect.SetActive(false);

        // 6️⃣ Призрак говорит реплику
        ghostMessage.text = ghostText;
        ghostMessage.gameObject.SetActive(true);
        ghostReady = true;

        // Ждём A
        while (ghostReady)
        {
            if (InputBridge.Instance.AButtonDown)
            {
                ghostReady = false;
                ghostMessage.gameObject.SetActive(false);

                // Передаём позицию игрока в GhostFollower
                Transform player = GameObject.FindGameObjectWithTag("Player").transform;
                ghost.GetComponent<GhostFollower>().StartFollowing(player);
            }

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