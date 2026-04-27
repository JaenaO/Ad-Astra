using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules
{
    [Serializable]
    internal struct ModulePlacementSnapshot
    {
        public string moduleId;
        public Vector2Int gridPosition;
    }

    internal static class ShipBuildSessionState
    {
        public static List<ModulePlacementSnapshot> SavedLayout;
    }
}
