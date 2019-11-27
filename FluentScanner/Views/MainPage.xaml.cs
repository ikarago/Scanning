using System;

using FluentScanner.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.Scanners;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Collections.Generic;
using FluentScanner.Helpers;

namespace FluentScanner.Views
{
    public sealed partial class MainPage : Page
    {
        // Properties
        public MainViewModel ViewModel { get; } = new MainViewModel();

        private DeviceWatcher scannerWatcher;
        private ObservableCollection<DeviceInformation> scannerCollection = new ObservableCollection<DeviceInformation>();
        private DeviceInformation selectedDevice;
        private ImageScanner selectedScanner; // Needs the reference to the Desktop SDK!

        private List<ImageScannerScanSource> scannerSources;
        private ImageScannerScanSource selectedScannerSource;

        private List<ImageScannerFormat> sourceFormats;
        private ImageScannerFormat selectedScannerFormat;


        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }
        //Initialize

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            cmbxScanner.ItemsSource = scannerCollection;
            cmbxScannerSource.ItemsSource = scannerSources;
            cmbxImageFormat.ItemsSource = sourceFormats;
        }




        // Click events
        private async void btnLoadScanners_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            InitDeviceWatcher();
            scannerWatcher.Start();
        }

        private async void cmbxScanner_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedDevice = (DeviceInformation)cmbxScanner.SelectedItem;
            selectedScanner = await ImageScanner.FromIdAsync(selectedDevice.Id);
            scannerSources = ScannerHelper.GetSupportedScanSources(selectedScanner);
            cmbxScannerSource.ItemsSource = scannerSources;
        }



        // Methods
        // Build with this resource: https://docs.microsoft.com/en-us/windows/uwp/devices-sensors/scan-from-your-app
        private void InitDeviceWatcher()
        {
            // Create a Device Watcher class for type Image Scanner for enumerating scanners
            scannerWatcher = DeviceInformation.CreateWatcher(DeviceClass.ImageScanner);

            scannerWatcher.Added += OnScannerAdded;
            scannerWatcher.Removed += OnScannerRemoved;
            scannerWatcher.EnumerationCompleted += OnScannerEnumerationCompleted;
        }

        private void UpdateItemCollection()
        {
            //cmbxScanner.ItemsSource = scannerCollection;
        }

        private void OnScannerEnumerationCompleted(DeviceWatcher sender, object args)
        {
            //cmbxScanner.ItemsSource = scannerCollection;
            UpdateItemCollection();
        }

        private async void OnScannerRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                Debug.WriteLine("MainPage - Removing Scanner...");
                // Create a temporary collection to enumerate through
                var tempCollection = scannerCollection;
                foreach (var item in tempCollection)
                {
                    if (args.Id == item.Id)
                    {
                        scannerCollection.Remove(item);
                    }
                }
                Debug.WriteLine("MainPage - Removing Scanner... Done! :)");
            });

        }

        // Run the update on the UI thread, as it's bound there
        private async void OnScannerAdded(DeviceWatcher sender, DeviceInformation args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                Debug.WriteLine("MainPage - Adding Scanner...");
                if (!scannerCollection.Contains(args))
                {
                    scannerCollection.Add(args);
                }
                Debug.WriteLine("MainPage - Adding Scanner... Done! :)");
            });

        }



        private async void ScanWithDefaultSettings()
        {
            StorageFolder tempFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;

            // Force to use bitmaps in the Flatbed config
            selectedScanner.FlatbedConfiguration.Format = ImageScannerFormat.DeviceIndependentBitmap;
            var result = selectedScanner.ScanFilesToFolderAsync(ImageScannerScanSource.Default, tempFolder);
            
        }

        private async void ScanWithSetSettings()
        {
            // #TODO: Find the proper moment to clear the temp folder to avoid overflowing it with data
            StorageFolder tempFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;

            // Scan the data
            var result = await selectedScanner.ScanFilesToFolderAsync(selectedScannerSource, tempFolder);


            //switch (selectedScannerSource)
            //{
            //    case ImageScannerScanSource.AutoConfigured:
            //        {
            //            // Set the custom settings
            //            selectedScanner.AutoConfiguration.Format = selectedScannerFormat;
            //            // #TODO Colour Mode
            //            // #TODO DPI
            //            break;
            //        }
            //    case ImageScannerScanSource.Flatbed:
            //        {
            //            // Set the custom settings
            //            selectedScanner.FlatbedConfiguration.Format = selectedScannerFormat;
            //            // #TODO Colour Mode
            //            // #TODO DPI
            //            break;
            //        }
            //    case ImageScannerScanSource.Feeder:
            //        {
            //            // Set the custom settings
            //            selectedScanner.FeederConfiguration.Format = selectedScannerFormat;
            //            // #TODO Colour Mode
            //            // #TODO DPI
            //            break;
            //        }
            //}
        }

        // 1 - Get scanners
        // 2 - Get available sources
        // 3 - Once selected, get all available options


        private void GetAllAvailableSettingsForCurrentScannerSource()
        {
            sourceFormats = ScannerHelper.GetSupportedImageFormats(selectedScanner, selectedScannerSource);
            cmbxImageFormat.ItemsSource = sourceFormats;
        }

        private void cmbxScannerSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedScannerSource = (ImageScannerScanSource)cmbxScannerSource.SelectedItem;
            GetAllAvailableSettingsForCurrentScannerSource();
        }

        private void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ScanWithSetSettings();
        }

        private void cmbxImageFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (selectedScannerSource)
            {
                case ImageScannerScanSource.AutoConfigured:
                    {
                        selectedScanner.AutoConfiguration.Format = (ImageScannerFormat)cmbxImageFormat.SelectedItem;
                        break;
                    }
                case ImageScannerScanSource.Flatbed:
                    {
                        selectedScanner.FlatbedConfiguration.Format = (ImageScannerFormat)cmbxImageFormat.SelectedItem;
                        break;
                    }
                case ImageScannerScanSource.Feeder:
                    {
                        selectedScanner.FeederConfiguration.Format = (ImageScannerFormat)cmbxImageFormat.SelectedItem;
                        break;
                    }


            }
        }
    }
}
