using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using ZaoHuaBMod.Core;
using ZaoHuaBMod.UI.Framework;
using ZaoHuaBMod.UI.Views;

namespace ZaoHuaBMod
{
    [BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
    public class Main : BaseUnityPlugin
    {
        public static Main Instance;

        private WindowData _mainWindow;

        private void Awake()
        {
            Instance = this;
            Log.Logger = Logger;
            Harmony.CreateAndPatchAll(typeof(Main));

            Log.Info("ZaoHuaBMod Loaded!");
            
            // 创建主窗口，默认显示以便测试
            _mainWindow = MainView.CreateWindow();
            _mainWindow.Show();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote)) {
                if (_mainWindow.Visible)
                    _mainWindow.Hide();
                else
                    _mainWindow.Show();
            }
        }

        private void OnGUI()
        {
            GUIManager.Instance.OnGUI();
        }
    }
}