using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

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

    public override void doStartAnimation()
    {
        //transform.DOShakeScale(0.2f, 0.1f);
    }
}
