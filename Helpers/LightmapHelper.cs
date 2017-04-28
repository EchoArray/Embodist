using UnityEngine;
using System;

public class LightmapHelper
{
    public static Color GetColor(RaycastHit raycastHit)
    {
        if (raycastHit.collider != null)
        {
            // If the collider does not have a renderer, abort
            Renderer renderer = raycastHit.collider.gameObject.GetComponent<Renderer>();

            if (renderer != null || renderer.lightmapIndex < 253)
            {
                try
                {
                    // Define surface color by the pixel color at the UV coordinates, return color
                    LightmapData lightMapInfo = LightmapSettings.lightmaps[renderer.lightmapIndex];
                    Texture2D lightmapTexture = lightMapInfo.lightmapColor;
                    Color color = lightmapTexture.GetPixelBilinear(raycastHit.lightmapCoord.x, raycastHit.lightmapCoord.y);

                    Development.AddTimedSphereGizmo(color, 0.1f, raycastHit.point, 1);

                    return color;
                }
                catch (System.Exception e)
                {
                }
            }
        }
        return Color.white;
    }
    public static Color GetColor(Vector3 position, Vector3 direction)
    {
        RaycastHit raycastHit;
        bool hit = Physics.Raycast(position, direction, out raycastHit, Mathf.Infinity, 1 << Globals.STRUCTURE_LAYER);
        if (hit)
            return GetColor(raycastHit);

        return Color.white;
    }
}
