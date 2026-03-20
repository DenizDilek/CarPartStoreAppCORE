using System.Windows;
using CarPartStoreApp.Localization;
using CarPartStoreApp.Models;
using CarPartStoreApp.ViewModels;

namespace CarPartStoreApp.Views
{
    /// <summary>
    /// Window for displaying part details in read-only mode
    /// </summary>
    public partial class PartDetailWindow : Window
    {
        public PartDetailWindow(CarPart part, ILocalizationService localization)
        {
            InitializeComponent();
            DataContext = new PartDetailViewModel(part, localization);

            // Enable title bar drag functionality
            TitleBar.MouseLeftButtonDown += (sender, e) => DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }
}
