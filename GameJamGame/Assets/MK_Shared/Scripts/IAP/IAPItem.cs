using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

[System.Serializable]
[CreateAssetMenu(fileName = "NewIAPItem", menuName = "MightyKingdom/IAP Item")]
public class IAPItem : ScriptableObject
{

    public string friendlyID; //The name this is known by in game
    [SerializeField]
    private string description;
    public ProductType productType; //Consumable, Non-Consumable, Subscription

    [SerializeField]
    private string storeID;

    [SerializeField]
    private string discountStoreID;

    [Header("Store Overrides")]

    [SerializeField]
    private string appleStoreID;

    [SerializeField]
    private string googleStoreID;

    [Header("Supported Stores")]

    public bool AppleAppStore = true;
    public bool GooglePlay = true;
    public bool AmazonApps = true;

    //Purchasing this item also unlocked these items. 
    //Does not work for consumables
    public IAPItem[] subPurchases;

    public string GetStoreID(bool discounted = false)
    {
        if (discounted)
            return discountStoreID;

        string id = storeID;

        //Store overrides
#if UNITY_ANDROID
        if (!string.IsNullOrEmpty(googleStoreID))
            id = googleStoreID;
#elif UNITY_IOS
        if(!string.IsNullOrEmpty(appleStoreID)) 
            id = appleStoreID;
#endif
        return id;
    }
}
