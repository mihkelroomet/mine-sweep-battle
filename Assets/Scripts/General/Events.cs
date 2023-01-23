using System;
using UnityEngine;

public static class Events
{
    // Score
    public static event Func<int> OnGetScore;
    public static int GetScore() => OnGetScore?.Invoke() ?? 0;
    public static event Action<int> OnSetScore;
    public static void SetScore(int value) => OnSetScore?.Invoke(value);

    // Powerups
    public static event Func<PowerupData> OnGetPowerupInFirstSlot;
    public static PowerupData GetPowerupInFirstSlot() => OnGetPowerupInFirstSlot?.Invoke() ?? null;
    public static event Action<PowerupData> OnSetPowerupInFirstSlot;
    public static void SetPowerupInFirstSlot(PowerupData value) => OnSetPowerupInFirstSlot?.Invoke(value);
    public static event Func<PowerupData> OnGetPowerupInSecondSlot;
    public static PowerupData GetPowerupInSecondSlot() => OnGetPowerupInSecondSlot?.Invoke() ?? null;
    public static event Action<PowerupData> OnSetPowerupInSecondSlot;
    public static void SetPowerupInSecondSlot(PowerupData value) => OnSetPowerupInSecondSlot?.Invoke(value);

    // Player Customization
    public static event Func<string> OnGetPlayerName;
    public static string GetPlayerName() => OnGetPlayerName?.Invoke() ?? "Player" + UnityEngine.Random.Range(100_000, 999_999);
    public static event Action<string> OnSetPlayerName;
    public static void SetPlayerName(string value) => OnSetPlayerName?.Invoke(value);
    public static event Func<int> OnGetHatColor;
    public static int GetHatColor() => OnGetHatColor?.Invoke() ?? 0;
    public static event Action<int> OnSetHatColor;
    public static void SetHatColor(int value) => OnSetHatColor?.Invoke(value);
    public static event Func<int> OnGetShirtColor;
    public static int GetShirtColor() => OnGetShirtColor?.Invoke() ?? 0;
    public static event Action<int> OnSetShirtColor;
    public static void SetShirtColor(int value) => OnSetShirtColor?.Invoke(value);
    public static event Func<int> OnGetPantsColor;
    public static int GetPantsColor() => OnGetPantsColor?.Invoke() ?? 0;
    public static event Action<int> OnSetPantsColor;
    public static void SetPantsColor(int value) => OnSetPantsColor?.Invoke(value);
    
    // Audio
    public static event Func<int> OnGetMusicVolume;
    public static int GetMusicVolume() => OnGetMusicVolume?.Invoke() ?? 0;
    public static event Action<int> OnSetMusicVolume;
    public static void SetMusicVolume(int value) => OnSetMusicVolume?.Invoke(value);
    public static event Func<int> OnGetSFXVolume;
    public static int GetSFXVolume() => OnGetSFXVolume?.Invoke() ?? 0;
    public static event Action<int> OnSetSFXVolume;
    public static void SetSFXVolume(int value) => OnSetSFXVolume?.Invoke(value);

    // End of round
    public static event Action OnEndRound;
    public static void EndRound() => OnEndRound?.Invoke();
}
