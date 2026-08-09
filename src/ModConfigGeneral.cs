using MGSC;
using ModConfigMenu.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GatherEntireFloorLoot
{
    // Token: 0x02000006 RID: 6
    public class ModConfigGeneral
    {

        // ====== combined ======
        // default, min, max value respectively
        public static int[] Enemy_Count_Check_Num_Array = new int[] { 5, 0, 100 };

        public ModConfigGeneral(string ModName, string ConfigPath)
        {
            //note: remove "STRING:" later.
            this.ModName = ModName;
            this.ModData = new ModConfigData(ConfigPath);
            this.ModData.AddConfigHeader("General Settings", "general");
            this.ModData.AddConfigValue("general", "Enemy_Count_Check_Bool", false, "Only Enable When Enemy Count is Low", "Turn this on to make button only appear when enemy count is low.");
            this.ModData.AddConfigValue("general", "Enemy_Count_Check_Num", Enemy_Count_Check_Num_Array[0], Enemy_Count_Check_Num_Array[1], Enemy_Count_Check_Num_Array[2], "Enable When Enemy is Less than", "Set Threshold for above enemy count check for above setting.");
            //amp option?

            this.ModData.AddConfigValue("general", "Disable_Amp_All", false, "Disable Amputate All", "Turn it off if you do not want it.");
            this.ModData.AddConfigValue("general", "Amp_All_Uses_Dur", false, "Amp All Consume Durability", "Turn this on to make button require durability.");

            this.ModData.AddConfigValue("general", "about_final", "<color=#f51b1b>The game must be restarted after setting then saving this config to take effect.</color>\n");
            this.ModData.RegisterModConfigData(ModName);
        }

        private string ModName;

        public ModConfigData ModData;

    }
}
