using System;

public static class Events
{
    public static event Action<int> OnSetScore;

    public static void SetScore(int value) => OnSetScore?.Invoke(value);

    public static event Func<int> OnGetScore;

    public static int GetScore() => OnGetScore?.Invoke() ?? 0;

    public static event Action OnEndOfRound;

    public static void EndOfRound() => OnEndOfRound?.Invoke();
}
