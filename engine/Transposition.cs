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
        public ulong Key { get; set; }
        public int Score { get; set; }
        public int Depth { get; set; }
        public NodeType Type { get; set; }
        public Move? BestMove { get; set; }
        public int Age { get; set; }
    }

    public class TranspositionTable
    {
        private TranspositionTableEntry?[] _table = new TranspositionTableEntry[1 << 22];

        public void Store(ulong hash, int score, int depth, NodeType type, Move? bestMove)
        {
            if (TryGet(hash, out TranspositionTableEntry? entry) && entry.Depth >= depth)
            {
                return;
            }

            int index = (int)(hash & (ulong)(_table.Length - 1));
            _table[index] = new TranspositionTableEntry
            {
                Key = hash,
                Depth = depth,
                Score = score,
                Type = type,
                BestMove = bestMove,
            };
            
        }

        public bool TryGet(ulong hash, out TranspositionTableEntry? entry)
        {
            int index = (int)(hash & (ulong)(_table.Length - 1));
            entry = _table[index];
            return entry != null && entry.Key == hash;
        }
    }

    public struct PawnTTEntry
    {
        public ulong Key;
        public int Score;
    }
}