using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform[] waypoints;

    public Enemy SpawnEnemy(GameObject enemyPrefab)
    {
        GameObject newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        Enemy enemyScript = newEnemy.GetComponent<Enemy>();
        enemyScript.waypoints = waypoints;
        return enemyScript;
    }
}