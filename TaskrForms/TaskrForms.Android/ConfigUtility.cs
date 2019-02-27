// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Intune.Mam.Client.App;
using Microsoft.Intune.Mam.Policy.AppConfig;
using System.Collections.Generic;
using System.Text;
using TaskrForms.Droid;
using TaskrForms.Droid.Authentication;
using Xamarin.Forms;
using Application = Android.App.Application;

[assembly: Dependency(typeof(ConfigUtility))]
namespace TaskrForms.Droid
{
    /// <summary>
    /// A utility for interacting with the Android MAM app config.
    /// </summary>
    class ConfigUtility : IConfigUtility
    {
        /// <summary>
        /// Gets the current MAM app config for the application.
        /// </summary>
        /// <returns>The current MAM app config.</returns>
        public string GetCurrentAppConfig()
        {
            IMAMAppConfigManager configManager = MAMComponents.Get<IMAMAppConfigManager>();
            IMAMAppConfig appConfig = configManager.GetAppConfig(Authenticator.User);

            if (appConfig != null)
            {
                StringBuilder builder = new StringBuilder();
                IList<IDictionary<string, string>> appConfigData = appConfig.FullData;
                foreach (IDictionary<string, string> dictionary in appConfigData)
                {
                    foreach (KeyValuePair<string, string> kvp in dictionary)
                    {
                        builder.AppendLine(string.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value));
                    }
                }

                return Application.Context.GetString(Resource.String.about_nav_config_text, builder.ToString());
            }

            return Application.Context.GetString(Resource.String.about_nav_config_text_missing);
        }
    }
}