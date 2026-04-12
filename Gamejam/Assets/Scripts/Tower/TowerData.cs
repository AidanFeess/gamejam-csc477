using UnityEngine;

[CreateAssetMenu(fileName = "NewTower", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Vanity")]
    public string towerName;
    [Header("Stats")]
    public int cost = 20;
    public int sellValue = 10;
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
    [Header("Buff Settings")]
    public float attackSpeedBuff = 1.15f;
    public float attackRangeBuff = 1.15f;
    [Header("Unlock Info")]
    public int unlockWave = 0;
    public bool isLocked = true;
    public Sprite infoSprite;
    [TextArea(3, 6)] public string infoDescription;
    [Header("Mastery")]
    public float masteryPerKill = 1f;
    public float masteryPerShot = 0f;
    public float masteryCap = 100f;

    // what does mastery multiply?
    public float damagePerMastery = 0f;     // e.g., 0.01 = +1% damage per point
    public float rangePerMastery = 0f;
    public float fireRatePerMastery = 0f;
    public float debuffDurationPerMastery = 0f;
}