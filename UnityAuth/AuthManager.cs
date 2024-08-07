using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_IOS
using Apple.GameKit;
#endif
using RobinsonGaming.Debugging;

/*
 PUT DEBUG STATEMENTAS EVERYWHERE
 */

public class AuthManager : MonoBehaviour
{

    public IAuthService _authService;

    public static AuthManager Instance;
    public bool IsGoogleActive { get; private set; }
    public bool IsGoogleAuthenticated { get; private set; }
    public bool IsAppleActive { get; private set; }
    public bool IsAppleAuthenticated { get; private set; }


    public bool Authenticated;
    public bool FirstLogin = false;
#if UNITY_IOS
    public GKLocalPlayer LocalPlayer;
#elif UNITY_ANDROID
    public string Token;
#endif
    public string UnityID;

    private const string _platformConnectedKey = "platformConnected";

    private void Awake()
    {
        if (Instance == null)
        {
#if UNITY_ANDROID
            _authService = new GoogleAuthManager();
#elif UNITY_IOS
            _authService = new AppleAuthManager();
#endif
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // initialize public variables
        IsGoogleAuthenticated = false;
        IsAppleAuthenticated = false;
        IsGoogleActive = false;
        IsAppleActive = false;
        Authenticated = false;

        // activate respective platform service
        _authService.Activate();
    }

    public async Task<bool> SignInAsyncWithUnity()
    {
        // attempt to login to platform (not link with unity)
#if UNITY_ANDROID 
        IsGoogleActive = await _authService.LoginAsyncToService();
#elif UNITY_IOS
        IsAppleActive = await _authService.LoginAsyncToService();
#endif

        Bug.Log("IsGoogleActive = " + IsGoogleActive.ToString());
        Bug.Log("IsAppleActive = " + IsAppleActive.ToString());

        // if player has linked account previously, move forward with that login or if first time logging in then try Apple/Google
        if (PlayerPrefs.HasKey(_platformConnectedKey) || !AuthenticationService.Instance.SessionTokenExists)
        {
            if (IsGoogleActive || IsAppleActive)
            {
                Bug.Log("Trying to authenticate Google Play with Unity on signin");
                bool connectToUnity = await _authService.SignInWithUnityAsync();
                if (connectToUnity)
                {
                    Bug.Log("Succesfully authenticated Third Party Platform with unity.");
                    PlayerPrefs.SetInt(_platformConnectedKey, 1);
#if UNITY_ANDROID
                    IsGoogleAuthenticated = true;
#elif UNITY_IOS
                IsAppleAuthenticated = true;
                FirebaseManager.Instance.LogAuthenticationMethod("Apple Game Center");
#endif
                    return true;
                }
            }
        }
        // default to anonymous login
        Bug.Log("Sign in with platform failed. Signing in anonymously.");
        bool anonSignIn = await SignInAnonymouslyAsync();
        if (anonSignIn)
        {
            // this is done to attempt to link any anon user to a platform account
            // if user already has account, this will set the platformConnectedKey to true for 
            // future logins
            if (IsAppleActive || IsGoogleActive)
            {
                bool tryToLink = await LinkWithPlatformAsync();
                if (tryToLink)
                {
                    Bug.Log("Succesfully linked with platform after an anon signin.");
#if UNITY_ANDROID
                    IsGoogleAuthenticated = true;
                    IsAppleAuthenticated = false;
                    PlayerPrefs.SetString(_platformConnectedKey, "true");
#elif UNITY_IOS
                IsGoogleAuthenticated = false;
                IsAppleAuthenticated = true;
                FirebaseManager.Instance.LogAuthenticationMethod("Apple Game Center");
#endif
                    return true;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Bug.Log("Player signed in anonymously.");
            Bug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            return true;
        }
        catch (Exception e)
        {
            Bug.LogError($"Failed to sign in anonymously: {e.Message}");
            //SceneManager.LoadScene("Error");
            return false;
        }
    }

    public async Task<bool> LinkWithPlatformAsync()
    {
        try
        {
            bool linkSuccessful = await _authService.LinkWithPlatformAsync();
            if (linkSuccessful == true)
            {
                Bug.Log("Succesfully linked with platform.");
                // set the PPrefs key to true
                PlayerPrefs.SetInt(_platformConnectedKey, 1);
#if UNITY_IOS
                IsAppleAuthenticated = true;
#elif UNITY_ANDROID
                IsGoogleAuthenticated = true;
#endif
                return true;
            }
            else
            {
                Bug.Log("Failed to link with platform.");
                return false;
            }
        }
        catch (Exception e)
        {
            Bug.Log("Failed to Link with platform: " + e.Message);
            return false;
        }
    }

    // subscribe to events 
    public void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () => {
            // Shows how to get a playerID
            Bug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Bug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

        };

        AuthenticationService.Instance.SignInFailed += (err) => {
            Bug.LogError(err.Message);
        };

        AuthenticationService.Instance.SignedOut += () => {
            Bug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () => {
            Bug.Log("Player session could not be refreshed and expired.");
        };
    }

    // unsubscribe to events
    void CloseEvents()
    {
        AuthenticationService.Instance.SignedIn -= () => {
            // Shows how to get a playerID
            Bug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Bug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

        };

        AuthenticationService.Instance.SignInFailed -= (err) => {
            Bug.LogError(err.Message);
        };

        AuthenticationService.Instance.SignedOut -= () => {
            Bug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired -= () => {
            Bug.Log("Player session could not be refreshed and expired.");
        };

    }

    public string GetUnityID()
    {
        if (Authenticated)
        {
            UnityID = AuthenticationService.Instance.PlayerId;
            return UnityID;
        }
        else
        {
            return "";
        }
    }


    private void OnDisable()
    {
        CloseEvents();
    }

}