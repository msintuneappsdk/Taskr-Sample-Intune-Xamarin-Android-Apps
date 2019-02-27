// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Android.App;
using Android.Runtime;
using Microsoft.Intune.Mam.Client.App;
using Microsoft.Intune.Mam.Client.Notification;
using Microsoft.Intune.Mam.Policy;
using Microsoft.Intune.Mam.Policy.Notification;
using TaskrForms.Droid.Authentication;
using TaskrForms.Droid.Receivers;

namespace TaskrForms.Droid
{
    [Application]
    class TaskrApp : MAMApplication
    {
        public TaskrApp(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer) { }

        public override void OnMAMCreate()
        {
            // Register the MAMAuthenticationCallback as soon as possible.
            // This will handle acquiring the necessary access token for MAM.
            IMAMEnrollmentManager mgr = MAMComponents.Get<IMAMEnrollmentManager>();
            mgr.RegisterAuthenticationCallback(new MAMWEAuthCallback());

            // Register the notification receivers to receive MAM notifications.
            // Applications can receive notifications from the MAM SDK at any time.
            // More information can be found here: https://docs.microsoft.com/en-us/intune/app-sdk-android#register-for-notifications-from-the-sdk
            IMAMNotificationReceiverRegistry registry = MAMComponents.Get<IMAMNotificationReceiverRegistry>();
            foreach (MAMNotificationType notification in MAMNotificationType.Values())
            {
                registry.RegisterReceiver(new ToastNotificationReceiver(this), notification);
            }
            registry.RegisterReceiver(new EnrollmentNotificationReceiver(this), MAMNotificationType.MamEnrollmentResult);
            registry.RegisterReceiver(new WipeNotificationReceiver(this), MAMNotificationType.WipeUserData);

            base.OnMAMCreate();
        }
    }
}