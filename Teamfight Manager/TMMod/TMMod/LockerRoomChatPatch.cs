using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TMMod;

[HarmonyPatch]
public class LockerRoomChatPatch
{
    private static readonly FieldInfo MText = typeof(UnityEngine.UI.Text).GetField("m_Text",
        BindingFlags.Instance | BindingFlags.NonPublic)!;

    // 原方法: LockerRoomChat.Init(string set, Athlete athlete, string champ)
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LockerRoomChat), "Init", typeof(string), typeof(Athlete), typeof(string))]
    public static bool Init_Prefix(ref IEnumerator __result, LockerRoomChat __instance,
        string set, Athlete athlete, string champ)
    {
        __result = Init_Replacement(__instance, set, athlete, champ);
        return false; // 跳过原方法
    }

    private static IEnumerator Init_Replacement(LockerRoomChat instance,
        string set, Athlete athlete, string champ)
    {
        yield return new WaitForSeconds(0.2f);

        // 生成4个I18n按钮key（与原逻辑一致）
        var buttons = new string[4];
        for (var i = 0; i < 4; i++)
        {
            var typeIdx = 3 - i;
            var key = string.Format("{0}.type{1}", set, typeIdx);
            var size = I18n.Info.Table.GetSize(key);
            var num = Random.Range(0, size);
            buttons[i] = string.Format("{0}.type{1}[{2}]", set, typeIdx, num);
        }

        var localeParams = new Dictionary<string, string>
        {
            { "champion", "champion." + champ.ToEnLower() }
        };

        // 解析各按钮文本 + 追加成功率
        var buttonTexts = new string[4];
        for (var j = 0; j < 4; j++)
        {
            var selected = 3 - j;
            var dist = (int)(Mathf.Abs(athlete.InterviewType - selected - 0.5f) - 0.5f);
            var successProb = GetSuccessProb(dist);
            var resolved = new I18nString(buttons[j], I18n.Info.Table).Build(localeParams);
            buttonTexts[j] = string.Format("{0}\n(+25 Condition, {1}%)",
                resolved, successProb);
        }

        // 手动实现 7-参数 Init 的 UI 逻辑
        instance.Panel.SetActive(true);
        instance.Contents.text = "stadium.locker_room.coach";
        instance.Contents.SetBuildParam(localeParams);

        // 使用 __RAW__ 前缀 + 反射设置 m_Text 字段，I18nLabel.text getter 会直接返回原始文本
        for (int k = 0; k < 4; k++)
        {
            MText.SetValue(instance.Buttons[k], "__RAW__" + buttonTexts[k]);
            instance.Buttons[k].SetVerticesDirty();
            instance.Buttons[k].SetLayoutDirty();
        }

        instance.Selected = -1;
        while (instance.Selected == -1)
        {
            yield return null;
        }

        instance.Panel.SetActive(false);

        instance.View.StopSay(athlete);
        yield return new WaitForSeconds(0.3f);
        yield return instance.View.CoachSay(
            new I18nString(buttons[3 - instance.Selected], I18n.Info.Table)
                .Build(localeParams));
    }

    private static int GetSuccessProb(int dist)
    {
        switch (dist)
        {
            case 0: return 90;
            case 1: return 50;
            case 2: return 20;
            default: return 0;
        }
    }
}
