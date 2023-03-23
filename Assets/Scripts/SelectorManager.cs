using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectorManager : MonoBehaviour
{
    public List<GameObject> SelectedEntitys = new List<GameObject>();
    public List<GameObject> HighlightedEntitys = new List<GameObject>();
    public GameObject Selector, rectanglePrefab, scanner;
    private InputManager inputs;
    public Material nonTransparent, Tranparent;
    public bool negatenextSelected, choosing = false, hasChosen = false;
    private Vector3 firstpos, latestpos, sidepos1, sidepos2, firstMousePos;
    private GameObject rect, scan;
    private int mask;
    private Mesh customMesh;
    private GameObject lastPointedGameobject;
    public bool hasUnit;
    public ETypes structureType = ETypes.None;
    public static SelectorManager instance;

    private void Start()
    {
        if (instance == null)
            instance = this;
        inputs = gameObject.GetComponent<InputManager>();
        customMesh = new Mesh();
    }

    private void Update()
    {

        mask = LayerMask.GetMask("Ground");

        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        bool unimportantUi = true;

        if (raycastResults.Count > 0)
        {
            foreach (var go in raycastResults)
            {
                if (go.gameObject.layer != 6) //6 is here the value of Layer number 6 (Unimportant UI)
                    unimportantUi = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) && unimportantUi && !negatenextSelected)
        {
            hasUnit = false;
            hasChosen = false;
            UICommands.instance.ResetCommands();
            //clear all existing selected
            if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftShift)))
            {
                List<GameObject> kurz = new List<GameObject>();
                foreach (GameObject G in SelectedEntitys)
                {
                    if(lastPointedGameobject != G)
                        kurz.Add(G);
                }

                foreach (GameObject G in kurz)
                {
                    RemoveEntity(G);
                }
            }

            if(lastPointedGameobject != null)
            {
                Highlight(lastPointedGameobject);
                SelectEntity(lastPointedGameobject);
            }


            gameObject.GetComponent<Cam>().SetState(false);
            Ray R = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(R, out RaycastHit h, Mathf.Infinity, mask);
            firstpos = h.point;

            choosing = true;
            rect = Instantiate(rectanglePrefab, Input.mousePosition, Quaternion.identity, GameObject.Find("Canvas").transform);
            rect.GetComponent<DrawRect>().targetVector = Input.mousePosition;
            rect.GetComponent<DrawRect>().JustUpdateDude();

            firstMousePos = Input.mousePosition;

            scan = Instantiate(scanner, Vector3.zero, Quaternion.identity);
            //scan.GetComponent<MeshCollider>().sharedMesh = new Mesh();
        }
        else if (Input.GetKey(KeyCode.Mouse0) && choosing && !negatenextSelected)
        {
            Highlightallentitys();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0) && choosing && !negatenextSelected)
        {
            Highlightallentitys();
            // Select all highlighted Gameobjects

            customMesh = new Mesh();
            choosing = false;
            if(HighlightedEntitys.Count > 0)
                hasChosen = true;
            gameObject.GetComponent<Cam>().SetState(true);
            Destroy(rect);
            Destroy(scan);

            foreach (GameObject G in HighlightedEntitys)
            {
                SelectEntity(G);
            }
        }
        else if (negatenextSelected && Input.GetKeyUp(KeyCode.Mouse0))
            negatenextSelected = false;


        if (choosing)
        {
            Vector3 campos = Camera.main.gameObject.transform.position;
            Debug.DrawLine(campos, firstpos);
            Debug.DrawLine(campos, latestpos);
            Debug.DrawLine(campos, sidepos1);
            Debug.DrawLine(campos, sidepos2);
        }
        else
        {
            Ray R = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(R, out RaycastHit h, Mathf.Infinity, mask);
            //Debug.DrawLine(R.origin, h.point, Color.red);
        }

        if (!Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0))
            choosing = false;
    }

    public void Highlight(GameObject high, bool ignoreTags = false, bool ignoreTeam = false)
    {
        if(!ignoreTags)
        {
            if (high.tag == "Units")
            {
                hasUnit = true;


                foreach (GameObject G in HighlightedEntitys)
                {
                    if (G.tag == "Structures")
                        StartCoroutine(RemoveEntityDelayed(G));
                }
            }


            if (hasUnit && high.tag != "Units")
                return;

            if (structureType == ETypes.None && !hasUnit)
                structureType = high.GetComponent<Entity>().type;
            else if ((structureType != ETypes.None && !hasUnit) && (structureType != high.GetComponent<Entity>().type))
            {
                return;
            }
        }

        if((!(HighlightedEntitys.Contains(high) || SelectedEntitys.Contains(high))) && (Player.myPlayer.myTeam.Equals(high.GetComponent<Entity>().myTeam) || ignoreTeam))
        {
            //Debug.Log("highlight2");
            HighlightedEntitys.Add(high);
            float s = high.GetComponent<HealthScript>().scale;
            GameObject G = Instantiate(Selector, high.transform.position + new Vector3(0,0.2f,0), Quaternion.identity, high.transform);
            if (high.transform.Find("Selector Origin"))
                G.transform.position = high.transform.Find("Selector Origin").position;
            G.GetComponent<Selector>().UpdateScale(s);
            foreach (MeshRenderer R in G.GetComponentsInChildren<MeshRenderer>())
            {
                R.material = Tranparent;
            }
            if (!ignoreTags || !hasChosen)
            {
                SelectionUi.instance.AddUIElement(high.GetComponent<Entity>().UnitDisplay, high);
            }
        }
        
    }

    public void SelectEntity(GameObject Selected)
    {
        if (Selected != null && !SelectedEntitys.Contains(Selected) && HighlightedEntitys.Contains(Selected) && Player.myPlayer.myTeam.Equals(Selected.GetComponent<Entity>().myTeam))
        {
            //Debug.Log("Select");
            SelectedEntitys.Add(Selected);
            foreach (MeshRenderer R in Selected.transform.Find("Selector(Clone)").GetComponentsInChildren<MeshRenderer>())
            {
                R.material = nonTransparent;
            }

            // Acess the commands of this unit / structure
            Selected.GetComponent<Entity>().GiveCommands();
            Selected.GetComponent<Entity>().OnSelected();
        }
    }

    public void RemoveEntity(GameObject Selected)
    {
        if(Selected != null)
        {
            bool s = SelectedEntitys.Contains(Selected) || HighlightedEntitys.Contains(Selected);
            SelectedEntitys.Remove(Selected);
            HighlightedEntitys.Remove(Selected);
            if(s)
                Destroy(Selected.transform.Find("Selector(Clone)").gameObject);

            SelectionUi.instance.RemoveUIElement(Selected);

            if (hasUnit && HighlightedEntitys.Count == 0)
                hasUnit = false;

            if (!hasUnit && structureType != ETypes.None && SelectedEntitys.Count == 0)
                structureType = ETypes.None;

            Selected.GetComponent<Entity>().OnDeselected();
        }
    }

    IEnumerator RemoveEntityDelayed(GameObject Selected)
    {
        yield return 0;
        if (Selected != null)
        {
            //Debug.Log("RemoveEntity");
            bool s = SelectedEntitys.Contains(Selected) || HighlightedEntitys.Contains(Selected);
            //Debug.Log(SelectedEntitys.Contains(Selected) + " | " + HighlightedEntitys.Contains(Selected));
            //Debug.Log("Remove2");
            SelectedEntitys.Remove(Selected);
            HighlightedEntitys.Remove(Selected);
            if (s)
                Destroy(Selected.transform.Find("Selector(Clone)").gameObject);

            SelectionUi.instance.RemoveUIElement(Selected);

            if (hasUnit && HighlightedEntitys.Count == 0)
                hasUnit = false;

            if (!hasUnit && structureType != ETypes.None && SelectedEntitys.Count == 0)
                structureType = ETypes.None;
        }
    }

    private void Highlightallentitys()
    {
        Ray Ray1 = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(Ray1, out RaycastHit hit1, Mathf.Infinity, mask);
        latestpos = hit1.point;

        Ray Ray2 = Camera.main.ScreenPointToRay(firstMousePos + new Vector3(Input.mousePosition.x - firstMousePos.x, 0, 0));
        Physics.Raycast(Ray2, out RaycastHit hit2, Mathf.Infinity, mask);
        sidepos1 = hit2.point;

        Ray Ray3 = Camera.main.ScreenPointToRay(firstMousePos + new Vector3(0, Input.mousePosition.y - firstMousePos.y, 0));
        Physics.Raycast(Ray3, out RaycastHit hit3, Mathf.Infinity, mask);
        sidepos2 = hit3.point;

        Ray Ray4 = Camera.main.ScreenPointToRay(firstMousePos);
        Physics.Raycast(Ray4, out RaycastHit hit4, Mathf.Infinity, mask);
        firstpos = hit4.point;



        rect.GetComponent<DrawRect>().targetVector = Input.mousePosition;

        //customMesh.Clear();
        MeshCollider MCol = scan.GetComponent<MeshCollider>();
        MeshFilter meshfilt = scan.GetComponent<MeshFilter>();

        Vector3[] verts = { firstpos + new Vector3(Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f)),
                sidepos1 + new Vector3(Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f)),
                latestpos + new Vector3(Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f)),
                sidepos2 + new Vector3(Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f)),
                firstpos + new Vector3(0,10,0) + new Vector3(Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f)),
                sidepos1 + new Vector3(0,10,0) + new Vector3(Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f)),
                latestpos + new Vector3(0,10,0) + new Vector3(Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f)),
                sidepos2 + new Vector3(0,10,0) + new Vector3(Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f),Random.Range(-0.01f,0.01f))
            };


        int[] trias = {0,1,2 ,0,2,3, 4,5,6, 4,6,7, 0,1,4, 1,5,4,
                2,3,6, 6,3,7,
                1,2,5 ,5,2,6,
                3,0,7, 0,4,7
            };

        customMesh.vertices = verts;
        customMesh.triangles = trias;
        customMesh.RecalculateNormals();

        meshfilt.mesh = customMesh;
        MCol.sharedMesh = customMesh;
        foreach (GameObject G in scan.GetComponent<coliisions>().collision)
        {
            if (!HighlightedEntitys.Contains(G))
            {
                Highlight(G);
            }
        }
        List<GameObject> temporär = new List<GameObject>();
        foreach (GameObject F in HighlightedEntitys)
        {
            if (!scan.GetComponent<coliisions>().collision.Contains(F))
            {
                temporär.Add(F);
            }
        }
        foreach (GameObject item in temporär)
        {
            if (!SelectedEntitys.Contains(item))
                RemoveEntity(item);
        }
    }

    public void HighlightPointedObject(GameObject pointed)
    {
        if(lastPointedGameobject == null && pointed != null)
        {
            lastPointedGameobject = pointed;
            Highlight(pointed, true, true);
        }
        else if(lastPointedGameobject != pointed && pointed != null)
        {
            if(!SelectedEntitys.Contains(lastPointedGameobject))
            {
                RemoveEntity(lastPointedGameobject);
                Highlight(pointed, true, true);
            }
            lastPointedGameobject = pointed;
        }
        else if(pointed == null && lastPointedGameobject != null)
        {
            if (!SelectedEntitys.Contains(lastPointedGameobject))
            {
                //Debug.Log(lastPointedGameobject);
                RemoveEntity(lastPointedGameobject);
            }
            lastPointedGameobject = null;
        }
    }

    public void HighlightTargetOfMoveCommand(GameObject pointed)
    {
        Highlight(pointed,true);
    }

    public bool IncludesUnits()
    {
        foreach (GameObject G in HighlightedEntitys)
        {
            if (G.tag == "Units")
                return true;
        }
        return false;
    }
}
