// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.Support.V4.Content;

namespace TaskrForms.Droid
{
    /// <summary>
    /// This FileProvider allows the app to export files to other apps.
    /// Will automatically be blocked by MAM if necessary.
    /// </summary>
    class CustomFileProvider : FileProvider { }
}