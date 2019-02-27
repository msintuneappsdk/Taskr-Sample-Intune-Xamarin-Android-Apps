// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text;
using TaskrForms.Models;

namespace TaskrForms
{
    /// <summary>
    /// A utility for formatting the tasks.
    /// </summary>
    public static class TaskUtility
    {
        /// <summary>
        /// Creates a CSV document of the current tasks.
        /// </summary>
        /// <returns>The current tasks formatted as a CSV.</returns>
        public static string CreateCSVDocument(List<Item> items)
        {
            StringBuilder docBuilder = new StringBuilder("\"ID\",\"Task description\"");

            foreach (Item task in items)
            {
                docBuilder.Append("\n");
                docBuilder.Append(task.ToString(","));
            }

            return docBuilder.ToString();
        }

        /// <summary>
        /// Creates an HTML document of the current tasks.
        /// </summary>
        /// <returns>The current tasks formatted as an HTML document.</returns>
        public static string CreateHTMLDocument(List<Item> items)
        {
            StringBuilder docBuilder = new StringBuilder();

            // Create the document/table
            docBuilder.Append("<!DOCTYPE html><html><body><table>");
            docBuilder.AppendLine();
            docBuilder.Append("<tr><th>");

            // Set the header
            docBuilder.Append("ID");
            docBuilder.Append("</th><th>");
            docBuilder.Append("Task description");
            docBuilder.Append("</th></tr>");
            docBuilder.AppendLine();

            // Add rows to the table
            foreach (Item task in items)
            {
                docBuilder.Append("<tr><td>");
                docBuilder.Append(task.Id);
                docBuilder.Append("</td><td>");
                docBuilder.Append(task.Description);
                docBuilder.Append("</td></tr>");
                docBuilder.AppendLine();
            }

            // End the document/table
            docBuilder.AppendLine();
            docBuilder.Append("</table></body></html>");

            return docBuilder.ToString();
        }
    }
}
