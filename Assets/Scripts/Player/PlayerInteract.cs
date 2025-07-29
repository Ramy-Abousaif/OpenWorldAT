using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public GenerateMap map;
    public LayerMask npc, gatherable;
    private Camera cam;
    private GiveQuest currentNpc;
    private GameObject canvas;
    private GameObject questWindow;
    public GameObject questDoneWindow;

    void Start()
    {
        map = GameObject.Find("Map").GetComponent<GenerateMap>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        canvas = GameObject.Find("Canvas");
        questWindow = canvas.transform.GetChild(0).gameObject;
        questDoneWindow = canvas.transform.GetChild(1).gameObject;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, npc))
            {
                if (hit.transform.GetComponent<GiveQuest>() != null)
                {
                    currentNpc = hit.transform.GetComponent<GiveQuest>();
                    if (!transform.GetComponent<Status>().quest.isActive)
                    {
                        hit.transform.GetComponent<GiveQuest>().OpenQuestWindow();
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
                    else
                    {
                        if(transform.GetComponent<Status>().quest.goal.IsReached())
                        {
                            hit.transform.GetComponent<GiveQuest>().OpenQuestDoneWindow();
                            Cursor.lockState = CursorLockMode.None;
                            Cursor.visible = true;
                        }
                    }
                }
            }
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, gatherable))
            {
                if (transform.GetComponent<Status>().quest.isActive)
                {
                    transform.GetComponent<Status>().quest.goal.CurrentAmt++;
                    Instantiate(Resources.Load("Prefabs/Sparks"), hit.transform.position, Quaternion.identity);
                    hit.transform.GetComponent<Orb>().active = false;
                    map.SaveOrbs();
                    Destroy(hit.transform.gameObject);
                }
            }
        }
    }

    public void AcceptQuest()
    {
        questWindow.SetActive(false);
        currentNpc.quest.isActive = true;
        GetComponent<Status>().quest = currentNpc.quest;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RefuseQuest()
    {
        questWindow.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuestDone()
    {
        questDoneWindow.SetActive(false);
        currentNpc.quest.isActive = false;
        GetComponent<Status>().gold += currentNpc.quest.goldReward;
        GetComponent<Status>().xp += currentNpc.quest.xpReward;
        GetComponent<Status>().quest = null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
