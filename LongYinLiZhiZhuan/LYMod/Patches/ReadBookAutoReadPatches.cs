using Il2Cpp;
using System;
using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using HarmonyLib;

namespace LYMod.Patches;

public class ReadBookAutoReadPatches
{
    private static GameObject _autoReadButton;
    private static float _lastClickTime;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ReadBookTextController), nameof(ReadBookTextController.Init))]
    public static void ReadBookTextController_Init_Postfix(ReadBookTextController __instance)
    {
        if (__instance == null || !Plugin.Instance.AutoReadBookFlag.Value || ModConfig.HaveReadBookPlus) return;
        __instance.SeeText();
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ReadBookController), nameof(ReadBookController.RealStartReadBook))]
    public static void ReadBookController_RealStartReadBook_Postfix(ReadBookController __instance)
    {
        if (__instance == null || !Plugin.Instance.AutoReadBookFlag.Value) return;
        CreateAutoReadButton(__instance);
    }

    private static void CreateAutoReadButton(ReadBookController readBookController)
    {
        DestroyAutoReadButton();

        var readBookUIPanel = readBookController.readBookUIPanel;
        if (readBookUIPanel == null) return;
        
        var finishButton = FindFinishButton(readBookUIPanel);
        if (finishButton == null) return;

        _autoReadButton = CreateButtonFromTemplate(finishButton, "AutoReadButton", "一键阅读");
        if (_autoReadButton == null) return;
        PositionButton(_autoReadButton, finishButton);
    }

    private static Button FindFinishButton(GameObject readBookUIPanel)
    {
        var buttons = readBookUIPanel.GetComponentsInChildren<Button>(true);
        
        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            if (!btn.gameObject.name.Equals("FinishReadButton", StringComparison.OrdinalIgnoreCase)) continue;
            return btn;
        }
        return null;
    }

    private static GameObject CreateButtonFromTemplate(Button template, string buttonName, string buttonText)
    {
        var newButton = Object.Instantiate(template.gameObject, template.transform.parent);
        newButton.name = buttonName;

        DisableNativeClick(newButton);
        var buttonComponent = newButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.RemoveAllListeners();
        }

        SetButtonText(newButton, buttonText);
        EnsureImagesVisible(newButton);

        return newButton;
    }

    private static void SetButtonText(GameObject buttonObj, string text)
    {
        for (int i = 0; i < buttonObj.transform.childCount; i++)
        {
            var child = buttonObj.transform.GetChild(i);
            if (child == null) continue;
            
            // Plugin.LOG.Msg($"[一键阅读] 检查子对象: {child.name}");
            
            var textComponent = child.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = text;
                textComponent.enabled = true;
                textComponent.raycastTarget = false;
                // Plugin.LOG.Msg($"[一键阅读] 已设置文本: {text}");
            }
        }
    }

    private static void EnsureImagesVisible(GameObject buttonObj)
    {
        var images = buttonObj.GetComponentsInChildren<Image>(true);
        foreach (var image in images)
        {
            if (image == null) continue;
            image.enabled = true;
            image.raycastTarget = false;
            // Plugin.LOG.Msg($"[一键阅读] 启用图片: {image.name}");
        }
    }

    private static void DisableNativeClick(GameObject root)
    {
        var button = root.GetComponent<Button>();
        if (button != null)
        {
            button.enabled = false;
        }
    }

    private static void PositionButton(GameObject autoReadButton, Button finishButton)
    {
        var finishRect = finishButton.GetComponent<RectTransform>();
        var autoReadRect = autoReadButton.GetComponent<RectTransform>();

        if (finishRect == null || autoReadRect == null) return;

        // Plugin.LOG.Msg($"[一键阅读] 阅毕按钮 - 位置: {finishRect.anchoredPosition}, 大小: {finishRect.sizeDelta}");
        // Plugin.LOG.Msg($"[一键阅读] 阅毕按钮 - 父对象: {finishButton.transform.parent.name}");
        // Plugin.LOG.Msg($"[一键阅读] 阅毕按钮 - Scale: {finishButton.transform.localScale}");
        // Plugin.LOG.Msg($"[一键阅读] 一键阅读按钮 - 原始Scale: {autoReadButton.transform.localScale}");
        
        autoReadButton.transform.localScale = Vector3.one;
        // Plugin.LOG.Msg($"[一键阅读] 一键阅读按钮 - 新Scale: {autoReadButton.transform.localScale}");
        
        autoReadRect.sizeDelta = finishRect.sizeDelta;
        autoReadRect.anchorMin = finishRect.anchorMin;
        autoReadRect.anchorMax = finishRect.anchorMax;
        autoReadRect.pivot = finishRect.pivot;
        
        var gap = 20f;
        autoReadRect.anchoredPosition = new Vector2(finishRect.anchoredPosition.x - finishRect.sizeDelta.x - gap, finishRect.anchoredPosition.y);
        
        // Plugin.LOG.Msg($"[一键阅读] 一键阅读按钮 - 位置: {autoReadRect.anchoredPosition}, 大小: {autoReadRect.sizeDelta}");
        
        autoReadButton.SetActive(true);
        autoReadButton.transform.SetAsLastSibling();
        // Plugin.LOG.Msg($"[一键阅读] 按钮激活状态: {autoReadButton.activeInHierarchy}");
        // Plugin.LOG.Msg($"[一键阅读] 按钮层级索引: {autoReadButton.transform.GetSiblingIndex()}");
    }

    private static void DestroyAutoReadButton()
    {
        if (_autoReadButton != null)
        {
            Object.Destroy(_autoReadButton);
            _autoReadButton = null;
        }
    }

    public static void HandleAutoReadButton()
    {
        if (_autoReadButton == null) return;

        var readBookController = ReadBookController.Instance;
        if (readBookController == null || !readBookController.readBookUIPanel.activeInHierarchy)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (Time.unscaledTime - _lastClickTime < 0.2f)
            return;

        CheckButtonClick(_autoReadButton, OnAutoReadClicked);
    }

    private static void CheckButtonClick(GameObject buttonObj, Func<IEnumerator> callback)
    {
        var rect = buttonObj.GetComponent<RectTransform>();
        if (rect == null) return;

        var canvas = buttonObj.GetComponentInParent<Canvas>();
        Camera camera = null;
        if (canvas != null && (int)canvas.renderMode != 0)
        {
            camera = canvas.worldCamera;
        }

        if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, camera))
        {
            _lastClickTime = Time.unscaledTime;
            MelonCoroutines.Start(callback());
        }
    }

    // 按分组顺序触发：第1组 -> 第2组 -> 第3组 -> 第4组
    private static readonly string[][] ClickOrderGroups =
    {
        new[] { "提纲", "注释", "明朗" },
        new[] { "预习" },
        new[] { "精要", "清醒" },
        new[] { "文本" },
        new[] { "融汇" }
    };
    /// <summary>
    /// 点击一键阅读按钮触发
    ///
    /// 此处代码由大佬 3DM：名取早耶香 提供优化
    /// </summary>
    private static IEnumerator OnAutoReadClicked()
    {
        var textGrid = GameObject.Find("Canvas/ReadBookUIPanel/Paper/TextGrid");
        if (textGrid == null) yield break;
        
        // 只遍历一次 UI 子节点，按 fullName 归档，保留原始子节点顺序
        var controllersByName = new Dictionary<string, List<ReadBookTextController>>(StringComparer.Ordinal);
        var childCount = textGrid.transform.childCount;

        for (var i = 0; i < childCount; i++)
        {
            var child = textGrid.transform.GetChild(i);
            if (!child.TryGetComponent<ReadBookTextController>(out var controller))
            {
                continue;
            }
            var fullName = controller.textData?.fullName;
            if (string.IsNullOrEmpty(fullName))
            {
                continue;
            }
            if (!controllersByName.TryGetValue(fullName, out var list))
            {
                list = new List<ReadBookTextController>();
                controllersByName[fullName] = list;
            }
            list.Add(controller);
        }

        // 按组顺序触发，语义与多轮 for 更接近，但避免重复扫描
        foreach (var group in ClickOrderGroups)
        {
            foreach (var keyword in group)
            {
                if (!controllersByName.TryGetValue(keyword, out var list))
                {
                    continue;
                }
                foreach (var controller in list)
                {
                    controller.OnClick();
                }
            }
        }
    }
}