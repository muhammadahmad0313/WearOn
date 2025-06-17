using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using UnityEngine.EventSystems;

public class Auth : MonoBehaviour
{
    // State tracking
    private bool signIn;
    private bool signUp;
    private bool forgotPassword;
    private bool otpBool;

    // Canvas references
    [Header("Canvas References")]
    [SerializeField] private Canvas signInCanvas;
    [SerializeField] private Canvas signUpCanvas;
    [SerializeField] private Canvas forgotPassCanvas;
    [SerializeField] private Canvas otpCanvas;
    
    // Text references
    [Header("Text References")]
    [SerializeField] private TMP_Text signUpText;
    [SerializeField] private TMP_Text forgotPass;
    [SerializeField] private TMP_Text backToSignInFromForgot;
    [SerializeField] private TMP_Text backToSignInFromSignUp;
    [SerializeField] private TMP_Text OtpToSignIn;

    // Button references
    [Header("Button References")]
    [SerializeField] private Button SignUpBtn;
    [SerializeField] private Button SendOtp;
    [SerializeField] private Button ConfirmOtp;

    void Start()
    {
        // Initialize state
        signIn = true;
        signUp = false;
        forgotPassword = false;
        otpBool = false;

        // Setup button listeners
        SetupButtonListeners();
        
        // Setup text element triggers
        SetupTextTriggers();
    }
    
    private void SetupButtonListeners()
    {
        // Add click events to all buttons
        if (SignUpBtn != null)
        {
            SignUpBtn.onClick.AddListener(CompleteSignUp);
        }

        if (SendOtp != null)
        {
            SendOtp.onClick.AddListener(SendOtpProcess);
        }

        if (ConfirmOtp != null)
        {
            ConfirmOtp.onClick.AddListener(ConfirmOtpProcess);
        }
    }
    
    private void SetupTextTriggers()
    {
        // Get or create EventTriggers for all clickable text elements
        EventTrigger SignIN_SignUp_Trigger = signUpText?.gameObject.GetComponent<EventTrigger>();
        EventTrigger SignIN_ForgotPass_Trigger = forgotPass?.gameObject.GetComponent<EventTrigger>();
        EventTrigger ForgotPass_SignIn_Trigger = backToSignInFromForgot?.gameObject.GetComponent<EventTrigger>();
        EventTrigger SignUp_SignIn_Trigger = backToSignInFromSignUp?.gameObject.GetComponent<EventTrigger>();
        EventTrigger SendOtp_SignIn_Trigger = OtpToSignIn?.gameObject.GetComponent<EventTrigger>();


        // OTP TO SIGN IN
        AddClickTrigger(OtpToSignIn, SendOtp_SignIn_Trigger, Switch_SendOtp_To_SignIn);

        // SIGN IN TO SIGN UP
        AddClickTrigger(signUpText, SignIN_SignUp_Trigger, Switch_SignIn_To_SignUp);

        // SIGN IN TO FORGOT PASSWORD
        AddClickTrigger(forgotPass, SignIN_ForgotPass_Trigger, Switch_SignIn_To_ForgotPass);

        // FORGOT PASSWORD TO SIGN IN
        AddClickTrigger(backToSignInFromForgot, ForgotPass_SignIn_Trigger, Switch_ForgotPass_To_SignIn);

        // SIGN UP TO SIGN IN
        AddClickTrigger(backToSignInFromSignUp, SignUp_SignIn_Trigger, Switch_SignUp_To_SignIn);
    }
    
    // Helper method to add click triggers to TMP_Text elements
    private void AddClickTrigger(TMP_Text textElement, EventTrigger trigger, UnityEngine.Events.UnityAction action)
    {
        if (textElement == null) return;
        
        // Get or add EventTrigger component
        if (trigger == null)
        {
            trigger = textElement.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = textElement.gameObject.AddComponent<EventTrigger>();
            }
        }
        
        // Create a pointer click entry
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) => { action(); });
        
        // Add the entry to the trigger
        trigger.triggers.Add(entry);
    }

    // Navigation methods
    private void Switch_SignIn_To_SignUp()
    {
        if (signInCanvas == null || signUpCanvas == null) {
            Debug.LogError("Canvas references missing in Auth component");
            return;
        }
        
        signIn = false;
        signUp = true;
        signInCanvas.gameObject.SetActive(false);
        signUpCanvas.gameObject.SetActive(true);
        Debug.Log("Navigated to Sign Up screen");
    }

    private void Switch_SignIn_To_ForgotPass()
    {
        if (signInCanvas == null || forgotPassCanvas == null) {
            Debug.LogError("Canvas references missing in Auth component");
            return;
        }
        
        signIn = false;
        forgotPassword = true;
        signInCanvas.gameObject.SetActive(false);
        forgotPassCanvas.gameObject.SetActive(true);
        Debug.Log("Navigated to Forgot Password screen");
    }

    private void Switch_ForgotPass_To_SignIn()
    {
        if (forgotPassCanvas == null || signInCanvas == null) {
            Debug.LogError("Canvas references missing in Auth component");
            return;
        }
        
        forgotPassword = false;
        signIn = true;
        forgotPassCanvas.gameObject.SetActive(false);
        signInCanvas.gameObject.SetActive(true);
        Debug.Log("Navigated back to Sign In screen from Forgot Password");
    }

    private void Switch_SignUp_To_SignIn()
    {
        if (signUpCanvas == null || signInCanvas == null) {
            Debug.LogError("Canvas references missing in Auth component");
            return;
        }
        
        signUp = false;
        signIn = true;
        signUpCanvas.gameObject.SetActive(false);
        signInCanvas.gameObject.SetActive(true);
        Debug.Log("Navigated back to Sign In screen from Sign Up");
    }

    // Button action methods
    private void CompleteSignUp()
    {
        // TODO: Implement actual sign up logic
        Debug.Log("Sign up successful!");

        // Navigate back to sign in
        Switch_SignUp_To_SignIn();
    }

    private void SendOtpProcess()
    {
        if (forgotPassCanvas == null || otpCanvas == null) {
            Debug.LogError("Canvas references missing in Auth component");
            return;
        }
        
        forgotPassword = false;
        otpBool = true;
        forgotPassCanvas.gameObject.SetActive(false);
        otpCanvas.gameObject.SetActive(true);
        Debug.Log("OTP sent and navigated to OTP screen");
    }

    private void ConfirmOtpProcess()
    {
        if (otpCanvas == null || signInCanvas == null) {
            Debug.LogError("Canvas references missing in Auth component");
            return;
        }
        
        otpBool = false;
        signIn = true;
        otpCanvas.gameObject.SetActive(false);
        signInCanvas.gameObject.SetActive(true);
        Debug.Log("OTP confirmed and navigated to Sign In screen");
    }

    private void Switch_SendOtp_To_SignIn()
    {
        ConfirmOtpProcess();
    }
}