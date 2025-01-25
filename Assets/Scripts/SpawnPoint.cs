using JetBrains.Annotations;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    /// <summary>
    /// ID from 0 -> 3 to designate the player that spawns here.
    /// 0: First player on team 1
    /// 1: First player on team 2
    /// 2: Second player on team 1
    /// 3: Second player on team 2
    /// </summary>
    public uint playerNum = 0;
    
    
    public static Transform GetSpawnPointTransformForPlayer(uint player)
    {
        var spawnPoints = GameObject.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.playerNum == player)
            {
                return spawnPoint.transform;
            }
        }

        Debug.LogError("No valid spawns for player " + player);
        return null;
    }
}
