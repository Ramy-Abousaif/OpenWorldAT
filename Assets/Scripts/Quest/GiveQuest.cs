using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GiveQuest : MonoBehaviour
{
    public Quest quest;

    public GameObject player;
    public GameObject canvas;
    public GameObject questWindow;
    public GameObject questDoneWindow;
    public Text titleText;
    public Text descriptionText;
    public Text finishedTitleText;
    public Text finishedDescriptionText;
    public Text xpText;
    public Text goldText;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        SetUpQuest();
    }

    void SetUpQuest()
    {
        canvas = GameObject.Find("Canvas");
        questWindow = canvas.transform.GetChild(0).gameObject;
        questDoneWindow = canvas.transform.GetChild(1).gameObject;
        titleText = questWindow.transform.GetChild(0).GetComponent<Text>();
        descriptionText = questWindow.transform.GetChild(1).GetComponent<Text>();
        finishedTitleText = questDoneWindow.transform.GetChild(0).GetComponent<Text>();
        finishedDescriptionText = questDoneWindow.transform.GetChild(1).GetComponent<Text>();
        xpText = questWindow.transform.GetChild(2).GetComponent<Text>();
        goldText = questWindow.transform.GetChild(3).GetComponent<Text>();
    }

    public void OpenQuestWindow()
    {
        questWindow.SetActive(true);
        titleText.text = quest.title;
        descriptionText.text = quest.description;
        xpText.text = "XP: " + quest.xpReward.ToString();
        goldText.text = "Gold: " + quest.goldReward.ToString();
    }

    public void OpenQuestDoneWindow()
    {
        questDoneWindow.SetActive(true);
        finishedTitleText.text = quest.title;
        finishedDescriptionText.text = quest.finishedDescription;
    }
}
