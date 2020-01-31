// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.App;
using Microsoft.Intune.Mam.Client.App;
using Microsoft.Intune.Mam.Client.Notification;
using Microsoft.Intune.Mam.Policy;
using Microsoft.Intune.Mam.Policy.Notification;
using System;
using TaskrAndroid.Authentication;
using TaskrAndroid.Receivers;
using TaskrAndroid.Utils;

namespace TaskrAndroid
{
    /// <summary>
    /// The Taskr application.
    /// </summary>
#if DEBUG
    /// <remarks>
    /// Due to an issue with debugging the Xamarin bound MAM SDK the Debuggable = false attribute must be added to the Application in order to enable debugging.
    /// Without this attribute the application will crash when launched in Debug mode. Additional investigation is being performed to identify the root cause.
    /// </remarks>
    [Application(Debuggable = false)]
#else
    [Application]
#endif
    class TaskrApplication : MAMApplication
    {
        public TaskrApplication(IntPtr handle, Android.Runtime.JniHandleOwnership transfer)
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

        public override void OnTerminate()
        {
            base.OnTerminate();

            // Delete all tasks and close the database connection.
            TaskManager.CloseConnection();
        }
    }
}