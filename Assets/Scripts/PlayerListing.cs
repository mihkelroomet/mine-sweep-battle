using UnityEngine;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class PlayerListing : MonoBehaviour
{
    public TextMeshProUGUI _text;
    public Image HostIcon;

    public Player Player { get; private set; }

    public void SetPlayerInfo(Player player)
    {
        Player = player;
        _text.text = player.NickName;
    }
}
