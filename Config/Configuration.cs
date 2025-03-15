using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomDropTables.Config;

public struct RangeInt
{
    public int Min { get; set; }
    public int Max { get; set; }

    public bool IsRange => Min != Max;
}

public class Configuration
{
    [JsonPropertyName("droptable_guids")]
    public Dictionary<int, IntDroptableData> IntDropTableGuids { get; private set; } = new Dictionary<int, IntDroptableData>();
    public Dictionary<PrefabGUID, DroptableData> DropTableGuids { get; private set; } = new Dictionary<PrefabGUID, DroptableData>();

    public class IntDroptableData
    {
        [JsonPropertyName("replace_all")]
        public bool ReplaceAll { get; set; }

        [JsonPropertyName("add_items")]
        public Dictionary<string, IntItemData> AddItems { get; set; } = new Dictionary<string, IntItemData>();
    }

    public class IntItemData
    {
        [JsonPropertyName("quantity")]
        [JsonConverter(typeof(JsonStringToRangeIntConverter))]
        public RangeInt Quantity { get; set; }

        [JsonPropertyName("spawnchance")]
        public float SpawnChance { get; set; }
    }

    public class DroptableData
    {
        [JsonPropertyName("replace_all")]
        public bool ReplaceAll { get; set; }

        [JsonPropertyName("add_items")]
        public Dictionary<PrefabGUID, ItemData> AddItems { get; set; } = new Dictionary<PrefabGUID, ItemData>();
    }

    public class ItemData
    {
        [JsonPropertyName("quantity")]
        [JsonConverter(typeof(JsonStringToRangeIntConverter))]
        public RangeInt Quantity { get; set; }

        [JsonPropertyName("spawnchance")]
        public float SpawnChance { get; set; }
    }

    private static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
    private static readonly string USERS_PATH = Path.Combine(CONFIG_PATH, "Configuration.json");
    private static readonly string CONFIG_RESOURCE = "CustomDropTables.Config.SampleConfiguration.json";

    private Configuration configuration { get; }
    private static Configuration _instance;
    public static Configuration Instance => _instance ??= new Configuration();

    private Configuration()
    {
        LoadConfiguration();
    }

    public void LoadConfiguration()
    {
        Directory.CreateDirectory(CONFIG_PATH);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new JsonStringToIntConverter());
        options.Converters.Add(new JsonStringToFloatConverter());
        options.Converters.Add(new JsonStringToPrefabGUIDConverter());
        options.Converters.Add(new JsonStringToRangeIntConverter());

        if (!File.Exists(USERS_PATH) || new FileInfo(USERS_PATH).Length == 0)
        {
            CreateSampleConfiguration(options);
        }

        try
        {
            string json = File.ReadAllText(USERS_PATH);
            IntDropTableGuids = JsonSerializer.Deserialize<Dictionary<int, IntDroptableData>>(json, options);

            if (IntDropTableGuids == null)
            {
                Plugin.Logger.LogError("Deserialization failed. Creating sample configuration.");
                CreateSampleConfiguration(options);
            }
            else
            {
                ConvertToPrefabGUIDData();
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Error loading configuration: {ex}");
            CreateSampleConfiguration(options);
        }
    }

    private void CreateSampleConfiguration(JsonSerializerOptions options)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(CONFIG_RESOURCE))
            {
                if (stream == null)
                {
                    Plugin.Logger.LogError($"Sample configuration resource '{CONFIG_RESOURCE}' not found.");
                    return;
                }

                using (var reader = new StreamReader(stream))
                {
                    string sampleJson = reader.ReadToEnd();

                    Plugin.Logger.LogMessage("Created new sample config for droptables: ");
                    Plugin.Logger.LogMessage(sampleJson);

                    File.WriteAllText(USERS_PATH, sampleJson);

                    IntDropTableGuids = JsonSerializer.Deserialize<Dictionary<int, IntDroptableData>>(sampleJson, options);

                    if (IntDropTableGuids == null)
                    {
                        Plugin.Logger.LogError("Error deserializing sample configuration.");
                    }
                    else
                    {
                        ConvertToPrefabGUIDData();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Error creating sample configuration: {ex}");
        }
    }

    private void ConvertToPrefabGUIDData()
    {
        DropTableGuids = new Dictionary<PrefabGUID, DroptableData>();
        foreach (var kvp in IntDropTableGuids)
        {
            var droptableData = new DroptableData
            {
                ReplaceAll = kvp.Value.ReplaceAll,
                AddItems = new Dictionary<PrefabGUID, ItemData>()
            };

            foreach (var itemKvp in kvp.Value.AddItems)
            {
                if (int.TryParse(itemKvp.Key, out int itemKeyInt))
                {
                    droptableData.AddItems.Add(new PrefabGUID(itemKeyInt), new ItemData
                    {
                        Quantity = itemKvp.Value.Quantity,
                        SpawnChance = itemKvp.Value.SpawnChance
                    });
                }
                else
                {
                    Plugin.Logger.LogError($"Failed to parse item key '{itemKvp.Key}' to integer.");
                }
            }
            DropTableGuids.Add(new PrefabGUID(kvp.Key), droptableData);
        }
    }
}