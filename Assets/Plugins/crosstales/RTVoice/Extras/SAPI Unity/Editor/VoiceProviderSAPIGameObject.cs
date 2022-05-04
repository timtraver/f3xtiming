#if UNITY_EDITOR
using UnityEditor;
using Crosstales.RTVoice.EditorUtil;

namespace Crosstales.RTVoice.SAPI
{
   /// <summary>Editor component for for adding the prefabs from 'SAPI Unity' in the "Hierarchy"-menu.</summary>
   public static class VoiceProviderSAPIGameObject
   {
      [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/VoiceProviderSAPI", false, EditorUtil.EditorHelper.GO_ID + 15)]
      private static void AddVoiceProvider()
      {
         EditorHelper.InstantiatePrefab("SAPI Unity", $"{EditorConfig.ASSET_PATH}Extras/SAPI Unity/Resources/Prefabs/");
      }

      [MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/VoiceProviderSAPI", true)]
      private static bool AddVoiceProviderValidator()
      {
         return !VoiceProviderSAPIEditor.isPrefabInScene;
      }
   }
}
#endif
// © 2019-2021 crosstales LLC (https://www.crosstales.com)