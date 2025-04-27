using System.Collections.Generic;
using System;

[Serializable]
public class ExpensesSave
{
    public string Name { get; set; }
    public int Type { get; set; }
    public int Cost { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
}
