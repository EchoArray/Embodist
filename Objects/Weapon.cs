using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Weapon : NetworkBehaviour
{
    #region Values
    /// <summary>
    /// Defines the player controlling the weapon
    /// </summary>
    [HideInInspector]
    public int GamerId;

    /// <summary>
    /// Defines the physical effect applied to the caster upon firing.
    /// </summary>
    public PhysicalEffect FirePhysicalEffect;

    /// <summary>
    /// Defines the projectile that the weapon fires
    /// </summary>
    [Space(5)]
    public GameObject Projectile;

    /// <summary>
    /// Defines the spread randomly applied to the projectile upon ejection
    /// </summary>
    [Space(15)]
    public float Spread;

    /// <summary>
    /// Defines the duration in-which it takes for the barrel to recover after a shot
    /// </summary>
    [Space(5)]
    public float ShotRecoveryDuration;
    // Defines the time that the shot will be recovered at
    private float _shotRecoveredTime;

    /// <summary>
    /// Defines the amount of heat added upon ejection.
    /// </summary>
    [Space(5)]
    public float HeatRate = 200f;
    /// <summary>
    /// Defines the rate in-which the heat cools when not firing or overheated.
    /// </summary>
    public float HeatCoolRate;
    /// <summary>
    /// Determines if the weapon is overheated
    /// </summary>
    [HideInInspector]
    [ValueReference("overheated")]
    public bool OverHeated;

    /// <summary>
    /// Defines the current heat of the weapon.
    /// </summary>
    [HideInInspector]
    [ValueReference("heat")]
    public float Heat;
    // Determines if heat cooling will be skipped along the next frame.
    private bool _skipHeatCooling; 
    #endregion

    #region Unity Functions
    public void Update()
    {
        UpdateCooling();
    } 
    #endregion

    #region Functions
    public void SetDefaults(int gamerId)
    {
        GamerId = gamerId;
    }

    private void UpdateCooling()
    {
        if (!_skipHeatCooling)
            Heat = Mathf.Max(Heat - (HeatCoolRate * Time.deltaTime), 0);
        else
            _skipHeatCooling = false;

        if (OverHeated && Heat <= 50f)
        {
            OverHeated = false;
            GameManager.Instance.GetProfileByGamerId(GamerId).LocalPlayer.HeadsUpDisplay.AddEvent("attached_weapon_heat_cooled");
        }
    }

    public void Fire()
    {
        // If the weapon is still recovering from the last shot or the weapon is overheated, abort
        if (Time.time < _shotRecoveredTime || OverHeated)
            return;
        Eject();
    }
    private void Eject()
    {
        if (FirePhysicalEffect != null)
            FirePhysicalEffect.Cast(GamerId);

        // Define shot recovery time
        _shotRecoveredTime = Time.time + ShotRecoveryDuration;

        // Add to heat, stop heat from cooling next frame
        Heat = Mathf.Min(Heat + (HeatRate * Time.deltaTime), 100);
        _skipHeatCooling = true;

        // If the heat is equal to one hundred overheat the weapon
        if (Heat == 100)
        {
            OverHeated = true;
            GameManager.Instance.GetProfileByGamerId(GamerId).LocalPlayer.HeadsUpDisplay.AddEvent("attached_weapon_overheated");
        }

        // Instantiate new projectile, define projectiles caster
        Quaternion rotation = this.transform.rotation * Quaternion.Euler(Random.Range(-Spread, Spread), Random.Range(-Spread, Spread), 0);
        
        GameObject newProjectile = Instantiate(Projectile, this.transform.position, rotation);
        Projectile projectile = newProjectile.GetComponent<Projectile>();
        projectile.Cast(GamerId);

        if (!NetworkSessionManager.IsLocal)
            NetworkSessionNode.Instance.SpawnProjectile(GamerId, Projectile, this.transform.position, rotation, NetworkSessionManager.IsHost);
    }
    #endregion
}
