// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace TaskrForms.Views
{
    /// <summary>
    /// Interface to manage authentication for the app.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Authenticate by signing in the user and enrolling the user's account with MAM.
        /// </summary>
        /// <returns></returns>
        Task<AuthenticationResult> Authenticate();

        /// <summary>
        /// Sign the user out and unenroll the user's account from MAM.
        /// </summary>
        void SignOut();
    }
}