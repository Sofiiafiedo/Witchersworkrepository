using UnityEngine;
using System.Collections;

public class PotionAppearEffect : MonoBehaviour
{
    [Header("References")]
    public GameObject portalEffect;      // Healing – объект с Particle System
    public AudioSource portalAudio;      // Музыка/звук портала
    public GameObject magicPotion;       // MagicPotion – объект зелья

    [Header("Timings")]
    public float fadeInDuration = 2f;    // Плавное появление портала + музыки
    public float delayBeforePotion = 2f; // Через 2 сек после старта — показать зелье
    public float potionHoldDuration = 2f;// Зелье держится 2 сек с порталом и музыкой
    public float fadeOutDuration = 2f;   // Плавное исчезновение портала + музыки

    private bool sequenceRunning = false;

    void Awake()
    {
        // На всякий случай — всё выключаем в начале
        if (portalEffect != null)
            portalEffect.SetActive(false);

        if (magicPotion != null)
            magicPotion.SetActive(false);

        if (portalAudio != null)
            portalAudio.volume = 0f;
    }

    /// <summary>
    /// Запускаем последовательность появления портала, музыки и зелья.
    /// ЭТОТ МЕТОД НУЖНО ВЫЗВАТЬ ПОСЛЕ НАЖАТИЯ A (там, где у тебя логика ведьмы/квеста).
    /// </summary>
    public void StartPotionSequence()
    {
        if (sequenceRunning) return;
        StartCoroutine(PotionSequence());
    }

    private IEnumerator PotionSequence()
    {
        sequenceRunning = true;

        // 1️⃣ Включаем портал и звук (оба появляются плавно)
        if (portalEffect != null)
            portalEffect.SetActive(true);

        if (portalAudio != null)
        {
            portalAudio.volume = 0f;
            portalAudio.Play();
        }

        float t = 0f;

        // Плавный fade-in портала (визуал пусть делает сам Particle System, мы только звук делаем плавным)
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeInDuration);

            if (portalAudio != null)
                portalAudio.volume = k;

            yield return null;
        }

        // 2️⃣ Через 2 секунды после начала (по ТЗ) — показываем зелье
        // Если хочешь: задержка считается с момента нажатия A.
        yield return new WaitForSeconds(delayBeforePotion);

        if (magicPotion != null)
            magicPotion.SetActive(true);

        // 3️⃣ Зелье висит 2 секунды вместе с порталом и музыкой
        yield return new WaitForSeconds(potionHoldDuration);

        // 4️⃣ Плавно гасим портал и музыку
        t = 0f;
        float startVolume = portalAudio != null ? portalAudio.volume : 1f;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(t / fadeOutDuration);

            if (portalAudio != null)
                portalAudio.volume = startVolume * k;

            yield return null;
        }

        if (portalAudio != null)
        {
            portalAudio.Stop();
            portalAudio.volume = 0f;
        }

        if (portalEffect != null)
            portalEffect.SetActive(false);

        // Зелье остаётся, как ты и хотела
        sequenceRunning = false;
    }
}