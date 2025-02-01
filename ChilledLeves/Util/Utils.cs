using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using ECommons.Automation.NeoTaskManager;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Reflection;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using static ChilledLeves.Util.Data;

namespace ChilledLeves.Util;

public static unsafe class Utils
{
    public static bool PluginInstalled(string name)
    {
        return DalamudReflector.TryGetDalamudPlugin(name, out _, false, true);
    }

    public static unsafe int GetItemCount(int itemID, bool includeHq = true)
        => includeHq ? InventoryManager.Instance()->GetInventoryItemCount((uint)itemID, true) 
        + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000)
        : InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000);

    public static bool ExecuteTeleport(uint aetheryteId) => UIState.Instance()->Telepo.Teleport(aetheryteId, 0);
    internal static unsafe float GetDistanceToPlayer(Vector3 v3) => Vector3.Distance(v3, Player.GameObject->Position);
    internal static unsafe float GetDistanceToPlayer(IGameObject gameObject) => GetDistanceToPlayer(gameObject.Position);
    internal static IGameObject? GetObjectByName(string name) => Svc.Objects.OrderBy(GetDistanceToPlayer).FirstOrDefault(o => o.Name.TextValue.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    public static float GetDistanceToPoint(float x, float y, float z) => Vector3.Distance(Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero, new Vector3(x, y, z));
    public static float GetDistanceToPointV(Vector3 targetPoint) => Vector3.Distance(Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero, targetPoint);
    private static readonly unsafe nint PronounModule = (nint)Framework.Instance()->GetUIModule()->GetPronounModule();
    #pragma warning disable IDE1006 // Naming Styles
    private static readonly unsafe delegate* unmanaged<nint, uint, GameObject*> getGameObjectFromPronounID = (delegate* unmanaged<nint, uint, GameObject*>)Svc.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 0F 85 ?? ?? ?? ?? 8D 4F DD");
    #pragma warning restore IDE1006 // Naming Styles
    public static unsafe GameObject* GetGameObjectFromPronounID(uint id) => getGameObjectFromPronounID(PronounModule, id);
    public static bool IsBetweenAreas => (Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51]);
    internal static bool GenericThrottle => FrameThrottler.Throttle("AutoRetainerGenericThrottle", 10);
    public static TaskManagerConfiguration DConfig => new(timeLimitMS: 10 * 60 * 3000, abortOnTimeout: false);
    public static bool HasPlugin(string name) => DalamudReflector.TryGetDalamudPlugin(name, out _, false, true);
    public static bool IsInZone(uint zoneID) => Svc.ClientState.TerritoryType == zoneID;

    public static void PluginLog(string message) => ECommons.Logging.PluginLog.Information(message);

    public static bool PlayerNotBusy()
    {
        return Player.Available
               && Player.Object.CastActionId == 0
               && !IsOccupied()
               && !Svc.Condition[ConditionFlag.Jumping]
               && Player.Object.IsTargetable;
    }

    public static (ulong id, Vector3 pos) FindAetheryte(uint id)
    {
        foreach (var obj in GameObjectManager.Instance()->Objects.IndexSorted)
            if (obj.Value != null && obj.Value->ObjectKind == ObjectKind.Aetheryte && obj.Value->BaseId == id)
                return (obj.Value->GetGameObjectId(), *obj.Value->GetPosition());
        return (0, default);
    }

    public static GameObject* LPlayer() => GameObjectManager.Instance()->Objects.IndexSorted[0].Value;

    public static Vector3 PlayerPosition()
    {
        var player = LPlayer();
        return player != null ? player->Position : default;
    }

    public static uint CurrentTerritory() => GameMain.Instance()->CurrentTerritoryTypeId;

    public static bool IsAddonActive(string AddonName) // Used to see if the addon is active/ready to be fired on
    {
        var addon = RaptureAtkUnitManager.Instance()->GetAddonByName(AddonName);
        return addon != null && addon->IsVisible && addon->IsReady;
    }

    public static float GetPlayerRawXPos(string character = "")
    {
        if (!character.IsNullOrEmpty())
        {
            unsafe
            {
                if (int.TryParse(character, out var p))
                {
                    var go = Utils.GetGameObjectFromPronounID((uint)(p + 42));
                    return go != null ? go->Position.X : -1;
                }
                else return Svc.Objects.Where(x => x.IsTargetable).FirstOrDefault(x => x.Name.ToString().Equals(character))?.Position.X ?? -1;
            }
        }
        return Svc.ClientState.LocalPlayer!.Position.X;
    }
    public static float GetPlayerRawYPos(string character = "")
    {
        if (!character.IsNullOrEmpty())
        {
            unsafe
            {
                if (int.TryParse(character, out var p))
                {
                    var go = Utils.GetGameObjectFromPronounID((uint)(p + 42));
                    return go != null ? go->Position.Y : -1;
                }
                else return Svc.Objects.Where(x => x.IsTargetable).FirstOrDefault(x => x.Name.ToString().Equals(character))?.Position.Y ?? -1;
            }
        }
        return Svc.ClientState.LocalPlayer!.Position.Y;
    }
    public static float GetPlayerRawZPos(string character = "")
    {
        if (!character.IsNullOrEmpty())
        {
            unsafe
            {
                if (int.TryParse(character, out var p))
                {
                    var go = Utils.GetGameObjectFromPronounID((uint)(p + 42));
                    return go != null ? go->Position.Z : -1;
                }
                else return Svc.Objects.Where(x => x.IsTargetable).FirstOrDefault(x => x.Name.ToString().Equals(character))?.Position.Z ?? -1;
            }
        }
        return Svc.ClientState.LocalPlayer!.Position.Z;
    }

    #region Node Visibility | Text

    // stuff to get the Node visibility. Moreso to test and see if they have an item unlocked.
    // Thank you Croizat for dealing with me asking dumb questions. I do appreciate it. / having a way this worked in lua

    public static unsafe bool IsNodeVisible(string addonName, params int[] ids)
    {
        var ptr = Svc.GameGui.GetAddonByName(addonName, 1);
        if (ptr == nint.Zero)
            return false;

        var addon = (AtkUnitBase*)ptr;
        var node = GetNodeByIDChain(addon->GetRootNode(), ids);
        return node != null && node->IsVisible();
    }
    private static unsafe AtkResNode* GetNodeByIDChain(AtkResNode* node, params int[] ids)
    {
        if (node == null || ids.Length <= 0)
            return null;

        if (node->NodeId == ids[0])
        {
            if (ids.Length == 1)
                return node;

            var newList = new List<int>(ids);
            newList.RemoveAt(0);

            var childNode = node->ChildNode;
            if (childNode != null)
                return GetNodeByIDChain(childNode, [.. newList]);

            if ((int)node->Type >= 1000)
            {
                var componentNode = node->GetAsAtkComponentNode();
                var component = componentNode->Component;
                var uldManager = component->UldManager;
                childNode = uldManager.NodeList[0];
                return childNode == null ? null : GetNodeByIDChain(childNode, [.. newList]);
            }

            return null;
        }

        //check siblings
        var sibNode = node->PrevSiblingNode;
        return sibNode != null ? GetNodeByIDChain(sibNode, ids) : null;
    }

    #endregion

    public static unsafe void LeveJobIcons(uint JobType, Vector2 size = default(Vector2))
    {
        if (size == default(Vector2))
        {
            size = new Vector2(20, 20);
        }

        if (GetRow<LeveAssignmentType>(JobType).Value.Icon is { } icon)
        {
            if (Svc.Texture.TryGetFromGameIcon(icon, out var texture))
                ImGui.Image(texture.GetWrapOrEmpty().ImGuiHandle, size);
            else
                ImGui.Dummy(size);
        }
    }

    public static unsafe void ItemIconLookup(uint itemID)
    {
        if (GetRow<Item>(itemID).Value.Icon is { } icon)
        {
            int icon2 = icon;
            if (Svc.Texture.TryGetFromGameIcon(icon2, out var texture2))
                ImGui.Image(texture2.GetWrapOrEmpty().ImGuiHandle, new(20, 20));
            else
                ImGui.Dummy(new(20, 20));
        }
    }

    public static unsafe void AllLeves(Vector2 size = default(Vector2))
    {
        if (size == default(Vector2))
        {
            size = new Vector2(20, 20);
        }

        if (GetRow<EventIconPriority>(10).Value.Icon[6] is { } icon)
        {
            int icon2 = (int)icon;
            if (Svc.Texture.TryGetFromGameIcon(icon2, out var texture2))
                ImGui.Image(texture2.GetWrapOrEmpty().ImGuiHandle, size);
            else
                ImGui.Dummy(size);
        }
    }

    public static bool IsRowEnabled(uint classId, bool AllEnabled)
    {
        if (AllEnabled)
            return true;
        else
        {
            int spot = 0;

            for (int i = 0; i < CraftingClass.Count; i++)
            {
                if (CraftingClass[i] == classId)
                {
                    spot = i;
                    break;
                }
            }

            return CraftingClassActive[spot];
        }
    }

    public static bool IsLocationEnabled(string location, bool allEnabled)
    {
        if (allEnabled)
            return true;
        else
        {
            int boolLoc = 0;

            for (int i = 0; i < AllLocations.Count; i++)
            {
                if (AllLocations[i] == location)
                {
                    boolLoc = i;
                    break;
                }
            }

            return LocationsActive[boolLoc];
        }
    }

    public static void PopulateDictionary()
    {
        var sheet = Svc.Data.GetExcelSheet<Leve>();

        if (sheet != null)
        {
            // Checking the Leve sheet (grabbed above) and just making it a lot more shortform
            foreach (var row in sheet)
            {
                // Tells us what the leve number is. This is currently the row that it's *-also-* tied to. 
                // Can't wait for square to bite me in the ass for this later
                uint leveNumber = row.RowId;

                // Checking to see if the starting city is even valid. This will filter out the blank ones from the sheet and let me not get garbo
                uint town = sheet.GetRow(leveNumber).Town.RowId;
                if (town != 0)
                {
                    // Checking the Jobtype, this is a very small number 
                    uint leveJob = sheet.GetRow(leveNumber).LeveAssignmentType.Value.RowId;

                    // Name of the leve that you're grabbing
                    string leveName = sheet.GetRow(leveNumber).Name.ToString();

                    // Amount to run, this is always 0 upon initializing
                    // Mainly there to be an input for users later
                    int amount = 0;

                    // The questID of the leve. Need this for another sheet but also, might be useful to check progress of other quest...
                    uint questID = sheet.GetRow(leveNumber).DataId.RowId;

                    // Item that is required for the leve to be turned in
                    // Uses the questID to look into another sheet to find which item you need to have for turnin
                    uint itemId = Svc.Data.GetExcelSheet<CraftLeve>().GetRow(questID).Item[0].RowId;

                    // Item name itself, that way people aren't just going "huh???"
                    string itemName = Svc.Data.GetExcelSheet<Item>().GetRow(itemId).Name.ToString();

                    // Amount of times that this quest can be turned in
                    // Not useful post Shadowbringers, but there's a lot of multi-turnin quest earlier on that's good for EXP
                    // Rip coffee biscuits, you'll be missed
                    int repeatAmount = Svc.Data.GetExcelSheet<CraftLeve>().GetRow(questID).Repeats.ToInt();

                    // Defaulting the necessary amount that you need to 0 here
                    // Moreso safety precaution than anything
                    int necessaryAmount = 0;

                    // Starting location that the leve initially starts in 
                    // Location location location... always important
                    // These are the *-actual-* Zone ID's of the places so, great for teleporting
                    uint startingCity = sheet.GetRow(leveNumber).PlaceNameStartZone.Value.RowId;

                    // Zone name itself. That way people know exactly where this leve is coming from
                    string ZoneName = Svc.Data.GetExcelSheet<PlaceName>().GetRow(startingCity).Name.ToString();

                }

            }
        }

        for (uint i = 0; i < 1808; i++)
        {
            uint leveID = i;
            uint itemID = 0;

            var leveJobType = Svc.Data.GetExcelSheet<Leve>().GetRow(leveID).LeveAssignmentType.Value.RowId;
            string leveName = Svc.Data.GetExcelSheet<Leve>().GetRow(leveID).Name.ToString();
            int amount = 0;
            uint questID = Svc.Data.GetExcelSheet<Leve>().GetRow(leveID).DataId.RowId;
            if (questID != 0)
            {
                itemID = Svc.Data.GetExcelSheet<CraftLeve>().GetRow(questID).Item[0].RowId;
                string currentItemCount = GetItemCount((int)itemID).ToString();
            }
            int repeatAmount = Svc.Data.GetExcelSheet<CraftLeve>().GetRow(questID).Repeats.ToInt();
            int necessaryAmount = 0;
            var startingCity = Svc.Data.GetExcelSheet<Leve>().GetRow(leveID).Town.RowId;
            string zoneName = Svc.Data.GetExcelSheet<Town>().GetRow(startingCity).Name.ToString();
            for (int x = 0; x < 3; x++)
            {
                if (itemID != 0)
                {
                    necessaryAmount = necessaryAmount + Svc.Data.GetExcelSheet<CraftLeve>().GetRow(questID).ItemCount[x].ToInt();
                }
            }
            if (repeatAmount != 0)
                necessaryAmount = necessaryAmount * (repeatAmount + 1);

            // Ensure the leveJobType is valid before inserting
            if (!LeveDict.ContainsKey(leveID) && leveJobType != 0)
            {
                LeveDict[leveID] = new LeveDataDict
                {
                    JobID = leveJobType,
                    LeveName = leveName,
                    Amount = amount,
                    QuestID = questID,
                    ItemID = itemID,
                    RepeatAmount = repeatAmount,
                    StartingCity = startingCity,
                    ZoneName = zoneName
                };
            }
        }
    }
}
