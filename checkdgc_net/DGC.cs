using System;
using System.Threading.Tasks;
using DgcReader;
using DgcReader.RuleValidators.Italy;
using DgcReader.TrustListProviders.Italy;
using System.Drawing;
using ZXing;
using System.Net.Http;
using System.Net;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing.Drawing2D;
using DgcReader.Models;
using DgcReader.Interfaces.BlacklistProviders;
using DgcReader.BlacklistProviders.Italy;

namespace checkdgc_net
{
    class DGC
    {

        private DgcReaderService dgcReader;
        private DgcItalianRulesValidator rulesValidator;
        private ItalianTrustListProvider trustListProvider;
        private ItalianDrlBlacklistProvider drlBlacklistProvider;
        private HttpClient httpClient;

        public DGC()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            httpClient = new HttpClient();

            // You can use the constructor
            rulesValidator = DgcItalianRulesValidator.Create(httpClient,
                new DgcItalianRulesValidatorOptions
                {
                    RefreshInterval = TimeSpan.FromHours(24),
                    MinRefreshInterval = TimeSpan.FromHours(1),
                    BasePath = "C:\\Users\\elia\\tmp",
                    UseAvailableValuesWhileRefreshing = true,
                    ValidationMode = ValidationMode.Basic3G
                });

            trustListProvider = ItalianTrustListProvider.Create(httpClient,
                new ItalianTrustListProviderOptions
                {
                    RefreshInterval = TimeSpan.FromHours(24),
                    MinRefreshInterval = TimeSpan.FromHours(1),
                    BasePath = "C:\\Users\\elia\\tmp",
                    SaveCertificate = true,
                    UseAvailableListWhileRefreshing = true
                });

            drlBlacklistProvider = ItalianDrlBlacklistProvider.Create(httpClient,
                new ItalianDrlBlacklistProviderOptions
                {
                    RefreshInterval = TimeSpan.FromHours(24),
                    MinRefreshInterval = TimeSpan.FromHours(1),
                    BasePath = "C:\\Users\\elia\\tmp",
                    UseAvailableValuesWhileRefreshing = true
                });

            // Create an instance of the DgcReaderService
            dgcReader = DgcReaderService.Create(
                trustListProviders: new[] { trustListProvider },
                blackListProviders: new IBlacklistProvider[] { rulesValidator, drlBlacklistProvider },
                rulesValidators: new[] { rulesValidator }
            );
        }

        public async Task<DgcValidationResult> ParseDgc(Bitmap img)
        {
            // create a barcode reader instance
            IBarcodeReader reader = new BarcodeReader();
            // load a bitmap
            var barcodeBitmap = img;
            // detect and decode the barcode inside the bitmap
            var decodedQrcode = reader.Decode(barcodeBitmap);

            //Console.WriteLine(decodedQrcode);
            // do something with the result
            if (decodedQrcode != null)
            {
                try
                {

                    Console.WriteLine(decodedQrcode.ToString());
                    string acceptanceCountry = "IT";    // Specify the 2-letter ISO code of the acceptance country

                    // Decode and validate the qr code data.
                    // The result will contain all the details of the validated object
                    var result = await dgcReader.Verify(decodedQrcode.ToString(), acceptanceCountry);


                    return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error verifying DGC: {e.Message}");
                }
            }
            return new DgcValidationResult();
        }

    }
}
