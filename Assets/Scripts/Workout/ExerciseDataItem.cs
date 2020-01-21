
using System;

[Serializable]
public class ExerciseDataItem : ListViewDataItem
{
    public string name;
    public int reps;    //repitions
    public int time;    //time in seconds

    public ExerciseDataItem(string name)
    {
        this.name = name;
    }
}
