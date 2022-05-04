#if UNITY_EDITOR
using UnityEditor;
using Crosstales.RTVoice.EditorUtil;

namespace Crosstales.RTVoice.SAPI
{
   /// <summary>Editor component for for adding the prefabs from 'SAPI Unity' in the "Tools"-menu.</summary>
   public static class VoiceProviderSAPIMenu
   {
      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Prefabs/VoiceProviderSAPI", false, EditorUtil.EditorHelper.MENU_ID + 210)]
      private static void AddVoiceProvider()
      {
         EditorHelper.InstantiatePrefab("SAPI Unity", $"{EditorConfig.ASSET_PATH}Extras/SAPI Unity/Resources/Prefabs/");
      }

      [MenuItem("Tools/" + Util.Constants.ASSET_NAME + "/Prefabs/VoiceProviderSAPI", true)]
      private static bool AddVoiceProviderValidator()
      {
         return !VoiceProviderSAPIEditor.isPrefabInScene;
      }
   }
}
#endif
// © 2019-2021 crosstales LLC (https://www.crosstales.com)