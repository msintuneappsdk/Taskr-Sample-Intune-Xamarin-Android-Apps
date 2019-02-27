// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TaskrForms.Models
{
    public enum MenuItemType
    {
        Tasks,
        About,
        SignOut
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
    }
}
