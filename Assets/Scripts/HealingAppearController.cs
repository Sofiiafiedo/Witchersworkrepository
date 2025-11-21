using UnityEngine;
using System.Collections;

public class HealingAppearController : MonoBehaviour
{
    [Header("Portal object (HEALING)")]
    public GameObject healingPortal;       // Объект Healing (с Particle System)
    public AudioSource healingAudio;       // AudioSource на этом портале

    [Header("Potion object")]
    public GameObject magicPotion;         // Объект MagicPotion (зелье)

    [Header("Timings (seconds)")]
    public float portalFadeInDuration = 1f;     // Сколько секунд портал и звук появляются
    public float delayBeforePotion = 2f;        // Через сколько секунд после старта появляется зелье
    public float potionVisibleDuration = 2f;    // Сколько секунд зелье висит вместе с порталом
    public float portalFadeOutDuration = 1f;    // Сколько секунд портал и звук исчезают

    private ParticleSystem portalPS;
    private Color portalBaseColor;
    private float baseVolume;
    private bool isRunning = false;

    void Awake()
    {
        // Кэшируем Particle System и базовый цвет
        if (healingPortal != null)
        {
            portalPS = healingPortal.GetComponent<ParticleSystem>();

            if (portalPS != null)
            {
                var main = portalPS.main;
                portalBaseColor = main.startColor.color;

                // Делаем портал невидимым при старте (альфа = 0)
                Color c = portalBaseColor;
                c.a = 0f;
                main.startColor = c;
            }

            healingPortal.SetActive(false);
        }

        // Кэшируем громкость звука
        if (healingAudio != null)
        {
            baseVolume = healingAudio.volume;
            healingAudio.volume = 0f;
            healingAudio.playOnAwake = false;
        }

        // Зелье должно быть выключено до ритуала
        if (magicPotion != null)
        {
            magicPotion.SetActive(false);
        }
    }

    /// <summary>
    /// Запускаем всю последовательность портала / звука / зелья.
    /// Вызывается из WitchController.
    /// </summary>
    public void StartHealingAppearance()
    {
        if (!isRunning)
        {
            StartCoroutine(HealingSequence());
        }
    }

    private IEnumerator HealingSequence()
    {
        isRunning = true;
        float startTime = Time.time;

        // ---------- 1) Включаем портал и звук (но они пока с альфой / громкостью 0) ----------
        if (healingPortal != null)
        {
            healingPortal.SetActive(true);
        }

        if (portalPS != null)
        {
            portalPS.Clear();
            portalPS.Play();
        }

        if (healingAudio != null)
        {
            if (!healingAudio.isPlaying)
                healingAudio.Play();

            healingAudio.volume = 0f;
        }

        // ---------- 2) Плавный fade-in портала и звука ----------
        float t = 0f;
        while (t < portalFadeInDuration)
        {
            t += Time.deltaTime;
            float k = portalFadeInDuration > 0 ? Mathf.Clamp01(t / portalFadeInDuration) : 1f;

            // Портал
            if (portalPS != null)
            {
                var main = portalPS.main;
                Color c = portalBaseColor;
                c.a = portalBaseColor.a * k;
                main.startColor = c;
            }

            // Музыка
            if (healingAudio != null)
            {
                healingAudio.volume = Mathf.Lerp(0f, baseVolume, k);
            }

            // Появление зелья ровно через delayBeforePotion секунд от начала
            if (magicPotion != null &&
                !magicPotion.activeSelf &&
                Time.time - startTime >= delayBeforePotion)
            {
                magicPotion.SetActive(true);
            }

            yield return null;
        }// Если fade-in закончился раньше, чем delayBeforePotion — ждём остаток
        float timeSinceStart = Time.time - startTime;
        if (magicPotion != null && !magicPotion.activeSelf)
        {
            float extraWait = Mathf.Max(0f, delayBeforePotion - timeSinceStart);
            if (extraWait > 0f)
                yield return new WaitForSeconds(extraWait);

            magicPotion.SetActive(true);
            timeSinceStart = Time.time - startTime;
        }

        // ---------- 3) Держим всё вместе potionVisibleDuration секунд ----------
        if (potionVisibleDuration > 0f)
        {
            yield return new WaitForSeconds(potionVisibleDuration);
        }

        // ---------- 4) Плавный fade-out портала и звука ----------
        t = 0f;
        while (t < portalFadeOutDuration)
        {
            t += Time.deltaTime;
            float k = portalFadeOutDuration > 0 ? Mathf.Clamp01(t / portalFadeOutDuration) : 1f;
            float rev = 1f - k;

            if (portalPS != null)
            {
                var main = portalPS.main;
                Color c = portalBaseColor;
                c.a = portalBaseColor.a * rev;
                main.startColor = c;
            }

            if (healingAudio != null)
            {
                healingAudio.volume = Mathf.Lerp(baseVolume, 0f, k);
            }

            yield return null;
        }

        // ---------- 5) Полностью выключаем портал и звук (зелье ОСТАЁТСЯ) ----------
        if (portalPS != null)
        {
            portalPS.Stop();
        }

        if (healingPortal != null)
        {
            healingPortal.SetActive(false);
        }

        if (healingAudio != null)
        {
            healingAudio.Stop();
            healingAudio.volume = baseVolume;
        }

        isRunning = false;
    }
}