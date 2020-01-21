using TMPro;

public class WorkoutItem : ListViewItem
{
    public TMP_Text nameTmp;
    public new ExerciseDataItem data;

    public override void init(ListViewDataItem data)
    {
        this.data = (ExerciseDataItem)data;
        nameTmp.text = this.data.name;
    }
}
