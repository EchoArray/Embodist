using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraEffector : MonoBehaviour
{
    #region Values
    /// <summary>
    /// Defines the base shader for the material applied to the cameras render texture.
    /// </summary>
    public Shader Shader;
    
    /// Defines the base effect of the camera effect.
    private CameraEffect.EffectSettings.ColorSettings _baseEffect;
    // A collection of effects, to be combined and applied along with the base effect
    private List<CameraEffect.EffectSettings> _effects = new List<CameraEffect.EffectSettings>();

    /// <summary>
    /// Defines the material applied to the render texture of the camera.
    /// </summary>
    private Material _material;
    #endregion
    
    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        UpdateEffects();
    }

    private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        if (_material != null)        
            Graphics.Blit(sourceTexture, destTexture, _material);
        else
            Graphics.Blit(sourceTexture, destTexture);
    }

    private void OnDestroy()
    {
        Destroy(_material);
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        if (!SystemInfo.supportsImageEffects)
        {
            this.enabled = false;
            return;
        }
        SetDefaults();
    }

    private void SetDefaults()
    {
        _material = new Material(Shader);
        SetBaseEffect(Globals.Instance.CameraEffect.Effect.Colors);
    }

    private void UpdateEffects()
    {
#if UNITY_EDITOR
        SetBaseEffect(Globals.Instance.CameraEffect.Effect.Colors);
#endif

        if (_baseEffect == null)
            return;

        // Define base colors
        CameraEffect.EffectSettings.ColorSettings baseColors = new CameraEffect.EffectSettings.ColorSettings(_baseEffect);

        for (int i = 0; i < _effects.Count; i++)
        {
            CameraEffect.EffectSettings effectSettings = _effects[i];

            // Define scale
            float duration = effectSettings.Properties.KillTime - effectSettings.Properties.StartTime;
            float timeRemaining = effectSettings.Properties.KillTime - Time.time;
            float scale = timeRemaining / duration;
            
            // Combine effects into base colors
            CombineEffectColors(effectSettings.Colors, ref baseColors, scale);

            // Remove camera effect if it has become durated
            if (Time.time >= effectSettings.Properties.KillTime)
            {
                i--;
                _effects.Remove(effectSettings);
            }

        }
        ApplyEffects(baseColors);
    }

    private void CombineEffectColors(CameraEffect.EffectSettings.ColorSettings effectA, ref CameraEffect.EffectSettings.ColorSettings effectB, float scale)
    {
        // Add each color componenet of the effects together
        effectB.Brightness = Mathf.Max(0, effectB.Brightness + (effectA.Brightness * scale));
        effectB.Saturation = Mathf.Max(0, effectB.Saturation + (effectA.Saturation * scale));
        effectB.Contrast = Mathf.Max(0, effectB.Contrast + (effectA.Contrast * scale));

        effectB.RedLevel = Mathf.Max(0, effectB.RedLevel + (effectA.RedLevel * scale));
        effectB.GreenLevel = Mathf.Max(0, effectB.GreenLevel + (effectA.GreenLevel * scale));
        effectB.BlueLevel = Mathf.Max(0, effectB.BlueLevel + (effectA.BlueLevel * scale));

        if (effectA.LightenColor != Color.clear || effectA.LightenColor != Color.black)
            effectB.LightenColor = effectB.LightenColor + Color.Lerp(Color.clear, effectA.LightenColor, scale) / 2;
    }
    
    private void ApplyEffects(CameraEffect.EffectSettings.ColorSettings colorSettings)
    {
        _material.SetFloat("_Brightness", colorSettings.Brightness);
        _material.SetFloat("_Saturation", colorSettings.Saturation);
        _material.SetFloat("_Contrast", colorSettings.Contrast);
        _material.SetColor("_LightenColor", colorSettings.LightenColor);
        _material.SetFloat("_RedLevel", colorSettings.RedLevel);
        _material.SetFloat("_GreenLevel", colorSettings.GreenLevel);
        _material.SetFloat("_BlueLevel", colorSettings.BlueLevel);
    }

    public void SetBaseEffect(CameraEffect.EffectSettings.ColorSettings effectSettings)
    {
        _baseEffect = new CameraEffect.EffectSettings.ColorSettings(effectSettings);
    }

    public void AddEffect(CameraEffect cameraEffect)
    {
        if(cameraEffect != null)
            AddEffect(cameraEffect.Effect);
    }
    public void AddEffect(CameraEffect.EffectSettings effectSettings)
    {
        // Try to find an existing effect, and update its times
        foreach (CameraEffect.EffectSettings effect in _effects)
        {
            if (effect.UniqueID == effectSettings.UniqueID)
            {
                effect.Properties.StartTime = Time.time;
                effect.Properties.KillTime = Time.time + effect.Properties.Duration;
                return;
            }
        }

        // Create new instance of effect settings
        CameraEffect.EffectSettings newEffectSettings = new CameraEffect.EffectSettings(effectSettings);
        // Define start and kill times
        newEffectSettings.Properties.StartTime = Time.time;
        newEffectSettings.Properties.KillTime = Time.time + newEffectSettings.Properties.Duration;

        _effects.Add(newEffectSettings);
    }

    public void ClearAllEffects()
    {
        _effects.Clear();
    }
    
    #endregion
}