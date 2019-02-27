// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.Util;
using Microsoft.Intune.Mam.Policy;
using System.Threading.Tasks;

namespace TaskrForms.Droid.Authentication
{
    /// <summary>
    /// Required by the MAM SDK. A token may be needed very early in the app lifecycle so the ideal
    /// place to register the callback is in the OnMAMCreate() method of the app's implementation
    /// of IMAMApplication.
    /// See https://docs.microsoft.com/en-us/intune/app-sdk-android#account-authentication
    /// </summary>
    class MAMWEAuthCallback : Java.Lang.Object, IMAMServiceAuthenticationCallback
    {
        public string AcquireToken(string upn, string aadId, string resourceId)
        {
            Log.Info(GetType().Name, string.Format("Providing token via the callback for aadID: {0} and resource ID: {1}", aadId, resourceId));
            Task<string> token = Authenticator.GetAuthenticator().GetAccessTokenForMAM(aadId, resourceId);

            return token?.Result;
        }
    }
}