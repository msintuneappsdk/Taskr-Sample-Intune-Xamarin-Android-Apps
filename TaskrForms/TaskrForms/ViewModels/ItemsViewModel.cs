// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using TaskrForms.Models;
using TaskrForms.Resx;
using TaskrForms.Views;
using Xamarin.Forms;

namespace TaskrForms.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        public ObservableCollection<Models.Item> Items { get; set; }
        public Command LoadItemsCommand { get; set; }

        public ItemsViewModel()
        {
            Title = AppResources.MyTasksTitle;
            Items = new ObservableCollection<Models.Item>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            MessagingCenter.Subscribe<NewItemPage, Models.Item>(this, "AddItem", async (obj, item) =>
            {
                var newItem = item as Models.Item;
                Items.Add(newItem);
                await DataStore.AddItemAsync(newItem);
            });

            MessagingCenter.Subscribe<ItemDetailPage, Models.Item>(this, "DeleteItem", async (obj, item) =>
            {
                var newItem = item as Models.Item;
                Items.Remove(newItem);
                await DataStore.DeleteItemAsync(newItem);
            });

            MessagingCenter.Subscribe<ItemsPage>(this, "PrintItems", async (obj) =>
            {
                List<Item> items = new List<Item>(await DataStore.GetItemsAsync());
                string document = TaskUtility.CreateHTMLDocument(items);

                var printUtility = DependencyService.Get<IPrintUtility>();
                printUtility.Print(document);
            });

            MessagingCenter.Subscribe<ItemsPage>(this, "SaveItemsToDevice", async (obj) =>
            {
                List<Item> items = new List<Item>(await DataStore.GetItemsAsync());
                string document = TaskUtility.CreateCSVDocument(items);

                var saveUtility = DependencyService.Get<ISaveUtility>();
                saveUtility.Save(document);
            });
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
