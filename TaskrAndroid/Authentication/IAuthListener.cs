// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TaskrAndroid.Authentication
{
    /// <summary>
    /// Listener that would be implemented by a UI class and called by an authentication class so
    /// they can interact.
    /// </summary>
    public interface IAuthListener
    {
        /// <summary>
        /// Called when the authenticator successfully signs in.
        /// </summary>
        void OnSignedIn(Microsoft.Identity.Client.AuthenticationResult result);

        /// <summary>
        /// Called when the authenticator successfully signs out.
        /// </summary>
        void OnSignedOut();
    }
}