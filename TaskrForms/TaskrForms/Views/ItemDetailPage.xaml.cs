// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using TaskrForms.Models;
using TaskrForms.Resx;
using TaskrForms.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TaskrForms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemDetailPage : ContentPage
    {
        ItemDetailViewModel viewModel;

        public ItemDetailPage(ItemDetailViewModel viewModel)
        {
            InitializeComponent();

            ToolbarItemComplete.Text = AppResources.ToolbarItemComplete;
            TaskDescriptionLabel.Text = AppResources.DescriptionTitle;

            BindingContext = this.viewModel = viewModel;
        }

        public ItemDetailPage()
        {
            InitializeComponent();

            var item = new Item
            {
                Description = "This is an item description."
            };

            viewModel = new ItemDetailViewModel(item);
            BindingContext = viewModel;
        }

        async void CompleteItem_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "DeleteItem", viewModel.Item);
            await Navigation.PopAsync();
        }
    }
}