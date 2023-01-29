using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    public static PowerupSpawner Instance;
    public float ExpiryTime; // Time from powerup spawn till self-destruct
    public PowerupData[] Powerups;
    public CollectablePowerup CollectablePowerupPrefab;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void SpawnPowerup(int col, int row, byte type, float expiresAt)
    {
        Transform tf = Grid.Instance.CellGrid[col][row].transform;
        CollectablePowerup powerup = Instantiate(CollectablePowerupPrefab, tf.position, tf.rotation);
        powerup.transform.parent = transform;
        powerup.Column = col;
        powerup.Row = row;
        powerup.Data = Powerups[type];
        powerup.ExpiresAt = expiresAt;
    }

    public void SpawnPowerup(int col, int row, byte type)
    {
        SpawnPowerup(col, row, type, GameController.Instance.TimeLeft - ExpiryTime);
    }

    /// <summary>
    /// Chooses random powerup from an array using weights provided by the ComparativeFrequency field
    /// </summary>
    /// <returns>Index of chosen powerup</returns>
    public static byte ChooseRandomPowerup(PowerupData[] powerups)
    {
        int runningTotal = 0;
        foreach (PowerupData powerup in powerups)
        {
            runningTotal += powerup.ComparativeFrequency;
        }

        int choice = Random.Range(0, runningTotal);
        runningTotal = 0;
        for (int i = 0; i < powerups.Length; i++)
        {
            runningTotal += powerups[i].ComparativeFrequency;
            if (runningTotal > choice) return (byte) i;
        }
        throw new System.Exception("No powerup could be chosen");
    }
}
