// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TaskrForms
{
    /// <summary>
    /// A utility for saving the current tasks to the device.
    /// </summary>
    public interface ISaveUtility
    {
        /// <summary>
        /// Save the current tasks to the device.
        /// </summary>
        /// <param name="doc">The formatted CSV string representation of the current tasks.</param>
        void Save(string doc);
    }
}
