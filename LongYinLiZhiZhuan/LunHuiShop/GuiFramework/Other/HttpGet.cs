using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using LunHuiShop.GuiFramework.Logger;
using MelonLoader;

namespace LunHuiShop.GuiFramework.Other
{
    public static class HttpGet
    {
        // ---------- 常量配置 ----------
        private const string BaseUrl = "https://api.counterapi.dev/v2/cans-team-4837/lymod/up";
        private const string AuthToken = "Bearer ut_gHz09Uw1VM6GgIqLcYv38NILsPUW81VsRjRf3OXv";
        private const int TimeoutSeconds = 10;

        // ---------- 公共状态 ----------
        public static int Count = 0;

        // ---------- 静态构造，TLS 仅设置一次 ----------
        static HttpGet()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        }

        // ---------- 核心请求协程 ----------
        private static IEnumerator SendRequest(Action<string> onSuccess, Action<string> onError)
        {
            var req = UnityWebRequest.Get(BaseUrl);
            req.SetRequestHeader("Authorization", AuthToken);
            req.SetRequestHeader("User-Agent", "Mozilla/5.0 (Unity)");
            req.timeout = TimeoutSeconds;
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(req.downloadHandler.text);
            }
            else
            {
                string errorMsg = $"错误码:{req.responseCode} 信息:{req.error}";
                if (!string.IsNullOrEmpty(req.downloadHandler.text))
                    errorMsg += $"\n响应内容:{req.downloadHandler.text}";
                onError?.Invoke(errorMsg);
            }
        }

        // ---------- 解析响应 ----------
        private static bool TryParseCount(string json, out int count)
        {
            count = 0;
            const string key = "\"up_count\":";
            int idx = json.IndexOf(key, StringComparison.Ordinal);
            if (idx < 0) return false;

            int start = idx + key.Length;
            int end = json.IndexOf(',', start);
            if (end < 0) end = json.IndexOf('}', start);
            if (end < 0) end = json.Length;

            return int.TryParse(json.Substring(start, end - start), out count);
        }

        // ---------- 对外调用入口 ----------
        public static int TryHit(MonoBehaviour runner)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Log.Info("无网络连接，跳过统计上报");
                return Count;
            }

            if (runner == null)
            {
                Log.Warning("TryHit 传入的 runner 为空，无法启动协程");
                return Count;
            }

            MelonCoroutines.Start(SendRequest(
                onSuccess: text =>
                {
                    if (TryParseCount(text, out int count))
                    {
                        Log.Info($"总访问量：{count}");
                        Count = count;
                    }
                    else
                    {
                        Log.Warning($"解析计数失败，原始响应：{text}");
                    }
                },
                onError: err => Log.Warning($"统计上报失败：{err}")
            ));

            return Count;
        }
    }
}