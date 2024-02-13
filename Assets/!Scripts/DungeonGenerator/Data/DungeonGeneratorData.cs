using UnityEngine;

[CreateAssetMenu(fileName = "New Dungeon", menuName = "DungeonMaster/New Dungeon")]
public class DungeonGeneratorData : ScriptableObject
{
    public bool UseRandomSeed = false;
    public int CurrentSeed;

    [Space]
    [Range(12, 36)] public int TargetRoomCount = 32;

    public int GetSeed()
    {
        if (UseRandomSeed)
            CurrentSeed = Random.Range(-10000, 10000);

        return CurrentSeed;
    }

    public int GetRandomSeed()
    {
        CurrentSeed = Random.Range(-10000, 10000);
        return CurrentSeed;
    }
}