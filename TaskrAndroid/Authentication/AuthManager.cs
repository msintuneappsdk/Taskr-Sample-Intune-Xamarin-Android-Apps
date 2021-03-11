// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Widget;
using Java.Lang;
using Microsoft.Identity.Client;
using Microsoft.Intune.Mam.Client.App;
using Microsoft.Intune.Mam.Policy;

namespace TaskrAndroid.Authentication
{
    /// <summary>
    /// Manages authentication for the app.
    /// 
    /// Deals with both MSAL and MAM, significantly.
    /// </summary>
    public class AuthManager
    {
        private const string _placeholderClientID = "<placeholder_aad_client_id>";
        private const string _placeholderRedirectURI = "<placeholder_redirect_uri>";

        /// <summary>
        /// The authority for the MSAL PublicClientApplication. Sign in will use this URL.
        /// </summary>
        private const string _authority = "https://login.microsoftonline.com/common";

        /// <summary>
        /// Identifier of the client requesting the token. 
        /// The client ID must be registered at https://apps.dev.microsoft.com.
        /// </summary>
        /// <remarks>
        /// This ID is unique to this application and should be replaced wth the MSAL Application ID.
        /// </remarks>
        private const string _clientID = _placeholderClientID; //TODO - Replace with your value.
        
        /// <summary>
        /// Address to return to upon receiving a response from the authority.
        /// </summary>
        /// <remarks>
        /// This URI is configurable while registering this application with MSAL and should be replaced with the MSAL Redirect URI.
        /// </remarks>
        private const string _redirectURI = _placeholderRedirectURI; //TODO - Replace with your value.
        
        /// <summary>
        /// Identifier of the target resource that is the recipient of the requested token.
        /// </summary>
        private string[] _scopes = { "https://graph.microsoft.com/User.Read" };

        private string _cachedResourceID;
        private string _cachedUPN;
        private string _cachedAADID;

        private const string _logTagMSAL = "Taskr MSAL Logs";
        private const string _logTagAuth = "Taskr Auth Logs";

        private static AuthManager instance;
        private static IPublicClientApplication pca;

        private const string SAVE_IS_AUTHED = "isAuthenticated";
        private static bool isAuthenticated;

        /// <summary>
        /// Required private, empty constructor
        /// </summary>
        private AuthManager() { }

        /// <summary>
        /// The current Authentication Context.
        /// </summary>
        private static IPublicClientApplication PCA
        {
            get
            {
                if (pca == null)
                {
                    pca = PublicClientApplicationBuilder
                        .Create(_clientID)
                        .WithAuthority(_authority)
                        .WithLogging(MSALLog, LogLevel.Info, true)
                        .WithBroker()
                        .WithRedirectUri(_redirectURI)
                        .Build();
                }
                return pca;
            }
        }

        /// <summary>
        /// Callback for MSAL logging.
        /// </summary>
        /// <remarks>
        /// By default this app has verbose logging for MSAL for troubleshooting purposes.
        /// </remarks>
        /// <param name="level">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="containsPii">True if the log contains PII information, false if otherwise.</param>
        private static void MSALLog(LogLevel level, string message, bool containsPii)
        {
            switch(level)
            {
               case LogLevel.Info:
                    Log.Info(_logTagMSAL, message);
                    break;
                case LogLevel.Warning:
                    Log.Warn(_logTagMSAL, message);
                    break;
                case LogLevel.Error:
                    Log.Error(_logTagMSAL, message);
                    break;
                case LogLevel.Verbose:
                    Log.Verbose(_logTagMSAL, message);
                    break;
                default:
                    Log.Debug(_logTagMSAL, message);
                    break;
            }
        }

        /// <summary>
        /// Gets the active AuthManager if it exists, otherwise creates a new one
        /// </summary>
        /// <returns>the AuthMananger</returns>
        public static AuthManager GetManager()
        {
            return instance ?? (instance = new AuthManager());
        }

        /// <summary>
        /// The current MAM user.
        /// </summary>
        /// <returns>The current user's username, null if it hasn't been found yet.</returns>
        public static string User
        {
            get
            {
                IMAMUserInfo info = MAMComponents.Get<IMAMUserInfo>();
                return info?.PrimaryUser;
            }
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="activity">The calling activity.</param>
        /// <returns>The authentication result.</returns>
        public async Task<AuthenticationResult> Authenticate(Activity activity)
        {
            // Check initial authentication values.
            if (_clientID.Equals(_placeholderClientID) || _redirectURI.Equals(_placeholderRedirectURI))
            {
                Toast.MakeText(Application.Context, "Please update the authentication values for your application.", ToastLength.Long).Show();
                Log.Info(_logTagAuth, "Authentication cancelled. Authentication values need to be updated with user provided values." +
                    " Client ID = " + _clientID + " Redirect URI = " + _redirectURI);
                return null;
            }

            if (!Uri.IsWellFormedUriString(_redirectURI, UriKind.RelativeOrAbsolute))
            {
                Toast.MakeText(Application.Context, "Please correct the redirect URI for your application.", ToastLength.Long).Show();
                Log.Info(_logTagAuth, "Authentication cancelled. Redirect URI needs to be corrected with a well-formed value." +
                    " Redirect URI = " + _redirectURI);
                return null;
            }

            AuthenticationResult result;

            // Attempt to sign the user in silently.
            result = await SignInSilent(_scopes);

            // If the user cannot be signed in silently, prompt the user to manually sign in.
            if (result == null)
            {
                result = await SignInWithPrompt(activity);
            }

            // If auth was successful, cache the values and log the success.
            if (result != null && result.AccessToken != null)
            {
                _cachedUPN = result.Account.Username;
                _cachedAADID = result.Account.HomeAccountId.ObjectId;

                Log.Info(_logTagAuth, "Authentication succeeded. UPN = " + _cachedUPN);
            }

            return result;
        }

        /// <summary>
        /// Attempt silent authentication through the broker.
        /// </summary>
        /// <param name="scopes"> The scopes we're authenticating against to obtain a token </param>
        /// <returns> The AuthenticationResult on succes, null otherwise</returns>
        public async Task<AuthenticationResult> SignInSilent(IEnumerable<string> scopes)
        {
            AuthenticationResult result = null;
            try
            {
                Log.Info(_logTagAuth, "Attempting silent authentication.");
                var currentAccounts = await PCA.GetAccountsAsync();
                if (currentAccounts.Count() > 0)
                {
                    result = await PCA.AcquireTokenSilent(scopes, currentAccounts.FirstOrDefault()).ExecuteAsync();
                }
            }
            catch (MsalUiRequiredException e)
            {
                // Expected if there is not token in the cache.
                Log.Warn(_logTagAuth, "Encountered error when attempting to silently authenticate. " +
                    "Error code = " + e.ErrorCode + ". Message = " + e.Message, e);

                return null;
            }

            return result;
        }

        /// <summary>
        /// Attempt interactive authentication through the broker.
        /// </summary>
        /// <returns>The AuthenticationResult on succes, null otherwise.</returns>
        public async Task<AuthenticationResult> SignInWithPrompt(Activity activity)
        {
            AuthenticationResult result = null;
            try
            {
                Log.Info(_logTagAuth, "Attempting interactive authentication");
                result = await PCA.AcquireTokenInteractive(_scopes)
                    .WithParentActivityOrWindow(activity)
                    .WithUseEmbeddedWebView(true)
                    .ExecuteAsync();
            }
            catch (MsalException e)
            {
                string msg = Resource.String.err_auth + e.Message;
                Log.Error(_logTagAuth, Throwable.FromException(e), msg);
                Toast.MakeText(activity, msg, ToastLength.Long).Show();
                return null;
            }

            isAuthenticated = true;

            return result;
        }

        /// <summary>
        /// Signs the user out of the application and unenrolls from MAM.
        /// </summary>
        /// <param name="listener"></param>
        public async void SignOut(IAuthListener listener)
        {
            // Clear the app's token cache so the user will be prompted to sign in again.
            var currentAccounts = await PCA.GetAccountsAsync();
            if (currentAccounts.Count() > 0)
            {
                await PCA.RemoveAsync(currentAccounts.FirstOrDefault());
            }

            string user = User;
            if (user != null)
            {
                // Remove the user's MAM policy from the app
                IMAMEnrollmentManager mgr = MAMComponents.Get<IMAMEnrollmentManager>();
                mgr.UnregisterAccountForMAM(user);
            }

            isAuthenticated = false;

            listener.OnSignedOut();
        }

        /// <summary>
        /// Attempt to get a token from the cache without prompting the user for authentication.
        /// </summary>
        /// <returns> A token on success, null otherwise </returns>
        public async void UpdateAccessTokenForMAM()
        {
            if (string.IsNullOrWhiteSpace(_cachedResourceID))
            {
                Log.Warn(_logTagAuth, "Resource ID is not set, cannot update access token for MAM.");
                return;
            }

            string token = await GetAccessTokenForMAM(_cachedAADID, _cachedResourceID);

            if (!string.IsNullOrWhiteSpace(token))
            {
                IMAMEnrollmentManager mgr = MAMComponents.Get<IMAMEnrollmentManager>();
                mgr.UpdateToken(_cachedUPN, _cachedAADID, _cachedResourceID, token);
            }
        }

        /// <summary>
        /// Attempt to get a token from the cache without prompting the user for authentication.
        /// </summary>
        /// <param name="aadId"> The AAD ID for the user </param>
        /// <param name="resourceId"> The resource we're authenticating against to obtain a token </param>
        /// <returns> A token on success, null otherwise </returns>
        public async Task<string> GetAccessTokenForMAM(string aadId, string resourceId)
        {
            _cachedResourceID = resourceId;

            Log.Info(_logTagAuth, "Attempting to get access token for MAM with resource " + resourceId);
            AuthenticationResult result = null;

            try
            {
                var currentAccounts = await PCA.GetAccountsAsync();
                if (currentAccounts.Count() > 0)
                {
                    result = await PCA.AcquireTokenSilent(new string[] { resourceId + "/.default" }, currentAccounts.FirstOrDefault().Username).ExecuteAsync();
                }
            }
            catch (MsalServiceException e)
            {
                // Expected if there is not token in the cache.
                Log.Warn(_logTagAuth, "Encountered error when attempting to silently authenticate. " +
                    "Error code = " + e.ErrorCode + ". Message = " + e.Message, e);
            }

            return result?.AccessToken;
        }

        /// <summary>
        /// Saves the current state of authentication to outState. Used by MainActivity to ensure
        /// the user stays signed in when MainActivity is reinitialized.
        /// </summary>
        /// <param name="outState"> the Bundle to save state to </param>
        public static void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean(SAVE_IS_AUTHED, isAuthenticated);
        }

        /// <summary>
        /// Reads inState and returns true if the user was signed in when inState was written to.
        /// </summary>
        /// <param name="inState"> inState the bundle to read state from </param>
        /// <returns>
        /// true if the user was signed in when inState was written to, false otherwise
        /// </returns>
        public static bool ShouldRestoreSignIn(Bundle inState)
        {
            return inState.GetBoolean(SAVE_IS_AUTHED);
        }
    }
}
