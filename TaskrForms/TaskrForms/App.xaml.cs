// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using TaskrForms.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TaskrForms
{
    public partial class App : Application
    {
        public static AuthenticationResult AuthenticationResult = null;

        public App()
        {
            InitializeComponent();

            MainPage = new LoginPage();

            StartLogin();
        }

        /// <summary>
        /// Begin the login process on app initialization.
        /// </summary>
        async private void StartLogin()
        {
            if (AuthenticationResult == null || AuthenticationResult.AccessToken == null)
            {
                AuthenticationResult = await DependencyService.Get<IAuthenticator>().Authenticate();
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
