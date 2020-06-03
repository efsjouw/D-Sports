using TMPro;

public class ExerciseItem : ListViewItem
{
    public TMP_Text nameTmp;
    public LinkButton linkButton;

    public new ExerciseDataItem data;

    public override void init(ListViewDataItem data)
    {
        this.data = (ExerciseDataItem) data;
        nameTmp.text = this.data.name;

        string searchQuery = this.data.name + " exercise";
        string imagesParameter = "&iax=images&ia=images";

        linkButton.appendToURL(searchQuery + imagesParameter);
    }
}
