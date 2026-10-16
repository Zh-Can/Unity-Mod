using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using SMR2Mod.GuiFramework.Config;
using SMR2Mod.GuiFramework.Controls;
using SMR2Mod.GuiFramework.Localization;
using SMR2Mod.GuiFramework.Logger;
using SMR2Mod.GuiFramework.Other;
using SMR2Mod.GuiFramework.Style;
using UnityEngine;

namespace SMR2Mod
{
    public class MainView : MonoBehaviour
    {
        private WindowData _mainWindow;
        private WindowData _hotkeyWindow;
        private HotkeyConfig _editingHotkey = new HotkeyConfig();
        private bool _capturingHotkey;

        private List<ItemDef> _itemDefs;
        private List<ItemDef> _filteredItems = new List<ItemDef>();
        private readonly List<string> _itemCategoryIds = new List<string>();
        private readonly Dictionary<string, Texture2D> _itemIcons = new Dictionary<string, Texture2D>();
        private string _itemSearch = "";
        private int _itemCategoryIndex;
        private bool _itemCategoryOpen;
        private int _selectedItem = -1;
        private int _itemPage;
        private const int ItemPageSize = 50;
        private string _giveCountText = "1";
        private Vector2 _itemScroll;

        private static Texture2D _placeholderTex;
        
        private int _tab;
        private static readonly string[] Tabs = { Loc.Get("通用"), Loc.Get("物品")};
        
        
        private void Start()
        {
            HttpGet.TryHit(this);
            MainGame.OnGameStarted += OnGameStarted;

            _mainWindow = UI.NewWindow(
                    new Rect(100, 100, 780, 780),
                    "SMR2Mod",
                    DrawMainWindow)
                .Id(1)
                .Hide()
                .Footer(DrawStatusBar)
                .Resizable()
                .Build();

            _hotkeyWindow = UI.NewWindow(
                    new Rect(300, 300, 320, 220),
                    "",
                    DrawHotkeyWindow)
                .Id(2)
                .Hide()
                .Build();
        }

        
        private void Update()
        {
            if (BaseConfig.Hotkey.IsPressed())
            {
                _mainWindow.Visible = !_mainWindow.Visible;
            }
        }

        private void OnDestroy()
        {
            MainGame.OnGameStarted -= OnGameStarted;
        }

        private void OnGameStarted()
        {
            _itemDefs = null;
            LoadItems();
        }
        
        private void OnGUI()
        {
            if (_capturingHotkey)
            {
                var e = Event.current;
                if (e.type == EventType.KeyDown)
                {
                    // 忽略纯修饰键
                    KeyCode key = e.keyCode;
                    if (key != KeyCode.LeftControl &&
                        key != KeyCode.RightControl &&
                        key != KeyCode.LeftAlt &&
                        key != KeyCode.RightAlt &&
                        key != KeyCode.LeftShift &&
                        key != KeyCode.RightShift &&
                        key != KeyCode.None)
                    {
                        _editingHotkey.Key = e.keyCode;
                        _editingHotkey.Ctrl = e.control;
                        _editingHotkey.Alt = e.alt;
                        _editingHotkey.Shift = e.shift;
                        _capturingHotkey = false;
                    }
                    e.Use();
                }
            }
            UI.WindowControls.OnGUI();
        }
        /// <summary>
        /// 主窗体绘制
        /// </summary>
        private void DrawMainWindow()
        {
            _tab = UI.TabGroup(Tabs, _tab);
            
            UI.Divider();
            
            switch (_tab)
            {
                case 0:
                    DrawBasicTab();
                    break;
                case 1:
                    DrawItemTab();
                    break;
            }
        }
        
        private void DrawBasicTab()
        {
            UI.Label("玩家").Title().Draw();
            UI.Horizontal(() =>
            {
                UI.Label("战斗时血量").Text().Draw();
                UI.Button("回满").OnClick(() =>
                {
                    MainGame.PlayerData.hpComponent.RestoreFullHp();
                }).Draw();
                UI.Space();
                GamePatches.CombatGodMode = UI.Toggle("无敌").Value(GamePatches.CombatGodMode).Draw();
            });
            UI.Horizontal(() =>
            {
                UI.Label("战斗时体力").Text().Draw();
                GamePatches.InfiniteCombatStamina = UI.Toggle("无限").Value(GamePatches.InfiniteCombatStamina).Draw();
                
                UI.Space();
            });
            UI.Horizontal(() =>
            {
                UI.Label("体力").Text().Draw();
                UI.Button("回满").OnClick(() =>
                {
                    var pd = MainGame.PlayerData;
                    var obj = pd.GetResSystem("energy");
                    pd.AddRes("energy", GameHelpers.GetMax(obj));
                }).Draw();
                UI.Space();
                GamePatches.InfiniteEnergy = UI.Toggle("无限").Value(GamePatches.InfiniteEnergy).Draw();
            });
            UI.Horizontal(() =>
            {
                UI.Label("疯狂").Text().Draw();
                UI.Button("清零").OnClick(() =>
                {
                    MainGame.PlayerData.SetRes("insanity", 0f);
                }).Draw();
                UI.Button("+100疯狂").OnClick(() =>
                {
                    MainGame.PlayerData.AddRes("insanity", 100f);
                }).Draw();
                UI.Space();
                GamePatches.LockInsanity = UI.Toggle("锁定").Value(GamePatches.LockInsanity).Draw();
            });
            UI.Horizontal(() =>
            {
                UI.Label("金钱").Text().Draw();
                UI.Button("+50铜币").Btn().OnClick(() =>
                {
                    MainGame.PlayerData.AddRes("money", 50);
                    LazyAudio.PlayAndForget("coins_sound");
                }).Draw();
                UI.Button("+5银币").Btn().OnClick(() =>
                {
                    MainGame.PlayerData.AddRes("money", 500);
                    LazyAudio.PlayAndForget("coins_sound");
                }).Draw();
                UI.Button("+1金币").Btn().OnClick(() =>
                {
                    MainGame.PlayerData.AddRes("money", 10000);
                    LazyAudio.PlayAndForget("coins_sound");
                }).Draw();
                UI.Space();
            });
            
            UI.Horizontal(() =>
            {
                UI.Label("科技点数").Text().Draw();
                UI.Button("+100 红色").Btn().OnClick(() =>
                {
                    MainGame.PlayerData.AddRes("tech_red", 100);
                }).Draw();
                UI.Button("+100 绿色").Btn().OnClick(() =>
                {
                    MainGame.PlayerData.AddRes("tech_green", 100);
                }).Draw();  
                UI.Button("+100 蓝色").Btn().OnClick(() =>
                {
                    MainGame.PlayerData.AddRes("tech_blue", 100);
                }).Draw();  
                UI.Space();
            });
            
            UI.Horizontal(() =>
            {
                UI.Label("小镇谢礼").Text().Draw();
                UI.Button("加满").Btn().OnClick(() =>
                {
                    var pd = MainGame.PlayerData;
                    var obj = pd.GetResSystem("happiness");
                    pd.AddRes("happiness", GameHelpers.GetMax(obj));
                }).Draw();
                UI.Space();
                GamePatches.FullHappiness = UI.Toggle("总是满值").Value(GamePatches.FullHappiness).Draw();
                UI.Space();
            });
            
            UI.Label("其他").Title().Draw();
            UI.Horizontal(() =>
            {
                GamePatches.OneHitWork = UI.Toggle("一次完成工作内容").Value(GamePatches.OneHitWork).Draw();
                UI.Space();
            });
            
        }
        private void DrawItemTab()
        {
            if (_itemDefs == null)
                LoadItems();

            UI.Horizontal(() =>
            {
                var search = UI.TextFiled(_itemSearch, DarkSkin.SField, Loc.Get("搜索物品名称"), GUILayout.Width(240));
                if (search != _itemSearch)
                {
                    _itemSearch = search;
                    ApplyItemFilter();
                }

                UI.Space(12);
                UI.Label("分类").Text().Draw();
                var options = _itemCategoryIds.Select((id, i) => i == 0 ? Loc.Get("全部") : GetGroupName(id)).ToArray();
                var newIndex = UI.Dropdown
                    .Options(options)
                    .Selected(_itemCategoryIndex)
                    .Draw(ref _itemCategoryOpen, GUILayout.Width(200));
                if (newIndex != _itemCategoryIndex)
                {
                    _itemCategoryIndex = newIndex;
                    ApplyItemFilter();
                }

                UI.FlexibleSpace();
                UI.Label($"{Loc.Get("共")} {_filteredItems.Count} {Loc.Get("项")}").AsHint().Draw();
            });

            UI.Space(4);
            UI.Horizontal(() =>
            {
                UI.Label("数量").Text().Draw();
                _giveCountText = UI.TextFiled(_giveCountText, DarkSkin.SField, "1", GUILayout.Width(80));

                UI.Space(12);
                var selectedId = _selectedItem >= 0 && _selectedItem < _filteredItems.Count
                    ? _filteredItems[_selectedItem].id
                    : Loc.Get("未选择");
                UI.Label($"{Loc.Get("已选")}: {selectedId}").AsHint().Draw();

                UI.FlexibleSpace();
                UI.Button("添加").Add().OnClick(GiveSelectedItem).Draw(GUILayout.Width(100));
            });

            UI.Space(4);

            var pageCount = Mathf.Max(1, Mathf.CeilToInt(_filteredItems.Count / (float)ItemPageSize));
            _itemPage = Mathf.Clamp(_itemPage, 0, pageCount - 1);

            var pageBase = _itemPage * ItemPageSize;
            var pageItemsCount = Mathf.Min(ItemPageSize, Mathf.Max(0, _filteredItems.Count - pageBase));
            var pageItems = _filteredItems.GetRange(pageBase, pageItemsCount);

            var localSelected = _selectedItem >= pageBase && _selectedItem < pageBase + pageItemsCount
                ? _selectedItem - pageBase
                : -1;

            localSelected = UI.Table(
                pageItems,
                def => new[]
                {
                    "",
                    GetItemName(def),
                    def.id ?? "",
                    def.stackCount > 1 ? def.stackCount.ToString() : ""
                },
                localSelected,
                new[] { "", Loc.Get("名称"), "ID", Loc.Get("堆叠") },
                ref _itemScroll,
                500f,
                colWidth: 160f,
                selectable: true,
                showIndex: false,
                availableWidth: 720f,
                onDrawFirstCell: (rect, def, _) =>
                {
                    var icon = GetItemIcon(def);
                    GUI.DrawTexture(new Rect(rect.x, rect.y + 2f, 26f, 26f), icon ?? PlaceholderTex(), ScaleMode.ScaleToFit, true);
                });

            _selectedItem = localSelected >= 0 ? pageBase + localSelected : -1;

            if (pageCount > 1)
            {
                UI.Space(4);
                UI.Horizontal(() =>
                {
                    GUI.enabled = _itemPage > 0;
                    if (UI.Button("◀").Btn().Draw(GUILayout.Width(40)))
                    {
                        _itemPage--;
                        _selectedItem = -1;
                        _itemScroll = Vector2.zero;
                    }
                    GUI.enabled = true;

                    UI.FlexibleSpace();
                    UI.Label($"{Loc.Get("第")} {_itemPage + 1} / {pageCount} {Loc.Get("页")}").AsHint().Draw();
                    UI.FlexibleSpace();

                    GUI.enabled = _itemPage < pageCount - 1;
                    if (UI.Button("▶").Btn().Draw(GUILayout.Width(40)))
                    {
                        _itemPage++;
                        _selectedItem = -1;
                        _itemScroll = Vector2.zero;
                    }
                    GUI.enabled = true;
                });
            }
        }
        
        private void LoadItems()
        {
            try
            {
                var balance = GameBalance.Me;
                if (balance?.itemDefs == null)
                    return;

                _itemDefs = balance.itemDefs;
                _itemCategoryIds.Clear();
                _itemCategoryIds.Add("");

                var groups = new HashSet<string>();
                foreach (var def in _itemDefs)
                {
                    if (def?.itemGroupIds == null)
                        continue;
                    foreach (var group in def.itemGroupIds)
                    {
                        if (string.IsNullOrEmpty(group))
                            continue;
                        if (IsHiddenGroup(group))
                            continue;
                        groups.Add(group);
                    }
                }

                _itemCategoryIds.AddRange(groups.OrderBy(g => g));
                ApplyItemFilter();
            }
            catch (Exception e)
            {
                Log.Error($"[SMR2Mod] 加载物品列表失败: {e}");
            }
        }

        private void ApplyItemFilter()
        {
            IEnumerable<ItemDef> query = _itemDefs ?? new List<ItemDef>();

            if (_itemCategoryIndex > 0 && _itemCategoryIndex < _itemCategoryIds.Count)
            {
                var cat = _itemCategoryIds[_itemCategoryIndex];
                query = query.Where(d => d?.itemGroupIds != null && d.itemGroupIds.Contains(cat));
            }

            if (!string.IsNullOrEmpty(_itemSearch))
                query = query.Where(d => d != null && GetItemName(d).IndexOf(_itemSearch, StringComparison.OrdinalIgnoreCase) >= 0);

            _filteredItems = query.ToList();
            _selectedItem = -1;
            _itemPage = 0;
            _itemScroll = Vector2.zero;
        }

        private void GiveSelectedItem()
        {
            if (_selectedItem < 0 || _selectedItem >= _filteredItems.Count)
                return;

            var count = 1;
            if (int.TryParse(_giveCountText, out var parsed) && parsed > 0)
                count = parsed;

            GiveItem(_filteredItems[_selectedItem], count);
        }

        private void GiveItem(ItemDef def, int count)
        {
            try
            {
                if (def == null)
                    return;

                // 大件物品（如 town_box 等）不能放入物品栏，生成在人物脚边
                if (def.itemSize == ItemSize.Big)
                {
                    DropItemOnGround(def.id, count);
                    return;
                }

                var playerData = MainGame.PlayerData;
                if (playerData?.inventory == null)
                    return;

                var item = new Item(def.id, count);
                playerData.inventory.AddItemToInventory(item, null, false);
                LazyAudio.PlayAndForget("coins_sound");
            }
            catch (Exception e)
            {
                Log.Error($"[SMR2Mod] 添加物品失败: {def?.id} x{count} - {e}");
            }
        }

        /// <summary>
        /// 将大件物品生成在玩家脚边（使用游戏自身的掉落系统与落点计算）
        /// </summary>
        private void DropItemOnGround(string id, int count)
        {
            var playerData = MainGame.PlayerData;
            var worldId = playerData?.currentGameSceneId;
            if (string.IsNullOrEmpty(worldId))
                return;

            var playerPos = playerData.position.Value;
            if (!SpecialPhysicsCastUtils.GetPlayerDropPosition(playerPos, playerData.Direction, out var dropPos))
                dropPos = playerPos;

            MainGame.Instance.dropSystem.DropItem(new Item(id, count), worldId, dropPos, null);
            LazyAudio.PlayAndForget("coins_sound");
        }

        private static string GetItemName(ItemDef def)
        {
            string name;
            try
            {
                name = def.GetHeader();
            }
            catch
            {
                name = def.id;
            }

            if (string.IsNullOrEmpty(name))
                name = def.id;

            var idx = name.LastIndexOf(':');
            if (idx > 0 && idx < name.Length - 1 && int.TryParse(name.Substring(idx + 1), out _))
                name = name.Substring(0, idx);

            return name;
        }

        private static readonly Dictionary<string, string> ItemGroupNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"u_bag", "基本物品"},
            {"alch_bag", "炼金相关"},
            {"buil_bag", "材料相关"},
            {"fish_bag", "鱼饵"},
            {"food_bag", "食物"},
            {"pot_bag", "战斗药水"},
            {"quest_bag", "任务物品"},
            {"bodypart", "身体部位"},
            {"burial_reward", "埋葬奖励"},
            {"crop", "叶子"},
            {"fertilizer", "肥料"},
            {"fishes", "鱼类"},
            {"fuel", "燃料"},
            {"gravebot", "墓地"},
            {"gravetop", "墓碑"},
            {"seed", "种子"},
            {"seedable", "可种植"},
            {"tool", "工具"},
            {"town_box", "城镇补给"},
            {"weapon", "武器"}
        };

        private static bool IsHiddenGroup(string group)
        {
            return group.StartsWith("gr_", StringComparison.OrdinalIgnoreCase)
                || group.Equals("body", StringComparison.OrdinalIgnoreCase)
                || group.StartsWith("battle_potion", StringComparison.OrdinalIgnoreCase)
                || group.StartsWith("overhead", StringComparison.OrdinalIgnoreCase)
                || group.Equals("mistake", StringComparison.OrdinalIgnoreCase)
                || group.Equals("melee", StringComparison.OrdinalIgnoreCase)
                || group.Equals("range", StringComparison.OrdinalIgnoreCase)
                || group.Equals("corpse", StringComparison.OrdinalIgnoreCase)
                || group.Equals("wild_zombie", StringComparison.OrdinalIgnoreCase)
                || group.Equals("zombie", StringComparison.OrdinalIgnoreCase)
                || group.Equals("vineyard_seed", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetGroupName(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Loc.Get("全部");

            var key = "itemgroup_" + id;
            var loc = Loc.Get(key);
            if (loc != key)
                return loc;

            return ItemGroupNames.TryGetValue(id, out var cn) ? cn : id;
        }

        private Texture2D GetItemIcon(ItemDef def)
        {
            if (def == null || string.IsNullOrEmpty(def.iconId))
                return null;

            if (_itemIcons.TryGetValue(def.iconId, out var cached))
                return cached;

            Texture2D tex = null;
            try
            {
                var collection = LazySingletonSO<EasySpritesCollection>.Instance;
                var sprite = collection?.GetSprite(def.iconId, null);
                if (sprite != null)
                    tex = SpriteToTexture(sprite);
            }
            catch
            {
                tex = null;
            }

            _itemIcons[def.iconId] = tex;
            return tex;
        }

        private static Texture2D SpriteToTexture(Sprite sprite)
        {
            var rect = sprite.textureRect;
            var w = (int)rect.width;
            var h = (int)rect.height;
            if (w <= 0 || h <= 0)
                return null;

            try
            {
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                tex.SetPixels(sprite.texture.GetPixels((int)rect.x, (int)rect.y, w, h));
                tex.Apply();
                tex.hideFlags = HideFlags.HideAndDontSave;
                return tex;
            }
            catch
            {
                try
                {
                    var rt = RenderTexture.GetTemporary(sprite.texture.width, sprite.texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                    Graphics.Blit(sprite.texture, rt);
                    var active = RenderTexture.active;
                    RenderTexture.active = rt;
                    var full = new Texture2D(sprite.texture.width, sprite.texture.height, TextureFormat.RGBA32, false);
                    full.ReadPixels(new Rect(0f, 0f, full.width, full.height), 0, 0);
                    full.Apply();
                    RenderTexture.active = active;
                    RenderTexture.ReleaseTemporary(rt);

                    var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                    tex.SetPixels(full.GetPixels((int)rect.x, (int)rect.y, w, h));
                    tex.Apply();
                    tex.hideFlags = HideFlags.HideAndDontSave;
                    return tex;
                }
                catch
                {
                    return null;
                }
            }
        }

        private static Texture2D PlaceholderTex()
        {
            if (_placeholderTex == null)
            {
                _placeholderTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                _placeholderTex.SetPixel(0, 0, new Color(0.2f, 0.22f, 0.26f, 1f));
                _placeholderTex.Apply();
                _placeholderTex.hideFlags = HideFlags.HideAndDontSave;
            }
            return _placeholderTex;
        }

        /// <summary>
        /// 底部
        /// </summary>
        private void DrawStatusBar()
        {
            UI.Divider(5);
            UI.Horizontal(() =>
            {
                UI.Button($"{Loc.Get("缩放")}: {Mathf.RoundToInt(UI.WindowControls.Scale * 100f)}%  {Loc.Get("按")}{BaseConfig.Hotkey.GetDisplayName()}{Loc.Get("键显示/隐藏")}")
                    .Label().OnClick(() =>
                    {
                        _editingHotkey = BaseConfig.Hotkey.Clone();
                        _hotkeyWindow.Show();
                    }).Style(DarkSkin.SMuted).Draw(GUILayout.Width(220));
                UI.FlexibleSpace();
                
                UI.Button(Loc.Get("点赞数:") + HttpGet.Count).Label().OnClick(() =>
                {
                    HttpGet.TryHit(this);
                }).Style(DarkSkin.SHint).Draw(GUILayout.Width(100));
            
            });
        }
        /// <summary>
        /// 自定义快捷键窗体
        /// </summary>
        private void DrawHotkeyWindow()
        {
            UI.Vertical(() =>
            {
                UI.Space(8);

                UI.Label($"{Loc.Get("当前热键")}: {(_capturingHotkey ? Loc.Get("请按下按键...") : _editingHotkey.GetDisplayName())}")
                    .AsMuted()
                    .Draw();

                UI.Space(8);

                if (UI.Button(_capturingHotkey ? Loc.Get("取消捕获") : Loc.Get("按下新按键")).Draw())
                {
                    _capturingHotkey = !_capturingHotkey;
                }

                UI.Space(12);
                UI.Label(Loc.Get("修饰键")).AsMuted().Draw();

                UI.Horizontal(() =>
                {
                    _editingHotkey.Ctrl = UI.Toggle("Ctrl").Value(_editingHotkey.Ctrl).Draw();
                    _editingHotkey.Alt = UI.Toggle("Alt").Value(_editingHotkey.Alt).Draw();
                    _editingHotkey.Shift = UI.Toggle("Shift").Value(_editingHotkey.Shift).Draw();
                });

                UI.Space(16);
                UI.Horizontal(() =>
                {
                    UI.FlexibleSpace();
                    if (UI.Button(Loc.Get("确定")).Add().Draw())
                    {
                        BaseConfig.Hotkey = _editingHotkey.Clone();
                        BaseConfig.Save();
                        _hotkeyWindow.Hide();
                    }
                    UI.Space();
                    if (UI.Button(Loc.Get("取消")).Draw())
                    {
                        _capturingHotkey = false;
                        _hotkeyWindow.Hide();
                    }
                    UI.FlexibleSpace();
                });
            });
        }
    }
}