using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrap : Collect
{
    public override void DoThing()
    {
        MiscData.scrap += 1;
        Destroy(gameObject);
    }
}
