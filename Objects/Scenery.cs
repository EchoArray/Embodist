using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scenery : MonoBehaviour
{
    #region Values
    // Defines the instantiated material of the renderer.
    private Material _material;
    // Defines the renderer component of the game object.
    private Renderer _renderer;

    /// <summary>
    /// Defines the light map color of the scenery object.
    /// </summary>
    public Color LightmapColor;
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
        _renderer = this.gameObject.GetComponent<Renderer>();
        _material = new Material(_renderer.material);
        _renderer.material = _material;

        UpdateMaterialLighting();
    }

    private void UpdateMaterialLighting()
    {
        LightmapColor = LightmapHelper.GetColor(this.transform.position, Vector3.down);
        _material.SetColor("_LightmapColor", LightmapColor);
    } 
    #endregion
}
