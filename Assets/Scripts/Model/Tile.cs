public class Tile
{
    public int Level { get; private set; }
    public bool IsAlredyMatched;
    public Tile(int level)
    {
        Level = level;
        IsAlredyMatched = false;
    }

    public void UpLevel()
    {
        ++Level;
    }
}
