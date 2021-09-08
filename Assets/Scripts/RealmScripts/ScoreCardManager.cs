using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Realms;
using UnityEngine;
using UnityEngine.UIElements;
using System.ComponentModel;

public class ScoreCardManager : MonoBehaviour
{
    private static Realm realm;
    public static VisualElement root;
    public static Label scoreCardHeader;
    public static string username;
    public static Player currentPlayer;
    public static Stat currentStat;
    public static PropertyChangedEventHandler propertyHandler = new PropertyChangedEventHandler((sender, e) => updateCurrentStats());


    // Start is called before the first frame update
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        scoreCardHeader = root.Q<Label>("score-card-header");

        
    }

    public static void setLoggedInUser(string loggedInUser)
    {
        username = loggedInUser;
        currentStat = RealmController.currentStat;
        updateCurrentStats(); // set initial stats
        watchForChangesToCurrentStats();

        //Debug.Log("Enemies d" + RealmController.currentStat.EnemiesDefeated);

    }

    public static void updateCurrentStats() // updates stats in UI
    {
        scoreCardHeader.text = username + "\n" +
        "Enemies Defeated: " + currentStat.EnemiesDefeated + "\n" +
        "Tokens Collected: " + currentStat.TokensCollected + "\n" +
        "Current Score: " + currentStat.Score;
    }


    public static void watchForChangesToCurrentStats()
    {

        currentStat.PropertyChanged += propertyHandler;
    }

    public static void unRegisterListener()
    {
        // unregister when the player has lost
        currentStat.PropertyChanged -= propertyHandler;
        scoreCardHeader.text = username + "\n" +
        "Enemies Defeated: " + 0 + "\n" +
        "Tokens Collected: " + 0 + "\n" +
        "Current Score: " + 0;

    }

    public static void setCurrentStat(Stat newStat)
    {
        // called when the game has reset
        currentStat = newStat;
        updateCurrentStats();
    }
}
