using System.Collections.Generic;
using System.Text;

namespace LYMod.Helpers;

public static class PinyinHelper
{
    private static readonly Dictionary<char, string> PinyinMap = new()
    {
        // 数字
        ['0'] = "0", ['1'] = "1", ['2'] = "2", ['3'] = "3", ['4'] = "4",
        ['5'] = "5", ['6'] = "6", ['7'] = "7", ['8'] = "8", ['9'] = "9",
        [':'] = ":", [' '] = " ",

        // 基本属性
        ['力'] = "li", ['道'] = "dao", ['灵'] = "ling", ['巧'] = "qiao",
        ['智'] = "zhi", ['力'] = "li", ['意'] = "yi", ['志'] = "zhi",
        ['体'] = "ti", ['质'] = "zhi", ['经'] = "jing", ['脉'] = "mai",
        ['内'] = "nei", ['功'] = "gong", ['轻'] = "qing", ['绝'] = "jue",
        ['技'] = "ji", ['拳'] = "quan", ['掌'] = "zhang", ['剑'] = "jian",
        ['法'] = "fa", ['刀'] = "dao", ['长'] = "chang", ['兵'] = "bing",
        ['奇'] = "qi", ['门'] = "men", ['射'] = "she", ['术'] = "shu",

        // 威力
        ['威'] = "wei",

        // 技艺
        ['医'] = "yi", ['毒'] = "du", ['学'] = "xue", ['识'] = "shi",
        ['口'] = "kou", ['才'] = "cai", ['采'] = "cai", ['伐'] = "fa",
        ['木'] = "mu", ['植'] = "zhi", ['锻'] = "duan", ['造'] = "zao",
        ['炼'] = "lian", ['药'] = "yao", ['烹'] = "peng", ['饪'] = "ren",

        // 潜力
        ['潜'] = "qian",

        // 战斗属性
        ['生'] = "sheng", ['命'] = "ming", ['上'] = "shang", ['限'] = "xian",
        ['体'] = "ti", ['力'] = "li", ['伤'] = "shang", ['害'] = "hai",
        ['护'] = "hu", ['甲'] = "jia", ['率'] = "lv", ['速'] = "su",
        ['度'] = "du", ['命'] = "ming", ['中'] = "zhong", ['闪'] = "shan",
        ['避'] = "bi", ['暴'] = "bao", ['击'] = "ji", ['卸'] = "xie",
        ['反'] = "fan", ['击'] = "ji", ['压'] = "ya", ['制'] = "zhi",
        ['连'] = "lian", ['断'] = "duan",

        // 其他属性
        ['经'] = "jing", ['验'] = "yan", ['获'] = "huo", ['取'] = "qu",
        ['恢'] = "hui", ['复'] = "fu", ['效'] = "xiao", ['抗'] = "kang",
        ['性'] = "xing", ['负'] = "fu", ['面'] = "mian", ['加'] = "jia",
        ['成'] = "cheng", ['伤'] = "shang", ['势'] = "shi",

        // 状态效果
        ['失'] = "shi", ['衡'] = "heng", ['急'] = "ji", ['速'] = "su",
        ['外'] = "wai", ['内'] = "nei", ['毒'] = "du", ['素'] = "su",
        ['吸'] = "xi", ['血'] = "xue", ['削'] = "xiao", ['灼'] = "zhuo",
        ['烧'] = "shao", ['疗'] = "liao", ['流'] = "liu", ['调'] = "tiao",
        ['息'] = "xi", ['雷'] = "lei", ['电'] = "dian", ['蓄'] = "xu",
        ['疲'] = "pi", ['劳'] = "lao", ['冰'] = "bing", ['寒'] = "han",
        ['无'] = "wu", ['敌'] = "di", ['眩'] = "xuan", ['晕'] = "yun",

        // 点穴
        ['手'] = "shou", ['臂'] = "bi", ['点'] = "dian", ['穴'] = "xue",
        ['腿'] = "tui", ['足'] = "zu", ['心'] = "xin", ['胸'] = "xiong",
        ['腰'] = "yao", ['腹'] = "fu", ['头'] = "tou", ['颈'] = "jing",

        // 更多状态
        ['虚'] = "xu", ['弱'] = "ruo", ['横'] = "heng", ['练'] = "lian",
        ['破'] = "po", ['轻'] = "qing", ['灵'] = "ling", ['迟'] = "chi",
        ['缓'] = "huan", ['鹰'] = "ying", ['眼'] = "yan", ['目'] = "mu",
        ['盲'] = "mang", ['飘'] = "piao", ['逸'] = "yi", ['笨'] = "ben",
        ['拙'] = "zhuo", ['霸'] = "ba", ['体'] = "ti", ['弱'] = "ruo",
        ['点'] = "dian", ['疯'] = "feng", ['魔'] = "mo", ['混'] = "hun",
        ['乱'] = "luan", ['死'] = "si", ['战'] = "zhan", ['穿'] = "chuan",
        ['壮'] = "zhuang", ['骨'] = "gu", ['断'] = "duan", ['舒'] = "shu",
        ['筋'] = "jin", ['伤'] = "shang", ['醒'] = "xing", ['脑'] = "nao",
        ['震'] = "zhen", ['定'] = "ding", ['神'] = "shen", ['乱'] = "luan",
        ['活'] = "huo", ['血'] = "xue", ['凝'] = "ning", ['通'] = "tong",
        ['脉'] = "mai", ['截'] = "jie", ['复'] = "fu", ['生'] = "sheng",
        ['伤'] = "shang", ['吸'] = "xi", ['内'] = "nei", ['推'] = "tui",
        ['退'] = "tui", ['拉'] = "la", ['近'] = "jin", ['慧'] = "hui",
        ['昏'] = "hun", ['沉'] = "chen", ['铜'] = "tong", ['皮'] = "pi",
        ['脆'] = "cui", ['弱'] = "ruo", ['不'] = "bu", ['屈'] = "qu",
        ['重'] = "zhong", ['碾'] = "nian", ['绵'] = "mian", ['腕'] = "wan",
        ['招'] = "zhao", ['阻'] = "zu", ['滞'] = "zhi", ['杀'] = "sha",
        ['机'] = "ji", ['康'] = "kang", ['愈'] = "yu", ['不'] = "bu",
        ['坚'] = "jian", ['毅'] = "yi", ['动'] = "dong", ['摇'] = "yao",
        ['劲'] = "jin", ['真'] = "zhen", ['回'] = "hui", ['自'] = "zi",
        ['愈'] = "yu", ['头'] = "tou", ['心'] = "xin", ['胸'] = "xiong",
        ['腹'] = "fu", ['真'] = "zhen", ['气'] = "qi", ['弹'] = "tan",
        ['减'] = "jian", ['定'] = "ding", ['身'] = "shen", ['移'] = "yi",
        ['距'] = "ju", ['裂'] = "lie", ['膝'] = "xi", ['拆'] = "chai",
        ['斗'] = "dou", ['转'] = "zhuan",

        // 其他
        ['声'] = "sheng", ['望'] = "wang", ['功'] = "gong", ['绩'] = "ji",
        ['旅'] = "lv", ['行'] = "xing", ['实'] = "shi", ['理'] = "li",
        ['论'] = "lun", ['技'] = "ji", ['艺'] = "yi", ['买'] = "mai",
        ['卖'] = "mai", ['优'] = "you", ['势'] = "shi", ['距'] = "ju",
        ['离'] = "li", ['坐'] = "zuo", ['骑'] = "qi", ['装'] = "zhuang",
        ['备'] = "bei", ['强'] = "qiang", ['化'] = "hua", ['耐'] = "nai",
        ['药'] = "yao", ['性'] = "xing", ['负'] = "fu", ['重'] = "zhong",
        ['机'] = "ji", ['关'] = "guan", ['耐'] = "nai", ['久'] = "jiu",
        ['城'] = "cheng", ['防'] = "fang", ['好'] = "hao", ['感'] = "gan",
        ['恶'] = "e", ['名'] = "ming", ['减'] = "jian", ['少'] = "shao",
        ['本'] = "ben", ['门'] = "men", ['武'] = "wu", ['学'] = "xue",
    };

    public static string GetPinyin(string chinese)
    {
        if (string.IsNullOrEmpty(chinese))
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var c in chinese)
        {
            if (PinyinMap.TryGetValue(c, out var py))
            {
                sb.Append(py);
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString().ToLower();
    }

    public static string GetPinyinInitials(string chinese)
    {
        if (string.IsNullOrEmpty(chinese))
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var c in chinese)
        {
            if (PinyinMap.TryGetValue(c, out var py) && py.Length > 0)
            {
                sb.Append(py[0]);
            }
            else if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLower(c));
            }
        }
        return sb.ToString();
    }

    public static bool ContainsPinyinOrChinese(string text, string search)
    {
        if (string.IsNullOrEmpty(search))
            return true;

        search = search.ToLower();
        var textLower = text.ToLower();
        var textPinyin = GetPinyin(text);
        var textInitials = GetPinyinInitials(text);

        return textLower.Contains(search) || 
               textPinyin.Contains(search) ||
               textInitials.Contains(search);
    }
}
