using System;
using System.Diagnostics;
using FluentScanner.Services.Ink;
using FluentScanner.ViewModels;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentScanner.Views
{
    // For more information regarding Windows Ink documentation and samples see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/ink.md
    public sealed partial class InkDrawPicturePage : Page
    {
        public InkDrawPictureViewModel ViewModel { get; } = new InkDrawPictureViewModel();

        private StorageFile tempScanFile;

        public InkDrawPicturePage()
        {
            InitializeComponent();

            Loaded += (sender, eventArgs) =>
            {
                SetCanvasSize();
                image.SizeChanged += Image_SizeChanged;

                var strokeService = new InkStrokesService(inkCanvas.InkPresenter);

                ViewModel.Initialize(
                    strokeService,
                    new InkPointerDeviceService(inkCanvas),
                    new InkFileService(inkCanvas, strokeService),
                    new InkZoomService(canvasScroll));
            };
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            tempScanFile = (StorageFile)e.Parameter;
            await ViewModel.OnLoadImageAsync(tempScanFile);

        }

        private void SetCanvasSize()
        {
            inkCanvas.Width = Math.Max(canvasScroll.ViewportWidth, 1000);
            inkCanvas.Height = Math.Max(canvasScroll.ViewportHeight, 1000);
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height == 0 || e.NewSize.Width == 0)
            {
                SetCanvasSize();
            }
            else
            {
                inkCanvas.Width = e.NewSize.Width;
                inkCanvas.Height = e.NewSize.Height;
            }
        }

        private async void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                // #TODO remove temp scanned file
                try
                {
                    Debug.WriteLine("InkDrawPicturePage - Attempting to delete TempScanFile...");
                    await tempScanFile.DeleteAsync();
                    Debug.WriteLine("InkDrawPicturePage - Deleted TempScanFile");

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("InkDrawPicturePage - Couldn't delete TempScanFile");
                    Debug.WriteLine(ex);
                }
                Frame.GoBack();
            }
        }

        private void canvasScroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ViewModel.ZoomFactor = canvasScroll.ZoomFactor;
        }
    }
}
