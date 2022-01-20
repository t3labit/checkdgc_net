using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DgcReader.Models;
using OpenCvSharp;


namespace checkgreenpass
{
    public class Core
    {

        private QRCodeDetector qrCodeDetector;
        private DGC dgc; 
        public Core()
        {
            dgc = new DGC("C:\\users\\elia\\tmp", true);

            USBCameraConnect usb = new USBCameraConnect();
            usb.Connect(0);

            qrCodeDetector = new QRCodeDetector();

            usb.IncomingCameraImage += OnIncomingCameraImage;
        }


        private async void OnIncomingCameraImage(object sender, EventArgs e)
        {
            ImageEventArgs tI = (e as ImageEventArgs);
           
            // Get Image
            Mat im = OpenCvSharp.Extensions.BitmapConverter.ToMat(tI.Image);

            //-----------------------------------------------------
            // Visualization to understand
            Mat im2show = im.Clone();
            qrCodeDetector.Detect(im, out Point2f[] points);
            if (points.Length == 4)
            {
                for (int i = 1; i < points.Length; i++)
                {
                    OpenCvSharp.Point pt1 = new OpenCvSharp.Point(points[i - 1].X, points[i - 1].Y);
                    OpenCvSharp.Point pt2 = new OpenCvSharp.Point(points[i].X, points[i].Y);
                    Cv2.Line(im2show, pt1, pt2, Scalar.Red, 3);
                }
                OpenCvSharp.Point pt1_ = new OpenCvSharp.Point(points[points.Length - 1].X, points[points.Length - 1].Y);
                OpenCvSharp.Point pt2_ = new OpenCvSharp.Point(points[0].X, points[0].Y);
                Cv2.Line(im2show, pt1_, pt2_, Scalar.Red, 3);
            }
            //-----------------------------------------------------

            // ----------------------------------------------------
            // DGC detector
            var res = await dgc.ParseDgc(tI.Image);
            string results_info = "";
            if (res != null && res.Status == DgcResultStatus.Valid)
            {
                results_info = res.Dgc.Name.GivenName + " " + res.Dgc.Name.FamilyName + "  " + res.Dgc.DateOfBirth;
                Console.WriteLine("Codice QR Valido:\n" + results_info);
                Console.Beep(440, 2000);
            }

            Cv2.ImShow("im", im2show);
            Cv2.WaitKey(33);
        }
    }
}
