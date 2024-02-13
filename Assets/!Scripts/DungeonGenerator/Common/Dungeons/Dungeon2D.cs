using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonMaster._2D
{
    [Serializable]
    public class Dungeon2D<TRoom> where TRoom : Node, new()
    {
        public const int DIMENSIONS = 9;
        public const int MAX_SIZE = 81;

        [SerializeField] private TRoom[] m_rooms;
        private int m_roomCount = 0;

        public int MinRooms { get; private set; }
        public int MaxRooms { get; private set; }

        public TRoom StartingRoom
        {
            get { return m_rooms[40]; }
        }

        public TRoom[] ValidRooms
        {
            get
            {
                List<TRoom> rooms = new();
                foreach (var room in m_rooms)
                {
                    if (room != null && room.IsRoom)
                        rooms.Add(room);
                }

                return rooms.ToArray();
            }
        }
        
        public Dungeon2D(DungeonGeneratorData data, TRoom startingRoom)
        {
            m_rooms = new TRoom[MAX_SIZE];
            MaxRooms = data.TargetRoomCount;
            MinRooms = MaxRooms - 3;

            m_roomCount = 0;
            AddRoom(startingRoom);
        }

        public int IndexOf(TRoom room)
        {
            return (room.x * DIMENSIONS) + room.y;
        }
        public bool InRange(TRoom room)
        {
            bool xValid = 0 <= room.x && room.x < DIMENSIONS;
            bool yValid = 0 <= room.y && room.y < DIMENSIONS;
            return xValid && yValid;
        }
        public bool Exists(TRoom room)
        {
            if (InRange(room) == false) return false;
            else if (m_rooms[IndexOf(room)] == null) return false;
            else if (m_rooms[IndexOf(room)].IsRoom == false) return false;
            else return true;
        }

        public bool AddRoom(TRoom room)
        {
            if (m_roomCount + 1 > MAX_SIZE) return false;
            else if (!InRange(room)) return false;
            else if (m_rooms[IndexOf(room)] != null) return false;

            m_rooms[IndexOf(room)] = room;
            m_roomCount++;
            return true;
        }
        private void AddSpecialRooms()
        {
            List<TRoom> deadends = GetDeadends().OrderByDescending(room => Node.Distance(StartingRoom, room)).ToList();
            deadends[0].RoomType = RoomType.Boss;
            deadends.RemoveAt(0);
            
            List<RoomType> remainingTypes = RoomTypeUtils.GetSpecialRoomTypes().ToList();
            foreach (TRoom room in deadends)
            {
                if (remainingTypes.Count <= 0) break;

                int randIndex = UnityEngine.Random.Range(0, remainingTypes.Count);
                room.RoomType = remainingTypes[randIndex];
                remainingTypes.RemoveAt(randIndex);
            }
        }
        private bool AddSecretRoom(int minNeighboursRequired = 3)
        {
            TRoom[] validPositions = GetValidSecretRoomPositions(minNeighboursRequired);
            if (validPositions.Length <= 0)
                return false;
            TRoom selectedRoom = validPositions[UnityEngine.Random.Range(0, validPositions.Length)];
            selectedRoom.GetNeighbourDirections(GetExistingNeighbours(selectedRoom));

            AddRoom(selectedRoom);
            return true;
        }
        public void AssignEntrances()
        {
            foreach (TRoom room in ValidRooms)
            {
                room.GetNeighbourDirections(GetExistingNeighbours(room));
            }
        }

        public bool Finalize()
        {
            AssignEntrances();
            AddSpecialRooms();

            for (int i = 3; i > 0; i--)
            {
                if (AddSecretRoom(i))
                    return true;
            }

            return false;
        }

        public TRoom[] GetDeadends(bool excludeStartingRoom = true)
        {
            List<TRoom> deadends = new();

            foreach (TRoom room in ValidRooms)
            {
                if (excludeStartingRoom && room == StartingRoom)
                    continue;
                else if (GetExistingNeighbours(room).GetValidCount() > 1)
                    continue;
                else
                    deadends.Add(room);
            }

            return deadends.ToArray();
        }

        public TRoom[] GetExistingNeighbours(TRoom room)
        {
            TRoom[] pseudoNeighbours = room.GetNeighbouringPositions() as TRoom[];
            TRoom[] existingNeighbours = new TRoom[4];

            for (int i = 0; i < 4; i++)
            {
                if (Exists(pseudoNeighbours[i]))
                {
                    existingNeighbours[i] = m_rooms[IndexOf(pseudoNeighbours[i])];
                }
            }
            //Debug.Log($"{room}'s neighbour count: {neighbourCount}.");
            return existingNeighbours;
        }
        public TRoom[] GetExistingNeighbours(int x, int y)
        {
            return GetExistingNeighbours(new Node(new Vector2Int(x, y)) as TRoom);
        }

        private TRoom[] GetValidSecretRoomPositions(int minNeighboursRequired)
        {
            List<TRoom> validPositions = new();

            foreach (TRoom existingRoom in ValidRooms)
            {
                TRoom[] pseudoNeighbours = existingRoom.GetNeighbouringPositions() as TRoom[];

                foreach (TRoom pseudoNeighbour in pseudoNeighbours)
                {
                    if (!InRange(pseudoNeighbour)) continue;
                    else if (Exists(pseudoNeighbour)) continue;

                    TRoom[] realNeighbours = GetExistingNeighbours(pseudoNeighbour);
                    
                    if (realNeighbours.GetValidCount() < minNeighboursRequired) continue;
                    else if (realNeighbours.Any(n => n != null && n.RoomType == RoomType.Boss)) continue;

                    TRoom validPosition = new Node(pseudoNeighbour.Position, true) as TRoom;
                    validPosition.RoomType = RoomType.Secret;
                    if (!validPositions.Contains(validPosition))
                        validPositions.Add(validPosition);
                }
            }

            return validPositions.ToArray();
        }

        public override string ToString()
        {
            return
                $"Room total: {ValidRooms.Length} " + '\n' +
                $"Target room count: {MaxRooms}";
        }
    }

    public static class Dungeon2DUtils
    {
        public static int GetValidCount<TRoom>(this TRoom[] rooms) where TRoom : Node
        {
            int validCount = 0;
            foreach (TRoom room in rooms)
            {
                if (room != null && room.IsRoom) validCount++;
            }

            return validCount;
        }
    }
}
