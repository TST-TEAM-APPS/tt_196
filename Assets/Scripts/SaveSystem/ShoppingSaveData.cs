using System.Collections.Generic;
using System;

[Serializable]
public class ShoppingSaveData : SaveData
{
    public List<ShoppingSave> Plans { get; set; }
    
    public ShoppingSaveData(List<ShoppingSave> elments)
    {
        Plans = elments;
    }
}
