using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realms;
using UnityEngine.UI;
using MongoDB.Bson;
using System.Threading.Tasks;
using Realms.Sync;
using System.Linq;

public class RealmController : MonoBehaviour
{
    private static Realm realm;
    private static App realmApp;
    public static RealmController Instance;
    private static int runTime; // lower run time rewards speedrunners with bonus points
    private static int bonusPoints = 0; // start with 0 bonus points and at the end of the game we add bonus points based on how long you played

    public static Player currentPlayer;
    public static Stat currentStat;

    void Awake()
    {
        Instance = this;
    }

    public static Realm GetRealm()
    {
        realm = Realm.GetInstance();
        Debug.Log($"Realm is located at: {realm.Config.DatabasePath}");
        return realm;
    }

    public static void setLoggedInUser(string loggedInUser)
    {
        realm = GetRealm();
        var matchedPlayers = realm.All<Player>().Where(p => p.Name == loggedInUser);

        if (matchedPlayers.Count() > 0) // if the player exists
        {
            currentPlayer = matchedPlayers.First();

            var s1 = new Stat();
            s1.StatOwner = currentPlayer;

            realm.Write(() =>
            {
                currentStat = realm.Add(s1);
                currentPlayer.Stats.Add(currentStat);
            });
        }
        else
        {
            var p1 = new Player();
            p1.Id = ObjectId.GenerateNewId().ToString();
            p1.Name = loggedInUser;

            var s1 = new Stat();
            s1.StatOwner = p1;

            realm.Write(() =>
            {
                currentPlayer = realm.Add(p1);
                currentStat = realm.Add(s1);
                currentPlayer.Stats.Add(currentStat);
            });
        }

        startGame();
    }

    public static void startGame()
    {
        // record each 10 seconds (runTime will be used to calculate bonus points once the player wins the game)
        var myTimer = new System.Timers.Timer(10000);
        myTimer.Enabled = true;
        myTimer.Elapsed += (sender, e) => runTime += 10;
    }

    public static void collectToken() // performs an update on the Character Model's token count
    {
        realm.Write(() =>
        {
            currentStat.TokensCollected += 1;
        });
    }
    public static void defeatEnemy() // performs an update on the Character Model's enemiesDefeated Count
    {
        realm.Write(() =>
        {
            currentStat.EnemiesDefeated += 1;
        });
    }

    public static void deleteCurrentScore()
    {
        ScoreCardManager.unRegisterListener();
        realm.Write(() =>
        {
            realm.Remove(currentStat);
            currentPlayer.Stats.Remove(currentStat);
        });
    }
    public static void restartGame()
    {
        if (currentPlayer != null)
        {
            // create a new score for the game
            var s1 = new Stat();
            s1.StatOwner = currentPlayer;

            realm.Write(() =>
            {
                currentStat = realm.Add(s1);
                currentPlayer.Stats.Add(currentStat);
            });

            ScoreCardManager.setCurrentStat(currentStat);
            ScoreCardManager.watchForChangesToCurrentStats();

            startGame();
        }
    }

    public static int[] playerWon()
    {
        if (runTime <= 30) // if the game is beat in in less than or equal to 30 seconds, +80 bonus points
        {
            bonusPoints = 80;
        }
        else if (runTime <= 60) // if the game is beat in in less than or equal to 1 min, +70 bonus points
        {
            bonusPoints = 70;
        }
        else if (runTime <= 90) // if the game is beat in less than or equal to 1 min 30 seconds, +60 bonus points
        {
            bonusPoints = 60;
        }
        else if (runTime <= 120) // if the game is beat in less than or equal to 2 mins, +50 bonus points
        {
            bonusPoints = 50;
        }

        // calculate final points + write to realm with points
        var finalScore = calculatePoints();
        realm.Write(() =>
        {
            currentStat.Score = finalScore;
        });


        var scoreAndBonusPoints = new int[2];
        scoreAndBonusPoints[0] = finalScore;
        scoreAndBonusPoints[1] = bonusPoints;

        return scoreAndBonusPoints;
    }

    public static int calculatePoints()
    {
        return (currentStat.EnemiesDefeated + 1) * (currentStat.TokensCollected + 1) + bonusPoints;
    }
}
