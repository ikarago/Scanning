using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using FluentScanner.Helpers;
using FluentScanner.Views.Dialogs;
using Windows.Devices.Enumeration;
using Windows.Devices.Scanners;
using Windows.Storage;
using Windows.UI.Core;

namespace FluentScanner.ViewModels
{
    public class MainViewModel : Observable
    {
        // Properties
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
                GetSelectedScannerSourceProperties();
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

        // Auto-Cropping
        private List<ImageScannerAutoCroppingMode> _scannerAutoCroppingModes;
        public List<ImageScannerAutoCroppingMode> ScannerAutoCropppingModes
        {
            get { return _scannerAutoCroppingModes; }
            set { Set(ref _scannerAutoCroppingModes, value); }
        }
        private ImageScannerAutoCroppingMode _selectedScannerAutoCroppingMode;
        public ImageScannerAutoCroppingMode SelectedScannerAutoCroppingMode
        {
            get { return _selectedScannerAutoCroppingMode; }
            set
            {
                Set(ref _selectedScannerAutoCroppingMode, value);
                UpdateSelectedScannerSourceProperties();
            }
        }

        // Resolution (DPI)



        // UI Elements





        // Constructor
        public MainViewModel()
        {
            Initialize();
        }
        // Initialize
        private void Initialize()
        {
            // Initialize properties
            ScannerSources = new List<ImageScannerScanSource>();
            ScannerFormats = new List<ImageScannerFormat>();
            ScannerColourModes = new List<ImageScannerColorMode>();
            ScannerAutoCropppingModes = new List<ImageScannerAutoCroppingMode>();


            // Start device watchers
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
                            ScanWithCustomSettings();
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
        // Device Watcher
        private void OnScannerAdded(DeviceWatcher sender, DeviceInformation args)
        {
            Debug.WriteLine("MainViewModel - Adding Scanner...");
            if (!ScannerCollection.Contains(args))
            {
                ScannerCollection.Add(args);
            }
            Debug.WriteLine("MainViewModel - Adding Scanner... Done! :)");
        }

        private void OnScannerRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
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
        }

        private void OnScannerEnumerationCompleted(DeviceWatcher sender, object args)
        {

        }

        public void StartDeviceWatcher()
        {
            _scannerWatcher.Start();
        }

        public void StopDeviceWatcher()
        {
            _scannerWatcher.Stop();
        }


        // Update methods
        private async void UpdateScannerInfo()
        {
            SelectedScanner = await ImageScanner.FromIdAsync(_selectedDevice.Id);
            ScannerSources = ScannerHelper.GetSupportedScanSources(SelectedScanner);
            // #TODO Set the default properties
        }

        /// <summary>
        /// Updates all the Lists containing the properties for the selected Scanner Source
        /// </summary>
        private void GetSelectedScannerSourceProperties()
        {
            // Get all supported options for the selected scanning source
            ScannerFormats.Clear();
            ScannerFormats = ScannerHelper.GetSupportedImageFormats(SelectedScanner, SelectedScannerSource);
            ScannerColourModes.Clear();
            ScannerColourModes = ScannerHelper.GetSupportedColourModes(SelectedScanner, SelectedScannerSource);
            ScannerAutoCropppingModes.Clear();
            ScannerAutoCropppingModes = ScannerHelper.GetSupportedAutoCroppingModes(SelectedScanner, SelectedScannerSource);
            // #TODO Build this for DPI

            // #TODO Select Defaults
            // Set defaults in _properyName to avoid triggering the UpdateSelectedScannerSourceProperties()-method
            


            // #TODO Update Visibility statusses


        }

        /// <summary>
        /// Updates the properties in the SelectedScanner in the correct selected source
        /// </summary>
        private void UpdateSelectedScannerSourceProperties()
        {
            // #TODO Do this with a switch case

        }

        private void SetScannerPropertiesForScanning()
        {
            
            switch (SelectedScannerSource)
            {
                case ImageScannerScanSource.Flatbed:
                {
                        // #TODO Find out why VS auto tabs this too far.
                        // #TODO Add in exception catchers
                    SelectedScanner.FlatbedConfiguration.Format = SelectedScannerFormat;
                    SelectedScanner.FlatbedConfiguration.ColorMode = SelectedScannerColourMode;
                        //if (SelectedScannerAutoCroppingMode != null)
                        //{   SelectedScanner.FlatbedConfiguration.AutoCroppingMode = SelectedScannerAutoCroppingMode; }
                        break;
                }
                case ImageScannerScanSource.Feeder:
                {
                    SelectedScanner.FeederConfiguration.Format = SelectedScannerFormat;
                    SelectedScanner.FeederConfiguration.ColorMode = SelectedScannerColourMode;
                    //SelectedScanner.FeederConfiguration.AutoCroppingMode = SelectedScannerAutoCroppingMode;
                    break;
                }
                case ImageScannerScanSource.AutoConfigured:
                {
                    SelectedScanner.FeederConfiguration.Format = SelectedScannerFormat;
                    break;
                }
                case ImageScannerScanSource.Default:
                {
                    break;
                }
            }
        }

        private async void ScanWithCustomSettings()
        {
            SetScannerPropertiesForScanning();
            // #TODO: Find the proper moment to clear the temp folder to avoid overflowing it with data
            StorageFolder tempFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;

            // Scan the data
            // #TODO Allow for cancelling the task
            var result = await SelectedScanner.ScanFilesToFolderAsync(SelectedScannerSource, tempFolder);

            // #TODO Show the picture in the details side with Windows Ink

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
