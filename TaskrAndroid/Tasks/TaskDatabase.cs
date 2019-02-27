// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TaskrAndroid
{
    /// <summary>
    /// TaskDatabase for storing information regarding the user's tasks.
    /// </summary>
    public class TaskDatabase
    {
        static readonly object locker = new object();

        private SQLiteConnection database;
        private readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TodoItemDB.db3");

        /// <summary>
        /// Initializes a new instance of the TaskDatabase.
        /// </summary>
        public TaskDatabase() {
            database = new SQLiteConnection(path);
            database.CreateTable<Task>();
        }

        public List<Task> GetTasks()
        {
            lock (locker)
            {
                return database.Table<Task>().ToList();
            }
        }

        public Task GetTask(int id)
        {
            lock (locker)
            {
                return database.Table<Task>().FirstOrDefault(x => x.ID == id);
            }
        }

        public int SaveTask(Task item)
        {
            lock (locker)
            {
                if (item.ID != 0)
                {
                    database.Update(item);
                    return item.ID;
                }
                else
                {
                    return database.Insert(item);
                }
            }
        }

        public int DeleteTask(int id)
        {
            lock (locker)
            {
                return database.Delete<Task>(id);
            }
        }

        public void DeleteAll()
        {
            lock (locker)
            {
                database.DeleteAll<Task>();
            }
        }
    }

}
