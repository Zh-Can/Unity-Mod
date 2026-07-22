using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using ZaoHuaBMod.Core;
using ZaoHuaBMod.UI.Views;

namespace ZaoHuaBMod
{
    [BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
    public class Main : BaseUnityPlugin
    {
        public static Main Instance;

        private void Start()
        {
            Instance = this;
            Log.Logger = Logger;
            Harmony.CreateAndPatchAll(typeof(Main));

            Log.Info("ZaoHuaBMod Loaded!");
            

            GameObject uiObj = new GameObject("ModUI");
            UnityEngine.Object.DontDestroyOnLoad(uiObj);
            uiObj.AddComponent<MainView>();
            
            Debug.Log($"创建物体成功：{uiObj != null} 组件实例：{uiObj != null}");
        }
    }
}
