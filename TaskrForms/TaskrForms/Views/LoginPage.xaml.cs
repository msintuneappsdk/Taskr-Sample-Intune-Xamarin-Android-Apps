// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using System;
using TaskrForms.Resx;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TaskrForms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage ()
        {
            InitializeComponent();

            welcomeViewMessage.Text = AppResources.AppWelcomeMessage;
            welcomeViewButton.Text = AppResources.AppWelcomeButton;
        }

        async void OnButtonClicked(object sender, EventArgs args)
        {
            if (App.AuthenticationResult == null || App.AuthenticationResult.AccessToken == null)
            {
                App.AuthenticationResult = await DependencyService.Get<IAuthenticator>().Authenticate();
            }

            // If the authentication was successful then enter the application.
            if (App.AuthenticationResult != null && App.AuthenticationResult.AccessToken != null)
            {
                Application.Current.MainPage = new MainPage();
            }
        }
    }
}