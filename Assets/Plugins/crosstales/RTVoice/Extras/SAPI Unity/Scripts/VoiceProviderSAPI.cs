using UnityEngine;
using System.Collections;
using System.Linq;

namespace Crosstales.RTVoice.SAPI
{
   /// <summary>
   /// Example for a custom voice provider (TTS-system) with all callbacks (only for demonstration - it doesn't do anything).
   /// NOTE: please make sure you understand the Wrapper and its variables
   /// </summary>
   [ExecuteInEditMode]
   public class VoiceProviderSAPI : Provider.BaseCustomVoiceProvider
   {
      #region Variables

      //private const int SPF_DEFAULT = 0;
      private const int SPF_ASYNC = 1;

      private const int SPF_PURGEBEFORESPEAK = 2;

      //private const int SPF_IS_FILENAME = 4;
      private const int SPF_IS_XML = 8;

      //private const int SPF_IS_NOT_XML = 16;
      //private const int SPF_PERSIST_XML = 32;
      //private const int SPF_NLP_SPEAK_PUNC = 64;
      //private const int SPF_PARSE_SAPI = 128;
      private const int SPF_PARSE_SSML = 256;

      private static bool isDestroyed;
      private static bool isInitalized;

      #endregion


      #region Bridge declaration and methods

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      [System.Runtime.InteropServices.DllImport("SAPI_UNITY_DLL")]
      private static extern int Uni_Voice_Init();

      [System.Runtime.InteropServices.DllImport("SAPI_UNITY_DLL")]
      private static extern void Uni_Voice_Close();

      [System.Runtime.InteropServices.DllImport("SAPI_UNITY_DLL")]
      private static extern int Uni_Voice_Status(int voiceStat);

      [System.Runtime.InteropServices.DllImport("SAPI_UNITY_DLL")]
      private static extern int Uni_Voice_Speak([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string TextToSpeech); // SPF_ASYNC & SPF_IS_XML

      [System.Runtime.InteropServices.DllImport("SAPI_UNITY_DLL")]
      private static extern int Uni_Voice_SpeakEX([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string TextToSpeech, int voiceFlag); // CUSTOM FLAG

      [System.Runtime.InteropServices.DllImport("SAPI_UNITY_DLL")]
      private static extern int Uni_Voice_Volume(int volume); //  zero to 100

      [System.Runtime.InteropServices.DllImport("SAPI_UNITY_DLL")]
      private static extern int Uni_Voice_Rate(int rate); // -10 to 10

      [System.Runtime.InteropServices.DllImport("SAPI_UNITY_DLL")]
      private static extern void Uni_Voice_Pause();

      [System.Runtime.InteropServices.DllImport("SAPI_UNITY_DLL")]
      private static extern void Uni_Voice_Resume();
#endif

      #endregion


      #region Properties

      public override string AudioFileExtension => "none";

      public override AudioType AudioFileType => AudioType.UNKNOWN;

      public override string DefaultVoiceName => "David";

      public override bool isWorkingInEditor => Util.Helper.isWindowsEditor;

      public override bool isWorkingInPlaymode => true;

      public override bool isPlatformSupported => Util.Helper.isWindowsPlatform || Util.Helper.isWindowsEditor;

      public override int MaxTextLength => 256000;

      public override bool isSpeakNativeSupported => true;

      public override bool isSpeakSupported => false;

      public override bool isSSMLSupported => true;

      public override bool isOnlineService => false;

      public override bool hasCoRoutines => true;

      public override bool isIL2CPPSupported => true;

      public override bool hasVoicesInEditor => true;

      #endregion


      #region MonoBehaviour methods

      protected override void Start()
      {
         base.Start();
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
         if (isPlatformSupported && !isInitalized && Speaker.Instance.CustomProvider == this)
         {
            Uni_Voice_Init();
            isInitalized = true;
         }
#endif
      }

      private void OnApplicationQuit()
      {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
         if (!isDestroyed && isInitalized)
         {
            Uni_Voice_Close();
            isDestroyed = true;
            isInitalized = false;
         }
#endif
      }

      #endregion


      #region Implemented methods

      public override void Load(bool forceReload = false)
      {
#if UNITY_STANDALONE_WIN && NET_4_6
         if (cachedVoices?.Count == 0 || forceReload)
         {
            try
            {
               const string speechTokens = "Software\\Microsoft\\Speech\\Voices\\Tokens";

               System.Collections.Generic.List<Model.Voice> voices = new System.Collections.Generic.List<Model.Voice>();

               using (Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(speechTokens))
               {
                  if (registryKey != null)
                  {
                     foreach (string regKeyFound in registryKey.GetSubKeyNames())
                     {
                        string finalKey = $"HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Speech\\Voices\\Tokens\\{regKeyFound}\\Attributes";

                        string voiceName = (string)Microsoft.Win32.Registry.GetValue(finalKey, "name", "");

                        if (!string.IsNullOrEmpty(voiceName))
                        {
                           //desc = voice.GetDescription();
                           string desc = voiceName;
                           string gender = (string)Microsoft.Win32.Registry.GetValue(finalKey, "gender", "");
                           string age = (string)Microsoft.Win32.Registry.GetValue(finalKey, "age", "");
                           string lang = (string)Microsoft.Win32.Registry.GetValue(finalKey, "language", "");
                           string vendor = (string)Microsoft.Win32.Registry.GetValue(finalKey, "vendor", "");
                           string version = (string)Microsoft.Win32.Registry.GetValue(finalKey, "version", "");

                           if (string.IsNullOrEmpty(lang))
                           {
                              lang = "409"; //en-US
                           }
                           else if (lang.Length > 4)
                           {
                              string[] codes = lang.Split(',');

                              lang = codes.Length > 1 ? codes[0] : "409";
                           }

                           int langCode = int.Parse(lang, System.Globalization.NumberStyles.HexNumber);

                           if (!Util.Helper.LocaleCodes.TryGetValue(langCode, out string culture))
                           {
                              Debug.LogWarning("Voice with name '" + voiceName + "' has an unknown language code: " +
                                               langCode + "(" + lang + ")!", this);

                              culture = "en-us";
                           }

                           voices.Add(new Model.Voice(voiceName, desc, Util.Helper.StringToGender(gender), age,
                              culture, regKeyFound, vendor, version));
                        }
                        else
                        {
                           Debug.LogWarning("Voice ignored because it has no name: " + regKeyFound, this);
                        }
                     }
                  }

                  cachedVoices = voices.OrderBy(s => s.Name).ToList();
               }
            }
            catch (System.Exception ex)
            {
               string errorMessage = "Could not get any voices: " + ex;
               Debug.LogError(errorMessage, this);
               onErrorInfo(null, errorMessage);
            }
         }
#else
         Debug.LogError("SAPI Unity is not supported under the current platform!", this);
#endif

         onVoicesReady();
      }

      public override IEnumerator Generate(Model.Wrapper wrapper)
      {
         Debug.LogError("'Generate' is not supported for SAPI Unity!", this);
         yield return null;
      }

      public override IEnumerator SpeakNative(Model.Wrapper wrapper)
      {
         yield return speak(wrapper, true);
      }

      public override IEnumerator Speak(Model.Wrapper wrapper)
      {
         yield return speak(wrapper, false);
      }

      public override void Silence()
      {
#if UNITY_STANDALONE_WIN
         //Debug.Log(Uni_Voice_Status(0));

         if (!isDestroyed)
            Uni_Voice_SpeakEX(" ", SPF_ASYNC | SPF_PURGEBEFORESPEAK);
         //Uni_Voice_Pause();
#endif

         base.Silence();
      }

      public override void Silence(string uid)
      {
         Silence();

         base.Silence(uid);
      }

      #endregion


      #region Private methods

      private IEnumerator speak(Model.Wrapper wrapper, bool isNative)
      {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
         if (wrapper == null)
         {
            Debug.LogWarning("'wrapper' is null!", this);
         }
         else
         {
            if (string.IsNullOrEmpty(wrapper.Text))
            {
               Debug.LogWarning("'wrapper.Text' is null or empty!", this);
            }
            else
            {
               yield return null; //return to the main process (uid)

               string voiceName = getVoiceName(wrapper);
               int calculatedRate = calculateRate(wrapper.Rate);
               int calculatedVolume = calculateVolume(wrapper.Volume);

               silence = false;

               if (!isNative)
               {
                  onSpeakAudioGenerationStart(wrapper); //just a fake event if some code needs the feedback...

                  yield return null;

                  onSpeakAudioGenerationComplete(wrapper); //just a fake event if some code needs the feedback...
               }

               //Uni_Voice_Resume();
               //yield return null;

               onSpeakStart(wrapper);

               Uni_Voice_Volume(calculatedVolume);
               Uni_Voice_Rate(calculatedRate);

               //TEST
               //wrapper.ForceSSML = false;

               if (wrapper.ForceSSML && !Speaker.Instance.AutoClearTags)
               {
                  Uni_Voice_SpeakEX(prepareText(wrapper, voiceName), SPF_ASYNC | SPF_PURGEBEFORESPEAK | SPF_PARSE_SSML);
               }
               else
               {
                  Uni_Voice_SpeakEX("<voice required='Name=" + voiceName + "'>" + getValidXML(wrapper.Text), SPF_ASYNC | SPF_PURGEBEFORESPEAK | SPF_IS_XML);
               }

               yield return new WaitForSeconds(0.1f);

               do
               {
                  yield return null;
               } while (Uni_Voice_Status(0) == 2 && !silence);

               if (Util.Config.DEBUG)
                  Debug.Log("Text spoken: " + wrapper.Text, this);

               onSpeakComplete(wrapper);
            }
         }
#else
            yield return null;
#endif
      }

      private static string prepareText(Model.Wrapper wrapper, string voiceName)
      {
         if (wrapper.ForceSSML && !Speaker.Instance.AutoClearTags)
         {
            System.Text.StringBuilder sbXML = new System.Text.StringBuilder();

            sbXML.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            sbXML.Append("<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"");
            sbXML.Append(wrapper.Voice == null ? "en-US" : wrapper.Voice.Culture);
            sbXML.Append("\">");

            sbXML.Append("<voice name=\"" + voiceName + "\">");

            float _pitch = wrapper.Pitch - 1f;

            if (Mathf.Abs(_pitch) > Common.Util.BaseConstants.FLOAT_TOLERANCE)
            {
               sbXML.Append("<prosody pitch='");

               sbXML.Append(_pitch > 0f
                  ? _pitch.ToString("+#0%", Util.Helper.BaseCulture)
                  : _pitch.ToString("#0%", Util.Helper.BaseCulture));

               sbXML.Append("'>");
            }

            sbXML.Append(wrapper.Text);

            if (Mathf.Abs(_pitch) > Common.Util.BaseConstants.FLOAT_TOLERANCE)
               sbXML.Append("</prosody>");

            sbXML.Append("</voice>");
            sbXML.Append("</speak>");

            return getValidXML(sbXML.ToString());
         }

         return wrapper.Text;
      }

      private static int calculateVolume(float volume)
      {
         return Mathf.Clamp((int)(100 * volume), 0, 100);
      }

      private static int calculateRate(float rate)
      {
         //allowed range: 0 - 3f - all other values were cropped
         int result = 0;

         if (Mathf.Abs(rate - 1f) > Common.Util.BaseConstants.FLOAT_TOLERANCE)
         {
            //relevant?
            if (rate > 1f)
            {
               //larger than 1
               if (rate >= 2.75f)
               {
                  result = 10; //2.78
               }
               else if (rate >= 2.6f && rate < 2.75f)
               {
                  result = 9; //2.6
               }
               else if (rate >= 2.35f && rate < 2.6f)
               {
                  result = 8; //2.39
               }
               else if (rate >= 2.2f && rate < 2.35f)
               {
                  result = 7; //2.2
               }
               else if (rate >= 2f && rate < 2.2f)
               {
                  result = 6; //2
               }
               else if (rate >= 1.8f && rate < 2f)
               {
                  result = 5; //1.8
               }
               else if (rate >= 1.6f && rate < 1.8f)
               {
                  result = 4; //1.6
               }
               else if (rate >= 1.4f && rate < 1.6f)
               {
                  result = 3; //1.45
               }
               else if (rate >= 1.2f && rate < 1.4f)
               {
                  result = 2; //1.28
               }
               else if (rate > 1f && rate < 1.2f)
               {
                  result = 1; //1.14
               }
            }
            else
            {
               //smaller than 1
               if (rate <= 0.3f)
               {
                  result = -10; //0.33
               }
               else if (rate > 0.3 && rate <= 0.4f)
               {
                  result = -9; //0.375
               }
               else if (rate > 0.4 && rate <= 0.45f)
               {
                  result = -8; //0.42
               }
               else if (rate > 0.45 && rate <= 0.5f)
               {
                  result = -7; //0.47
               }
               else if (rate > 0.5 && rate <= 0.55f)
               {
                  result = -6; //0.525
               }
               else if (rate > 0.55 && rate <= 0.6f)
               {
                  result = -5; //0.585
               }
               else if (rate > 0.6 && rate <= 0.7f)
               {
                  result = -4; //0.655
               }
               else if (rate > 0.7 && rate <= 0.8f)
               {
                  result = -3; //0.732
               }
               else if (rate > 0.8 && rate <= 0.9f)
               {
                  result = -2; //0.82
               }
               else if (rate > 0.9 && rate < 1f)
               {
                  result = -1; //0.92
               }
            }
         }

         if (Util.Constants.DEV_DEBUG)
            Debug.Log("calculateRate: " + result + " - " + rate);

         return result;
      }

      #endregion


      #region Editor-only methods

#if UNITY_EDITOR
      public override void GenerateInEditor(Model.Wrapper wrapper)
      {
         Debug.LogError("'GenerateInEditor' is not supported for SAPI Unity!", this);
      }

      public override void SpeakNativeInEditor(Model.Wrapper wrapper)
      {
#if UNITY_EDITOR_WIN
         if (wrapper == null)
         {
            Debug.LogWarning("'wrapper' is null!", this);
         }
         else
         {
            if (string.IsNullOrEmpty(wrapper.Text))
            {
               Debug.LogWarning("'wrapper.Text' is null or empty!", this);
            }
            else
            {
               string voiceName = getVoiceName(wrapper);
               int calculatedRate = calculateRate(wrapper.Rate);
               int calculatedVolume = calculateVolume(wrapper.Volume);

               silence = false;

               onSpeakStart(wrapper);

               Uni_Voice_Volume(calculatedVolume);
               Uni_Voice_Rate(calculatedRate);
               Uni_Voice_Resume();
               Uni_Voice_SpeakEX("<voice required='Name=" + voiceName + "'>" + getValidXML(wrapper.Text), SPF_ASYNC | SPF_IS_XML | SPF_PURGEBEFORESPEAK);

               do
               {
                  System.Threading.Thread.Sleep(50);
               } while (Uni_Voice_Status(0) == 2 && !silence);

               if (Util.Config.DEBUG)
                  Debug.Log("Text spoken: " + wrapper.Text, this);

               onSpeakComplete(wrapper);
            }
         }
#endif
      }
#endif

      #endregion
   }
}
// © 2019-2021 crosstales LLC (https://www.crosstales.com)