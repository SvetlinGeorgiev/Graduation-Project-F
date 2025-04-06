using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemTextPrefab;
    public Transform itemListContainer; 

    private List<GameObject> itemUIObjects = new List<GameObject>();

    public void UpdateInventoryUI(List<Item> items)
    {
        foreach (var obj in itemUIObjects)
        {
            Destroy(obj);
        }
        itemUIObjects.Clear();

        foreach (var item in items)
        {
            GameObject textObj = Instantiate(itemTextPrefab, itemListContainer);
            textObj.GetComponent<TMP_Text>().text = $"{item.itemName} (x{item.amount})";
            itemUIObjects.Add(textObj);
        }
    }
}
