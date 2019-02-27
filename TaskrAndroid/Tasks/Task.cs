// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using SQLite;
using System.Collections.Generic;

namespace TaskrAndroid
{
    /// <summary>
    /// Task business object for Android applications
    /// </summary>
    public class Task : Java.Lang.Object
    {
        public Task() { }

        // SQLite attributes
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int ID { get; set; }
        public string Description { get; set; }

        public bool Equals(Task task)
        {
            return ID == task.ID && Description.Equals(task.Description);
        }

        public override int GetHashCode()
        {
            var hashCode = -1355608983;
            hashCode = hashCode * -1521134295 + ID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            return hashCode;
        }

        /// <summary>
        /// Returns a representation of this task with fields separated by spearator
        /// </summary>
        /// <param name="separator">the string to separate the fields of the task</param>
        /// <returns>a string representation of this task</returns>
        public string ToString(string separator)
        {
            return Quote("" + ID) + separator + Quote(Description);
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