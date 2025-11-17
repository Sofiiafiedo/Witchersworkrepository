using UnityEngine;

public class FootstepSurface : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip[] grassSteps;
    public AudioClip[] groundSteps;
    public AudioClip[] stoneSteps;

    public float stepInterval = 0.4f;

    private float stepTimer;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller.isGrounded && controller.velocity.magnitude > 0.2f)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                stepTimer = 0f;
                DetectSurfaceAndPlay();
            }
        }
    }

    void DetectSurfaceAndPlay()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            string tag = hit.collider.tag;

            if (tag == "Grass")
                PlayRandom(grassSteps);

            else if (tag == "Ground")
                PlayRandom(groundSteps);

            else if (tag == "Stone")
                PlayRandom(stoneSteps);
        }
    }

    void PlayRandom(AudioClip[] clips)
    {
        if (clips.Length > 0)
        {
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }
}