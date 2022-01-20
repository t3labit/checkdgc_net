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
        private string filesPath = "C:\\users\\elia\\tmp";
        private bool verbose = true;
        public IServiceProvider Services { get; set; }
        private IServiceCollection serviceCollection = new ServiceCollection();
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDgcReader()                     // Add the DgcReaderService as singleton
                .AddItalianTrustListProvider(o =>       // Register at least one trust list provider
                {
                    o.RefreshInterval = TimeSpan.FromHours(24);
                    o.MinRefreshInterval = TimeSpan.FromHours(1);
                    o.BasePath = filesPath;
                    o.SaveCertificate = true;
                    o.UseAvailableListWhileRefreshing = true;
                })
                .AddItalianDrlBlacklistProvider(o =>
                {
                    o.RefreshInterval = TimeSpan.FromHours(24);
                    o.MinRefreshInterval = TimeSpan.FromHours(1);
                    o.UseAvailableValuesWhileRefreshing = true;
                    o.BasePath = filesPath;
                })      // The blacklist provider(s)
                .AddItalianRulesValidator(o =>
                {
                    o.RefreshInterval = TimeSpan.FromHours(24);
                    o.MinRefreshInterval = TimeSpan.FromHours(1);
                    o.BasePath = filesPath;
                    o.UseAvailableValuesWhileRefreshing = true;
                    o.ValidationMode = ValidationMode.Basic3G;
                });         // Finally, the rule validator(s)

            Log("Service Configured!");
        }

        public DGC(string path = "C:\\users\\elia\\tmp", bool _verbose = true)
        {
            if (!string.IsNullOrEmpty(path))
            {
                filesPath = path;
            }
            if (verbose)
            {
                verbose = _verbose;
            }

            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();
        }

        public async void RefreshData()
        {
            await Services.GetService<ItalianTrustListProvider>().RefreshTrustList();
            await Services.GetService<ItalianDrlBlacklistProvider>().RefreshBlacklist();
            await Services.GetService<DgcItalianRulesValidator>().RefreshRules();
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
                    Log(decodedQrcode.ToString());
                    string acceptanceCountry = "IT";    // Specify the 2-letter ISO code of the acceptance country
                    var dgcReader = Services.GetService<DgcReaderService>();
                    // Decode and validate the qr code data.
                    // The result will contain all the details of the validated object
                    res = await dgcReader.Verify(decodedQrcode.ToString(), acceptanceCountry);
                    Log("RES: " + res.RulesValidation.Status);
                }
                catch (Exception e)
                {
                    Log($"Error verifying DGC: {e.Message}");
                }
            }
            return res;
        }

        private void Log(string msg)
        {
            if (verbose)
            {
                Console.WriteLine(msg);
            }
        }
    }
}
