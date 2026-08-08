using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using UnityEngine;
using static HarmonyLib.Code;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace GatherEntireFloorLoot
{
    [HarmonyPatch(typeof(ShuttleWindow), nameof(ShuttleWindow.Configure))]
    public class LootButtons
    {
        public static CommonButton lootAllButton;
        public static CommonButton ampAllButton;


        static bool Disable_Amp_All = Plugin.ConfigGeneral.ModData.GetConfigValue<bool>("Disable_Amp_All", false);
        static void Prefix(ShuttleWindow __instance)
        {
            bool active = ElevatorLootButton.CheckButtonActiveStatus();
            if (active && lootAllButton == null)
            {
                lootAllButton = UnityEngine.Object.Instantiate(__instance._missionExitButton, __instance._missionExitButton.transform.parent.transform);
                lootAllButton.OnClick += ElevatorLootButton.LootAllButtonClick;
                lootAllButton.name = "LootAllButton";

                //active only when condition is met
                lootAllButton.gameObject.SetActive(active);

                Transform captionTransform = lootAllButton.transform.Find("Caption");
                if (captionTransform != null)
                {
                    LocalizableLabel locLabel = captionTransform.GetComponent<LocalizableLabel>();
                    if (locLabel != null)
                    {
                        locLabel._label = "ui.caption.takeall";
                        //consider changing this in the future.
                    }
                }
                lootAllButton.SetInteractable(true);

                if (!Disable_Amp_All)
                {
                    ampAllButton = UnityEngine.Object.Instantiate(__instance._missionExitButton, __instance._missionExitButton.transform.parent.transform);
                    ampAllButton.OnClick += ElevatorLootButton.AmpAllButtonClick;
                    ampAllButton.name = "AmpAllButton";

                    //active only when condition is met
                    ampAllButton.gameObject.SetActive(active);

                    Transform captionTransform2 = ampAllButton.transform.Find("Caption");
                    if (captionTransform2 != null)
                    {
                        LocalizableLabel locLabel = captionTransform2.GetComponent<LocalizableLabel>();
                        if (locLabel != null)
                        {
                            //locLabel._label = "Amp All";
                            locLabel._label = "ui.label.amputate_corpse_slots";
                            //ui.label.amputate_corpse_slots
                        }
                    }
                    ampAllButton.SetInteractable(true);
                }

            }
        }



    }
}
