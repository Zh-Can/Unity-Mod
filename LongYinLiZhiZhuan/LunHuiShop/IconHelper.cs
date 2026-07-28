﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.U2D;
using LunHuiShop.GuiFramework.Logger;

namespace LunHuiShop;

/// <summary>
///     游戏图标辅助类。
///     用 RenderTexture + ReadPixels 从 GPU 读取像素数据（不要求纹理 readable），
///     创建独立 Texture2D 完全绕开 SpriteAtlas 生命周期问题。
/// </summary>
public static class IconHelper
{
    private static Dictionary<string, Texture2D>? _textureCache;
    private static SpriteAtlas? _atlas;
    private static bool _initAttempted;

    private static readonly string[] CommonIconNames =
    {
        "1_0_0", "1_1_0", "1_2_0", "1_3_0", "1_4_0", "1_5_0",
        "2_1_0", "2_2_0", "2_3_0", "2_4_0", "2_5_0",
        "3_1_0", "3_2_0", "3_3_0", "3_4_0", "3_5_0",
        "4_1_0", "4_2_0", "4_3_0", "4_4_0", "4_5_0",
        "0_1_1_5", "0_2_4_5", "0_3_0_5",
        "2_1_4", "2_2_2",
        "3_1_5", "3_2_5", "3_5_5",
        "4_3_5", "4_4_5"
    };

    public static void EnsureInit()
    {
        if (_textureCache != null || _initAttempted) return;
        _initAttempted = true;

        try
        {
            _atlas = Resources.Load<SpriteAtlas>("IconAtlas");
            if (_atlas == null)
            {
                Log.Warning("IconHelper: Resources.Load<SpriteAtlas>(\"IconAtlas\") 返回 null");
                return;
            }

            _textureCache = new Dictionary<string, Texture2D>();

            foreach (var name in CommonIconNames)
                CacheSprite(name);

            Log.Info($"IconHelper: 初始化成功, 已缓存 {_textureCache.Count} 个图标");
        }
        catch (Exception ex)
        {
            Log.Warning($"IconHelper: EnsureInit 异常: {ex.GetType().Name}: {ex.Message}");
            _initAttempted = false;
        }
    }

    private static void CacheSprite(string iconName)
    {
        if (_atlas == null || _textureCache == null) return;
        if (_textureCache.ContainsKey(iconName)) return;

        try
        {
            var sprite = _atlas.GetSprite(iconName);
            if (sprite == null) return;

            var srcTex = sprite.texture;
            if (srcTex == null) return;

            var r = sprite.textureRect;
            var w = (int)r.width;
            var h = (int)r.height;
            if (w <= 0 || h <= 0) return;

            // 用 RenderTexture 读取不可读纹理的像素数据
            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            try
            {
                Graphics.Blit(srcTex, rt, new Vector2((float)w / srcTex.width, (float)h / srcTex.height),
                    new Vector2(r.x / srcTex.width, r.y / srcTex.height));
                RenderTexture.active = rt;

                var dstTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                dstTex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                dstTex.Apply();
                dstTex.hideFlags = HideFlags.DontSave;

                _textureCache[iconName] = dstTex;
            }
            finally
            {
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(rt);
            }
        }
        catch
        {
            // 忽略单个图标加载失败
        }
    }

    public static void DrawIcon(Rect rect, string iconName)
    {
        if (string.IsNullOrEmpty(iconName) || _textureCache == null) return;

        if (!_textureCache.TryGetValue(iconName, out var tex))
        {
            CacheSprite(iconName);
            if (!_textureCache.TryGetValue(iconName, out tex))
                return;
        }

        if (tex == null) return;
        GUI.DrawTexture(rect, tex);
    }

    public static void DrawCellWithIcon(Rect cellRect, string? iconName, string text, float iconSize = 24f)
    {
        if (string.IsNullOrEmpty(iconName) || !CanDrawIcon(iconName))
        {
            GUI.Label(cellRect, text, GuiFramework.Style.DarkSkin.SLabel);
            return;
        }

        var iconRect = new Rect(cellRect.x + 2f,
            cellRect.y + (cellRect.height - iconSize) * 0.5f,
            iconSize, iconSize);
        DrawIcon(iconRect, iconName);

        var textRect = new Rect(cellRect.x + iconSize + 4f, cellRect.y,
            Mathf.Max(0f, cellRect.width - iconSize - 6f), cellRect.height);
        GUI.Label(textRect, text, GuiFramework.Style.DarkSkin.SLabel);
    }

    private static bool CanDrawIcon(string iconName)
    {
        EnsureInit();
        if (_textureCache == null) return false;
        if (_textureCache.ContainsKey(iconName)) return true;
        CacheSprite(iconName);
        return _textureCache.ContainsKey(iconName);
    }
}
