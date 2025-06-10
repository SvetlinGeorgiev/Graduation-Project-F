using UnityEngine;

public class BossHealthBar : MonoBehaviour
{
    public static BossHealthBar Instance { get; private set; }

    public GameObject healthBarUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public void Show()
    {
        healthBarUI.SetActive(true);
    }

    public void Hide()
    {
        healthBarUI.SetActive(false);
    }
}
