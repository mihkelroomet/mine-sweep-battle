using UnityEngine;

public enum PowerupType
{
    BombPowerup,
    SpeedPowerup
}

[CreateAssetMenu(menuName = "MineSweepBattle/Powerup")]
public class PowerupData : ScriptableObject
{
    public PowerupType Type;
    public Sprite CollectablePic;
    public Sprite Pic32;
    public Sprite Pic64;
    public int ComparativeFrequency; // This number is compared against those of all other powerups when deciding which powerup to randomly spawn
}
