using UnityEngine;
using UnityEngine.UI;

public class UIContentToggle : MonoBehaviour
{
    public Button mainButton;       // The button that shows the content
    public GameObject contentPanel; // Panel that holds the text, image, and close button
    public Button closeButton;      // Close button inside the content panel

    void Start()
    {
        // Hide the content initially
        contentPanel.SetActive(false);

        // Add listeners
        mainButton.onClick.AddListener(ShowContent);
        closeButton.onClick.AddListener(HideContent);
    }

    void ShowContent()
    {
        contentPanel.SetActive(true);
    }

    void HideContent()
    {
        contentPanel.SetActive(false);
    }
}
