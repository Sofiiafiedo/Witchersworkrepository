using UnityEngine;
using System.Collections;

public class MagicPotionSpawn : MonoBehaviour
{
    [Header("References")]
    public GameObject potionPrefab;        // Префаб зелья
    public Transform spawnPoint;           // Точка появления зелья

    public GameObject portalObject;        // Портал (объект со скриптом FadeFX)
    public FadeFX portalFade;              // Скрипт FadeFX портала

    public AudioSource portalSound;        // Звук портала (должен быть с 0 громкостью)

    [Header("Timings")]
    public float soundFadeTime = 1f;
    public float portalFadeIn = 2f;
    public float portalStayTime = 2f;
    public float portalFadeOut = 2f;

    private bool spawned = false;

    public void SpawnPotion()
    {
        if (spawned) return;
        spawned = true;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        // 1) Включаем портал
        portalObject.SetActive(true);

        // Готовим звук
        portalSound.volume = 0f;
        portalSound.Play();

        // 1.1) Плавное появление звука
        float t = 0;
        while (t < soundFadeTime)
        {
            t += Time.deltaTime;
            portalSound.volume = Mathf.Lerp(0, 1, t / soundFadeTime);
            yield return null;
        }

        // 2) Плавное появление портала
        yield return StartCoroutine(portalFade.FadeIn(portalFadeIn));

        // 3) Спавним зелье резко
        Instantiate(potionPrefab, spawnPoint.position, spawnPoint.rotation);

        // 4) Портал и звук держатся
        yield return new WaitForSeconds(portalStayTime);

        // 5) Плавное исчезновение портала
        yield return StartCoroutine(portalFade.FadeOut(portalFadeOut));

        // 6) Плавно выключаем звук
        float t2 = 0;
        float startVol = portalSound.volume;
        while (t2 < soundFadeTime)
        {
            t2 += Time.deltaTime;
            portalSound.volume = Mathf.Lerp(startVol, 0, t2 / soundFadeTime);
            yield return null;
        }
        portalSound.Stop();

        // 7) Выключаем портал
        portalObject.SetActive(false);
    }
}
