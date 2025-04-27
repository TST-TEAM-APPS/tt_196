using System.Collections.Generic;
using System;

[Serializable]
public class ExpensesSaveData : SaveData
{
    public List<ExpensesSave> Plans { get; set; }
    public int Budget { get; set; }
    
    public ExpensesSaveData(List<ExpensesSave> elments, int budget)
    {
        Plans = elments;
        Budget = budget;
    }
}
