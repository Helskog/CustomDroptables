using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using CustomDropTables.Config;
using HarmonyLib;

namespace CustomDropTables;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.Bloodstone")]
[Bloodstone.API.Reloadable]

public class Plugin : BasePlugin
{
    public static ManualLogSource Logger;

    static Harmony _harmony;
    public static Harmony Harmony => _harmony;

    internal static Plugin Instance { get; private set; }

    public override void Load()
    {
        Logger = Log;
        Instance = this; 

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

        Configuration.Instance.LoadConfiguration();

        Log.LogMessage("Configured custom droptables: ");
        foreach (var i in Configuration.Instance.DropTableGuids)
        {
            Plugin.Logger.LogMessage(i.Key);
        }

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        return true;
    }
}
