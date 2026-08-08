using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GatherEntireFloorLoot
{
    public class Plugin
    {

        //public static State _state { get; set; }

        public static string ModAssemblyName
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Name;
            }
        }

        private static string ModPersistenceFolder
        {
            get
            {
                return Path.Combine(Application.persistentDataPath + "/../Quasimorph_ModConfigs", "LoC_GatherEntireFloorLoot");
            }
        }
        private static string ConfigPath
        {
            get
            {
                return Path.Combine(Plugin.ModPersistenceFolder, "config.txt");
            }
        }

        private static string SavePath
        {
            get
            {
                return Path.Combine(Plugin.ModPersistenceFolder, "savedata.json");
            }
        }

        public static Logger Logger { get; private set; } = new Logger("");

        public static ModConfigGeneral ConfigGeneral { get; set; }

        public static ModSave Save { get; set; }

        public static State State;

        [Hook(ModHookType.AfterConfigsLoaded)]
        public static void AfterConfig(IModContext context)
        {

            Plugin.State = context.State;
            Plugin.ConfigGeneral = new ModConfigGeneral("Gather Entire Floor Loot", Plugin.ConfigPath);
            Plugin.Save = new ModSave(Plugin.SavePath);
            new Harmony("LoC_" + Plugin.ModAssemblyName).PatchAll();
        }
    }
}
