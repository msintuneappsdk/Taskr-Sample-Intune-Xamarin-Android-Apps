// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskrForms.Services
{
    public interface IDataStore<T>
    {
        Task<bool> AddItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(T item);
        Task<T> GetItemAsync(int id);
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
        Task<bool> DeleteItemsAsync();
    }
}
