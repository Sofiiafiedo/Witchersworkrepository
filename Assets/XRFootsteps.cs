using UnityEngine;

public class XRFootsteps : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip[] grassSteps;
    public AudioClip[] groundSteps;
    public AudioClip[] stoneSteps;

    public float stepInterval = 0.5f;

    private float stepTimer = 0f;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        if (speed > 0.1f)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                stepTimer = 0f;
                PlayFootstep();
            }
        }
    }

    void PlayFootstep()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            string tag = hit.collider.tag;

            if (tag == "Grass") PlayRandom(grassSteps);
            else if (tag == "Ground") PlayRandom(groundSteps);
            else if (tag == "Stone") PlayRandom(stoneSteps);
        }
    }

    void PlayRandom(AudioClip[] clips)
    {
        if (clips.Length > 0)
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
