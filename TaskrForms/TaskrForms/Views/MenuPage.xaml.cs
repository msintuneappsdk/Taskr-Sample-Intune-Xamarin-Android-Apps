// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using TaskrForms.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TaskrForms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        List<HomeMenuItem> menuItems;
        public MenuPage()
        {
            InitializeComponent();

            menuItems = new List<HomeMenuItem>
            {
                new HomeMenuItem {Id = MenuItemType.Tasks, Title="My Tasks" },
                new HomeMenuItem {Id = MenuItemType.About, Title="About" },
                new HomeMenuItem {Id = MenuItemType.SignOut, Title="Sign Out" }
            };

            ListViewMenu.ItemsSource = menuItems;

            ListViewMenu.SelectedItem = menuItems[0];
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                if (((HomeMenuItem)e.SelectedItem).Id == MenuItemType.SignOut)
                {
                    DependencyService.Get<IAuthenticator>().SignOut();
                    App.AuthenticationResult = null;
                    Application.Current.MainPage = new LoginPage();
                }
                else
                {
                    var id = (int)((HomeMenuItem)e.SelectedItem).Id;
                    await RootPage.NavigateFromMenu(id);
                }
            };
        }
    }
}