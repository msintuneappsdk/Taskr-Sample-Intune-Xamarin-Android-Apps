// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Microsoft.Intune.Mam.Client.Support.V4.App;

namespace TaskrAndroid.Fragments
{
    /// <summary>
    /// A Fragment subclass that handles the creation of a view of the submit screen.
    /// </summary>
    class SubmitFragment : MAMFragment
    {
        public override View OnMAMCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View rootView = inflater.Inflate(Resource.Layout.fragment_submit, container, false);
            if (Activity == null)
            {
                return rootView;
            }

            // This ensures that the keyboard doesn't automatically pop up when the app starts
            Window window = Activity.Window;
            if (window != null)
            {
                window.SetSoftInputMode(SoftInput.StateAlwaysHidden);
            }

            // Check if the app was sent an intent, if it's valid set the description field
            Intent intent = Activity.Intent;
            string type = intent.Type;
            if (type != null)
            {
                string text = intent.GetStringExtra(Intent.ExtraText);
                if (!type.Equals("text/plain") || text == null)
                {
                    Toast.MakeText(Activity, Resource.String.err_bad_intent, ToastLength.Long).Show();
                }
                else
                {
                    EditText description = rootView.FindViewById<EditText>(Resource.Id.submit_nav_description_text);
                    description.Text = text;
                }
            }

            // Attach the 'Submit' button listener
            rootView.FindViewById(Resource.Id.submit_nav_submit).Click += (sender, e) =>
            {
                if (Activity == null)
                {
                    return;
                }

                // Hide the keyboard for convenience
                InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
                View currentFocus = Activity.Window.CurrentFocus;
                if (imm != null && currentFocus != null)
                {
                    imm.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
                }

                // Get the text the user entered
                EditText descriptionField = Activity.FindViewById<EditText>(Resource.Id.submit_nav_description_text);

                // Confirm the fields look valid
                int toastMessage;
                if (string.IsNullOrEmpty(descriptionField.Text))
                {
                    toastMessage = Resource.String.submit_nav_no_description;
                }
                else
                {
                    // We know the user input is valid, submit it
                    Task task = new Task()
                    {
                        Description = descriptionField.Text
                    };

                    TaskManager.InsertTask(task);

                    // Now clear the input field
                    descriptionField.Text = "";
                    toastMessage = Resource.String.submit_nav_submitted;
                }

                Toast.MakeText(Activity, toastMessage, ToastLength.Long).Show();
            };

            return rootView;
        }
    }
}