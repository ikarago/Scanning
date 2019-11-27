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
        public static List<ImageScannerScanSource> GetAvailableScanSources(ImageScanner scanner)
        {
            List<ImageScannerScanSource> availableScanSources = new List<ImageScannerScanSource>();

            foreach (ImageScannerScanSource scanSource in (ImageScannerScanSource[]) Enum.GetValues(typeof(ImageScannerScanSource)))
            {
                if (scanner.IsScanSourceSupported(scanSource))
                { availableScanSources.Add(scanSource); }
            }

            return availableScanSources;
        }







        /// <summary>
        /// Gets all available Image Formats for a Scan Source
        /// </summary>
        /// <param name="scanner"></param>
        /// <param name="source">0 = Flatbed, 1 = Feeder, 2 = AutoConfiguration</param>
        /// <returns>List with all available ImageScannerFormats for the given source</returns>
        public static List<ImageScannerFormat> GetAvailableImageFormats(ImageScanner scanner, ImageScannerScanSource source)
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
            }

            return availableFormats;
        }



    }
}
