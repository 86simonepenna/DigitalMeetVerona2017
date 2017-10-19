using DigitalMeet.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UIDispatcher = Windows.ApplicationModel.Core.CoreApplication;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DigitalMeet
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public ObservableCollection<string> WordsRecognized { get; set; }
        private bool _isScanning;
        private string _buttonText;

        SpeakingService speakingService;


        public MainPage()
        {
            _isScanning = false;
            WordsRecognized = new ObservableCollection<string>();
            speakingService = new SpeakingService();
            speakingService.SpeechRecognized += SpeakingService_SpeechRecognized;

            this.InitializeComponent();

        }

        private void Start()
        {
            CameraService cameraService = new CameraService();
            CognitiveService cognitiveService = new CognitiveService();


            int countSearchTimes = 0;

            _isScanning = true;
            List_wordsRicognized.Visibility = Visibility.Collapsed;
            Button_start.Visibility = Visibility.Collapsed;
            Button_stop.Visibility = Visibility.Visible;
            Grid_loading.Visibility = Visibility.Visible;
            DM_logo.Visibility = Visibility.Collapsed;

            Task.Factory.StartNew(async () =>
            {

                if (cameraService.IsInitialized() == false)
                {
                    await cameraService.InitializeAsync();
                }

                while (_isScanning)
                {
                    countSearchTimes++;

                    Task.Delay(Config.Instance.ScanTimeDeley).Wait();

                    if (_isScanning)
                    {
                        var stream = (await cameraService.CapturePhoto());
                        stream.Seek(0);
                        byte[] imgBytes = new byte[stream.Size];
                        await stream.AsStream().ReadAsync(imgBytes, 0, (int)stream.Size);

                        var _words = await cognitiveService.ReadTextFromImage(imgBytes);

                        if (_words != null && _words.Count > 0)
                        {
                            await UIDispatcher.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                WordsRecognized.Clear();
                                foreach (var item in _words)
                                {
                                    WordsRecognized.Add(item);
                                }
                                Grid_loading.Visibility = Visibility.Collapsed;
                                List_wordsRicognized.Visibility = Visibility.Visible;
                                Button_start.Visibility = Visibility.Visible;
                                Button_stop.Visibility = Visibility.Collapsed;
                            });

                            _isScanning = false;
                        }
                    }
                }
            }).Wait();
        }

        private void StartRecognition(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void Stop()
        {
            _isScanning = false;
            Button_start.Visibility = Visibility.Visible;
            Button_stop.Visibility = Visibility.Collapsed;
            List_wordsRicognized.Visibility = Visibility.Collapsed;
            Grid_loading.Visibility = Visibility.Collapsed;
            DM_logo.Visibility = Visibility.Visible;
        }

        private void StopRecognition(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void SpeakingService_SpeechRecognized(object sender, EventArgs e)
        {
            Task.Factory.StartNew(async () =>
            {
                await UIDispatcher.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {

                    SpeakingService.SPEECH_OPERATIONS speechOperation = (SpeakingService.SPEECH_OPERATIONS)sender;

                    switch (speechOperation)
                    {
                        case SpeakingService.SPEECH_OPERATIONS.OK_HOLO:
                            speakingService.SpeakText("", true);
                            break;
                        case SpeakingService.SPEECH_OPERATIONS.START_FOUND:
                            Start();
                            break;
                        case SpeakingService.SPEECH_OPERATIONS.STOP_FOUND:
                            Stop();
                            break;
                        default:
                            break;
                    }
                });
            });
        }
    }
}
