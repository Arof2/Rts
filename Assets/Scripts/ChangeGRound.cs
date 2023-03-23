using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ChangeGRound : MonoBehaviour
{
    public GameObject effekt;
    public float range = 10;
    [Range(0.1f, 1.5f)]
    public float rangeMultiplierGrass = 0.6f;
    [Range(0.1f, 5f)]
    public float speedModifier = 1;
    
    public bool StructDeathEffect = false;
    public Gradient colorRange;
    public Color mixColorfirst, mixColorsecond;
    private Color playercolor;



    private bool firstDone = false, secondDone = false;
    private List<Vector2> inOrderTris = new List<Vector2>();
    private List<int> inOrderGrass = new List<int>();
    private GroundGeneration generator = GroundGeneration.instance;

    [Header("Wahrscheinlichkeiten")]
    [Range(0f, 1f)]
    [Tooltip("1 = 0% | 0 = 100%")]
    public float particleChance = 0.85f;
    [Range(0f, 1f)]
    public float chancetoSpawn = 1;
    [Range(0f, 1f)]
    public float chanceForTeamColor = 0.05f;

    public bool customOrigin = false;
    public Transform newOrigin;



    public void Start()
    {
        /*if(!StructDeathEffect)
        {
            colorRange = new Gradient();

            playercolor = GetComponent<Entity>().myTeam.currentColor;

            GradientColorKey[] colorkey = new GradientColorKey[2];
            //colorkey[0].color = (mixColorfirst * playerColorAnteil + playercolor) / (1+ playerColorAnteil);
            colorkey[0].color = mixColorfirst;
            colorkey[0].time = 0.0f;
            //colorkey[1].color = (mixColorsecond * playerColorAnteil + playercolor) / (1 + playerColorAnteil);
            colorkey[1].color = mixColorsecond;
            colorkey[1].time = 1.0f;

            GradientAlphaKey[] alphakey = new GradientAlphaKey[2];
            alphakey[0].alpha = 1.0f;
            alphakey[0].time = 0.0f;
            alphakey[1].alpha = 1.0f;
            alphakey[1].time = 1.0f;

            colorRange.SetKeys(colorkey, alphakey);
        }*/

        //Destroy(this, 7.5f);
        StartCoroutine(DelayedMeshUpdate());
        int size = generator.size;
        Vector3 pos;
        if (customOrigin)
            pos = newOrigin.position;
        else
            pos = transform.position;

        List<Vector2> inRangeTris = new List<Vector2>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (Vector3.Distance(generator.pixelpos[x, y], pos) < range)
                {
                    inRangeTris.Add(new Vector2(x, y));
                }
            }
        }

        List<int> inRangeGrass = new List<int>();

        for (int i = 0; i < generator.meadowPos.Length; i++)
        {
            if (Vector3.Distance(generator.meadowPos[i], pos) < range * rangeMultiplierGrass)
            {
                inRangeGrass.Add(i);
            }
        }


        inOrderGrass = inRangeGrass.OrderBy(V => Vector3.Distance(transform.position, generator.meadowPos[V])).ToList();

        inOrderTris = inRangeTris.OrderBy(V => Vector3.Distance(transform.position, generator.pixelpos[(int)V.x, (int)V.y])).ToList();

        StartCoroutine(StartEvolutionTris("fore"));
        StartCoroutine(StartEvolutionTris("back"));
        StartCoroutine(DestroyNature());

    }

    IEnumerator StartEvolutionTris(string layer)
    {
        int i = 0;
        foreach (Vector2 V in inOrderTris)
        {
            if (chancetoSpawn >= (float)Random.Range(0f, 1f))
            {
                Vector3 triPos = generator.pixelpos[(int)V.x, (int)V.y];
                Color currentColor = generator.GetPixelColor(V, layer);
                Color newColor;
                if (Random.Range(0f, 1f) < chanceForTeamColor && !StructDeathEffect)
                {
                    Color mixmixcolor = (colorRange.Evaluate(1) + colorRange.Evaluate(0)) / 2;
                    newColor = Color.Lerp(currentColor,(Player.myPlayer.myTeam.currentColor + mixmixcolor * 2) / 3, Mathf.Abs(Vector3.Distance(transform.position, triPos) / range - 1));
                }
                else
                    newColor = Color.Lerp(currentColor, colorRange.Evaluate(Random.Range(0, 1f)), Mathf.Abs(Vector3.Distance(transform.position, triPos) / range - 1));


                generator.ChangePixel(V, newColor, layer);
                if (Random.Range(0f, 1f) > particleChance)
                {
                    GameObject G = Instantiate(effekt, triPos + new Vector3(Random.Range(0.05f, 0.25f), Random.Range(0.05f, 0.25f), Random.Range(0.05f, 0.25f)), Quaternion.identity, transform);
                    ParticleSystem.MainModule P = G.GetComponent<ParticleSystem>().main;
                    G.transform.rotation = Quaternion.Euler(90, 0, 0);
                    G.transform.localPosition += new Vector3(0, 0.4f, 0);
                    P.startColor = newColor;
                    Destroy(G, P.duration);
                }


                if (i > 5 || StructDeathEffect)
                {
                    yield return new WaitForSeconds(speedModifier * 0.02f);
                    i = 0;
                }
                else
                    i++;
            }
        }

        firstDone = true;
        if (secondDone && StructDeathEffect)
        {
            Destroy(gameObject, 1);
        }

    }

    IEnumerator DestroyNature()
    {
        foreach (int I in inOrderGrass)
        {
            if (generator.meadowObjects[I] != null)
            {
                generator.meadowObjects[I].GetComponent<DestroyOnCollision>().Selfdestruct();
                yield return new WaitForSeconds(speedModifier * 0.02f);
            }
        }
        secondDone = true;
        if (firstDone && StructDeathEffect)
        {
            Destroy(gameObject, 1);
        }

    }

    IEnumerator DelayedMeshUpdate()
    {
        yield return 0;
        InputManager.instance.UpdateMesh();
    }
}
