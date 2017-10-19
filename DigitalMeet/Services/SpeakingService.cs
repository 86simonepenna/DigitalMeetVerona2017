using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace DigitalMeet.Services
{
    public class SpeakingService 
    {

        private MediaElement _mediaElement;
        private SpeechSynthesizer _speechSynthesizer;
        private SpeechRecognizer _speechRecognizer;
        private DateTime currentTime;
        private Language _recognizerLanguage;

        public event EventHandler SpeechRecognized;

        public enum SPEECH_OPERATIONS
        {
            OK_HOLO = 0,
            START_FOUND = 1,
            STOP_FOUND = 2
        }

        /// <summary>
        /// Map semantic meaning retrieved from speech to SPEECH_OPERATION Enum
        /// </summary>
        private static Dictionary<string, SPEECH_OPERATIONS> commandLookup = new Dictionary<string, SPEECH_OPERATIONS>
        {
            {"OK_HOLO",SPEECH_OPERATIONS.OK_HOLO},
            {"START_FOUND",SPEECH_OPERATIONS.START_FOUND },
            {"STOP_FOUND", SPEECH_OPERATIONS.STOP_FOUND}
        };

        public SpeakingService()
        {
            _mediaElement = new MediaElement();
            _speechSynthesizer = new SpeechSynthesizer();

            _recognizerLanguage = SpeechRecognizer.SystemSpeechLanguage;
            _speechRecognizer = new SpeechRecognizer(_recognizerLanguage);

            Task.Factory.StartNew(async () =>
            {

                string languagetag = _recognizerLanguage.LanguageTag;
                string filename = string.Format("srgs\\{0}\\basegrammar.xml", languagetag);
                StorageFile grammarfile = await Package.Current.InstalledLocation.GetFileAsync(filename);

                var grammar = new SpeechRecognitionGrammarFileConstraint(grammarfile, "basegrammar");
                _speechRecognizer.Constraints.Add(grammar);
                var compileResult = await _speechRecognizer.CompileConstraintsAsync();

                if (compileResult.Status == SpeechRecognitionResultStatus.Success)
                    await StartRecognizing();

            });

        }

        public async void SpeakText(string textToSpeak, bool IsDefaultSound = false)
        {
            if (IsDefaultSound)
            {
                var audio = await Package.Current.InstalledLocation.GetFileAsync(@"Assets\Speech-On.wav");
                var file = await audio.OpenReadAsync();
                _mediaElement.SetSource(file, file.ContentType);
                //_mediaElement.Play();
            }
            else
            {
                SpeechSynthesisStream voiceStream = await _speechSynthesizer.SynthesizeTextToStreamAsync(textToSpeak);
                _mediaElement.SetSource(voiceStream, voiceStream.ContentType);
                //_mediaElement.Play();
            }

        }

        /// <summary>
        /// Do voice command recognition, select the right command and fire an event
        /// </summary>
        /// <param name="isVoiceActive"> If true vocal command are available</param>
        /// <returns></returns>
        private async Task StartRecognizing(bool isVoiceActive = false)
        {
            try
            {
                SpeechRecognitionResult result = await _speechRecognizer.RecognizeAsync();

                if (DateTime.Now.Subtract(currentTime).TotalMilliseconds > 10000)
                {
                    isVoiceActive = false;
                }

                if (result.Status == SpeechRecognitionResultStatus.Success)
                {
                    if (result.Confidence!= SpeechRecognitionConfidence.Rejected &&  result.SemanticInterpretation.Properties.ContainsKey("cmd"))
                    {
                        var command = result.SemanticInterpretation.Properties["cmd"][0].ToString();
                        var cmd = commandLookup[command];

                        if (isVoiceActive)
                        {
                            switch (cmd)
                            {
                                case SPEECH_OPERATIONS.START_FOUND:
                                    currentTime = DateTime.Now;
                                    SpeechRecognized(SPEECH_OPERATIONS.START_FOUND, null);
                                    break;
                                case SPEECH_OPERATIONS.STOP_FOUND:
                                    currentTime = DateTime.Now;
                                    SpeechRecognized(SPEECH_OPERATIONS.STOP_FOUND, null);
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (cmd == SPEECH_OPERATIONS.OK_HOLO)
                        {
                            isVoiceActive = true;
                            currentTime = DateTime.Now;
                            SpeechRecognized(SPEECH_OPERATIONS.OK_HOLO, null);
                        }
                    }
                }

                await StartRecognizing(isVoiceActive);

                //if (isVoiceActive)
                //{
                //    switch (result.Text.ToLower())
                //    {
                //        case "find":
                //            currentTime = DateTime.Now;
                //            SpeechRecognized(SPEECH_OPERATIONS.START_DEVICE_FOUND, null);
                //            break;
                //        case "home":
                //            currentTime = DateTime.Now;
                //            SpeechRecognized(SPEECH_OPERATIONS.GO_HOME, null);
                //            break;
                //        case "telemetry":
                //            currentTime = DateTime.Now;
                //            SpeechRecognized(SPEECH_OPERATIONS.GO_TELEMETRY, null);
                //            break;
                //        case "remote support":
                //            currentTime = DateTime.Now;
                //            SpeechRecognized(SPEECH_OPERATIONS.REMOTE_SUPPORT, null);
                //            break;
                //        case "documents":
                //            currentTime = DateTime.Now;
                //            SpeechRecognized(SPEECH_OPERATIONS.DOCUMENTS, null);
                //            break;
                //        default:
                //            break;
                //    }
                //}


                //if (result.Text.ToLower() == "ciao" && isVoiceActive == false)
                //{
                //    isVoiceActive = true;
                //    currentTime = DateTime.Now;
                //    SpeechRecognized(SPEECH_OPERATIONS.OK_HOLO, null);
                //}

            }
            catch (Exception e)
            {
                throw;
            }


        }
    }
}
