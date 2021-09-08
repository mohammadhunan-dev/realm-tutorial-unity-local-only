using System;
using MongoDB.Bson;
using Realms;

public class Stat : RealmObject
{


    [MapTo("_id")]
    [PrimaryKey]
    public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

    public DateTimeOffset Time { get; private set; } = DateTimeOffset.Now;

    [MapTo("score")]
    public int Score { get; set; }

    [MapTo("enemiesDefeated")]
    public int EnemiesDefeated { get; set; }

    [MapTo("tokensCollected")]
    public int TokensCollected { get; set; }

    [MapTo("statOwner")]
    public Player StatOwner { get; set; }

    public Stat()
    {

    }
}