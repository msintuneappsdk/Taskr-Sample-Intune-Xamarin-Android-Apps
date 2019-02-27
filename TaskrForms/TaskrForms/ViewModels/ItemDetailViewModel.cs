// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using TaskrForms.Models;

namespace TaskrForms.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public Item Item { get; set; }
        public ItemDetailViewModel(Item item = null)
        {
            Item = item;
        }
    }
}
