// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Widget;
using Java.Lang;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Intune.Mam.Client.App;
using Microsoft.Intune.Mam.Policy;

namespace TaskrAndroid.Authentication
{
    /// <summary>
    /// Manages authentication for the app.
    /// 
    /// Deals with both ADAL and MAM, significantly.
    /// </summary>
    public class AuthManager
    {
        private static AuthenticationContext authContext;
        private static AuthManager instance;

        /// <summary>
        /// The authority for the ADAL AuthenticationContext. Sign in will use this URL.
        /// </summary>
        public const string _authority = "https://login.microsoftonline.com/common";

        /// <summary>
        /// Identifier of the client requesting the token. 
        /// The client ID must be registered at https://apps.dev.microsoft.com.
        /// </summary>
        /// <remarks>
        /// This ID is unique to this application and should be replaced for yours.
        /// </remarks>
        public const string _clientID = "<your_aad_client_id>";

        /// <summary>
        /// Address to return to upon receiving a response from the authority.
        /// </summary>
        public const string _redirectURI = "<your_redirect_uri>";

        /// <summary>
        /// Identifier of the target resource that is the recipient of the requested token.
        /// </summary>
        public const string _resourceID = "https://graph.microsoft.com/";

        private string _cachedResourceID;
        private string _cachedUPN;
        private string _cachedAADID;

        private const string _logTagADAL = "Taskr ADAL Logs";
        private const string _logTagAuth = "Taskr Auth Logs";

        private const string SAVE_IS_AUTHED = "isAuthenticated";
        private static bool isAuthenticated;

        /// <summary>
        /// Required private, empty constructor
        /// </summary>
        private AuthManager() { }

        /// <summary>
        /// The current Authentication Context.
        /// </summary>
        private static AuthenticationContext AuthContext
        {
            get
            {
                if (authContext == null)
                {
                    authContext = new AuthenticationContext(_authority);
                    if (authContext.TokenCache.ReadItems().Count() > 0)
                    {
                        authContext = new AuthenticationContext(
                            AuthContext.TokenCache.ReadItems().First().Authority);
                    }
                }
                return authContext;
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
            switch(level)
            {
               case LogLevel.Information:
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
        /// <param name="behavior">The ADAL prompt behavior.</param>
        /// <returns>The authentication result.</returns>
        public async Task<AuthenticationResult> Authenticate(Activity activity, PromptBehavior behavior)
        {
            AuthenticationResult result = null;

            // Register the callback to capture ADAL logs.
            LoggerCallbackHandler.LogCallback = ADALLog;
            LoggerCallbackHandler.PiiLoggingEnabled = true;

            // Attempt to sign the user in silently.
            result = await SignInSilent(_resourceID, null);

            // If the user cannot be signed in silently, prompt the user to manually sign in.
            if (result == null)
            {
                result = await SignInWithPrompt(new PlatformParameters(activity, false, behavior));
            }

            // If auth was successful, cache the values and log the success.
            if (result != null && result.AccessToken != null)
            {
                _cachedUPN = result.UserInfo.DisplayableId;
                _cachedAADID = result.UserInfo.UniqueId;

                Log.Info(_logTagAuth, "Authentication succeeded. UPN = " + _cachedUPN);
            }

            return result;
        }

        /// <summary>
        /// Attempt silent authentication through the broker.
        /// </summary>
        /// <param name="resourceId"> The resource we're authenticating against to obtain a token </param>
        /// <param name="aadId"> The AAD ID for the user, null if not known </param>
        /// <returns> The AuthenticationResult on succes, null otherwise</returns>
        public async Task<AuthenticationResult> SignInSilent(string resourceId, string aadId)
        {
            AuthenticationResult result = null;
            try
            {
                Log.Info(_logTagAuth, "Attempting silent authentication.");
                if (aadId != null)
                {
                    result = await AuthContext.AcquireTokenSilentAsync(
                        resourceId,
                        _clientID,
                        new UserIdentifier(aadId, UserIdentifierType.RequiredDisplayableId));
                }
                else
                {
                    Log.Warn(_logTagAuth, "No AAD ID provided, continuing silent authentication attempt.");
                    result = await AuthContext.AcquireTokenSilentAsync(resourceId, _clientID);
                }
            }
            catch (AdalSilentTokenAcquisitionException e)
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
        public async Task<AuthenticationResult> SignInWithPrompt(PlatformParameters platformParams)
        {
            AuthenticationResult result = null;
            try
            {
                Log.Info(_logTagAuth, "Attempting interactive authentication");
                result = await AuthContext.AcquireTokenAsync(
                    _resourceID,
                    _clientID,
                    new Uri(_redirectURI),
                    platformParams);
            }
            catch (AdalException e)
            {
                string msg = Resource.String.err_auth + e.Message;
                Log.Error(_logTagAuth, Throwable.FromException(e), msg);
                Toast.MakeText(platformParams.CallerActivity, msg, ToastLength.Long).Show();
                return null;
            }

            isAuthenticated = true;

            return result;
        }

        /// <summary>
        /// Signs the user out of the application and unenrolls from MAM.
        /// </summary>
        /// <param name="listener"></param>
        public void SignOut(IAuthListener listener)
        {
            // Clear the app's token cache so the user will be prompted to sign in again.
            authContext.TokenCache.Clear();

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
                result = await AuthContext.AcquireTokenSilentAsync(
                    resourceId,
                    _clientID,
                    new UserIdentifier(aadId, UserIdentifierType.UniqueId));
            }
            catch (AdalSilentTokenAcquisitionException e)
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
