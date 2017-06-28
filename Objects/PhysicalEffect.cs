using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PhysicalEffect : MonoBehaviour
{
    #region Values

    /// <summary>
    /// Defines the gamer id for the caster of this physical effect.
    /// </summary>
    [HideInInspector]
    public int GamerId;
    
    public enum ExecutionType
    {
        OnAwake,
        Internal,
        Trigger,
    }
    /// <summary>
    /// Determines when to cast the damage effect.
    /// </summary>
    public ExecutionType Type;

    /// <summary>
    /// Determines if the effect directly affects its castor.
    /// </summary>
    [Space(15)]
    public bool DirectlyAffectCastor;

    public bool OnlyApplyCameraEffectOnDamage;
    /// <summary>
    /// Defines the camera effect applied to any intersecting inanimate objects camera effector.
    /// </summary>
    public CameraEffect CameraEffect;

    /// <summary>
    /// Defines the duration in-which the effect is allowed to live.
    /// </summary>
    [Space(15)]
    public float LifeSpan;
    /// <summary>
    /// Determines if the physical effect lives forever.
    /// </summary>
    public bool LivesForever;

    /// <summary>
    /// Defines the radius of the effect
    /// </summary>
    [Space(15)]
    public float Radius;



    [Serializable]
    public class ForceSettings
    {
        public enum ForceDirectionType
        {
            Omni,
            LocalUp,
            LocalForward,
            LocalRight,
            Up,
            Forward,
            Right,
            Custom
        }
        /// <summary>
        /// Determines the direction in-which force will be applied to the affected objects rigid body.
        /// </summary>
        public ForceDirectionType ForceDirection;
        /// <summary>
        /// Defines a custom force direction, only used when custom (direction) is selected.
        /// </summary>
        public Vector3 CustomDirection;

        public enum ForceApplicationType
        {
            Set,
            Add
        }
        /// <summary>
        /// Determines how to apply force to an affected object
        /// </summary>
        [Space(10)]
        public ForceMode ForceMode = ForceMode.Force;
        /// <summary>
        /// Determines if force application will be clamped if the affected objects has reached or is within range of the applied force.
        /// </summary>
        public bool ClampForce;
        /// <summary>
        /// Determines if the volicity of the affected object will be striped previous to the new force application.
        /// </summary>
        public bool RemoveExistingVelocity;

        /// <summary>
        /// Defines the force added to the affected object.
        /// </summary>
        [Space(10)]
        public float Force;

        /// <summary>
        /// Determines how force will be scaled based on the affected objects distance from the center of the effect with-in its radius.
        /// </summary>
        public AnimationCurve DistanceFalloff = AnimationCurve.Linear(0, 1, 1, 1);
    }
    [Space(10)]
    public ForceSettings Force;
    
    [Serializable]
    public class DamageSettings
    {
        /// <summary>
        /// Defines the maximum damage applied to an object affected with the effect
        /// </summary>
        public float Damage;
        /// <summary>
        /// Determines how damage will be scaled based on the affected objects distance from the center of the effect.
        /// </summary>
        public AnimationCurve DistanceFalloff = AnimationCurve.Linear(0, 1, 1, 1);
    }
    public DamageSettings Damage;

    [Serializable]
    public class VibrationSettings
    {
        /// <summary>
        /// Defines the intensity of the vibration applied to the affecties input device.
        /// </summary>
        public float Intensity;
        /// <summary>
        /// Defines the duration of the vibration.
        /// </summary>
        public float Duration;
        /// <summary>
        /// Determines how vibration intensity and duration will be scaled based on the affected objects distance from the center of the effect.
        /// </summary>
        public AnimationCurve DistanceFalloff = AnimationCurve.Linear(0, 1, 1, 1);
    }
    public VibrationSettings Vibration;

    [Serializable]
    public class ScreenShakeSettings
    {
        /// <summary>
        /// Defines the intensity of the shake applied to the affecties camera.
        /// </summary>
        public float Intensity;
        /// <summary>
        /// Defines the duration of the screen shake.
        /// </summary>
        public float Duration;
        /// <summary>
        /// Determines the intensity at-which the screen will shake along the duration.
        /// </summary>
        public AnimationCurve IntensityOverLifetime = AnimationCurve.Linear(0, 1, 1, 0);
        /// <summary>
        /// Determines how screen shake intensity and duration will be scaled based on the affected objects distance from the center of the effect.
        /// </summary>
        public AnimationCurve DistanceFalloff = AnimationCurve.Linear(0, 1, 1, 1);
    }
    public ScreenShakeSettings ScreenShake;

    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        if(LifeSpan > 0)
            Omit();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        if(this.transform.parent == null)
            this.transform.SetParent(Globals.Instance.Containers.Effects);

        if (Type == ExecutionType.OnAwake)
            Cast();

        if (!LivesForever)
            Destroy(this.gameObject, LifeSpan);
    }

    public void Cast(int gamerId = 0)
    {
        Development.AddTimedSphereGizmo(Color.red, Radius, this.transform.position, 3);

        GamerId = gamerId;
        Omit();
    }

    private void Omit()
    {
        if (DirectlyAffectCastor)
        {
            if (GamerId != 0)
            {
                LocalPlayer localPlayer = GameManager.Instance.GetProfileByGamerId(GamerId).LocalPlayer;
                if (localPlayer != null)
                {
                    InanimateObject casterInanimateObject = localPlayer.InanimateObject;
                    Affect(casterInanimateObject.gameObject, this.transform, false);
                }
                
            }
        }
        else
        {
            // Find and apply to all objects in radius
            Collider[] objectsInRange = Physics.OverlapSphere(this.transform.position, Radius);
            List<GameObject> previouslyAffectedGameObjects = new List<GameObject>();
            foreach (Collider objectInRange in objectsInRange)
            {
                if (!previouslyAffectedGameObjects.Contains(objectInRange.gameObject))
                {
                    // Avoid duplicate applications to objects with mutiple colliders
                    previouslyAffectedGameObjects.Add(objectInRange.gameObject);

                    // Apply
                    Affect(objectInRange.gameObject, this.transform, true);
                }
            }
        }
    }

    public void Affect(GameObject gameObject, Transform effectTransform, bool showDirection)
    {
        // Define distance
        float distance = Vector3.Distance(effectTransform.position, gameObject.transform.position);

        // Determine caster inanimate object
        InanimateObject caster = GameManager.Instance.GetInanimateByGamerId(GamerId);
        // Determine affectie inanimate object
        InanimateObject affectie = DirectlyAffectCastor ? caster : gameObject.GetComponent<InanimateObject>();

        bool affectieFound = affectie != null;

        if (!affectieFound || (affectieFound && !affectie.Movement.Lunging))
            ApplyForce(gameObject, effectTransform, distance, Radius, Force);

        if (affectieFound && affectie.Controlled)
        {
            ApplyVibration(affectie, distance, Radius, Vibration);
            ApplyScreenShake(affectie, distance, Radius, ScreenShake);

            if(!OnlyApplyCameraEffectOnDamage && CameraEffect != null)
                CameraEffect.Apply(affectie);

            bool hasCaster = caster != null;
            if (!hasCaster || hasCaster && DirectlyAffectCastor || (!GameManager.Instance.Game.GameType.TeamGame || (GameManager.Instance.Game.GameType.TeamGame && caster.TeamId != affectie.TeamId)) && caster != affectie)
            {
                Vector3 position = hasCaster ? caster.transform.position : effectTransform.position;
                ApplyDamage(affectie, distance, Radius, Damage,GamerId, position, showDirection);

                if(OnlyApplyCameraEffectOnDamage && CameraEffect != null)
                    CameraEffect.Apply(affectie);
            }
        }
    }
    
    public static void ApplyVibration(InanimateObject inanimateObject, float distance, float radius, VibrationSettings vibrationSettings)
    {
        // Applies vibration to a local players controller.

        if (inanimateObject.LocalPlayer == null || vibrationSettings.Intensity == 0)
            return;
        // Determine scale

        float scale = FalloffScale(distance, radius, vibrationSettings.DistanceFalloff);
        float intensity = vibrationSettings.Intensity * scale;
        float duration = vibrationSettings.Duration * scale;
        
        inanimateObject.LocalPlayer.AddVibration(new XboxInputManager.Vibration(inanimateObject.LocalPlayer.Profile.ControllerId, intensity, duration));
    }
    public static void ApplyScreenShake(InanimateObject inanimateObject, float distance, float radius, ScreenShakeSettings screenShakeSettings)
    {
        if (screenShakeSettings.Duration == 0 || inanimateObject.LocalPlayer == null)
            return;

        // Determine scale based on distance
        float scale = FalloffScale(distance, radius, screenShakeSettings.DistanceFalloff);

        // Scale aspects
        float duration = screenShakeSettings.Duration * scale;
        float intensity = screenShakeSettings.Intensity * scale;

        inanimateObject.LocalPlayer.CameraController.AddShake(duration, intensity, screenShakeSettings.IntensityOverLifetime);
    }
    public static void ApplyDamage(InanimateObject inanimateObject, float distance, float radius, DamageSettings damageSettings, int casterId, Vector3 position, bool showDirection)
    {
        // Applies damage to an inanimate object.

        if (damageSettings.Damage <= 0)
            return;


        // If the object is an inanimate object, scale damage based on distance and apply
        float scale = FalloffScale(distance, radius, damageSettings.DistanceFalloff);
        // Scale damage
        float damage = damageSettings.Damage * scale;

        if (damage != 0)
        {
            // Define profiles
            GameManager.GameAspects.Profile casterProfile = GameManager.Instance.GetProfileByGamerId(casterId);
            GameManager.GameAspects.Profile affectieProfile = GameManager.Instance.GetProfileByGamerId(inanimateObject.GamerId);
            
            if (casterProfile != null && affectieProfile != null && casterProfile!= affectieProfile && casterProfile.LocalPlayer != null)
                casterProfile.LocalPlayer.AttachedDamagedEnemy();

            if (!affectieProfile.Local)
                NetworkSessionNode.Instance.CmdTransferDamage(casterId, affectieProfile.GamerId, damage);
            else if (casterProfile == null || casterProfile.Local)
                inanimateObject.Damage(damage, position, casterId, showDirection);
        }
    }
    public static void ApplyForce(GameObject gameObject, Transform effectTransform, float distance, float radius, ForceSettings forceSettings)
    {
        // Applies force to the affected objects rigid body.

        Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();

        if (rigidBody == null || forceSettings.Force == 0)
            return;

        // Determine direciton
        Vector3 forceDirection = Vector3.zero;
        switch (forceSettings.ForceDirection)
        {
            case ForceSettings.ForceDirectionType.Omni:
                forceDirection = (gameObject.transform.position - effectTransform.transform.position).normalized;
                break;
            case ForceSettings.ForceDirectionType.LocalUp:
                forceDirection = effectTransform.up;
                break;
            case ForceSettings.ForceDirectionType.LocalForward:
                forceDirection = effectTransform.forward;
                break;
            case ForceSettings.ForceDirectionType.LocalRight:
                forceDirection = effectTransform.right;
                break;
            case ForceSettings.ForceDirectionType.Up:
                forceDirection = Vector3.up;
                break;
            case ForceSettings.ForceDirectionType.Forward:
                forceDirection = Vector3.forward;
                break;
            case ForceSettings.ForceDirectionType.Right:
                forceDirection = Vector3.right;
                break;
            case ForceSettings.ForceDirectionType.Custom:
                forceDirection = forceSettings.CustomDirection;
                break;
        }

        // Scale force
        float scale = FalloffScale(distance, radius, forceSettings.DistanceFalloff);
        // Scale force
        float force = forceSettings.Force * scale;

        if (forceSettings.ClampForce)
        {
            // Clamp force, as to avoid exceeding the applied velocity
            float existingVelocity = Vector3.Dot(rigidBody.velocity, forceDirection);
            if (existingVelocity >= force)
                return;
            else
            {
                force = force - Mathf.Abs(existingVelocity);

                if (forceSettings.Force > 0 && force < 0 || forceSettings.Force < 0 && force > 0)
                    force = 0;
            }
        }

        if (forceSettings.RemoveExistingVelocity)
            rigidBody.velocity = Vector3.zero;


        rigidBody.AddForce(force * forceDirection, forceSettings.ForceMode);

    }

    public static float FalloffScale(float distance, float radius, AnimationCurve falloff)
    {
        if (distance > radius)
            return 1;
        else
        {
            float time = radius == 0 ? 1 : Mathf.Max(distance / radius, 0);
            return falloff.Evaluate(time);
        }
    }
    #endregion
}
