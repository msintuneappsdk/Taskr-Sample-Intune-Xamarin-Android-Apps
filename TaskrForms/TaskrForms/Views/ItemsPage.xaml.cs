// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using TaskrForms.Resx;
using TaskrForms.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TaskrForms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemsPage : ContentPage
    {
        ItemsViewModel viewModel;

        public ItemsPage()
        {
            InitializeComponent();

            AddTaskButton.Text = AppResources.AddTaskButton;

            BindingContext = viewModel = new ItemsViewModel();
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (!(args.SelectedItem is Models.Item item))
                return;

            await Navigation.PushAsync(new ItemDetailPage(new ItemDetailViewModel(item)));

            // Manually deselect item.
            ItemsListView.SelectedItem = null;
        }

        void PrintItems_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "PrintItems");
        }

        void SaveItems_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "SaveItemsToDevice");
        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new NewItemPage()));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }
    }
}