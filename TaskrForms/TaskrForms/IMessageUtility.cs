// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TaskrForms
{
    /// <summary>
    /// A utility for displaying messages to the user.
    /// </summary>
    public interface IMessageUtility
    {
        /// <summary>
        /// Displays a long alert to the user.
        /// </summary>
        /// <param name="message">The message to alert to the user.</param>
        void LongAlert(string message);

        /// <summary>
        /// Displays a short alert to the user.
        /// </summary>
        /// <param name="message">The message to alert to the user.</param>
        void ShortAlert(string message);
    }
}
