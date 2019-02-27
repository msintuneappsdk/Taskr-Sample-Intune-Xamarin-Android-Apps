// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using SQLite;

namespace TaskrForms.Models
{
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Returns a representation of this task with fields separated by spearator
        /// </summary>
        /// <param name="separator">the string to separate the fields of the task</param>
        /// <returns>a string representation of this task</returns>
        public string ToString(string separator)
        {
            return Quote("" + Id) + separator + Quote(Description);
        }

        /// <summary>
        /// Wraps str in quotes
        /// </summary>
        /// <param name="str">the string to wrap</param>
        /// <returns>str surounded by " characters</returns>
        private string Quote(string str)
        {
            return "\"" + str + "\"";
        }
    }
}