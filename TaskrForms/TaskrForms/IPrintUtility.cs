// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TaskrForms
{
    /// <summary>
    /// A utility for printing the current tasks.
    /// </summary>
    public interface IPrintUtility
    {
        /// <summary>
        /// Prints the tasks formatted as an HTML document.
        /// </summary>
        /// <param name="doc">The formatted HTML string representation of the current tasks.</param>
        void Print(string doc);
    }
}
