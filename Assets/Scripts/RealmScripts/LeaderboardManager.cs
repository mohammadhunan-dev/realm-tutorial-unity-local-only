using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Realms;
using System.Linq;
using Realms.Sync;

public class LeaderboardManager : MonoBehaviour
{
    private Realm realm;
    private IDisposable listenerToken;
    public ListView listView;
    public Button toggleButton;
    public Label displayTitle;
    public int currentPlayerHighestScore = 0; // 0 til it's set
    public string username;
    public bool isLeaderboardGUICreated = false;
    public static LeaderboardManager Instance;
    public bool isUIVisible;
    public int maximumAmountOfTopStats;
    public List<Stat> topStats;
    public VisualElement root;


    // 
    void Awake()
    {
        Instance = this;
    }

    public void setLoggedInUser(string loggedInUser)
    {
        username = loggedInUser;

        realm = Realm.GetInstance();

        // only create the leaderboard on the first run, consecutive restarts/reruns will already have a leaderboard created
        if (isLeaderboardGUICreated == false)
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            createLeaderboardUI();
            root.Add(toggleButton);
            root.Add(displayTitle);
            root.Add(listView);
            isUIVisible = true;

            toggleUIVisible();

            toggleButton.clicked += () =>
            {
                toggleUIVisible();
            };
            setStatListener(); // start listening for score changes once the leaderboard GUI has launched
            isLeaderboardGUICreated = true;
        }
    }

    public int getRealmPlayerTopStat()
    {
        var realmPlayer = realm.All<Player>().Where(p => p.Name == username).First();
        var realmPlayerTopStat = realmPlayer.Stats.OrderByDescending(s => s.Score).First().Score;
        //Debug.Log("r player exists - " + realmPlayer.Name);

        //Debug.Log("r player top score - en defeated " + realmPlayer.Stats.ElementAt(0).EnemiesDefeated);
        return realmPlayer.Stats.OrderByDescending(s => s.Score).First().Score;
    }

    void toggleUIVisible()
    {
        if (isUIVisible == true)
        {
            // if ui is already visible, and the toggle button is pressed, then hide it
            displayTitle.RemoveFromClassList("visible");
            displayTitle.AddToClassList("hide");
            listView.RemoveFromClassList("visible");
            listView.AddToClassList("hide");
            isUIVisible = false;
        }
        else
        {
            // if ui is not visible, and the toggle button is pressed, then show it
            displayTitle.RemoveFromClassList("hide");
            displayTitle.AddToClassList("visible");
            listView.RemoveFromClassList("hide");
            listView.AddToClassList("visible");
            isUIVisible = true;
        }
    }
    void createLeaderboardUI()
    {
        // create toggle button
        toggleButton = new Button();
        toggleButton.text = "Toggle Leaderboard & Settings";
        toggleButton.AddToClassList("toggle-button");

        // create leaderboard title
        displayTitle = new Label();
        displayTitle.text = "Leaderboard:";
        displayTitle.AddToClassList("display-title");


        topStats = realm.All<Stat>().OrderByDescending(s => s.Score).ToList();
        createTopStatListView();


    }
    public void createTopStatListView()
    {
        if (topStats.Count > 4)
        {
            maximumAmountOfTopStats = 5;
        }
        else
        {
            maximumAmountOfTopStats = topStats.Count;
        }


        var topStatsListItems = new List<string>();

        topStatsListItems.Add("Your top points: " + getRealmPlayerTopStat());


        for (int i = 0; i < maximumAmountOfTopStats; i++)
        {
            if (topStats[i].Score > 1) // if there's not many players there may not be 5 top scores yet
            {
                topStatsListItems.Add($"{topStats[i].StatOwner.Name}: {topStats[i].Score} points");
            }
        };

        // Create a new label for each top score
        var label = new Label();
        label.AddToClassList("list-item-game-name-label");
        Func<VisualElement> makeItem = () => new Label();

        // Bind Stats to the UI
        Action<VisualElement, int> bindItem = (e, i) => {
            (e as Label).text = topStatsListItems[i];
            (e as Label).AddToClassList("list-item-game-name-label");
        };

        // Provide the list view with an explict height for every row
        // so it can calculate how many items to actually display
        const int itemHeight = 5;

        listView = new ListView(topStatsListItems, itemHeight, makeItem, bindItem);
        listView.AddToClassList("list-view");

    }
    public void setStatListener()
    {
        //// Observe collection notifications. Retain the token to keep observing.
        listenerToken = realm.All<Stat>()
            .SubscribeForNotifications((sender, changes, error) =>
            {

                if (error != null)
                {
                    // Show error message
                    Debug.Log("an error occurred while listening for score changes :" + error);
                    return;
                }

                if(changes != null)
                {
                    setNewlyInsertedScores(changes.InsertedIndices);
                }
                // we only need to check for inserted because scores can't be modified or deleted after the run is complete
                
            });
    }
    public void setNewlyInsertedScores(int[] insertedIndices)
    {
        foreach (var i in insertedIndices)
        {
            // ... handle insertions ...
            var newStat = realm.All<Stat>().ElementAt(i);

            for (var scoreIndex = 0; scoreIndex < topStats.Count; scoreIndex++)
            {
                if (topStats.ElementAt(scoreIndex).Score < newStat.Score)
                {
                    if (topStats.Count > 4)
                    { // An item shouldnt be removed if its the leaderboard is less than 5 items
                        topStats.RemoveAt(topStats.Count - 1);
                    }
                    topStats.Insert(scoreIndex, newStat);
                    root.Remove(listView); // remove the old listView
                    createTopStatListView(); // create a new listView
                    root.Add(listView); // add the new listView to the UI
                    break;
                }
            }
        }
    }
    void OnDisable()
    {
        if (realm != null)
        {
            realm.Dispose();
        }

        if (listenerToken != null)
        {
            realm.Dispose();
        }
    }
}