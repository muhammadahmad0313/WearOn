using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Back : MonoBehaviour
{
    [SerializeField] private Button backButton;

    // Start is called before the first frame update
    void Start()
    {
        // Add click listener to back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBackToAuth);
        }
        else
        {
            // Try to get the button component from this GameObject if not assigned
            backButton = GetComponent<Button>();
            if (backButton != null)
            {
                backButton.onClick.AddListener(GoBackToAuth);
            }
            else
            {
                Debug.LogWarning("Back button reference is missing. Please assign it in the inspector.");
            }
        }
    }

    // Method to handle back button click
    public void GoBackToAuth()
    {
        Debug.Log("Back button clicked. Loading Auth scene with previousPanel: " + Auth.previousPanel);
        SceneManager.LoadScene("Auth");
        // The static variable in Auth class will be preserved across scenes
    }

    // Update is called once per frame
    void Update()
    {
        // Also handle Android back button press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoBackToAuth();
        }
    }
}
