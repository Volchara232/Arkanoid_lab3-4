using System.Collections.Generic;
using Arkanoid.Entities;

namespace Arkanoid.State
{
    public class GameState
    {
        public GameField Field { get; set; } = new GameField(20, 30);
        public List<Brick> Bricks { get; set; } = new();
        public List<Loot> Loots { get; set; } = new();
        public Ball Ball { get; set; } = new Ball();
        public Paddle Paddle { get; set; } = new Paddle();
        public int RandomSeed { get; set; } = 42;

        public GameState() { }
    }
}