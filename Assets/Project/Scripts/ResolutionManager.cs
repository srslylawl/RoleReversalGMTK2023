using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    private Vector2Int previousScreenSize;
    [SerializeField] private Vector2 targetAspectRatio = new Vector2(16, 9);
    
    private Camera cam;

    private void Start()
    {
        previousScreenSize = new Vector2Int(Screen.width, Screen.height);
        cam = Camera.main;
        UpdateAspectRatio();
    }

    private void Update()
    {
        // Check for changes in screen size
        if (Screen.width != previousScreenSize.x || Screen.height != previousScreenSize.y)
        {
            var newSize =  new Vector2Int(Screen.width, Screen.height);
            Debug.Log($"Window resized from: {previousScreenSize} to {newSize}");
            Screen.SetResolution(newSize.x, newSize.y, Screen.fullScreenMode);
            // if (_screenRenderTexture) {
            //     _screenRenderTexture.Release();
            //     _screenRenderTexture.width = newSize.x;
            //     _screenRenderTexture.height = newSize.y;
            //     _screenRenderTexture.Create();
            // }
            UpdateAspectRatio();
            previousScreenSize = newSize;
        }
    }
    
    private void UpdateAspectRatio() {
        float currentAspectRatio = (float)Screen.width / Screen.height;
        float scaleHeight = currentAspectRatio / (targetAspectRatio.x / targetAspectRatio.y);

        // Debug.Log($"ScaleHeight: {scaleHeight}");
        // Adjust camera's viewport rect to add black bars
        Rect rect = cam.rect;
        if (scaleHeight < 1.0f) {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else if (scaleHeight > 1.0f) {
            var scaleWidth = 1 / scaleHeight;
            rect.height = 1;
            rect.width = scaleWidth;
            rect.y = 0;
            rect.x = (1.0f - scaleWidth) / 2.0f;
        }
        else {
            // Reset camera's viewport rect if aspect ratio matches or exceeds target
            rect.x = 0;
            rect.y = 0;
            rect.width = 1.0f;
            rect.height = 1.0f;
        }

        cam.rect = rect;
    }
}
