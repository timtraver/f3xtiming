#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Crosstales.RTVoice.SAPI
{
   /// <summary>Custom editor for the 'VoiceProviderSAPI'-class.</summary>
   [CustomEditor(typeof(VoiceProviderSAPI))]
   public class VoiceProviderSAPIEditor : Editor
   {
      #region Variables

      private VoiceProviderSAPI script;

      #endregion


      #region Properties

      public static bool isPrefabInScene => GameObject.Find("SAPI Unity") != null;

      #endregion


      #region Editor methods

      private void OnEnable()
      {
         script = (VoiceProviderSAPI)target;
      }

      public override void OnInspectorGUI()
      {
         DrawDefaultInspector();

         //EditorHelper.SeparatorUI();

         if (script.isActiveAndEnabled)
         {
            if (!script.isPlatformSupported)
            {
               //EditorHelper.SeparatorUI();
               EditorGUILayout.HelpBox("The current platform is not supported by SAPI Unity. Please use MaryTTS or a custom provider (e.g. Klattersynth).", MessageType.Error);
            }
            /*
            else
            {
               //add stuff if needed
            }
            */
         }
         else
         {
            //EditorHelper.SeparatorUI();
            EditorGUILayout.HelpBox("Script is disabled!", MessageType.Info);
         }
      }

      #endregion
   }
}
#endif
// © 2019-2021 crosstales LLC (https://www.crosstales.com)