using System;

namespace _02_Scripts.Reward
{
    public class RewardProgress
    {
        public RewardType Type { get; }
        public int Count { get; }
        public int Goal { get; }
        public bool Completed => Count >= Goal;   // 완료(Completed)
        public bool Received { get; }              // 수령 여부(Received)
        public bool Receivable => Completed && !Received; // 수령 가능(Receivable)
        public int Remaining => Math.Max(0, Goal - Count); // 남은 수량

        public RewardProgress(RewardType type, int count, int goal, bool received = false)
        {
            Type = type;
            Count = count;
            Goal = Math.Max(1, goal);
            Received = received;
        }
    }
}