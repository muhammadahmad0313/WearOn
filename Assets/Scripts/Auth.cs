using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using UnityEngine.EventSystems;

public class Auth : MonoBehaviour
{
    // Panel references
    [Header("Panels References")]
    [SerializeField] private GameObject signInPanel;
    [SerializeField] private GameObject signUpPanel;
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject feedPanel;
    [SerializeField] private GameObject cartPanel;
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private GameObject footerPanel;

    // Button references
    [Header("Button References")]
    [SerializeField] private Button SignUpBtn;
    [SerializeField] private Button LoginBtn;
    [SerializeField] private Button HomeBtn;
    [SerializeField] private Button FeedBtn;
    [SerializeField] private Button CartBtn;
    [SerializeField] private Button ProfileBtn;
    [SerializeField] private Button LogoutBtn;
    [SerializeField] private Button SignUpTextBtn;
    [SerializeField] private Button LoginTextBtn;

    
    private Transform ActiveHomeBtn;
    private Transform ActiveFeedBtn;
    private Transform ActiveCartBtn;
    private Transform ActiveProfileBtn;

    void Start()
    {
        SetupButtonListeners();
        SetUpActiveBtns();
    }

    private void SetupButtonListeners()
    {
        if (SignUpBtn != null)
        {
            SignUpBtn.onClick.AddListener(Switch_SignUp);
        }

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

        if (CartBtn != null)
        {
            CartBtn.onClick.AddListener(MoveOn__Cart);
        }

        if (ProfileBtn != null)
        {
            ProfileBtn.onClick.AddListener(MoveOn_Profile);
        }

        if (LogoutBtn != null)
        {
            LogoutBtn.onClick.AddListener(Switch_Logout);
        }

        if (SignUpTextBtn != null)
        {
            SignUpTextBtn.onClick.AddListener(Switch_SignUpPanel);
        }

        if (LoginTextBtn != null)
        {
            LoginTextBtn.onClick.AddListener(Switch_SignUp);
        }
    }

    private void SetUpActiveBtns()
    {
        if (HomeBtn != null && HomeBtn.transform.childCount > 0)
            ActiveHomeBtn = HomeBtn.transform.GetChild(0);

        if (FeedBtn != null && FeedBtn.transform.childCount > 0)
            ActiveFeedBtn = FeedBtn.transform.GetChild(0);

        if (CartBtn != null && CartBtn.transform.childCount > 0)
            ActiveCartBtn = CartBtn.transform.GetChild(0);
        
         if (ProfileBtn != null && ProfileBtn.transform.childCount > 0)
            ActiveProfileBtn = ProfileBtn.transform.GetChild(0);
        
    
    }
    
    private void Switch_SignUp()
    {
        SwitchToPanel(signInPanel);
    }
    
    private void Switch_Login()
    {
        SwitchToPanel(homePanel);
    }
    
    private void MoveOn__Home()
    {
        ActiveHomeBtn.gameObject.SetActive(true);
        ActiveFeedBtn.gameObject.SetActive(false);
        ActiveCartBtn.gameObject.SetActive(false);
        ActiveProfileBtn.gameObject.SetActive(false);

        SwitchToPanel(homePanel);
    }
    
    private void MoveOn__Feed()
    {
        ActiveHomeBtn.gameObject.SetActive(false);
        ActiveFeedBtn.gameObject.SetActive(true);
        ActiveCartBtn.gameObject.SetActive(false);
        ActiveProfileBtn.gameObject.SetActive(false);
        SwitchToPanel(feedPanel);
    }
    
    private void MoveOn__Cart()
    {
        ActiveHomeBtn.gameObject.SetActive(false);
        ActiveFeedBtn.gameObject.SetActive(false);
        ActiveCartBtn.gameObject.SetActive(true);
        ActiveProfileBtn.gameObject.SetActive(false);
        SwitchToPanel(cartPanel);
    }
    
    private void MoveOn_Profile()
    {
        ActiveHomeBtn.gameObject.SetActive(false);
        ActiveFeedBtn.gameObject.SetActive(false);
        ActiveCartBtn.gameObject.SetActive(false);
        ActiveProfileBtn.gameObject.SetActive(true);
        SwitchToPanel(profilePanel);
    }
    
    private void Switch_Logout()
    {
        HomeBtn.gameObject.SetActive(true);
        ActiveHomeBtn.gameObject.SetActive(true);
        ActiveFeedBtn.gameObject.SetActive(false);
        ActiveCartBtn.gameObject.SetActive(false);
        ActiveProfileBtn.gameObject.SetActive(false);
        SwitchToPanel(signInPanel);
    }

    private void Switch_SignUpPanel()
    {
        SwitchToPanel(signUpPanel);
    }
    private void SwitchToPanel(GameObject panelToActivate)
    {
        // Deactivate all panels
        if (signInPanel != null) signInPanel.SetActive(false);
        if (signUpPanel != null) signUpPanel.SetActive(false);
        if (homePanel != null) homePanel.SetActive(false);
        if (feedPanel != null) feedPanel.SetActive(false);
        if (cartPanel != null) cartPanel.SetActive(false);
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
                              panelToActivate == cartPanel || panelToActivate == profilePanel);
            footerPanel.SetActive(showFooter);
        }
    }
     
}