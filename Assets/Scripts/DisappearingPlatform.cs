using UnityEngine;
using System.Collections;

public class DisappearingPlatform : MonoBehaviour
{
    public float disappearDelay = 1f; 
    public float reappearDelay = 3f; 
    public float colorChangeDuration = 0.5f; 

    private Collider platformCollider;
    private Renderer platformRenderer;
    private Color originalColor;

    private void Start()
    {
        platformCollider = GetComponent<Collider>();
        platformRenderer = GetComponent<Renderer>();
        originalColor = platformRenderer.material.color; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) 
        {
            StartCoroutine(HandlePlatform());
        }
    }

    private IEnumerator HandlePlatform()
    {
        yield return StartCoroutine(ChangeColor(Color.red, colorChangeDuration)); 

        yield return new WaitForSeconds(disappearDelay - colorChangeDuration); 

        platformRenderer.enabled = false;
        platformCollider.enabled = false;

        yield return new WaitForSeconds(reappearDelay);

        platformRenderer.enabled = true;
        platformCollider.enabled = true;
        StartCoroutine(ChangeColor(originalColor, colorChangeDuration)); 
    }

    private IEnumerator ChangeColor(Color targetColor, float duration)
    {
        float elapsedTime = 0;
        Color startColor = platformRenderer.material.color;

        while (elapsedTime < duration)
        {
            platformRenderer.material.color = Color.Lerp(startColor, targetColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        platformRenderer.material.color = targetColor;
    }
}
