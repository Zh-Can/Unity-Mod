using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using LunHuiShop.GuiFramework.Logger;

namespace LunHuiShop.GuiFramework.Other
{
    public static class HttpGet
    {
        static IEnumerator SimpleGet(string url, Action<string> success, Action<string> error)
        {
            UnityWebRequest req = UnityWebRequest.Get(url);
            req.SetRequestHeader("Authorization", "Bearer ut_gHz09Uw1VM6GgIqLcYv38NILsPUW81VsRjRf3OXv");
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
        private static readonly string HitCountUrl = "https://api.counterapi.dev/v2/cans-team-4837/lymod/up";
        
        public static int TryHit(MonoBehaviour runner)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Log.Info("无网络连接，跳过统计上报");
                return Count++;
            }
            MelonLoader.MelonCoroutines.Start(SimpleGet(HitCountUrl, text =>
                {
                    const string key = "\"up_count\":";
                    var idx = text.IndexOf(key, StringComparison.Ordinal);
                    if (idx >= 0)
                    {
                        var start = idx + key.Length;
                        var end = text.IndexOf(',', start);
                        if (end < 0) end = text.IndexOf('}', start);
                        if (end < 0) end = text.Length;
                        if (int.TryParse(text[start..end], out var count))
                        {
                            Log.Info($"总访问量：{count}");
                            Count = count;
                        }
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