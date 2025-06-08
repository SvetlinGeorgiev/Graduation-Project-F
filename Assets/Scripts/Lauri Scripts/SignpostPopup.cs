using UnityEngine;
using TMPro;
using System.Collections;

public class SignpostPopup : MonoBehaviour
{
    public TextMeshProUGUI popupText; // Assign in Inspector
    public float displayDuration = 3f;

    private Coroutine hideCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && popupText != null)
        {
            popupText.gameObject.SetActive(true);

            // Restart the timer if already running
            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideAfterDelay());
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        popupText.gameObject.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && popupText != null)
        {
            popupText.gameObject.SetActive(false);
            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);
        }
    }
}
