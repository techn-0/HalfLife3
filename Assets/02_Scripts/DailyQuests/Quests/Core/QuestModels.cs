using System;
using UnityEngine;

[Serializable]
public struct Reward { public int xp; public float coin; }

[Serializable]
public class QuestData
{
    public string id;               // e.g., 20250809-Portfolio-001
    public TrackType track;
    public string title;
    public string description;
    public QuestStatus status;
    public Reward reward;           // 퀘스트당 보상
}

[Serializable]
public class DailySave
{
    public string date;             // "YYYY-MM-DD"
    public QuestData[] quests;
    public int xpTotal;
    public float coinTotal;
    public int streak;              // Perfect Day 스트릭
    public int gachaTickets;        // Perfect Day 보너스
}
