using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCB : MonoBehaviour
{
    public string Name { get; }
    public string PCBMountingType { get; }
    public Resistor Resistor { get; }
    public Ñapacitor Capacitor { get; }
    public Diod Diod { get; }
    public bool Detected { get; set; }

    public PCB(string name, string pcbMT, Resistor resistor, Ñapacitor capacitor, Diod diod, bool detected=false)
    {
        Name = name;
        PCBMountingType = pcbMT;
        Resistor = resistor;
        Capacitor = capacitor;
        Diod = diod;
        Detected = detected;
    }

    
}
