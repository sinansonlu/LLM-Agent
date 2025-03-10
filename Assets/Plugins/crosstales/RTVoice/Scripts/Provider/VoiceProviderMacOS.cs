﻿#if UNITY_STANDALONE_OSX || UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Linq;

namespace Crosstales.RTVoice.Provider
{
   /// <summary>MacOS voice provider.</summary>
   public class VoiceProviderMacOS : BaseVoiceProvider<VoiceProviderMacOS>
   {
      #region Variables

      private static readonly System.Text.RegularExpressions.Regex sayRegex = new System.Text.RegularExpressions.Regex(@"^([^#]+?)\s*([^ ]+)\s*# (.*?)$");

      private const int defaultRate = 175;

#if ENABLE_IL2CPP && CT_PROC
      private System.Collections.Generic.Dictionary<string, Crosstales.Common.Util.CTProcess> processCreators = new System.Collections.Generic.Dictionary<string, Crosstales.Common.Util.CTProcess>();
#endif

      private bool isLoading;

      #endregion


      #region Properties

/*
      /// <summary>Returns the singleton instance of this class.</summary>
      /// <returns>Singleton instance of this class.</returns>
      public static VoiceProviderMacOS Instance => instance ?? (instance = new VoiceProviderMacOS());
*/
      public override string AudioFileExtension => ".wav";

      public override AudioType AudioFileType => AudioType.WAV;

      public override string DefaultVoiceName => "Daniel";

      public override bool isWorkingInEditor => Crosstales.RTVoice.Util.Helper.isMacOSEditor;

      public override bool isWorkingInPlaymode => Crosstales.RTVoice.Util.Helper.isMacOSEditor;

      public override int MaxTextLength => 256000;

      public override bool isSpeakNativeSupported => true;

      public override bool isSpeakSupported => true;

      public override bool isPlatformSupported => Crosstales.RTVoice.Util.Helper.isMacOSPlatform;

      public override bool isSSMLSupported => false;

      public override bool isOnlineService => false;

      public override bool hasCoRoutines => true;

      public override bool isIL2CPPSupported => true;

      public override bool hasVoicesInEditor => true;

      public override int MaxSimultaneousSpeeches => 50;

      #endregion


      #region Implemented methods

      public override void Load(bool forceReload = false)
      {
         if (cachedVoices?.Count == 0 || forceReload)
         {
            if (Crosstales.RTVoice.Util.Helper.isEditorMode)
            {
#if UNITY_EDITOR
               getVoicesInEditor();
#endif
            }
            else
            {
               if (!isLoading)
               {
                  isLoading = true;
                  Speaker.Instance.StartCoroutine(getVoices());
               }
            }
         }
         else
         {
            onVoicesReady();
         }
      }

      public override IEnumerator SpeakNative(Crosstales.RTVoice.Model.Wrapper wrapper)
      {
#if UNITY_STANDALONE || UNITY_EDITOR
         if (wrapper == null)
         {
            Debug.LogWarning("'wrapper' is null!");
         }
         else
         {
            if (string.IsNullOrEmpty(wrapper.Text))
            {
               Debug.LogWarning("'wrapper.Text' is null or empty: " + wrapper);
            }
            else
            {
               yield return null; //return to the main process (uid)

               string voiceName = getVoiceName(wrapper);
               int calculatedRate = calculateRate(wrapper.Rate);
#if ENABLE_IL2CPP && CT_PROC
               using (Crosstales.Common.Util.CTProcess process = new Crosstales.Common.Util.CTProcess())
#else
               using (System.Diagnostics.Process process = new System.Diagnostics.Process())
#endif
               {
                  string args = (string.IsNullOrEmpty(voiceName)
                                   ? string.Empty
                                   : " -v \"" + voiceName.Replace('"', '\'') + '"') +
                                (calculatedRate != defaultRate ? " -r " + calculatedRate : string.Empty) + " \"" +
                                wrapper.Text.Replace('"', '\'') + '"';

                  if (Crosstales.RTVoice.Util.Config.DEBUG)
                     Debug.Log("Process arguments: " + args);

                  process.StartInfo.FileName = Crosstales.RTVoice.Util.Config.TTS_MACOS;
                  process.StartInfo.Arguments = args;

                  System.Threading.Thread worker = new System.Threading.Thread(() => startProcess(process, 0, false, false, false)) { Name = wrapper.Uid };
                  worker.Start();

                  silence = false;
#if ENABLE_IL2CPP
                  processCreators.Add(wrapper.Uid, process);
#else
                  processes.Add(wrapper.Uid, process);
#endif
                  onSpeakStart(wrapper);

                  do
                  {
                     yield return null;
                  } while (worker.IsAlive || !process.HasExited);
#if ENABLE_IL2CPP
                  if (process.ExitCode == 0 || process.ExitCode == 123456) //123456 = Killed
#else
                  if (process.ExitCode == 0 || process.ExitCode == -1 || process.ExitCode == 137) //0 = normal ended, -1/137 = killed
#endif
                  {
                     if (Crosstales.RTVoice.Util.Config.DEBUG)
                        Debug.Log("Text spoken: " + wrapper.Text);

                     onSpeakComplete(wrapper);
                  }
                  else
                  {
                     using (System.IO.StreamReader sr = process.StandardError)
                     {
                        string errorMessage = "Could not speak the text: " + wrapper + System.Environment.NewLine +
                                              "Exit code: " + process.ExitCode + System.Environment.NewLine +
                                              sr.ReadToEnd();
                        Debug.LogError(errorMessage);
                        onErrorInfo(wrapper, errorMessage);
                     }
                  }
#if ENABLE_IL2CPP
                  processCreators.Remove(wrapper.Uid);
#else
                  processes.Remove(wrapper.Uid);
#endif
               }
            }
         }
#else
         yield return null;
#endif
      }

      public override IEnumerator Speak(Crosstales.RTVoice.Model.Wrapper wrapper)
      {
#if UNITY_STANDALONE || UNITY_EDITOR
         if (wrapper == null)
         {
            Debug.LogWarning("'wrapper' is null!");
         }
         else
         {
            if (string.IsNullOrEmpty(wrapper.Text))
            {
               Debug.LogWarning("'wrapper.Text' is null or empty: " + wrapper);
            }
            else
            {
               if (wrapper.Source == null)
               {
                  Debug.LogWarning("'wrapper.Source' is null: " + wrapper);
               }
               else
               {
                  yield return null; //return to the main process (uid)

                  string voiceName = getVoiceName(wrapper);
                  int calculatedRate = calculateRate(wrapper.Rate);
                  string outputFile = getOutputFile(wrapper.Uid);
#if ENABLE_IL2CPP && CT_PROC
                  using (Crosstales.Common.Util.CTProcess process = new Crosstales.Common.Util.CTProcess())
#else
                  using (System.Diagnostics.Process process = new System.Diagnostics.Process())
#endif
                  {
                     string args = (string.IsNullOrEmpty(voiceName)
                                      ? string.Empty
                                      : $" -v \"{voiceName.Replace('"', '\'')}\"") +
                                   (calculatedRate != defaultRate ? $" -r {calculatedRate}" : string.Empty) +
                                   $" -o \"{outputFile.Replace('"', '\'')}\"" +
                                   //" --file-format=AIFFLE"
                                   " --file-format=WAVE" +
                                   " --data-format=LEI16@22050" +
                                   $" \"{wrapper.Text.Replace('"', '\'')}\"";

                     if (Crosstales.RTVoice.Util.Config.DEBUG)
                        Debug.Log("Process arguments: " + args);

                     process.StartInfo.FileName = Crosstales.RTVoice.Util.Config.TTS_MACOS;
                     process.StartInfo.Arguments = args;

                     System.Threading.Thread worker = new System.Threading.Thread(() => startProcess(process, 0, false, false, false)) { Name = wrapper.Uid };
                     worker.Start();

                     silence = false;
#if ENABLE_IL2CPP
                     processCreators.Add(wrapper.Uid, process);
#else
                     processes.Add(wrapper.Uid, process);
#endif
                     onSpeakAudioGenerationStart(wrapper);

                     do
                     {
                        yield return null;
                     } while (worker.IsAlive || !process.HasExited);

                     if (process.ExitCode == 0)
                     {
                        yield return playAudioFile(wrapper, Crosstales.Common.Util.NetworkHelper.GetURLFromFile(outputFile), outputFile, AudioFileType);
                     }
                     else
                     {
                        using (System.IO.StreamReader sr = process.StandardError)
                        {
                           string errorMessage = "Could not speak the text: " + wrapper +
                                                 System.Environment.NewLine + "Exit code: " + process.ExitCode +
                                                 System.Environment.NewLine + sr.ReadToEnd();
                           Debug.LogError(errorMessage);
                           onErrorInfo(wrapper, errorMessage);
                        }
                     }
#if ENABLE_IL2CPP
                     processCreators.Remove(wrapper.Uid);
#else
                     processes.Remove(wrapper.Uid);
#endif
                  }
               }
            }
         }
#else
         yield return null;
#endif
      }

      public override IEnumerator Generate(Crosstales.RTVoice.Model.Wrapper wrapper)
      {
#if UNITY_STANDALONE || UNITY_EDITOR
         if (wrapper == null)
         {
            Debug.LogWarning("'wrapper' is null!");
         }

         else
         {
            if (string.IsNullOrEmpty(wrapper.Text))
            {
               Debug.LogWarning("'wrapper.Text' is null or empty: " + wrapper);
            }
            else
            {
               yield return null; //return to the main process (uid)

               string voiceName = getVoiceName(wrapper);
               int calculatedRate = calculateRate(wrapper.Rate);
               string outputFile = getOutputFile(wrapper.Uid);
#if ENABLE_IL2CPP && CT_PROC
               using (Crosstales.Common.Util.CTProcess process = new Crosstales.Common.Util.CTProcess())
#else
               using (System.Diagnostics.Process process = new System.Diagnostics.Process())
#endif
               {
                  string args = (string.IsNullOrEmpty(voiceName)
                                   ? string.Empty
                                   : $" -v \"{voiceName.Replace('"', '\'')}\"") +
                                (calculatedRate != defaultRate ? $" -r {calculatedRate}" : string.Empty) +
                                $" -o \"{outputFile.Replace('"', '\'')}\"" +
                                //" --file-format=AIFFLE"
                                " --file-format=WAVE" +
                                " --data-format=LEI16@22050" +
                                $" \"{wrapper.Text.Replace('"', '\'')}\"";

                  if (Crosstales.RTVoice.Util.Config.DEBUG)
                     Debug.Log("Process arguments: " + args);

                  process.StartInfo.FileName = Crosstales.RTVoice.Util.Config.TTS_MACOS;
                  process.StartInfo.Arguments = args;

                  System.Threading.Thread worker = new System.Threading.Thread(() => startProcess(process, 0, false, false, false)) { Name = wrapper.Uid };
                  worker.Start();

                  silence = false;
#if ENABLE_IL2CPP
                  processCreators.Add(wrapper.Uid, process);
#else
                  processes.Add(wrapper.Uid, process);
#endif
                  onSpeakAudioGenerationStart(wrapper);

                  do
                  {
                     yield return null;
                  } while (worker.IsAlive || !process.HasExited);

                  if (process.ExitCode == 0)
                  {
                     processAudioFile(wrapper, outputFile);
                  }
                  else
                  {
                     using (System.IO.StreamReader sr = process.StandardError)
                     {
                        string errorMessage = "Could not generate the text: " + wrapper +
                                              System.Environment.NewLine + "Exit code: " + process.ExitCode +
                                              System.Environment.NewLine + sr.ReadToEnd();
                        Debug.LogError(errorMessage);
                        onErrorInfo(wrapper, errorMessage);
                     }
                  }
#if ENABLE_IL2CPP
                  processCreators.Remove(wrapper.Uid);
#else
                  processes.Remove(wrapper.Uid);
#endif
               }
            }
         }
#else
         yield return null;
#endif
      }

      public override void Silence()
      {
         base.Silence();
#if ENABLE_IL2CPP
         foreach (var kvp in processCreators.Where(kvp => kvp.Value.isBusy))
         {
             kvp.Value.Kill();
         }

         processCreators.Clear();
#endif
      }

      public override void Silence(string uid)
      {
         base.Silence(uid);
#if ENABLE_IL2CPP
         if (!string.IsNullOrEmpty(uid))
         {
             if (processCreators.ContainsKey(uid))
             {
                 if (processCreators[uid].isBusy)
                     processCreators[uid].Kill();

                 processCreators.Remove(uid);
             }
         }
#endif
      }

      #endregion


      #region Private methods

      private IEnumerator getVoices()
      {
#if UNITY_STANDALONE || UNITY_EDITOR
#if ENABLE_IL2CPP && CT_PROC
         using (Crosstales.Common.Util.CTProcess process = new Crosstales.Common.Util.CTProcess())
#else
         using (System.Diagnostics.Process process = new System.Diagnostics.Process())
#endif
         {
            process.StartInfo.FileName = Crosstales.RTVoice.Util.Config.TTS_MACOS;
            process.StartInfo.Arguments = "-v ?";
            process.Start();

            System.Threading.Thread worker = new System.Threading.Thread(() => startProcess(process, Crosstales.RTVoice.Util.Constants.DEFAULT_TTS_KILL_TIME));
            worker.Start();

            do
            {
               yield return null;
            } while (worker.IsAlive || !process.HasExited);

            if (process.ExitCode == 0)
            {
               System.Collections.Generic.List<Crosstales.RTVoice.Model.Voice> voices =
                  new System.Collections.Generic.List<Crosstales.RTVoice.Model.Voice>(200);

               using (System.IO.StreamReader streamReader = process.StandardOutput)
               {
                  while (!streamReader.EndOfStream)
                  {
                     string reply = streamReader.ReadLine();

                     if (!string.IsNullOrEmpty(reply))
                     {
                        System.Text.RegularExpressions.Match match = sayRegex.Match(reply);

                        if (match.Success)
                        {
                           string name = match.Groups[1].ToString();
                           voices.Add(new Crosstales.RTVoice.Model.Voice(name, match.Groups[3].ToString(),
                              Crosstales.RTVoice.Util.Helper.AppleVoiceNameToGender(name), Crosstales.RTVoice.Util.Constants.VOICE_AGE_UNKNOWN,
                              match.Groups[2].ToString(), "", "Apple"));
                        }
                     }
                  }
               }

               cachedVoices = voices.OrderBy(s => s.Name).ToList();

               if (Crosstales.RTVoice.Util.Constants.DEV_DEBUG)
                  Debug.Log("Voices read: " + cachedVoices.CTDump());
            }
            else
            {
               using (System.IO.StreamReader sr = process.StandardError)
               {
                  string errorMessage = "Could not get any voices: " + process.ExitCode + System.Environment.NewLine +
                                        sr.ReadToEnd();
                  Debug.LogError(errorMessage);
                  onErrorInfo(null, errorMessage);
               }
            }
         }

         isLoading = false;

         onVoicesReady();
#else
         yield return null;
#endif
      }

      private static int calculateRate(float rate)
      {
         int result =
            Mathf.Clamp(
               Mathf.Abs(rate - 1f) > Crosstales.Common.Util.BaseConstants.FLOAT_TOLERANCE
                  ? (int)(defaultRate * rate)
                  : defaultRate, 1, 3 * defaultRate);
         if (Crosstales.RTVoice.Util.Constants.DEV_DEBUG)
            Debug.Log("calculateRate: " + result + " - " + rate);
         return result;
      }

      #endregion


      #region Editor-only methods

#if UNITY_EDITOR
      public override void GenerateInEditor(Crosstales.RTVoice.Model.Wrapper wrapper)
      {
         if (wrapper == null)
         {
            Debug.LogWarning("'wrapper' is null!");
         }

         else
         {
            if (string.IsNullOrEmpty(wrapper.Text))
            {
               Debug.LogWarning("'wrapper.Text' is null or empty: " + wrapper);
            }
            else
            {
               string voiceName = getVoiceName(wrapper);
               int calculatedRate = calculateRate(wrapper.Rate);
               string outputFile = getOutputFile(wrapper.Uid);
#if ENABLE_IL2CPP && CT_PROC
               using (Crosstales.Common.Util.CTProcess process = new Crosstales.Common.Util.CTProcess())
#else
               using (System.Diagnostics.Process process = new System.Diagnostics.Process())
#endif
               {
                  string args = (string.IsNullOrEmpty(voiceName)
                                   ? string.Empty
                                   : $" -v \"{voiceName.Replace('"', '\'')}\"") +
                                (calculatedRate != defaultRate ? $" -r {calculatedRate}" : string.Empty) +
                                $" -o \"{outputFile.Replace('"', '\'')}\"" +
                                //" --file-format=AIFFLE"
                                " --file-format=WAVE" +
                                " --data-format=LEI16@22050" +
                                $" \"{wrapper.Text.Replace('"', '\'')}\"";

                  if (Crosstales.RTVoice.Util.Config.DEBUG)
                     Debug.Log("Process arguments: " + args);

                  process.StartInfo.FileName = Crosstales.RTVoice.Util.Config.TTS_MACOS;
                  process.StartInfo.Arguments = args;

                  System.Threading.Thread worker = new System.Threading.Thread(() => startProcess(process, 0, false, false, false)) { Name = wrapper.Uid };
                  worker.Start();

                  silence = false;
                  onSpeakAudioGenerationStart(wrapper);

                  do
                  {
                     System.Threading.Thread.Sleep(50);
                  } while (worker.IsAlive || !process.HasExited);

                  if (process.ExitCode == 0)
                  {
                     processAudioFile(wrapper, outputFile);
                  }
                  else
                  {
                     using (System.IO.StreamReader sr = process.StandardError)
                     {
                        string errorMessage = "Could not generate the text: " + wrapper +
                                              System.Environment.NewLine + "Exit code: " + process.ExitCode +
                                              System.Environment.NewLine + sr.ReadToEnd();
                        Debug.LogError(errorMessage);
                        onErrorInfo(wrapper, errorMessage);
                     }
                  }
               }
            }
         }
      }

      public override void SpeakNativeInEditor(Crosstales.RTVoice.Model.Wrapper wrapper)
      {
         if (wrapper == null)
         {
            Debug.LogWarning("'wrapper' is null!");
         }

         else
         {
            if (string.IsNullOrEmpty(wrapper.Text))
            {
               Debug.LogWarning("'wrapper.Text' is null or empty: " + wrapper);
            }
            else
            {
               string voiceName = getVoiceName(wrapper);
               int calculatedRate = calculateRate(wrapper.Rate);
#if ENABLE_IL2CPP && CT_PROC
               using (Crosstales.Common.Util.CTProcess process = new Crosstales.Common.Util.CTProcess())
#else
               using (System.Diagnostics.Process process = new System.Diagnostics.Process())
#endif
               {
                  string args = (string.IsNullOrEmpty(voiceName)
                                   ? string.Empty
                                   : " -v \"" + voiceName.Replace('"', '\'') + '"') +
                                (calculatedRate != defaultRate ? " -r " + calculatedRate : string.Empty) + " \"" +
                                wrapper.Text.Replace('"', '\'') + '"';

                  if (Crosstales.RTVoice.Util.Config.DEBUG)
                     Debug.Log("Process arguments: " + args);

                  process.StartInfo.FileName = Crosstales.RTVoice.Util.Config.TTS_MACOS;
                  process.StartInfo.Arguments = args;

                  System.Threading.Thread worker = new System.Threading.Thread(() => startProcess(process, 0, false, false, false)) { Name = wrapper.Uid };
                  worker.Start();

                  silence = false;
                  onSpeakStart(wrapper);

                  do
                  {
                     System.Threading.Thread.Sleep(50);

                     if (silence && !process.HasExited)
                     {
                        process.Kill();
                     }
                  } while (worker.IsAlive || !process.HasExited);

#if ENABLE_IL2CPP
               if (process.ExitCode == 0 || process.ExitCode == 123456) //123456 = Killed
#else
                  if (process.ExitCode == 0 || process.ExitCode == -1 || process.ExitCode == 137
                     ) //0 = normal ended, -1/137 = killed
#endif
                  {
                     if (Crosstales.RTVoice.Util.Config.DEBUG)
                        Debug.Log("Text spoken: " + wrapper.Text);

                     onSpeakComplete(wrapper);
                  }
                  else
                  {
                     using (System.IO.StreamReader sr = process.StandardError)
                     {
                        string errorMessage = "Could not speak the text: " + wrapper + System.Environment.NewLine +
                                              "Exit code: " + process.ExitCode + System.Environment.NewLine +
                                              sr.ReadToEnd();
                        Debug.LogError(errorMessage);
                        onErrorInfo(wrapper, errorMessage);
                     }
                  }
               }
            }
         }
      }

      private void getVoicesInEditor()
      {
#if ENABLE_IL2CPP && CT_PROC
         using (Crosstales.Common.Util.CTProcess process = new Crosstales.Common.Util.CTProcess())
#else
         using (System.Diagnostics.Process process = new System.Diagnostics.Process())
#endif
         {
            process.StartInfo.FileName = Crosstales.RTVoice.Util.Config.TTS_MACOS;
            process.StartInfo.Arguments = "-v ?";

            try
            {
               System.Threading.Thread voiceWorker = new System.Threading.Thread(() => startProcess(process, Crosstales.RTVoice.Util.Constants.DEFAULT_TTS_KILL_TIME));
               voiceWorker.Start();

               do
               {
                  System.Threading.Thread.Sleep(50);
               } while (voiceWorker.IsAlive || !process.HasExited);

               if (Crosstales.RTVoice.Util.Constants.DEV_DEBUG)
                  Debug.Log("Finished after: " + (process.ExitTime - process.StartTime).Seconds);

               if (process.ExitCode == 0)
               {
                  System.Collections.Generic.List<Crosstales.RTVoice.Model.Voice> voices =
                     new System.Collections.Generic.List<Crosstales.RTVoice.Model.Voice>(200);

                  using (System.IO.StreamReader streamReader = process.StandardOutput)
                  {
                     while (!streamReader.EndOfStream)
                     {
                        string reply = streamReader.ReadLine();

                        if (!string.IsNullOrEmpty(reply))
                        {
                           System.Text.RegularExpressions.Match match = sayRegex.Match(reply);

                           if (match.Success)
                           {
                              string name = match.Groups[1].ToString();
                              voices.Add(new Crosstales.RTVoice.Model.Voice(match.Groups[1].ToString(), match.Groups[3].ToString(),
                                 Crosstales.RTVoice.Util.Helper.AppleVoiceNameToGender(name), Crosstales.RTVoice.Util.Constants.VOICE_AGE_UNKNOWN,
                                 match.Groups[2].ToString(), "", "Apple"));
                           }
                        }
                     }
                  }

                  cachedVoices = voices.OrderBy(s => s.Name).ToList();

                  if (Crosstales.RTVoice.Util.Constants.DEV_DEBUG)
                     Debug.Log("Voices read: " + cachedVoices.CTDump());
               }
               else
               {
                  using (System.IO.StreamReader sr = process.StandardError)
                  {
                     string errorMessage = "Could not get any voices: " + process.ExitCode +
                                           System.Environment.NewLine + sr.ReadToEnd();
                     Debug.LogError(errorMessage);
                  }
               }
            }
            catch (System.Exception ex)
            {
               string errorMessage = "Could not get any voices!" + System.Environment.NewLine + ex;
               Debug.LogError(errorMessage);
            }
         }

         onVoicesReady();
      }
#endif

      #endregion
   }
}
#endif
// © 2015-2023 crosstales LLC (https://www.crosstales.com)