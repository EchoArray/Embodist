using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Globals : MonoBehaviour 
{
    #region Values
    public static Globals Instance;
    /// <summary>
    /// Defines the layer in-which each inanimate object is set.
    /// </summary>
    public const int INANIMATE_OBJECT_LAYER = 8;
    /// <summary>
    /// Defines the layer in-which each structure object is set.
    /// </summary>
    public const int STRUCTURE_LAYER = 12;
    /// <summary>
    /// Defines the layer in-which each scenery object is set.
    /// </summary>
    public const int SCENERY_LAYER = 13;


    /// <summary>
    /// Determines the layers that are used to retrieve lightmap color data.
    /// </summary>
    public LayerMask LightmapColorLayers;

    public CameraEffect CameraEffect;

    /// <summary>
    /// Defines the applied level of gravity.
    /// </summary>
    public Material AreaIsolatorMaterial;

    [Serializable]
    public class InanimateValues
    {
        [Serializable]
        public class GenericColors
        {
            /// <summary>
            /// Defines the allowed selection material color for a throwy inanimate object when highlighted.
            /// </summary>
            public Color ThrowySelectionColor;
            /// <summary>
            /// Defines the allowed selection material color for a throwy inanimate object when highlighted.
            /// </summary>
            public Color SquirtySelectionColor;
            /// <summary>
            /// Defines the allowed selection material color for a throwy inanimate object when highlighted.
            /// </summary>
            public Color SmashySelectionColor;
        }
        public GenericColors Colors;

        /// <summary>
        /// Defines the camera effect applied to a players camera upon being damaged.
        /// </summary>
        public CameraEffect TakeDamageCameraEffect;

        /// <summary>
        /// Defines the grounding and jumping angle bias for an inanimate object.
        /// </summary>
        public float AngleBias = 43f;

        /// <summary>
        /// Defines the duration in which a jump will be trigger upon an airborne jump attempt and landing.
        /// </summary>
        [Space(10)]
        public float PreGroundedJumpWindow;
        /// <summary>
        /// Defines the duration in which an inanimate object is allowed to jump upon becoming ungrounded without a jump.
        /// </summary>
        public float PostGroundedJumpWindow;

        /// <summary>
        /// Determines the duration after a jump in-which stay collision detection is stunned.
        /// </summary>
        public float StayGroundingPostJumpDelay;

        /// <summary>
        /// Defines the movement addition when there is no gravity.
        /// </summary>
        public float ZeroGravityMovementAddition;
        /// <summary>
        /// Defines the movement fraction when there is no gravity.
        /// </summary>
        public float ZeroGravityMovementFraction;
        /// <summary>
        /// Defines the angular velocity fraction when there is no gravity.
        /// </summary>
        public float ZeroGravityAngularFraction;

        /// <summary>
        /// Defines the targeting radius of an inanimate object.
        /// </summary>
        [Space(10)]
        public float TargetingRadius;
        /// <summary>
        /// Defines the targeting distance of an inanimate object.
        /// </summary>
        public float TargetingDistance;
        /// <summary>
        /// Determines the layers ignored for targeting.
        /// </summary>
        public LayerMask TargetingIgnoredLayers;

        public class ClassValues
        {
            /// <summary>
            /// Defines the initial health of an inanimate object.
            /// </summary>
            public float Health;
            /// <summary>
            /// Defines the duration in which health regeneration is delayed after the heavy inanimate object has received damage.
            /// </summary>
            public float HealthRegenerationDelay = 3.243f;
            /// <summary>
            /// Defines the rate in-which the health of the inanimate object regenerates.
            /// </summary>
            public float HealthRegenerationRate = 24f;
            /// <summary>
            /// Defines the movement values of an inanimate object.
            /// </summary>
            [Space(5)]
            public InanimateObject.MovementSettings Movement;
        }

        [Serializable]
        public class LightClassValues : ClassValues
        {
            /// <summary>
            /// Defines the lunge values of a light class inanimate object
            /// </summary>
            [Serializable]
            public class GlobalLungeValues
            {
                /// <summary>
                /// Defines the minimum distance in-which the camera will correct its rotation upon lunging.
                /// </summary>
                public float MinCameraCorrectionLungeDistance;

                /// <summary>
                /// Defines the physical effect applied to a lunging inanimate object up-on impact.
                /// </summary>
                public PhysicalEffect SelfImpactPhysicalEffect;
                /// <summary>
                /// Defines the physical effect applied to a lunging inanimate object up-on impact.
                /// </summary>
                public GameObject ImpactEffect;
                /// <summary>
                /// Defines the velocity of the lunge of a light class inanimate object.
                /// </summary>
                public float Velocity;
                /// <summary>
                /// Defines the camera offset correction rate while a light class inanimate object is lunging.
                /// </summary>
                public float FollowOffsetCorrectionRate;
                /// <summary>
                /// Defines the look sensitivity of a lunging light class inanimate object.
                /// </summary>
                public float CameraOrbitSensitivity;
                /// <summary>
                /// Defines the duration in which a light class inanimate object must wait for its next lunge.
                /// </summary>
                public float NextLungeDelay;
                /// <summary>
                /// Defines the velocity loss of a light class inanimate object upon impact.
                /// </summary>
                public float ImpactVelocityLossFraction;
            }
            public GlobalLungeValues Lunge;

        }
        [Space(10)]
        public LightClassValues Light;

        [Serializable]
        public class MediumClassValues : ClassValues
        {
            /// <summary>
            /// Defines the rate in which the medium class inanimate object rotates toward its aiming direction.
            /// </summary>
            [Space(10)]
            public float AimingTurnRate;
        }
        public MediumClassValues Medium;

        [Serializable]
        public class HeavyClassValues : ClassValues
        {
            /// <summary>
            /// Defines the physical effect applied to the inanimate object that the heavy inanimate object has collided with.
            /// </summary>
            [Space(10)]
            public PhysicalEffect CollidePhysicalEffect;
            public PhysicalEffect CollideSelfPhysicalEffect;

            public float DamageApplicationVelocityMagnitude = 3.243f;
            /// <summary>
            /// Defines the minimum velocity that identifies as the heavy inanimate object as being stuck.
            /// </summary>
            public float StuckVelocityMin;
            /// <summary>
            /// Defines the increase in velocity applied when the heavy inanimate object is stuck.
            /// </summary>
            public float StuckAngularVelocityIncrease;
        }
        public HeavyClassValues Heavy;
    }
    public InanimateValues InanimateDefaults;
    
    [Serializable]
    public class WeaponValues
    {
        /// <summary>
        /// Determines the layers ignored by a projectile.
        /// </summary>
        public LayerMask ProjectileIgnoredLayers;
    }
    public WeaponValues WeaponDefaults;

    [Serializable]
    public class GlobalContainers
    {
        /// <summary>
        /// Defines the container for all camera objects.
        /// </summary>
        public Transform Cameras;
        /// <summary>
        /// Defines the container for all player objects.
        /// </summary>
        public Transform Players;
        /// <summary>
        /// Defines the container for all projectile objects.
        /// </summary>
        public Transform Projectiles;
        /// <summary>
        /// Defines the container for all inanimate objects.
        /// </summary>
        public Transform InanimateObjects;
        /// <summary>
        /// Defines the container for all area isolator objects.
        /// </summary>
        public Transform AreaIsolators;
        /// <summary>
        /// Defines the container for all ui objects.
        /// </summary>
        public Transform UI;
        /// <summary>
        /// Defines the container for all objective objects.
        /// </summary>
        public Transform Objectives;
        /// <summary>
        /// Defines the container for all effect objects.
        /// </summary>
        public Transform Effects;
    }
    [Space(15)]
    public GlobalContainers Containers;

    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    } 
    #endregion

    #region Functions
    private void Initialize()
    {
        Instance = this;
    }
    #endregion
}
