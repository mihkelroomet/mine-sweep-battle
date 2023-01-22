using UnityEngine;
using Photon.Realtime;
using TMPro;

public class RoomListing : MonoBehaviour
{
    public TextMeshProUGUI _text;
    public RoomInfo RoomInfo { get; private set; }
    public void SetRoomInfo(RoomInfo roomInfo)
    {
        RoomInfo = roomInfo;
        _text.text = roomInfo.Name +
            " | " + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers +
            " | " + roomInfo.CustomProperties["Rows"] + "x" + roomInfo.CustomProperties["Columns"] +
            " | Mine Freq: " + Mathf.Round((float) roomInfo.CustomProperties["MineFrequency"] * 10) +
            " | Round Len: " + roomInfo.CustomProperties["RoundLength"] + "s";
    }

    public RoomInfo GetRoomInfo()
    {
        return RoomInfo;
    }
}
