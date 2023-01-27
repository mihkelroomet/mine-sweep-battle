using UnityEngine;
using TMPro;

public class ScoreRow : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _playerScoreText;

    public void SetPlayerNameText(string text)
    {
        _playerNameText.text = text;
    }

    public void SetPlayerScoreText(string text)
    {
        _playerScoreText.text = text;
    }
}
