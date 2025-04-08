using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossCameraSettings : MonoBehaviour
{
    public CinemachineVirtualCamera p1CamLeft;
    public CinemachineVirtualCamera p1CamRight;
    public CinemachineVirtualCamera p2CamLeft;
    public CinemachineVirtualCamera p2CamRight;
    public Image bossHP;
    public GameObject player1BossIndicator;
    public GameObject player2BossIndicator;
    public GameManager gameManager;


    public float targetOrthographicSize = 15f;
    private float lerpSpeed = 1f;

    public AudioSource audioSource;
    public AudioClip newMusicClip;


    // Start is called before the first frame update
    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Player2"))
        {
            StartCoroutine(LerpOrthographicSize(p1CamLeft, targetOrthographicSize));
            StartCoroutine(LerpOrthographicSize(p1CamRight, targetOrthographicSize));
            StartCoroutine(LerpOrthographicSize(p2CamLeft, targetOrthographicSize));
            StartCoroutine(LerpOrthographicSize(p2CamRight, targetOrthographicSize));
            if (gameManager.singlePlayerMode != true)
            {
                player1BossIndicator.SetActive(true);
            }
            player2BossIndicator.SetActive(true);
            bossHP.gameObject.SetActive(true);
            ChangeBackgroundMusic();
        }
    }

    private IEnumerator LerpOrthographicSize(CinemachineVirtualCamera cam, float targetSize)
    {
        float startSize = cam.m_Lens.OrthographicSize;
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * lerpSpeed;
            cam.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, progress);
            yield return null;
        }

        cam.m_Lens.OrthographicSize = targetSize;
    }

    private void ChangeBackgroundMusic()
    {
        if (audioSource != null && newMusicClip != null)
        {
            audioSource.Stop();
            audioSource.clip = newMusicClip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource or newMusicClip is not set in BossCameraSettings.");
        }
    }
}