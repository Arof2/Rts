using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct BackgroundMaterial
{
    public Color c1, c2, c3, c4;
    public Vector2 offset;
    public float scale;
}

public class ChangeBackgroundLobby : MonoBehaviour
{
    public float speed = 3.5f;
    public int frames = 720;

    public Image backgroundImage;
    public static ChangeBackgroundLobby instance;
    private Color color1, color2, color3, color4;
    private Vector2 offset;
    private float imageScale;

    public Color defcolor1, defcolor2, defcolor3, defcolor4;
    public Vector2 defoffset;
    public float defimageScale;

    [SerializeField]
    public List<BackgroundMaterial> allBackgrounds;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        RestoreDefault();
    }

    private void RestoreDefault()
    {
        Material shader = backgroundImage.material;
        shader.SetColor("_Color_1", defcolor1);
        shader.SetColor("_Color_2", defcolor2);
        shader.SetColor("_Color_3", defcolor3);
        shader.SetColor("_Color_4", defcolor4);
        shader.SetVector("_Offset", defoffset);
        shader.SetFloat("_scale", defimageScale);

        color1 = defcolor1;
        color2 = defcolor2;
        color3 = defcolor3;
        color4 = defcolor4;
        offset = defoffset;
        imageScale = defimageScale;
    }

    private void OnDestroy()
    {
        RestoreDefault();
    }

    public void ChangeTo(int index)
    {
        // 0 = MainMenu | 1 = Host Screen | 2 = Lobby Screen | 3 = PasswordScreen | 4 = inLobby Screen | 5 = MultiplayerLobby
        Color[] newArray = new Color[4];
        newArray[0] = allBackgrounds[index].c1;
        newArray[1] = allBackgrounds[index].c2;
        newArray[2] = allBackgrounds[index].c3;
        newArray[3] = allBackgrounds[index].c4;
        StopAllCoroutines();
        StartCoroutine(ChangeBackground(newArray, allBackgrounds[index].offset, allBackgrounds[index].scale));
        
    }

    IEnumerator ChangeBackground(Color[] allColors, Vector2 newOffset, float newImageScale)
    {
        if (allColors.Length == 4)
        {
            Material shader = backgroundImage.material;
            for (int i = 0; i < frames; i++)
            {
                shader.SetColor("_Color_1", Color.Lerp(shader.GetColor("_Color_1"), allColors[0], speed * Time.deltaTime));
                shader.SetColor("_Color_2", Color.Lerp(shader.GetColor("_Color_2"), allColors[1], speed * Time.deltaTime));
                shader.SetColor("_Color_3", Color.Lerp(shader.GetColor("_Color_3"), allColors[2], speed * Time.deltaTime));
                shader.SetColor("_Color_4", Color.Lerp(shader.GetColor("_Color_4"), allColors[3], speed * Time.deltaTime));
                shader.SetVector("_Offset", Vector2.Lerp(shader.GetVector("_Offset"), newOffset, speed * Time.deltaTime));
                shader.SetFloat("_scale", Mathf.Lerp(shader.GetFloat("_scale"), newImageScale, speed * Time.deltaTime));
                yield return 0;
            }

            shader.SetColor("_Color_1", allColors[0]);
            shader.SetColor("_Color_2", allColors[1]);
            shader.SetColor("_Color_3", allColors[2]);
            shader.SetColor("_Color_4", allColors[3]);
            shader.SetVector("_Offset", newOffset);
            shader.SetFloat("_scale", newImageScale);
        }
    }
}
