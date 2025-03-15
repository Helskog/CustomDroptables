using CustomDropTables;
using CustomDropTables.Config;
using HarmonyLib;
using ProjectM;
using ProjectM.Shared;
using Stunlock.Core;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using static CustomDropTables.Config.Configuration;

namespace CustomDroptables;

[HarmonyPatch(typeof(DropInInventoryOnSpawnSystem), "OnUpdate")]
public class Patch_DropInInventoryOnSpawnSystem
{
    private static HashSet<PrefabGUID> processedDropTables = new HashSet<PrefabGUID>();
    private static Dictionary<PrefabGUID, DroptableData> DropTableGuids = Configuration.Instance.DropTableGuids;

    static void Prefix(DropInInventoryOnSpawnSystem __instance)
    {

        EntityManager entityManager = __instance.EntityManager;

        EntityQuery serverGameBalanceQuery = entityManager.CreateEntityQuery(ComponentType.ReadWrite<ServerGameBalanceSettings>());
        ServerGameBalanceSettings serverGameSettings = serverGameBalanceQuery.GetSingleton<ServerGameBalanceSettings>();

        EntityQuery query = __instance.GetEntityQuery(
            ComponentType.ReadOnly<DropInInventoryOnSpawn>(),
            ComponentType.ReadOnly<SpawnTag>(),
            ComponentType.ReadWrite<DropTableBuffer>()
        );

        NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            if (Core.EntityManager.TryGetBuffer<DropTableBuffer>(entity, out var dropTableBuffer))
            {
                PrefabGUID entityDroptableGuid = dropTableBuffer[0].DropTableGuid;

                if (DropTableGuids.TryGetValue(entityDroptableGuid, out var configData))
                {
                    if (processedDropTables.Contains(entityDroptableGuid))
                        continue;

                    modifyDataBuffer(entityDroptableGuid, configData, serverGameSettings.DropTableModifier_General);
                    processedDropTables.Add(entityDroptableGuid);

                }
                else
                {
                    continue;
                }
            }
        }

        entities.Dispose();
    }

    private static void modifyDataBuffer(PrefabGUID entityDroptableGuid, DroptableData cfg, half modifier)
    {
        if (Core.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(entityDroptableGuid, out Entity dropTableEntity))
        {
            EntityManager entityManager = Core.EntityManager;
            DynamicBuffer<DropTableDataBuffer> dropTableDataBuffer = entityManager.GetBuffer<DropTableDataBuffer>(dropTableEntity);

            if (cfg.ReplaceAll)
                dropTableDataBuffer.Clear();

            Plugin.Logger.LogMessage($"Modifying droptable {entityDroptableGuid} - buffer length before: " + dropTableDataBuffer.Length);

            foreach (KeyValuePair<PrefabGUID, Configuration.ItemData> itemKvp in cfg.AddItems)
            {
                PrefabGUID itemGuid = itemKvp.Key;
                Configuration.ItemData itemData = itemKvp.Value;

                int spawnQuantity;

                if (itemData.Quantity.IsRange)
                {
                    spawnQuantity = UnityEngine.Random.Range(itemData.Quantity.Min, itemData.Quantity.Max + 1);
                }
                else
                {
                    spawnQuantity = itemData.Quantity.Min;
                }

                // Couldnt find another way to account for global modifier, this will have to do for now.
                int adjustedQuantity = Mathf.RoundToInt(spawnQuantity / modifier);

                dropTableDataBuffer.Add(new DropTableDataBuffer
                {
                    ItemGuid = itemGuid,
                    Quantity = adjustedQuantity,
                    DropRate = itemData.SpawnChance,
                    ItemType = DropItemType.Item
                });
            }

            Plugin.Logger.LogMessage($"Finished modifying droptable {entityDroptableGuid} - buffer length after: " + dropTableDataBuffer.Length);
        }
    }
}
