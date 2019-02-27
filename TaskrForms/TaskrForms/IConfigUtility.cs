// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TaskrForms
{
    /// <summary>
    /// A utility for interacting with the MAM app config.
    /// </summary>
    public interface IConfigUtility
    {
        /// <summary>
        /// Gets the current MAM app config.
        /// </summary>
        /// <returns>The current MAM app config.</returns>
        string GetCurrentAppConfig();
    }
}
