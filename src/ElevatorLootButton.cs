using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading;
using UnityEngine;
using static HarmonyLib.Code;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace GatherEntireFloorLoot
{
    [HarmonyPatch(typeof(ElevatorWindow), nameof(ElevatorWindow.Configure))]
    public class ElevatorLootButton
    {
        public static CommonButton lootAllButton;
        public static CommonButton ampAllButton;

        static bool Enemy_Count_Check_Bool = Plugin.ConfigGeneral.ModData.GetConfigValue<bool>("Enemy_Count_Check_Bool", false);
        static int Enemy_Count_Check_Num = Plugin.ConfigGeneral.ModData.GetConfigValue<int>("Enemy_Count_Check_Num", ModConfigGeneral.Enemy_Count_Check_Num_Array[0]);


        static bool Disable_Amp_All = Plugin.ConfigGeneral.ModData.GetConfigValue<bool>("Disable_Amp_All", false);
        static bool Amp_All_Uses_Dur = Plugin.ConfigGeneral.ModData.GetConfigValue<bool>("Amp_All_Uses_Dur", false);

        static void Prefix(ElevatorWindow __instance)
        {
            bool active = CheckButtonActiveStatus();
            if (active && lootAllButton == null)
            {
                lootAllButton = UnityEngine.Object.Instantiate(__instance._missionExitButton, __instance._missionExitButton.transform.parent.transform);
                lootAllButton.OnClick += LootAllButtonClick;
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
                    ampAllButton.OnClick += AmpAllButtonClick;
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



        public static void AmpAllButtonClick(CommonButton button, int arg2)
        {

            UI.Hide<ElevatorWindow>();
            bool amp_issue = false;

            Statistics statistics = StateManager.ActiveState.Get<Statistics>();
            MagnumProgression magnumProgression = StateManager.ActiveState.Get<MagnumProgression>();
            MapGrid mapGrid = StateManager.ActiveState.Get<MapGrid>();

            UI.Chain<InventoryScreen>().Show(false).Invoke(delegate (InventoryScreen v)
            {
                
                ItemsOnFloor _itemsOnFloor = StateManager.ActiveState.Get<ItemsOnFloor>();
                ItemOnFloor pc_itemOnFloor = _itemsOnFloor.GetOrCreate(v._creatures.Player.CreatureData.Position);

                MapGrid temp_MapGrid = v._creatures.Player._mapGrid;

                int x_coord_max = temp_MapGrid.MaxWidth;
                int y_coord_max = temp_MapGrid.MaxHeight;


                //we have basic ground to work with
                if (StateManager.ActiveState != null)
                {
                    //only for bodies this time
                    MapObstacles mapObstacles = StateManager.ActiveState.Get<MapObstacles>();
                    foreach (MapObstacle mapObstacle in mapObstacles.Obstacles)
                    {
                        //for all corpses
                        if (mapObstacle.CorpseStorage != null)
                        {
                            bool is_explored = false;
                            foreach (CellPosition cellPosition in mapObstacle.OccupiedCells)
                            {
                                if (temp_MapGrid.GetCell(cellPosition.X, cellPosition.Y, true).IsExplored)
                                {
                                    //container that is lit
                                    is_explored = true;
                                }
                            }
                            if (is_explored)
                            {
                                //uuuh we just go through few things
                                //Plugin.Logger.Log("" + mapObstacle.CorpseStorage);
                                amp_issue = AmputateAllFromCorpseStorage(mapObstacle.CorpseStorage, v._creatures.Player, _itemsOnFloor, statistics, magnumProgression, mapGrid);
                            }
                        }
                    }
                }



                //make sound
                if (!amp_issue)
                {
                    SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.Amputation, false, 0f);
                }
                else 
                {
                    SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack, false, 0f);
                }


            }).SetBackOnBackgroundClick(true);

            //ffs, just do it
            UI.Hide<InventoryScreen>();

            UI.Chain<InventoryScreen>().Show(true).Invoke(delegate (InventoryScreen v)
            {

                ItemsOnFloor _itemsOnFloor = StateManager.ActiveState.Get<ItemsOnFloor>();
                ItemOnFloor pc_itemOnFloor = _itemsOnFloor.GetOrCreate(v._creatures.Player.CreatureData.Position);
                //then sort.

                SpaceTime spaceTime = StateManager.ActiveState.Get<SpaceTime>();
                pc_itemOnFloor.Storage.SortWithExpandByTypeAndName(spaceTime);


                //then open inventory screen.
                v._lastInteractObstacle = null;

                if (pc_itemOnFloor != null && !pc_itemOnFloor.Storage.Empty)
                {
                    v._tabsView.AddTab(v._itemsOnFloorView, pc_itemOnFloor.Storage, TabType.Nymeric, false);
                }
                v._hideAfterItemsOnFloorLooted = (v._tabsView.TabsCount > 0);
                v._tabsView.SelectAndShowFirstTab();
            }).SetBackOnBackgroundClick(true);
        }




        static private bool AmputateAllFromCorpseStorage(CorpseStorage corpseStorage, Player player, ItemsOnFloor itemsOnFloor, Statistics statistics, MagnumProgression magnumProgression, MapGrid mapGrid)
        {
            // Extract slot IDs directly from the corpse's data map
            List<string> woundSlotIds = corpseStorage.CreatureData.WoundSlotMap.Keys.ToList();


            BasePickupItem firstAmputationWeapon = player.CreatureData.Inventory.GetFirstAmputationWeapon();
            string damageType = "";
            float additionalImplantDropChance = player.CreatureData.GetAdditionalImplantDropChance();

            if (firstAmputationWeapon == null && Amp_All_Uses_Dur) {
                return true;
            }
            damageType = Amp_All_Uses_Dur?DamageSystem.GetDamageType(firstAmputationWeapon) : "lacer";

            foreach (string slotId in woundSlotIds)
            {

                bool flag = AmputationSystem.AmputateCorpse(
                    magnumProgression,
                    mapGrid,
                    itemsOnFloor,
                    corpseStorage,
                    slotId,
                    player.CreatureData.Inventory,
                    player.CreatureData.Position,
                    damageType,
                    additionalImplantDropChance
                );

                if (flag)
                {
                    if (firstAmputationWeapon != null && Amp_All_Uses_Dur)
                    {
                        firstAmputationWeapon.Comp<BreakableItemComponent>()?.Break(1);
                    }
                    statistics.IncreaseStatistic(StatisticType.AmputationCorpse, 1);
                }
            }
            return false;
        }


        public static void LootAllButtonClick(CommonButton button, int arg2)
        {

            UI.Hide<ElevatorWindow>();
            //make sound
            SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.TakeItem, false, 0f);

            UI.Chain<InventoryScreen>().Show(true).Invoke(delegate (InventoryScreen v)
            {
                //v.ConfigureTabs(shuttle_instance._mapObstacle);
                /*
                ItemOnFloor pc_itemOnFloor = v._itemsOnFloor.Get(v._creatures.Player.CreatureData.Position);
                if (pc_itemOnFloor == null) {
                    pc_itemOnFloor = SingletonMonoBehaviour<ItemFactory>.Instance.CreateItemOnFloor(v._creatures.Player.CreatureData.Position.X, v._creatures.Player.CreatureData.Position.Y, 0f);
                }
                pc_itemOnFloor.Storage.Resize(8,5000);
                */

                ItemsOnFloor _itemsOnFloor = StateManager.ActiveState.Get<ItemsOnFloor>();
                ItemOnFloor pc_itemOnFloor = _itemsOnFloor.GetOrCreate(v._creatures.Player.CreatureData.Position);

                MapGrid temp_MapGrid = v._creatures.Player._mapGrid;

                //Plugin.Logger.Log(""+_itemsOnFloor.IsEmpty(v._creatures.Player.CreatureData.Position.X, v._creatures.Player.CreatureData.Position.Y));
                

                int x_coord_max = temp_MapGrid.MaxWidth;
                int y_coord_max = temp_MapGrid.MaxHeight;


                //for all cells
                /*
                for (int i = 0; i < y_coord_max; i++)
                {
                    for (int j = 0; j < x_coord_max; j++)
                    {
                        MapCell cell = temp_MapGrid.GetCell(j, i, true);
                        //if cell is explored (which means visible I assume)

                        if (cell.IsExplored) {
                            //ref InventoryScreen.ConfigureTabs 
                            

                        }

                    }
                }
                */

                //we have basic ground to work with
                if (StateManager.ActiveState != null)
                {
                    //for all containers that is lit
                    //gather up.
                    //from all BS
                    MapObstacles mapObstacles = StateManager.ActiveState.Get<MapObstacles>();
                    foreach (MapObstacle mapObstacle in mapObstacles.Obstacles)
                    {
                        //for all containers 
                        if (mapObstacle.Store != null) {
                            bool is_explored = false;
                            foreach (CellPosition cellPosition in mapObstacle.OccupiedCells)
                            {
                                if (temp_MapGrid.GetCell(cellPosition.X, cellPosition.Y, true).IsExplored)
                                {
                                    //container that is lit
                                    is_explored = true;
                                }
                            }

                            if (is_explored && !(mapObstacle.Store is StationStash) && !(mapObstacle.Store is AutonomousCapsuleStore))
                            {

                                List<BasePickupItem> list = new List<BasePickupItem>();
                                list.AddRange(mapObstacle.Store.storage.Items);
                                foreach (BasePickupItem basePickupItem in list)
                                {
                                    //pc_itemOnFloor.Storage.AddItemAndReshuffleOptional(basePickupItem);
                                    //Plugin.Logger.Log(basePickupItem.Id);
                                    Fixed_StackItemOnFloor(basePickupItem, pc_itemOnFloor.Storage);



                                    //Plugin.Logger.Log("" + pc_itemOnFloor.Storage.Items.Count);

                                    //basePickupItem.Storage.Remove(basePickupItem, true);
                                    //basePickupItem.Storage = null;
                                }
                                mapObstacle.Store.Looted = true;
                                mapObstacle.RefreshVisual();
                            }
                        }

                        //for all corpses

                        if (mapObstacle.CorpseStorage != null)  
                        {
                            bool is_explored = false;
                            foreach (CellPosition cellPosition in mapObstacle.OccupiedCells)
                            {
                                if (temp_MapGrid.GetCell(cellPosition.X, cellPosition.Y, true).IsExplored)
                                {
                                    //container that is lit
                                    is_explored = true;
                                }
                            }
                            if (is_explored)
                            {
                                foreach (ItemStorage itemStorage in mapObstacle.CorpseStorage._creatureData.Inventory.AllContainers)
                                {

                                    List<BasePickupItem> list = new List<BasePickupItem>();
                                    list.AddRange(itemStorage.Items);
                                    foreach (BasePickupItem basePickupItem in list)
                                    {
                                        Fixed_StackItemOnFloor(basePickupItem, pc_itemOnFloor.Storage);
                                    }
                                    mapObstacle.CorpseStorage.CheckLootTrigger();
                                    mapObstacle.RefreshVisual();
                                }
                            }
                        }
                    }
                }


                //Plugin.Logger.Log("" + _itemsOnFloor.IsEmpty(v._creatures.Player.CreatureData.Position.X, v._creatures.Player.CreatureData.Position.Y));



                ItemsOnFloor itemsOnFloor = v._itemsOnFloor;
                foreach (ItemOnFloor itemOnFloor in itemsOnFloor.Values)
                {
                    //all from floor, and is lit.
                    //gather up.
                    bool is_explored = false;
                    if (temp_MapGrid.GetCell(itemOnFloor.pos.X, itemOnFloor.pos.Y, true).IsExplored)
                    {
                        //floor that is lit
                        is_explored = true;
                    }
                    //from everywhere else but current tile
                    if (is_explored && (!PosCompare(itemOnFloor.pos, pc_itemOnFloor.pos)))
                    {
                        List<BasePickupItem> list = new List<BasePickupItem>();
                        list.AddRange(itemOnFloor.Storage.Items);
                        foreach (BasePickupItem basePickupItem in list)
                        {
                            //pc_itemOnFloor.Storage.AddItemAndReshuffleOptional(basePickupItem);

                            Fixed_StackItemOnFloor(basePickupItem, pc_itemOnFloor.Storage);
                            //basePickupItem.Storage.Remove(basePickupItem, true);
                            //basePickupItem.Storage = null;
                        }
                    }
                }
                //then sort.

                SpaceTime spaceTime = StateManager.ActiveState.Get<SpaceTime>();
                pc_itemOnFloor.Storage.SortWithExpandByTypeAndName(spaceTime);


                //then open inventory screen.
                v._lastInteractObstacle = null;

                if (pc_itemOnFloor != null && !pc_itemOnFloor.Storage.Empty)
                {
                    v._tabsView.AddTab(v._itemsOnFloorView, pc_itemOnFloor.Storage, TabType.Nymeric, false);
                }
                v._hideAfterItemsOnFloorLooted = (v._tabsView.TabsCount > 0);
                v._tabsView.SelectAndShowFirstTab();

            }).SetBackOnBackgroundClick(true);
        }




        public static void Fixed_StackItemOnFloor(BasePickupItem basePickupItem, ItemStorage itemOnFloor)
        {
            ItemOnFloorSystem.StackItemOnFloor(basePickupItem, itemOnFloor);

            //item somehow have 0 count.
            if (basePickupItem.StackCount <= 0)
            {
                //blow this shit up
                basePickupItem.Storage.Remove(basePickupItem, true);
                basePickupItem.Storage = null;
            }
        }


        public static bool PosCompare(CellPosition cel11, CellPosition cell2)
        {
            return ((cel11.X == cell2.X) && (cel11.Y == cell2.Y));
        }

        public static bool CheckButtonActiveStatus()
        {
            bool active = false;
            //we have basic ground to work with
            if (StateManager.ActiveState != null)
            {
                //in one of the random mission
                RaidMetadata _raidMetadata = StateManager.ActiveState.Get<RaidMetadata>();
                if (_raidMetadata.RaidType == RaidType.ProcMission)
                {
                    //if enemy count check is on
                    if (Enemy_Count_Check_Bool == true)
                    {
                        if (StateManager.ActiveState != null)
                        {
                            Creatures creatures = StateManager.ActiveState.Get<Creatures>();
                            int temp_mon_count = 0;
                            foreach (Monster creature in creatures.Monsters)
                            {
                                if (creature.CreatureData.Health.Alive && !creature.CreatureData.IsAlly(creatures.Player.CreatureData))
                                {
                                    temp_mon_count++;
                                }
                            }
                            if (temp_mon_count <= Enemy_Count_Check_Num)
                            {
                                active = true;
                            }
                        }
                    }
                    //otherwise
                    else
                    {
                        active = true;
                    }
                }
            }
            return active;
        }


    }

    public class WoundSlotAmputateHandler : IWoundSlotCanAmputate
    {
        private readonly bool _canAmputate;

        public WoundSlotAmputateHandler(bool canAmputate)
        {
            _canAmputate = canAmputate;
        }

        public bool CanAmputate() => _canAmputate;
    }
}
