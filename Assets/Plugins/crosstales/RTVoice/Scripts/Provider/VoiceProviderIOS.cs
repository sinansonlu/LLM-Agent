﻿#if UNITY_IOS || UNITY_TVOS || UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Linq;

namespace Crosstales.RTVoice.Provider
{
   /// <summary>iOS voice provider.</summary>
   public class VoiceProviderIOS : BaseVoiceProvider<VoiceProviderIOS>
   {
      #region Variables

      private static System.Collections.Generic.List<Crosstales.RTVoice.Model.Voice> cachediOSVoices = new System.Collections.Generic.List<Crosstales.RTVoice.Model.Voice>();

      private static bool isWorking;
      private static Crosstales.RTVoice.Model.Wrapper wrapperNative;
      private static bool isPaused;
      private static string[] speechTextArray;
      private static int wordIndex;
      private static bool isLoading;

      #endregion


      #region Properties

/*
      /// <summary>Returns the singleton instance of this class.</summary>
      /// <returns>Singleton instance of this class.</returns>
      public static VoiceProviderIOS Instance => instance ?? (instance = new VoiceProviderIOS());
*/
      public override string AudioFileExtension => "none";

      public override AudioType AudioFileType => AudioType.UNKNOWN;

      public override string DefaultVoiceName => "Daniel";

      public override System.Collections.Generic.List<Crosstales.RTVoice.Model.Voice> Voices => cachediOSVoices;

      public override bool isWorkingInEditor => false;

      public override bool isWorkingInPlaymode => false;

      public override int MaxTextLength => 256000;

      public override bool isSpeakNativeSupported => true;

      public override bool isSpeakSupported => false;

      public override bool isPlatformSupported => Crosstales.RTVoice.Util.Helper.isIOSBasedPlatform;

      public override bool isSSMLSupported => false;

      public override bool isOnlineService => false;

      public override bool hasCoRoutines => true;

      public override bool isIL2CPPSupported => true;

      public override bool hasVoicesInEditor => false;

      public override int MaxSimultaneousSpeeches => 1;

      #endregion


      #region Wrapper callbacks

#if !UNITY_EDITOR || CT_DEVELOP
      /// <summary>Receives all voices</summary>
      /// <param name="voicesText">All voices as text string.</param>
      public static void SetVoices(string voicesText)
      {
         string[] voices = voicesText.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

         if (voices.Length % 3 == 0)
         {
            System.Collections.Generic.List<Crosstales.RTVoice.Model.Voice> voicesList = new System.Collections.Generic.List<Crosstales.RTVoice.Model.Voice>(200);

            //for (int ii = 0; ii < voices.Length; ii += 2)
            for (int ii = 0; ii < voices.Length; ii += 3)
            {
               string name = voices[ii + 1];
               string culture = voices[ii + 2];
               Crosstales.RTVoice.Model.Voice newVoice = new Crosstales.RTVoice.Model.Voice(name, "iOS voice: " + name + " " + culture, Crosstales.RTVoice.Util.Helper.AppleVoiceNameToGender(name), Crosstales.RTVoice.Util.Constants.VOICE_AGE_UNKNOWN, culture, voices[ii], "Apple");

               voicesList.Add(newVoice);
            }

            cachediOSVoices = voicesList.OrderBy(s => s.Name).ToList();

            if (Crosstales.RTVoice.Util.Constants.DEV_DEBUG)
               Debug.Log("Voices read: " + cachediOSVoices.CTDump());
         }
         else
         {
            Debug.LogWarning($"Voice-string contains wrong number of elements: {voices.Length}");
         }

         isLoading = false;
         Instance.onVoicesReady();

         //NativeMethods.FreeMemory();
      }

      /// <summary>Receives the state of the speaker.</summary>
      /// <param name="state">The state of the speaker.</param>
      public static void SetState(string state)
      {
         switch (state)
         {
            case "Start":
               // do nothing
               break;
            case "Finish":
               isWorking = false;
               break;
            default:
               //cancel
               isWorking = false;
               break;
         }
      }

      /// <summary>Called every time a new word is spoken.</summary>
      public static void WordSpoken(string word)
      {
         if (Crosstales.RTVoice.Util.Constants.DEV_DEBUG)
            Debug.Log($"Word spoken: {word}");

         if (wrapperNative != null)
         {
            Instance.onSpeakCurrentWord(wrapperNative, word);

            Instance.onSpeakCurrentWord(wrapperNative, speechTextArray, wordIndex);
            wordIndex++;
         }
      }
#endif

      #endregion


      #region Implemented methods

      public override void Load(bool forceReload = false)
      {
#if !UNITY_EDITOR || CT_DEVELOP
         if (cachediOSVoices?.Count == 0 || forceReload)
         {
            if (!isLoading)
            {
               isLoading = true;
               NativeMethods.RTVGetVoices();
            }
         }
         else
         {
            onVoicesReady();
         }
#endif
      }

      public override IEnumerator SpeakNative(Crosstales.RTVoice.Model.Wrapper wrapper)
      {
         yield return speak(wrapper, true);
      }

      public override IEnumerator Speak(Crosstales.RTVoice.Model.Wrapper wrapper)
      {
         yield return speak(wrapper, false);
      }

      public override IEnumerator Generate(Crosstales.RTVoice.Model.Wrapper wrapper)
      {
         Debug.LogError("'Generate' is not supported for iOS!");
         yield return null;
      }

      public override void Silence()
      {
#if !UNITY_EDITOR || CT_DEVELOP
         NativeMethods.RTVStop();
         //NativeMethods.FreeMemory();
#endif

         base.Silence();
      }

      public override void Silence(string uid)
      {
         Silence();

         base.Silence(uid);
      }

      public void Pause()
      {
#if !UNITY_EDITOR || CT_DEVELOP
         isPaused = true;
#endif
         Silence();
      }

      #endregion


      #region Private methods

      private IEnumerator speak(Crosstales.RTVoice.Model.Wrapper wrapper, bool isNative)
      {
#if !UNITY_EDITOR || CT_DEVELOP
         if (wrapper == null)
         {
            Debug.LogWarning("'wrapper' is null!");
         }
         else
         {
            if (string.IsNullOrEmpty(wrapper.Text))
            {
               Debug.LogWarning("'wrapper.Text' is null or empty!");
            }
            else
            {
               yield return null; //return to the main process (uid)

               string voiceId = getVoiceId(wrapper);

               isPaused = false;
               silence = false;

               if (!isNative && !wrapper.isPartial)
               {
                  onSpeakAudioGenerationStart(wrapper); //just a fake event if some code needs the feedback...

                  yield return null;

                  onSpeakAudioGenerationComplete(wrapper); //just a fake event if some code needs the feedback...
               }

               if (!wrapper.isPartial)
                  onSpeakStart(wrapper);

               isWorking = true;

               speechTextArray = Crosstales.RTVoice.Util.Helper.CleanText(wrapper.Text, false).Split(splitCharWords, System.StringSplitOptions.RemoveEmptyEntries);
               wordIndex = 0;
               wrapperNative = wrapper;

               NativeMethods.RTVSpeak(voiceId, wrapper.Text, calculateRate(wrapper.Rate), wrapper.Pitch, wrapper.Volume);

               do
               {
                  yield return null;
               } while (isWorking && !silence);

               if (Crosstales.RTVoice.Util.Config.DEBUG)
                  Debug.Log("Text spoken: " + wrapper.Text);

               wrapperNative = null;

               if (!isPaused)
                  onSpeakComplete(wrapper);

               //NativeMethods.FreeMemory();
            }
         }
#else
            yield return null;
#endif
      }

      private static float calculateRate(float rate)
      {
         float result = rate;

         if (rate > 1f)
         {
            //result = (rate + 1f) * 0.5f;
            result = 1f + (rate - 1f) * 0.25f;
         }

         if (Crosstales.RTVoice.Util.Constants.DEV_DEBUG)
            Debug.Log("calculateRate: " + result + " - " + rate);

         return result;
      }

      private string getVoiceId(Crosstales.RTVoice.Model.Wrapper wrapper)
      {
         if (wrapper != null && string.IsNullOrEmpty(wrapper.Voice?.Identifier))
         {
            if (Crosstales.RTVoice.Util.Config.DEBUG)
               Debug.LogWarning("'wrapper.Voice' or 'wrapper.Voice.Identifier' is null! Using the OS 'default' voice.");

            return Speaker.Instance.VoiceForName(DefaultVoiceName)?.Identifier;
         }

         return wrapper != null ? wrapper.Voice?.Identifier : Speaker.Instance.VoiceForName(DefaultVoiceName)?.Identifier;
      }

      #endregion


      #region Editor-only methods

#if UNITY_EDITOR

      public override void GenerateInEditor(Crosstales.RTVoice.Model.Wrapper wrapper)
      {
         Debug.LogError("'GenerateInEditor' is not supported for iOS!");
      }

      public override void SpeakNativeInEditor(Crosstales.RTVoice.Model.Wrapper wrapper)
      {
         Debug.LogError("'SpeakNativeInEditor' is not supported for iOS!");
      }
#endif

      #endregion
   }

   /// <summary>Native methods (bridge to iOS).</summary>
   internal static class NativeMethods
   {
      /*
      /// <summary>Bridge to the native tts system</summary>
      /// <param name="name">Name of the voice to speak.</param>
      /// <param name="text">Text to speak.</param>
      /// <param name="rate">Speech rate of the speaker in percent (default: 1, optional).</param>
      /// <param name="pitch">Pitch of the speech in percent (default: 1, optional).</param>
      /// <param name="volume">Volume of the speaker in percent (default: 1, optional).</param>
      [System.Runtime.InteropServices.DllImport("__Internal")]
      internal static extern Speak(string name, string text, float rate = 1f, float pitch = 1f, float volume = 1f);
      */

      /// <summary>Bridge to the native tts system</summary>
      /// <param name="id">Identifier of the voice to speak.</param>
      /// <param name="text">Text to speak.</param>
      /// <param name="rate">Speech rate of the speaker in percent (default: 1, optional).</param>
      /// <param name="pitch">Pitch of the speech in percent (default: 1, optional).</param>
      /// <param name="volume">Volume of the speaker in percent (default: 1, optional).</param>
      [System.Runtime.InteropServices.DllImport("__Internal")]
      internal static extern void RTVSpeak(string id, string text, float rate = 1f, float pitch = 1f, float volume = 1f);

      /// <summary>Silence the current TTS-provider.</summary>
      [System.Runtime.InteropServices.DllImport("__Internal")]
      internal static extern void RTVGetVoices();

      /// <summary>Silence the current TTS-provider.</summary>
      [System.Runtime.InteropServices.DllImport("__Internal")]
      internal static extern void RTVStop();
   }
}
#endif
// © 2016-2023 crosstales LLC (https://www.crosstales.com)