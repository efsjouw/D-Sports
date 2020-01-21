using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListController : MonoBehaviour
{   
    //PanelNavigator to bind lists to
    public bool navigateOnInstantiate = true;
    public bool keepListPersistent = false;
    public BetterListView listViewPrefab;
    public PanelNavigator panelNavigator;

    //TODO: Rename to ListViews or something
    [Serializable]
    public class ListItem {
        public string name;
        public ListViewItem item;
        public RectTransform rectBoundaries;
    }

    public List<ListItem> listItems;
    private Dictionary<string, BetterListView> _instanceList;

    void Awake()
    {
        _instanceList = new Dictionary<string, BetterListView>();
    }

    /// <summary>
    /// For inspector use
    /// </summary>
    /// <param name="panelName"></param>
    public void showList(string panelName)
    {
        showList(panelName, null);
    }

    /// <summary>
    /// Used when you want to assign an action
    /// </summary>
    /// <param name="panelName"></param>
    /// <param name="onClickAction"></param>
    public void showList(string panelName, Action<ListViewItem> onClickAction = null)
    {        
        navigateToList(panelName);
        _instanceList[panelName].gameObject.SetActive(true);
        fillList(panelName, onClickAction);        
    }

    public void hideList(string panelName)
    {
        if (_instanceList != null && _instanceList.ContainsKey(panelName)) _instanceList[panelName].gameObject.SetActive(false);
    }

    public void setListBoundaries(string panelName, RectTransform rectTransform)
    {
        ListItem listItem = getListItem(panelName);
        setBoundaries(listItem);
    }

    private void fillList(string panelName, Action<ListViewItem> onClickAction = null)
    {
        switch(panelName)
        {
            case "exercises":
                //Only fill list view if empty                
                if (_instanceList.ContainsKey(panelName))
                {
                    bool listEmpty = _instanceList[panelName].scrollRect.content.childCount == 0;
                    if(listEmpty || !keepListPersistent)
                    {
                        List<ListViewDataItem> exerciseItems = loadExercises();
                        _instanceList[panelName].scrollRect.horizontal = false;
                        _instanceList[panelName].fillListView(exerciseItems, onClickAction);
                    }                 
                }                              
                break;
            case "workouts":
                //TODO: Make workouts
                //if (instanceList[panelName].scrollRect.content.childCount == 0)
                //{
                //    List<ListViewDataItem> exerciseItems = loadExercises();
                //    instanceList[panelName].scrollRect.horizontal = false;
                //    instanceList[panelName].fillListView(exerciseItems, onClickExerciseItem);
                //}
                Debug.Log("load workouts");
                break;
        }        
    }

    public void navigateToList(string panelName)
    {
        //Create a new list if it is does not exist yet
        if (!_instanceList.ContainsKey(panelName) || _instanceList[panelName] == null) _instanceList[panelName] = createList(getListItem(panelName).item, listViewPrefab);
        if (navigateOnInstantiate) panelNavigator.goToPanel(panelName);

        string listInstanceName = panelName.FirstCharToUpper() + "ListCanvas";
        _instanceList[panelName].gameObject.name = listInstanceName;

        PanelNavigatorBase.PanelEntry panelEntry = panelNavigator.getPanel(panelName);
        if (panelEntry != null)
        {
            if (panelNavigator.getPanel(panelName).extraCanvas == null)
                panelNavigator.getPanel(panelName).extraCanvas = _instanceList[panelName].GetComponent<Canvas>();
        }

        ListItem listItem = getListItem(panelName);
        StartCoroutine(setBoundaries(listItem));
    }

    private IEnumerator setBoundaries(ListItem listItem)
    {
        yield return new WaitForEndOfFrame();
        if (listItem.rectBoundaries)
        {
            Vector2 boundaries = new Vector2(listItem.rectBoundaries.sizeDelta.x, listItem.rectBoundaries.sizeDelta.y);
            _instanceList[listItem.name].scrollRect.GetComponent<RectTransform>().sizeDelta = boundaries;
        }
    }

    private ListItem getListItem(string key)
    {
        foreach(ListItem listItem in listItems)
        {
            if (listItem.name == key) return listItem;
        }
        return null;
    }

    private List<ListViewDataItem> loadExercises()
    {
        List<ListViewDataItem> data = new List<ListViewDataItem>();
        string json = Resources.Load<TextAsset>("exercises").text;
        JArray array = JArray.Parse(json);
        foreach (JObject jobject in array)
        {
            data.Add(new ExerciseDataItem((string)jobject["name"]));
        }
        return data;
    }    

    private BetterListView createList(ListViewItem item, BetterListView prefab)
    {
        BetterListView instance = Instantiate(prefab);
        instance.listViewItem = item; //Set list item prefab
        return instance;        
    }
}
