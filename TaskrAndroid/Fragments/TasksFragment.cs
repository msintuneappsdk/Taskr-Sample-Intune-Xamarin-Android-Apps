// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Intune.Mam.Client.Identity;
using Microsoft.Intune.Mam.Client.Support.V4.App;
using Microsoft.Intune.Mam.Policy;
using TaskrAndroid.Authentication;
using TaskrAndroid.Utils;

namespace TaskrAndroid.Fragments
{
    /// <summary>
    /// A Fragment subclass that handles the creation of a view of the tasks screen
    /// </summary>
    class TasksFragment : MAMFragment
    {
        public override View OnMAMCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View rootView = inflater.Inflate(Resource.Layout.fragment_tasks, container, false);

            // Make a view adapter that will update the list as data updates
            TaskListAdapter adapter = new TaskListAdapter(Context);
            ListView displayList = rootView.FindViewById<ListView>(Resource.Id.tasks_nav_list_view);
            displayList.Adapter = adapter;

            // Example of MAM policy - allow printing.
            // Will be automatically blocked by MAM if necessary.
            rootView.FindViewById(Resource.Id.tasks_nav_print_icon).Click += (sender, e) =>
            {
                if (Activity != null)
                {
                    PrintHelper printer = new PrintHelper(Activity);
                    printer.PrintTasks();
                }
                else
                {
                    Toast.MakeText(Activity, Resource.String.err_not_active, ToastLength.Long).Show();
                }
            };

            // Example of MAM policy - allow saving to device.
            // A manual check of the current MAM policy must be performed to determine whether or not saving to the device is allowed.
            // NOTE: If the user's policy asks the app to encrypt files, the output of this process will also be encrypted.
            rootView.FindViewById(Resource.Id.tasks_nav_save_icon).Click += (sender, e) =>
            {
                if (MAMPolicyManager.GetPolicy(Activity).GetIsSaveToLocationAllowed(SaveLocation.Local, AuthManager.User))
                {
                    if (Activity != null && Context != null)
                    {
                        SaveHelper saveHelper = new SaveHelper(Activity, Context, TargetRequestCode);
                        saveHelper.SaveFile();
                    }
                    else
                    {
                        Toast.MakeText(Activity, Resource.String.err_not_active, ToastLength.Long).Show();
                    }
                }
                else
                {
                    Toast.MakeText(Activity, Resource.String.err_not_allowed, ToastLength.Long).Show();
                }
            };

            return rootView;
        }
    }
}