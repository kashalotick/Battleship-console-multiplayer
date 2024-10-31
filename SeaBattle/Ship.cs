namespace SeaBattle;

public class Ship
{
    public int type { get; set; }
    public int y { get; set; }
    public int x { get; set; }
    public bool vertical { get; set; }

    public Ship(int type, int y, int x, bool vertical)
    {
        this.type = type;
        this.y = y;
        this.x = x;
        this.vertical = vertical;
    }
}