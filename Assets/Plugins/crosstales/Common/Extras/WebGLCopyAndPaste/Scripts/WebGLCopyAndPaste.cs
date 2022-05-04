using UnityEngine;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;

//#define WEBGL_COPY_AND_PASTE_SUPPORT_TEXTMESH_PRO //Uncomment this line if "TextMesh Pro" is used
namespace Crosstales.Internal
{
   /// <summary>
   /// Allows copy and paste in WebGL.
   /// 
   /// Based on https://github.com/greggman/unity-webgl-copy-and-paste
   /// </summary>
   public class WebGLCopyAndPaste : Common.Util.Singleton<WebGLCopyAndPaste>
   {
#if UNITY_WEBGL

      #region Initalizer

      [RuntimeInitializeOnLoadMethod]
      private static void setup()
      {
         PrefabPath = "Prefabs/WebGLCopyAndPaste";

         if (instance == null)
            CreateInstance();
      }

      #endregion

      private void Start()
      {
#if !UNITY_EDITOR //|| CT_DEVELOP
            WebGLCopyAndPasteAPI.Init(name, "GetClipboard", "ReceivePaste");
#endif
      }

      private void sendKey(string baseKey)
      {
         string appleKey = "%" + baseKey;
         string naturalKey = "^" + baseKey;

         GameObject currentObj = EventSystem.current.currentSelectedGameObject;
         if (currentObj == null)
            return;

         {
            UnityEngine.UI.InputField input = currentObj.GetComponent<UnityEngine.UI.InputField>();
            if (input != null)
            {
               // I don't know what's going on here. The code in InputField is looking for ctrl-c but that fails on Mac Chrome/Firefox
               input.ProcessEvent(Event.KeyboardEvent(naturalKey));
               input.ProcessEvent(Event.KeyboardEvent(appleKey));
               // so let's hope one of these is basically a noop
               return;
            }
         }
#if WEBGL_COPY_AND_PASTE_SUPPORT_TEXTMESH_PRO
         {
            TMPro.TMP_InputField input = currentObj.GetComponent<TMPro.TMP_InputField>();
            if (input != null)
            {
               // I don't know what's going on here. The code in InputField is looking for ctrl-c but that fails on Mac Chrome/Firefox
               // so let's hope one of these is basically a noop
               input.ProcessEvent(Event.KeyboardEvent(naturalKey));
               input.ProcessEvent(Event.KeyboardEvent(appleKey));
            }
         }
#endif
      }

      public void GetClipboard(string key)
      {
         sendKey(key);
#if !UNITY_EDITOR //|| CT_DEVELOP
         WebGLCopyAndPasteAPI.PassCopyToBrowser(GUIUtility.systemCopyBuffer);
#endif
      }

      public void ReceivePaste(string str)
      {
         GUIUtility.systemCopyBuffer = str;
      }
#endif
   }

#if UNITY_WEBGL && !UNITY_EDITOR //|| CT_DEVELOP
   public class WebGLCopyAndPasteAPI
   {
      [DllImport("__Internal")]
      private static extern void initWebGLCopyAndPaste(string objectName, string cutCopyCallbackFuncName, string pasteCallbackFuncName);

      [DllImport("__Internal")]
      private static extern void passCopyToBrowser(string str);

      static public void Init(string objectName, string cutCopyCallbackFuncName, string pasteCallbackFuncName)
      {
         initWebGLCopyAndPaste(objectName, cutCopyCallbackFuncName, pasteCallbackFuncName);
      }

      static public void PassCopyToBrowser(string str)
      {
         passCopyToBrowser(str);
      }
   }
#endif
}
// © 2021 crosstales LLC (https://www.crosstales.com)