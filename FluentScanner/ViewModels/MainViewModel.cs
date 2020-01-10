using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using FluentScanner.Helpers;
using FluentScanner.Views.Dialogs;
using Windows.Devices.Enumeration;
using Windows.Devices.Scanners;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

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
            }
        }

        // Resolution (DPI)


        public Frame DetailsFrame { get; set; }

        // UI Elements


        // TEMP - Image for in Details
        private StorageFile _imageFile;
        public StorageFile ImageFile
        {
            get { return _imageFile; }
            set { Set(ref _imageFile, value); }
        }

        private BitmapImage _scannedImage;
        public BitmapImage ScannedImage
        {
            get { return _scannedImage; }
            set { Set(ref _scannedImage, value); }
        }





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
            DetailsFrame = new Frame();
            DetailsFrame.Navigate(typeof(Views.EmptyDetailsPage));

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

        //private ICommand _saveEditedScanCommand;
        //public ICommand SaveEditedSaveCommand
        //{
        //    get
        //    {
        //        if (_saveEditedScanCommand == null)
        //        {
        //            _saveEditedScanCommand = new RelayCommand(
        //                () =>
        //                {
        //                    SaveModifiedScan();
        //                });
        //        }
        //        return _saveEditedScanCommand;
        //    }
        //}

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
        private async void OnScannerAdded(DeviceWatcher sender, DeviceInformation args)
        {
            Debug.WriteLine("MainViewModel - Adding Scanner...");
            if (!ScannerCollection.Contains(args))
            {
                ScannerCollection.Add(args);
                if (ScannerCollection.Count == 1)   // If this is the first scanner added, the count will be set to 1. IF os, set this scanner as selected by default
                {
                    Debug.WriteLine("MainViewModel - Adding Scanner - Setting as selected");
                    //SelectedDevice = ScannerCollection[0];
                }
            }
            Debug.WriteLine("MainViewModel - Adding Scanner... Done! :)");
        }

        private void OnScannerRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            // #TODO Remove this from selected if this scanner was the selected one

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
            while (SelectedScanner == null)
            {
                try
                {
                    SelectedScanner = await ImageScanner.FromIdAsync(_selectedDevice.Id);
                    ScannerSources = ScannerHelper.GetSupportedScanSources(SelectedScanner);
                }
                catch (Exception ex)
                {
                    // https://stackoverflow.com/questions/15772373/error-code-when-trying-to-connect-to-a-scanner-using-wpf

                    Debug.WriteLine("MainViewModel - Scanner is busy");
                    Debug.WriteLine(ex);
                }
            }

            // Set the default properties
            var defaultScannerSource = SelectedScanner.DefaultScanSource;
            if (ScannerSources.Contains(defaultScannerSource))
            {
                SelectedScannerSource = defaultScannerSource;
            }
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
            //ScannerAutoCropppingModes.Clear();
            //ScannerAutoCropppingModes = ScannerHelper.GetSupportedAutoCroppingModes(SelectedScanner, SelectedScannerSource);
            // #TODO Build this for DPI

            // Select Defaults
            switch (SelectedScannerSource)
            {
                case ImageScannerScanSource.Feeder:
                    {
                        // Format
                        var defaultScannerFormat = SelectedScanner.FeederConfiguration.DefaultFormat;
                        if (ScannerFormats.Contains(defaultScannerFormat))
                        { SelectedScannerFormat = defaultScannerFormat; }
                        // Colour mode
                        var defaultColourMode = SelectedScanner.FeederConfiguration.DefaultColorMode;
                        if (ScannerColourModes.Contains(defaultColourMode))
                        { SelectedScannerColourMode = defaultColourMode; }
                        // Auto Cropping Mode
                        //SelectedScanner.FeederConfiguration.AutoCroppingMode = ImageScannerAutoCroppingMode.Disabled;

                        break;
                    }
                case ImageScannerScanSource.Flatbed:
                    {
                        // Format
                        var defaultScannerFormat = SelectedScanner.FlatbedConfiguration.DefaultFormat;
                        if (ScannerFormats.Contains(defaultScannerFormat))
                        { SelectedScannerFormat = defaultScannerFormat; }
                        // Colour mode
                        var defaultColourMode = SelectedScanner.FlatbedConfiguration.DefaultColorMode;
                        if (ScannerColourModes.Contains(defaultColourMode))
                        { SelectedScannerColourMode = defaultColourMode; }
                        // Auto Cropping Mode
                        //SelectedScanner.FlatbedConfiguration.AutoCroppingMode = ImageScannerAutoCroppingMode.Disabled;

                        break;
                    }
                case ImageScannerScanSource.AutoConfigured:
                    {
                        // Only Format is available as a customisable option here
                        var defaultScannerFormat = SelectedScanner.AutoConfiguration.DefaultFormat;
                        if (ScannerFormats.Contains(defaultScannerFormat))
                        { SelectedScannerFormat = defaultScannerFormat; }
                        break;
                    }
                case ImageScannerScanSource.Default:
                    {
                        // No available settings for this
                        break;
                    }
            }


            // #TODO Update Visibility statusses


        }


        private void SetScannerPropertiesForScanning()
        {
            
            switch (SelectedScannerSource)
            {
                case ImageScannerScanSource.Flatbed:
                {
                    // #TODO Find out why VS auto tabs this too far.
                    // #TODO Add in exception catchers
                    //SelectedScanner.FlatbedConfiguration.Format = SelectedScannerFormat;
                    SelectedScanner.FlatbedConfiguration.Format = ImageScannerFormat.DeviceIndependentBitmap;   // Forcing scanning as a bitmap for high quality manipulation in the editor
                    SelectedScanner.FlatbedConfiguration.ColorMode = SelectedScannerColourMode;
                    //if (SelectedScannerAutoCroppingMode != null)
                    //{   SelectedScanner.FlatbedConfiguration.AutoCroppingMode = SelectedScannerAutoCroppingMode; }
                    break;
                }
                case ImageScannerScanSource.Feeder:
                {
                    //SelectedScanner.FeederConfiguration.Format = SelectedScannerFormat;
                    SelectedScanner.FeederConfiguration.Format = ImageScannerFormat.DeviceIndependentBitmap;    // Forcing scanning as a bitmap for high quality manipulation in the editor
                    SelectedScanner.FeederConfiguration.ColorMode = SelectedScannerColourMode;
                    //SelectedScanner.FeederConfiguration.AutoCroppingMode = SelectedScannerAutoCroppingMode;
                    break;
                }
                case ImageScannerScanSource.AutoConfigured:
                {
                    //SelectedScanner.AutoConfiguration.Format = SelectedScannerFormat;
                    SelectedScanner.AutoConfiguration.Format = ImageScannerFormat.DeviceIndependentBitmap;  // Forcing scanning as a bitmap for high quality manipulation in the editor
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

            var listOfFiles = result.ScannedFiles;
            // #TODO Check if it's an image (Bitmap, jpg or png file), otherwise show alternative details screen
            ImageFile = listOfFiles[0];
            // Show the picture in the details side with Windows Ink
            OpenScannedImageInDetails();
        }

        private async void OpenScannedImageInDetails()
        {
            DetailsFrame.Navigate(typeof(Views.InkDrawPicturePage), ImageFile);
            // Get the image
            try
            {
                //ScannedImage = await ImageHelper.GetBitmapFromImageAsync(ImageFile);

                // Open it in the details
                
            }
            catch { }
            
            // Check if it's an supported Image File


        }

        ////private async void SaveModifiedScan()
        ////{
        ////    FileSavePicker picker = new FileSavePicker();
        ////    picker.FileTypeChoices.Add("JPEG", new List<string>() { ".jpg" });

        ////    picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        ////    picker.SuggestedFileName = ("Scan" + DateTime.Now.ToString());

        ////    StorageFile file = await picker.PickSaveFileAsync();
        ////    if (file != null)
        ////    {
        ////        // Get the software bitmap
        ////        SoftwareBitmap softwareBitmap;
        ////        using (IRandomAccessStream stream = await ImageFile.OpenAsync(FileAccessMode.Read))
        ////        {
        ////            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
        ////            softwareBitmap = await decoder.GetSoftwareBitmapAsync();
        ////        }

        ////        // Write the software bitmap
        ////        using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
        ////        {
        ////            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
        ////            encoder.SetSoftwareBitmap(softwareBitmap);
        ////            encoder.IsThumbnailGenerated = true;

        ////            try
        ////            {
        ////                await encoder.FlushAsync();
        ////            }
        ////            catch { }
        ////        }
        ////    }
        ////}


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
