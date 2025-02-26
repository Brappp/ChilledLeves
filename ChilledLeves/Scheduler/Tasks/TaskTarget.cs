﻿using Dalamud.Game.ClientState.Objects.Types;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskTarget
    {
        public static void Enqueue(ulong objectID)
        {
            Svc.Log.Debug($"Targeting {objectID}");
            IGameObject? gameObject = null;
            P.taskManager.Enqueue(() => TryGetObjectByDataId(objectID, out gameObject), "Getting Object");
            P.taskManager.Enqueue(() => PluginLog($"Targeting By ID. Target is: {gameObject?.DataId}"));
            P.taskManager.Enqueue(() => TargetgameObject(gameObject), "Targeting Object");
        }
    }
}
