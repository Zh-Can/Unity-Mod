using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using ZaoHuaMod.GuiFramework.Logger;

namespace ZaoHuaMod.GuiFramework.Other
{
    public class HttpGet
    {
        static IEnumerator SimpleGet(string url, Action<string> success, Action<string> error)
        {
            UnityWebRequest req = UnityWebRequest.Get(url);
            req.timeout = 8;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                success?.Invoke(req.downloadHandler.text);
            }
            else
            {
                error?.Invoke(req.error);
            }
        }

        
        
        public static int Count = 0;
        public static readonly string GetCountUrl = "https://countapi.mileshilliard.com/api/v1/get/demo_visits";
        public static readonly string HitCountUrl = "https://countapi.mileshilliard.com/api/v1/hit/demo_visits";
        
        /// <summary>执行统计上报（需要传入一个 MonoBehaviour 来启动协程）。没网时自动跳过。</summary>
        public static int TryGetStat(MonoBehaviour runner)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Log.Info("无网络连接，跳过统计上报");
                return Count++;
            }
            runner.StartCoroutine(SimpleGet(GetCountUrl, text =>
                {
                    // 手动解析 {"key":"demo_visits","value":31168}
                    var valStr = text.Substring(text.IndexOf("\"value\":") + 8);
                    var end = valStr.IndexOfAny(new[] { ',', '}' });
                    if (end >= 0) valStr = valStr.Substring(0, end);
                    if (int.TryParse(valStr.Trim(), out var count))
                    {
                        Log.Info($"总访问量：{count}");
                        Count = count;
                    }
                },
                err =>
                {
                    Log.Warning("统计上报失败：" + err);
                }));
            return Count;
        }

        public static int TryHit(MonoBehaviour runner)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Log.Info("无网络连接，跳过统计上报");
                return Count++;
            }
            runner.StartCoroutine(SimpleGet(HitCountUrl, text =>
                {
                    // 手动解析 {"key":"demo_visits","value":31168}
                    var valStr = text.Substring(text.IndexOf("\"value\":") + 8);
                    var end = valStr.IndexOfAny(new[] { ',', '}' });
                    if (end >= 0) valStr = valStr.Substring(0, end);
                    if (int.TryParse(valStr.Trim(), out var count))
                    {
                        Log.Info($"总访问量：{count}");
                        Count = count;
                    }
                },
                err =>
                {
                    Log.Warning("统计上报失败：" + err);
                }));
            return Count;
        }
    }
}