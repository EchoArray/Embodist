using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

public class EffectUtility : NetworkBehaviour
{
    #region Values
    [HideInInspector]
    [SyncVar]
    public int GamerId;
    [HideInInspector]
    [SyncVar]
    public bool Host;

    public enum ExecutionType
    {
        OnAwake,
        Internal,
    }
    public ExecutionType Type;

    public float Lifespan;

    public bool UseLightmapColoration;

    [Serializable]
    public class Effect
    {
        public GameObject Prefab;
        [Space(10)]
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;
    }
    public Effect[] Effects; 
    #endregion

    #region Unity Functions
    private void Start()
    {
        if (hasAuthority || NetworkSessionManager.IsHost && Host)
            this.gameObject.SetActive(false);
        else
            Initialize();
    } 
    #endregion

    private void Initialize()
    {
        this.transform.SetParent(Globals.Instance.Containers.Effects);
        if (Type == ExecutionType.OnAwake)
            Cast();
    }

    #region Functions

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
