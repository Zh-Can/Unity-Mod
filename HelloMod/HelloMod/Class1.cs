using Il2Cppgame;
using MelonLoader;
using UnityEngine;
using HarmonyLib;
using Il2Cpp;
using Il2Cppmodules.MainFrame;

[assembly:MelonInfo(typeof(HelloMod.TestMod), "HelloMod", "1.0.0", "Can")]
[assembly:MelonGame("bushuai", "FickleCardLegend")]
[assembly:MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace HelloMod
{
    public class TestMod : MelonMod
    {
        
        // 初始化
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            Melon<TestMod>.Logger.Msg("successssssssssssssss!");
        }

        // 每一帧调用一次（在Update时调用）
        public override void OnUpdate()
        {
            base.OnUpdate();
            //Melon<TestMod>.Logger.Msg("Hello Mod!");
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                Melon<TestMod>.Logger.Msg("ReFreshMonsters");
                MonsterLayoutSystem.ReFreshMonsters();
            }
            
        }

        
    }
    
    // patch要执行的函数
    public class PatchTest
    {
        public void Fun1(int i)
        {
            Melon<TestMod>.Logger.Msg($"Fun1: {i}");
        }
    }    


    // HarmonyPatch 
    // 两种写法
    [HarmonyPatch(typeof(PatchTest), nameof(PatchTest.Fun1))]
    // [HarmonyPatch(typeof(PatchTest), "Fun1")]
    public static class Patch1
    {
        // Postfix : 在函数调用后执行
        [HarmonyPostfix]
        public static void PatchPostfix()
        {
            Melon<TestMod>.Logger.Msg("Patch1的Post被调用");
        }
        // Prefix : 在函数调用之前执行
        // 返回：bool true/false 表示是否继续执行原函数
        [HarmonyPrefix]
        public static bool PatchPrefix(ref int i)
        {
            i = 123123123;
            Melon<TestMod>.Logger.Msg($"Patch1的Pre被调用,原值被修改为{i}");
            return true;
        }
    }

    [HarmonyPatch(typeof(MonsterDie), nameof(MonsterDie.dropItem))]
    public static class Patch
    {
        [HarmonyPrefix]
        public static bool PatchPrefix( ref int count,
            int dropID,
            ref int dropMoney,
            List<GameObject> items,
            int dropLevelInGroup,
            int premiumRate)
        {
            count = count * 2;
            dropMoney = dropMoney * 100;
            return true;
        }
    }
    
  
}

