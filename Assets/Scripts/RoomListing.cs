using System.Collections;
using System.Collections.Generic;
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
        _text.text = roomInfo.Name + ", " + "Players: " + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
    }

    public RoomInfo GetRoomInfo()
    {
        return RoomInfo;
    }
}
