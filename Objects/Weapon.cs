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
    public InanimateObject Owner;
    /// <summary>
    /// Defines the projectile that the weapon fires
    /// </summary>
    public GameObject Projectile;

    /// <summary>
    /// Defines the duration in-which it takes for the barrel to recover after a shot
    /// </summary>
    public float ShotRecoveryDuration;
    // Defines the time that the shot will be recovered at
    private float _shotRecoveredTime;
    /// <summary>
    /// Defines the spread randomly applied to the projectile upon ejection
    /// </summary>
    public float Spread;

    public GameObject FirePhysicalEffect;
    private PhysicalEffect _firePhysicalEffect;

    /// <summary>
    /// Defines the amount of heat added upon ejection.
    /// </summary>
    public float HeatAddition;
    /// <summary>
    /// Defines the rate in-which the heat cools when not firing or overheated.
    /// </summary>
    public float HeatCoolRate;
    /// <summary>
    /// Determines if the weapon is overheated
    /// </summary>
    [ValueReference("overheated")]
    public bool OverHeated;

    /// <summary>
    /// Defines the current heat of the weapon.
    /// </summary>
    [ValueReference("heat")]
    public float Heat;
    // Determines if heat cooling will be skipped along the next frame.
    private bool _skipHeatCooling; 
    #endregion

    #region Unity Functions
    public void Awake()
    {
        Initialize();
    }
    public void Update()
    {
        UpdateHeatCooling();
    } 
    #endregion

    #region Functions
    private void Initialize()
    {
        if (FirePhysicalEffect != null)
            _firePhysicalEffect = Instantiate(FirePhysicalEffect, this.transform.position, this.transform.rotation, this.transform).GetComponent<PhysicalEffect>(); 
    }

    public void IdentifyOwner(int gamerId, InanimateObject owner)
    {
        GamerId = gamerId;
        Owner = owner;
    }

    private void UpdateHeatCooling()
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
        // Define shot recovery time
        _shotRecoveredTime = Time.time + ShotRecoveryDuration;

        // Add to heat, stop heat from cooling next frame
        Heat = Mathf.Min(Heat + (HeatAddition * Time.deltaTime), 100);
        _skipHeatCooling = true;

        // If the heat is equal to one hundred overheat the weapon
        if (Heat == 100)
        {
            OverHeated = true;
            GameManager.Instance.GetProfileByGamerId(GamerId).LocalPlayer.HeadsUpDisplay.AddEvent("attached_weapon_overheated");
        }

        // Instantiate new projectile, define projectiles caster
        Quaternion direction = this.transform.rotation * Quaternion.Euler(Random.Range(-Spread, Spread), Random.Range(-Spread, Spread), 0);


        GameObject newProjectile = Instantiate(Projectile, this.transform.position, direction);
        Projectile projectile = newProjectile.GetComponent<Projectile>();
        projectile.SetDefaults(GamerId);

        if (!NetworkSessionManager.IsLocal)
            NetworkSessionNode.Instance.SpawnProjectile(GamerId, Projectile, this.transform.position, direction, NetworkSessionManager.IsHost);


        if (_firePhysicalEffect != null)
            _firePhysicalEffect.Cast(GamerId);
    }
    #endregion
}
