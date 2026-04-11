using UnityEngine;

[CreateAssetMenu(fileName = "NewTower", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Vanity")]
    public string towerName;
    [Header("Stats")]
    public int cost = 20;
    public float range = 10f;
    public float damage = 5f;
    public float fireRate = 2f; // shots per second
    public float projectileSpeed = 20f;
    [Header("Function")]
    public bool isAOE = false;
    public bool isSupport = false;
    public bool isDebuff = false;
    [Header("References")]
    public GameObject projectilePrefab;
    public Sprite towerSprite;
    [Header("Debuff Settings")]
    public float speedDebuff = 0.8f;
    public float debuffTime = 0.5f;
}