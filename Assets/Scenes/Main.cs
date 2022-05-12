using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class Main : MonoBehaviour
{

    private AndroidJavaObject nativeObject;
    private int width, height;
    public MeshRenderer meshRenderer;
    private Texture2D texture;

    // Use this for initialization
    void Start()
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        nativeObject = new AndroidJavaObject("com.pvr.videoplugin.VideoPlugin", jo);
        width = 1600;
        height = 900;
        Debug.Log("VideoPlugin:" + width + ", " + height);
        Invoke(nameof(Init), 2.0f);
        Invoke(nameof(Release), 22.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (texture != null && nativeObject.Call<bool>("isUpdateFrame"))
        {
            Debug.Log("VideoPlugin:Update");
            nativeObject.Call("updateTexture");
            GL.InvalidateState();
        }
    }

    private void Init()
    {
        Debug.Log("VideoPlugin:Start");
        texture = new Texture2D(width, height, TextureFormat.RGB24, false, false);
        nativeObject.Call("start", (int)texture.GetNativeTexturePtr(), width, height);
        meshRenderer.material.mainTexture = texture;
    }

    private void Release()
    {
        Debug.Log("VideoPlugin:Release");
        texture = null;
        meshRenderer.material.mainTexture = null;
        nativeObject.Call("release");
    }

}
