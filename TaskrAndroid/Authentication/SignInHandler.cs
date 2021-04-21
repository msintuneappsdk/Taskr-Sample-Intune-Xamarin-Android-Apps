// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.App;
using Android.OS;
using Microsoft.Identity.Client;

namespace TaskrAndroid.Authentication
{
    /// <summary>
    /// Helper class to handle the sign in process
    /// </summary>
    public class SignInHandler : Handler
    {
        private AuthManager manager;
        private readonly Activity activity;
        private readonly IAuthListener listener;

        public SignInHandler(Looper looper, Activity callingActivity, AuthManager authManager, 
            IAuthListener authListener) : base(looper)
        {
            manager = authManager;
            activity = callingActivity;
            listener = authListener;
        }

        public async override void HandleMessage(Message msg)
        {
            AuthenticationResult result = await manager.Authenticate(activity);

            // If we were able to get an access token, return to the main view
            if (result != null && result.AccessToken != null)
            {
                listener.OnSignedIn(result);
            }
        }
    }
}

