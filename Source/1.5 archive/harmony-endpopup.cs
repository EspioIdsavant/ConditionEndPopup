using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.Emit;
using System.Reflection;
using static Mono.Security.X509.X520;
using UnityEngine.UIElements;

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

                //var codeMatcher = new CodeMatcher(instructions, generator);

                //codeMatcher.MatchStartForward()
                //    .MatchStartForward(CodeMatch.Calls(m_MessagesMessage))
                //    .ThrowIfInvalid("Could not find where end condition calls message")
                //    .ThrowIfNotMatch("Could not find where end condition calls message")
                //    .RemoveInstruction()
                //    .Insert(
                //        CodeInstruction.Call(() => DisplayEndDialog("hello world"))
                //    );



            }
            public static void DisplayEndDialog(GameCondition __instance)
            {
                Dialog_MessageBox endDialog = new Dialog_MessageBox(__instance.def.endMessage, "Confirm".Translate(), null, null, null, null, false, null, delegate
                {
                }, WindowLayer.Dialog);
                var flag = false;
                if (__instance.def.letterDef != null)
                {
                    if (__instance.def.letterDef.defName == "NegativeEvent") { flag = true; }
                }
                if (__instance.def.conditionClass != null)
                {
                    if (__instance.def.conditionClass.ToString().Contains("PurpleEvent")) { flag = true; }
                    
                }
                if (__instance.Duration > 90000 && flag)
                {
                    Find.WindowStack.Add(endDialog);
                }

            }
        }




    }

 }

 





