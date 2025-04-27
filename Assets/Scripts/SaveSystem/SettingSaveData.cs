using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SettingSaveData : SaveData
{
    public List<bool> IsFavorite { get; set; }
    public SettingSaveData (List<bool> isFavorite)
    {
        IsFavorite = isFavorite;
    }
}
