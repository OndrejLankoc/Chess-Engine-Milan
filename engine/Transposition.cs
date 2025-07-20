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
    }

    public class TranspositionTable
    {
        private Dictionary<ulong, TranspositionTableEntry> table = new();

        public void Store(ulong hash, int depth, int score, NodeType type, Move bestMove)
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
                BestMove = bestMove
            };
            
        }

        public bool TryGet(ulong hash, out TranspositionTableEntry entry)
        {
            return table.TryGetValue(hash, out entry);
        }
    }
}