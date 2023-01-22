using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class Lobby : MonoBehaviour
{
    // Components
    [SerializeField] private PhotonView _view;

    // Lobby Panel
    [SerializeField] private TMP_Text RoomName;
    [SerializeField] private TMP_Text PlayersText;
    [SerializeField] private GameObject Options;
    [SerializeField] private Button StartButton;
    [SerializeField] private Button LeaveButton;
    [SerializeField] private PlayerListing PlayerListingPrefab;
    [SerializeField] private Transform Content;
    private List<PlayerListing> _playerListings = new List<PlayerListing>();

    // Lobby Inputs
    [SerializeField] private Slider RowsSlider;
    [SerializeField] private TMP_InputField RowsInputField;
    [SerializeField] private Button RowsSliderHandle;
    [SerializeField] private Slider ColumnsSlider;
    [SerializeField] private TMP_InputField ColumnsInputField;
    [SerializeField] private Button ColumnsSliderHandle;
    [SerializeField] private Slider MineFrequencySlider;
    [SerializeField] private TMP_InputField MineFrequencyInputField;
    [SerializeField] private Button MineFrequencySliderHandle;
    [SerializeField] private Slider RoundLengthSlider;
    [SerializeField] private TMP_InputField RoundLengthInputField;
    [SerializeField] private Button RoundLengthSliderHandle;
    private Slider[] _sliders;
    private TMP_InputField[] _inputFields;
    private Button[] _sliderHandles;
}
