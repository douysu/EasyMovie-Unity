using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using AOT;
using System.Runtime.InteropServices;

public class Main : MonoBehaviour
{

    private AndroidJavaObject nativeObject;
    private int width, height;
    public MeshRenderer meshRenderer;
    private Texture2D texture;
    private delegate void RenderEventDelegate(int eventID);
    private RenderEventDelegate RenderThreadHandle;
    private IntPtr RenderThreadHandlePtr;

    void Awake()
    {
        RenderThreadHandle = new RenderEventDelegate(RunOnRenderThread);
        RenderThreadHandlePtr = Marshal.GetFunctionPointerForDelegate(RenderThreadHandle);
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("OnStart");
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        nativeObject = new AndroidJavaObject("com.pvr.videoplugin.VideoPlugin", jo);
        width = 1600;
        height = 900;
        Debug.Log("VideoPlugin:" + width + ", " + height);

        nativeObject.Call("initObject");

        start(0, width, height);
        updateTexture();
        release();

        if (SystemInfo.graphicsMultiThreaded)
        {
            GL.IssuePluginEvent(RenderThreadHandlePtr, GL_INIT_EVENT);
        }
        else
        {
            RunOnRenderThread(GL_INIT_EVENT);
        }
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        if (SystemInfo.graphicsMultiThreaded)
        {
            GL.IssuePluginEvent(RenderThreadHandlePtr, GL_DESTROY_EVENT);
        }
        else
        {
            RunOnRenderThread(GL_DESTROY_EVENT);
        }
    }
    
    void Update()
    {
        if (SystemInfo.graphicsMultiThreaded)
        {
            GL.IssuePluginEvent(RenderThreadHandlePtr, GL_UPDATE_EVENT);
        }
        else
        {
            RunOnRenderThread(GL_UPDATE_EVENT);
        }
    }
    
    private void updateTexutre()
    {
        if (texture != null && nativeObject.Call<bool>("isUpdateFrame"))
        {
            Debug.Log("VideoPlugin:Update");
            nativeObject.Call("updateTexture");
            GL.InvalidateState();
        }
    }
    
    private void createTexture()
    {
        Debug.Log("VideoPlugin:Start");
        texture = new Texture2D(width, height, TextureFormat.RGB24, false, false);
        nativeObject.Call("start", (int)texture.GetNativeTexturePtr(), width, height);
        meshRenderer.material.mainTexture = texture;
    }
    
    private void destroyTexture()
    {
        Debug.Log("VideoPlugin:Release");
        texture = null;
        meshRenderer.material.mainTexture = null;
        nativeObject.Call("release");
    }
    
    private const int GL_INIT_EVENT = 0x0001;
    private const int GL_UPDATE_EVENT = 0x0002;
    private const int GL_DESTROY_EVENT = 0x0003;
    [MonoPInvokeCallback(typeof(RenderEventDelegate))]
    private void RunOnRenderThread(int eventID)
    {
        switch (eventID)
        {
            case GL_INIT_EVENT:
                createTexture(); 
                break;
            case GL_UPDATE_EVENT:
                updateTexutre();
                GL.InvalidateState();
                break;
            case GL_DESTROY_EVENT:
                destroyTexture();
                break;
        }
    }

    [DllImport("application")]
    private static extern void start(int unityTextureId, int width, int height);

    [DllImport("application")]
    private static extern void release();

    [DllImport("application")]
    private static extern void updateTexture();
}
