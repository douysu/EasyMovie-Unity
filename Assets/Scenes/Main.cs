﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using AOT;
using System.Runtime.InteropServices;

public class Main : MonoBehaviour
{

    private AndroidJavaObject nativeObject;
    private static int width, height;
    public MeshRenderer meshRenderer;
    private Texture2D texture;
    private delegate void RenderEventDelegate(int eventID);
    private RenderEventDelegate RenderThreadHandle;
    private IntPtr RenderThreadHandlePtr;
    private static int texturePtr;
    private static IntPtr clz_OurAppActitvityClass;

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

        //IntPtr clz = AndroidJNI.FindClass("com/unity3d/player/UnityPlayer");
        //IntPtr fid = AndroidJNI.GetStaticFieldID(clz, "currentActivity", "Landroid/app/Activity;");
        //IntPtr obj = AndroidJNI.GetStaticObjectField(clz, fid);
        //clz_OurAppActitvityClass = AndroidJNI.FindClass("com/pvr/videoplugin/VideoPlugin");
        //IntPtr methodId = AndroidJNI.GetMethodID(clz_OurAppActitvityClass, "initObject", "()V");
        //jvalue v = new jvalue();
        //v.l = AndroidJNI.NewStringUTF("()V");
        // AndroidJNI.CallVoidMethod(obj, methodId, new jvalue[] { v });

        if (SystemInfo.graphicsMultiThreaded)
        {
            Debug.Log("VideoPlugin:Start");
            texture = new Texture2D(width, height, TextureFormat.RGB24, false, false);
            texturePtr = (int)texture.GetNativeTexturePtr();
            GL.IssuePluginEvent(RenderThreadHandlePtr, GL_INIT_EVENT);
            meshRenderer.material.mainTexture = texture;
        }
        else
        {
            Debug.Log("VideoPlugin:Start");
            texture = new Texture2D(width, height, TextureFormat.RGB24, false, false);
            texturePtr = (int)texture.GetNativeTexturePtr();
            RunOnRenderThread(GL_INIT_EVENT);
            meshRenderer.material.mainTexture = texture;
        }
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        if (SystemInfo.graphicsMultiThreaded)
        {
            texture = null;
            meshRenderer.material.mainTexture = null;
            GL.IssuePluginEvent(RenderThreadHandlePtr, GL_DESTROY_EVENT);
        }
        else
        {
            texture = null;
            meshRenderer.material.mainTexture = null;
            RunOnRenderThread(GL_DESTROY_EVENT);
        }
    }
    
    void Update()
    {
        if (SystemInfo.graphicsMultiThreaded)
        {
            GL.IssuePluginEvent(RenderThreadHandlePtr, GL_UPDATE_EVENT);
            GL.InvalidateState();
        }
        else
        {
            RunOnRenderThread(GL_UPDATE_EVENT);
            GL.InvalidateState();
        }
    }
    
    private void updateTexutreCsharp()
    {
        if (texture != null && nativeObject.Call<bool>("isUpdateFrame"))
        {
            Debug.Log("VideoPlugin:Update");
            updateTexture();
            //nativeObject.Call("updateTexture");
            GL.InvalidateState();
        }
    }
    
    private void createTexture()
    {
        Debug.Log("VideoPlugin:Start");
        texture = new Texture2D(width, height, TextureFormat.RGB24, false, false);
        // start((int)texture.GetNativeTexturePtr(), width, height);
        // nativeObject.Call("start", (int)texture.GetNativeTexturePtr(), width, height);
        meshRenderer.material.mainTexture = texture;
    }
    
    private void destroyTexture()
    {
        Debug.Log("VideoPlugin:Release");
        texture = null;
        meshRenderer.material.mainTexture = null;
        release();
        //nativeObject.Call("release");
    }
    
    private const int GL_INIT_EVENT = 0x0001;
    private const int GL_UPDATE_EVENT = 0x0002;
    private const int GL_DESTROY_EVENT = 0x0003;
    [MonoPInvokeCallback(typeof(RenderEventDelegate))]
    private static void RunOnRenderThread(int eventID)
    {
        switch (eventID)
        {
            case GL_INIT_EVENT:
                start(texturePtr, width, height);
                break;
            case GL_UPDATE_EVENT:
                updateTexture();
                break;
            case GL_DESTROY_EVENT:
                release();
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
