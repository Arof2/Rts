using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICommands : MonoBehaviour
{
    public GameObject holder, holderUi;
    public List<CommandComponents> commands = new List<CommandComponents>();
    private Dictionary<string, KeyCode> commandShortcuts = new Dictionary<string, KeyCode>();
    public static UICommands instance;

    public void Start()
    {
        if(UICommands.instance == null)
            instance = this;

        for (int i = 0; i < commands.Count; i++)
        {
            commandShortcuts.Add(commands[i].commandNames, commands[i].commandKeys);
        }
    }

    private void Update()
    {
        if (holder.transform.childCount != 0)
            holderUi.SetActive(true);
        else
            holderUi.SetActive(false);

    }
    
    public Button AddCommand(string commandname)
    {
        for (int i = 0; i < commands.Count; i++)
        {
            if(commands[i].commandNames == commandname)
            {
                if(!commands[i].commandsActive)
                {
                    // Command doesnt exist yet
                    // Instantiate Command Prefab
                    GameObject com = Instantiate(commands[i].commandsPrefabs, Vector3.zero, Quaternion.identity, holder.transform);
                    commands[i].commandsActive = true;
                    Button butt = com.GetComponent<Button>();
                    
                    com.GetComponent<CommandShortcut>().Shortcut = commandShortcuts[commandname];
                    com.GetComponent<CommandShortcut>().commandName = commandname;
                    return butt;
                }
                else
                {
                    // Command already exist
                    foreach (Transform T in holder.transform)
                    {
                        if(T.gameObject != holder)
                        {
                            if(T.gameObject.GetComponent<CommandShortcut>().commandName == commandname)
                            {
                                Button butt = T.gameObject.GetComponent<Button>();
                                return butt;
                            }
                        }
                    }
                }

            }
        }

        Debug.LogError("No suitable command found for: " + commandname);
        return null;
    }

    public void ResetCommands()
    {
        foreach (Transform T in holder.transform)
        {
            if(T.gameObject != gameObject)
            {
                Destroy(T.gameObject);
            }
                
        }

        for (int i = 0; i < commands.Count; i++)
        {
            commands[i].commandsActive = false;
        }
    }
}
