namespace mc_c_.level
{
    public interface LevelListener
    {
        void TileChanged(int x, int y, int z);

        void LightColumnChanged(int x, int y, int z, int lightDepth);

        void AllChanged();
    }
}
