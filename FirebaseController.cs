using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using Firebase.Extensions;

public class FirebaseController : MonoBehaviour
{
    public GameObject loginPanel, signupPanel, profilePanel, forgetPasswordPanel, notificationPanel;

    public InputField loginEmail, loginPassword, signupEmail, signupPassword, signupCpassword, signupUserName, forgetPassEmail;

    public Text notif_Title_Text, notif_Message_Text, profileUserName_Text, profileUserEmail_Text;

    public Toggle rememberMe;

    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;

    bool isSignIn = false;

    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                InitializeFirebase();

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }

    public void OpenSignUpPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }

    public void OpenProfilePanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(true);
        forgetPasswordPanel.SetActive(false);
    }

    public void OpenForgetPassPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(true);
    }

    public void LoginUser()
    {
        if (string.IsNullOrEmpty(loginEmail.text) && string.IsNullOrEmpty(loginPassword.text))
        {
            showNotificationMessage("Error", "Feilds Empty! Please Input Details In All Feilds");
            return;
        }


        //Do Login
        SignInUser(loginEmail.text, loginPassword.text);

    }

    public void SignUpUser()
    {
        if (string.IsNullOrEmpty(signupEmail.text) && string.IsNullOrEmpty(signupPassword.text) && string.IsNullOrEmpty(signupPassword.text) && string.IsNullOrEmpty(signupCpassword.text) && string.IsNullOrEmpty(signupUserName.text))
        {
            showNotificationMessage("Error", "Feilds Empty! Please Input Details In All Feilds");
            return;
        }

        //Do SignUp
        CreateUser(signupEmail.text, signupPassword.text, signupUserName.text);
    }

    public void forgetPass()
    {
        if (string.IsNullOrEmpty(forgetPassEmail.text))
        {
            showNotificationMessage("Error", "Feilds Empty! Please Input Details In All Feilds");
            return;
        }

        forgetPasswordSubmit(forgetPassEmail.text);
    }

    private void showNotificationMessage(string title, string message)
    {
        notif_Title_Text.text = "" + title;
        notif_Message_Text.text = "" + message;

        notificationPanel.SetActive(true);
    }

    public void CloseNoif_Panel()
    {
        notif_Title_Text.text = "";
        notif_Message_Text.text = "";

        notificationPanel.SetActive(false);
    }

    public void LogOut()
    {
        auth.SignOut();
        profileUserEmail_Text.text = "";
        profileUserName_Text.text = "";
        OpenLoginPanel();
    }

    void CreateUser(string email, string password, string Username)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);


                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        showNotificationMessage("Error", GetErrorMessage(errorCode));
                    }
                }
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            UpdateUserProfile(Username);
        });
    }

    public void SignInUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                //Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        showNotificationMessage("Error", GetErrorMessage(errorCode));
                    }
                }

                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            profileUserName_Text.text = "" + newUser.DisplayName;
            profileUserEmail_Text.text = "" + newUser.Email;
            OpenProfilePanel();
        });
    }

    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                isSignIn = true;
            }
        }
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    void UpdateUserProfile(string Username)
    {
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            {
                DisplayName = Username,
                PhotoUrl = new System.Uri("https://via.placeholder.com/150%20C/O%20https://placeholder.com/"),
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");

                showNotificationMessage("Alert", "Account Successfully Created");
            });
        }

    }


    bool isSigned = false;
    void Update()
    {
        if (isSignIn)
        {
            if (isSigned)
            {
                isSigned = true;
                profileUserName_Text.text = "" + user.DisplayName;
                profileUserEmail_Text.text = "" + user.Email;
                OpenProfilePanel();
            }
        }
    }

    private static string GetErrorMessage(AuthError errorCode)
    {
        var message = "";
        switch (errorCode)
        {
            case AuthError.AccountExistsWithDifferentCredentials:
                message = "Account Not Exist";
                break;
            case AuthError.MissingPassword:
                message = "Missing Password";
                break;
            case AuthError.WeakPassword:
                message = "Password So Weak";
                break;
            case AuthError.WrongPassword:
                message = "Wrong Password";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "Your Email Already In Use";
                break;
            case AuthError.InvalidEmail:
                message = "Your Email is Invalid";
                break;
            case AuthError.MissingEmail:
                message = "Your Email is Missing";
                break;
            default:
                message = "Invalid Error";
                break;
        }
        return message;
    }

    void forgetPasswordSubmit(string forgetPasswordEmail)
    {
        auth.SendPasswordResetEmailAsync(forgetPasswordEmail).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SendPasswordResetEmailAsync was canceled");
            }

            if (task.IsFaulted)
            {
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        showNotificationMessage("Error", GetErrorMessage(errorCode));
                    }
                }
            }

            showNotificationMessage("Alert", "Successfully Send Email For Reset Password");
        }
        );
    }

}