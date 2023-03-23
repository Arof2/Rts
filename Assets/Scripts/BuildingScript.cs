using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BuildingScript : NetworkBehaviour
{
    public GameObject sphere;
    private Grid gridManager;
    private RaycastHit hit;
    private Vector3Int LastPossibleBuildingPlace;
    private GameObject transparentBuildingObject;
    public int width, height;
    private Vector3 displacement;
    [SerializeField]
    private int teamNumber = -1;

    public Transform buildingParent;
    public bool canBuild, buildable;
    public bool statusBuildMode;
    public GameObject buildingObjectPrefab, transparentBuildingObjectPrefab;
    NetworkVariable<int> buildingCount = new NetworkVariable<int>(); //How many buildings have been placed on the server

    public static BuildingScript instance;

    private void Start()
    {
        if (instance == null)
            instance = this;

        gridManager = gameObject.GetComponent<Grid>();
    }

    //Requirement so that this script can set the team for each building
    public void SetTeam(int TeamNumber)
    {
        teamNumber = TeamNumber;
    }

    public override void OnNetworkSpawn()
    {
        buildingCount.Value = 0;
    }

    void Update()
    {
        if (statusBuildMode && !UiSettings.instance.inSettings)
        {
            if (transparentBuildingObject != null)
                transparentBuildingObject.SetActive(true);

            int mask = LayerMask.GetMask("Units", "Structures");
            Ray MouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(MouseRay, out hit, mask);
            ShowtransparentBuilding();
        }
        else if (UiSettings.instance.inSettings && transparentBuildingObject != null)
        {
            transparentBuildingObject.SetActive(false);
        }
    }

    public void ShowtransparentBuilding()
    {
        Vector3Int pos = new Vector3Int(Mathf.RoundToInt(hit.point.x), 0, Mathf.RoundToInt(hit.point.z));
        buildable = CanBuildHere(new Vector2(pos.x + displacement.x, pos.z + displacement.z));

        if (!canBuild)
        {
            canBuild = buildable;
        }

        if (buildable)
            LastPossibleBuildingPlace = pos;


        if (canBuild && transparentBuildingObject == null)
        {
            transparentBuildingObject = Instantiate(transparentBuildingObjectPrefab, LastPossibleBuildingPlace, Quaternion.identity, buildingParent);
        }
        else if (canBuild)
        {
            transparentBuildingObject.transform.position = LastPossibleBuildingPlace;
        }
    }

    private bool CanBuildHere(Vector2 inputCords)
    {
        InputManager manager = InputManager.instance;
        bool buildable = true;
        for (int i = 0; i < manager.allPlacedStructuresheight.Count; i++)
        {
            if (Overlaps(manager.allPlacedStructuresPositions[i],
                manager.allPlacedStructuresheight[i],
                manager.allPlacedStructureswidth[i],
                inputCords, height, width))
            {
                buildable = false;
                break;
            }
        }
        return buildable;
    }

    //Wenn true dann overalpt | Wenn false dann nicht overlapt
    public bool Overlaps(Vector2 incomingPos, int incomingheight, int incomingWidth, Vector2 originalPosition, int originalWidht, int originalHeight)
    {
        return originalPosition.x < incomingPos.x + incomingWidth &&
        originalPosition.x + originalWidht > incomingPos.x &&
        originalPosition.y < incomingPos.y + incomingheight &&
        originalHeight + originalPosition.y > incomingPos.y;
    }

    // This will be called in Input Manager during Build Mode
    public bool BuildStructure() //only happens on this client
    {
        if (canBuild)
        {
            Vector2 centeredPosition = new Vector2(LastPossibleBuildingPlace.x + displacement.x, LastPossibleBuildingPlace.z + displacement.z);
            RequestPlaceBuildingServerRpc(buildingObjectPrefab.GetComponent<Structur>().type, transparentBuildingObject.transform.position, centeredPosition, teamNumber);
            resetCanBuild();
            return true;
        }
        return false;
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void RequestPlaceBuildingServerRpc(ETypes prefab, Vector3 pos, Vector2 centerdPosition, int teamN)
    {
        //PlaceBuildingClientRpc(prefab, pos, centerdPosition);

        GameObject aimPrefab = null;
        foreach (StructuresTypes S in DefaultPrefabs.instance.allStructures)
        {
            if (S.tpye == prefab)
                aimPrefab = S.prefab;
        }

        if (aimPrefab != null)
        {
            GameObject G = Instantiate(aimPrefab, pos, Quaternion.identity, null);

            G.GetComponent<NetworkObject>().Spawn();
            string name = G.name + " " + buildingCount.Value;
            G.GetComponent<Structur>().SetNameClientRpc(name);
            buildingCount.Value++;

            G.transform.parent = buildingParent;

            SetPositionClientRpc(name, centerdPosition, teamN);

        }
        else
        {
            Debug.LogError("Server couldnt Spawn Prefab");
            Debug.LogError("no Prefab of Type: " + prefab + "found");
        }
    }

    [ClientRpc]
    public void SetPositionClientRpc(string name,Vector2 centerdPosition, int teamN)
    {
        GameObject G = GameObject.Find(name); ;

        Structur thisStructure = G.GetComponent<Structur>();
        thisStructure.myTeam = TeamsMananger.instance.GetTeam(teamN);
        thisStructure.position = centerdPosition;
        InputManager.instance.AddStructureValuesServerRpc(centerdPosition, thisStructure.height, thisStructure.width);
    }

    

    public void Reset()
    {
        buildingObjectPrefab = null;
        transparentBuildingObjectPrefab = null;
        if (transparentBuildingObject != null)
            Destroy(transparentBuildingObject);
        LastPossibleBuildingPlace = Vector3Int.zero;
        statusBuildMode = false;
        canBuild = false;
        displacement = Vector3.zero;
    }

    public void SetBuildings(GameObject newBuildObject, GameObject newBuildObjecttranparent, int newwidth, int newheight)
    {
        buildingObjectPrefab = newBuildObject;
        transparentBuildingObjectPrefab = newBuildObjecttranparent;
        statusBuildMode = true;
        width = newwidth;
        height = newheight;

        displacement = new Vector3(-width / 2, 0, -height / 2);
    }

    public void resetCanBuild()
    {
        canBuild = false;
        buildable = false;
        Destroy(transparentBuildingObject);
    }
}
