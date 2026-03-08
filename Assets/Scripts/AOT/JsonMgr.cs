using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace AOT
{
    public sealed class JsonMgr
    {
        private static JsonMgr s_Instance;
        public static JsonMgr instance => s_Instance ??= new JsonMgr();

        private JsonMgr()
        {
        }

        /// <summary>
        /// 异步读取 JSON 数据 (泛型)
        /// 优先级: PersistentDataPath -> StreamingAssetsPath
        /// </summary>
        /// <typeparam name="T">要反序列化的目标数据类型</typeparam>
        /// <param name="fileName">文件名，例如 "GameConfig.json"</param>
        /// <returns>反序列化后的对象实例</returns>
        public async UniTask<T> LoadJsonDataAsync<T>(string fileName, GamePath.DataType type)
        {
            var persistPath = GamePath.GetFilePath(type, fileName);
            var streamingPath = Path.Combine(Application.streamingAssetsPath, "Json", fileName);

            // 1. 优先读取持久化目录，流式读取，因为可能用户多次保存后的数据很大
            if (File.Exists(persistPath))
            {
                return await DeserializeFromFile<T>(persistPath);
            }

            // 2. 持久化目录没有，读取 StreamingAssets，初始化数据，一般很小，直接读取整个文本到内存再反序列化
            var jsonContent = string.Empty;
            if (Application.platform == RuntimePlatform.Android)
            {
                using var webRequest = UnityWebRequest.Get(streamingPath);
                await webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    jsonContent = webRequest.downloadHandler.text;
                }
            }
            //非 Android
            else if (File.Exists(streamingPath))
            {
                jsonContent = await File.ReadAllTextAsync(streamingPath);
            }

            if (!string.IsNullOrEmpty(jsonContent))
            {
                // 自动备份到 Persistent，下次直接读本地
                try
                {
                    await File.WriteAllTextAsync(persistPath, jsonContent);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[JsonMgr] 自动备份失败: {e.Message}");
                }
                return JsonConvert.DeserializeObject<T>(jsonContent);
            }

            return default;
        }

        // 流式解析减少内存开销
        private async UniTask<T> DeserializeFromFile<T>(string path)
        {
            return await UniTask.RunOnThreadPool(() =>
            {
                using var sr = new StreamReader(path);
                using var reader = new JsonTextReader(sr);
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            });
        }

        /// <summary>
        /// 安全的异步保存
        /// </summary>
        public async UniTask SaveJsonDataAsync<T>(string fileName, T data, GamePath.DataType type)
        {
            var persistPath = GamePath.GetFilePath(type, fileName);
            var tempPath = GamePath.GetFilePath(GamePath.DataType.Tmp, fileName);

            try
            {
                await UniTask.RunOnThreadPool(() =>
                {
                    var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                    File.WriteAllText(tempPath, json);
                    if (File.Exists(persistPath))
                    {
                        File.Delete(persistPath);
                    }
                    File.Move(tempPath, persistPath);
                });
                Debug.Log($"[JsonMgr] 安全保存成功: {persistPath}");
            }
            catch (Exception e)
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                Debug.LogError($"[JsonMgr] 保存失败: {e.Message}");
            }
        }
    }
}