using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

public class EffectUtility : NetworkBehaviour
{
    #region Values
    /// <summary>
    /// Defines the gamer id for the caster of this effect utility.
    /// </summary>
    [HideInInspector]
    [SyncVar]
    public int GamerId;
    /// <summary>
    /// Determines if the effect utility was spawned by the host.
    /// </summary>
    [HideInInspector]
    [SyncVar]
    public bool Host;

    public enum ExecutionType
    {
        OnAwake,
        Internal,
    }
    /// <summary>
    /// Determines when to cast the effect utility.
    /// </summary>
    public ExecutionType Type;

    /// <summary>
    /// Defines the duration in-which the effect is allowed to live.
    /// </summary>
    public float Lifespan;

    /// <summary>
    /// Determines if the effect utility uses the color of the lightmap to modify the color of instantiated effects.
    /// </summary>
    public bool UseLightmapColoration;

    [Serializable]
    public class Effect
    {
        /// <summary>
        /// Defines the spawned game object.
        /// </summary>
        public GameObject Prefab;
        /// <summary>
        /// Defines the position offset the spawed game object.
        /// </summary>
        [Space(10)]
        public Vector3 PositionOffset;
        /// <summary>
        /// Defines the rotation offset the spawed game object.
        /// </summary>
        public Vector3 RotationOffset;
    }
    /// <summary>
    /// A collection of effects to be instantiated upon cast.
    /// </summary>
    public Effect[] Effects;
    #endregion

    #region Unity Functions
    private void Start()
    {
        Initialize();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        this.transform.SetParent(Globals.Instance.Containers.Effects);

        if (hasAuthority || NetworkSessionManager.IsHost && Host)
            this.gameObject.SetActive(false);
        else if (Type == ExecutionType.OnAwake)
                Cast();
    }

    public void Cast(GameObject hitObject = null, int gamerId = 0, bool host = false)
    {
        Color surfaceColor = Color.white;
        if (UseLightmapColoration)
            surfaceColor = LightmapHelper.GetColor(this.transform.position + Vector3.up, Vector3.down);

        Cast(surfaceColor, hitObject, gamerId, host);
    }
    public void Cast(Color lightmapColor, GameObject hitObject = null, int gamerId = 0, bool host = false)
    {
        GamerId = gamerId;
        Host = host;

        if (UseLightmapColoration)
        {
            ParticleSystem particleSystem = this.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule mainModule = particleSystem.main;
            mainModule.startColor = particleSystem.main.startColor.color * Mathf.Clamp(lightmapColor.a * 7, 0, 1);
        }
        else
            lightmapColor = Color.white;

        SpawnObjects(hitObject, lightmapColor);


        Destroy(this.gameObject, Lifespan);
    }

    private void SpawnObjects(GameObject hitObject, Color lightmapColor)
    {
        foreach (Effect effect in Effects)
        {
            if (effect.Prefab == null)
                continue;

            Quaternion rotation = this.transform.rotation * Quaternion.Euler(effect.RotationOffset);
            Vector3 position = (rotation * effect.PositionOffset) + this.transform.position;

            GameObject newEffect = Instantiate(effect.Prefab, position, rotation, this.transform);

            PhysicalEffect damageEffect = newEffect.GetComponent<PhysicalEffect>();
            if (damageEffect != null)
            {
                damageEffect.Cast(GamerId);
                continue;
            }

            Decal decal = newEffect.GetComponent<Decal>();
            if (decal != null)
            {
                decal.Cast(hitObject, lightmapColor);
                continue;
            }
        }
    }
    #endregion
}
