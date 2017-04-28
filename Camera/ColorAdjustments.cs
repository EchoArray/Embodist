using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class ColorAdjustments : MonoBehaviour
{
    #region Values
    public Shader Shader;
    
    public float Brightness = 1.0f;
    public float Saturation = 1.0f;
    public float Contrast = 1.0f;

    public Color LightenColor;

    private Material material;
    public Material Material
    {
        get
        {
            if (material == null)
            {
                material = new Material(Shader);
                material.hideFlags = HideFlags.HideAndDontSave;
            }
            return material;
        }
    }
    #endregion

    // Use this for initialization
    private void Awake()
    {
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }
    }

    private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        if (Shader != null)
        {
            Material.SetFloat("_Brightness", Brightness);
            Material.SetFloat("_Saturation", Saturation);
            Material.SetFloat("_Contrast", Contrast);
            Material.SetColor("_LightenColor", LightenColor);
            Graphics.Blit(sourceTexture, destTexture, Material);
        }
        else
        {
            Graphics.Blit(sourceTexture, destTexture);
        }
    }

    private void OnDisable()
    {
        if (material)
        {
            DestroyImmediate(material);
        }
    }
}