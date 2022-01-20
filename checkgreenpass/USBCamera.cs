using System;
using OpenCvSharp;
using System.Threading;
using System.Drawing;


namespace checkgreenpass
{
    public class ImageEventArgs : EventArgs
    {
        public Bitmap Image { get; set; }
    }

    public class USBCameraConnect
    {
        public event EventHandler InitEvent;
        public event EventHandler IncomingCameraFaultEvent;
        public event EventHandler IncomingCameraImage;

        protected virtual void OnInitEvent(EventArgs e)
        {
            InitEvent?.Invoke(this, e);
        }
        protected virtual void OnFaultEvent(EventArgs e)
        {
            IncomingCameraFaultEvent?.Invoke(this, e);
        }
        protected virtual void OnIncomingCameraImageMessage(EventArgs e)
        {
            IncomingCameraImage?.Invoke(this, e);
        }
        private bool connected;
        private VideoCapture capture;
        private Thread ImageAcquisitionThread;

        public USBCameraConnect()
        { 
            connected = false;
        }

        private void ImageAcquisitionThreadProc()
        {
            try
            {
                if (capture.IsOpened())
                {
                    while (connected == true)
                    {
                        Mat imageCol = new Mat();
                        Mat imageGray = new Mat();

                        capture.Read(imageCol);
                        //Cv2.Resize(imageCol, imageCol, new OpenCvSharp.Size(cameraParams.Width_Px, cameraParams.Height_Px));

                        Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imageCol);
                        OnIncomingCameraImageMessage(new ImageEventArgs() { Image = bmp });
                        Thread.Sleep(33);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[X] ImageAcquisitionThreadProc:{e.Message}");
            }
        }

        public bool Connect()
        {
            capture = new VideoCapture(1);
            ImageAcquisitionThread = new Thread(new ThreadStart(ImageAcquisitionThreadProc));
            ImageAcquisitionThread.Start();
            connected = true;
            OnInitEvent(new EventArgs());
            return true;
        }

        public void Disconnect()
        {
            connected = false;
            capture.Release();
        }

        public bool IsConnected()
        {
            return connected;
        }
    }
}
