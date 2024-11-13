using UnityEngine;

[CreateAssetMenu(menuName = "Example/Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponName;
    public int price;
    public bool isRanged;
    public string[] ammoTypes;
    public Stats stats;

    [System.Serializable]
    public class Stats
    {
        public float damage;
        public float accuracy;
        public float mobility;
    }
}