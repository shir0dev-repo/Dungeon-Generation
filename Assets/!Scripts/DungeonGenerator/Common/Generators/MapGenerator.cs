using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace DungeonMaster._2D
{
    public static class MapGenerator
    {
        public static Dungeon2D<Node> Generate2D(DungeonGeneratorData data)
        {
            // initialize seeded random
            Random random = new Random(data.GetSeed());

            // generate new dungeon
            Dungeon2D<Node> dungeon = Generate2DInitial(data, random, out int roomCount);

            // dungeon generation completed before iterations begin
            if (roomCount >= dungeon.MaxRooms && dungeon.GetDeadends().Length > 3)
            {
                if (dungeon.Finalize())
                {
                    Debug.Log(GetFinalString(0, data.CurrentSeed, dungeon));
                    return dungeon;
                }
            }

            // iterate through dungeon, creating a new dungeon if current iteration does not suffice
            int currentIteration = 0;
            while (currentIteration < 30)
            {
                Queue<Node> deadendQueue = new Queue<Node>(dungeon.GetDeadends(excludeStartingRoom: false));

                // iterate through deadends, enqueueing any new rooms created
                int attempts = 0;
                while (attempts < 300 && roomCount < dungeon.MaxRooms && deadendQueue.TryDequeue(out Node currentNode))
                {
                    Node[] neighbours = currentNode.GetNeighbouringPositions();
                    foreach (Node neighbour in neighbours)
                    {
                        if (dungeon.Exists(neighbour)) continue;
                        else if (neighbour.Validate(dungeon, random.Next(100)))
                        {
                            if (dungeon.AddRoom(neighbour))
                            {
                                deadendQueue.Enqueue(neighbour);
                                roomCount++;
                            }
                        }
                    }
                }

                if (roomCount >= dungeon.MinRooms && dungeon.GetDeadends().Length > 3)
                {
                    if (dungeon.Finalize())
                    {
                        Debug.Log(GetFinalString(currentIteration, data.CurrentSeed, dungeon));
                        
                        return dungeon;
                    }
                }
                else
                {
                    if (!data.UseRandomSeed)
                        Debug.LogWarning($"Seed {data.CurrentSeed} could not generate valid dungeon. Defaulting to random seed generation.");

                    random = new Random(data.GetRandomSeed());
                    dungeon = Generate2DInitial(data, random, out roomCount);
                    currentIteration++;
                }
            }

            throw new StackOverflowException($"Dungeon could not generate after {currentIteration} attempts.");
        }

        private static string GetFinalString(int currentIteration, int seed, Dungeon2D<Node> dungeon)
        {
            return $"Dungeon generated successfully. See console message for more info: " + '\n' +
                            $"Attempts: {currentIteration + 1}" + '\n' +
                            $"Seed: {seed}" + '\n' +
                            $"Dungeon stats:\n{dungeon}";
        }

        private static Dungeon2D<Node> Generate2DInitial(DungeonGeneratorData data, Random random, out int roomCount)
        {
            Node startingRoom = new Node(new(4, 4), true);
            Dungeon2D<Node> dungeon = new Dungeon2D<Node>(data, startingRoom);

            Stack<Node> roomStack = new Stack<Node>();

            roomStack.PushRange(startingRoom.GetNeighbouringPositions());

            roomCount = 1;
            int attempts = 0;
            int maxIterations = 150;
            int nextRand;


            while (attempts < maxIterations && roomCount < dungeon.MaxRooms)
            {
                attempts++;

                if (!roomStack.TryPop(out Node currentNode))
                    break;

                nextRand = random.Next(100);

                if (!currentNode.Validate(dungeon, nextRand))
                    continue;

                if (!dungeon.AddRoom(currentNode))
                    continue;

                roomCount++;
                roomStack.PushRange(currentNode.GetNeighbouringPositions());
            }

            return dungeon;
        }

        private static void PushRange<T>(this Stack<T> stack, T[] items)
        {
            for (int i = items.Length - 1; i >= 0; i--)
            {
                stack.Push(items[i]);
            }
        }
    }
}