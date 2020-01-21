
using System;
using System.Collections.Generic;

[Serializable]
public class WorkoutDataItem : ListViewDataItem
{
    public string name;
    public bool timed;
    public int repetitions;
    public List<ExerciseDataItem> exercises;

    public WorkoutDataItem(string name, List<ExerciseDataItem> exercises)
    {
        this.name = name;
    }
}
