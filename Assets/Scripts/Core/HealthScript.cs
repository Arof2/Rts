using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HealthScript : NetworkBehaviour
{
    private float health;
    public float scale, maxHealth, armour;
    [SerializeField]
    private GameObject greenBar, yellowBar;
    private Entity myEntity;
    private IEnumerator lastCoroutine;

    private void Awake()
    {
        health = maxHealth;
        myEntity = GetComponent<Entity>();

        greenBar.transform.parent.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            ChangeHealthServerRpc(5);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ChangeHealthServerRpc(-5);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeHealthServerRpc(float amount)
    {
        ChangeHealthClientRpc(amount);
    }

    [ClientRpc]
    private void ChangeHealthClientRpc(float amount)
    {
        greenBar.transform.parent.gameObject.SetActive(true);

        health += amount;
        if (health > maxHealth)
            health = maxHealth;
        else if (health < 0)
            health = 0;
        float scale = health / maxHealth;
        greenBar.GetComponent<RectTransform>().localScale = new Vector3(scale,1,1);
        if(lastCoroutine != null)
            StopCoroutine(lastCoroutine);
        lastCoroutine = moveHealthbar(yellowBar, scale, 400);
        StartCoroutine(lastCoroutine);

        if(health <= 0)
        {
            // Entity should die
            myEntity.Destroy();
        }
        else
        {
            // Entity should live
            
        }
    }

    IEnumerator moveHealthbar(GameObject Healthbar, float targetscale, float frames)
    {
        RectTransform R = Healthbar.GetComponent<RectTransform>();
        for (int i = 1; i < frames; i++)
        {
            R.localScale = new Vector3(Mathf.Lerp(R.localScale.x, targetscale, Time.deltaTime * Mathf.Pow((i/(frames * 0.4f)),2)),1,1);
            yield return 0;
        }
        R.localScale = new Vector3(targetscale, 1, 1);

        if(health == maxHealth)
            greenBar.transform.parent.gameObject.SetActive(false);
    }

    public float getHealth()
    {
        return health;
    }
}
