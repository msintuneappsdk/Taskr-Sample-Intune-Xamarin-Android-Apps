// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Windows.Input;
using TaskrForms.Resx;
using Xamarin.Forms;

namespace TaskrForms.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public string AppConfig { get; private set; }

        public AboutViewModel()
        {
            Title = AppResources.AboutTitle;

            var configUtility = DependencyService.Get<IConfigUtility>();
            AppConfig = configUtility.GetCurrentAppConfig();
        }

        public ICommand OpenWebCommand => new Command<string>((url) =>
        {
            Device.OpenUri(new Uri(url));
        });
    }
}