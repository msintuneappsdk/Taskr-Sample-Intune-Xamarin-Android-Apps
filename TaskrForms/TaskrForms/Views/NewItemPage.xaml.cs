// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using TaskrForms.Models;
using TaskrForms.Resx;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TaskrForms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();

            ToolbarItemCancel.Text = AppResources.ToolbarItemCancel;
            ToolbarItemSave.Text = AppResources.ToolbarItemSave;

            Item = new Item();
            TaskDescriptionLabel.Text = AppResources.DescriptionTitle;
            TaskDescriptionEditor.Placeholder = AppResources.TaskEditorPlaceholder;

            BindingContext = this;
        }

        async void Save_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Item.Description))
            {
                DependencyService.Get<IMessageUtility>().ShortAlert(AppResources.NoTaskDescriptionMessage);
                return;
            }

            MessagingCenter.Send(this, "AddItem", Item);
            await Navigation.PopModalAsync();
        }

        async void Cancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}