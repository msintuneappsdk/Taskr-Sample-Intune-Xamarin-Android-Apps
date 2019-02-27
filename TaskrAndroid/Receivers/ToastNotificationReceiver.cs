// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.Content;
using Android.OS;
using Android.Widget;
using Microsoft.Intune.Mam.Client.Notification;
using Microsoft.Intune.Mam.Policy.Notification;

namespace TaskrAndroid.Utils
{
    /// <summary>
    /// Receives notifications from the Intune service and displays toast.
    /// See: https://docs.microsoft.com/en-us/intune/app-sdk-android#mamnotificationreceiver
    /// </summary>
    class ToastNotificationReceiver : Java.Lang.Object, IMAMNotificationReceiver
    {
        private readonly Context context;

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="context"> the context of the receiver.</param>
        public ToastNotificationReceiver(Context context)
        {
            this.context = context;
        }

        /// <summary>
        /// Handles the incoming notification.
        /// </summary>
        /// <param name="notification">Incoming notification.</param>
        /// <returns>True.</returns>
        public bool OnReceive(IMAMNotification notification)
        {
            Handler handler = new Handler(context.MainLooper);
            handler.Post(() => { Toast.MakeText(context, "Received MAMNotification of type " + notification.Type, ToastLength.Short).Show(); });
            return true;
        }
    }
}