using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[DisallowMultipleComponent]
public class TriggerVolume : MonoBehaviour
{
    #region Values
    /// <summary>
    /// Determines which layers the volume will ignore.
    /// </summary>
    public LayerMask IgnoredLayers;
    public TriggerEvent[] Events;


    /// <summary>
    /// Defines the game object teleported into the volume that is to be ignored on enter - to avoid teleportation send-back.
    /// </summary>
    [HideInInspector]
    public GameObject TeleportationEnterIgnore;

    [Serializable]
    public enum TriggerOn
    {
        OnEnter,
        OnStay,
        OnExit
    }
    [Serializable]
    public enum TriggerAction
    {
        None,
        Kill,
        Destroy,
        Teleport,
    }
    [Serializable]
    public class TriggerEvent
    {
        /// <summary>
        /// Defines the condition in-which the event is called.
        /// </summary>
        public TriggerOn Condition;
        /// <summary>
        /// Defines the Action upon triggering.
        /// </summary>
        public TriggerAction Action;
        /// <summary>
        /// Defines the physical effect applied to the intersecting objects.
        /// </summary>
        public GameObject PhysicalEffect;
        /// <summary>
        /// Defines the seconday trigger volume in-which an interseting object will be sent to.
        /// </summary>
        public TriggerVolume TeleportationDestination;

        [Serializable]
        public class Target
        {
            /// <summary>
            /// Defines the object in-which the method will be invoked upon.
            /// </summary>
            public GameObject GameObject;
            /// <summary>
            /// Defines the name of the method to be invoked.
            /// </summary>
            public string MethodCalled;
        }
        public Target[] Targets;
    }
    // A collection of the box collider components of this game object.
    private Collider[] _boxColliders;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {

    }

    private void OnDrawGizmos()
    {
        BoxCollider trigger = null;

        BoxCollider[] boxColliders = this.gameObject.GetComponents<BoxCollider>();
        foreach (BoxCollider boxCollider in boxColliders)
        {
            if (!boxCollider.isTrigger)
                continue;
            trigger = boxCollider;
            break;
        }

        if (trigger == null)
            return;

        // Show the decals size, position and direction
        Matrix4x4 matrixBackup = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(trigger.center, trigger.size);

        Color color = Color.cyan;
        color.a = 0.25f;
        Gizmos.color = color;
        Gizmos.DrawCube(trigger.center, trigger.size);

        Gizmos.matrix = matrixBackup;
    }
    private void OnDrawGizmosSelected()
    {
        BoxCollider trigger = null;
        BoxCollider[] boxColliders = this.gameObject.GetComponents<BoxCollider>();
        foreach (BoxCollider boxCollider in boxColliders)
        {
            if (!boxCollider.isTrigger)
                continue;
            trigger = boxCollider;
            break;
        }
        if (trigger == null)
            return;

        // Show the decals size, position and direction
        Matrix4x4 matrixBackup = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(trigger.center, trigger.size);

        Color color = Color.yellow;
        color.a = 0.75f;
        Gizmos.color = color;
        Gizmos.DrawCube(trigger.center, trigger.size);

        Gizmos.matrix = matrixBackup;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject == TeleportationEnterIgnore)
        {
            TeleportationEnterIgnore = null;
            return;
        }
        // If the object pertains to an ignored layer, abort
        if (IgnoredLayers == (IgnoredLayers | (1 << collider.gameObject.layer)))
            return;

        // Call triggers
        ProcessTriggers(collider.gameObject, TriggerOn.OnEnter);
    }
    private void OnTriggerStay(Collider collider)
    {
        // If the object pertains to an ignored layer, abort
        if (IgnoredLayers == (IgnoredLayers | (1 << collider.gameObject.layer)))
            return;

        // Call triggers
        ProcessTriggers(collider.gameObject, TriggerOn.OnStay);
    }
    private void OnTriggerExit(Collider collider)
    {
        // If the object pertains to an ignored layer, abort
        if (IgnoredLayers == (IgnoredLayers | (1 << collider.gameObject.layer)))
            return;

        // Call triggers
        if (!collider.GetComponent<Rigidbody>().isKinematic)
            ProcessTriggers(collider.gameObject, TriggerOn.OnExit);
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        _boxColliders = this.gameObject.GetComponents<BoxCollider>();
    }

    private void ProcessTriggers(GameObject gameObject, TriggerOn condition)
    {
        foreach (TriggerEvent triggerEvent in Events)
        {
            if (triggerEvent.Condition == condition)
            {
                // Apply default actions
                switch (triggerEvent.Action)
                {
                    case TriggerAction.Kill:
                        InanimateObject inanimateObject = null;
                        if (inanimateObject != null)
                            inanimateObject.Kill(true);
                        break;
                    case TriggerAction.Destroy:
                        Destroy(gameObject);
                        break;
                    case TriggerAction.Teleport:
                        TeleportGameObject(gameObject, triggerEvent.TeleportationDestination);
                        break;

                }

                // Apply physical effect
                if (triggerEvent.PhysicalEffect != null)
                {
                    PhysicalEffect physicalEffect = triggerEvent.PhysicalEffect.GetComponent<PhysicalEffect>();
                    if (physicalEffect != null)
                        physicalEffect.Affect(gameObject, this.transform, false);
                }

                // Method calls
                if (triggerEvent.Targets.Length > 0)
                {
                    foreach (TriggerEvent.Target target in triggerEvent.Targets)
                    {
                        if (target.MethodCalled != string.Empty)
                            target.GameObject.SendMessage(target.MethodCalled, gameObject, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Teleports a game object to another volume.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="destinationVolume"></param>
    public void TeleportGameObject(GameObject gameObject, TriggerVolume destinationVolume)
    {
        destinationVolume.TeleportationEnterIgnore = gameObject;
        Vector3 offset = gameObject.transform.position - this.transform.position;
        gameObject.transform.position = destinationVolume.transform.position + offset;
    }

    /// <summary>
    /// Determines if a vector3 point is within the bounds of any trigger box collider attached to this volume.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool BoxContainsPoint(Vector3 point)
    {
        foreach (BoxCollider boxCollider in _boxColliders)
        {
            if (!boxCollider.isTrigger)
                continue;

            point = this.transform.InverseTransformPoint(point) - boxCollider.center;

            float halfX = (boxCollider.size.x * 0.5f);
            float halfY = (boxCollider.size.y * 0.5f);
            float halfZ = (boxCollider.size.z * 0.5f);
            if (point.x < halfX && point.x > -halfX &&
               point.y < halfY && point.y > -halfY &&
               point.z < halfZ && point.z > -halfZ)
                return true;
        }
        return false;
    }
    #endregion
}
