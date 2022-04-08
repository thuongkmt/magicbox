using Konbini.RfidFridge.TagManagement.Enums;
using System;
using System.Windows;

namespace Konbini.RfidFridge.TagManagement.Entities
{
    public class MenuItemModel
    {
        public string DisplayName { get; set; }
        public Screen ScreenName { get; set; }
        public bool IsDisplay { get; set; }
        public Visibility Visibility => IsDisplay ? Visibility.Visible : Visibility.Collapsed;
    }
}