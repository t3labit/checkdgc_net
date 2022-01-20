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
using Microsoft.Extensions.DependencyInjection;

namespace checkgreenpass
{
    public class DGC
    {
        public IServiceProvider Services { get; set; }
        private IServiceCollection serviceCollection = new ServiceCollection();
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDgcReader()                     // Add the DgcReaderService as singleton
                .AddItalianTrustListProvider(o =>       // Register at least one trust list provider
                {
                    o.RefreshInterval = TimeSpan.FromHours(24);
                    o.MinRefreshInterval = TimeSpan.FromHours(1);
                    o.BasePath = "C:\\Users\\elia\\tmp";
                    o.SaveCertificate = true;
                    o.UseAvailableListWhileRefreshing = true;
                })
                .AddItalianDrlBlacklistProvider(o =>
                {
                    o.RefreshInterval = TimeSpan.FromHours(24);
                    o.MinRefreshInterval = TimeSpan.FromHours(1);
                    o.UseAvailableValuesWhileRefreshing = true;
                    o.BasePath = "C:\\Users\\elia\\tmp";
                })      // The blacklist provider(s)
                .AddItalianRulesValidator(o =>
                {
                    o.RefreshInterval = TimeSpan.FromHours(24);
                    o.MinRefreshInterval = TimeSpan.FromHours(1);
                    o.BasePath = "C:\\Users\\elia\\tmp";
                    o.UseAvailableValuesWhileRefreshing = true;
                    o.ValidationMode = ValidationMode.Basic3G;
                });         // Finally, the rule validator(s)
        }

        public DGC()
        {
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();
        }

        public async Task<DgcValidationResult> ParseDgc(Bitmap img)
        {
            // create a barcode reader instance
            IBarcodeReader reader = new BarcodeReader();
            // load a bitmap
            var barcodeBitmap = img;
            // detect and decode the barcode inside the bitmap
            var decodedQrcode = reader.Decode(barcodeBitmap);

            DgcValidationResult res = new DgcValidationResult();

            //Console.WriteLine(decodedQrcode);
            // do something with the result
            if (decodedQrcode != null)
            {
                try
                {

                    Console.WriteLine(decodedQrcode.ToString());
                    string acceptanceCountry = "IT";    // Specify the 2-letter ISO code of the acceptance country
                    var dgcReader = Services.GetService<DgcReaderService>();
                    // Decode and validate the qr code data.
                    // The result will contain all the details of the validated object
                    res = await dgcReader.Verify(decodedQrcode.ToString(), acceptanceCountry);
                    Console.WriteLine("RES: " + res.RulesValidation.Status);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error verifying DGC: {e.Message}");
                }
            }
            return res;
        }

    }
}
