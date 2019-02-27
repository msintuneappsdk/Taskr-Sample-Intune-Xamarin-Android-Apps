// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.Util;
using System;
using TaskrForms.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(NotificationUtility))]
namespace TaskrForms.Droid
{
    class NotificationUtility : INotificationUtility
    {
        public static NotificationUtility instance;

        public event EventHandler<EventArgs> wipeNotificationReceived;

        public NotificationUtility()
        {
            instance = this;
            Log.Info("Spartan117", "Init notification utility in Android");
        }

        public void SendWipeNotification()
        {
            Log.Info(GetType().Name, "Performing application wipe and deleting the app database.");

            if(wipeNotificationReceived == null)
                Log.Info("Spartan117", "Eventhandler is null ahhhhh");
            wipeNotificationReceived?.Invoke(this, new EventArgs());
        }
    }
}