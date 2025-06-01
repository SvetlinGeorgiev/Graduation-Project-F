using UnityEngine;

public class ControlsIntro : MonoBehaviour
{
    public GameObject introPanel; 

    void Start()
    {
        introPanel.SetActive(true);
        
    }

    void Update()
    {
        if (introPanel.activeSelf && Input.anyKeyDown)
        {
            introPanel.SetActive(false);
            
        }
    }
}
