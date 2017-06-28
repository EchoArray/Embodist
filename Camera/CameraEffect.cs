using UnityEngine;
using System.Collections;
using System;

public class CameraEffect : MonoBehaviour
{
    #region Values
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

    [Serializable]
    public class EffectSettings
    {
        public int UniqueID;

        [Serializable]
        public class PropertySettings
        {
            /// <summary>
            /// Defines the radius of the effect.
            /// </summary>
            public float Radius;
            /// <summary>
            /// Defines the duration of the camera effect.
            /// </summary>
            public float Duration;
            /// <summary>
            /// Defines the time in-which the effect started.
            /// </summary>
            [HideInInspector]
            public float StartTime;
            /// <summary>
            /// Defines the time in-which the effect will end;
            /// </summary>
            [HideInInspector]
            public float KillTime;

            public PropertySettings() { }
            public PropertySettings(PropertySettings propertySettings)
            {
                Radius = propertySettings.Radius;
                Duration = propertySettings.Duration;
                StartTime = propertySettings.StartTime;
                KillTime = propertySettings.KillTime;
            }
        }
        public PropertySettings Properties;

        [Serializable]
        public class ColorSettings
        {
            /// <summary>
            /// Defines the brightness for the shader applied to the render texture for the camera.
            /// </summary>
            public float Brightness = 0f;
            /// <summary>
            /// Defines the saturation for the shader applied to the render texture for the camera.
            /// </summary>
            public float Saturation = 0f;
            /// <summary>
            /// Defines the contrast for the shader applied to the render texture for the camera.
            /// </summary>
            public float Contrast = 0f;

            /// <summary>
            /// Defines the red levels for the shader applied to the render texture for the camera.
            /// </summary>
            public float RedLevel;
            /// <summary>
            /// Defines the green levels for the shader applied to the render texture for the camera.
            /// </summary>
            public float GreenLevel;
            /// <summary>
            /// Defines the blue levels for the shader applied to the render texture for the camera.
            /// </summary>
            public float BlueLevel;

            /// <summary>
            /// Defines the lighten color for the shader applied to the render texture for the camera.
            /// </summary>
            public Color LightenColor;

            public ColorSettings() { }
            public ColorSettings(ColorSettings effectSettings)
            {
                
                Brightness = effectSettings.Brightness;
                Saturation = effectSettings.Saturation;
                Contrast = effectSettings.Contrast;
                LightenColor = effectSettings.LightenColor;

                RedLevel = effectSettings.RedLevel;
                GreenLevel = effectSettings.GreenLevel;
                BlueLevel = effectSettings.BlueLevel;
            }
        }
        public ColorSettings Colors;

        public EffectSettings() { }
        public EffectSettings(EffectSettings cameraEffect)
        {
            UniqueID = cameraEffect.UniqueID;
            Properties = cameraEffect.Properties;
            Colors = cameraEffect.Colors;
        }
    }
    public EffectSettings Effect;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        if (Type == ExecutionType.OnAwake)
            Cast();
    }
    #endregion

    #region Functions
    public void Cast()
    {
        // Loop through to each inanimate object
        for (int i = 0; i < Globals.Instance.Containers.InanimateObjects.childCount; i++)
        {
            // If the inanimate object is within the radius apply effects
            Transform transform = Globals.Instance.Containers.InanimateObjects.GetChild(i);
            float distance = Vector3.Distance(this.transform.position, transform.position);
            if (distance <= Effect.Properties.Radius)
            {
                InanimateObject inanimateObject = transform.GetComponent<InanimateObject>();
                if (inanimateObject != null)
                    Apply(inanimateObject);
            }
        }

        Destroy(this.gameObject);
    }

    public void Apply(InanimateObject inanimateObject)
    {
        if (inanimateObject.LocalPlayer != null)
        {
            CameraEffector cameraEffector = inanimateObject.LocalPlayer.CameraController.GetComponent<CameraEffector>();
            cameraEffector.AddEffect(Effect);
        }
    }
    #endregion

}