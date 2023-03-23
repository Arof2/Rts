using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Fps : MonoBehaviour
{
    public int delayBetweenUpdates = 100;
    private TextMeshProUGUI text;
    private List<float> lastFrames = new List<float>();
    private int count;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = (Mathf.Round(1f/Time.deltaTime)).ToString();
    }
    void Update()
    {
        lastFrames.Add(1f / Time.deltaTime);
        if (count > delayBetweenUpdates)
        {
            int average = 0;
            float added = 0;
            foreach (float item in lastFrames)
            {
                added += item;
            }
            average = Mathf.RoundToInt(added / delayBetweenUpdates);

            text.text = average.ToString();

            lastFrames.Clear();
            count = 0;
        }

        count++;
    }
}
