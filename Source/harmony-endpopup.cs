using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;

namespace EndPopup
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "espio.endpopup");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(GameCondition), "End")]
        public static class GameCondition_End_Patch
        {
            static readonly MethodInfo m_MessagesMessage = AccessTools.Method(typeof(Messages), nameof(Messages.Message), new[] { typeof(string), typeof(MessageTypeDef), typeof(bool) });
            static readonly MethodInfo m_ThisDisplayEnd = AccessTools.Method(typeof(GameCondition_End_Patch), nameof(GameCondition_End_Patch.DisplayEndDialog));

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {

                foreach (var inst in instructions)
                {
                    if (inst.Calls(m_MessagesMessage))
                    {
                        yield return inst;
                        yield return new CodeInstruction(OpCodes.Ldarg_0); //this
                        yield return new CodeInstruction(OpCodes.Call, m_ThisDisplayEnd);
                    }
                    else
                        yield return inst;
                }

            }
            public static void DisplayEndDialog(GameCondition __instance)
            {
                if (__instance.def.endMessage == null) { return; }
                var flag = false;
                if (__instance.def.letterDef != null)
                {
                    if (__instance.def.letterDef.defName == "NegativeEvent") { flag = true; }
                }
                if (__instance.def.conditionClass != null && !flag)
                {
                    if (__instance.def.conditionClass.ToString().Contains("PurpleEvent") || __instance.def.conditionClass.ToString().Contains("MultiStage")) { flag = true; }                  
                }
                if (!flag)
                {
                    if (__instance.def.defName == "VolcanicAsh" ||
                        __instance.def.defName == "Drought" ||
                        __instance.def.defName == "DeepFreeze" ||
                        __instance.def.defName == "GillRot") { flag = true; }
                }
                if (__instance.Permanent) {flag = true; }
                else
                {
                    try
                    {
                        if (__instance.Duration > 90000)
                        {
                            flag = true;
                        }
                    }
                    catch { }
                }

                if (flag)
                {
                    Dialog_MessageBox endDialog = new Dialog_MessageBox(__instance.def.endMessage, "Confirm".Translate(), null, null, null, null, false, null, delegate { }, WindowLayer.Dialog);
                    Find.WindowStack.Add(endDialog);
                }
            }
        }
    }
 }