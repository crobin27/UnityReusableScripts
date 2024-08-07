using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;

public interface IAuthService
{
    Task<bool> LoginAsyncToService();
    Task<bool> SignInWithUnityAsync();
    Task<bool> LinkWithPlatformAsync();

    // Task<bool> UnlinkWithPlatformAsync();

    bool SetPlayerToken();

    void Activate();
}
