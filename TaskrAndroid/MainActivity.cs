// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Views;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Intune.Mam.Client.App;
using Microsoft.Intune.Mam.Policy;
using TaskrAndroid.Fragments;
using TaskrAndroid.Authentication;
using Android.Support.V4.View;
using Android.Support.V7.App;

namespace TaskrAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public partial class MainActivity : AppCompatActivity,
        NavigationView.IOnNavigationItemSelectedListener, IAuthListener
    {
        private Handler handler;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // If the app has already started, the user has signed in, and the activity was just
            // restarted, skip the rest of this initialization and open the main UI
            if (savedInstanceState != null && AuthManager.ShouldRestoreSignIn(savedInstanceState))
            {
                RunOnUiThread(OpenMainview);
                return;
            }

            AuthManager authManager = AuthManager.GetManager();

            // Open a sign in view and wait for success before moving on to the main view
            OpenSignInView();
            handler = new SignInHandler(Looper.MainLooper, this, authManager, this);
        }

        /// <summary>
        /// Required override method for ADAL integration
        /// </summary>
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode, data);
        }

        private void OpenSignInView()
        {
            SetContentView(Resource.Layout.sign_in);

            View signInButton = FindViewById(Resource.Id.sign_in_button);
            signInButton.Click += (sender, e) =>
            {
                handler.SendEmptyMessage((int)PromptBehavior.Always);
            };
        }

        /// <summary>
        /// Attempts to register the account for MAM using the given access token before moving on
        /// to the main view
        /// </summary>
        /// <param name="result"> the AuthenticationResult containing a valid access token</param>
        public void OnSignedIn(AuthenticationResult result)
        {
            string upn = result.UserInfo.DisplayableId;
            string aadId = result.UserInfo.UniqueId;
            string tenantId = result.TenantId;

            // Register the account for MAM
            // See: https://docs.microsoft.com/en-us/intune/app-sdk-android#account-authentication
            // This app requires ADAL authentication prior to MAM enrollment so we delay the registration
            // until after the sign in flow.
            IMAMEnrollmentManager mgr = MAMComponents.Get<IMAMEnrollmentManager>();
            mgr.RegisterAccountForMAM(upn, aadId, tenantId);

            //Must be run on the UI thread because it is modifying the UI
            RunOnUiThread(OpenMainview);
        }

        /// <summary>
        /// Called when the user signs out and returns the user to the sign in view.
        /// </summary>
        /// <remarks>
        /// In a real app, the tasks would have been pushed to a server and not stored locally.
        /// Here the tasks are left in the cache and persist from one user to the next.
        /// </remarks>
        public void OnSignedOut()
        {
            Toast.MakeText(this, Resource.String.auth_out_success, ToastLength.Short).Show();
            RunOnUiThread(OpenSignInView);
        }

        private void OpenMainview()
        {
            SetContentView(Resource.Layout.activity_main);
            Android.Support.V7.Widget.Toolbar toolbar =
                FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            Android.Support.V7.App.ActionBarDrawerToggle toggle =
                new Android.Support.V7.App.ActionBarDrawerToggle(
                    this, drawer, toolbar, Resource.String.navigation_drawer_open,
                    Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            ChangeNavigationView(Resource.Id.nav_submit);
            Toast.MakeText(this, Resource.String.auth_success, ToastLength.Short).Show();
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            return ChangeNavigationView(item.ItemId);
        }

        /// <summary>
        /// Changes the sidebar view of the app. Used when a user clicks on a menu item or to
        /// manually change the view
        /// </summary>
        /// <param name="id"> the id of the fragment that should be displayed</param>
        private bool ChangeNavigationView(int id)
        {
            Android.Support.V4.App.Fragment frag = null;

            switch (id)
            {
                case Resource.Id.nav_tasks:
                    frag = new TasksFragment();
                    break;
                case Resource.Id.nav_about:
                    frag = new AboutFragment();
                    break;
                case Resource.Id.nav_sign_out:
                    AuthManager.GetManager().SignOut(this);
                    break;
                default: // If we don't recognize the id, go to the default (submit) rather than crashing
                case Resource.Id.nav_submit:
                    frag = new SubmitFragment();
                    break;
            }

            bool didChangeView = frag != null;
            if (didChangeView)
            {
                try
                {
                    // Display the fragment
                    Android.Support.V4.App.FragmentManager fragManager = SupportFragmentManager;
                    fragManager.BeginTransaction().Replace(Resource.Id.flContent, frag).Commit();
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine(e.StackTrace);
                    didChangeView = false;
                }
            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if (drawer != null)
            {
                drawer.CloseDrawer(Android.Support.V4.View.GravityCompat.Start);
            }
            return didChangeView;
        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if (drawer != null && drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            AuthManager.OnSaveInstanceState(outState);
        }
    }
}

