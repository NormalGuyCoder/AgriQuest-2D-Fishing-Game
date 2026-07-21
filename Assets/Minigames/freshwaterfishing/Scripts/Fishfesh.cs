using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreshwaterFish
{
    public new string name;
    public string spriteID;
    public int baseCost;
    public int spokeWeight;

    public FreshwaterFish(string a, string b, int c, int d)
    {
        name = a;
        spriteID = b;
        baseCost = c;
        spokeWeight = d;
    }
}