using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Scanners;

namespace FluentScanner.Helpers
{
    /// <summary>
    /// A static class that helps to get the available options for the scanner, because the standard desktop API set is kinda crappy imho
    /// </summary>
    public static class ScannerHelper
    {

        /// <summary>
        /// Gets all available Scan Sources for a scanner
        /// </summary>
        /// <param name="scanner"></param>
        /// <returns>List with all available ImageScannerScanSources for the given scanner</returns>
        public static List<ImageScannerScanSource> GetSupportedScanSources(ImageScanner scanner)
        {
            List<ImageScannerScanSource> availableScanSources = new List<ImageScannerScanSource>();

            foreach (ImageScannerScanSource scanSource in (ImageScannerScanSource[]) Enum.GetValues(typeof(ImageScannerScanSource)))
            {
                if (scanner.IsScanSourceSupported(scanSource))
                {
                    if (scanSource != ImageScannerScanSource.Default)   // Disabling Default as it'll only be used for getting preferences from
                    {
                        availableScanSources.Add(scanSource);

                    }
                }
            }

            return availableScanSources;
        }







        /// <summary>
        /// Gets all available Image Formats for a Scan Source
        /// </summary>
        /// <param name="scanner"></param>
        /// <param name="source"></param>
        /// <returns>List with all available ImageScannerFormats for the given source</returns>
        public static List<ImageScannerFormat> GetSupportedImageFormats(ImageScanner scanner, ImageScannerScanSource source)
        {
            List<ImageScannerFormat> availableFormats = new List<ImageScannerFormat>();

            switch (source)
            {
                case ImageScannerScanSource.Flatbed: // Flatbed
                    {
                        foreach (ImageScannerFormat format in (ImageScannerFormat[]) Enum.GetValues(typeof(ImageScannerFormat)))
                        {
                            if (scanner.FlatbedConfiguration.IsFormatSupported(format))
                            { availableFormats.Add(format); }
                        }
                        break;
                    }
                case ImageScannerScanSource.Feeder: // Feeder
                    {
                        foreach (ImageScannerFormat format in (ImageScannerFormat[])Enum.GetValues(typeof(ImageScannerFormat)))
                        {
                            if (scanner.FeederConfiguration.IsFormatSupported(format))
                            { availableFormats.Add(format); }
                        }
                        break;
                    }
                case ImageScannerScanSource.AutoConfigured: // Auto Configured
                    {
                        foreach (ImageScannerFormat format in (ImageScannerFormat[])Enum.GetValues(typeof(ImageScannerFormat)))
                        {
                            if (scanner.AutoConfiguration.IsFormatSupported(format))
                            { availableFormats.Add(format); }
                        }
                        break;
                    }
                case ImageScannerScanSource.Default:    // Catch the otherwise empty set by only providing Bitmap as option, as this is device independant
                    {
                        availableFormats.Add(ImageScannerFormat.DeviceIndependentBitmap);
                        break;
                    }
            }

            return availableFormats;
        }


        /// <summary>
        /// Gets all available Colour Modes for a Scan Source
        /// </summary>
        /// <param name="scanner"></param>
        /// <param name="source"></param>
        /// <returns>List with all available ImageScannerColorMode for the given source</returns>
        public static List<ImageScannerColorMode> GetSupportedColourModes(ImageScanner scanner, ImageScannerScanSource source)
        {
            List<ImageScannerColorMode> availableColorModes = new List<ImageScannerColorMode>();

            switch (source)
            {
                case ImageScannerScanSource.Flatbed: // Flatbed
                    {
                        foreach (ImageScannerColorMode colourMode in (ImageScannerColorMode[])Enum.GetValues(typeof(ImageScannerColorMode)))
                        {
                            if (scanner.FlatbedConfiguration.IsColorModeSupported(colourMode))
                            { availableColorModes.Add(colourMode); }
                        }
                        break;
                    }
                case ImageScannerScanSource.Feeder: // Feeder
                    {
                        foreach (ImageScannerColorMode colourMode in (ImageScannerColorMode[])Enum.GetValues(typeof(ImageScannerColorMode)))
                        {
                            if (scanner.FeederConfiguration.IsColorModeSupported(colourMode))
                            { availableColorModes.Add(colourMode); }
                        }
                        break;
                    }
                case ImageScannerScanSource.AutoConfigured: // Auto Configured
                    {
                        // Not possible, return empty List
                        break;
                    }
                case ImageScannerScanSource.Default:    // Catch the otherwise empty set by only providing Bitmap as option, as this is device independant
                    {
                        // Not possible, return empty List
                        break;
                    }
            }

            return availableColorModes;
        }

        /// <summary>
        /// Gets all available Colour Modes for a Scan Source
        /// </summary>
        /// <param name="scanner"></param>
        /// <param name="source"></param>
        /// <returns>List with all available ImageScannerColorMode for the given source</returns>
        public static List<ImageScannerAutoCroppingMode> GetSupportedAutoCroppingModes(ImageScanner scanner, ImageScannerScanSource source)
        {
            List<ImageScannerAutoCroppingMode> availableAutoCroppingModes = new List<ImageScannerAutoCroppingMode>();

            switch (source)
            {
                case ImageScannerScanSource.Flatbed: // Flatbed
                    {
                        foreach (ImageScannerAutoCroppingMode autoCroppingMode in (ImageScannerAutoCroppingMode[])Enum.GetValues(typeof(ImageScannerAutoCroppingMode)))
                        {
                            if (scanner.FlatbedConfiguration.IsAutoCroppingModeSupported(autoCroppingMode))
                            { availableAutoCroppingModes.Add(autoCroppingMode); }
                        }
                        break;
                    }
                case ImageScannerScanSource.Feeder: // Feeder
                    {
                        foreach (ImageScannerAutoCroppingMode autoCroppingMode in (ImageScannerAutoCroppingMode[])Enum.GetValues(typeof(ImageScannerAutoCroppingMode)))
                        {
                            if (scanner.FeederConfiguration.IsAutoCroppingModeSupported(autoCroppingMode))
                            { availableAutoCroppingModes.Add(autoCroppingMode); }
                        }
                        break;
                    }
                case ImageScannerScanSource.AutoConfigured: // Auto Configured
                    {
                        // Not possible, return empty List
                        break;
                    }
                case ImageScannerScanSource.Default:    // Catch the otherwise empty set by only providing Bitmap as option, as this is device independant
                    {
                        // Not possible, return empty List
                        break;
                    }
            }

            return availableAutoCroppingModes;
        }
    }
}
