using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TMMod;

[BepInPlugin("tmmod", "TMMod", "1.0")]
public class TMMod : BaseUnityPlugin
{
    

    private readonly string[] tabNames = { "通用", "选手" };

    // 配置项
    private ConfigEntry<KeyCode> _hotkey;
    private ConfigEntry<bool> actNumChangeToggle;
    private ConfigEntry<bool> streamingSuccessToggle;
    private ConfigEntry<bool> specialTrainningSuccessToggle;
    private ConfigEntry<bool> testNumChangeToggle;
    private ConfigEntry<bool> trainingChangeToggle;
    private ConfigEntry<int> trainingNum;
    private ConfigEntry<bool> maxStatusToggle;



    // 数据
    private string cacheLable = "<size=12><color=red>数据未加载</color></size>";
    private TodayData todayData;
    private List<Athlete> _athletes = new();
    private Athlete _athlete;
    
    

    // GUI状态
    private Vector2 mainScrollPos;
    private Rect mainWindowRect = new(50, 50, 450, 600);


    // 标签页状态
    private int selectedTab;
    private bool showMainWindow = true;
    

    private void Start()
    {
        Logger.LogInfo("TMMod loaded!");

        Harmony.CreateAndPatchAll(typeof(TMMod));
        _hotkey = Config.Bind("Config", "hotkey", KeyCode.Tab, "开启/关闭窗体快捷键");
        testNumChangeToggle = Config.Bind("Data", "testNumChangeToggle", false, "是否开启修改组合模拟次数");
        actNumChangeToggle = Config.Bind("Data", "actNumChangeToggle", false, "是否开启修改周活动次数");
        trainingChangeToggle = Config.Bind("Data", "trainingChangeToggle", false, "是否开启修改训练积分");
        trainingNum = Config.Bind("Data", "trainingNum", 3, "训练积分");
        specialTrainningSuccessToggle = Config.Bind("Data", "specialTrainingSuccessToggle", false, "是否开启修改特殊训练必定成功");
        streamingSuccessToggle = Config.Bind("Data", "streamingSuccessToggle", false, "是否开启修改直播必定成功");
        maxStatusToggle = Config.Bind("Data", "maxStatusToggle", false, "是否开启保持选手最高状态");
    }

    private void Update()
    {
        // 按键切换GUI
        if (Input.GetKeyDown(_hotkey.Value)) showMainWindow = !showMainWindow;
        
        
        // 所有天赋
        // List<PropertyProb> propertyProbs = Store.Global.Config.PropertyTraining.Property.Properties;
        // 所有英雄
        // List<ChampionInfo> gameChampions = Store.Global.Config.Game.Champions;
        // 获取自己团队的所有选手信息
        // List<Athlete> athletes = Store.Global.Get<TeamInfo>(NetworkHandler.PlayerIndex).Athletes;
        // 按首发和后补排序选手信息
        // athletes = athletes.Where((Athlete a) => a.Belong == AthleteBelong.FirstTeam).ToList<Athlete>();
        
        // 装备
        // 31个 Headset
        // 37个 Controller
        // 29个 Chair
        // 32个 Uniform
        // 缺少装备的 int type
        //todayData.AddItem(0,1, true);
    }

    private void OnGUI()
    {
        // 开启 / 关闭窗体
        if (showMainWindow)
            // 主窗口
            mainWindowRect = GUI.Window(0, mainWindowRect, DrawMainWindow, "TMMod by Can");

        if (_loaded)
        {
            if (actNumChangeToggle.Value && todayData.RemainActivity < 1)
            {
                todayData.RemainActivity = 1;
            }

            if (testNumChangeToggle.Value && todayData.RemainSimulation < 1)
            {
                todayData.RemainSimulation = 1;
            }

            
            if (maxStatusToggle.Value)
            {
                foreach (var a in _athletes)
                {
                    if (a.Condition <= 30) a.AddCondition(50);
                }
            }
        }
    }

    /**
     * 主窗体绘制
     */
    private void DrawMainWindow(int windowId)
    {
        // 启用窗口拖拽（仅标题栏区域）
        GUI.DragWindow(new Rect(0, 0, Screen.width, 20));
        

        // 标签页选择
        GUILayout.BeginHorizontal();
        for (var i = 0; i < tabNames.Length; i++)
        {
            if (GUILayout.Toggle(selectedTab == i, tabNames[i], "Button")) selectedTab = i;
            GUILayout.Space(10);
        }

        GUILayout.EndHorizontal();


        // 滚动区域
        mainScrollPos = GUILayout.BeginScrollView(mainScrollPos, GUILayout.Width(430), GUILayout.Height(540));

        // 根据选择的标签页绘制不同内容
        switch (selectedTab)
        {
            case 0: // 通用
                DrawGeneralTab();
                break;
            case 1: // 选手
                DrawSquadTab();
                break;
        }

        GUILayout.EndScrollView();
    }

    /**
     * 通用界面绘制
     */
    private void DrawGeneralTab()
    {
        GUILayout.Label("<size=14><b>通用功能</b></size>", RichTextStyle());
      
        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("加载存档后点击加载数据", GUILayout.Width(50), GUILayout.ExpandWidth(true)))
        {
            todayData = Store.Global.Get<TodayData>(0);
            cacheLable = todayData != null
                ? "<size=12><color=green>数据加载成功</color></size>"
                : "<size=12><color=red>数据加载失败，请确认已加载存档</color></size>";
            if (trainingChangeToggle.Value)
                Store.Global.Get<TodayData>(NetworkHandler.PlayerIndex).Facility.TrainingLimit = trainingNum.Value;
            if (specialTrainningSuccessToggle.Value)
                Store.Global.Config.SpecialTraining.SuperProb = 1;
            _athletes = Store.Global.Get<TeamInfo>(NetworkHandler.PlayerIndex).Athletes;
            _loaded = true;
        }
        GUILayout.Space(10);
        GUILayout.Label(cacheLable, RichTextStyle());
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(10);
        
        
        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        maxStatusToggle.Value =  GUILayout.Toggle(maxStatusToggle.Value, "锁定选手全最高状态");
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(10);
        

        // 钱
        GUILayout.BeginVertical("Box");
        GUILayout.Label("<b>金币</b>", RichTextStyle());
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+1000金币", GUILayout.ExpandWidth(true))) todayData?.AddGold(1000);
        GUILayout.Space(10);
        if (GUILayout.Button("+5000金币",  GUILayout.ExpandWidth(true))) todayData?.AddGold(5000);
        GUILayout.Space(10);
        if (GUILayout.Button("+10000金币", GUILayout.ExpandWidth(true))) todayData?.AddGold(10000);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Space(10);

        // 材料
        GUILayout.BeginVertical("Box");
        GUILayout.Label("<b>材料</b>", RichTextStyle());

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+10材料",  GUILayout.ExpandWidth(true)))
            for (var i = 0; i < 4; i++)
                todayData?.AddMaterial(i, 10);

        GUILayout.Space(10);
        if (GUILayout.Button("+50材料", GUILayout.ExpandWidth(true)))
            for (var i = 0; i < 4; i++)
                todayData?.AddMaterial(i, 50);

        GUILayout.Space(10);
        if (GUILayout.Button("+100材料",  GUILayout.ExpandWidth(true)))
            for (var i = 0; i < 4; i++)
                todayData?.AddMaterial(i, 100);

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Space(10);

        // 其他可保存修改的项
        GUILayout.BeginVertical("Box");
        GUILayout.Label("<b>其他</b>", RichTextStyle());
        GUILayout.Space(10);

        // 组合测试次数
        GUILayout.BeginHorizontal();
        var testNumChangeToggleNow = GUILayout.Toggle(testNumChangeToggle.Value, " 是否锁定组合测试次数", GUILayout.ExpandWidth(true));
        if (testNumChangeToggleNow != testNumChangeToggle.Value)
        {
            testNumChangeToggle.Value = testNumChangeToggleNow;
            Config.Save();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10); 

        // 周活动次数
        GUILayout.BeginHorizontal();
        var actNumChangeToggleNow = GUILayout.Toggle(actNumChangeToggle.Value, " 是否锁定周活动次数", GUILayout.ExpandWidth(true));
        if (actNumChangeToggleNow !=  actNumChangeToggle.Value)
        {
            actNumChangeToggle.Value = actNumChangeToggleNow;
            Config.Save();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        // 训练积分
        GUILayout.BeginHorizontal();
        var trainingChangeToggleNow =
            GUILayout.Toggle(trainingChangeToggle.Value, " 是否修改训练积分", GUILayout.ExpandWidth(true));
        if (trainingChangeToggleNow != trainingChangeToggle.Value)
        {
            trainingChangeToggle.Value = trainingChangeToggleNow;
            Config.Save();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        float trainingNumF = trainingNum.Value;
        var trainingNumSliderValue = GUILayout.HorizontalSlider(trainingNumF, 3, 99);
        GUILayout.Label($"{trainingNumSliderValue:F0}", GUILayout.ExpandWidth(true));
        var trainingNumSliderValueInt = Mathf.RoundToInt(trainingNumSliderValue);
        if (trainingChangeToggleNow && trainingNumSliderValueInt != trainingNum.Value)
        {
            trainingNum.Value = trainingNumSliderValueInt;
            Store.Global.Get<TodayData>(NetworkHandler.PlayerIndex).Facility.TrainingLimit = trainingNumSliderValueInt;
            Config.Save();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        // 直播成功
        GUILayout.BeginHorizontal();
        var streamingSuccessToggleNow =
            GUILayout.Toggle(streamingSuccessToggle.Value, " 直播必定成功", GUILayout.ExpandWidth(true));
        if (streamingSuccessToggleNow != streamingSuccessToggle.Value)
        {
            streamingSuccessToggle.Value = streamingSuccessToggleNow;
            Config.Save();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        // 特训成功
        GUILayout.BeginHorizontal();
        var specialTrainingSuccessToggleNow = GUILayout.Toggle(specialTrainningSuccessToggle.Value, " 特训必定大成功");
        if (specialTrainingSuccessToggleNow != specialTrainningSuccessToggle.Value)
        {
            specialTrainningSuccessToggle.Value = specialTrainingSuccessToggleNow;
            Store.Global.Config.SpecialTraining.SuperProb = 1;
            Config.Save();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }


    /**
     * 选手信息界面绘制
     */
    private void DrawSquadTab()
    {
        GUILayout.Label("<size=14><b>选手</b></size>", RichTextStyle());
        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("重新获取数据", GUILayout.ExpandWidth(true)))
        {
            _athletes = Store.Global.Get<TeamInfo>(NetworkHandler.PlayerIndex).Athletes;
        }
        
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        
        GUILayout.BeginVertical("Box");
        
        foreach (var a in _athletes)
        {
            
            if (GUILayout.Button(new I18nString(a.Name, I18n.Info.Table).Build(), GUILayout.ExpandWidth(true)))
            {
                
                _athlete = a;
                _potential = a.Potential;
                _name = a.Name;
                _age = a.Age;
                _salary = a.Salary;
                _attack = a.Attack;
                _defence = a.Defence;
                _fan = a.Fan;
                _athleteProperties = _athlete.Properties;
                
                switch (_athleteProperties.Count)
                {
                    case 0:
                        _property1 = "";
                        _pIndex1 = -1;
                        _pIndex2 = -1;
                        _property2 = "";
                        _property3 = "";
                        _pIndex3 = -1;
                        break;
                    case 1:
                        _property1 = _athleteProperties[0].Name;
                        _pIndex1 = _keyProperty.FirstOrDefault(k => k.Value == _property1).Key;
                        _pIndex2 = -1;
                        _property2 = "";
                        _property3 = "";
                        _pIndex3 = -1;
                        break;
                    case 2:
                        _property1 = _athleteProperties[0].Name;
                        _pIndex1 = _keyProperty.FirstOrDefault(k => k.Value == _property1).Key;
                        _property2 = _athleteProperties[1].Name;
                        _pIndex2 = _keyProperty.FirstOrDefault(k => k.Value == _property2).Key;
                        _property3 = "";
                        _pIndex3 = -1;
                        break;
                    case 3:
                        _property1 = _athleteProperties[0].Name;
                        _pIndex1 = _keyProperty.FirstOrDefault(k => k.Value == _property1).Key;
                        _property2 = _athleteProperties[1].Name;
                        _pIndex2 = _keyProperty.FirstOrDefault(k => k.Value == _property2).Key;
                        _property3 = _athleteProperties[2].Name;
                        _pIndex3 = _keyProperty.FirstOrDefault(k => k.Value == _property3).Key;
                        break;
                }

                _championExps = a.GetExps();
                
                switch (_championExps.Count)
                {
                    case 0:
                        _champ1 = 0;
                        _champion1 = "";
                        _cIndex1 = -1;
                        _champ2 = 0;
                        _champion2 = "";
                        _cIndex2 = -1;
                        _champ3 = 0;
                        _champion3 = "";
                        _cIndex3 = -1;
                        _champ4 = 0;
                        _champion4 = "";
                        _cIndex4 = -1;
                        break;
                    case 1:
                        _champ1 = _championExps[0].Value.Champ;
                        _champion1 = _championExps[0].Name;
                        _cIndex1 = _championDict.FirstOrDefault(k => k.Value == _champion1).Key;
                        _champ2 = 0;
                        _champion2 = "";
                        _cIndex2 = -1;
                        _champ3 = 0;
                        _champion3 = "";
                        _cIndex3 = -1;
                        _champ4 = 0;
                        _champion4 = "";
                        _cIndex4 = -1;
                        break;
                    case 2:
                        _champ1 = _championExps[0].Value.Champ;
                        _champion1 = _championExps[0].Name;
                        _cIndex1 = _championDict.FirstOrDefault(k => k.Value == _champion1).Key;
                        _champ2 = _championExps[1].Value.Champ;
                        _champion2 = _championExps[1].Name;
                        _cIndex2 = _championDict.FirstOrDefault(k => k.Value == _champion2).Key;
                        _champ3 = 0;
                        _champion3 = "";
                        _cIndex3 = -1;
                        _champ4 = 0;
                        _champion4 = "";
                        _cIndex4 = -1;
                        break;
                    case 3:
                        _champ1 = _championExps[0].Value.Champ;
                        _champion1 = _championExps[0].Name;
                        _cIndex1 = _championDict.FirstOrDefault(k => k.Value == _champion1).Key;
                        _champ2 = _championExps[1].Value.Champ;
                        _champion2 = _championExps[1].Name;
                        _cIndex2 = _championDict.FirstOrDefault(k => k.Value == _champion2).Key;
                        _champ3 = _championExps[2].Value.Champ;
                        _champion3 = _championExps[2].Name;
                        _cIndex3 = _championDict.FirstOrDefault(k => k.Value == _champion3).Key;
                        _champ4 = 0;
                        _champion4 = "";
                        _cIndex4 = 0;
                        break;
                    case 4:
                        _champ1 = _championExps[0].Value.Champ;
                        _champion1 = _championExps[0].Name;
                        _cIndex1 = _championDict.FirstOrDefault(k => k.Value == _champion1).Key;
                        _champ2 = _championExps[1].Value.Champ;
                        _champion2 = _championExps[1].Name;
                        _cIndex2 = _championDict.FirstOrDefault(k => k.Value == _champion2).Key;
                        _champ3 = _championExps[2].Value.Champ;
                        _champion3 = _championExps[2].Name;
                        _cIndex3 = _championDict.FirstOrDefault(k => k.Value == _champion3).Key;
                        _champ4 = _championExps[3].Value.Champ;
                        _champion4 = _championExps[3].Name;
                        _cIndex4 = _championDict.FirstOrDefault(k => k.Value == _champion4).Key;
                        break;
                }
            }
            GUILayout.Space(3);
        }
        
        GUILayout.EndVertical();
        
        
        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        GUILayout.Label("选手信息", RichTextStyle());
        
        if (GUILayout.Button("修改信息"))
        {
            List<AthleteProperty> list = new();
            if (_pIndex1 >= 0) list.Add(AthleteProperty.Create(_keyProperty[_pIndex1]));
            if (_pIndex2 >= 0) list.Add(AthleteProperty.Create(_keyProperty[_pIndex2]));
            if (_pIndex3 >= 0) list.Add(AthleteProperty.Create(_keyProperty[_pIndex3]));
            
            _athlete.Age = _age;
            _athlete.Salary = _salary;
            _athlete.Potential = _potential;
            _athlete.Properties = list;

            if (_champion1 != _championDict.GetValueOrDefault(_cIndex1, ""))
                _athlete.ChangeChampionExp(_champion1,_championDict.GetValueOrDefault(_cIndex1),10000);
            if (_champion2 != _championDict.GetValueOrDefault(_cIndex2, ""))
                _athlete.ChangeChampionExp(_champion2,_championDict.GetValueOrDefault(_cIndex2),10000);
            if (_champion3 != _championDict.GetValueOrDefault(_cIndex3, ""))
                _athlete.ChangeChampionExp(_champion3,_championDict.GetValueOrDefault(_cIndex3),10000);
            if (_champion4 != _championDict.GetValueOrDefault(_cIndex4, ""))
                _athlete.ChangeChampionExp(_champion4,_championDict.GetValueOrDefault(_cIndex4),10000);
            
            Store.Global.Set<Athlete>(_athlete.ID, _athlete);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("姓名："+ new I18nString(_name, I18n.Info.Table).Build());
        GUILayout.Label("年龄：");
        _age = Convert.ToInt32(GUILayout.TextField(Convert.ToString(_age)));
        GUILayout.Label("资质(0-3)：");
        _potential = Convert.ToInt32(GUILayout.TextField(Convert.ToString(_potential)));
        GUILayout.Label("薪资：");
        _salary =  Convert.ToInt32(GUILayout.TextField(Convert.ToString(_salary)));
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        GUILayout.Label($"攻击：{_attack}");
        if (GUILayout.Button("+")) _athlete?.AddAttack(); 
        GUILayout.Space(10);
        GUILayout.Label($"防御：{_defence}");
        if (GUILayout.Button("+")) _athlete?.AddDefence();
        GUILayout.Space(10);
        GUILayout.Label($"粉丝：{_fan}");
        if (GUILayout.Button("+")) _athlete?.AddFans(true);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        int p1 = _pIndex1;
        string pn1 = _cnProperty.GetValueOrDefault(_keyProperty.GetValueOrDefault(p1, ""), "");
        GUILayout.Label($"天赋1: {pn1}");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        p1 = Mathf.RoundToInt(GUILayout.HorizontalSlider(p1, -1, 35));
        _pIndex1 = p1;
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        string pnd1 = _desProperty.GetValueOrDefault(_keyProperty.GetValueOrDefault(p1, ""), "");
        GUILayout.Label($"天赋描述: {pnd1}");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        int p2 = _pIndex2;
        string pn2 = _cnProperty.GetValueOrDefault(_keyProperty.GetValueOrDefault(p2, ""), "");
        GUILayout.Label($"天赋2: {pn2}");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        p2 = Mathf.RoundToInt(GUILayout.HorizontalSlider(p2, -1, 35));
        _pIndex2 = p2;
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        string pnd2 = _desProperty.GetValueOrDefault(_keyProperty.GetValueOrDefault(p2, ""), "");
        GUILayout.Label($"天赋描述: {pnd2}");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        int p3 = _pIndex3;
        string pn3 = _cnProperty.GetValueOrDefault(_keyProperty.GetValueOrDefault(p3, ""), "");
        GUILayout.Label($"天赋3: {pn3}");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        p3 = Mathf.RoundToInt(GUILayout.HorizontalSlider(p3, -1, 35));
        _pIndex3 = p3;
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        string pnd3 = _desProperty.GetValueOrDefault(_keyProperty.GetValueOrDefault(p3, ""), "");
        GUILayout.Label($"天赋描述: {pnd3}");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        
        
        GUILayout.BeginHorizontal();
        int c1 = _cIndex1;
        string cn1 = _championCnDict.GetValueOrDefault(_championDict.GetValueOrDefault(c1, ""), "");
        GUILayout.Label($"擅长英雄1: {cn1}");
        GUILayout.Space(10);
        GUILayout.Label($"熟练度: {_champ1}");
        if (GUILayout.Button("+")) _athlete?.AddExp(_championDict.GetValueOrDefault(c1, "")); 
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        c1 = Mathf.RoundToInt(GUILayout.HorizontalSlider(c1, -1, 39));
        _cIndex1 = c1;
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        int c2 = _cIndex2;
        string cn2 = _championCnDict.GetValueOrDefault(_championDict.GetValueOrDefault(c2, ""), "");
        GUILayout.Label($"擅长英雄2: {cn2}");
        GUILayout.Space(10);
        GUILayout.Label($"熟练度: {_champ2}");
        if (GUILayout.Button("+")) _athlete?.AddExp(_championDict.GetValueOrDefault(c2, "")); 
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        c2 = Mathf.RoundToInt(GUILayout.HorizontalSlider(c2, -1, 39));
        _cIndex2 = c2;
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        int c3 = _cIndex3;
        string cn3 = _championCnDict.GetValueOrDefault(_championDict.GetValueOrDefault(c3, ""), "");
        GUILayout.Label($"擅长英雄3: {cn3}");
        GUILayout.Space(10);
        GUILayout.Label($"熟练度: {_champ3}");
        if (GUILayout.Button("+")) _athlete?.AddExp(_championDict.GetValueOrDefault(c3, "")); 
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        c3 = Mathf.RoundToInt(GUILayout.HorizontalSlider(c3, -1, 39));
        _cIndex3 = c3;
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        int c4 = _cIndex4;
        string cn4 = _championCnDict.GetValueOrDefault(_championDict.GetValueOrDefault(c4, ""), "");
        GUILayout.Label($"擅长英雄4: {cn4}");
        GUILayout.Space(10);
        GUILayout.Label($"熟练度: {_champ4}");
        if (GUILayout.Button("+")) _athlete?.AddExp(_championDict.GetValueOrDefault(c4, "")); 
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        c4 = Mathf.RoundToInt(GUILayout.HorizontalSlider(c4, -1, 39));
        _cIndex4 = c4;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        
    }
    

    private string _name = "";
    private int _age = 0;
    private int _salary = 0;
    private int _attack = 0;
    private int _defence = 0;
    private int _fan = 0;
    private int _potential = 0;
    private List<AthleteProperty> _athleteProperties = new();
    private string _property1 = "";
    private string _property2 = "";
    private string _property3 = "";
    private int _pIndex1 = -1;
    private int _pIndex2 = -1;
    private int _pIndex3 = -1;
    private List<ChampionExp> _championExps= new ();
    private string _champion1 = "";
    private string _champion2 = "";
    private string _champion3 = "";
    private string _champion4 = "";
    private int _champ1 = 0;
    private int _champ2 = 0;
    private int _champ3 = 0;
    private int _champ4 = 0;
    private int _cIndex1 = -1;
    private int _cIndex2 = -1;
    private int _cIndex3 = -1;
    private int _cIndex4 = -1;


    
    // 自定义style
    private GUIStyle RichTextStyle()
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            richText = true
        };
        return style;
    }


    /**
     * 直播成功
     */
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StreamingHandler), nameof(StreamingHandler.CreateResult))]
    public static bool StreamingHandler_CreateResult(int athlete, ref int prob)
    {
        // 获取插件实例
        var instance = BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<TMMod>();
        if (instance != null && instance.streamingSuccessToggle.Value)
        {
            prob = 100;
        }
        return true;
    }

    Dictionary<string, string> _championCnDict = new Dictionary<string, string>()
    {
        { "Archer", "弓箭手" },
        { "Fighter", "格斗家" },
        { "Knight", "骑士" },
        { "Monk", "僧人" },
        { "Ninja", "忍者" },
        { "Priest", "圣职者" },
        { "Pyromancer", "炎术师" },
        { "Swordman", "剑客" },
        { "Pythoness", "巫女" },
        { "Berserker", "狂战士" },
        { "Sniper", "步枪手" },
        { "IceMage", "冰法师" },
        { "MagicKnight", "魔剑士" },
        { "ShieldBearer", "盾牌兵" },
        { "Ghost", "幽灵" },
        { "LightningMage", "雷电法师" },
        { "Necromancer", "死灵法师" },
        { "BoomerangHunter", "回力镖猎人" },
        { "PlagueDoctor", "瘟疫医生" },
        { "PoisonDartHunter", "毒针术师" },
        { "BarrierMagician", "结界术师" },
        { "Demon", "恶魔" },
        { "Vampire", "吸血鬼" },
        { "Gambler", "赌徒" },
        { "Lancer", "枪兵" },
        { "DuelBlader", "双剑士" },
        { "Executioner", "处刑人" },
        { "Bard", "吟游诗人" },
        { "Gunner", "神枪手" },
        { "Illusionist", "幻影法师" },
        { "Shadowmancer", "阴影法师" },
        { "Chef", "厨师" },
        { "Exorcist", "驱魔师" },
        { "Clown", "艺人" },
        { "Ogre", "食人魔" },
        { "Werewolf", "狼人" },
        { "Taoist", "道人" },
        { "Dancer", "舞姬" },
        { "DarkMage", "黑法师" }, // 修正原“黑法师师”的笔误
        { "Jiangshi", "僵尸" }
    };
    Dictionary<int, string> _championDict = new Dictionary<int, string>()
    {
        { 0, "Archer" },
        { 1, "Fighter" },
        { 2, "Knight" },
        { 3, "Monk" },
        { 4, "Ninja" },
        { 5, "Priest" },
        { 6, "Pyromancer" },
        { 7, "Swordman" },
        { 8, "Pythoness" },
        { 9, "Berserker" },
        { 10, "Sniper" },
        { 11, "IceMage" },
        { 12, "MagicKnight" },
        { 13, "ShieldBearer" },
        { 14, "Ghost" },
        { 15, "LightningMage" },
        { 16, "Necromancer" },
        { 17, "BoomerangHunter" },
        { 18, "PlagueDoctor" },
        { 19, "PoisonDartHunter" },
        { 20, "BarrierMagician" },
        { 21, "Demon" },
        { 22, "Vampire" },
        { 23, "Gambler" },
        { 24, "Lancer" },
        { 25, "DuelBlader" },
        { 26, "Executioner" },
        { 27, "Bard" },
        { 28, "Gunner" },
        { 29, "Illusionist" },
        { 30, "Shadowmancer" },
        { 31, "Chef" },
        { 32, "Exorcist" },
        { 33, "Clown" },
        { 34, "Ogre" },
        { 35, "Werewolf" },
        { 36, "Taoist" },
        { 37, "Dancer" },
        { 38, "DarkMage" },
        { 39, "Jiangshi" }
    };

    private bool _loaded = false;
    

    private static Dictionary<int, string> _keyProperty = new Dictionary<int, string>()
    {
        { 0, "target_min_hp" },
        { 1, "chicken" },
        { 2, "fast_man" },
        { 3, "wind" },
        { 4, "winning_mind" },
        { 5, "weak_mind" },
        { 6, "blood_smell" },
        { 7, "first" },
        { 8, "healer" },
        { 9, "hero_mind" },
        { 10, "kill_heal" },
        { 11, "slow_man" },
        { 12, "cockroach" },
        { 13, "distraction" },
        { 14, "wait" },
        { 15, "dragon" },
        { 16, "clutch" },
        { 17, "save" },
        { 18, "positive" },
        { 19, "negative" },
        { 20, "fast_cast" },
        { 21, "spear" },
        { 22, "together" },
        { 23, "thorn" },
        { 24, "moodmaker" },
        { 25, "iron_body" },
        { 26, "glass_body" },
        { 27, "rage" },
        { 28, "sloth" },
        { 29, "safe" },
        { 30, "king" },
        { 31, "min_deal" },
        { 32, "distance" },
        { 33, "magician" },
        { 34, "blue" },
        { 35, "red" }
    };
    private static Dictionary<string, string> _cnProperty = new Dictionary<string, string>()
    {
        {"target_min_hp", "蔑视弱者"},
        {"chicken", "胆小鬼"},
        {"fast_man", "骏马"},
        {"wind", "疾风"},
        {"winning_mind", "好胜"},
        {"weak_mind", "玻璃心"},
        {"blood_smell", "血腥"},
        {"first", "先锋"},
        {"healer", "特蕾莎"},
        {"hero_mind", "英雄心理"},
        {"kill_heal", "捷报"},
        {"slow_man", "树懒"},
        {"cockroach", "蟑螂"},
        {"distraction", "精神涣散"},
        {"wait", "大器晚成"},
        {"dragon", "龙头蛇尾"},
        {"clutch", "替补击球员"},
        {"save", "救援投手"},
        {"positive", "乐观"},
        {"negative", "悲观"},
        {"fast_cast", "快手"},
        {"spear", "贯穿之矛"},
        {"together", "夹击"},
        {"thorn", "尖刺"},
        {"moodmaker", "氛围制造机"},
        {"iron_body", "金剛不坏"},
        {"glass_body", "玻璃人"},
        {"rage", "愤怒"},
        {"sloth", "懒惰"},
        {"safe", "安全第一"},
        {"king", "连战之王"},
        {"min_deal", "堂堂正正"},
        {"distance", "保持距离"},
        {"magician", "大法师"},
        {"blue", "蓝色情报"},
        {"red", "红色情深"}
    };
    private static Dictionary<string, string> _desProperty = new Dictionary<string, string>()
    {
        {"target_min_hp", "优先攻击生命值低的英雄 （只适合刺客的技能 战士直接冲后排也容易倒）"},
        {"chicken", "友军存在时才走近敌军"},
        {"fast_man", "移动速度+1"},
        {"wind", "攻击速度+10% （核心输出 射手战士使用 奶妈也能带个）"},
        {"winning_mind", "优先攻击生命值较高的英雄"},
        {"weak_mind", "人头优势时所有能力+10，劣势时所有能力-10"},
        {"blood_smell", "每当击杀敌军时，在该场比赛中攻击力提升2（最多20） （核心输出 战士法师射手刺客都可以使用）"},
        {"first", "直到第一滴血，所有能力+10"},
        {"healer", "回复量+10%"},
        {"hero_mind", "人头优势时所有能力-10，劣势时所有能力+10"},
        {"kill_heal", "每次击杀或助攻回复生命10"},
        {"slow_man", "移动速度-1"},
        {"cockroach", "生命值30%以下时逃跑，逃跑时移速+2 （即便是辅助依然不是很好用 因为太能跑了）"},
        {"distraction", "每5秒变更攻击目标"},
        {"wait", "所有能力-10。每在场2秒，所有能力+1。"},
        {"dragon", "所有能力+10。每在场2秒，所有能力-1。"},
        {"clutch", "在最后小局，所有能力+10"},
        {"save", "首发时所有能力-10，替补时所有能力+10"},
        {"positive", "状态不会低于普通"},
        {"negative", "状态不能高于普通"},
        {"fast_cast", "技能冷却时间-10% （战士法师射手携带）"},
        {"spear", "获得穿甲10 （核心输出 不过本游戏的伤害计算机制导致破甲对低防更有效 因此更适合刺客战士使用）"},
        {"together", "优先攻击友方英雄最近打击的敌军"},
        {"thorn", "普通攻击减少目标所受的治疗和回复效果3秒"},
        {"moodmaker", "参赛时调高一段友军状态 （不增加自身 但可叠加）"},
        {"iron_body", "受到的伤害-5%"},
        {"glass_body", "受到的伤害+5%"},
        {"rage", "每当生命值下降5%时，攻击力+1 （狂战必备 或者配合瘟疫医生 ）"},
        {"sloth", "生命值满时，攻击速度-5%"},
        {"safe", "生命值满时，攻击速度+5%"},
        {"king", "在BO5中所有能力+5"},
        {"min_deal", "优先攻击受到伤害最少的敌军"},
        {"distance", "使用射手英雄时，攻击速度+5%"},
        {"magician", "使用法师英雄时，技能冷却时间-5%"},
        {"blue", "属于蓝队时所有能力+5"},
        {"red", "属于红队时所有能力+5"}
    };
}