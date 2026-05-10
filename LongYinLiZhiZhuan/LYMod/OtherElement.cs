using LYMod.Helpers;
using UnityEngine;

namespace LYMod;

public static class OtherElement
{
    private static string _searchText = "";
    private static Vector2 _scrollPosition;

    public static void ForceSpeFunction()
    {
        var scale = Plugin.Instance.WindowScaling.Value;
        var builder = UIHelper.CreateBuilder(scale);
        builder.BeginFoldout("门派特性")
            .DrawForceSpeFunction()
            .EndFoldout();

        builder.BeginFoldout("门派建筑效果")
            .DrawSelectBuildings()
            .EndFoldout();
    }

    public static void Label()
    {
        const float fontScale = 1.5f;
        const int columnCount = 3;
        const float rowHeight = 30f;
        const float spacing = 8f;

        var gridStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fixedWidth = 145f,
            fixedHeight = rowHeight,
            margin = new RectOffset(4, 4, 4, 4),
            wordWrap = false,
            clipping = TextClipping.Clip,
            fontSize = Mathf.RoundToInt(14 * fontScale)
        };

        var attrLabels = new[]
        {
            "0:力道", "1:灵巧", "2:智力", "3:意志", "4:体质",
            "5:经脉", "6:内功", "7:轻功", "8:绝技", "9:拳掌",
            "10:剑法", "11:刀法", "12:长兵", "13:奇门", "14:射术",
            "15:内功威力", "16:轻功威力", "17:绝技威力", "18:拳掌威力",
            "19:剑法威力", "20:刀法威力", "21:长兵威力", "22:奇门威力",
            "23:射术威力",
            "24:医术", "25:毒术", "26:学识", "27:口才", "28:采伐",
            "29:木植", "30:锻造", "31:炼药", "32:烹饪",
            "33:力道潜力", "34:灵巧潜力", "35:智力潜力", "36:意志潜力",
            "37:体质潜力", "38:经脉潜力", "39:内功潜力", "40:轻功潜力",
            "41:绝技潜力", "42:拳掌潜力", "43:剑法潜力", "44:刀法潜力",
            "45:长兵潜力", "46:奇门潜力", "47:射术潜力", "48:医术潜力",
            "49:毒术潜力", "50:学识潜力", "51:口才潜力", "52:采伐潜力",
            "53:木植潜力", "54:锻造潜力", "55:炼药潜力", "56:烹饪潜力",
            "57:生命上限", "58:体力上限", "59:内力上限",
            "60:伤害", "61:护甲", "62:护甲率", "63:速度", "64:命中",
            "65:闪避", "66:暴击", "67:卸力", "68:反击", "69:压制",
            "70:连击", "71:断连",
            "72:经验获取", "73:恢复效率", "74:伤害抗性", "75:负面加成",
            "76:负面抗性", "77:伤势抗性",
            "78:失衡", "79:急速", "80:外伤", "81:内伤", "82:毒素",
            "83:吸血", "84:吸内", "85:削内", "86:恢复", "87:灼烧",
            "88:疗伤", "89:流血", "90:调息", "91:雷电", "92:蓄力",
            "93:疲劳", "94:冰寒", "95:无敌", "96:眩晕",
            "97:手臂点穴", "98:腿足点穴", "99:心胸点穴", "100:腰腹点穴",
            "101:头颈点穴",
            "102:虚弱", "103:横练", "104:破甲", "105:轻灵", "106:迟缓",
            "107:鹰眼", "108:目盲", "109:飘逸", "110:笨拙", "111:霸体",
            "112:弱点", "113:疯魔", "114:混乱", "115:死战", "116:穿甲",
            "117:壮骨", "118:断骨", "119:舒筋", "120:伤筋", "121:醒脑",
            "122:震脑", "123:定神", "124:乱神", "125:活血", "126:凝血",
            "127:通脉", "128:截脉", "129:复生", "130:反伤",
            "131:反伤吸内", "132:反伤削内", "133:击退", "134:拉近", "135:慧眼",
            "136:昏沉", "137:铜皮", "138:脆弱", "139:不屈", "140:重碾",
            "141:连绵", "142:断腕", "143:断招", "144:阻滞", "145:杀机",
            "146:康复", "147:不愈", "148:坚毅", "149:动摇", "150:内劲",
            "151:真伤", "152:回血", "153:回内", "154:回体", "155:自愈",
            "156:护头", "157:护心", "158:护胸", "159:护腹", "160:护手",
            "161:护足", "162:护体真气", "163:反弹", "164:减伤",
            "165:定身", "166:移距", "167:神行", "168:裂膝", "169:破招",
            "170:拆招", "171:斗转",
            "172:声望获取", "173:功绩获取", "174:旅行速度", "175:疗伤效率",
            "176:实战经验", "177:理论经验", "178:技艺经验", "179:内功经验",
            "180:轻功经验", "181:绝技经验", "182:拳掌经验", "183:剑法经验",
            "184:刀法经验", "185:长兵经验", "186:奇门经验", "187:射术经验",
            "188:医术经验", "189:毒术经验", "190:学识经验", "191:口才经验",
            "192:采伐经验", "193:木植经验", "194:锻造经验", "195:炼药经验",
            "196:烹饪经验", "197:买卖优势", "198:拳掌距离", "199:剑法距离",
            "200:刀法距离", "201:长兵距离", "202:奇门距离", "203:射术距离",
            "204:坐骑加成", "205:装备强化", "206:抗耐药性", "207:装备负重",
            "208:机关伤害", "209:机关速度", "210:机关耐久", "211:城防耐久",
            "212:好感获取", "214:恶名减少", "213:本门武学威力"
        };

        GUILayout.BeginVertical("Box");
        
        // 搜索框
        GUILayout.BeginHorizontal();
        GUILayout.Label("拼音搜索:", GUILayout.Width(50));
        _searchText = GUILayout.TextField(_searchText, GUILayout.Height(25));
        if (GUILayout.Button("清除", GUILayout.Width(50)))
        {
            _searchText = "";
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 过滤标签
        var filteredLabels = string.IsNullOrEmpty(_searchText)
            ? attrLabels
            : Array.FindAll(attrLabels, label =>
                PinyinHelper.ContainsPinyinOrChinese(label, _searchText));

        // 滚动视图 - 使用剩余空间
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(990));

        for (var i = 0; i < filteredLabels.Length; i += columnCount)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(rowHeight));
            GUILayout.Space(spacing);

            for (var col = 0; col < columnCount; col++)
            {
                var index = i + col;
                if (index >= filteredLabels.Length) break;

                GUILayout.Label(filteredLabels[index], gridStyle);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(spacing);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }
}