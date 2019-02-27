// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TaskrAndroid
{
    /// <summary>
    /// Manager classes are an abstraction on the data access layers
    /// </summary>
    public class TaskManager
    {
        private TaskDatabase _database;
        private TaskDatabase Database {
            get {
                if (_database == null)
                {
                    _database = new TaskDatabase();
                }

                return _database;
            }
            set { _database = value; }
        }

        public static TaskManager taskManager;
        static TaskManager()
        {
            taskManager = new TaskManager();
        }

        protected TaskManager() { }
        
        public static List<Task> GetAllTasks()
        {
            return new List<Task>(taskManager.Database.GetTasks());
        }

        public static int InsertTask(Task item)
        {
            return taskManager.Database.SaveTask(item);
        }

        public static int CompleteTask(int id)
        {
            return taskManager.Database.DeleteTask(id);
        }

        public static void DeleteAll()
        {
            taskManager.Database.DeleteAll();
        }

        /// <summary>
        /// Close the connection to the database and remove the file.
        /// </summary>
        public static bool CloseConnection()
        {
            bool success = true;

            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TodoItemDB.db3");
                SQLiteConnection database = new SQLiteConnection(path);
                database.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                File.Delete(path);

                taskManager.Database = null;
            }
            catch (Exception ex)
            {
                Log.Error(taskManager.GetType().Name, "Could not close connection to database. Exception: " + ex.Message);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Creates a CSV document of the current tasks.
        /// </summary>
        /// <returns>The current tasks formatted as a CSV.</returns>
        public static string CreateCSVDocument()
        {
            StringBuilder docBuilder = new StringBuilder("\"ID\",\"Task description\"");

            List<Task> tasks = GetAllTasks();
            foreach (Task task in tasks)
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
        public static string CreateHTMLDocument()
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
            List<Task> tasks = GetAllTasks();
            foreach (Task task in tasks)
            {
                docBuilder.Append("<tr><td>");
                docBuilder.Append(task.ID);
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
