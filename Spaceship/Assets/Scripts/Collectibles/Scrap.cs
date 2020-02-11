using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrap : Collect
{
    public override void DoThing()
    {
        MiscData.Scrap += 1+GameManager.current.boosts["Scrap Collection Increase"];
        Destroy(gameObject);
    }
}
