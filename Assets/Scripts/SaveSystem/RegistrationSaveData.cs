using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RegistrationSaveData : SaveData
{
    public string Link { get; set; }
    public bool Registered { get; set; }

    public RegistrationSaveData(string link, bool registered)
    {
        Link = link;
        Registered = registered;
    }
}
