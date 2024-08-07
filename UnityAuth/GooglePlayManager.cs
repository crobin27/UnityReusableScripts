#if UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using RobinsonGaming.Debugging;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Net;
using Unity.VisualScripting.Antlr3.Runtime;

public class GoogleAuthManager : IAuthService
{

    private string _token, _error;

    public void Activate()
    {
        _token = null;
        PlayGamesPlatform.Activate();
    }

    public bool SetPlayerToken()
    {
        if (_token == null)
        {
            Bug.Log("Token is null");
            return false;
        }
        AuthManager.Instance.Token = _token;
        return true;
    }

    public Task<bool> LoginAsyncToService()
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        try
        {
            PlayGamesPlatform.Instance.Authenticate(async (success) => {
                if (success == SignInStatus.Success)
                {
                    Bug.Log("Login with Google Play games successful.");

                    // Note: Assuming RequestServerSideAccess is synchronous.
                    // If it's async, you'll need to wait for its completion as well.
                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => {
                        Bug.Log("Authorization code: " + code);
                        _token = code;
                        SetPlayerToken();
                    });

                    tcs.SetResult(true);
                }
                else
                {
                    _error = "Failed to retrieve Google play games authorization code";
                    Bug.Log("Login Unsuccessful with Google Play");
                    tcs.SetResult(false);
                }
            });
        }
        catch (Exception e)
        {
            Bug.Log("Exception Caught, Couldn't log in to Google Play");
            Bug.LogException(e);
            tcs.SetResult(false);
        }

        return tcs.Task;
    }


    public async Task<bool> SignInWithUnityAsync()
    {

        try
        {
            // wait 1 second then try again for few seconds, then quit if still null
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(1000);

                if (_token != null)
                {
                    Bug.Log("Token awaited and received: " + _token);
                    await Task.Delay(100);
                    break;
                }
                if (i == 4)
                {
                    Bug.Log("Failed to get token from Google Play Games");
                    return false;
                }
            }
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(_token);
            Bug.Log("SignIn is successful.");
            return true;
        }
        catch (AuthenticationException ex)
        {
            Bug.Log("Failed to auth with Gplay: " + ex.Message);
            return false;
        }
        catch (RequestFailedException ex)
        {
            Bug.Log("Failed to auth with Gplay: " + ex.Message);
            return false;
        }
        catch (InvalidOperationException e)
        {
            Bug.Log("Failed to auth with Gplay: " + e.Message);
            return false;
        }
        catch (Exception e)
        {
            Bug.Log("Failed to auth with Gplay: " + e.Message);
            return false;
        }
    }

    public async Task<bool> LinkWithPlatformAsync()
    {
        // wait 1 second then try again for few seconds, then quit if still null
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(1000);

            if (_token != null)
            {
                Bug.Log("Token awaited and received: " + _token);
                await Task.Delay(100);
                break;
            }
            if (i == 4)
            {
                Bug.Log("Failed to get token from Google Play Games");
                return false;
            }
        }

        try
        {
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(_token);
            Bug.Log("Link is successful.");
            PlayerPrefs.SetInt("platformConnected", 1); // set this to sign in with google play next time
            return true;
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Bug.Log("This user is already linked with another account. Log in instead.");
            PlayerPrefs.SetInt("platformConnected", 1); // set this to sign in with google play next time
            /*
             * This section is for when the user is already linked with another account
             * and attempts to sign in to that instead by clearing the session token (sign out anon)
             * and then resigning into the platform account, if fails, signs in anon again
             */
            /*AuthenticationService.Instance.ClearSessionToken();*/
            //bool serviceSignInAttempt = await LoginAsyncToService();
            bool newSignIn = await SignInWithUnityAsync();
            if (newSignIn == true)
            {
                return true;
            }
            else
            {
                await AuthManager.Instance.SignInAnonymouslyAsync();
                return false;
            }

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message

            Bug.LogException(ex);
            return false;
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message

            Bug.LogException(ex);
            return false;
        }
        catch (Exception ex)
        {
            // Notify the player with the proper error message
            Bug.Log("Unknown Exception on Link");
            Bug.LogException(ex);
            return false;
        }

    }
}
#endif
