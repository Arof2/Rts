using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(HealthScript))]
public class Structur : Entity
{
    public GridPoint myPoint;
    public GameObject deathGroundEffect;
    public Vector2 position;
    public int height = 2;
    public int width = 2;

    public override void Start()
    {
        base.Start();
    }

    [ClientRpc]
    public override void DestroyClientRpc()
    {
        if(deathGroundEffect != null)
            Instantiate(deathGroundEffect, new Vector3(0 + position.x + width / 2, 0, 0 + position.y + height / 2), Quaternion.identity, transform.parent);
        InputManager.instance.RemoveStructureServerRpc(position);
        base.DestroyClientRpc();
    }

    public void ShowHitbox()
    {
        Vector3 originalPos = new Vector3(position.x, 0, position.y);
        Debug.DrawLine(originalPos, originalPos + new Vector3(0, 5, 0));
        Debug.DrawLine(originalPos + new Vector3(width, 0, 0), originalPos + new Vector3(width, 5, 0));
        Debug.DrawLine(originalPos + new Vector3(width, 0, width), originalPos + new Vector3(width, 5, width));
        Debug.DrawLine(originalPos + new Vector3(0, 0, width), originalPos + new Vector3(0, 5, width));
        Debug.DrawLine(originalPos + new Vector3(width, 5, 0), originalPos + new Vector3(width, 5, width));
        Debug.DrawLine(originalPos + new Vector3(0, 5, width), originalPos + new Vector3(width, 5, width));
        Debug.DrawLine(originalPos + new Vector3(width, 5, 0), originalPos + new Vector3(0, 5, 0));
        Debug.DrawLine(originalPos + new Vector3(0, 5, width), originalPos + new Vector3(0, 5, 0));
    }
}
