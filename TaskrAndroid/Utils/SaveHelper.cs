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

namespace TaskrAndroid.Utils
{
    /// <summary>
    /// A helper for saving the file to the device.
    /// The helper assumes the calling code has checked on the MAM policy to confirm that this is allowed.
    /// For example, <see cref="Fragments.TasksFragment"/>
    /// </summary>
    public class SaveHelper
    {
        private Activity activity;
        private Context context;
        private int requestCode;

        public SaveHelper(Activity activity, Context context, int requestCode)
        {
            this.activity = activity;
            this.context = context;
            this.requestCode = requestCode;
        }

        /// <summary>
        /// Save the file to device.
        /// </summary>
        public void SaveFile()
        {
            // Create the CSV task document
            string doc = TaskManager.CreateCSVDocument();

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
                Toast.MakeText(context, e.Message, ToastLength.Long).Show();
                return;
            }

            // And try to open it. Will be blocked by MAM if necessary
            Toast.MakeText(context, context.GetString(Resource.String.save_success, exportFile.Path), ToastLength.Short).Show();
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

            Uri uri = FileProvider.GetUriForFile(context, typeof(CustomFileProvider).ToString(), file);
            intent.SetDataAndType(uri, "text/csv");

            // Confirm the user has at least one app that can open a CSV before trying to open it for them.
            PackageManager packageManager = context.PackageManager;
            var activities = packageManager.QueryIntentActivities(intent, 0);
            if (activities.Count > 0)
            {
                context.StartActivity(intent);
            }
            else
            {
                Toast.MakeText(context, Resource.String.open_file_failure, ToastLength.Long).Show();
            }
        }

        /// <summary>
        /// Confirm we can write the user's device, and if we currently can't, ask to.
        /// </summary>
        private void ConfirmWritePermission()
        {
            if (PermissionChecker.CheckSelfPermission(context, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(activity, new string[] { Manifest.Permission.WriteExternalStorage }, requestCode);
            }
        }
    }
}