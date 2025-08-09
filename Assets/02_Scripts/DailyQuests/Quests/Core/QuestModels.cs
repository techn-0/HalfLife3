using System;
using UnityEngine;

[Serializable]
public class QuestData
{
    public string id;               // e.g., 20250809-Portfolio-001
    public TrackType track;
    public string title;
    public string description;
    public QuestStatus status;
}

[Serializable]
public class DailySave
{
    public string date;             // "YYYY-MM-DD"
    public QuestData[] quests;
    public int streak;              // Perfect Day 스트릭
}
