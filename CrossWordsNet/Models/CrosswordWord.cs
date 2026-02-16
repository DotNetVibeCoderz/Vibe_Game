using System.Collections.Generic;

namespace CrossWordsNet.Models
{
    public enum Direction { Horizontal, Vertical }

    public class CrosswordWord
    {
        public string Word { get; set; }
        public string Clue { get; set; }
        public int StartRow { get; set; }
        public int StartCol { get; set; }
        public Direction Direction { get; set; }
        public int Number { get; set; }
    }
}