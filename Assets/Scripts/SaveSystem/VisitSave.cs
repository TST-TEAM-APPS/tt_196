using System.Collections.Generic;
using System;

[Serializable]
public class VisitSave
{
    public string Name { get; set; }
    public byte[] ImagePath { get; set; } = new byte[] { };
    public List<SubtaskSave> Subtasks { get; set; } = new List<SubtaskSave>();
    public string Description { get; set; }
}
