// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace TaskrForms
{
    public interface INotificationUtility
    {
        event EventHandler<EventArgs> wipeNotificationReceived;
    }
}
