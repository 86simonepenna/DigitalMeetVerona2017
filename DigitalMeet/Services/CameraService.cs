using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace DigitalMeet.Services
{
    public class CameraService :  IDisposable
    {
        private MediaCapture mediaCapture;
        private bool isInitialized = false;

        public MediaCapture MediaCaptureInstance
        {
            get { return mediaCapture; }
        }

        public async Task<bool> InitializeAsync()
        {
            if (mediaCapture == null)
            {
                var cameraDevice = await FindCameraDevice();

                if (cameraDevice == null)
                {
                    isInitialized = false;
                    return false;
                }

                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

                mediaCapture = new MediaCapture();
                try
                {
                    await mediaCapture.InitializeAsync(settings);
                    isInitialized = true;
                    return true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine("UsbCamera: UnauthorizedAccessException: " + ex.ToString() + "Ensure webcam capability is added in the manifest.");
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("UsbCamera: Exception when initializing MediaCapture:" + ex.ToString());
                    throw;
                }
            }
            return false;
        }

      

        public async Task<InMemoryRandomAccessStream> CapturePhoto()
        {
            var stream = new InMemoryRandomAccessStream();

            await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreatePng(), stream);
            return stream;
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public void Dispose()
        {
            mediaCapture?.Dispose();
            isInitialized = false;
        }

        private static async Task<DeviceInformation> FindCameraDevice()
        {
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return allVideoDevices.Count > 0 ? allVideoDevices[0] : null;
        }
    }
}