using System.Collections.Generic;
using System;

[Serializable]
public class ProductSave
{
    public string Name { get; set; }
    public string Article { get; set; }
    public int Cost { get; set; }
    public int Count { get; set; }
    public bool Completed { get; set; }
}
