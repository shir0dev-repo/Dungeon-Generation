using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GeneratorUI : MonoBehaviour
{
    const string CRT_SEED = "Current Seed:\n";
    const string MAX_ROOMS = "Max Rooms: ";
    [SerializeField] TMP_Text _currentSeed;
    [SerializeField] TMP_Text _maxRooms;
    [SerializeField] TMP_InputField _seedInputField;
    [SerializeField] Toggle _randomSeedToggle;
    [SerializeField] Slider _roomCountSlider;
    
    public void InitUI(DungeonGeneratorData data)
    {
        SetSeedText(data.CurrentSeed.ToString());
        SetMaxRoomsText(data.TargetRoomCount);
        _randomSeedToggle.isOn = data.UseRandomSeed;
        _roomCountSlider.value = data.TargetRoomCount;
    }

    public void SetSeedText(string seed)
    {
        _currentSeed.text = CRT_SEED + seed;
    }

    public void SetMaxRoomsText(float maxRooms)
    {
        _maxRooms.text = MAX_ROOMS + (int)maxRooms;
    }

    public void UpdateUI(DungeonGeneratorData data)
    {
        SetSeedText(data.CurrentSeed.ToString());
        _seedInputField.SetTextWithoutNotify(data.CurrentSeed.ToString());
    }
}
