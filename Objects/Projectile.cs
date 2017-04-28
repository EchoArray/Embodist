using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour
{
    // TODO: Add multiple barrel exit points (toaster)

    #region Values
    [SyncVar]
    public int GamerId;

    /// <summary>
    /// Defines the duration in-which the projectile is allowed to live.
    /// </summary>
    public float Lifespan;
    
    [SyncVar]
    public bool Host = false;

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

    /// <summary>
    /// Defines half of the length of the projectiles collision.
    /// </summary>
    [Space(15)]
    public float CollisionLength;
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

    // Defines the rigid body component of the game object.
    private Rigidbody _rigidBody;
    #endregion

    #region Unity Functions
    private void OnDrawGizmos()
    {
        Vector3 castPosition = this.transform.position + (this.transform.forward * CollisionForwardOffset);
        Debug.DrawLine(castPosition, castPosition + (this.transform.forward * CollisionLength), Color.yellow);
    }

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
        if (!this.gameObject.activeSelf)
            return;

        UpdateConstantVelocity();
        UpdateRotation();
        UpdateCollision();
    }
    #endregion

    #region Functions
    // Define defaults
    private void Initialize()
    {
        this.gameObject.transform.SetParent(Globals.Instance.Containers.Projectiles);
        _rigidBody = this.GetComponent<Rigidbody>();

        _rigidBody.useGravity = UseGravity;
        _rigidBody.velocity = this.transform.forward * Velocity;
        _rigidBody.mass = 0;
        Destroy(this.gameObject, Lifespan);
    }
    public void SetDefaults(int gamerId, bool host = false)
    {
        GamerId = gamerId;
        Host = host;
    }

    private void UpdateCollision()
    {
        if (CollisionLength <= 0)
            return;

        RaycastHit raycastHit;
        Vector3 castPosition = this.transform.position + (this.transform.forward * CollisionForwardOffset);

        bool hit = Physics.SphereCast(castPosition, CollisionRadius, this.transform.forward, out raycastHit, CollisionLength, ~Globals.Instance.WeaponDefaults.ProjectileIgnoredLayers);
        if(!hit)
            hit = Physics.Raycast(castPosition, this.transform.forward, out raycastHit, CollisionLength, ~Globals.Instance.WeaponDefaults.ProjectileIgnoredLayers);

        if (hit)
        {
            InanimateObject colliderInanimateObject = raycastHit.collider.gameObject.GetComponent<InanimateObject>();

            // Avoid collisions with casting player
            if (colliderInanimateObject != null && colliderInanimateObject.GamerId == GamerId)
                return;

            Impact(raycastHit);
        }
    }
    private void UpdateConstantVelocity()
    {
        if (VelocityApplication == VelocityType.Constant)
            _rigidBody.velocity = this.transform.forward * Velocity;
    }
    private void UpdateRotation()
    {
        _rigidBody.MoveRotation(Quaternion.LookRotation(_rigidBody.velocity.normalized));
    }
    
    private void Impact(RaycastHit raycastHit)
    {
        if (ImpactEffect != null)
        {
            Quaternion rotation = raycastHit.normal != Vector3.zero ? Quaternion.LookRotation(raycastHit.normal) : Quaternion.identity;

            GameObject impactEffect = Instantiate(ImpactEffect, raycastHit.point, rotation);

            EffectUtility effectUtility = impactEffect.GetComponent<EffectUtility>();

            Color surfaceColor = Color.white;
            bool hitUnlit = false;
            switch (raycastHit.collider.gameObject.layer)
            {
                default:
                    hitUnlit = true;
                    break;
                case Globals.INANIMATE_OBJECT_LAYER:
                    surfaceColor = raycastHit.collider.gameObject.GetComponent<InanimateObject>().LightmapColor;
                    break;
                case Globals.SCENERY_LAYER:
                    surfaceColor = raycastHit.collider.gameObject.GetComponent<Scenery>().LightmapColor;
                    break;
            }
            if(hitUnlit)
                effectUtility.Cast(raycastHit.collider.gameObject, GamerId);
            else
                effectUtility.Cast(surfaceColor, raycastHit.collider.gameObject, GamerId);

        }

        Destroy(this.gameObject);


        if (NetworkSessionManager.IsClient && hasAuthority)
            NetworkSessionNode.Instance.CmdDestroy(this.gameObject);
    }
    #endregion
}
