using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIAnimationSet : MonoBehaviour
{
    #region Values
    [Serializable]
    public class Animation
    {
        /// <summary>
        /// Determines if the animation is currently playing.
        /// </summary>
        [HideInInspector]
        public bool Playing;

        /// <summary>
        /// Defines the name of the animation.
        /// </summary>
        public string Name;

        /// <summary>
        /// Determines if the animation is to play upon game object enable.
        /// </summary>
        [Space(15)]
        public bool PlayOnEnable;
        /// <summary>
        /// Determines if the animation is to repeat upon completion.
        /// </summary>
        public bool Loop;

        /// <summary>
        /// Determines if the animation is slave to Unity's Time.timeScale
        /// </summary>
        [Space(15)]
        public bool TimeScaleIndependent;

        /// <summary>
        /// Defines the duration in which the animation will wait to begin.
        /// </summary>
        [Space(15)]
        public float StartDelay;
        /// <summary>
        /// Defines the progress along the start delay.
        /// </summary>
        [HideInInspector]
        public float StartTime;
        /// <summary>
        /// Defines the time span of the animation.
        /// </summary>
        public float Duration;
        /// <summary>
        /// Defines the progress along the duration of the animation.
        /// </summary>
        [HideInInspector]
        public float Progress;
        

        /// <summary>
        /// Defines the action that is to be invoked upon animation completion.
        /// </summary>
        [Space(10)]
        [HideInInspector]
        public Action Action;

        public enum AnimationCompletionAction
        {
            None,
            Deactivate,
            Destroy
        }
        /// <summary>
        /// Defines a specific action that is to trigger upon completion.
        /// </summary>
        public AnimationCompletionAction CompletionAction;

        [Serializable]
        public class AnimationColor
        {
            /// <summary>
            /// Determines if the animation is to use color.
            /// </summary>
            public bool Use;
            /// <summary>
            /// Defines the color of the maskable graphic along the span of the animation.
            /// </summary>
            public Gradient Color;
        }
        [Space(15)]
        public AnimationColor Color;

        [Serializable]
        public class AnimationPosition
        {
            /// <summary>
            /// Determines if the animation is to use position.
            /// </summary>
            public bool Use;
            /// <summary>
            /// Defines the target position that the rect transform is to progress to over the span of the animation.
            /// </summary>
            public Vector3 Position;

            [Serializable]
            public class RectTransfromUtilityAnchor
            {
                public Vector2 Min;
                public Vector2 Max;
            }
            /// <summary>
            /// Defines the target anchor that the rect transform is to progress to over the span of the animation.
            /// </summary>
            public RectTransfromUtilityAnchor Anchor;
        }
        [Space(15)]
        public AnimationPosition Position;


        [Serializable]
        public class AnimationRotation
        {
            /// <summary>
            /// Determines if the animation is to use rotation.
            /// </summary>
            public bool Use;
            /// <summary>
            /// Determines the target rotation that the rect transform is to progress to over the duration of the animation.
            /// </summary>
            public Vector3 Rotation;
        }
        [Space(15)]
        public AnimationRotation Rotation;

        [Serializable]
        public class AnimationScale
        {
            /// <summary>
            /// Determines if the animation is to use scale.
            /// </summary>
            public bool Use;
            /// <summary>
            /// Determines if the scale is to be set to zero along the duration of the start delay.
            /// </summary>
            public bool StartAtZeroOnDelay;
            /// <summary>
            /// Determines if the X axis is to be multiplied by -1.
            /// </summary>
            public bool FlipX;
            /// <summary>
            /// Determines the X axis' scale of the rect transform over the progression of the animation.
            /// </summary>
            public AnimationCurve ScaleX;
            /// <summary>
            /// Determines if the Y axis is to be multiplied by -1.
            /// </summary>
            public bool FlipY;
            /// <summary>
            /// Determines the Y axis' scale of the rect transform over the progression of the animation.
            /// </summary>
            public AnimationCurve ScaleY;
            /// <summary>
            /// Determines if the Z axis is to be multiplied by -1.
            /// </summary>
            public bool FlipZ;
            /// <summary>
            /// Determines the Z axis' scale of the rect transform over the progression of the animation.
            /// </summary>
            public AnimationCurve ScaleZ;
        }
        [Space(15)]
        public AnimationScale Scale;
    }
    public Animation[] Animations;

    // Defines the rect transform component of the game object.
    private RectTransform _rectTransform;
    // Defines the maskable graphic component of the game object.
    private MaskableGraphic _maskableGraphic;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        PlayOnEnable();
    }
    
    private void LateUpdate()
    {
        StepAnimations();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        _rectTransform = this.GetComponent<RectTransform>();
        _maskableGraphic = this.transform.GetComponent<MaskableGraphic>();
    }

    public void PlayOnEnable()
    {
        // Find each animation that is set to play on enable and play it.
        foreach (Animation animation in Animations)
            if (animation.PlayOnEnable)
            {
                StopAll();
                Play(animation.Name, true);
            }
    }

    private void StepAnimations()
    {
        // Processes each animation.
        foreach (Animation animation in Animations)
        {
            if (!animation.Playing || (animation.StartTime > Time.time))
                continue;

            // Define the progress of the animation
            animation.Progress = Mathf.Min(animation.Progress + (animation.TimeScaleIndependent ? Time.unscaledDeltaTime : Time.deltaTime), animation.Duration);
            float time = animation.Progress / animation.Duration;

            // Step each aspect
            StepPosition(animation, time);
            StepRotation(animation, time);
            StepScale(animation, time);
            StepColor(animation, time);

            if (animation.Progress == animation.Duration)
            {
                // If the animation has reached its end, process actions, reset progress and loop if contextual
                ProcessActions(animation);

                animation.Progress = 0;
                if (!animation.Loop)
                    animation.Playing = false;
            }
        }
    }
    
    private void StepPosition(Animation animation, float time)
    {
        // Progresses the position of the animation.
        if (!animation.Position.Use)
            return;

        _rectTransform.anchorMin = Vector2.Lerp(_rectTransform.anchorMin, animation.Position.Anchor.Min, time);
        _rectTransform.anchorMax = Vector2.Lerp(_rectTransform.anchorMax, animation.Position.Anchor.Max, time);
        _rectTransform.anchoredPosition = Vector2.Lerp(_rectTransform.anchoredPosition, animation.Position.Position, time);
    }
    private void StepRotation(Animation animation, float time)
    {
        // Progresses the rotation of the animation.
        if (animation.Rotation.Use)
            _rectTransform.eulerAngles = animation.Rotation.Rotation * time;
    }
    private void StepScale(Animation animation, float time)
    {
        // Progresses the scale of the animation.
        if (!animation.Scale.Use)
            return;

        Vector3 scale = Vector3.zero;
        scale.x = animation.Scale.ScaleX.Evaluate(time) * (animation.Scale.FlipX ? -1 : 1);
        scale.y = animation.Scale.ScaleY.Evaluate(time) * (animation.Scale.FlipY ? -1 : 1);
        scale.z = animation.Scale.ScaleZ.Evaluate(time) * (animation.Scale.FlipZ ? -1 : 1);

        _rectTransform.localScale = scale;
    }
    private void StepColor(Animation animation, float time)
    {
        // Progresses the color of the animation.
        if (animation.Color.Use)
            _maskableGraphic.color = animation.Color.Color.Evaluate(time);
    }

    private void ProcessActions(Animation animation)
    {
        // Processes the defined actions upon completion of the animation.
        if (animation.Action != null)
            animation.Action.Invoke();

        switch (animation.CompletionAction)
        {
            case Animation.AnimationCompletionAction.Deactivate:
                this.gameObject.SetActive(false);
                break;
            case Animation.AnimationCompletionAction.Destroy:
                Destroy(this.gameObject);
                break;
        }
    }

    /// <summary>
    /// Plays a specified animation within the set.
    /// </summary>
    /// <param name="name"> The name of the animation that is to be played.</param>
    /// <param name="completionCallback"> The action that is to be called upon the completion of the animation.</param>
    public void Play(string name, bool reset = true, Action completionCallback = null)
    {
        Animation animation = Array.Find(Animations, p => p.Name == name);
        if (animation == null)
            return;

        if (!reset && animation.Playing)
            return;

        animation.Playing = true;
        animation.StartTime = Time.time + animation.StartDelay;
        animation.Progress = 0;
        animation.Action = completionCallback;
    }

    /// <summary>
    /// Pauses a specified animation within the set.
    /// </summary>
    /// <param name="name"> The name of the animation that is to be paused.</param>
    public void Pause(string name)
    {
        Animation animation = Array.Find(Animations, p => p.Name == name);
        animation.Playing = false;
    }
    /// <summary>
    /// Pauses each animation within the set.
    /// </summary>
    public void PauseAll()
    {
        foreach (Animation animation in Animations)
            animation.Playing = false;
    }

    /// <summary>
    /// Stops a specified animation within the set.
    /// </summary>
    /// <param name="name"> The name of the animation that is to be stopped.</param>
    public void Stop(string name)
    {
        Animation animation = Array.Find(Animations, p => p.Name == name);
        animation.Playing = false;
        animation.Progress = 0;
    }
    /// <summary>
    /// Stops each animation within the set.
    /// </summary>
    public void StopAll()
    {
        foreach (Animation animation in Animations)
        {
            animation.Playing = false;
            animation.Progress = 0;
        }
    }
    #endregion
}
