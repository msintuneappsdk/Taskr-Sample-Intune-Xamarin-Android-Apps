// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.App;
using Android.Util;
using Android.Widget;
using Java.Lang;
using Microsoft.Identity.Client;
using Microsoft.Intune.Mam.Client.App;
using Microsoft.Intune.Mam.Policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskrForms.Droid.Authentication;
using TaskrForms.Views;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(Authenticator))]
namespace TaskrForms.Droid.Authentication
{
    /// <summary>
    /// Manages authentication for the app.
    /// 
    /// Deals with both MSAL and MAM, significantly.
    /// </summary>
    class Authenticator : IAuthenticator
    {
        private const string _placeholderClientID = "<placeholder_aad_client_id>";
        private const string _placeholderRedirectURI = "<placeholder_redirect_uri>";

        /// <summary>
        /// The authority for the ADAL AuthenticationContext. Sign in will use this URL.
        /// </summary>
        private const string _authority = "https://login.microsoftonline.com/common";

        /// <summary>
        /// Identifier of the client requesting the token. 
        /// The client ID must be registered at https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps.
        /// </summary>
        /// <remarks>
        /// This ID is unique to this application and should be replaced wth the ADAL Application ID.
        /// </remarks>
        private const string _clientID = _placeholderClientID; //TODO - Replace with your value.
        
        /// <summary>
        /// Address to return to upon receiving a response from the authority.
        /// </summary>
        /// <remarks>
        /// This URI is configurable while registering this application with ADAL and should be replaced with the ADAL Redirect URI.
        /// </remarks>
        private const string _redirectURI = _placeholderRedirectURI; //TODO - Replace with your value.
        
        /// <summary>
        /// Identifier of the target resource that is the recipient of the requested token.
        /// </summary>
        private string[] _scopes = { "https://graph.microsoft.com/User.Read" };

        private string _cachedResourceID;
        private string _cachedUPN;
        private string _cachedAADID; 

        private const string _logTagADAL = "Taskr ADAL Logs";
        private const string _logTagAuth = "Taskr Auth Logs";

        private static Authenticator instance;
        //private static AuthenticationContext authContext;
        private static IPublicClientApplication pca;

        ///// <summary>
        ///// The current Authentication Context.
        ///// </summary>
        //private static AuthenticationContext AuthContext
        //{
        //    get
        //    {
        //        if (authContext == null)
        //        {
        //            authContext = new AuthenticationContext(_authority);
        //            if (authContext.TokenCache.ReadItems().Count() > 0)
        //            {
        //                authContext = new AuthenticationContext(
        //                    AuthContext.TokenCache.ReadItems().First().Authority);
        //            }
        //        }
        //        return authContext;
        //    }
        //}

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
                        .WithLogging(ADALLog, LogLevel.Info, true)
                        .WithBroker()
                        .WithRedirectUri(_redirectURI)
                        .Build();
                }
                return pca;
            }
        }

        /// <summary>
        /// Callback for ADAL logging.
        /// </summary>
        /// <remarks>
        /// By default this app has verbose logging for ADAL for troubleshooting purposes.
        /// </remarks>
        /// <param name="level">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="containsPii">True if the log contains PII information, false if otherwise.</param>
        private static void ADALLog(LogLevel level, string message, bool containsPii)
        {
            switch (level)
            {
                case LogLevel.Info:
                    Log.Info(_logTagADAL, message);
                    break;
                case LogLevel.Warning:
                    Log.Warn(_logTagADAL, message);
                    break;
                case LogLevel.Error:
                    Log.Error(_logTagADAL, message);
                    break;
                case LogLevel.Verbose:
                    Log.Verbose(_logTagADAL, message);
                    break;
                default:
                    Log.Debug(_logTagADAL, message);
                    break;
            }
        }

        /// <summary>
        /// Gets the active AuthManager if it exists, otherwise creates a new one.
        /// </summary>
        /// <returns>the AuthMananger</returns>
        public static Authenticator GetAuthenticator()
        {
            return instance ?? (instance = new Authenticator());
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
        /// <returns>The authentication result.</returns>
        public async Task<AuthenticationResult> Authenticate()
        {
            // Check initial authentication values.
            if (_clientID.Equals(_placeholderClientID) || _redirectURI.Equals(_placeholderRedirectURI))
            {
                Toast.MakeText(Android.App.Application.Context, "Please update the authentication values for your application.", ToastLength.Long).Show();
                Log.Info(_logTagAuth, "Authentication cancelled. Authentication values need to be updated with user provided values." +
                    " Client ID = " + _clientID + " Redirect URI = " + _redirectURI);
                return null;
            }

            if (!Uri.IsWellFormedUriString(_redirectURI, UriKind.RelativeOrAbsolute))
            {
                Toast.MakeText(Android.App.Application.Context, "Please correct the redirect URI for your application.", ToastLength.Long).Show();
                Log.Info(_logTagAuth, "Authentication cancelled. Redirect URI needs to be corrected with a well-formed value." +
                    " Redirect URI = " + _redirectURI);
                return null;
            }

            AuthenticationResult result;

            // Register the callback to capture ADAL logs.
            //LoggerCallbackHandler.LogCallback = ADALLog;
            //LoggerCallbackHandler.PiiLoggingEnabled = true;

            // Attempt to sign the user in silently.
            result = await SignInSilent(_scopes);

            // If the user cannot be signed in silently, prompt the user to manually sign in.
            if (result == null)
            {
                result = await SignInWithPrompt((Activity)Forms.Context);
            }

            // If auth was successful, cache the values and log the success.
            if (result != null && result.AccessToken != null)
            {
                _cachedUPN = result.Account.Username;
                _cachedAADID = result.Account.HomeAccountId.Identifier;

                Log.Info(_logTagAuth, "Authentication succeeded. UPN = " + _cachedUPN);

                // Register the account for MAM
                // See: https://docs.microsoft.com/en-us/intune/app-sdk-android#account-authentication
                // This app requires ADAL authentication prior to MAM enrollment so we delay the registration
                // until after the sign in flow.
                IMAMEnrollmentManager mgr = MAMComponents.Get<IMAMEnrollmentManager>();
                mgr.RegisterAccountForMAM(_cachedUPN, _cachedAADID, result.TenantId);
            }

            return result;
        }

        /// <summary>
        /// Attempt silent authentication through the broker.
        /// </summary>
        /// <param name="resourceId"> The resource we're authenticating against to obtain a token </param>
        /// <param name="aadId"> The AAD ID for the user, null if not known </param>
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
                else
                {
                    Log.Warn(_logTagAuth, "No AAD ID provided, continuing silent authentication attempt.");
                    //result = await AuthContext.AcquireTokenSilentAsync(resourceId, _clientID);
                    return null;
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
        /// <param name="platformParams"> Additional paramaters for authentication.</param>
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

            return result;
        }

        /// <summary>
        /// Signs the user out of the application and unenrolls from MAM.
        /// </summary>
        public async void SignOut()
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

            Toast.MakeText(Android.App.Application.Context, Resource.String.auth_out_success, ToastLength.Short).Show();
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
                    result = await PCA.AcquireTokenSilent(new string[] { resourceId + "/.default" } , currentAccounts.FirstOrDefault().Username).ExecuteAsync();
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
    }
}