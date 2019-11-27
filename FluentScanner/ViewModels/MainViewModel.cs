using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using FluentScanner.Helpers;
using FluentScanner.Views.Dialogs;
using Windows.Devices.Enumeration;
using Windows.Devices.Scanners;
using Windows.UI.Core;

namespace FluentScanner.ViewModels
{
    public class MainViewModel : Observable
    {
        // Properties
        public CoreDispatcher Dispatcher;

        private DeviceWatcher _scannerWatcher;

        // Scanners
        public ObservableCollection<DeviceInformation> ScannerCollection = new ObservableCollection<DeviceInformation>();
        private DeviceInformation _selectedDevice;
        public DeviceInformation SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                Set(ref _selectedDevice, value);
                UpdateScannerInfo();
            }
        }
        private ImageScanner _selectedScanner;
        public ImageScanner SelectedScanner
        {
            get { return _selectedScanner; }
            set { Set(ref _selectedScanner, value); }
        }

        // Scanner Source
        private List<ImageScannerScanSource> _scannerSources;
        public List<ImageScannerScanSource> ScannerSources
        {
            get { return _scannerSources; }
            set { Set(ref _scannerSources, value); }
        }
        private ImageScannerScanSource _selectedScannerSource;
        public ImageScannerScanSource SelectedScannerSource
        {
            get { return _selectedScannerSource; }
            set
            {
                Set(ref _selectedScannerSource, value);
                UpdateScannerSourceProperties();
            }
        }

        // Formats
        private List<ImageScannerFormat> _scannerFormats;
        public List<ImageScannerFormat> ScannerFormats
        {
            get { return _scannerFormats; }
            set { Set(ref _scannerFormats, value); }
        }
        private ImageScannerFormat _selectedScannerFormat;
        public ImageScannerFormat SelectedScannerFormat
        {
            get { return _selectedScannerFormat; }
            set
            {
                Set(ref _selectedScannerFormat, value);
                UpdateSelectedScannerSourceProperties();
            }
        }

        // Colour Modes
        private List<ImageScannerColorMode> _scannerColourModes;
        public List<ImageScannerColorMode> ScannerColourModes
        {
            get { return _scannerColourModes; }
            set { Set(ref _scannerColourModes, value); }
        }
        private ImageScannerColorMode _selectedScannerColourMode;
        public ImageScannerColorMode SelectedScannerColourMode
        {
            get { return _selectedScannerColourMode; }
            set
            {
                Set(ref _selectedScannerColourMode, value);
                UpdateSelectedScannerSourceProperties();
            }
        }

        // Resolution (DPI)



        // UI Elements





        // Constructor
        public MainViewModel()
        {
            // Get the dispatcher from the UI in case the UI is going to be a cunt again when adding and removing stuff from the collections
            //Dispatcher = dispatcher;
            Initialize();
        }
        // Initialize
        private void Initialize()
        {



            InitializeDeviceWatcher();
            StartDeviceWatcher();
        }
        private void InitializeDeviceWatcher()
        {
            // Create a Device Watcher class for type Image Scanner for enumerating scanners
            _scannerWatcher = DeviceInformation.CreateWatcher(DeviceClass.ImageScanner);

            _scannerWatcher.Added += OnScannerAdded;
            _scannerWatcher.Removed += OnScannerRemoved;
            _scannerWatcher.EnumerationCompleted += OnScannerEnumerationCompleted;
        }



        // Commands
        private ICommand _newScanCommand;
        public ICommand NewScanCommand
        {
            get
            {
                if (_newScanCommand == null)
                {
                    _newScanCommand = new RelayCommand(
                        () =>
                        {
                            // #TODO
                        });
                }
                return _newScanCommand;
            }
        }

        private ICommand _previewScanCommand;
        public ICommand PreviewScanCommand
        {
            get
            {
                if (_previewScanCommand == null)
                {
                    _previewScanCommand = new RelayCommand(
                        () =>
                        {
                            // #TODO
                        });
                }
                return _previewScanCommand;
            }
        }

        // #TODO Commands for starting and stopping the DeviceWatcher?


        private ICommand _aboutCommand;
        public ICommand AboutCommand
        {
            get
            {
                if (_aboutCommand == null)
                {
                    _aboutCommand = new RelayCommand(
                        () =>
                        {
                            ShowAboutDialog();
                        });
                }
                return _aboutCommand;
            }
        }

        private ICommand _settingsCommand;
        public ICommand SettingsCommand
        {
            get
            {
                if (_settingsCommand == null)
                {
                    _settingsCommand = new RelayCommand(
                        () =>
                        {
                            ShowSettingsDialog();
                        });
                }
                return _settingsCommand;
            }
        }


        // Methodes
        private void OnScannerEnumerationCompleted(DeviceWatcher sender, object args)
        {

        }

        private async void OnScannerRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            //{
                Debug.WriteLine("MainViewModel - Removing Scanner...");
                // Create a temporary collection to enumerate through
                var tempCollection = ScannerCollection;
                foreach (var item in tempCollection)
                {
                    if (args.Id == item.Id)
                    {
                        ScannerCollection.Remove(item);
                    }
                }
                Debug.WriteLine("MainViewModel - Removing Scanner... Done! :)");
            //});

        }

        // Run the update on the UI thread, as it's bound there
        private async void OnScannerAdded(DeviceWatcher sender, DeviceInformation args)
        {
            //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            //{
                Debug.WriteLine("MainViewModel - Adding Scanner...");
                if (!ScannerCollection.Contains(args))
                {
                    ScannerCollection.Add(args);
                }
                Debug.WriteLine("MainViewModel - Adding Scanner... Done! :)");
            //});

        }

        public void StartDeviceWatcher()
        {
            _scannerWatcher.Start();
        }

        public void StopDeviceWatcher()
        {
            _scannerWatcher.Stop();
        }


        private async void UpdateScannerInfo()
        {
            SelectedScanner = await ImageScanner.FromIdAsync(SelectedDevice.Id);
            ScannerSources = ScannerHelper.GetSupportedScanSources(SelectedScanner);
            // #TODO Set the default properties
        }

        private void UpdateScannerSourceProperties()
        {
            // #TODO Get the properties and set the defaults as the default selected option
            ScannerFormats = ScannerHelper.GetSupportedImageFormats(SelectedScanner, SelectedScannerSource);
            // #TODO Build a set for the colour modes

            //switch (SelectedScannerSource)
            //{
            //    case ImageScannerScanSource.AutoConfigured:
            //        {
            //            // #TODO Test this?
            //            SelectedScannerFormat = SelectedScanner.AutoConfiguration.DefaultFormat;
            //            break;
            //        }
            //}

        }

        /// <summary>
        /// Updates the properties in the SelectedScanner in the correct selected source
        /// </summary>
        private void UpdateSelectedScannerSourceProperties()
        {
            // #TODO Do this with a switch case

        }






        /// <summary>
        /// Opens the Settings Dialog
        /// </summary>
        private async void ShowSettingsDialog()
        {
            var dialog = new SettingsDialog();
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Opens the About Dialog
        /// </summary>
        private async void ShowAboutDialog()
        {
            var dialog = new AboutDialog();
            await dialog.ShowAsync();
        }





    }
}
