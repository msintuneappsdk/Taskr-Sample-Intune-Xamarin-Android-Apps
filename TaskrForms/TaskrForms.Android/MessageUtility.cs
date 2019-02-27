// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.Widget;
using TaskrForms.Droid;
using Xamarin.Forms;
using Application = Android.App.Application;

[assembly: Dependency(typeof(MessageUtility))]
namespace TaskrForms.Droid
{
    /// <summary>
    /// A utility for displaying messages to the user.
    /// </summary>
    class MessageUtility : IMessageUtility
    {
        /// <summary>
        /// Toasts a long alert to the user.
        /// </summary>
        /// <param name="message">The message to toast to the user.</param>
        public void LongAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }

        /// <summary>
        /// Toasts a short alert to the user.
        /// </summary>
        /// <param name="message">The message to toast to the user.</param>
        public void ShortAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }
    }
}