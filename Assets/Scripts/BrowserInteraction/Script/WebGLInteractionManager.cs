using UnityEngine;

public class WebGLInteractionManager : MonoBehaviour
{
      public void OnadsTrigger()
    {
          #if UNITY_WEBGL && !UNITY_EDITOR
            // Call the JavaScript function from Unity
            Application.ExternalCall("adsTrigger");
        #endif
    }

}