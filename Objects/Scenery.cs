using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scenery : MonoBehaviour
{
    // Defines the instantiated material of the renderer.
    private Material _material;
    // Defines the renderer component of the game object.
    private Renderer _renderer;

    private void Awake()
    {
        Initialize();
    }

	private void Initialize()
    {
        _renderer = this.gameObject.GetComponent<Renderer>();
        _material = new Material(_renderer.material);
        _renderer.material = _material;

        UpdateMaterialLighting();
	}
    public Color LightmapColor;
    private void UpdateMaterialLighting()
    {
        // Cast a ray downward in order to define the renderer and retrieve the light map uv position, index and light map info
        // Set the materials light map color to that of the pixel at the uv position

        LightmapColor = LightmapHelper.GetColor(this.transform.position, Vector3.down);

        _material.SetColor("_LightmapColor", LightmapColor);
    }
}
