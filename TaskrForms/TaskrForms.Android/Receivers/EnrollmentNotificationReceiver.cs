// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Microsoft.Intune.Mam.Client.Notification;
using Microsoft.Intune.Mam.Policy;
using Microsoft.Intune.Mam.Policy.Notification;
using TaskrForms.Droid.Authentication;

namespace TaskrForms.Droid.Receivers
{
    /// <summary>
    /// Receives enrollment notifications from the Intune service and performs the corresponding action for the enrollment result.
    /// See: https://docs.microsoft.com/en-us/intune/app-sdk-android#mamnotificationreceiver
    /// </summary>
    class EnrollmentNotificationReceiver : Java.Lang.Object, IMAMNotificationReceiver
    {
        private Context context;

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="context"></param>
        public EnrollmentNotificationReceiver(Context context)
        {
            this.context = context;
        }

        /// <summary>
        /// When using the MAM-WE APIs found in IMAMEnrollManager, your app wil receive 
        /// IMAMEnrollmentNotifications back to signal the result of your calls.
        /// 
        /// More information can be found here: https://docs.microsoft.com/en-us/intune/app-sdk-android#result-and-status-codes
        /// </summary>
        public bool OnReceive(IMAMNotification notification)
        {
            if (notification.Type != MAMNotificationType.MamEnrollmentResult)
            {
                return true;
            }

            IMAMEnrollmentNotification enrollmentNotification = notification.JavaCast<IMAMEnrollmentNotification>();
            MAMEnrollmentManagerResult result = enrollmentNotification.EnrollmentResult;
            string upn = enrollmentNotification.UserIdentity;

            string message = string.Format(
                "Received MAM Enrollment result {0} for user {1}.", result.Name(), upn);
            Log.Info(GetType().Name, message);

            Handler handler = new Handler(context.MainLooper);
            handler.Post(() => { Toast.MakeText(context, message, ToastLength.Long).Show(); });

            if (result.Equals(MAMEnrollmentManagerResult.EnrollmentSucceeded)
                || result.Equals(MAMEnrollmentManagerResult.NotLicensed)
                || result.Equals(MAMEnrollmentManagerResult.Pending)
                || result.Equals(MAMEnrollmentManagerResult.UnenrollmentFailed)
                || result.Equals(MAMEnrollmentManagerResult.UnenrollmentSucceeded))
            {
                // You are not required to do anything here, these are primarily informational callbacks
                // so you can know the state of the enrollment attempt.
            }
            else if (result.Equals(MAMEnrollmentManagerResult.AuthorizationNeeded))
            {
                // Attempt to re-authorize.
                Authenticator.GetAuthenticator().UpdateAccessTokenForMAM();
            }
            else if (result.Equals(MAMEnrollmentManagerResult.CompanyPortalRequired))
            {
                // Intune blocks the user until the Company Portal is installed on the device.
                // An app can override OnMAMCompanyPortalRequired in a MAMActivity to add custom handling to this behavior.
            }
            else if (result.Equals(MAMEnrollmentManagerResult.EnrollmentFailed)
                || result.Equals(MAMEnrollmentManagerResult.WrongUser))
            {
                string blockMessage = Application.Context.GetString(Resource.String.err_blocked, result.Name());
                BlockUser(handler, blockMessage);
            }
            else
            {
                throw new NotSupportedException(string.Format("Unknown result code: {0}", result.Name()));
            }

            return true;
        }

        /// <summary>
        /// Blocks the user from accessing the application.
        /// </summary>
        /// <remarks>
        /// In a real application, the user would need to be blocked from proceeding forward and accessing corporate data. 
        /// In this sample app, we ask them politely to stop.
        /// </remarks>
        /// <param name="handler">Associated handler.</param>
        /// <param name="blockMessage">Message to display to the user.</param>
        private void BlockUser(Handler handler, string blockMessage)
        {
            Log.Error(GetType().Name, blockMessage);
            handler.Post(() => { Toast.MakeText(context, blockMessage, ToastLength.Long).Show(); });
        }
    }
}