﻿#if UNITY_EDITOR && !UNITY_CLOUD_BUILD
using UnityEditor;
using Enumerable = System.Linq.Enumerable;

namespace Crosstales.RTVoice.EditorTask
{
   /// <summary>Show the configuration window on the first launch.</summary>
   public class Launch : AssetPostprocessor
   {
      public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
      {
         if (Enumerable.Any(importedAssets, str => str.Contains(Crosstales.RTVoice.EditorUtil.EditorConstants.ASSET_UID.ToString())))
         {
            Crosstales.Common.EditorTask.SetupResources.Setup();
            SetupResources.Setup();

            Crosstales.RTVoice.EditorIntegration.ConfigWindow.ShowWindow(4);
         }
      }
   }
}
#endif
// © 2017-2023 crosstales LLC (https://www.crosstales.com)