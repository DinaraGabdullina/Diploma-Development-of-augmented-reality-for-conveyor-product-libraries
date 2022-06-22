using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    public class Сapacitor
    {
        public string Name;
        public string Capacity;
        public int Count;
        public bool Polarity;
        public string Position;
        public bool Detected { get; set; }
        public Сapacitor(string name, string capacity, string position, int count, bool polarity=false, bool detected = false)
        {
            Name = name;
            Capacity = capacity;
            Position = position;
            Count = count;
            Polarity = polarity;
            Detected = detected;
        }
    }
}
