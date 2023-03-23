using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum InteractionMode
{
    Selecting,
    Building,
    Escape
}

public class InputManager : NetworkBehaviour
{
    private Camera cam;
    private Grid gridManager;
    private SelectorManager selector;
    private SelectionUi UISelector;
    private RaycastHit hit, hitGround;
    private BuildingScript buildscript;
    //private bool buildMode;
    private float lastBuildDataID = 0;
    public GameObject LastEventObject, nullObject, LastPointedObject, Arrows;
    public InteractionMode currentIntMode = InteractionMode.Selecting;
    private InteractionMode lastIntMode;

    public NetworkVariable<int> unitCount = new NetworkVariable<int>();

    private List<GameObject> SelectedOnUI = new List<GameObject>();
    public NavMeshSurface[] allNavs;

    public NetworkList<Vector2> allPlacedStructuresPositions;
    public NetworkList<int> allPlacedStructuresheight;
    public NetworkList<int> allPlacedStructureswidth;


    public static InputManager instance;
    private NetworkVariableWritePermission permission;

    //Debug bools
    public bool structureHitboxes;

    void Awake()
    {
        if (instance == null)
            instance = this;
        cam = Camera.main;
        gridManager = gameObject.GetComponent<Grid>();
        buildscript = gameObject.GetComponent<BuildingScript>();
        selector = gameObject.GetComponent<SelectorManager>();
        UISelector = gameObject.GetComponent<SelectionUi>();
        LastPointedObject = nullObject;

        unitCount.Value = 0;

        //these variables can only be changed by the server
        permission = IsServer ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner; //Doesnt fucking work

        allPlacedStructuresPositions = new NetworkList<Vector2>();
        allPlacedStructuresheight = new NetworkList<int>();
        allPlacedStructureswidth = new NetworkList<int>();

    }

    [ServerRpc(RequireOwnership = false)]
    public void AddStructureValuesServerRpc(Vector2 pos, int hei, int wid)
    {
        allPlacedStructuresPositions.Add(pos);
        allPlacedStructuresheight.Add(hei);
        allPlacedStructureswidth.Add(wid);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveStructureServerRpc(Vector2 pos)
    {
        int posInList = -1;
        for (int i = 0; i < allPlacedStructuresPositions.Count; i++)
        {
            if (allPlacedStructuresPositions[i] == pos)
            {
                posInList = i;
            }
        }

        allPlacedStructuresPositions.RemoveAt(posInList);
        allPlacedStructuresheight.RemoveAt(posInList);
        allPlacedStructureswidth.RemoveAt(posInList);
    }

    public bool GetBuildMode()
    {
        if (currentIntMode == InteractionMode.Building)
            return true;
        return false;
    }

    void Update()
    {
        if (UiSettings.instance.inSettings && currentIntMode != InteractionMode.Escape)
        {
            lastIntMode = currentIntMode;
            currentIntMode = InteractionMode.Escape;
        }
        else if (!UiSettings.instance.inSettings && currentIntMode == InteractionMode.Escape)
        {
            currentIntMode = lastIntMode;
        }

        int maskGround = LayerMask.GetMask("Ground");

        Ray MouseRay = cam.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(MouseRay, out hit, Mathf.Infinity);

        Ray MouseRayToGround = cam.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(MouseRay, out hitGround, Mathf.Infinity, maskGround);

        //Debug Commands
        if(Input.GetKey(KeyCode.LeftControl))
        {
            if(Input.GetKeyDown(KeyCode.H))
            {
                //Show Structure Hitboxes
                structureHitboxes = ToggleBool(structureHitboxes);
            }
        }

        if(structureHitboxes)
        {
            foreach (Structur G in GameObject.FindObjectsOfType<Structur>())
            {
                //Show Structure Hitboxes
                G.ShowHitbox();
            }
        }

        if (currentIntMode == InteractionMode.Selecting)
        {
            if (LastPointedObject == null && hit.collider.gameObject != null)
                LastPointedObject = hit.collider.gameObject;
            else if (hit.collider != null && LastPointedObject != hit.collider.gameObject)
                LastPointedObject = hit.collider.gameObject;

            if ((LastPointedObject.tag == "Units" || LastPointedObject.tag == "Structures") && (!Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse0) && !Input.GetKeyUp(KeyCode.Mouse0)))
                selector.HighlightPointedObject(LastPointedObject);
            else if (!(LastPointedObject.tag == "Units" || LastPointedObject.tag == "Structures") && (!Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse0) && !Input.GetKeyUp(KeyCode.Mouse0)))
                selector.HighlightPointedObject(null);

            if (Input.GetKeyDown(KeyCode.Mouse1))
                EventSystem.current.SetSelectedGameObject(LastEventObject);

            //Units moven
            if (selector.hasUnit && selector.SelectedEntitys.Count != 0 && Input.GetKeyDown(KeyCode.Mouse1))
            {
                MoveUnits(hit.collider.gameObject, hitGround.point);
            }
        }


        if (currentIntMode == InteractionMode.Building)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
                DeactivateBuildMode();

            if ((Input.GetKeyDown(KeyCode.Mouse0)) && currentIntMode == InteractionMode.Building && EventSystem.current.IsPointerOverGameObject() && EventSystem.current.currentSelectedGameObject == null)
                DeactivateBuildMode();

            if (Input.GetKeyDown(KeyCode.Mouse0) && currentIntMode == InteractionMode.Building && !EventSystem.current.IsPointerOverGameObject())
            {
                if (buildscript.BuildStructure() != false && Input.GetKey(KeyCode.LeftShift) == false)
                {
                    DeactivateBuildMode();
                    selector.negatenextSelected = true;

                }
                else if (buildscript.BuildStructure() != false && Input.GetKey(KeyCode.LeftShift) == true)
                {
                }
                else
                {
                    Debug.Log("Reset pointedobject AHHH");
                    EventSystem.current.SetSelectedGameObject(LastEventObject);
                }
                //Rebake the mesh so the placed building cant be overrun
                UpdateMesh();
            }

            if (Input.anyKey && !Input.GetKey(KeyCode.Mouse1) && (Input.GetKeyDown(KeyCode.Mouse0) && EventSystem.current.IsPointerOverGameObject()))
            {
                EventSystem.current.SetSelectedGameObject(LastPointedObject);
                Debug.Log("Reset pointedobject");
            }




            LastEventObject = EventSystem.current.currentSelectedGameObject;
        }


        //Debug
        //gridManager.DrawGrid();
        Debug.DrawLine(MouseRay.origin, hit.point);
        //Debug.DrawLine(MouseRay.origin, hitGround.point,Color.red);
    }

    public void ActivatedBuildMode(GameObject buildObject, GameObject tranparentBuildObject, int widht, int height, float ID)
    {
        if (!(currentIntMode == InteractionMode.Building))
        {
            //Debug.Log("new one");
            buildscript.SetBuildings(buildObject, tranparentBuildObject, widht, height);
            currentIntMode = InteractionMode.Building;
        }
        else if (lastBuildDataID == ID)
        {
            //Debug.Log("same one");
            DeactivateBuildMode();
            EventSystem.current.currentSelectedGameObject.GetComponent<Animator>().SetTrigger("Reset");
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            //Debug.Log("other one");
            DeactivateBuildMode();
            buildscript.SetBuildings(buildObject, tranparentBuildObject, widht, height);
            currentIntMode = InteractionMode.Building;
        }
        lastBuildDataID = ID;
    }

    public void DeactivateBuildMode()
    {
        buildscript.Reset();
        currentIntMode = InteractionMode.Selecting;
    }

    public void ToggleGameobject(GameObject Object)
    {
        if (Object.activeSelf)
            Object.SetActive(false);
        else
            Object.SetActive(true);
    }

    public void MoveUnits(GameObject hitGameobject, Vector3 hitCoords)
    {
        float targetRadius;
        Vector3 targetPosition;
        if (hitGameobject.GetComponent<Unit>() != null)
        {
            // Follow Unit
            bool hostile = hitGameobject.GetComponent<Entity>().myTeam != Player.myPlayer.myTeam;

            foreach (GameObject G in selector.SelectedEntitys)
            {
                Unit U = G.GetComponent<Unit>();
                U.GoToServerRpc(hitGameobject.name);
            }

            selector.HighlightTargetOfMoveCommand(hitGameobject);

            Color SelectorColor;
            if(hostile)
                SelectorColor = Color.red;
            else
                SelectorColor = Color.yellow;

            StartCoroutine(ChangeMaterialColor(hitGameobject.transform.Find("Selector(Clone)").GetComponentsInChildren<MeshRenderer>(), SelectorColor, 0.5f));
            return;
        }
        else if (hitGameobject.GetComponent<Structur>() != null)
        {
            //Goto Structure
            bool hostile = hitGameobject.GetComponent<Entity>().myTeam != Player.myPlayer.myTeam;

            foreach (GameObject G in selector.SelectedEntitys)
            {
                Unit U = G.GetComponent<Unit>();
                U.GoToServerRpc(hitGameobject.name);
            }

            
            targetPosition = new Vector3(hitGameobject.transform.position.x, 0, hitGameobject.transform.position.z);
            targetRadius = hitGameobject.GetComponent<Entity>().pathingRadius;

            selector.HighlightTargetOfMoveCommand(hitGameobject);

            Color SelectorColor;
            if (hostile)
                SelectorColor = Color.red;
            else
                SelectorColor = Color.yellow;

            StartCoroutine(ChangeMaterialColor(hitGameobject.transform.Find("Selector(Clone)").GetComponentsInChildren<MeshRenderer>(), SelectorColor, 0.5f));
        }
        else
        {
            // Boden oder so getroffen
            // hier noch ein Indicator für die Position
            targetPosition = new Vector3(hitCoords.x, 0, hitCoords.z);
            targetRadius = 0;
            Instantiate(Arrows, targetPosition + new Vector3(0, 0.5f, 0), Quaternion.identity, transform);

            foreach (GameObject G in selector.SelectedEntitys)
            {
                Unit U = G.GetComponent<Unit>();
                U.GoToServerRpc(targetPosition);
            }
        }
    }

    IEnumerator ChangeMaterialColor(MeshRenderer[] targetMaterials, Color targetColor, float durationInSec = 1)
    {
        Color startColor = targetMaterials[0].material.GetColor("_EmissionColor");
        float lerpValue = 1 / (durationInSec * 10);
        targetColor *= 6;

        foreach (MeshRenderer R in targetMaterials)
        {
            R.material.SetColor("_EmissionColor", targetColor);
            R.material.EnableKeyword("_EMISSION");
        }
        yield return 0;
        while (durationInSec > 0)
        {
            if (targetMaterials[0] == null)
                break;
            foreach (MeshRenderer R in targetMaterials)
            {
                R.material.SetColor("_EmissionColor", Color.Lerp(R.material.GetColor("_EmissionColor"), startColor, lerpValue));
                R.material.EnableKeyword("_EMISSION");
            }
            durationInSec -= 0.1f;
            yield return new WaitForSeconds(0.1f);

        }

        // Be sure that the end color is the same as it was before
        if (targetMaterials[0] != null)
        {
            foreach (MeshRenderer R in targetMaterials)
            {
                R.material.SetColor("_EmissionColor", startColor);
                R.material.EnableKeyword("_EMISSION");
            }
        }


    }

    public void UpdateMesh()
    {
        foreach (NavMeshSurface surf in allNavs)
        {
            surf.BuildNavMesh();
        }
    }

    public bool ToggleBool(bool inputBool)
    {
        if (inputBool)
            return false;
        else
            return true;
    }
}