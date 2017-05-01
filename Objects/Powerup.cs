using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Powerup : NetworkBehaviour
{
    #region Values
    public enum PowerupType
    {
        Switch,
        Heal,
        Spawn

    }
    /// <summary>
    /// Determines the powerups effect on an inanimate object.
    /// </summary>
    public PowerupType Type;

    /// <summary>
    /// Defines the speed in-which the powerup will rotate on each axis.
    /// </summary>
    public Vector3 RotationSpeed;
    #endregion

    #region Unity Functions
    private void Update()
    {
        UpdateRotation();
    }

    private void OnTriggerEnter(Collider collider)
    {
        // If the intersecting object is an inanimate object, apply
        InanimateObject inanimateObject = collider.GetComponent<InanimateObject>();
        if (inanimateObject != null && inanimateObject.Controlled && TestClientApply(inanimateObject))
        {
            if (!NetworkSessionManager.IsClient)
                Apply(inanimateObject);
            else
            {
                NetworkSessionNode.Instance.CmdRequestPowerup(this.gameObject, inanimateObject.gameObject, (int)Type);
            }
        }
    } 
    #endregion

    #region Functions

    public void Apply(InanimateObject inanimateObject)
    {
        inanimateObject.ApplyPowerup(Type);
        Destroy(this.gameObject);
    }

    public bool TestClientApply(InanimateObject inanimateObject)
    {
        switch (Type)
        {
            case Powerup.PowerupType.Heal:
                if (inanimateObject.Health == inanimateObject.MaxHealth)
                    return false;
                break;
        }
        return true;
    }
    private void UpdateRotation()
    {
        // Update the rotation of this powerup
        this.transform.eulerAngles += RotationSpeed * Time.deltaTime;
    } 
    #endregion
}
