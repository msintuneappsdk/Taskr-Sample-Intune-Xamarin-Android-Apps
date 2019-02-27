// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TaskrForms.Models;

namespace TaskrForms.Services
{
    public class DataStore : IDataStore<Item>
    {
        readonly SQLiteConnection database;

        public DataStore()
        {
            database = new SQLiteConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TodoSQLite.db3"));
            database.CreateTable<Item>();
        }

        public async Task<bool> AddItemAsync(Models.Item item)
        {
            database.Insert(item);
            return await System.Threading.Tasks.Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Models.Item item)
        {
            database.Update(item);
            return await System.Threading.Tasks.Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(Item item)
        {
            database.Delete(item);
            return await System.Threading.Tasks.Task.FromResult(true);
        }

        public async Task<Item> GetItemAsync(int id)
        {
            return await Task.FromResult(database.Table<Item>().Where(i => i.Id == id).FirstOrDefault());
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(database.Table<Item>().ToList());
        }

        public async Task<bool> DeleteItemsAsync()
        {
            database.DeleteAll<Item>();
            return await System.Threading.Tasks.Task.FromResult(true);
        }
    }
}