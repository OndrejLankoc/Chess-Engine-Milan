public class Move
{
    public Square From { get; set; }
    public Square To { get; set; }

    public Move(Square from, Square to)
    {
        From = from;
        To = to;
    }
}