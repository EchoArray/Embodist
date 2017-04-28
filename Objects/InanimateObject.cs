using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class InanimateObject :  NetworkBehaviour
{
    #region Values
    /// <summary>
    /// Defines the controlling player of the inanimate object.
    /// </summary>
    [HideInInspector]
    public LocalPlayer LocalPlayer;

    /// <summary>
    /// Determines if the inanimate object is being controlled.
    /// </summary>
    [HideInInspector]
    [SyncVar]
    public bool Controlled;
    /// <summary>
    /// Defines the unique id associated with the controlling player of the inanimate object.
    /// </summary>
    [HideInInspector]
    public int GamerId;
    /// <summary>
    /// Defines which team the inanimate object is a part of.
    /// </summary>
    //[HideInInspector]
    public int TeamId;

    /// <summary>
    /// Defines the color of the object when highlighted.
    /// </summary>
    public Color SelectionColor;

    /// <summary>
    /// Defines the name of the inanimate objects.
    /// </summary>
    public string Name;


    /// <summary>
    /// Defines the offset per axis in-which the waypoint will rest from the inanimate objects position.
    /// </summary>
    public Vector3 WaypointOffset;
    public enum Classification
    {
        Throwy,
        Squirty,
        Smashy,
        Bomb,
        None
    }
    /// <summary>
    /// Determines the classification of the inanimate object.
    /// </summary>
    public Classification Class;


    [Serializable]
    public class TargetingBox
    {
        public Vector3 Center;
        public Vector3 Size;
        public Vector3 Rotation;
    }
    [Space(15)]
    public TargetingBox[] TargetingBoxes;


    /// <summary>
    /// Defines the distances per axis in-which the camera will rest offset from the inanimate object.
    /// </summary>
    [Space(15)]
    public Vector3 CameraFollowOffset;
    // Defines the default camera follow offset of the inanimate object.
    private Vector3 _defaultCameraFollowOffset;

    /// <summary>
    /// Defines the current health of the inanimate object.
    /// </summary>
    [ValueReference("health")]
    [SyncVar]
    [HideInInspector]
    public float Health;
    /// <summary>
    /// Defines the starting, and maximum health of the inanimate object.
    /// </summary>
    [ValueReference("max_health")]
    public float MaxHealth;

    /// <summary>
    /// Defines the duration in which health regeneration is delayed after the heavy inanimate object has received damage.
    /// </summary>
    public float HealthRegenerationDelay = 3.243f;
    /// <summary>
    /// Defines the rate in-which the health of the inanimate object regenerates.
    /// </summary>
    public float HealthRegenerationRate = 24f;

    // Defines the time in-which health will begin to regenerate.
    private float _healthRegenerationStartTime;
    // Defines the unique id of the last player to do damage to the inanimate object.
    private int _lastDamageCaster;
    // Defines the time in-which the last damage caster will be cleared.
    private float _lastDamageCasterClearTime;
    // Determines if the inanimate object had died.
    private bool _dead;

    /// <summary>
    /// Defines the effect instantiated upon the death of the inanimate object.
    /// </summary>
    public GameObject DeathEffect;

    [Serializable]
    public class MovementSettings
    {


        /// <summary>
        /// Determines if the inanimate object is grounded or not.
        /// </summary>
        [HideInInspector]
        public bool Grounded = true;
        /// <summary>
        /// Defines the time in-which the jump window will be closed after becoming ungrounded.
        /// </summary>
        [HideInInspector]
        public float UngroundedJumpWindowExpiration;
        /// <summary>
        /// Defines the velocity applied to the inanimate objects rigid body when jumping.
        /// </summary>
        public float JumpVelocity;
        /// <summary>
        /// Defines the time that a previous jump attempt will become expired.
        /// </summary>
        [HideInInspector]
        public float JumpAttemptExpiration;
        /// <summary>
        /// Determines if the inanimate object has jumped and has yet to be grounded.
        /// </summary>
        [HideInInspector]
        public bool Jumped = false;
        /// <summary>
        /// Defines fixed update time for the most recent time that grounded was set to true.
        /// </summary>
        [HideInInspector]
        public float LastStayGroundSetTime;
        [HideInInspector]
        public float JumpedTime;

        /// <summary>
        ///  Defines the speed in-which the inanimate object moves while grounded.
        /// </summary>
        [Space(10)]
        public float GroundedMovementSpeed;
        /// <summary>
        ///  Defines the speed in-which the inanimate object moves while airborne.
        /// </summary>
        public float AirborneMovementSpeed;

        /// <summary>
        /// Defines the scale of the inanimate objects movement angular velocity when grounded.
        /// </summary>
        public float GroundedAngularVelocityScale;
        /// <summary>
        /// Defines the scale of the inanimate objects movement angular velocity when airborne.
        /// </summary>
        public float AirborneAngularVelocityScale;


        /// <summary>
        /// Determines if this inanimate object is lunging.
        /// </summary>
        [HideInInspector]
        public bool Lunging = false;

        /// <summary>
        /// Determines the time in-which the inanimate object is next allowed to lunge.
        /// </summary>
        [HideInInspector]
        public float NextLungeTime;
        /// <summary>
        /// Determines if a lunge notification has been made during the lunge time out.
        /// </summary>
        [HideInInspector]
        public bool LungeNotified;
        /// <summary>
        /// Determines if the camera will correct its rotation during the camera repositioning while lunging.
        /// </summary>
        [HideInInspector]
        public bool AllowLungeRotationCorrection;
        [HideInInspector]
        public Vector3 LungeLookingPosition;

        public MovementSettings(MovementSettings movementSettings)
        {
            Grounded = movementSettings.Grounded;
            UngroundedJumpWindowExpiration = movementSettings.UngroundedJumpWindowExpiration;
            JumpVelocity = movementSettings.JumpVelocity;
            JumpAttemptExpiration = movementSettings.JumpAttemptExpiration;
            Jumped = movementSettings.Jumped;
            GroundedMovementSpeed = movementSettings.GroundedMovementSpeed;
            AirborneMovementSpeed = movementSettings.AirborneMovementSpeed;
            GroundedAngularVelocityScale = movementSettings.GroundedAngularVelocityScale;
            AirborneAngularVelocityScale = movementSettings.AirborneAngularVelocityScale;
            Lunging = movementSettings.Lunging;
            NextLungeTime = movementSettings.NextLungeTime;
            LungeNotified = movementSettings.LungeNotified;
        }
    }
    public MovementSettings Movement;

    /// <summary>
    /// Defines the weapon of the inanimate object.
    /// </summary>
    [Space(15)]
    [ValueReference("weapon")]
    public Weapon Weapon;
    // Determines if the inanimate object is currently aiming or not.
    private bool _aiming;
    
    public enum TargetingType
    {
        None,
        Friendly,
        Enemy
    }
    /// <summary>
    /// Determines the current targeting state of the inanimate object.
    /// </summary>
    [HideInInspector]
    [ValueReference("targeting_state")]
    public TargetingType TargetingState;


    public Color LightmapColor;

    [HideInInspector]
    public NetworkTransform NetworkTransform;

    // Defines the instantiated material of the renderer.
    private Material _material;
    // Defines the rigid body component of the game object.
    private Rigidbody _rigidbody;
    // Defines the renderer component of the game object.
    private Renderer _renderer;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        if (Class == Classification.Bomb)
            HUDManager.Instance.AddWaypointToTeamsHud(TeamId, HeadsUpDisplay.Waypoint.WaypointType.ObjectiveBomb, this.transform, WaypointOffset);
    }

    private void Update()
    {
        UpdateMaterialLighting(true);
        if (Controlled && (hasAuthority || NetworkSessionManager.IsLocal))
        {
            UpdateAiming();
            UpdateLunge();
            UpdateLastDamageCaster();
            UpdateHealthRegeneration();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckEnterCollision(collision);

    }
    private void OnCollisionStay(Collision collision)
    {
        CheckStayCollision(collision.contacts);
    }
    private void OnCollisionExit(Collision collision)
    {
        if (!Controlled)
            return;
        SetGrounding(false);
    }

    private void OnDrawGizmos()
    {
        Matrix4x4 matrixBackup = Gizmos.matrix;


        foreach (TargetingBox targetingBox in TargetingBoxes)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation * Quaternion.Euler(targetingBox.Rotation), transform.lossyScale);

            Color gizmoColor = Color.green;
            gizmoColor.a = 0.5f;
            Gizmos.color = gizmoColor;

            Gizmos.DrawCube(targetingBox.Center, targetingBox.Size);

            Vector3 halfExtends = targetingBox.Size / 2;
            Vector3 topLeft = targetingBox.Center + new Vector3(halfExtends.x, halfExtends.y, halfExtends.z);
            Vector3 topRight = targetingBox.Center + new Vector3(-halfExtends.x, halfExtends.y, halfExtends.z);
            Vector3 bottomLeft = targetingBox.Center + new Vector3(halfExtends.x, -halfExtends.y, halfExtends.z);
            Vector3 bottomRight = targetingBox.Center + new Vector3(-halfExtends.x, -halfExtends.y, halfExtends.z);

            float pointScale = 0.05f;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(topLeft, Vector3.one * pointScale);
            Gizmos.DrawWireCube(topRight, Vector3.one * pointScale);
            Gizmos.DrawWireCube(bottomLeft, Vector3.one * pointScale);
            Gizmos.DrawWireCube(bottomRight, Vector3.one * pointScale);

            Gizmos.DrawWireCube(topLeft - (Vector3.forward * targetingBox.Size.z), Vector3.one * pointScale);
            Gizmos.DrawWireCube(topRight - (Vector3.forward * targetingBox.Size.z), Vector3.one * pointScale);
            Gizmos.DrawWireCube(bottomLeft - (Vector3.forward * targetingBox.Size.z), Vector3.one * pointScale);
            Gizmos.DrawWireCube(bottomRight - (Vector3.forward * targetingBox.Size.z), Vector3.one * pointScale);


        }


        Gizmos.matrix = matrixBackup;
    }

    private void OnDestroy()
    {
        Destroy(_material);
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        // Defines various defaults within the inanimate object.
        this.transform.SetParent(Globals.Instance.Containers.Objects);

        _renderer = this.gameObject.GetComponent<Renderer>();
        _material = new Material(_renderer.material);
        _renderer.material = _material;

        NetworkTransform = this.gameObject.GetComponent<NetworkTransform>();
        
        _rigidbody = this.gameObject.GetComponent<Rigidbody>();
        _rigidbody.maxDepenetrationVelocity = 10;

        _defaultCameraFollowOffset = CameraFollowOffset;
        
        SetProspectColor();
    }
    private void SetDefaults()
    {
        switch (Class)
        {
            case Classification.Throwy:
                Movement = new MovementSettings(Globals.Instance.InanimateDefaults.Light.Movement);
                MaxHealth = Globals.Instance.InanimateDefaults.Light.Health;
                HealthRegenerationRate = Globals.Instance.InanimateDefaults.Light.HealthRegenerationRate;
                HealthRegenerationDelay = Globals.Instance.InanimateDefaults.Light.HealthRegenerationDelay;
                break;
            case Classification.Squirty:
                Movement = new MovementSettings(Globals.Instance.InanimateDefaults.Medium.Movement);
                MaxHealth = Globals.Instance.InanimateDefaults.Medium.Health;
                HealthRegenerationRate = Globals.Instance.InanimateDefaults.Medium.HealthRegenerationRate;
                HealthRegenerationDelay = Globals.Instance.InanimateDefaults.Medium.HealthRegenerationDelay;
                break;
            case Classification.Smashy:
                Movement = new MovementSettings(Globals.Instance.InanimateDefaults.Heavy.Movement);
                MaxHealth = Globals.Instance.InanimateDefaults.Heavy.Health;
                HealthRegenerationRate = Globals.Instance.InanimateDefaults.Heavy.HealthRegenerationRate;
                HealthRegenerationDelay = Globals.Instance.InanimateDefaults.Heavy.HealthRegenerationDelay;
                break;
        }

        Movement.Grounded = true;
        Movement.Jumped = false;
        Movement.Lunging = false;

        Health = MaxHealth;
    }


    private void SetProspectColor()
    {
        Color selectionColor = SelectionColor;
        switch (Class)
        {
            case Classification.Throwy:
                selectionColor = Globals.Instance.InanimateDefaults.Colors.ThrowySelectionColor;
                break;
            case Classification.Squirty:
                selectionColor = Globals.Instance.InanimateDefaults.Colors.SquirtySelectionColor;
                break;
            case Classification.Smashy:
                selectionColor = Globals.Instance.InanimateDefaults.Colors.SmashySelectionColor;
                break;
        }
        _material.SetColor("_SelectionColor", selectionColor);
    }
    /// <summary>
    /// Sets the state of show selection color, in the inanimate objects material.
    /// </summary>
    /// <param name="show"> Determines the state of show selection color.</param>
    public void ToggleProspectColor(bool show)
    {
        _material.SetFloat("_ShowSelectionColor", show ? 1 : 0);
    }

    public void UpdateMaterialLighting(bool velocityBias = false)
    {
        // Defines the color of the light map color property in the inanimate objects material to that of the light map pixel below
        if (velocityBias && _rigidbody != null && _rigidbody.velocity.magnitude == 0)
            return;

        LightmapColor = LightmapHelper.GetColor(this.transform.position, Vector3.down);

        _material.SetColor("_LightmapColor", LightmapColor);
    }


    public void SetControlled(LocalPlayer localPlayer)
    {
        LocalPlayer = localPlayer;

        GamerId = localPlayer.Profile.GamerId;
        TeamId = localPlayer.Profile.TeamId;

        Controlled = true;

        AlertControlled();
        SetDefaults();

        if (Weapon != null)
            Weapon.IdentifyOwner(GamerId, this);
    }
    public void AlertControlled()
    {
        if (NetworkSessionManager.IsLocal)
            HUDManager.Instance.PopulateWaypointsOnControl(GamerId);
    }
    [ClientRpc]
    public void RpcSetControlled(int gamerId, int teamId)
    {
        GamerId = gamerId;
        TeamId = teamId;
        Controlled = true;

        NetworkTransform.sendInterval = 0.03243f;
        NetworkTransform.interpolateMovement = 1;
        HUDManager.Instance.PopulateWaypointsOnControl(gamerId);
    }


    /// <summary>
    /// Moves the inanimate object along the input direction.
    /// </summary>
    /// <param name="input"> Defines the direction that the inanimate object is to move along.</param>
    public void Move(Vector2 input)
    {
        if (input.magnitude == 0)
            return;
        if (Movement.Lunging)
            return;

        // Defines directions
        Vector3 camerFacing = LocalPlayer.CameraController.transform.forward;
        Vector3 forward = Quaternion.Euler(0, LocalPlayer.CameraController.transform.rotation.eulerAngles.y, 0) * Vector3.forward;
        Vector3 right =  LocalPlayer.CameraController.transform.right;

        // Determine movement speed, scale velocity by input, apply force.
        float movementSpeed = Movement.Grounded ? Movement.GroundedMovementSpeed : Movement.AirborneMovementSpeed;
        

        Vector3 velocity = ((forward * input.y) + (right * input.x)) * movementSpeed;
        

        _rigidbody.AddForce(velocity - _rigidbody.velocity, ForceMode.Force);

        if (!_aiming)
        {

            // Defines the angular velocity mod based on if the heavy inanimate object is stuck.
            float angularVelocityMod = 1;
            bool possiblyStuck = _rigidbody.angularVelocity.magnitude < Globals.Instance.InanimateDefaults.Heavy.StuckVelocityMin;
            if (Class == Classification.Smashy && Movement.Grounded && possiblyStuck)
                angularVelocityMod = Globals.Instance.InanimateDefaults.Heavy.StuckAngularVelocityIncrease;

            float angularVelocityScale = Movement.Grounded ? Movement.GroundedAngularVelocityScale : Movement.AirborneAngularVelocityScale;

            Vector3 angularVelocity = ((right * input.y) + (forward * -input.x)) * (movementSpeed * angularVelocityScale) * angularVelocityMod;
            _rigidbody.angularVelocity += angularVelocity;
        }
    }
    public void ResetVelocity()
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        UpdateMaterialLighting();
    }


    private void UpdateAiming()
    {
        RaycastHit raycastHit;
        bool hit = Target(out raycastHit);
        // Casts rays to determine the aiming position and rotates the inanimate object to face toward the aiming position.
        if (_aiming)
        {

            Vector3 direction = hit ? (raycastHit.point - Weapon.transform.position).normalized :
                ((LocalPlayer.CameraController.transform.position + (LocalPlayer.CameraController.transform.forward * Globals.Instance.InanimateDefaults.TargetingDistance)) - this.transform.position).normalized;

            // Determine rotation scale and speed, rotate
            float rotationScale = Mathf.Min(Vector3.Distance(direction, Weapon.transform.forward), 1);
            float rotationSpeed = Globals.Instance.InanimateDefaults.Medium.AimingTurnRate * rotationScale;

            Vector3 x = Vector3.Cross(Weapon.transform.forward, direction).normalized;
            Vector3 w = (x / Time.fixedDeltaTime) * rotationScale;
            Quaternion q = this.transform.rotation * _rigidbody.inertiaTensorRotation;

            Vector3 t = q * Vector3.Scale(_rigidbody.inertiaTensor, Quaternion.Inverse(q) * w);

            _rigidbody.angularVelocity = t * rotationSpeed;
        }
    }
    /// <summary>
    /// Rotates the inanimate object toward the viewing direction of its controlling players camera controller on input.
    /// </summary>
    public void Aim(bool input)
    {
        if (Class != InanimateObject.Classification.Squirty)
            return;

        _aiming = input;
        LocalPlayer.CameraController.Zooming = input;
    }

    private bool Target(out RaycastHit raycastHit)
    {
        // Store original layer, change layer to ignore raycast layer in order to avoid intersecting with the cast.
        int layerBackup = this.gameObject.layer;
        this.gameObject.layer = 2;

        // Determine if the cast hit.
        bool hit = LocalPlayer.CameraController.Target(Globals.Instance.InanimateDefaults.TargetingRadius * LocalPlayer.HeadsUpDisplay.ScaleFactor,
            Globals.Instance.InanimateDefaults.TargetingDistance, out raycastHit, Globals.Instance.InanimateDefaults.TargetingIgnoredLayers);


        // Restore original layer post cast
        this.gameObject.layer = layerBackup;

        CheckTargetingState(raycastHit);
        return hit;
    }
    private void CheckTargetingState(RaycastHit raycastHit)
    {
        TargetingState = TargetingType.None;
        LocalPlayer.CameraController.TurningScale = 1;

        if (raycastHit.collider != null)
        {
            InanimateObject inanimateObject = raycastHit.collider.gameObject.GetComponent<InanimateObject>();
            if (inanimateObject != null && inanimateObject.Controlled)
            {
                if (GameManager.Instance.Game.GameType.TeamGame)
                {
                    if (inanimateObject.TeamId != LocalPlayer.Profile.TeamId)
                        TargetingState = TargetingType.Enemy;
                    else
                        TargetingState = TargetingType.Friendly;
                }
                else
                    TargetingState = TargetingType.Enemy;

                if (TargetingState == TargetingType.Enemy)
                    LocalPlayer.CameraController.TurningScale = 0.5f;
            }
        }
    }


    /// <summary>
    /// Instantaneously sets the y-axis' velocity of the inanimate object to that of the jump velocity.
    /// </summary>
    public void Jump()
    {

        if (Movement.Lunging)
            return;
        
        if (!Movement.Grounded)
        {
            Movement.JumpAttemptExpiration = Time.timeSinceLevelLoad + Globals.Instance.InanimateDefaults.PreGroundedJumpWindow;

            bool withinJumpWindow = Movement.UngroundedJumpWindowExpiration > Time.timeSinceLevelLoad;
            if ((!withinJumpWindow || Movement.Jumped) && _rigidbody.velocity.magnitude > 0.01f)
                return;
        }
        Movement.JumpedTime = Time.timeSinceLevelLoad;
        // Add jump velocity, set grounding
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, Movement.JumpVelocity, _rigidbody.velocity.z);
        Movement.Grounded = false;
        Movement.JumpAttemptExpiration = -1;
        Movement.Jumped = true;
        Movement.UngroundedJumpWindowExpiration = -1;
    }
    private void CheckJumpAttempt()
    {
        // Determines upon landing; if the inanimate object has attempted to jump while airborne.
        if (Movement.Grounded && Movement.JumpAttemptExpiration >= Time.timeSinceLevelLoad)
            Jump();
    }
    private void SetGrounding(bool ground, bool hit = false)
    {
        if (hit && GameManager.Instance.Game.GameType.ClimbWalls)
        {
            Movement.Grounded = true;
            return;
        }
        // Determine the grounded state of the inanimate object.
        if (!ground && !hit)
           Movement.UngroundedJumpWindowExpiration = Time.timeSinceLevelLoad + Globals.Instance.InanimateDefaults.PostGroundedJumpWindow;
        
        Movement.Grounded = ground;

        if (ground)
        {
            Movement.Jumped = false;
            CheckJumpAttempt();
        }
    }

    /// <summary>
    /// Executes the inanimate objects attack.
    /// </summary>
    public void Attack()
    {
        switch (Class)
        {
            case Classification.Throwy:
                Lunge();
                break;
            case Classification.Squirty:
                Weapon.Fire();
                break;
        }
    }
    private void DirectAttack(InanimateObject inanimateObject)
    {
        if (inanimateObject == null || !inanimateObject.Controlled || _rigidbody.velocity.magnitude <= Globals.Instance.InanimateDefaults.Heavy.DamageApplicationVelocityMagnitude)
            return;

        // Directly applies damage to an inanimate object.
        GameManager.GameAspects.Profile profile = GameManager.Instance.GetProfileByGamerId(inanimateObject.GamerId);
        if (GameManager.Instance.Game.GameType.TeamGame && profile.TeamId == TeamId)
            return;

        Globals.Instance.InanimateDefaults.Heavy.CollideSelfPhysicalEffect.Cast(GamerId);

        // TODO: Add transfer physical effect, and vibration for controlling player
        if (profile.Local)
            inanimateObject.Damage(Globals.Instance.InanimateDefaults.Heavy.CollidePhysicalEffect.Damage.Damage, this.transform.position, GamerId);
        else
            NetworkSessionNode.Instance.CmdTransferDamage(GamerId, inanimateObject.GamerId, Globals.Instance.InanimateDefaults.Heavy.CollidePhysicalEffect.Damage.Damage);
        
        LocalPlayer.AttachedDamagedEnemy();
    }

    private void UpdateLunge()
    {
        if (Class != Classification.Throwy)
            return;

        // Updates the lunging process of the inanimate object.
        if (Controlled && !Movement.LungeNotified && Movement.NextLungeTime < Time.time)
        {
            Movement.LungeNotified = true;
            LocalPlayer.HeadsUpDisplay.AddEvent("lunge_allowed");
        }

        // If the object isn't lunging, abort
        if (Movement.Lunging)
        {
            if (Movement.AllowLungeRotationCorrection && CameraFollowOffset.x != 0 && CameraFollowOffset.y != 0)
                LocalPlayer.CameraController.transform.LookAt(Movement.LungeLookingPosition);

            // Update camera offset and set velocity
            Vector3 cameraFollowOffsetStored = CameraFollowOffset;
            CameraFollowOffset = Vector3.MoveTowards(CameraFollowOffset, new Vector3(0, 0, _defaultCameraFollowOffset.z), Globals.Instance.InanimateDefaults.Light.Lunge.FollowOffsetCorrectionRate * Time.deltaTime);

            _rigidbody.velocity = LocalPlayer.CameraController.transform.forward * Globals.Instance.InanimateDefaults.Light.Lunge.Velocity;
        }
        else
        {
            CameraFollowOffset = Vector3.MoveTowards(CameraFollowOffset, _defaultCameraFollowOffset, Globals.Instance.InanimateDefaults.Light.Lunge.FollowOffsetCorrectionRate * Time.deltaTime);
        }
    }

    private void Lunge()
    {
        // Starts the lunging process of the inanimate object.
        if (Movement.Lunging || Movement.NextLungeTime > Time.time)
            return;

        RaycastHit raycastHit;
        bool hit = Physics.Raycast(LocalPlayer.CameraController.transform.position, LocalPlayer.CameraController.transform.forward, out raycastHit,
            Mathf.Infinity, ~Globals.Instance.InanimateDefaults.TargetingIgnoredLayers);


        Movement.LungeLookingPosition = hit ? raycastHit.point : LocalPlayer.CameraController.transform.position + (LocalPlayer.CameraController.transform.forward * LocalPlayer.CameraController.Camera.farClipPlane);


        float distanceToLookingPosition = Vector3.Distance(LocalPlayer.CameraController.transform.position, Movement.LungeLookingPosition);
        Movement.AllowLungeRotationCorrection = distanceToLookingPosition > Globals.Instance.InanimateDefaults.Light.Lunge.MinCameraCorrectionLungeDistance;

        Development.AddTimedSphereGizmo(Color.white, 1f, Movement.LungeLookingPosition, 5f);

        Movement.Lunging = true;
        LocalPlayer.HeadsUpDisplay.AddEvent("lunge_disallowed");
    }
    private void LungeImpact(Vector3 position, Vector3 surfaceNormal)
    {
        if (!Movement.Lunging)
            return;

        // Create effect, update lunge status, set notification state, scale velocity, reduce health
        Movement.Lunging = false;
        Movement.NextLungeTime = Time.time + Globals.Instance.InanimateDefaults.Light.Lunge.NextLungeDelay;
        Movement.LungeNotified = false;
        
        // Reduce velocity
        _rigidbody.velocity *= Globals.Instance.InanimateDefaults.Light.Lunge.ImpactVelocityLossFraction;

        // Create impact effect
        GameObject newEffect = Instantiate(Globals.Instance.InanimateDefaults.Light.Lunge.ImpactEffect, position, Quaternion.LookRotation(surfaceNormal), Globals.Instance.Containers.Effects);
        EffectUtility effectUtility = newEffect.GetComponent<EffectUtility>();
        if (effectUtility != null)
            effectUtility.Cast(null, GamerId, false);

        if (!NetworkSessionManager.IsLocal)
            NetworkSessionNode.Instance.SpawnEffectUtility(GamerId, Globals.Instance.InanimateDefaults.Light.Lunge.ImpactEffect, position, Quaternion.LookRotation(surfaceNormal), NetworkSessionManager.IsHost);


        // Damage(Globals.Instance.InanimateDefaults.Light.Lunge.SelfImpactPhysicalEffect.Damage, Vector3.zero, GamerId);
        Globals.Instance.InanimateDefaults.Light.Lunge.SelfImpactPhysicalEffect.Cast(GamerId);
    }

    private void CheckStayCollision(ContactPoint[] contacts)
    {
        if (!Controlled)
            return;

        // If not controlled, or grounding has been set this fixed update - abort.
        if (Time.fixedTime == Movement.LastStayGroundSetTime || Time.timeSinceLevelLoad < Movement.JumpedTime + Globals.Instance.InanimateDefaults.StayGroundingPostJumpDelay)
            return;

        
        bool grounded = false;
        for (int contact = 0; contact < contacts.Length; contact++)
        {
            bool withinAngleBias = Vector3.Angle(Vector3.up, contacts[contact].normal) < Globals.Instance.InanimateDefaults.AngleBias;

            if (!Movement.Lunging)
            {
                // Check grounding
                if (withinAngleBias)
                {
                    grounded = true;
                    break;
                }
            }
            else
            {
                // Check lunge collision
                InanimateObject inanimateObject = contacts[contact].otherCollider.gameObject.GetComponent<InanimateObject>();

                if (inanimateObject != null || !withinAngleBias)
                {
                    LungeImpact(contacts[contact].point, contacts[contact].normal);
                    return;
                }
            }
        }

        SetGrounding(grounded, true);

        if(grounded)
            Movement.LastStayGroundSetTime = Time.fixedTime;
    }
    private void CheckEnterCollision(Collision collision)
    {
        if (!Controlled)
            return;

        if (Movement.Lunging)
            LungeImpact(collision.contacts[0].point, collision.contacts[0].normal);
        else
        {

            float contactAngle = Vector3.Angle(Vector3.up, collision.contacts[0].normal);
            SetGrounding(contactAngle < Globals.Instance.InanimateDefaults.AngleBias, true);

            if (Class == Classification.Smashy)
                DirectAttack(collision.gameObject.GetComponent<InanimateObject>());
        }
    }


    /// <summary>
    /// Attempts to apply a power up to the inanimate object.
    /// </summary>
    /// <param name="powerupType"> Defines the type of powerup trying to be applied.</param>
    /// <returns> Determines if the powerup was applied.</returns>
    public void ApplyPowerup(Powerup.PowerupType powerupType)
    {
        switch (powerupType)
        {
            case Powerup.PowerupType.Heal:
                Health = MaxHealth;
                break;
        }
    }


    // Death and damage

    /// <summary>
    /// Applies damage to the inanimate object.
    /// </summary>
    /// <param name="damage"> Defines the amount of damage applied to the inanimate object.</param>
    public void Damage(float damage, Vector3 damagePosition, bool showDirection = true)
    {
        Damage(damage, damagePosition, 0, showDirection);
    }
    /// <summary>
    /// Applies damage to the inanimate object.
    /// </summary>
    /// <param name="damage"> Defines the amount of damage applied to the inanimate object.</param>
    /// <param name="player"> Determines which player is responsible for the damage.</param>
    public void Damage(float damage, Vector3 damagePosition, int casterGamerId = 0, bool showDirection = true)
    {
        if (_dead)
            return;

        if (!NetworkSessionManager.IsLocal && !hasAuthority)
            return;

        _lastDamageCaster = casterGamerId;
        _lastDamageCasterClearTime = Time.time + GameManager.Instance.Game.GameType.LastDamageCasterDuration;


        LocalPlayer.AttachedDamaged(damagePosition, showDirection);

        // Apply damage, die if health is depleted
        Health = Mathf.Max(Health - damage, 0);

        _healthRegenerationStartTime = Time.time + HealthRegenerationDelay;

        if (Health <= 0)
            Die(true, casterGamerId, this.transform.position, Quaternion.LookRotation(Vector3.up));
    }
    /// <summary>
    /// Instantly kills the inanimate object.
    /// </summary>
    /// <param name="countDeath"> Determines if the death will be counted toward the inanimate objects controlling players profile.</param>
    public void Kill(bool countDeath)
    {
        Die(countDeath, _lastDamageCaster, this.transform.position, Quaternion.LookRotation(Vector3.up));
    }
    /// <summary>
    /// Triggers the death process of the inanimate object.
    /// </summary>
    /// <param name="player"> Determines which player is responsible for the death.</param>
    /// <param name="countDeath"> Determines if the death will be counted toward the inanimate objects controlling players profile.</param>
    public void Die(bool countDeath = true, int casterGamerId = 0, Vector3 effectPosition = new Vector3(), Quaternion effectRotation = new Quaternion())
    {
        if (_dead || LocalPlayer == null)
            return;

        _dead = true;

        if (casterGamerId == 0)
            casterGamerId = _lastDamageCaster;

        LocalPlayer.AttachedDied(countDeath, casterGamerId);

        if (effectPosition == Vector3.zero)
            effectPosition = this.transform.position;

        SpawnDeathEffect(casterGamerId, effectPosition, effectRotation);

        this.gameObject.SetActive(false);

        if (!NetworkSessionManager.IsClient)
            Destroy(this.gameObject);

        if (NetworkSessionManager.IsClient)
            NetworkSessionNode.Instance.CmdDestroy(this.gameObject);
    }
    public void SpawnDeathEffect(int casterGamerId = 0, Vector3 effectPosition = new Vector3(), Quaternion effectRotation = new Quaternion())
    {
        // Instantiate death effect
        if (DeathEffect != null)
        {
            GameObject deathEffect = Instantiate(DeathEffect, effectPosition, effectRotation);
            EffectUtility effectUtility = deathEffect.GetComponent<EffectUtility>();
            effectUtility.Cast(null, GamerId);

            if (!NetworkSessionManager.IsLocal)
                NetworkSessionNode.Instance.SpawnEffectUtility(GamerId, DeathEffect, effectPosition, effectRotation, NetworkSessionManager.IsHost);

        }
    }

    private void UpdateLastDamageCaster()
    {
        if (Time.time >= _lastDamageCasterClearTime)
            _lastDamageCaster = 0;
    }
    private void UpdateHealthRegeneration()
    {
        if (Time.time >= _healthRegenerationStartTime && !Movement.Lunging)
            Health = Mathf.Min(Health + Time.deltaTime * HealthRegenerationRate, MaxHealth);
    }
    #endregion
}
