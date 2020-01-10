using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Graphics.Canvas;

using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace FluentScanner.Services.Ink
{
    public class InkFileService
    {
        private readonly InkCanvas _inkCanvas;
        private readonly InkStrokesService _strokesService;

        public InkFileService(InkCanvas inkCanvas, InkStrokesService strokesService)
        {
            _inkCanvas = inkCanvas;
            _strokesService = strokesService;
        }

        public async Task<bool> LoadInkAsync()
        {
            var openPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            openPicker.FileTypeFilter.Add(".gif");

            var file = await openPicker.PickSingleFileAsync();
            return await _strokesService.LoadInkFileAsync(file);
        }

        public async Task SaveInkAsync()
        {
            if (!_strokesService.GetStrokes().Any())
            {
                return;
            }

            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            savePicker.FileTypeChoices.Add("Gif with embedded ISF", new List<string> { ".gif" });

            var file = await savePicker.PickSaveFileAsync();
            await _strokesService.SaveInkFileAsync(file);
        }

        public async Task<StorageFile> ExportToImageAsync(StorageFile imageFile = null)
        {
            //if (!_strokesService.GetStrokes().Any())
            //{
            //    return null;
            //}

            if (imageFile != null)
            {
                return await ExportCanvasAndImageAsync(imageFile);
            }
            else
            {
                return await ExportCanvasAsync();
            }
        }

        private async Task<StorageFile> ExportCanvasAndImageAsync(StorageFile imageFile)
        {
            var saveFile = await GetImageToSaveAsync();

            if (saveFile == null)
            {
                return null;
            }

            // Prevent updates to the file until updates are finalized with call to CompleteUpdatesAsync.
            CachedFileManager.DeferUpdates(saveFile);

            using (var outStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var device = CanvasDevice.GetSharedDevice();

                CanvasBitmap canvasbitmap;
                using (var stream = await imageFile.OpenAsync(FileAccessMode.Read))
                {
                    canvasbitmap = await CanvasBitmap.LoadAsync(device, stream);
                }

                using (var renderTarget = new CanvasRenderTarget(device, (int)_inkCanvas.Width, (int)_inkCanvas.Height, canvasbitmap.Dpi))
                {
                    using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
                    {
                        ds.DrawImage(canvasbitmap, new Rect(0, 0, (int)_inkCanvas.Width, (int)_inkCanvas.Height));
                        ds.DrawInk(_strokesService.GetStrokes());
                    }

                    await SaveImageAsync(imageFile, renderTarget, outStream);
                }
            }

            // Finalize write so other apps can update file.
            await CachedFileManager.CompleteUpdatesAsync(saveFile);

            return saveFile;
        }

        private async Task<StorageFile> ExportCanvasAsync()
        {
            var file = await GetImageToSaveAsync();

            if (file == null)
            {
                return null;
            }

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)_inkCanvas.Width, (int)_inkCanvas.Height, 96);

            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.DrawInk(_strokesService.GetStrokes());
            }

            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await SaveImageAsync(file, renderTarget, fileStream);
            }

            return file;
        }

        /// <summary>
        /// Opens the SaveFile Picker to let the user save their image
        /// </summary>
        /// <returns>StoageFile with the name and chosen FileType</returns>
        private async Task<StorageFile> GetImageToSaveAsync()
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedFileName = ("Scan" + "_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "_" + DateTime.Now.ToShortTimeString());
            //savePicker.SuggestedFileName = ("Scan" + "_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToLongTimeString()); 
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("PNG", new List<string>() { ".png" });
            savePicker.FileTypeChoices.Add("JPEG", new List<string>() { ".jpg" });
            savePicker.FileTypeChoices.Add("Bitmap", new List<string>() { ".bmp" });
            savePicker.FileTypeChoices.Add("TIFF", new List<string>() { ".tiff" });
            var saveFile = await savePicker.PickSaveFileAsync();

            return saveFile;
        }

        /// <summary>
        /// Writes the image to the given StorageFile
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="renderTarget"></param>
        /// <param name="outStream"></param>
        /// <returns></returns>
        private async Task<bool> SaveImageAsync(StorageFile imageFile, CanvasRenderTarget renderTarget, IRandomAccessStream outStream)
        {
            if (imageFile.FileType == ".png")
            {
                await renderTarget.SaveAsync(outStream, CanvasBitmapFileFormat.Png);
            }
            else if (imageFile.FileType == ".jpg")
            {
                await renderTarget.SaveAsync(outStream, CanvasBitmapFileFormat.Jpeg);
            }
            else if (imageFile.FileType == ".bmp")
            {
                await renderTarget.SaveAsync(outStream, CanvasBitmapFileFormat.Bmp);
            }
            else if (imageFile.FileType == ".tiff")
            {
                await renderTarget.SaveAsync(outStream, CanvasBitmapFileFormat.Tiff);
            }
            return true;
        }
    }
}
