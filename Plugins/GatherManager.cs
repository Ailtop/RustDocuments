using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Gather Manager", "Mughisi/Ryan/Arainrr", "2.3.0", ResourceId = 675)]
    [Description("Modify the gather multiple of server resources")]
    public class GatherManager : RustPlugin
    {
        //TODO  修复倍数为小数时，得到的不是刚好的。例如石头默认1000个，倍数是1.5，最终只能得到1492个差不多
        //TODO  看看抽油机是否有用？？？
        //TODO  支持权限？？？
        //TODO  支持马粪？？？
        //TODO  添加克隆支持？？？  没钩子

        #region Fields

        private const string WILDCARD = "*";
        private bool configChanged;
        private readonly Dictionary<ExcavatorArm, ExcavatorArmData> excavatorArms = new Dictionary<ExcavatorArm, ExcavatorArmData>();
        private readonly Dictionary<MiningQuarry, MiningQuarryData> miningQuarries = new Dictionary<MiningQuarry, MiningQuarryData>();
        private readonly Dictionary<string, ItemDefinition> validResources = new Dictionary<string, ItemDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, ResourceDispenser.GatherType> validDispensers = new Dictionary<string, ResourceDispenser.GatherType>(StringComparer.OrdinalIgnoreCase)
        {
            ["tree"] = ResourceDispenser.GatherType.Tree,
            ["ore"] = ResourceDispenser.GatherType.Ore,
            ["corpse"] = ResourceDispenser.GatherType.Flesh,
            ["flesh"] = ResourceDispenser.GatherType.Flesh,
        };

        private struct ExcavatorArmData
        {
            public float resourceProductionTickRate;
            public float timeForFullResources;
        }

        private struct MiningQuarryData
        {
            public float processRate;
        }

        private enum GatherRateType
        {
            Dispenser,
            Pickup,
            Quarry,
            Survey,
            Excavator,
            Crop
        }

        #endregion Fields

        #region Oxide Hooks

        private void Init()
        {
            if (configData.cropResourceModifiers.All(x => x.Value <= 0f))
            {
                Unsubscribe(nameof(OnGrowableGathered));
            }
            if (configData.pickupResourceModifiers.All(x => x.Value <= 0f))
            {
                Unsubscribe(nameof(OnCollectiblePickup));
            }
            if (configData.quarryResourceModifiers.All(x => x.Value <= 0f))
            {
                Unsubscribe(nameof(OnQuarryGather));
            }
            if (configData.excavatorResourceModifiers.All(x => x.Value <= 0f))
            {
                Unsubscribe(nameof(OnExcavatorGather));
            }
            if (configData.surveyResourceModifiers.All(x => x.Value <= 0f))
            {
                Unsubscribe(nameof(OnSurveyGather));
            }
            if (configData.gatherResourceModifiers.All(x => x.Value <= 0f)
                && configData.gatherDispenserModifiers.All(x => x.Value <= 0f))
            {
                Unsubscribe(nameof(OnDispenserBonus));
                Unsubscribe(nameof(OnDispenserGather));
            }
            Unsubscribe(nameof(OnEntitySpawned));
            cmd.AddChatCommand(configData.chatS.command, this, nameof(CmdGather));
        }

        private void OnServerInitialized()
        {
            foreach (var itemDefinition in ItemManager.GetItemDefinitions())
            {
                if (itemDefinition.category == ItemCategory.Food || itemDefinition.category == ItemCategory.Resources)
                {
                    validResources.Add(itemDefinition.displayName.english, itemDefinition);
                }
            }

            Subscribe(nameof(OnEntitySpawned));
            foreach (var serverEntity in BaseNetworkable.serverEntities)
            {
                var excavatorArm = serverEntity as ExcavatorArm;
                if (excavatorArm != null)
                {
                    OnEntitySpawned(excavatorArm);
                    continue;
                }

                var miningQuarry = serverEntity as MiningQuarry;
                if (miningQuarry != null)
                {
                    OnEntitySpawned(miningQuarry);
                }
            }
        }

        private void Unload()
        {
            RefreshAllExcavatorArms(true);
            RefreshAllMiningQuarries(true);
            if (configChanged)
            {
                SaveConfig();
            }
        }

        private void OnServerSave()
        {
            if (configChanged)
            {
                timer.Once(UnityEngine.Random.Range(0f, 60f), () =>
                {
                    SaveConfig();
                    configChanged = false;
                });
            }
        }

        private void OnEntitySpawned(MiningQuarry miningQuarry)
        {
            if (miningQuarry != null)
            {
                var miningQuarryData = new MiningQuarryData
                {
                    processRate = miningQuarry.processRate,
                };
                miningQuarries.Add(miningQuarry, miningQuarryData);
                RefreshMiningQuarry(miningQuarry, miningQuarryData);
            }
        }

        private void OnEntitySpawned(ExcavatorArm excavatorArm)
        {
            if (excavatorArm != null)
            {
                var excavatorArmData = new ExcavatorArmData
                {
                    timeForFullResources = excavatorArm.timeForFullResources,
                    resourceProductionTickRate = excavatorArm.resourceProductionTickRate,
                };
                excavatorArms.Add(excavatorArm, excavatorArmData);
                RefreshExcavatorArm(excavatorArm, excavatorArmData);
            }
        }

        private void OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            OnDispenserGather(dispenser, player, item);
        }

        //private void OnDispenserGather(ResourceDispenser dispenser, BasePlayer player, Item item)
        //{
        //    if (player == null || item == null || dispenser == null)
        //    {
        //        return;
        //    }

        //    var amount = item.amount;
        //    var gatherType = dispenser.gatherType.ToString("G");

        //    float modifier;
        //    if (configData.gatherResourceModifiers.TryGetValue(item.info.displayName.english, out modifier))
        //    {
        //        item.amount = (int)(item.amount * modifier);
        //    }
        //    else if (configData.gatherResourceModifiers.TryGetValue(WILDCARD, out modifier))
        //    {
        //        item.amount = (int)(item.amount * modifier);
        //    }
        //    if (!configData.gatherResourceModifiers.ContainsKey(gatherType))
        //    {
        //        return;
        //    }

        //var dispenserModifier = GatherDispenserModifiers[gatherType];
        //PrintError($"amount:{amount}. item.amount:{item.amount}. dispenserModifier:{dispenserModifier}. singleAmount:{dispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount}");
        //try
        //{
        //    dispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount += amount - item.amount / dispenserModifier;

        //    if (dispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount< 0)
        //    {
        //        item.amount += (int) dispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount;
        //    }
        //    PrintError($"item.amount:{item.amount}. singleAmount:{dispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount}.");
        //}
        //catch { }
        //}
        private void OnDispenserGather(ResourceDispenser resourceDispenser, BasePlayer player, Item item)
        {
            if (item == null || resourceDispenser == null)
            {
                return;
            }
            var amount = item.amount;
            var gatherType = resourceDispenser.gatherType.ToString();

            float modifier;
            var existing = configData.gatherResourceModifiers.TryGetValue(item.info.displayName.english, out modifier);
            if (existing && modifier > 0f)
            {
                item.amount = (int)(item.amount * modifier);
            }
            else
            {
                if (configData.gatherResourceModifiers.TryGetValue(WILDCARD, out modifier) && modifier > 0f)
                {
                    item.amount = (int)(item.amount * modifier);
                }
                if (!existing)
                {
                    configData.gatherResourceModifiers.Add(item.info.displayName.english, 0f);
                    configChanged = true;
                }
            }

            existing = configData.gatherDispenserModifiers.TryGetValue(gatherType, out modifier);
            if (existing && modifier > 0f)
            {
                if (resourceDispenser.containedItems != null)
                {
                    //var singleItem = resourceDispenser.containedItems.Single(x => x.itemid == item.info.itemid);
                    //singleItem.amount += amount - item.amount / 2;
                    //if (singleItem.amount < 0) {
                    //    item.amount += (int)singleItem.amount;
                    //}

                    PrintError($"amount:{amount}. item.amount:{item.amount}. modifier:{modifier}. singleAmount:{resourceDispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount}");
                    foreach (var containedItem in resourceDispenser.containedItems)
                    {
                        if (containedItem.itemid == item.info.itemid)
                        {
                            containedItem.amount += amount - item.amount / modifier;
                            if (containedItem.amount < 0f)
                            {
                                item.amount += (int)containedItem.amount;
                            }
                        }
                    }
                    PrintError($"item.amount:{item.amount}. singleAmount:{resourceDispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount}.");
                }
            }
            else
            {
                if (configData.gatherDispenserModifiers.TryGetValue(WILDCARD, out modifier) && modifier > 0f)
                {
                    if (resourceDispenser.containedItems != null)
                    {
                        foreach (var containedItem in resourceDispenser.containedItems)
                        {
                            if (containedItem.itemid == item.info.itemid)
                            {
                                containedItem.amount += amount - item.amount / modifier;
                                if (containedItem.amount < 0f)
                                {
                                    item.amount += (int)containedItem.amount;
                                }
                            }
                        }
                    }
                }
                if (!existing)
                {
                    configData.gatherDispenserModifiers.Add(gatherType, 0f);
                    configChanged = true;
                }
            }
        }

        private void OnGrowableGathered(GrowableEntity growable, Item item, BasePlayer player)
        {
            if (item == null) return;
            float modifier;
            var existing = configData.cropResourceModifiers.TryGetValue(item.info.displayName.english, out modifier);
            if (existing && modifier > 0f)
            {
                item.amount = (int)(item.amount * modifier);
            }
            else
            {
                if (configData.cropResourceModifiers.TryGetValue(WILDCARD, out modifier) && modifier > 0f)
                {
                    item.amount = (int)(item.amount * modifier);
                }
                if (!existing)
                {
                    configData.cropResourceModifiers.Add(item.info.displayName.english, 0f);
                    configChanged = true;
                }
            }
        }

        private void OnCollectiblePickup(Item item, BasePlayer receiver, CollectibleEntity collectibleEntity)
        {
            if (item == null) return;
            float modifier;
            var existing = configData.pickupResourceModifiers.TryGetValue(item.info.displayName.english, out modifier);
            if (existing && modifier > 0f)
            {
                item.amount = (int)(item.amount * modifier);
            }
            else
            {
                if (configData.pickupResourceModifiers.TryGetValue(WILDCARD, out modifier) && modifier > 0f)
                {
                    item.amount = (int)(item.amount * modifier);
                }
                if (!existing)
                {
                    configData.pickupResourceModifiers.Add(item.info.displayName.english, 0f);
                    configChanged = true;
                }
            }
        }

        private void OnQuarryGather(MiningQuarry quarry, Item item)
        {
            if (item == null) return;
            float modifier;
            var existing = configData.quarryResourceModifiers.TryGetValue(item.info.displayName.english, out modifier);
            if (existing && modifier > 0f)
            {
                item.amount = (int)(item.amount * modifier);
            }
            else
            {
                if (configData.quarryResourceModifiers.TryGetValue(WILDCARD, out modifier) && modifier > 0f)
                {
                    item.amount = (int)(item.amount * modifier);
                }
                if (!existing)
                {
                    configData.quarryResourceModifiers.Add(item.info.displayName.english, 0f);
                    configChanged = true;
                }
            }
        }

        private void OnSurveyGather(SurveyCharge surveyCharge, Item item)
        {
            if (item == null) return;
            float modifier;
            var existing = configData.surveyResourceModifiers.TryGetValue(item.info.displayName.english, out modifier);
            if (existing && modifier > 0f)
            {
                item.amount = (int)(item.amount * modifier);
            }
            else
            {
                if (configData.surveyResourceModifiers.TryGetValue(WILDCARD, out modifier) && modifier > 0f)
                {
                    item.amount = (int)(item.amount * modifier);
                }
                if (!existing)
                {
                    configData.surveyResourceModifiers.Add(item.info.displayName.english, 0f);
                    configChanged = true;
                }
            }
        }

        private void OnExcavatorGather(ExcavatorArm excavatorArm, Item item)
        {
            if (item == null) return;
            float modifier;
            var existing = configData.excavatorResourceModifiers.TryGetValue(item.info.displayName.english, out modifier);
            if (existing && modifier > 0f)
            {
                item.amount = (int)(item.amount * modifier);
            }
            else
            {
                if (configData.excavatorResourceModifiers.TryGetValue(WILDCARD, out modifier) && modifier > 0f)
                {
                    item.amount = (int)(item.amount * modifier);
                }
                if (!existing)
                {
                    configData.excavatorResourceModifiers.Add(item.info.displayName.english, 0f);
                    configChanged = true;
                }
            }
        }

        #endregion Oxide Hooks

        #region Methods

        private void RefreshAllExcavatorArms(bool unload = false)
        {
            foreach (var entry in excavatorArms)
            {
                var excavatorArm = entry.Key;
                var excavatorArmData = entry.Value;
                RefreshExcavatorArm(excavatorArm, excavatorArmData, unload);
            }
        }

        private void RefreshAllMiningQuarries(bool unload = false)
        {
            foreach (var entry in miningQuarries)
            {
                var miningQuarry = entry.Key;
                var miningQuarryData = entry.Value;
                RefreshMiningQuarry(miningQuarry, miningQuarryData, unload);
            }
        }

        private void RefreshExcavatorArm(ExcavatorArm excavatorArm, ExcavatorArmData excavatorArmData, bool unload = false)
        {
            if (excavatorArm == null || excavatorArm.IsDestroyed) return;
            if (unload)
            {
                if (excavatorArm.IsOn() && excavatorArm.resourceProductionTickRate != excavatorArmData.resourceProductionTickRate)
                {
                    excavatorArm.CancelInvoke(excavatorArm.ProduceResources);
                    excavatorArm.InvokeRepeating(excavatorArm.ProduceResources, excavatorArmData.resourceProductionTickRate, excavatorArmData.resourceProductionTickRate);
                }
                excavatorArm.resourceProductionTickRate = excavatorArmData.resourceProductionTickRate;
                excavatorArm.timeForFullResources = excavatorArmData.timeForFullResources;
            }
            else
            {
                if (configData.excavatorTickRate <= 0f)
                {
                    configData.excavatorTickRate = excavatorArm.resourceProductionTickRate;
                    configChanged = true;
                }
                else if (configData.excavatorTickRate != excavatorArm.resourceProductionTickRate)
                {
                    if (excavatorArm.IsOn())
                    {
                        excavatorArm.CancelInvoke(excavatorArm.ProduceResources);
                        excavatorArm.InvokeRepeating(excavatorArm.ProduceResources, configData.excavatorTickRate, configData.excavatorTickRate);
                    }
                    excavatorArm.resourceProductionTickRate = configData.excavatorTickRate;
                }

                if (configData.excavatorTimeForFullResources <= 0f)
                {
                    configData.excavatorTimeForFullResources = excavatorArm.timeForFullResources;
                    configChanged = true;
                }
                else if (configData.excavatorTimeForFullResources != excavatorArm.timeForFullResources)
                {
                    excavatorArm.timeForFullResources = configData.excavatorTimeForFullResources;
                }
            }
        }

        private void RefreshMiningQuarry(MiningQuarry miningQuarry, MiningQuarryData miningQuarryData, bool unload = false)
        {
            if (miningQuarry == null || miningQuarry.IsDestroyed) return;
            if (unload)
            {
                if (miningQuarry.IsOn() && miningQuarry.processRate != miningQuarryData.processRate)
                {
                    miningQuarry.CancelInvoke(miningQuarry.ProcessResources);
                    miningQuarry.InvokeRepeating(miningQuarry.ProcessResources, miningQuarryData.processRate, miningQuarryData.processRate);
                }
                miningQuarry.processRate = miningQuarryData.processRate;
            }
            else
            {
                if (configData.quarryTickRate <= 0f)
                {
                    configData.quarryTickRate = miningQuarry.processRate;
                    configChanged = true;
                }
                else if (configData.quarryTickRate != miningQuarry.processRate)
                {
                    if (miningQuarry.IsOn())
                    {
                        miningQuarry.CancelInvoke(miningQuarry.ProcessResources);
                        miningQuarry.InvokeRepeating(miningQuarry.ProcessResources, configData.quarryTickRate, configData.quarryTickRate);
                    }
                    miningQuarry.processRate = configData.quarryTickRate;
                }
            }
        }

        #endregion Methods

        #region Commands

        private void CmdGather(BasePlayer player, string command, string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Lang("HelpTextPlayer", player.UserIDString));

            sb.AppendLine(Lang("HelpTextPlayerGains", player.UserIDString, Lang("Dispensers", player.UserIDString)));
            if (configData.gatherResourceModifiers.Any(x => x.Value > 0f))
            {
                foreach (var entry in configData.gatherResourceModifiers)
                {
                    if (entry.Value > 0f)
                    {
                        sb.AppendLine(Lang("HelpEntryFormat", player.UserIDString, entry.Key, entry.Value));
                    }
                }
            }
            else
            {
                sb.AppendLine(Lang("HelpTextPlayerDefault", player.UserIDString));
            }

            sb.AppendLine(Lang("HelpTextPlayerGains", player.UserIDString, Lang("Pickups", player.UserIDString)));
            if (configData.pickupResourceModifiers.Any(x => x.Value > 0f))
            {
                foreach (var entry in configData.pickupResourceModifiers)
                {
                    if (entry.Value > 0f)
                    {
                        sb.AppendLine(Lang("HelpEntryFormat", player.UserIDString, entry.Key, entry.Value));
                    }
                }
            }
            else
            {
                sb.AppendLine(Lang("HelpTextPlayerDefault", player.UserIDString));
            }

            sb.AppendLine(Lang("HelpTextPlayerGains", player.UserIDString, Lang("MiningQuarries", player.UserIDString)));
            if (configData.quarryResourceModifiers.Any(x => x.Value > 0f))
            {
                foreach (var entry in configData.quarryResourceModifiers)
                {
                    if (entry.Value > 0f)
                    {
                        sb.AppendLine(Lang("HelpEntryFormat", player.UserIDString, entry.Key, entry.Value));
                    }
                }
            }
            else
            {
                sb.AppendLine(Lang("HelpTextPlayerDefault", player.UserIDString));
            }

            sb.AppendLine(Lang("HelpTextPlayerGains", player.UserIDString, Lang("Excavators", player.UserIDString)));
            if (configData.excavatorResourceModifiers.Any(x => x.Value > 0f))
            {
                foreach (var entry in configData.excavatorResourceModifiers)
                {
                    if (entry.Value > 0f)
                    {
                        sb.AppendLine(Lang("HelpEntryFormat", player.UserIDString, entry.Key, entry.Value));
                    }
                }
            }
            else
            {
                sb.AppendLine(Lang("HelpTextPlayerDefault", player.UserIDString));
            }

            sb.AppendLine(Lang("HelpTextPlayerGains", player.UserIDString, Lang("SurveyCharges", player.UserIDString)));
            if (configData.surveyResourceModifiers.Any(x => x.Value > 0f))
            {
                foreach (var entry in configData.surveyResourceModifiers)
                {
                    if (entry.Value > 0f)
                    {
                        sb.AppendLine(Lang("HelpEntryFormat", player.UserIDString, entry.Key, entry.Value));
                    }
                }
            }
            else
            {
                sb.AppendLine(Lang("HelpTextPlayerDefault", player.UserIDString));
            }

            sb.AppendLine(Lang("HelpTextPlayerGains", player.UserIDString, Lang("Crops", player.UserIDString)));
            if (configData.cropResourceModifiers.Any(x => x.Value > 0f))
            {
                foreach (var entry in configData.cropResourceModifiers)
                {
                    if (entry.Value > 0f)
                    {
                        sb.AppendLine(Lang("HelpEntryFormat", player.UserIDString, entry.Key, entry.Value));
                    }
                }
            }
            else
            {
                sb.AppendLine(Lang("HelpTextPlayerDefault", player.UserIDString));
            }

            if (miningQuarries.Count > 0)
            {
                var defaultQuarryTickRate = miningQuarries.FirstOrDefault().Value.processRate;
                if (defaultQuarryTickRate > 0f && configData.quarryTickRate != defaultQuarryTickRate)
                {
                    sb.AppendLine(Lang("HelpTextMiningQuarryTickRate", player.UserIDString, configData.quarryTickRate));
                }
            }
            if (excavatorArms.Count > 0)
            {
                var defaultExcavatorTickRate = excavatorArms.FirstOrDefault().Value.resourceProductionTickRate;
                if (defaultExcavatorTickRate > 0f && configData.excavatorTickRate != defaultExcavatorTickRate)
                {
                    sb.AppendLine(Lang("HelpTextExcavatorTickRate", player.UserIDString, configData.excavatorTickRate));
                }
            }
            Print(player, sb.ToString());
            if (player.IsAdmin)
            {
                Print(player, Lang("HelpTextAdmin", player.UserIDString));
            }
        }

        [ConsoleCommand("gather.rate")]
        private void CCmdGatherRate(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player != null && !player.IsAdmin)
            {
                Print(arg, "You don't have permission to use this command.");
                return;
            }
            if (!arg.HasArgs(3))
            {
                Print(arg, "Syntax error!!! Usage: gather.rate <type: dispenser|pickup|quarry|survey|crop> <resource> <multiplier>");
                return;
            }
            var rateType = arg.GetString(0);
            GatherRateType gatherRateType;
            if (!Enum.TryParse(rateType, true, out gatherRateType))
            {
                Print(arg, $"{rateType} is not a valid rate type. type: dispenser|pickup|quarry|survey|crop");
                return;
            }

            var resource = arg.GetString(1);
            ItemDefinition resourceItemDef = null;
            if (resource != WILDCARD && !validResources.TryGetValue(resource, out resourceItemDef))
            {
                Print(arg, $"{resource} is not a valid resource. Check the 'gather.resources' console command for a list of available options.");
                return;
            }
            resource = resourceItemDef != null ? resourceItemDef.displayName.english : WILDCARD;
            var modifier = arg.GetFloat(2);
            var remove = false;
            if (modifier <= 0f)
            {
                if (arg.GetString(2).ToLower() == "remove")
                {
                    remove = true;
                }
                else
                {
                    Print(arg, "You can't set the modifier lower than 0!");
                    return;
                }
            }

            switch (gatherRateType)
            {
                case GatherRateType.Dispenser:
                    if (remove)
                    {
                        configData.gatherResourceModifiers.Remove(resource);
                        Print(arg, $"You have reset the gather rate for {resource} from Resource Dispensers.");
                    }
                    else
                    {
                        configData.gatherResourceModifiers[resource] = modifier;
                        Print(arg, $"You have set the gather rate for {resource} to x{modifier} from Dispensers.");
                    }
                    break;

                case GatherRateType.Pickup:
                    if (remove)
                    {
                        configData.pickupResourceModifiers.Remove(resource);
                        Print(arg, $"You have reset the gather rate for {resource} from Pickups.");
                    }
                    else
                    {
                        configData.pickupResourceModifiers[resource] = modifier;
                        Print(arg, $"You have set the gather rate for {resource} to x{modifier} from Pickups.");
                    }
                    break;

                case GatherRateType.Survey:
                    if (remove)
                    {
                        configData.surveyResourceModifiers.Remove(resource);
                        Print(arg, $"You have reset the gather rate for {resource} from Survey Charges.");
                    }
                    else
                    {
                        configData.surveyResourceModifiers[resource] = modifier;
                        Print(arg, $"You have set the gather rate for {resource} to x{modifier} from Survey Charges.");
                    }
                    break;

                case GatherRateType.Quarry:
                    if (remove)
                    {
                        configData.quarryResourceModifiers.Remove(resource);
                        Print(arg, $"You have reset the gather rate for {resource} from Mining Quarries.");
                    }
                    else
                    {
                        configData.quarryResourceModifiers[resource] = modifier;
                        Print(arg, $"You have set the gather rate for {resource} to x{modifier} from Quarries.");
                    }
                    break;

                case GatherRateType.Excavator:
                    if (remove)
                    {
                        configData.excavatorResourceModifiers.Remove(resource);
                        Print(arg, $"You have reset the gather rate for {resource} from Excavators.");
                    }
                    else
                    {
                        configData.excavatorResourceModifiers[resource] = modifier;
                        Print(arg, $"You have set the gather rate for {resource} to x{modifier} from Excavators.");
                    }
                    break;

                case GatherRateType.Crop:
                    if (remove)
                    {
                        configData.cropResourceModifiers.Remove(resource);
                        Print(arg, $"You have reset the gather rate for {resource} from Crops.");
                    }
                    else
                    {
                        configData.cropResourceModifiers[resource] = modifier;
                        Print(arg, $"You have set the gather rate for {resource} to x{modifier} from Crops.");
                    }
                    break;
            }
            configChanged = true;
        }

        [ConsoleCommand("dispenser.scale")]
        private void CCmdDispenserRate(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player != null && !player.IsAdmin)
            {
                Print(arg, "You don't have permission to use this command.");
                return;
            }

            if (!arg.HasArgs(2))
            {
                Print(arg, "Syntax error!!! Usage: dispenser.scale <dispenser: tree|ore|corpse> <multiplier>");
                return;
            }

            var dispenser = arg.GetString(0);
            ResourceDispenser.GatherType gatherType;
            if (!validDispensers.TryGetValue(dispenser, out gatherType))
            {
                Print(arg, $"{dispenser} is not a valid dispenser. Check the 'gather.dispensers' console command for a list of available options.");
                return;
            }

            var modifier = arg.GetFloat(1);
            if (modifier <= 0f)
            {
                Print(arg, "You can't set the modifier lower than 0!");
                return;
            }
            configData.gatherDispenserModifiers[gatherType.ToString()] = modifier;
            configChanged = true;
            Print(arg, $"You have set the resource amount for {dispenser} dispensers to x{modifier}");
        }

        [ConsoleCommand("quarry.tickrate")]
        private void CCmdMiningQuarryTickRate(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player != null && !player.IsAdmin)
            {
                return;
            }

            if (!arg.HasArgs())
            {
                Print(arg, "Syntax error!!! Usage: quarry.tickrate <time between gathers in seconds>");
                return;
            }

            var modifier = arg.GetFloat(0);
            if (modifier <= 0f)
            {
                Print(arg, "You can't set the tick rate lower than 0!");
                return;
            }

            configData.quarryTickRate = modifier;
            configChanged = true;
            RefreshAllMiningQuarries();
            Print(arg, $"The Mining Quarry will now provide resources every {modifier} second(s).");
        }

        [ConsoleCommand("excavator.tickrate")]
        private void CCmdExcavatorTickRate(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player != null && !player.IsAdmin)
            {
                Print(arg, "You don't have permission to use this command.");
                return;
            }

            if (!arg.HasArgs())
            {
                Print(arg, "Syntax error!!! Usage: excavator.tickrate <time between gathers in seconds>");
                return;
            }

            var modifier = arg.GetFloat(0);
            if (modifier <= 0f)
            {
                Print(arg, "You can't set the tick rate lower than 0!");
                return;
            }

            configData.excavatorTickRate = modifier;
            configChanged = true;
            RefreshAllExcavatorArms();
            Print(arg, $"The Excavator will now provide resources every {modifier} second(s).");
        }

        [ConsoleCommand("gather.resources")]
        private void CCmdGatherResources(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player != null && !player.IsAdmin)
            {
                Print(arg, "You don't have permission to use this command.");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available resources:");
            foreach (var entry in validResources)
            {
                sb.AppendLine(entry.Value.displayName.english);
            }
            sb.AppendLine("* (For all resources that are not setup separately)");
            Print(arg, sb.ToString());
        }

        [ConsoleCommand("gather.dispensers")]
        private void CCmdGatherDispensers(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player != null && !player.IsAdmin)
            {
                Print(arg, "You don't have permission to use this command.");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available dispensers:");
            var gatherTypes = new HashSet<ResourceDispenser.GatherType>(validDispensers.Values);
            foreach (var gatherType in gatherTypes)
            {
                sb.AppendLine(gatherType.ToString());
            }
            Print(arg, sb.ToString());
        }

        #endregion Commands

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Mining Quarry Resource Tick Rate")]
            public float quarryTickRate;//10

            [JsonProperty(PropertyName = "Excavator Resource Tick Rate")]
            public float excavatorTickRate;//1

            [JsonProperty(PropertyName = "Excavator Time For Full Resources")]
            public float excavatorTimeForFullResources;//120

            [JsonProperty(PropertyName = "Gather Dispenser Modifiers")]
            public Dictionary<string, float> gatherDispenserModifiers = new Dictionary<string, float>();

            [JsonProperty(PropertyName = "Gather Resource Modifiers")]
            public Dictionary<string, float> gatherResourceModifiers = new Dictionary<string, float>();

            [JsonProperty(PropertyName = "Crop Resource Modifiers")]
            public Dictionary<string, float> cropResourceModifiers = new Dictionary<string, float>();

            [JsonProperty(PropertyName = "Pickup Resource Modifiers")]
            public Dictionary<string, float> pickupResourceModifiers = new Dictionary<string, float>();

            [JsonProperty(PropertyName = "Survey Resource Modifiers")]
            public Dictionary<string, float> surveyResourceModifiers = new Dictionary<string, float>();

            [JsonProperty(PropertyName = "Quarry Resource Modifiers")]
            public Dictionary<string, float> quarryResourceModifiers = new Dictionary<string, float>();

            [JsonProperty(PropertyName = "Excavator Resource Modifiers")]
            public Dictionary<string, float> excavatorResourceModifiers = new Dictionary<string, float>();

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatSettings chatS = new ChatSettings();

            public class ChatSettings
            {
                [JsonProperty(PropertyName = "Chat Command")]
                public string command = "gather";

                [JsonProperty(PropertyName = "Chat Prefix")]
                public string prefix = "<color=#008000>[Gather Manager]</color>: ";

                [JsonProperty(PropertyName = "Chat SteamID Icon")]
                public ulong steamIDIcon = 0;
            }

            [JsonProperty(PropertyName = "Version")]
            public VersionNumber version;
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                configData = Config.ReadObject<ConfigData>();
                if (configData == null)
                {
                    LoadDefaultConfig();
                }
                else
                {
                    UpdateConfigValues();
                }
            }
            catch (Exception ex)
            {
                PrintError($"The configuration file is corrupted. \n{ex}");
                LoadDefaultConfig();
            }
            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file");
            configData = new ConfigData();
            configData.version = Version;
        }

        protected override void SaveConfig() => Config.WriteObject(configData);

        private void UpdateConfigValues()
        {
            if (configData.version < Version)
            {
                if (configData.version <= default(VersionNumber))
                {
                    float value;
                    if (GetConfigValue(out value, "Options", "ExcavatorResourceTickRate"))
                    {
                        configData.excavatorTickRate = value;
                    }
                    if (GetConfigValue(out value, "Options", "ExcavatorTimeForFullResources"))
                    {
                        configData.excavatorTimeForFullResources = value;
                    }
                    if (GetConfigValue(out value, "Options", "MiningQuarryResourceTickRate"))
                    {
                        configData.quarryTickRate = value;
                    }

                    //这个是不支持的
                    Dictionary<string, float> values;
                    if (GetConfigValue(out values, "Options", "ExcavatorResourceModifiers"))
                    {
                        configData.excavatorResourceModifiers = values;
                    }
                    if (GetConfigValue(out values, "Options", "PickupResourceModifiers"))
                    {
                        configData.pickupResourceModifiers = values;
                    }
                    if (GetConfigValue(out values, "Options", "QuarryResourceModifiers"))
                    {
                        configData.quarryResourceModifiers = values;
                    }
                    if (GetConfigValue(out values, "Options", "SurveyResourceModifiers"))
                    {
                        configData.surveyResourceModifiers = values;
                    }
                    if (GetConfigValue(out values, "Options", "GatherDispenserModifiers"))
                    {
                        configData.gatherDispenserModifiers = values;
                    }
                    if (GetConfigValue(out values, "Options", "GatherResourceModifiers"))
                    {
                        var crops = new List<string>
                        {
                            "Corn",
                            "Cloth",
                            "Potato",
                            "Pumpkin",
                            "Red Berry",
                            "White Berry",
                            "Green Berry",
                            "Blue Berry",
                            "Yellow Berry",
                            "Black Berry",
                        };
                        foreach (var entry in values)
                        {
                            if (entry.Key == WILDCARD)
                            {
                                configData.cropResourceModifiers.Add(entry.Key, entry.Value);
                                configData.gatherResourceModifiers.Add(entry.Key, entry.Value);
                                continue;
                            }
                            if (crops.Contains(entry.Key))
                            {
                                configData.cropResourceModifiers.Add(entry.Key, entry.Value);
                            }
                            else
                            {
                                configData.gatherResourceModifiers.Add(entry.Key, entry.Value);
                            }
                        }
                    }

                    string prefix, prefixColor;
                    if (GetConfigValue(out prefix, "Settings", "ChatPrefix") && GetConfigValue(out prefixColor, "Settings", "ChatPrefixColor"))
                    {
                        configData.chatS.prefix = $"<color={prefixColor.Substring(0, 7)}>{prefix}</color>: ";
                    }
                }

                configData.version = Version;
            }
        }

        private bool GetConfigValue<T>(out T value, params string[] path)
        {
            var configValue = Config.Get(path);
            if (configValue == null)
            {
                value = default(T);
                return false;
            }
            value = Config.ConvertValue<T>(configValue);
            return true;
        }

        #endregion ConfigurationFile

        //USED BY HELP TEXT PLUGIN
        private void SendHelpText(BasePlayer player) => Print(player, Lang("HelpText", player.UserIDString, configData.chatS.command));

        #region LanguageFile

        private void Print(BasePlayer player, string message)
        {
            Player.Message(player, message, configData.chatS.prefix, configData.chatS.steamIDIcon);
        }

        private void Print(ConsoleSystem.Arg arg, string message)
        {
            //SendReply(arg, message);
            var player = arg.Player();
            if (player == null) Puts(message);
            else PrintToConsole(player, message);
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                //["NotAllowed"] = "You don't have permission to use this command.",
                ["Crops"] = "Crops",
                ["Pickups"] = "Pickups",
                ["Excavators"] = "Excavators",
                ["SurveyCharges"] = "Survey Charges",
                ["MiningQuarries"] = "Mining Quarries",
                ["Dispensers"] = "Resource Dispensers",
                ["HelpText"] = "/{0} - Shows you detailed gather information.",
                ["HelpTextPlayer"] = "Resources gained from gathering have been scaled to the following:",
                ["HelpTextPlayerGains"] = "Resources gained from <color=#009EFF>{0}</color>:",
                ["HelpTextPlayerDefault"] = "   <color=#C4FF00>Default values</color>",
                ["HelpEntryFormat"] = "    <color=#FFFF00>{0}</color>: <color=#FF6347>x{1}</color>",
                ["HelpTextExcavatorTickRate"] = "Time between Excavator gathers: <color=#009EFF>{0}</color> second(s).",
                ["HelpTextMiningQuarryTickRate"] = "Time between Mining Quarry gathers: <color=#009EFF>{0}</color> second(s).",
                ["HelpTextAdmin"] = "\nTo change the resources gained by gathering use the command:\n" +
                                    "<color=#009EFF>gather.rate <type: dispenser|pickup|quarry|survey|crop> <resource> <multiplier></color>\n" +
                                    "To change the amount of resources in a dispenser type use the command:\n" +
                                    "<color=#009EFF>dispenser.scale <dispenser: tree|ore|corpse> <multiplier></color>\n" +
                                    "To change the time between Mining Quarry gathers:\n" +
                                    "<color=#009EFF>quarry.tickrate <seconds></color>\n" +
                                    "To change the time between Excavator gathers:\n" +
                                    "<color=#009EFF>excavator.tickrate <seconds></color>\n",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                //["NotAllowed"] = "您没有使用该命令的权限",
                ["Dispensers"] = "采集物",
                ["Crops"] = "农作物",
                ["Pickups"] = "可拾取物品",
                ["Excavators"] = "挖掘机",
                ["SurveyCharges"] = "矿坑",
                ["MiningQuarries"] = "矿机",
                ["HelpText"] = "/{0} - 显示服务器的采集倍数信息",
                ["HelpTextPlayer"] = "服务器各类资源倍数如下：",
                ["HelpTextPlayerGains"] = "<color=#009EFF>{0}</color> 倍数:",
                ["HelpTextPlayerDefault"] = "   <color=#C4FF00>默认值</color>",
                ["HelpEntryFormat"] = "    <color=#FFFF00>{0}</color>: <color=#FF6347>x{1}</color>",
                ["HelpTextMiningQuarryTickRate"] = "挖矿机采集资源的间隔: <color=#009EFF>{0}</color> second(s).",
                ["HelpTextExcavatorTickRate"] = "挖掘机采集资源的间隔: <color=#009EFF>{0}</color> second(s).",
                ["HelpTextAdmin"] = "\n通过以下命令来修改采集资源倍数:\n" +
                                    "<color=#009EFF>gather.rate <类型: dispenser|pickup|quarry|survey|crop> <资源名称> <倍数></color>\n" +
                                    "通过以下命令来修改资源分配器倍数:\n" +
                                    "<color=#009EFF>dispenser.scale <类型: tree|ore|corpse> <multiplier></color>\n" +
                                    "通过以下命令来修改挖矿机采集资源的间隔:\n" +
                                    "<color=#009EFF>quarry.tickrate <时间（秒）></color>\n" +
                                    "通过以下命令来修改挖掘机采集资源的间隔:\n" +
                                    "<color=#009EFF>excavator.tickrate <时间（秒）></color>\n",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}