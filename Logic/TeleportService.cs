using Exiled.API.Enums;
using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCP153.Logic
{
    internal static class TeleportService
    {
        private static readonly HashSet<RoomType> Corridors = new HashSet<RoomType>
        {
            RoomType.LczStraight, RoomType.LczCurve, RoomType.LczCrossing, RoomType.LczTCross,
            RoomType.HczStraight, RoomType.HczCurve, RoomType.HczCrossing
        };
        private static readonly Vector3[] Samples =
        {
            Vector3.zero, new Vector3(1.5f, 0f, 0f), new Vector3(-1.5f, 0f, 0f),
            new Vector3(0f, 0f, 1.5f), new Vector3(0f, 0f, -1.5f),
            new Vector3(2.5f, 0f, 0f), new Vector3(-2.5f, 0f, 0f),
            new Vector3(0f, 0f, 2.5f), new Vector3(0f, 0f, -2.5f)
        };

        public static bool TryRelocate(Player target, bool heavyOnly = false)
        {
            // After detonation keep combat on the surface; never send victims underground.
            if (target == null || !target.IsAlive || target.Health <= 0f || Warhead.IsDetonated) return false;
            var rooms = Room.List.Where(r => Corridors.Contains(r.Type) &&
                (r.Zone == ZoneType.HeavyContainment ||
                 (!heavyOnly && !Map.IsLczDecontaminated && r.Zone == ZoneType.LightContainment)))
                .OrderBy(_ => Random.value);
            foreach (var room in rooms)
            {
                if (room.Doors.Any() && room.Doors.All(d => d.IsLocked)) continue;
                foreach (var local in Samples)
                {
                    if (!TryFindPosition(room, local, out var destination)) continue;
                    target.Teleport(destination);
                    return Vector3.Distance(target.Position, destination) < 2f;
                }
            }
            // No unvalidated room.Position fallback.
            return false;
        }

        private static bool TryFindPosition(Room room, Vector3 local, out Vector3 position)
        {
            position = default;
            Vector3 origin = room.Transform.TransformPoint(local) + Vector3.up * 2f;
            var hits = Physics.RaycastAll(origin, Vector3.down, 5f, Physics.AllLayers, QueryTriggerInteraction.Ignore)
                .OrderBy(h => h.distance);
            foreach (var hit in hits)
            {
                if (hit.collider == null || hit.collider.GetComponentInParent<ReferenceHub>() != null) continue;
                // The first solid obstacle must be a walkable floor, not the top of a wall/door.
                if (hit.normal.y < 0.95f || Mathf.Abs(hit.point.y - room.Position.y) > 1.5f) return false;
                Vector3 lower = hit.point + Vector3.up * 0.4f;
                Vector3 upper = hit.point + Vector3.up * 1.65f;
                if (Physics.CheckCapsule(lower, upper, 0.32f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                    return false;
                position = hit.point + Vector3.up * 1.1f;
                return true;
            }
            return false;
        }
    }
}
