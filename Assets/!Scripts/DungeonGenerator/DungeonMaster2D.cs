using DungeonMaster2D;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DungeonHolder : MonoBehaviour
{
    [SerializeField] private DungeonGeneratorData _generatorData;
    [SerializeField] private GeneratorUI _generatorUI;
    [SerializeField] private GameObject _roomPlaceholder;
    [SerializeField] private GameObject _hallwayPlaceholder;
    public Dungeon2D dungeonMap;

    private void Awake()
    {
        _generatorUI.InitUI(_generatorData);
    }

    public void GenerateMap()
    {
        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            if (child == transform) continue;
            Destroy(child.gameObject);
        }

        dungeonMap = MapGenerator.Generate2D(_generatorData);

        Dictionary<Vector2Int, GameObject> blocks = new();

        foreach (Node room in dungeonMap.ValidNodes)
        {
            if (room != null && !(blocks.ContainsKey(room.Position)))
            {
                GameObject go = Instantiate(_roomPlaceholder, room, Quaternion.identity);
                go.transform.SetParent(transform);
                blocks.Add(room.Position, go);
                go.transform.GetChild(0).GetComponent<SpriteRenderer>().color = room.NodeType switch
                {
                    NodeType.Basic => Color.white,
                    NodeType.Boss => Color.red,
                    NodeType.Sanctuary => Color.cyan,
                    NodeType.SanctuaryOld => Color.grey,
                    NodeType.Treasure => Color.yellow,
                    NodeType.Secret => Color.magenta,
                    _ => Color.white
                };


                foreach (var entrance in room.Entrances.GetFlags())
                {
                    Vector2 dir2 = ((Direction)entrance).GetVector();
                    dir2.Normalize();
                    dir2 *= 0.5f;
                    Vector3 dir = new(dir2.x, dir2.y, -1);
                    GameObject go1 = Instantiate(_hallwayPlaceholder, go.transform.position, Quaternion.identity);
                    go1.transform.SetParent(go.transform);
                    go1.transform.position += (Vector3)dir;
                }
            }
        }

        blocks[new(4, 4)].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.green;

        _generatorUI.UpdateUI(_generatorData);
    }

    public void SetMaxRooms(float max)
    {
        _generatorData.TargetRoomCount = (int)max;
    }

    public void SetRandomSeed(bool toggle)
    {
        _generatorData.UseRandomSeed = toggle;
    }

    public void SetCustomSeed(string seed)
    {
        if (int.TryParse(seed, out int newSeed))
            _generatorData.CurrentSeed = Mathf.Clamp(newSeed, -10000, 10000);
    }
}
