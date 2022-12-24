using System;

public static class Events
{
    // Score

    public static event Func<int> OnGetScore;

    public static int GetScore() => OnGetScore?.Invoke() ?? 0;
    public static event Action<int> OnSetScore;

    public static void SetScore(int value) => OnSetScore?.Invoke(value);

    // Powerups

    public static event Func<PowerupData> OnGetFirstPowerupSlot;

    public static PowerupData GetFirstPowerupSlot() => OnGetFirstPowerupSlot?.Invoke() ?? null;
    public static event Action<PowerupData> OnSetFirstPowerupSlot;

    public static void SetFirstPowerupSlot(PowerupData value) => OnSetFirstPowerupSlot?.Invoke(value);

    public static event Func<PowerupData> OnGetSecondPowerupSlot;

    public static PowerupData GetSecondPowerupSlot() => OnGetSecondPowerupSlot?.Invoke() ?? null;
    public static event Action<PowerupData> OnSetSecondPowerupSlot;

    public static void SetSecondPowerupSlot(PowerupData value) => OnSetSecondPowerupSlot?.Invoke(value);

    // End of round
    public static event Action OnEndOfRound;

    public static void EndOfRound() => OnEndOfRound?.Invoke();
}
