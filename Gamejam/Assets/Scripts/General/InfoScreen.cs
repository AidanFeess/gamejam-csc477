using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoScreen : MonoBehaviour
{
    public static InfoScreen Instance { get; private set; }

    [Header("References")]
    public GameObject panel;
    public TextMeshProUGUI descriptionText; 
    public Image iconImage;                 

    private float savedTimeScale = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panel != null) panel.SetActive(false);
    }

    public void Show(string description, Sprite icon)
    {
        if (panel == null) return;

        if (descriptionText != null) descriptionText.text = description;
        if (iconImage != null) iconImage.sprite = icon;

        panel.SetActive(true);
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    // Wire this to the close button's OnClick
    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        Time.timeScale = savedTimeScale;
    }
}