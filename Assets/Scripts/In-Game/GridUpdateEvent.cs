public class GridUpdateEvent
{
    public int Column;
    public int Row;
    public byte CellSprite;

    public GridUpdateEvent(int col, int row, byte cellSprite)
    {
        Column = col;
        Row = row;
        CellSprite = cellSprite;
    }
}
