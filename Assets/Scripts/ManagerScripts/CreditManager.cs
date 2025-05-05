using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CreditManager : MonoBehaviour
{

    
    public float fadeDuration = 1f;
    public float holdDuration = 2f;
    
    private bool creditsPlaying = false;
    private Coroutine creditRoutine;
    public GameObject creditsPanel; // This should be the parent object (e.g., your canvas or panel)
    public List<GameObject> creditImages = new List<GameObject>();

    private void Start()
    {
        PopulateCreditImages();

        creditsPanel.SetActive(false);
    }
    public void PopulateCreditImages()
    {
        creditImages.Clear(); // Clear the list in case it's already populated

        foreach (Transform child in creditsPanel.transform)
        {
            creditImages.Add(child.gameObject);

        }

        Debug.Log("Number of credit images found: " + creditImages.Count);
    }
    private void Update()
    {
        if (creditsPlaying && !GameSettings.arcadeMode && Input.anyKeyDown)
        {
            StopCredits();
        }
    }

    public void StartCredits()
    {
   
        if (!creditsPlaying)
        {
            if (creditRoutine != null)
            {
                StopCoroutine(creditRoutine);
                creditRoutine = null;
            }

            creditsPanel.SetActive(true);
            creditRoutine = StartCoroutine(PlayCreditsSequence());
           
            
        }

    }
    private void StopCredits()
    {
        if (creditRoutine != null)
        {
            StopCoroutine(creditRoutine);
            creditRoutine = null;
        }

        creditsPlaying = false;
        creditsPanel.SetActive(false);
        ResetAllCreditsAlpha();
    }

    private void ResetAllCreditsAlpha()
    {
        foreach (GameObject obj in creditImages)
        {
            RawImage img = obj.GetComponent<RawImage>();
            TMP_Text textChild = obj.GetComponentInChildren<TMP_Text>();

            if (img != null)
            {
                Color c = img.color;
                c.a = 0f;
                img.color = c;
            }
            if (textChild != null)
            {
                Color txtc = textChild.color;
                txtc.a = 0f;
                textChild.color = txtc;
            }
        }
    }


    private IEnumerator PlayCreditsSequence()
    {
        creditsPlaying = true;
        ResetAllCreditsAlpha();
        // Set all to transparent at the start
       
        for (int i = 0; i < creditImages.Count; i++)
        {
            RawImage current = creditImages[i].GetComponent<RawImage>();
            RawImage next = (i + 1 < creditImages.Count) ? creditImages[i + 1].GetComponent<RawImage>() : null;

            if (current != null)
                yield return StartCoroutine(FadeIn(current));

            yield return new WaitForSeconds(holdDuration);

            if (current != null || next != null)
                yield return StartCoroutine(FadeOut(current, next));
            if (next == null)
                yield return new WaitForSeconds(1);
            
        }
        
        creditsPlaying = false;
        creditsPanel.SetActive(false);
    }

    private IEnumerator FadeIn(RawImage image)
    {
        float timer = 0f;
        float startAlpha = image.color.a;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            if (image != null)
            {
                Color c = image.color;
                c.a = Mathf.Lerp(startAlpha, 1f, t);
                image.color = c;
                TMP_Text textChild = image.GetComponentInChildren<TMP_Text>();
                if (textChild != null)
                {
                    Debug.Log("Found a text Child and fading in");
                    Color txtc = textChild.color;
                    txtc.a = Mathf.Lerp(startAlpha, 1f, t); ;
                    textChild.color = txtc;
                }

            }

            yield return null;
        }
    }

    private IEnumerator FadeOut(RawImage current, RawImage next)
    {
        float timer = 0f;

        float startCurrentAlpha = current != null ? current.color.a : 1f;
        float startNextAlpha = next != null ? next.color.a : 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            if (current != null)
            {
                Color c = current.color;
                c.a = Mathf.Lerp(startCurrentAlpha, 0f, t);
                current.color = c;
                TMP_Text textChild = current.GetComponentInChildren<TMP_Text>();
                if (textChild != null)
                {
                    Debug.Log("Found a Text Child and fading out!");
                    Color txtc = textChild.color;
                    txtc.a = Mathf.Lerp(startCurrentAlpha, 0f, t); ;
                    textChild.color = txtc;
                }
            }

            if (next != null)
            {
                Color nc = next.color;
                nc.a = Mathf.Lerp(startNextAlpha, 1f, t);
                next.color = nc;
                TMP_Text nextTextChild = next.GetComponentInChildren<TMP_Text>();
                if (nextTextChild != null)
                {
                    Color txtc = nextTextChild.color;
                    txtc.a = Mathf.Lerp(startNextAlpha, 1f, t); ;
                    nextTextChild.color = txtc;
                }
            }

            yield return null;
        }
    }


}
