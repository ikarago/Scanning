using System;

using FluentScanner.ViewModels;

using Windows.UI.Xaml.Controls;

namespace FluentScanner.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
        }
    }
}
