using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndZone : DesignatedZoneClass
{
    public int currentRequiredRoundScore;
    public override void EnteredDesignatedZone()
    {
        gm.EndZoneEntry(currentRequiredRoundScore);
        base.EnteredDesignatedZone();
    }

}
