using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Auth : MonoBehaviour
{
    // Panel references
    [Header("Panels References")]
    [SerializeField] private GameObject signInPanel;
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject feedPanel;
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private GameObject footerPanel;

    // Button references
    [Header("Button References")]
    [SerializeField] private Button LoginBtn;
    [SerializeField] private Button HomeBtn;
    [SerializeField] private Button FeedBtn;
    [SerializeField] private Button ProfileBtn;
    [SerializeField] private Button LogoutBtn;
    
    private Transform ActiveHomeBtn;
    private Transform ActiveFeedBtn;
    private Transform ActiveProfileBtn;

    [Header("AR Buttons")]
    [SerializeField] private Button WhiteSofaHome;
    [SerializeField] private Button WhiteSofaFeed;
     [SerializeField] private Button BrownSofaHome;
    [SerializeField] private Button BrownSofaFeed;
    [SerializeField] private Button LightBrownSofaHome;
    [SerializeField] private Button LightBrownSofaFeed;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;

    public static int selected = 0;
    public static int previousPanel = -1; // -1 means show login, 0 = home, 1 = feed


    void Start()
    {
        SetupButtonListeners();
        SetUpActiveBtns();
        
        
        if (previousPanel == 0) // Home panel
        {
            // Return to Home panel if we came from home
            if (homePanel != null)
            {
                MoveOn__Home();
                Debug.Log("Returning to Home panel from AR");
            }
        }
        else if (previousPanel == 1) // Feed panel
        {
            // Return to Feed panel if we came from feed
            if (feedPanel != null)
            {
                MoveOn__Feed();
                Debug.Log("Returning to Feed panel from AR");
            }
        }
        else
        {
            // First time loading or explicitly set to login (-1)
            // Start with login panel - don't need to do anything as this is the default
            Debug.Log("Starting with login panel, previousPanel: " + previousPanel);
        }
    }

    private void SetupButtonListeners()
    {
        if (LoginBtn != null)
        {
            LoginBtn.onClick.AddListener(Switch_Login);
        }

        if (HomeBtn != null)
        {
            HomeBtn.onClick.AddListener(MoveOn__Home);
        }

        if (FeedBtn != null)
        {
            FeedBtn.onClick.AddListener(MoveOn__Feed);
        }

        if (ProfileBtn != null)
        {
            ProfileBtn.onClick.AddListener(MoveOn_Profile);
        }

        if (LogoutBtn != null)
        {
            LogoutBtn.onClick.AddListener(Switch_Logout);
        }
        
        // Setup AR furniture buttons
        SetupARButtonListeners();
    }

    private void SetUpActiveBtns()
    {
        if (HomeBtn != null && HomeBtn.transform.childCount > 0)
            ActiveHomeBtn = HomeBtn.transform.GetChild(0);

        if (FeedBtn != null && FeedBtn.transform.childCount > 0)
            ActiveFeedBtn = FeedBtn.transform.GetChild(0);
        
        if (ProfileBtn != null && ProfileBtn.transform.childCount > 0)
            ActiveProfileBtn = ProfileBtn.transform.GetChild(0);
    }
    
    private void Switch_Login()
    {
        SwitchToPanel(homePanel);
    }
    
    private void MoveOn__Home()
    {
        ActiveHomeBtn.gameObject.SetActive(true);
        ActiveFeedBtn.gameObject.SetActive(false);
        ActiveProfileBtn.gameObject.SetActive(false);

        SwitchToPanel(homePanel);
    }
    
    private void MoveOn__Feed()
    {
        ActiveHomeBtn.gameObject.SetActive(false);
        ActiveFeedBtn.gameObject.SetActive(true);
        ActiveProfileBtn.gameObject.SetActive(false);
        SwitchToPanel(feedPanel);
    }
    
    private void MoveOn_Profile()
    {
        ActiveHomeBtn.gameObject.SetActive(false);
        ActiveFeedBtn.gameObject.SetActive(false);
        ActiveProfileBtn.gameObject.SetActive(true);
        SwitchToPanel(profilePanel);
    }
    
    private void Switch_Logout()
    {
        HomeBtn.gameObject.SetActive(true);
        ActiveHomeBtn.gameObject.SetActive(true);
        ActiveFeedBtn.gameObject.SetActive(false);
        ActiveProfileBtn.gameObject.SetActive(false);
        
        // Reset the previousPanel to -1 to ensure login panel is shown next time
        previousPanel = -1;
        
        // Clear email and password fields
        if (emailField != null)
        {
            emailField.text = "";
        }
        
        if (passwordField != null)
        {
            passwordField.text = "";
        }
        
        SwitchToPanel(signInPanel);
    }

    private void SwitchToPanel(GameObject panelToActivate)
    {
        // Deactivate all panels
        if (signInPanel != null) signInPanel.SetActive(false);
        if (homePanel != null) homePanel.SetActive(false);
        if (feedPanel != null) feedPanel.SetActive(false);
        if (profilePanel != null) profilePanel.SetActive(false);

        // Activate the requested panel
        if (panelToActivate != null)
        {
            panelToActivate.SetActive(true);
        }
        
        //footer visibility
        if (footerPanel != null)
        {
            bool showFooter = (panelToActivate == homePanel || panelToActivate == feedPanel || 
                               panelToActivate == profilePanel);
            footerPanel.SetActive(showFooter);
        }
    }
    
    // Setup AR furniture button listeners
    private void SetupARButtonListeners()
    {
        // White Sofa buttons
        if (WhiteSofaHome != null)
        {
            WhiteSofaHome.onClick.AddListener(() => {
                selected = 0;
                previousPanel = 0; // Coming from Home panel
                Debug.Log("White Sofa selected (Home)");
                LoadARScene();
            });
        }
        
        if (WhiteSofaFeed != null)
        {
            WhiteSofaFeed.onClick.AddListener(() => {
                selected = 0;
                previousPanel = 1; // Coming from Feed panel
                Debug.Log("White Sofa selected (Feed)");
                LoadARScene();
            });
        }
        
        // Brown Sofa buttons
        if (BrownSofaHome != null)
        {
            BrownSofaHome.onClick.AddListener(() => {
                selected = 1;
                previousPanel = 0; // Coming from Home panel
                Debug.Log("Brown Sofa selected (Home)");
                LoadARScene();
            });
        }
        
        if (BrownSofaFeed != null)
        {
            BrownSofaFeed.onClick.AddListener(() => {
                selected = 1;
                previousPanel = 1; // Coming from Feed panel
                Debug.Log("Brown Sofa selected (Feed)");
                LoadARScene();
            });
        }
        
        // Light Brown Sofa buttons
        if (LightBrownSofaHome != null)
        {
            LightBrownSofaHome.onClick.AddListener(() => {
                selected = 2;
                previousPanel = 0; // Coming from Home panel
                Debug.Log("Light Brown Sofa selected (Home)");
                LoadARScene();
            });
        }
        
        if (LightBrownSofaFeed != null)
        {
            LightBrownSofaFeed.onClick.AddListener(() => {
                selected = 2;
                previousPanel = 1; // Coming from Feed panel
                Debug.Log("Light Brown Sofa selected (Feed)");
                LoadARScene();
            });
        }
    }
    
    // Load the AR scene
    private void LoadARScene()
    {
        Debug.Log("Loading AR scene with selected item: " + selected);
        UnityEngine.SceneManagement.SceneManager.LoadScene("AR");
    }
}