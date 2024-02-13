using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RoomType
{
    Basic,
    Boss,
    Treasure,
    Sanctuary,
    SanctuaryOld,
    Secret
}

public static class RoomTypeUtils
{
    public static IEnumerable<RoomType> GetSpecialRoomTypes()
    {
        List<RoomType> roomTypes = new List<RoomType>(Enum.GetValues(typeof(RoomType)).Cast<RoomType>().ToArray());

        roomTypes.Remove(RoomType.Basic);
        roomTypes.Remove(RoomType.Boss);
        roomTypes.Remove(RoomType.Secret);

        return roomTypes;
    }
}
