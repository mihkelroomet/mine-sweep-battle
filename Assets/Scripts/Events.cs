using System;

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

    // End of round
    public static event Action OnEndOfRound;

    public static void EndOfRound() => OnEndOfRound?.Invoke();
}
