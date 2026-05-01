using MelonLoader;
using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(SmartTrade.Plugin), "SmartTrade", "1.7.4", "Can")]
[assembly: MelonGame("TppStudio", "LongYinLiZhiZhuan")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace SmartTrade
{
    public class Plugin : MelonMod
    {
        public static Plugin Instance;
        public static MelonLogger.Instance LOG => Instance.LoggerInstance;
        private MelonPreferences_Category _mainCategory= null!;
        public bool RedQuality; // 所有物品品质为红
        public bool GoodTreasure; // 珍宝品质修改当前等级全红
        
        #region 配置

        // 卖出百分比
        public static float SellRate;
        // 地区买卖百分比
        public static float AreaRate;
        // 记录物品，物品买入价格
        public static List<PurchaseItem> PurchaseItems = new();
        // 当前英雄口才值
        public static float KouCai;
        public static List<TableListEntity> TableDatas = new();
        public static bool SellHighQuality = true;
        // 身上珍宝列表
        public static List<ItemData> PlayerTreasures = new();

        #endregion

        #region UI相关

        private bool _showWindow;
        private GameObject _canvasRoot;
        private GameObject _uiRoot;
        private RectTransform _panelRect;
        private RectTransform _titleRect;
        private RectTransform _contentRect;
        private Font _uiFont;
        private Texture2D _whiteTexture;
        private bool _windowDragging;
        private Vector3 _windowDragMouseStart;
        private Vector2 _windowDragStartPos;
        private bool _lastMouseButtonState;
        
        private Toggle _toggleAll;
        private Toggle _toggleUp;
        private Toggle _toggleDown;
        private Toggle _sellHighQualityToggle;
        private Button _refreshButton;
        private Text _countLabel;
        private GameObject _listContent;
        private ScrollRect _scrollRect;
        
        private int _currentFilter;
        private bool _sortByIncome = true;
        private readonly List<GameObject> _listItems = new();
        private readonly List<float> _listItemHeights = new();  // 每行的间距（含间距）

        #endregion

        public override void OnInitializeMelon()
        {
            Instance = this;
            LOG.Msg("[SmartTrade] 初始化完成, 按~显示/隐藏窗体");
            
            _mainCategory = MelonPreferences.CreateCategory("LYModConfig", "功能配置");
            _mainCategory.SetFilePath(MelonEnvironment.UserDataDirectory + "\\LYModConfig.cfg");
            RedQuality = _mainCategory.GetEntry<bool>("RedQuality") != null && _mainCategory.GetEntry<bool>("RedQuality").Value;
            GoodTreasure = _mainCategory.GetEntry<bool>("GoodTreasure") != null && _mainCategory.GetEntry<bool>("GoodTreasure").Value;
        }

        public override void OnUpdate()
        {
            TradeFuncModule.HandleTradeButtons();
            
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                ToggleWindow();
            }
            
            bool currentMouseButton = Input.GetMouseButton(0);
            bool mouseJustReleased = _lastMouseButtonState && !currentMouseButton;
            _lastMouseButtonState = currentMouseButton;
            
            if (mouseJustReleased)
            {
                _windowDragging = false;
            }
            
            if (_showWindow)
            {
                HandleInput();
            }
        }

        private void ToggleWindow()
        {
            _showWindow = !_showWindow;
            
            if (_showWindow)
            {
                EnsureUGUI();
                if (_canvasRoot != null)
                    _canvasRoot.SetActive(true);
                TradeFuncModule.RefreshData();
                TradeFuncModule.ScanPlayerTreasures();
                UpdatePurchasedCount();
                RefreshList();
            }
            else
            {
                _windowDragging = false;
                if (_canvasRoot != null)
                    _canvasRoot.SetActive(false);
            }
        }

        private Texture2D GetWhiteTexture()
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }
            return _whiteTexture;
        }

        private void EnsureUGUI()
        {
            if (_canvasRoot != null) return;
            
            try
            {
                if (_uiRoot == null)
                {
                    _uiRoot = new GameObject("SmartTradeMod_Root");
                    Object.DontDestroyOnLoad(_uiRoot);
                }
                
                _canvasRoot = new GameObject("SmartTradeMod_Canvas");
                _canvasRoot.transform.SetParent(_uiRoot.transform, false);
                
                var canvas = _canvasRoot.AddComponent(Il2CppType.Of<Canvas>()).TryCast<Canvas>();
                if (canvas == null) return;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 32000;
                
                _canvasRoot.AddComponent(Il2CppType.Of<CanvasScaler>());
                _canvasRoot.AddComponent(Il2CppType.Of<GraphicRaycaster>());
                
                var panelObj = CreateUIObject("Panel", _canvasRoot.transform);
                var rawImage = panelObj.AddComponent(Il2CppType.Of<RawImage>()).TryCast<RawImage>();
                if (rawImage != null)
                {
                    rawImage.texture = GetWhiteTexture();
                    rawImage.color = new Color(0.12f, 0.12f, 0.14f, 0.98f);
                    rawImage.raycastTarget = true;
                }
                
                _panelRect = panelObj.GetComponent<RectTransform>();
                if (_panelRect == null) return;
                _panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                _panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                _panelRect.sizeDelta = new Vector2(540f, 600f);
                _panelRect.anchoredPosition = new Vector2(200f, 0f);
                
                CreateTitle();
                CreateToolbar();
                CreateContentArea();
                LOG.Msg("[SmartTrade] UGUI创建完成");
            }
            catch (Exception ex)
            {
                LOG.Error("[SmartTrade] EnsureUGUI Error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
        
        private void CreateTitle()
        {
            var titleObj = CreateUIObject("Title", _panelRect.transform);
            
            var titleBgComp = titleObj.AddComponent(Il2CppType.Of<RawImage>());
            var titleBg = titleBgComp?.TryCast<RawImage>();
            if (titleBg != null)
            {
                titleBg.texture = GetWhiteTexture();
                titleBg.color = new Color(0.18f, 0.35f, 0.55f, 1f);
                titleBg.raycastTarget = true;
            }
            
            var textObj = CreateUIObject("Text", titleObj.transform);
            var textComp = textObj.AddComponent(Il2CppType.Of<Text>());
            var text = textComp?.TryCast<Text>();
            if (text != null)
            {
                var font = GetFont();
                if (font != null)
                    text.font = font;
                text.text = "智能交易助手";
                text.fontSize = 20;
                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white;
            }
            
            var textRect = textObj.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = new Vector2(0f, 0f);
                textRect.anchorMax = new Vector2(1f, 1f);
                textRect.sizeDelta = new Vector2(0f, 0f);
            }
            
            _titleRect = titleObj.GetComponent<RectTransform>();
            if (_titleRect != null)
            {
                _titleRect.anchorMin = new Vector2(0f, 1f);
                _titleRect.anchorMax = new Vector2(1f, 1f);
                _titleRect.pivot = new Vector2(0.5f, 1f);
                _titleRect.anchoredPosition = new Vector2(0f, 0f);
                _titleRect.sizeDelta = new Vector2(0f, 40f);
            }
        }
        
        private void CreateToolbar()
        {
            var toolbar1Obj = CreateUIObject("Toolbar1", _panelRect.transform);
            
            var toolbar1BgComp = toolbar1Obj.AddComponent(Il2CppType.Of<RawImage>());
            var toolbar1Bg = toolbar1BgComp?.TryCast<RawImage>();
            if (toolbar1Bg != null)
            {
                toolbar1Bg.texture = GetWhiteTexture();
                toolbar1Bg.color = new Color(0.15f, 0.15f, 0.18f, 1f);
                toolbar1Bg.raycastTarget = true;
            }
            
            var toolbar1Rect = toolbar1Obj.GetComponent<RectTransform>();
            if (toolbar1Rect == null) return;
            
            toolbar1Rect.anchorMin = new Vector2(0f, 1f);
            toolbar1Rect.anchorMax = new Vector2(1f, 1f);
            toolbar1Rect.pivot = new Vector2(0.5f, 1f);
            toolbar1Rect.anchoredPosition = new Vector2(0f, -45f);
            toolbar1Rect.sizeDelta = new Vector2(-16f, 35f);
            
            #region 刷新数据按钮
            var refreshBtnObj = CreateUIObject("RefreshBtn", toolbar1Rect);
            
            var refreshBgComp = refreshBtnObj.AddComponent(Il2CppType.Of<RawImage>());
            var refreshBg = refreshBgComp?.TryCast<RawImage>();
            if (refreshBg != null)
            {
                refreshBg.texture = GetWhiteTexture();
                refreshBg.color = new Color(0.25f, 0.55f, 0.35f, 1f);
                refreshBg.raycastTarget = true;
            }
            
            var refreshTextObj = CreateUIObject("Text", refreshBtnObj.transform);
            var refreshTextComp = refreshTextObj.AddComponent(Il2CppType.Of<Text>());
            var refreshText = refreshTextComp?.TryCast<Text>();
            if (refreshText != null)
            {
                var font = GetFont();
                if (font != null)
                    refreshText.font = font;
                refreshText.text = "刷新数据";
                refreshText.fontSize = 16;
                refreshText.fontStyle = FontStyle.Bold;
                refreshText.alignment = TextAnchor.MiddleCenter;
                refreshText.color = Color.white;
            }
            
            var textRect = refreshTextObj.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = new Vector2(0f, 0f);
                textRect.anchorMax = new Vector2(1f, 1f);
                textRect.sizeDelta = new Vector2(0f, 0f);
            }
            
            var refreshRect = refreshBtnObj.GetComponent<RectTransform>();
            if (refreshRect != null)
            {
                refreshRect.anchorMin = new Vector2(0f, 0.5f);
                refreshRect.anchorMax = new Vector2(0f, 0.5f);
                refreshRect.pivot = new Vector2(0f, 0.5f);
                refreshRect.anchoredPosition = new Vector2(8f, 0f);
                refreshRect.sizeDelta = new Vector2(75f, 28f);
            }
            
            var refreshBtnComp = refreshBtnObj.AddComponent(Il2CppType.Of<Button>());
            _refreshButton = refreshBtnComp?.TryCast<Button>();
            if (_refreshButton != null)
            {
                _refreshButton.onClick.AddListener(new Action(RefreshList));
            }
            #endregion

            #region 清空数据按钮
            var clearBtnObj = CreateUIObject("ClearBtn", toolbar1Rect);
            
            var clearBgComp = clearBtnObj.AddComponent(Il2CppType.Of<RawImage>());
            var clearBg = clearBgComp?.TryCast<RawImage>();
            if (clearBg != null)
            {
                clearBg.texture = GetWhiteTexture();
                clearBg.color = new Color(0.6f, 0.25f, 0.25f, 1f);
                clearBg.raycastTarget = true;
            }
            
            var clearTextObj = CreateUIObject("Text", clearBtnObj.transform);
            var clearTextComp = clearTextObj.AddComponent(Il2CppType.Of<Text>());
            var clearText = clearTextComp?.TryCast<Text>();
            if (clearText != null)
            {
                var font = GetFont();
                if (font != null)
                    clearText.font = font;
                clearText.text = "清空数据";
                clearText.fontSize = 16;
                clearText.fontStyle = FontStyle.Bold;
                clearText.alignment = TextAnchor.MiddleCenter;
                clearText.color = Color.white;
            }
            
            var clearTextRect = clearTextObj.GetComponent<RectTransform>();
            if (clearTextRect != null)
            {
                clearTextRect.anchorMin = new Vector2(0f, 0f);
                clearTextRect.anchorMax = new Vector2(1f, 1f);
                clearTextRect.sizeDelta = new Vector2(0f, 0f);
            }
            
            var clearRect = clearBtnObj.GetComponent<RectTransform>();
            if (clearRect != null)
            {
                clearRect.anchorMin = new Vector2(0f, 0.5f);
                clearRect.anchorMax = new Vector2(0f, 0.5f);
                clearRect.pivot = new Vector2(0f, 0.5f);
                clearRect.anchoredPosition = new Vector2(90f, 0f);
                clearRect.sizeDelta = new Vector2(75f, 28f);
            }
            
            var clearBtnComp = clearBtnObj.AddComponent(Il2CppType.Of<Button>());
            var clearButton = clearBtnComp?.TryCast<Button>();
            if (clearButton != null)
            {
                clearButton.onClick.AddListener(new Action(ClearPurchaseData));
            }
            #endregion

            #region 售卖精良以上Toggle
            _sellHighQualityToggle = CreateToggle(toolbar1Rect, "☑售卖精良以上", -50, 120f);
            if (_sellHighQualityToggle != null)
            {
                _sellHighQualityToggle.isOn = true;
    
                _sellHighQualityToggle.onValueChanged.AddListener(new Action<bool>(isOn => 
                { 
                    SellHighQuality = isOn; 
                    // 动态修改label文本
                    var label = _sellHighQualityToggle.GetComponentInChildren<Text>();
                    if (label != null)
                    {
                        label.text = isOn ? "☑售卖精良以上" : "☐售卖精良以上";
                    }
                }));
            }
            #endregion

            #region 已购入标签
            var countLabelObj = CreateUIObject("CountLabel", toolbar1Rect);
            var countLabelComp = countLabelObj.AddComponent(Il2CppType.Of<Text>());
            _countLabel = countLabelComp?.TryCast<Text>();
            if (_countLabel != null)
            {
                var font = GetFont();
                if (font != null)
                    _countLabel.font = font;
                _countLabel.text = "拥有：0";
                _countLabel.fontSize = 16;
                _countLabel.fontStyle = FontStyle.Bold;
                _countLabel.alignment = TextAnchor.MiddleRight;
                _countLabel.color = new Color(1f, 0.85f, 0.2f);
            }

            var countLabelRect = countLabelObj.GetComponent<RectTransform>();
            if (countLabelRect != null)
            {
                countLabelRect.anchorMin = new Vector2(1f, 0.5f);
                countLabelRect.anchorMax = new Vector2(1f, 0.5f);
                countLabelRect.pivot = new Vector2(1f, 0.5f);
                countLabelRect.anchoredPosition = new Vector2(-8f, 0f);
                countLabelRect.sizeDelta = new Vector2(240f, 28f);
            }
            #endregion

            #region 筛选工具栏
            var toolbar2Obj = CreateUIObject("Toolbar2", _panelRect.transform);
            
            var toolbar2BgComp = toolbar2Obj.AddComponent(Il2CppType.Of<RawImage>());
            var toolbar2Bg = toolbar2BgComp?.TryCast<RawImage>();
            if (toolbar2Bg != null)
            {
                toolbar2Bg.texture = GetWhiteTexture();
                toolbar2Bg.color = new Color(0.1f, 0.1f, 0.12f, 1f);
                toolbar2Bg.raycastTarget = true;
            }
            
            var toolbar2Rect = toolbar2Obj.GetComponent<RectTransform>();
            if (toolbar2Rect == null) return;
            
            toolbar2Rect.anchorMin = new Vector2(0f, 1f);
            toolbar2Rect.anchorMax = new Vector2(1f, 1f);
            toolbar2Rect.pivot = new Vector2(0.5f, 1f);
            toolbar2Rect.anchoredPosition = new Vector2(0f, -85f);
            toolbar2Rect.sizeDelta = new Vector2(-16f, 35f);
            
            float toggleWidth = 90f;
            float startX = -140f;
            
            _toggleAll = CreateToggle(toolbar2Rect, "全部", startX, toggleWidth);
            _toggleUp = CreateToggle(toolbar2Rect, "涨价", startX + toggleWidth + 15f, toggleWidth);
            _toggleDown = CreateToggle(toolbar2Rect, "降价", startX + (toggleWidth + 15f) * 2, toggleWidth);
            
            if (_toggleAll != null)
            {
                _toggleAll.isOn = true;
                _toggleAll.onValueChanged.AddListener(new Action<bool>(isOn => { if (isOn) SetFilter(0); }));
            }
            
            if (_toggleUp != null)
            {
                _toggleUp.onValueChanged.AddListener(new Action<bool>(isOn => { if (isOn) SetFilter(1); }));
            }
            
            if (_toggleDown != null)
            {
                _toggleDown.onValueChanged.AddListener(new Action<bool>(isOn => { if (isOn) SetFilter(2); }));
            }
            #endregion
        }
        
        private Toggle CreateToggle(Transform parent, string label, float posX, float width)
        {
            var toggleObj = CreateUIObject("Toggle_" + label, parent);
            
            var toggleComp = toggleObj.AddComponent(Il2CppType.Of<Toggle>());
            var toggle = toggleComp?.TryCast<Toggle>();
            
            var bgComp = toggleObj.AddComponent(Il2CppType.Of<RawImage>());
            var bg = bgComp?.TryCast<RawImage>();
            if (bg != null)
            {
                bg.texture = GetWhiteTexture();
                bg.color = new Color(0.25f, 0.25f, 0.28f, 1f);
            }
            
            var checkObj = CreateUIObject("Checkmark", toggleObj.transform);
            var checkBgComp = checkObj.AddComponent(Il2CppType.Of<RawImage>());
            var checkBg = checkBgComp?.TryCast<RawImage>();
            if (checkBg != null)
            {
                checkBg.texture = GetWhiteTexture();
                checkBg.color = new Color(0.2f, 0.6f, 0.4f, 1f);
            }
            
            var checkRect = checkObj.GetComponent<RectTransform>();
            if (checkRect != null)
            {
                checkRect.anchorMin = new Vector2(0f, 0f);
                checkRect.anchorMax = new Vector2(1f, 1f);
                checkRect.sizeDelta = new Vector2(-3f, -3f);
                checkRect.anchoredPosition = new Vector2(0f, 0f);
            }
            
            if (toggle != null && checkBg != null)
            {
                toggle.graphic = checkBg;
                toggle.group = null;
            }
            
            var labelObj = CreateUIObject("Label", toggleObj.transform);
            var labelTextComp = labelObj.AddComponent(Il2CppType.Of<Text>());
            var labelText = labelTextComp?.TryCast<Text>();
            if (labelText != null)
            {
                var font = GetFont();
                if (font != null)
                    labelText.font = font;
                labelText.text = label;
                labelText.fontSize = 16;
                labelText.fontStyle = FontStyle.Bold;
                labelText.alignment = TextAnchor.MiddleCenter;
                labelText.color = Color.white;
            }
            
            var labelRect = labelObj.GetComponent<RectTransform>();
            if (labelRect != null)
            {
                labelRect.anchorMin = new Vector2(0f, 0f);
                labelRect.anchorMax = new Vector2(1f, 1f);
                labelRect.sizeDelta = new Vector2(0f, 0f);
            }
            
            var toggleRect = toggleObj.GetComponent<RectTransform>();
            if (toggleRect != null)
            {
                toggleRect.anchorMin = new Vector2(0.5f, 0.5f);
                toggleRect.anchorMax = new Vector2(0.5f, 0.5f);
                toggleRect.pivot = new Vector2(0f, 0.5f);
                toggleRect.anchoredPosition = new Vector2(posX, 0f);
                toggleRect.sizeDelta = new Vector2(width, 28f);
            }
            
            return toggle;
        }
        
        private void SetFilter(int filter)
        {
            _currentFilter = filter;
            
            if (_toggleAll != null)
                _toggleAll.SetIsOnWithoutNotify(filter == 0);
            if (_toggleUp != null)
                _toggleUp.SetIsOnWithoutNotify(filter == 1);
            if (_toggleDown != null)
                _toggleDown.SetIsOnWithoutNotify(filter == 2);
            
            RefreshList();
        }
        
        private void CreateContentArea()
        {
            var contentObj = CreateUIObject("Content", _panelRect.transform);
            
            var contentBgComp = contentObj.AddComponent(Il2CppType.Of<RawImage>());
            var contentBg = contentBgComp?.TryCast<RawImage>();
            if (contentBg != null)
            {
                contentBg.texture = GetWhiteTexture();
                contentBg.color = new Color(0.08f, 0.08f, 0.1f, 1f);
            }
            
            _contentRect = contentObj.GetComponent<RectTransform>();
            if (_contentRect != null)
            {
                _contentRect.anchorMin = new Vector2(0f, 0f);
                _contentRect.anchorMax = new Vector2(1f, 1f);
                _contentRect.pivot = new Vector2(0.5f, 0.5f);
                _contentRect.anchoredPosition = new Vector2(0f, -58.5f);
                _contentRect.sizeDelta = new Vector2(-16f, -182f);
            }
            
            CreateTableHeader(contentObj.transform);
            CreateScrollView(contentObj.transform);
        }
        
        private void CreateTableHeader(Transform parent)
        {
            var headerObj = CreateUIObject("Header", parent);
            
            var headerBgComp = headerObj.AddComponent(Il2CppType.Of<RawImage>());
            var headerBg = headerBgComp?.TryCast<RawImage>();
            if (headerBg != null)
            {
                headerBg.texture = GetWhiteTexture();
                headerBg.color = new Color(0.2f, 0.25f, 0.35f, 1f);
            }
            
            var headerRect = headerObj.GetComponent<RectTransform>();
            if (headerRect != null)
            {
                headerRect.anchorMin = new Vector2(0f, 1f);
                headerRect.anchorMax = new Vector2(1f, 1f);
                headerRect.pivot = new Vector2(0.5f, 1f);
                headerRect.anchoredPosition = new Vector2(0f, 0f);
                headerRect.sizeDelta = new Vector2(0f, 28f);
            }
            
            float[] widths = [80f, 80f, 65f, 80f, 55f];
            string[] labels = ["城市/村庄", "特价商品", "买卖价格", "预计收益", "去往"];
            float startX = -195f;
            
            for (int i = 0; i < labels.Length; i++)
            {
                var colObj = CreateUIObject("Col" + i, headerRect);
                var textComp = colObj.AddComponent(Il2CppType.Of<Text>());
                var text = textComp?.TryCast<Text>();
                if (text != null)
                {
                    var font = GetFont();
                    if (font != null)
                        text.font = font;
                    text.text = i == 3 ? "预计收益▼" : labels[i];
                    text.fontSize = 16;
                    text.fontStyle = FontStyle.Bold;
                    text.alignment = TextAnchor.MiddleCenter;
                    text.color = i == 3 ? new Color(1f, 0.85f, 0.2f) : new Color(0.9f, 0.9f, 0.95f);
                }
                
                if (i == 3)
                {
                    var btnComp = colObj.AddComponent(Il2CppType.Of<Button>());
                    var btn = btnComp?.TryCast<Button>();
                    if (btn != null)
                    {
                        btn.onClick.AddListener(new Action(ToggleSort));
                    }
                }
                
                var colRect = colObj.GetComponent<RectTransform>();
                if (colRect != null)
                {
                    colRect.anchorMin = new Vector2(0.5f, 0.5f);
                    colRect.anchorMax = new Vector2(0.5f, 0.5f);
                    colRect.pivot = new Vector2(0.5f, 0.5f);
                    float offsetX = -50f;  // 所有列都向左移动50px
                    colRect.anchoredPosition = new Vector2(startX + i * 98f + widths[i] / 2f + offsetX, 0f);
                    colRect.sizeDelta = new Vector2(widths[i], 26f);
                }
            }
        }
        
        private void ToggleSort()
        {
            _sortByIncome = !_sortByIncome;
            RefreshList();
        }
        
        private void CreateScrollView(Transform parent)
        {
            var scrollObj = CreateUIObject("ScrollView", parent);
            
            var scrollRectComp = scrollObj.AddComponent(Il2CppType.Of<ScrollRect>());
            _scrollRect = scrollRectComp?.TryCast<ScrollRect>();
            _scrollRect?.scrollSensitivity = 30f;
            
            var scrollBgComp = scrollObj.AddComponent(Il2CppType.Of<RawImage>());
            var scrollBg = scrollBgComp?.TryCast<RawImage>();
            if (scrollBg != null)
            {
                scrollBg.texture = GetWhiteTexture();
                scrollBg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            }
            
            var scrollRect = scrollObj.GetComponent<RectTransform>();
            if (scrollRect != null)
            {
                scrollRect.anchorMin = new Vector2(0f, 0f);
                scrollRect.anchorMax = new Vector2(1f, 1f);
                scrollRect.pivot = new Vector2(0.5f, 0.5f);
                scrollRect.anchoredPosition = new Vector2(0f, -12.5f);
                scrollRect.sizeDelta = new Vector2(0f, -25f);
            }
            
            var viewportObj = CreateUIObject("Viewport", scrollObj.transform);
            var maskComp = viewportObj.AddComponent(Il2CppType.Of<Mask>());
            var mask = maskComp?.TryCast<Mask>();
            if (mask != null)
            {
                mask.showMaskGraphic = false;
            }
            
            var viewportBgComp = viewportObj.AddComponent(Il2CppType.Of<RawImage>());
            var viewportBg = viewportBgComp?.TryCast<RawImage>();
            if (viewportBg != null)
            {
                viewportBg.texture = GetWhiteTexture();
                viewportBg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            }
            
            var viewportRect = viewportObj.GetComponent<RectTransform>();
            if (viewportRect != null)
            {
                viewportRect.anchorMin = new Vector2(0f, 0f);
                viewportRect.anchorMax = new Vector2(1f, 1f);
                viewportRect.pivot = new Vector2(0.5f, 0.5f);
                viewportRect.sizeDelta = new Vector2(0f, 0f);
            }
            
            _listContent = CreateUIObject("Content", viewportObj.transform);
            var contentRect = _listContent.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = new Vector2(1f, 1f);
                contentRect.pivot = new Vector2(0.5f, 1f);
                contentRect.anchoredPosition = new Vector2(0f, 0f);
                contentRect.sizeDelta = new Vector2(0f, 0f);
            }
            
            var scrollbarObj = CreateUIObject("Scrollbar", scrollObj.transform);
            var scrollbarComp = scrollbarObj.AddComponent(Il2CppType.Of<Scrollbar>());
            var scrollbar = scrollbarComp?.TryCast<Scrollbar>();
            
            var scrollbarBgComp = scrollbarObj.AddComponent(Il2CppType.Of<RawImage>());
            var scrollbarBg = scrollbarBgComp?.TryCast<RawImage>();
            if (scrollbarBg != null)
            {
                scrollbarBg.texture = GetWhiteTexture();
                scrollbarBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            }
            
            var scrollbarRect = scrollbarObj.GetComponent<RectTransform>();
            if (scrollbarRect != null)
            {
                scrollbarRect.anchorMin = new Vector2(1f, 0f);
                scrollbarRect.anchorMax = new Vector2(1f, 1f);
                scrollbarRect.pivot = new Vector2(1f, 0.5f);
                scrollbarRect.anchoredPosition = new Vector2(0f, 0f);
                scrollbarRect.sizeDelta = new Vector2(20f, 0f);
            }
            
            var handleObj = CreateUIObject("Handle", scrollbarObj.transform);
            var handleBgComp = handleObj.AddComponent(Il2CppType.Of<RawImage>());
            var handleBg = handleBgComp?.TryCast<RawImage>();
            if (handleBg != null)
            {
                handleBg.texture = GetWhiteTexture();
                handleBg.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            
            var handleRect = handleObj.GetComponent<RectTransform>();
            if (handleRect != null)
            {
                handleRect.anchorMin = new Vector2(0f, 0f);
                handleRect.anchorMax = new Vector2(1f, 1f);
                handleRect.sizeDelta = new Vector2(0f, 0f);
            }
            
            if (scrollbar != null)
            {
                scrollbar.direction = Scrollbar.Direction.BottomToTop;
                scrollbar.handleRect = handleRect;
                scrollbar.size = 0.5f;
            }
            
            if (_scrollRect != null)
            {
                _scrollRect.content = contentRect;
                _scrollRect.viewport = viewportRect;
                _scrollRect.verticalScrollbar = scrollbar;
                _scrollRect.horizontal = false;
                _scrollRect.vertical = true;
            }
        }

        private void AddListItem(TableListEntity entity)
        {
            if (_listContent == null || entity == null) return;
            
            // 根据特价商品数量决定行高：超过2个时换行，行高加倍
            int treasureCount = entity.TreasurePriceInfo?.Count ?? 0;
            bool needWrap = treasureCount > 2;
            float rowHeight = needWrap ? 48f : 28f;  // 换行时行高48f，否则28f
            float rowSpacing = needWrap ? 50f : 30f;  // 换行时行间距50f，否则30f
            
            var rowObj = CreateUIObject("Row_" + _listItems.Count, _listContent.transform);
            
            var rowBgComp = rowObj.AddComponent(Il2CppType.Of<RawImage>());
            var rowBg = rowBgComp?.TryCast<RawImage>();
            if (rowBg != null)
            {
                rowBg.texture = GetWhiteTexture();
                rowBg.color = _listItems.Count % 2 == 0 
                    ? new Color(0.12f, 0.12f, 0.14f, 1f) 
                    : new Color(0.16f, 0.16f, 0.18f, 1f);
            }
            
            // 计算当前行的Y偏移（累加之前所有行的高度）
            float yOffset = 0f;
            foreach (var h in _listItemHeights)
            {
                yOffset += h;
            }
            
            var rowRect = rowObj.GetComponent<RectTransform>();
            if (rowRect != null)
            {
                rowRect.anchorMin = new Vector2(0f, 1f);
                rowRect.anchorMax = new Vector2(1f, 1f);
                rowRect.pivot = new Vector2(0.5f, 1f);
                rowRect.anchoredPosition = new Vector2(0f, -yOffset);
                rowRect.sizeDelta = new Vector2(-10f, rowHeight);
            }
            
            float[] widths = [100f, 80f, 65f, 80f, 55f];
            string[] values =
            [
                entity.AreaName ?? "", 
                entity.SpeTreasureTypeName ?? "", 
                $"{entity.AreaSpePriceRate:P0}", 
                entity.Income > 0 ? $"+{entity.Income:F0}" : entity.Income.ToString("F0"), 
                ""
            ];
            var startX = -195f;
            
            for (int i = 0; i < values.Length; i++)
            {
                if (i == 4)
                {
                    var btnObj = CreateUIObject("GoBtn", rowRect);
                    var btnBgComp = btnObj.AddComponent(Il2CppType.Of<RawImage>());
                    var btnBg = btnBgComp?.TryCast<RawImage>();
                    if (btnBg != null)
                    {
                        btnBg.texture = GetWhiteTexture();
                        btnBg.color = new Color(0.25f, 0.5f, 0.65f, 1f);
                    }
                    
                    var btnTextObj = CreateUIObject("Text", btnObj.transform);
                    var btnTextComp = btnTextObj.AddComponent(Il2CppType.Of<Text>());
                    var btnText = btnTextComp?.TryCast<Text>();
                    if (btnText != null)
                    {
                        var font = GetFont();
                        if (font != null)
                            btnText.font = font;
                        btnText.text = "出发";
                        btnText.fontSize = 14;
                        btnText.fontStyle = FontStyle.Bold;
                        btnText.alignment = TextAnchor.MiddleCenter;
                        btnText.color = Color.white;
                    }
                    
                    var btnTextRect = btnTextObj.GetComponent<RectTransform>();
                    if (btnTextRect != null)
                    {
                        btnTextRect.anchorMin = new Vector2(0f, 0f);
                        btnTextRect.anchorMax = new Vector2(1f, 1f);
                        btnTextRect.sizeDelta = new Vector2(0f, 0f);
                    }
                    
                    var btnRect = btnObj.GetComponent<RectTransform>();
                    if (btnRect != null)
                    {
                        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
                        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
                        btnRect.pivot = new Vector2(0.5f, 0.5f);
                        float offsetX = -50f;  // 与其他列保持一致
                        btnRect.anchoredPosition = new Vector2(startX + i * 98f + widths[i] / 2f + offsetX, 0f);
                        btnRect.sizeDelta = new Vector2(50f, 26f);
                    }
                    
                    var btnComp = btnObj.AddComponent(Il2CppType.Of<Button>());
                    var btn = btnComp?.TryCast<Button>();
                    if (btn != null)
                    {
                        var areaId = entity.AreaId;
                        btn.onClick.AddListener(new Action(() => GoToArea(areaId)));
                    }
                }
                else
                {
                    var colObj = CreateUIObject("Col" + i, rowRect);
                    var textComp = colObj.AddComponent(Il2CppType.Of<Text>());
                    var text = textComp?.TryCast<Text>();
                    if (text != null)
                    {
                        var font = GetFont();
                        if (font != null)
                            text.font = font;
                        text.text = values[i];
                        text.fontSize = 14;
                        text.alignment = TextAnchor.MiddleCenter;
                        // 特价商品列：超过2个商品时启用换行
                        if (i == 1 && needWrap)
                        {
                            text.horizontalOverflow = HorizontalWrapMode.Wrap;
                            text.verticalOverflow = VerticalWrapMode.Overflow;
                        }
                        if (i == 3 && entity.Income > 0)
                            text.color = new Color(0.3f, 0.9f, 0.4f);
                        else if (i == 3 && entity.Income < 0)
                            text.color = new Color(0.9f, 0.3f, 0.3f);
                        else
                            text.color = new Color(0.85f, 0.85f, 0.9f);
                    }
                    
                    var colRect = colObj.GetComponent<RectTransform>();
                    if (colRect != null)
                    {
                        colRect.anchorMin = new Vector2(0.5f, 0.5f);
                        colRect.anchorMax = new Vector2(0.5f, 0.5f);
                        colRect.pivot = new Vector2(0.5f, 0.5f);
                        float offsetX = -50f;  // 所有列都向左移动50px
                        colRect.anchoredPosition = new Vector2(startX + i * 98f + widths[i] / 2f + offsetX, 0f);
                        colRect.sizeDelta = new Vector2(widths[i], rowHeight - 2f);
                    }
                }
            }
            
            _listItems.Add(rowObj);
            _listItemHeights.Add(rowSpacing);  // 记录每行的间距
            
            // 计算总内容高度
            float totalHeight = 0f;
            foreach (var h in _listItemHeights)
            {
                totalHeight += h;
            }
            var contentRect = _listContent.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                contentRect.sizeDelta = new Vector2(0f, totalHeight);
            }
        }
        
        private async void GoToArea(int areaId)
        {
            LOG.Msg($"[SmartTrade] 前往区域: {areaId}");
            
            if (!IsBigMapVisible())
            {
                _showWindow = false;
                if (_canvasRoot != null)
                    _canvasRoot.SetActive(false);
                
                var areaController = AreaController.Instance;
                if (areaController != null)
                {
                    areaController.PlayerLeaveArea();
                }
                else
                {
                    var gameController = GameController.Instance;
                    if (gameController != null)
                    {
                        gameController.PlayerLeaveArea();
                    }
                }
                
                while (!IsBigMapVisible())
                {
                    await System.Threading.Tasks.Task.Delay(500);
                }
                await System.Threading.Tasks.Task.Delay(1000);
            }
            
            var bigMapController = BigMapController.Instance;
            if (bigMapController != null)
            {
                bigMapController.SetPlayerMoveTargetArea(areaId);
            }
        }

        private void ClearList()
        {
            foreach (var item in _listItems)
            {
                if (item != null)
                    Object.Destroy(item);
            }
            _listItems.Clear();
            _listItemHeights.Clear();
            
            if (_listContent != null)
            {
                var contentRect = _listContent.GetComponent<RectTransform>();
                if (contentRect != null)
                {
                    contentRect.sizeDelta = new Vector2(0f, 0f);
                }
            }
        }
        
        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.AddComponent(Il2CppType.Of<RectTransform>());
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private Font GetFont()
        {
            if (_uiFont == null)
            {
                _uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (_uiFont == null)
                {
                    var fonts = Resources.FindObjectsOfTypeAll<Font>();
                    if (fonts != null && fonts.Length > 0)
                        _uiFont = fonts[0];
                }
            }
            if (_uiFont == null)
                LOG.Warning("[SmartTrade] 字体获取失败!");
            return _uiFont;
        }
        
        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var mousePos = Input.mousePosition;
                _windowDragging = false;
                
                if (_titleRect != null && RectTransformUtility.RectangleContainsScreenPoint(_titleRect, mousePos, null))
                {
                    _windowDragging = true;
                    _windowDragMouseStart = mousePos;
                    _windowDragStartPos = _panelRect.anchoredPosition;
                }
            }

            if (_windowDragging && _panelRect != null)
            {
                var mousePos = Input.mousePosition;
                var delta = mousePos - _windowDragMouseStart;
                _panelRect.anchoredPosition = _windowDragStartPos + new Vector2(delta.x, delta.y);
            }
        }
        
        private void ClearPurchaseData()
        {
            LOG.Msg("[SmartTrade] 清空已购数据");
            PurchaseItems.Clear();
            UpdatePurchasedCount();
            RefreshList();
        }
        
        private void RefreshList()
        {
            LOG.Msg($"[SmartTrade] 刷新列表 - 当前筛选: {_currentFilter}");
            
            TradeFuncModule.RefreshData();
            
            ClearList();
            
            var sortedDatas = TableDatas
                .Where(d => !(_currentFilter == 1 && !d.HasExpensive))
                .Where(d => !(_currentFilter == 2 && !d.HasCheap))
                .OrderByDescending(d => _sortByIncome ? d.Income : 0)
                .ThenByDescending(d => !_sortByIncome ? d.Income : 0)
                .ToList();
            
            foreach (var data in sortedDatas)
            {
                AddListItem(data);
            }
            
            UpdatePurchasedCount();
            UpdateSortHeader();
        }
        
        private void UpdateSortHeader()
        {
            if (_contentRect == null) return;
            var header = _contentRect.transform.Find("Header");
            if (header == null) return;
            var col3 = header.Find("Col3");
            if (col3 == null) return;
            var text = col3.GetComponent<Text>();
            if (text != null)
            {
                text.text = _sortByIncome ? "预计收益▼" : "预计收益▲";
            }
        }

        private void UpdatePurchasedCount()
        {
            if (_countLabel == null) return;
            var ownedCount = Plugin.PlayerTreasures?.Count ?? 0;
            _countLabel.text = $"拥有：{ownedCount}";
        }

        public static bool IsBigMapVisible()
        {
            var instance = BigMapController.Instance;
            if (instance == null) return false;
            var bigmapUIPanel = instance.bigmapUIPanel;
            if (bigmapUIPanel == null || !bigmapUIPanel.activeSelf) return false;
            return true;
        }

        public static bool IsDisableOperate()
        {
            return !Instance._showWindow;
        }
    }
}
