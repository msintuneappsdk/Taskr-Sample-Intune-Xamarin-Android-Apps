// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.Content;
using Android.Util;
using Microsoft.Intune.Mam.Client.Notification;
using Microsoft.Intune.Mam.Policy.Notification;

namespace TaskrForms.Droid.Receivers
{
    /// <summary>
    /// Receives wipe notifications from the Intune service and deletes task data.
    /// See: https://docs.microsoft.com/en-us/intune/app-sdk-android#mamnotificationreceiver
    /// </summary>
    class WipeNotificationReceiver : Java.Lang.Object, IMAMNotificationReceiver
    {
        private Context context;

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="context"></param>
        public WipeNotificationReceiver(Context context)
        {
            this.context = context;
        }

        /// <summary>
        /// Handles the incoming wipe notification.
        /// </summary>
        /// <param name="notification">Incoming notification.</param>
        /// <returns>
        /// The receiver should return true if it handled the notification without error(or if it decided to ignore the notification). 
        /// If the receiver tried to take some action in response to the notification but failed to complete that action it should return false.
        /// </returns>
        public bool OnReceive(IMAMNotification notification)
        {
            Log.Info(GetType().Name, "Performing application wipe and clearing the app database.");

            return true;
        }
    }
}