namespace Engine
{
    public enum NodeType
    {
        Exact,
        LowerBound,
        UpperBound
    }

    public class TranspositionTableEntry
    {
        public int Depth { get; set; }
        public int Score { get; set; }
        public NodeType Type { get; set; }
        public Move BestMove { get; set; }
        public int Age { get; set; }
    }

    public class TranspositionTable
    {
        private Dictionary<ulong, TranspositionTableEntry> table = new();

        public void Store(ulong hash, int depth, int score, NodeType type, Move bestMove, int age)
        {
            if (table.TryGetValue(hash, out TranspositionTableEntry entry))
            {
                if (entry.Depth >= depth)
                {
                    return;
                }
            }

            table[hash] = new TranspositionTableEntry
            {
                Depth = depth,
                Score = score,
                Type = type,
                BestMove = bestMove,
                Age = age
            };
            
        }

        public bool TryGet(ulong hash, out TranspositionTableEntry entry)
        {
            return table.TryGetValue(hash, out entry);
        }

        public void ClearOldEntries(int currentAge)
        {
            var keysToRemove = table.Where(ktr => ktr.Value.Age < currentAge - 4).Select(kvp => kvp.Key).ToList();
            foreach (var key in keysToRemove)
            {
                table.Remove(key);
            }
        }
    }

    public struct PawnTTEntry
    {
        public ulong Key;
        public int Score;
    }
}