// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using TaskrForms.Resx;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TaskrForms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            IntuneLink.Text = AppResources.TaskrAboutLinkIntune;
            AppSDKLink.Text = AppResources.TaskrAboutLinkHere;
            AppSDKAndroidLink.Text = AppResources.TaskrAboutLinkHere;

            AboutSection1.Text = AppResources.TaskrAboutSection1;
            AboutSection2.Text = AppResources.TaskrAboutSection2;
            AboutSection3.Text = AppResources.TaskrAboutSection3;
            AboutSection4.Text = AppResources.TaskrAboutSection4;
            AboutSection5.Text = AppResources.TaskrAboutSection5;

            AboutFooter.Text = AppResources.TaskrAboutFooter;
            AboutFooterLink.Text = AppResources.TaskrAboutLinkMicrosoft;
        }
    }
}