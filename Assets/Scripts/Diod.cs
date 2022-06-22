using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    public class Diod
    {
        public string Name;
        public int Count;
        public bool Polarity;
        public string Position;
        public bool Detected { get; set; }
        public Diod(string name, int count, string position, bool polarity = true, bool detected = false)
        {
            Name = name;
            Count = count;
            Position = position;
            Polarity = polarity;
            Detected = detected;
            Position = position;
        }
    }
}
