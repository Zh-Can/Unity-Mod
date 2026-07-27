using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using ZaoHuaBMod.GuiFramework.Logger;

namespace ZaoHuaBMod.GuiFramework.Other
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
        private static readonly string HitCountUrl = "https://api.counterapi.dev/v2/cans-team-4837/zaohuamod/up";
        
        public static int TryHit(MonoBehaviour runner)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Log.Info("无网络连接，跳过统计上报");
                return Count++;
            }
            runner.StartCoroutine(SimpleGet(HitCountUrl, text =>
                {
                    var m = System.Text.RegularExpressions.Regex.Match(text, "\"up_count\":(\\d+)");
                    if (m.Success && int.TryParse(m.Groups[1].Value, out var count))
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