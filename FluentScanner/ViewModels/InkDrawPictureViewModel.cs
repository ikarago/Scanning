using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using FluentScanner.Helpers;
using FluentScanner.Services.Ink;

using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentScanner.ViewModels
{
    public class InkDrawPictureViewModel : Observable
    {
        private InkStrokesService _strokeService;
        private InkPointerDeviceService _pointerDeviceService;
        private InkFileService _fileService;
        private InkZoomService _zoomService;

        private bool enableTouch = false;
        private bool enableMouse = false;

        private BitmapImage image;
        private StorageFile tempScanFile;

        private ICommand loadImageCommand;
        private ICommand saveImageCommand;
        private ICommand clearAllCommand;
        private ICommand zoomInCommand;
        private ICommand zoomOutCommand;
        private ICommand resetZoomCommand;
        private ICommand fitToScreenCommand;

        public InkDrawPictureViewModel()
        {
        }

        public void Initialize(
            InkStrokesService strokeService,
            InkPointerDeviceService pointerDeviceService,
            InkFileService fileService,
            InkZoomService zoomService)
        {
            _strokeService = strokeService;
            _pointerDeviceService = pointerDeviceService;
            _fileService = fileService;
            _zoomService = zoomService;

            _strokeService.StrokesCollected += (s, e) => RefreshCommands();
            _strokeService.StrokesErased += (s, e) => RefreshCommands();
            _strokeService.ClearStrokesEvent += (s, e) => RefreshCommands();

            EnableTouch = false;
            EnableMouse = false;

            _pointerDeviceService.DetectPenEvent += (s, e) => EnableTouch = false;
        }

        public bool EnableTouch
        {
            get => enableTouch;
            set
            {
                Set(ref enableTouch, value);
                _pointerDeviceService.EnableTouch = value;
            }
        }

        public bool EnableMouse
        {
            get => enableMouse;
            set
            {
                Set(ref enableMouse, value);
                _pointerDeviceService.EnableMouse = value;
            }
        }

        public StorageFile ImageFile { get; set; }

        public BitmapImage Image
        {
            get => image;
            set
            {
                Set(ref image, value);
                RefreshCommands();
            }
        }

        public ICommand LoadImageCommand => loadImageCommand
            ?? (loadImageCommand = new RelayCommand(async () => await OnLoadImageAsync()));

        public ICommand SaveImageCommand => saveImageCommand
            ?? (saveImageCommand = new RelayCommand(async () => await OnSaveImageAsync(), CanSaveImage));

        public ICommand ZoomInCommand => zoomInCommand
            ?? (zoomInCommand = new RelayCommand(() => _zoomService?.ZoomIn()));

        public ICommand ZoomOutCommand => zoomOutCommand
            ?? (zoomOutCommand = new RelayCommand(() => _zoomService?.ZoomOut()));

        public ICommand ResetZoomCommand => resetZoomCommand
            ?? (resetZoomCommand = new RelayCommand(() => _zoomService?.ResetZoom()));

        public ICommand FitToScreenCommand => fitToScreenCommand
            ?? (fitToScreenCommand = new RelayCommand(() => _zoomService?.FitToScreen()));

        public ICommand ClearAllCommand => clearAllCommand
           ?? (clearAllCommand = new RelayCommand(ClearAll, CanClearAll));

        private async Task OnLoadImageAsync()
        {
            var file = await ImageHelper.LoadImageFileAsync();
            var bitmapImage = await ImageHelper.GetBitmapFromImageAsync(file);

            if (file != null && bitmapImage != null)
            {
                ClearAll();
                ImageFile = file;
                Image = bitmapImage;
                _zoomService?.FitToSize(Image.PixelWidth, Image.PixelHeight);
            }
        }

        // Improve this :)
        public async Task OnLoadImageAsync(StorageFile file)
        {
            tempScanFile = file;
            var bitmapImage = await ImageHelper.GetBitmapFromImageAsync(tempScanFile);

            if (file != null && bitmapImage != null)
            {
                ClearAll();
                ImageFile = file;
                Image = bitmapImage;
                _zoomService?.FitToSize(Image.PixelWidth, Image.PixelHeight);
            }
        }

        private async Task OnSaveImageAsync()
        {
            await _fileService?.ExportToImageAsync(ImageFile);

            try
            {
                Debug.WriteLine("InkDrawPictureViewModel - Attempting to delete TempScanFile...");
                await tempScanFile.DeleteAsync();
                Debug.WriteLine("InkDrawPictureViewModel - Deleted TempScanFile");

            }
            catch (Exception ex)
            {
                Debug.WriteLine("InkDrawPictureViewModel - Couldn't delete TempScanFile");
                Debug.WriteLine(ex);
            }
        }

        private bool CanSaveImage()
        {
            return Image != null;
                //&& _strokeService != null
                //&& _strokeService.GetStrokes().Any();
        }

        private bool CanClearAll()
        {
            return Image != null
                || (_strokeService != null
                    && _strokeService.GetStrokes().Any());
        }

        private void ClearAll()
        {
            _strokeService?.ClearStrokes();
            ImageFile = null;
            Image = null;
        }

        private void RefreshCommands()
        {
            (SaveImageCommand as RelayCommand)?.OnCanExecuteChanged();
            (ClearAllCommand as RelayCommand)?.OnCanExecuteChanged();
        }
    }
}
