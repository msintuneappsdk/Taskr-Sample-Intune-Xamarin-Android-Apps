// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Widget;
using Java.IO;
using Microsoft.Intune.Mam.Client.Identity;
using Microsoft.Intune.Mam.Policy;
using TaskrForms.Droid;
using TaskrForms.Droid.Authentication;
using Xamarin.Forms;
using Application = Android.App.Application;

[assembly: Dependency(typeof(SaveUtility))]
namespace TaskrForms.Droid
{
    /// <summary>
    /// A utility for saving the current tasks to either the device or OneDrive.
    /// </summary>
    class SaveUtility : ISaveUtility
    {
        /// <summary>
        /// Save the current tasks to the device.
        /// </summary>
        /// <remarks>
        /// Example of MAM policy - allow saving to device.
        /// A manual check of the current MAM policy must be performed to determine whether or not saving to the device is allowed.
        /// NOTE: If the user's policy asks the app to encrypt files, the output of this process will also be encrypted.
        /// </remarks>
        /// <param name="doc">The formatted CSV string representation of the current tasks.</param>
        public void Save(string doc)
        {
            if (!MAMPolicyManager.GetPolicy(Application.Context).GetIsSaveToLocationAllowed(SaveLocation.Local, Authenticator.User))
            {
                Toast.MakeText(Application.Context, Resource.String.err_not_allowed, ToastLength.Long).Show();
                return;
            }

            // Confirm we're allowed to save to this device, ask for permission if not.
            ConfirmWritePermission();

            // Get the default location for the export.
            File exportFile = new File(Android.OS.Environment.ExternalStorageDirectory, "tasks.csv");

            // Now try to write the document to their device.
            try
            {
                PrintWriter writer = new PrintWriter(exportFile);

                writer.Append(doc);
                writer.Flush();
            }
            catch (IOException e)
            {
                Toast.MakeText(Application.Context, e.Message, ToastLength.Long).Show();
                return;
            }

            // And try to open it. Will be blocked by MAM if necessary
            Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.save_success, exportFile.Path), ToastLength.Short).Show();
            OpenFile(exportFile);
        }

        /// <summary>
        /// Opens file as a CSV in an editor on the user's device, if one exists.
        /// </summary>
        /// <param name="file">The file to open.</param>
        private void OpenFile(File file)
        {
            Intent intent = new Intent(Intent.ActionView);
            intent.AddFlags(ActivityFlags.GrantWriteUriPermission | ActivityFlags.GrantReadUriPermission);

            Uri uri = FileProvider.GetUriForFile(Application.Context, typeof(CustomFileProvider).ToString(), file);
            intent.SetDataAndType(uri, "text/csv");

            // Confirm the user has at least one app that can open a CSV before trying to open it for them.
            PackageManager packageManager = Application.Context.PackageManager;
            var activities = packageManager.QueryIntentActivities(intent, 0);
            if (activities.Count > 0)
            {
                Application.Context.StartActivity(intent);
            }
            else
            {
                Toast.MakeText(Application.Context, Resource.String.open_file_failure, ToastLength.Long).Show();
            }
        }

        /// <summary>
        /// Confirm we can write the user's device, and if we currently can't, request the permission.
        /// </summary>
        private void ConfirmWritePermission()
        {
            if (PermissionChecker.CheckSelfPermission(Application.Context, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions((Activity)Forms.Context, new string[] { Manifest.Permission.WriteExternalStorage }, 0);
            }
        }
    }
}