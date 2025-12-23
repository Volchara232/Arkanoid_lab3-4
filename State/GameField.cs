namespace Arkanoid.State
{
    public class GameField
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public GameField() { } // Для JSON
        public GameField(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }
}