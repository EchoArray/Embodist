using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HeadsUpDisplayBeaconNode : MonoBehaviour
{
    public float LifeSpan;

    private void Awake()
    {
        Destroy(this.gameObject, LifeSpan);
    }

    public void Cast(int teamId)
    {
        string name = string.Empty;
        HUDManager.Instance.AddWaypointToTeamsHud(teamId, HeadsUpDisplay.Waypoint.WaypointType.Beacon, this.transform, Vector3.zero, name);
    }
}
