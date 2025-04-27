using System.Collections.Generic;
using System;

[Serializable]
public class VisitSaveData : SaveData
{
    public List<VisitSave> Plans { get; set; }
    
    public VisitSaveData(List<VisitSave> elments)
    {
        Plans = elments;
    }
}
