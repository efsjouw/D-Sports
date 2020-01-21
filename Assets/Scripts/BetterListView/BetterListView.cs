using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Require the canvas component so the list view will not have to rebuild everything when it is marked dirty
/// </summary>
[RequireComponent(typeof(Canvas))]
public class BetterListView : MonoBehaviour
{
    //public class ItemClickedEvent : UnityEvent<ListViewItem> { };
    //public ItemClickedEvent itemClicked = new ItemClickedEvent();

    //Set in inspector
    public ScrollRect scrollRect;
    public Image scrollImage;
    public RectTransform scrollbarHorizontal;
    public RectTransform scrollbarVertical;
    public ListViewItem listViewItem; //inherit your prefab from this

    public Action<ListViewItem> onClickAction;

    [HideInInspector] public List<ListViewItem> items;
    private bool filling = false;

    public void clearList()
    {
        foreach(Transform child in scrollRect.content)
        {
            Destroy(child.gameObject);
        }
    }

    public void clearOnClickActions()
    {
        foreach (ListViewItem item in items)
        {
            item.button.onClick.RemoveAllListeners();
        }
    }

    public void setOnClickActions(Action<ListViewItem> onClickAction)
    {
        this.onClickAction = onClickAction;
        foreach (ListViewItem item in items)
        {
            item.button.onClick.AddListener(() =>
            {
                onClickAction(item);
            });
        }
    }

    public void fillListView(List<ListViewDataItem> itemData, Action<ListViewItem> onClickAction = null, bool clear = true)
    {        
        if (clear) clearList();
        this.onClickAction = onClickAction;
        StartCoroutine(fillList(itemData));
    }

    private IEnumerator fillList(List<ListViewDataItem> items)
    {
        filling = true;
        foreach(ListViewDataItem itemData in items)
        {
            addItem(itemData, this.onClickAction);
        }
        //Wait for end of frame so layout can be organized
        yield return new WaitForEndOfFrame();
        disableLayoutGroups(scrollRect.content);
        filling = false;
    }

    public ListViewItem addItem(ListViewDataItem itemData, Action<ListViewItem> onClickOverrideAction = null)
    {
        ListViewItem newItem = Instantiate(listViewItem, scrollRect.content);
        newItem.init(itemData);
        if (onClickAction != null && newItem.button != null)
        {
            newItem.button.onClick.AddListener(() =>
            {
                onClickAction(newItem);
            });
        }
        this.items.Add(newItem);
        return newItem;
    }

    /// <summary>
    /// Disable layout groups for performance reasons
    /// Ony call this after WaitForEndOfFrame to wait for the layout to be applied
    /// </summary>
    private void disableLayoutGroups(Transform transform)
    {
        foreach (Transform child in scrollRect.content)
        {
            HorizontalOrVerticalLayoutGroup[] layoutGroups = child.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(false);            
            foreach (HorizontalOrVerticalLayoutGroup layoutGroup in layoutGroups)
            {
                layoutGroup.enabled = false;
            }

            LayoutElement[] layoutElements = child.GetComponentsInChildren<LayoutElement>(false);
            foreach (LayoutElement layoutElement in layoutElements)
            {
                layoutElement.enabled = false;
            }
        }
    }
}
