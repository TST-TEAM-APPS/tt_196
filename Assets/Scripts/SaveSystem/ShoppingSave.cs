using System.Collections.Generic;
using System;

[Serializable]
public class ShoppingSave
{
    public string Name { get; set; }
    public List<ProductSave> Subtasks { get; set; } = new List<ProductSave>();
    public string Description { get; set; }
}
