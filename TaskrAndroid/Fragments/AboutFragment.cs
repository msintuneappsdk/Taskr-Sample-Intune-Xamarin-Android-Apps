// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.OS;
using Android.Support.V4.App;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Microsoft.Intune.Mam.Client.App;
using Microsoft.Intune.Mam.Policy.AppConfig;
using System.Collections.Generic;
using System.Text;
using TaskrAndroid.Authentication;

/// <summary>
/// A Fragment subclass that handles the creation of a view of the about screen.
/// </summary>
namespace TaskrAndroid.Fragments
{
    class AboutFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_about, container, false);

            // Needed to make the links active
            TextView body1 = view.FindViewById<TextView>(Resource.Id.about_nav_body_1);
            body1.MovementMethod = LinkMovementMethod.Instance;
            TextView body2 = view.FindViewById<TextView>(Resource.Id.about_nav_body_2);
            body2.MovementMethod = LinkMovementMethod.Instance;
            TextView footer = view.FindViewById<TextView>(Resource.Id.about_nav_footer);
            footer.MovementMethod = LinkMovementMethod.Instance;

            TextView configText = view.FindViewById<TextView>(Resource.Id.about_nav_config_text);

            // Get and show the targeted application configuration
            IMAMAppConfigManager configManager = MAMComponents.Get<IMAMAppConfigManager>();
            IMAMAppConfig appConfig = configManager.GetAppConfig(AuthManager.User);

            if (appConfig == null)
            {
                configText.Text = GetString(Resource.String.err_unset);
            }
            else
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

                configText.Text = GetString(Resource.String.about_nav_config_text, builder.ToString());
            }

            return view;
        }
    }
}