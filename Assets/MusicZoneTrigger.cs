using UnityEngine;

public class MusicZoneTrigger : MonoBehaviour
{
    public AudioSource musicSource; // ссылка на ваш AudioSource
    public float targetVolume = 0.2f; // громкость в зоне
    public float fadeSpeed = 1f; // скорость затухания

    private float originalVolume;

    void Start()
    {
        if (musicSource == null)
        {
            Debug.LogError("AudioSource не назначен!");
            return;
        }
        originalVolume = musicSource.volume;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // убедитесь, что у игрока тег "Player"
        {
            StopAllCoroutines();
            StartCoroutine(FadeVolume(musicSource.volume, targetVolume));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeVolume(musicSource.volume, originalVolume));
        }
    }

    System.Collections.IEnumerator FadeVolume(float start, float end)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * fadeSpeed;
            musicSource.volume = Mathf.Lerp(start, end, t);
            yield return null;
        }
        musicSource.volume = end;
    }
}
