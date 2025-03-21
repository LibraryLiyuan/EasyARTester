﻿//================================================================================================================================
//
//  Copyright (c) 2015-2023 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en">Path type.</para>
    /// <para xml:lang="zh">路径类型。</para>
    /// </summary>
    public enum PathType
    {
        /// <summary>
        /// <para xml:lang="en">Absolute path.</para>
        /// <para xml:lang="zh">绝对路径。</para>
        /// </summary>
        Absolute,
        /// <summary>
        /// <para xml:lang="en">Unity StreamingAssets path.</para>
        /// <para xml:lang="zh">UnityStreamingAssets路径。</para>
        /// </summary>
        StreamingAssets,
        /// <summary>
        /// 通过Addressable进行管理，Addressable路径
        /// </summary>
        Addressable,
        /// <summary>
        /// <para xml:lang="en">Not file storage.</para>
        /// <para xml:lang="zh">不是文件存储。</para>
        /// </summary>
        None,
    }

    /// <summary>
    /// <para xml:lang="en">Output file path type.</para>
    /// <para xml:lang="zh">文件输出路径类型。</para>
    /// </summary>
    public enum WritablePathType
    {
        /// <summary>
        /// <para xml:lang="en">Absolute path.</para>
        /// <para xml:lang="zh">绝对路径。</para>
        /// </summary>
        Absolute,
        /// <summary>
        /// <para xml:lang="en">Unity persistent data path.</para>
        /// <para xml:lang="zh">Unity沙盒路径。</para>
        /// </summary>
        PersistentDataPath,
    }

    /// <summary>
    /// <para xml:lang="en">File utility.</para>
    /// <para xml:lang="zh">文件工具。</para>
    /// </summary>
    public static class FileUtil
    {
        private static bool streamingAssetsImported;
        /// <summary>
        /// <para xml:lang="en">Async Load file and return <see cref="Buffer"/> object in the callback.</para>
        /// <para xml:lang="zh">异步加载文件，回调返回<see cref="Buffer"/>对象。</para>
        /// </summary>
        public static IEnumerator LoadFile(string filePath, PathType filePathType, Action<Buffer> onLoad)
        {
            return LoadFile(filePath, filePathType, (data) =>
            {
                if (onLoad == null)
                {
                    return;
                }
                using (var buffer = Buffer.wrapByteArray(data))
                {
                    onLoad(buffer);
                }
            });
        }

        /// <summary>
        /// <para xml:lang="en">Async Load file and return byte array in the callback.</para>
        /// <para xml:lang="zh">异步加载文件，回调返回字节数组。</para>
        /// </summary>
        public static IEnumerator LoadFile(string filePath, PathType filePathType, Action<byte[]> onLoad, Action<string> onError = null)
        {
            if (onLoad == null)
            {
                yield break;
            }
            var path = filePath;
            if (filePathType == PathType.Addressable)
            {
                // TODO: Addressable Load
                var operationHandle = Addressables.LoadAssetAsync<TextAsset>(path);
                yield return operationHandle.WaitForCompletion();
                var error = $"fail to load file {filePath} of type {filePathType}";
                if (operationHandle.Status == AsyncOperationStatus.Failed || operationHandle.Status == AsyncOperationStatus.None)
                {
                    Debug.LogError($"{error}: {operationHandle.Status} {operationHandle.OperationException}");
                    onError?.Invoke($"{error}: {operationHandle.Status} {operationHandle.OperationException}");
                    yield break;
                }
                while (!operationHandle.IsDone)
                {
                    yield return 0;
                }
                if (operationHandle.Result == null)
                {
                    Debug.LogError($"{error}: data is null");
                    onError?.Invoke($"{error}: data is null");
                    yield break;
                }
                onLoad(operationHandle.Result.bytes);
            }
            else
            {
                if (filePathType == PathType.StreamingAssets)
                {
                    path = Application.streamingAssetsPath + "/" + path;
                }
                using (var request = UnityWebRequest.Get(PathToUrl(path)))
                {
                    yield return request.SendWebRequest();
                    var error = $"fail to load file {filePath} of type {filePathType}";
#if UNITY_2020_1_OR_NEWER
                    if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
#else
                if (request.isHttpError || request.isNetworkError)
#endif
                    {
                        Debug.LogError($"{error}: {request.error}");
                        onError?.Invoke($"{error}: {request.error}");
                        yield break;
                    }
                    while (!request.isDone)
                    {
                        yield return 0;
                    }
                    if (request.downloadHandler.data == null)
                    {
                        Debug.LogError($"{error}: data is null");
                        onError?.Invoke($"{error}: data is null");
                        yield break;
                    }
                    onLoad(request.downloadHandler.data);
                }
            }
        }

        /// <summary>
        /// <para xml:lang="en">Convert file path to URL.</para>
        /// <para xml:lang="zh">将路径转换成URL。</para>
        /// </summary>
        public static string PathToUrl(string path)
        {
            if (string.IsNullOrEmpty(path) || path.StartsWith("jar:file://") || path.StartsWith("file://") || path.StartsWith("http://") || path.StartsWith("https://"))
            {
                return path;
            }
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android)
            {
                path = "file://" + path;
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                path = "file:///" + path;
            }
            return path;
        }

        /// <summary>
        /// <para xml:lang="en">Import sample streaming assets into Unity project.</para>
        /// <para xml:lang="zh">导入sample streaming assets到工程中。</para>
        /// </summary>
        public static void ImportSampleStreamingAssets()
        {
#if UNITY_EDITOR
            if (streamingAssetsImported)
            {
                return;
            }
            streamingAssetsImported = true;
            var pacakge = $"Packages/{UnityPackage.Name}/Samples~/StreamingAssets/assets.unitypackage";
            if (!File.Exists($"{Application.streamingAssetsPath}/{UnityPackage.Name}-{UnityPackage.Version}") && File.Exists(Path.GetFullPath(pacakge)))
            {
                AssetDatabase.ImportPackage(pacakge, false);
            }
#endif
        }
    }
}
