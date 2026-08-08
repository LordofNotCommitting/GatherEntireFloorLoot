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
            this.ModName = ModName;
            this.ModData = new ModConfigData(ConfigPath);
            this.ModData.AddConfigHeader("STRING:General Settings", "general");
            this.ModData.AddConfigValue("general", "Enemy_Count_Check_Bool", false, "STRING:Only Enable When Enemy Count is Low", "STRING:Turn this on to make button only appear when enemy count is low.");
            this.ModData.AddConfigValue("general", "Enemy_Count_Check_Num", Enemy_Count_Check_Num_Array[0], Enemy_Count_Check_Num_Array[1], Enemy_Count_Check_Num_Array[2], "STRING:Enable When Enemy is Less than", "STRING:Set Threshold for above enemy count check for above setting.");
            //amp option?

            this.ModData.AddConfigValue("general", "Disable_Amp_All", false, "STRING:Disable Amputate All", "STRING:Turn it off if you do not want it.");
            this.ModData.AddConfigValue("general", "Amp_All_Uses_Dur", false, "STRING:Amp All Consume Durability", "STRING:Turn this on to make button require durability.");

            this.ModData.AddConfigValue("general", "about_final", "STRING:<color=#f51b1b>The game must be restarted after setting then saving this config to take effect.</color>\n");
            this.ModData.RegisterModConfigData(ModName);
        }

        private string ModName;

        public ModConfigData ModData;

    }
}
