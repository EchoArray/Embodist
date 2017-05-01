using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour
{
    #region Values
    /// <summary>
    /// Defines the gamer id of the casting player.
    /// </summary>
    [SyncVar]
    public int GamerId;
    /// <summary>
    /// Determines if the projectile was fired by the host.
    /// </summary>
    [SyncVar]
    public bool Host = false;

    /// <summary>
    /// Defines the duration in-which the projectile is allowed to live.
    /// </summary>
    public float Lifespan;

    public enum VelocityType
    {
        Initial,
        Constant
    }
    /// <summary>
    /// Determines when velocity is applied to the projectile.
    /// </summary>
    [Space(15)]
    public VelocityType VelocityApplication;
    /// <summary>
    /// Defines the velocity of the projectile.
    /// </summary>
    public float Velocity;
    /// <summary>
    /// Determines if the projectile uses gravity.
    /// </summary>
    public bool UseGravity;
    // Defines the rigid body component of the game object.
    private Rigidbody _rigidBody;

    /// <summary>
    /// Defines half of the length of the projectiles collision.
    /// </summary>
    [Space(15)]
    public float CollisionLength;
    /// <summary>
    /// Defines the offset of the starting position of the projectiles.
    /// </summary>
    public float CollisionForwardOffset;
    /// <summary>
    /// Defines the radius of the projectiles collision.
    /// </summary>
    public float CollisionRadius;

    /// <summary>
    /// Defines the effect instantiated upon projectile impact.
    /// </summary>
    [Space(15)]
    public GameObject ImpactEffect;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }
    private void Start()
    {
        if (hasAuthority || NetworkSessionManager.IsHost && Host)
            this.gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateVelocity();
        UpdateRotation();
        UpdateCollision();
    }

    private void OnDrawGizmos()
    {
        Vector3 castPosition = this.transform.position + (this.transform.forward * CollisionForwardOffset);
        Debug.DrawLine(castPosition, castPosition + (this.transform.forward * CollisionLength), Color.yellow);
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        this.gameObject.transform.SetParent(Globals.Instance.Containers.Projectiles);

        // Define rigid body and its defaults
        _rigidBody = this.GetComponent<Rigidbody>();
        _rigidBody.useGravity = UseGravity;
        _rigidBody.velocity = this.transform.forward * Velocity;
        _rigidBody.mass = 0;

        Destroy(this.gameObject, Lifespan);
    }
    
    public void Cast(int gamerId, bool host = false)
    {
        GamerId = gamerId;
        Host = host;
    }

    private void UpdateCollision()
    {
        if (CollisionLength <= 0)
            return;

        // Cast a ray and sphere forward to determine if the projectile has hit another object

        RaycastHit raycastHit;
        Vector3 castPosition = this.transform.position + (this.transform.forward * CollisionForwardOffset);

        bool hit = Physics.Raycast(castPosition, this.transform.forward, out raycastHit, CollisionLength, ~Globals.Instance.WeaponDefaults.ProjectileIgnoredLayers);
        if(!hit)
            Physics.SphereCast(castPosition, CollisionRadius, this.transform.forward, out raycastHit, CollisionLength, ~Globals.Instance.WeaponDefaults.ProjectileIgnoredLayers);

        if (hit)
        {
            InanimateObject colliderInanimateObject = raycastHit.collider.gameObject.GetComponent<InanimateObject>();

            // Avoid collision with casting player
            if (colliderInanimateObject != null && colliderInanimateObject.GamerId == GamerId)
                return;

            Impact(raycastHit);
        }
    }
    private void UpdateVelocity()
    {
        // If constant velocity is active, always add velocity
        if (VelocityApplication == VelocityType.Constant)
            _rigidBody.velocity = this.transform.forward * Velocity;
    }
    private void UpdateRotation()
    {
        // Rotate the projectile toward its velocity
        _rigidBody.MoveRotation(Quaternion.LookRotation(_rigidBody.velocity.normalized));
    }
    
    private void Impact(RaycastHit raycastHit)
    {
        if (ImpactEffect != null)
        {
            // Determine the rotation of the effect
            Quaternion rotation = raycastHit.normal != Vector3.zero ? Quaternion.LookRotation(raycastHit.normal) : Quaternion.identity;
            // Create the effect
            GameObject impactEffect = Instantiate(ImpactEffect, raycastHit.point, rotation);
            EffectUtility effectUtility = impactEffect.GetComponent<EffectUtility>();

            // If the effect uses lightmap coloration, attempt to get a color
            if (effectUtility.UseLightmapColoration)
            {
                // Determine if there is a lightmap color on the impacted object, if not let the effect get the color on its own
                bool hitUnlit = false;
                Color lightmapColor = Color.white;
                // If the projectile hit a scenery or inanimate object, get its lightmap color
                switch (raycastHit.collider.gameObject.layer)
                {
                    default:
                        hitUnlit = true;
                        break;
                    case Globals.INANIMATE_OBJECT_LAYER:
                        lightmapColor = raycastHit.collider.gameObject.GetComponent<InanimateObject>().LightmapColor;
                        break;
                    case Globals.SCENERY_LAYER:
                        lightmapColor = raycastHit.collider.gameObject.GetComponent<Scenery>().LightmapColor;
                        break;
                }
                if (hitUnlit)
                    effectUtility.Cast(raycastHit.collider.gameObject, GamerId);
                else
                    effectUtility.Cast(lightmapColor, raycastHit.collider.gameObject, GamerId);
            }
            else
                effectUtility.Cast(raycastHit.collider.gameObject, GamerId);

        }

        Destroy(this.gameObject);
        
        if (NetworkSessionManager.IsClient && hasAuthority)
            NetworkSessionNode.Instance.CmdDestroy(this.gameObject);
    }
    #endregion
}
