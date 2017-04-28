using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[DisallowMultipleComponent]
public class TriggerVolume : MonoBehaviour
{
    #region Values
    public string AreaName;
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
        public TriggerOn Condition;
        public TriggerAction Action;
        public GameObject PhysicalEffect;
        public TriggerVolume TeleportationDestination;
        [Serializable]
        public class Target
        {
            public GameObject GameObject;
            public string MethodCalled;
        }
        public Target[] Targets;
    }

    private Collider[] _boxColliders;
    private BoxCollider _boxCollider;
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
        // For each trigger event that is an exit or enter, apply appropriately.
        foreach (TriggerEvent triggerEvent in Events)
            if (triggerEvent.Condition == condition)
            {
                InanimateObject inanimateObject = null;
                if (AreaName != string.Empty)
                {
                    if (inanimateObject == null)
                        inanimateObject = gameObject.GetComponent<InanimateObject>();

                    if (inanimateObject != null && inanimateObject.LocalPlayer != null)
                        inanimateObject.LocalPlayer.AreaName = AreaName;
                }

                
                switch (triggerEvent.Action)
                {
                    case TriggerAction.Kill:
                        if(inanimateObject == null)
                            inanimateObject = gameObject.GetComponent<InanimateObject>();
                        if (inanimateObject != null)
                            inanimateObject.Kill(true);
                        break;
                    case TriggerAction.Destroy:
                        Destroy(gameObject);
                        break;
                    case TriggerAction.Teleport:
                        Vector3 offset = gameObject.transform.position - this.transform.position;
                        gameObject.transform.position = triggerEvent.TeleportationDestination.transform.position + offset;
                        triggerEvent.TeleportationDestination.TeleportationEnterIgnore = gameObject;

                        break;

                }
                if (triggerEvent.PhysicalEffect != null)
                {
                    PhysicalEffect physicalEffect = triggerEvent.PhysicalEffect.GetComponent<PhysicalEffect>();
                    if (physicalEffect != null)
                        physicalEffect.Affect(gameObject, this.transform, false);
                }
                if (triggerEvent.Targets.Length > 0)
                    foreach (TriggerEvent.Target target in triggerEvent.Targets)
                        if (target.MethodCalled != string.Empty)
                            target.GameObject.SendMessage(target.MethodCalled, gameObject, SendMessageOptions.DontRequireReceiver);
            }
    }

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
