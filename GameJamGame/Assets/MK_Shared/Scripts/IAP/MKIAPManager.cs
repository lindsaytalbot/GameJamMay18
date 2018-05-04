using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace MightyKingdom
{
    public class MKIAPManager : MonoBehaviour, IStoreListener
    {
        private const string PURCHASED_ITEMS_KEY = "MK_PurchaseItems";

        //Is the IAP manager ready to process purchases
        public static bool ReadyToProcessPurchases { get; protected set; }

        private static MKIAPManager _instance;
        private static List<IAPItem> iapItems;

        private static IStoreController controller;
        private static IExtensionProvider extensions;

        private static Action<string> onSuccessOneOff;
        private static Action<string, string> onFailureOneOff;

        private static List<IPurchaseListener> purchaseListeners = new List<IPurchaseListener>();
        private static List<IPurchaseEventsListener> purchaseEventsListeners = new List<IPurchaseEventsListener>();
        private static Dictionary<string, IAPItem> purchasedItems = new Dictionary<string, IAPItem>();

        private static bool restoringPurchases;
        private static int restoredPurchaseCount;
        private static string currentPurchaseLocation = "N/A";

        private const string DISCOUNT_SUFFIX = "_discount";

        //List of all purchases that have not been processed
        private static List<PurchaseEventArgs> pendingPurchases = new List<PurchaseEventArgs>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (_instance != null)
                return;

            MKPlayerPrefs.Init();

            //Create GameObject and add components
            GameObject go = new GameObject();
            go.name = "MKIAPManager";
            _instance = go.AddComponent<MKIAPManager>();
            DontDestroyOnLoad(go);

            MKLog.Log("MKIAPManager created", "green");

            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            //Load IAP objects
            iapItems = new List<IAPItem>(Resources.LoadAll<IAPItem>("IAP"));

            //Load offline purchases
            List<string> savedPurchases = new List<string>(MKPlayerPrefs.GetStringArray(PURCHASED_ITEMS_KEY));

            //Configure IAP
            foreach (IAPItem item in iapItems)
            {
                IDs storeIds = new IDs { };

                if (string.IsNullOrEmpty(item.friendlyID) || string.IsNullOrEmpty(item.GetStoreID()))
                {
                    MKLog.LogError("IAP Item not configured properly: " + item.name);
                    continue;
                }

                List<string> supportedStores = new List<string>();

                if (item.AppleAppStore)
                    supportedStores.Add(AppleAppStore.Name);
                if (item.GooglePlay)
                    supportedStores.Add(GooglePlay.Name);
                if (item.AmazonApps)
                    supportedStores.Add(AmazonApps.Name);

                //Not allowed on any store
                if (supportedStores.Count == 0)
                {
                    MKLog.LogError("IAP Items has not supported stores: " + item.name);
                    continue;
                }

                //Regular Product
                if (string.IsNullOrEmpty(item.GetStoreID()))
                    continue;

                storeIds.Add(item.GetStoreID(), supportedStores.ToArray());

                //Add Regular Product
                builder.AddProduct(item.friendlyID, item.productType, storeIds);

                //Discounted product
                if (!string.IsNullOrEmpty(item.GetStoreID(true)))
                {
                    storeIds = new IDs { };
                    storeIds.Add(item.GetStoreID(true), supportedStores.ToArray());
                    //Add Discounted Product
                    builder.AddProduct(item.friendlyID + DISCOUNT_SUFFIX, item.productType, storeIds);
                }

                //Add to purchased items
                if (savedPurchases.Contains(item.friendlyID))
                {
                    SetItemPurchased(item.friendlyID);
                }
            }

            UnityPurchasing.Initialize(_instance, builder);
        }

        public static bool IsPurchasingAvailable()
        {
            return controller != null && controller.products != null;
        }

        /// <summary>
        /// Attempts to purchase an item.
        /// </summary>
        /// <param name="itemName">Friendly ID of the IAP</param>
        /// <param name="onSuccess">One off callback on success. Passes the itemName</param>
        /// <param name="onFailure">One off callback on failure. Passed the itemName and the error message</param>
        public static void PurchaseItem(string itemName, Action<string> onSuccess = null, Action<string, string> onFailure = null, string purchaseLocation = "", bool discounted = false)
        {
            //You must call ReadyAndProcessPendingPurchases() before attempting purchases
            if (!ReadyToProcessPurchases)
            {
                MKLog.LogError("IAP Manager not marked as ready! Call ReadyAndProcessPendingPurchases() before attempting any purchases");
                return;
            }

            if (!CanPurchaseItem(itemName))
                return;

            onSuccessOneOff = onSuccess;
            onFailureOneOff = onFailure;

            foreach (IPurchaseEventsListener listener in purchaseEventsListeners)
            {
                listener.OnPurchaseStart(itemName);
            }

            //User offline or not signed in
            if (IsPurchasingAvailable())
            {
                currentPurchaseLocation = purchaseLocation;

                if (discounted)
                    controller.InitiatePurchase(itemName + DISCOUNT_SUFFIX);
                else
                    controller.InitiatePurchase(itemName);
            }
            //Initiate purchase
            else
            {
                _instance.OnPurchaseFailed(null, PurchaseFailureReason.PurchasingUnavailable);
            }
        }

        //Returns true if this item is available for purchase
        public static bool CanPurchaseItem(string itemName)
        {
            IAPItem item = GetItem(itemName);
            if (item == null)
            {
                MKLog.Log("Item " + itemName + " does not exists");
                return false;
            }

            if (HasPurchasedItem(itemName))
            {
                MKLog.Log("Item " + itemName + " already purchased");
                return false;
            }

            return true;
        }

        //Returns the formatted price for the given item
        public static bool HasDiscountPrice(string itemName)
        {
            //Controller not initialized, likely means user is offline or not signed in
            if (IsPurchasingAvailable() == false || string.IsNullOrEmpty(itemName))
            {
                if (IsPurchasingAvailable() == false)
                    MKLog.LogError("STORE NOT INITIALIZED");
                return false;
            }

            //Get discounted product
            Product product = controller.products.WithID(itemName + DISCOUNT_SUFFIX);

            return product != null && product.metadata != null;
        }

        //Returns the formatted price for the given item
        public static string GetPriceForItem(string itemName, bool discounted = false)
        {
            //Controller not initialized, likely means user is offline or not signed in
            if (IsPurchasingAvailable() == false || string.IsNullOrEmpty(itemName))
            {
                if (IsPurchasingAvailable() == false)
                    MKLog.LogError("STORE NOT INITIALIZED");
                return "Unavailable";
            }

            Product product = null;

            if (discounted)
                product = controller.products.WithID(itemName + DISCOUNT_SUFFIX);
            else
                product = controller.products.WithID(itemName);

            //User offline or product does not exist
            if (product == null || product.metadata == null)
            {
                //Try return the regular price
                if (discounted)
                    return GetPriceForItem(itemName, false);
                return "Unavailable";
            }

            return product.metadata.localizedPriceString;
        }

        //Registers the given listener to recieves events whenever a purchase is made
        public static void RegisterListener(IPurchaseListener listener)
        {
            if (listener != null && !purchaseListeners.Contains(listener))
            {
                purchaseListeners.Add(listener);
            }
        }
        //Registers the given listener to recieves events whenever a purchase is made, started, or restored
        public static void RegisterListener(IPurchaseEventsListener listener)
        {
            if (listener != null && !purchaseEventsListeners.Contains(listener))
            {
                purchaseEventsListeners.Add(listener);
            }
        }

        //Remove the given listener
        public static void UnregisterListener(IPurchaseListener listener)
        {
            purchaseListeners.Remove(listener);
        }

        //Remove the given listener
        public static void UnregisterListener(IPurchaseEventsListener listener)
        {
            purchaseEventsListeners.Remove(listener);
        }

        //Marks the IAPItem with the given name as purchased
        private static void SetItemPurchased(string itemName)
        {
            IAPItem item = GetItem(itemName);

            if (item == null || item.productType == ProductType.Consumable)
                return;

            purchasedItems[itemName] = item;

            foreach (IAPItem subItem in item.subPurchases)
            {
                SetItemPurchased(subItem.friendlyID);
            }
        }

        //Returns if the user has already purchased this item. 
        //Always returns false for consumables since they are not added to purchasedItems
        public static bool HasPurchasedItem(string itemName)
        {
            return (purchasedItems.ContainsKey(itemName));
        }

        //Returns the IAPItem with the given name
        private static IAPItem GetItem(string itemName)
        {
            return iapItems.Find(t => t.friendlyID == itemName);
        }

        //Returns true if this item has a store reciept
        private static bool HasRecieptFor(string itemName)
        {
            //User is offline or not signed in
            if (IsPurchasingAvailable() == false)
                return false;

            Product product = controller.products.WithID(itemName);
            Product discountProduct = controller.products.WithID(itemName + DISCOUNT_SUFFIX);

            bool regularReceipt = product != null && product.hasReceipt;
            bool discountReceipt = discountProduct != null && discountProduct.hasReceipt;
            return regularReceipt || discountReceipt;
        }

        //Save purchases to disk for loading offline later
        private static void SavePurchases()
        {
            List<string> purchasedNames = new List<string>();

            foreach (var entry in purchasedItems)
            {
                purchasedNames.Add(entry.Value.friendlyID);
            }

            MKPlayerPrefs.SetStringArray(PURCHASED_ITEMS_KEY, purchasedNames.ToArray());
        }


        //Restores the users purchases
        public static void RestorePurchases()
        {
            //Restore purchases only required on iOS
            if (Application.platform != RuntimePlatform.IPhonePlayer)
                return;

            //Inform all listeners
            foreach (IPurchaseEventsListener listener in purchaseEventsListeners)
            {
                listener.OnRestoreStart();
            }

            restoringPurchases = true;
            restoredPurchaseCount = 0;

            IAppleExtensions apple = extensions.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result) =>
            {
                foreach (IPurchaseEventsListener listener in purchaseEventsListeners)
                {
                    listener.OnPurchasesRestored(result, restoredPurchaseCount);
                }

                restoringPurchases = false;
                restoredPurchaseCount = 0;
            });
        }

        //Called when Unity successfully inititializes it's purchasing modules
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            MKIAPManager.controller = controller;
            MKIAPManager.extensions = extensions;

            foreach (IAPItem item in iapItems)
            {
                //Add to purchased items
                if (HasRecieptFor(item.friendlyID))
                {
                    SetItemPurchased(item.friendlyID);
                }
            }

            SavePurchases();

            //Inform all listeners
            foreach (IPurchaseEventsListener listener in purchaseEventsListeners)
            {
                listener.OnPurchasesInitialized();
            }
        }

        //Called when Unity fails to initialize it's purchasing modules
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            MKLog.LogError(error.ToString());
        }

        //Called on purchase failed
        public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
        {
            string productID = "";
            if (i != null)
            {
                productID = i.definition.id;
                productID = productID.Replace(DISCOUNT_SUFFIX, "");
            }

            //Inform all listeners
            foreach (IPurchaseEventsListener listener in purchaseEventsListeners)
            {
                listener.OnPurchaseFailure(productID, p);
            }

            //Complete one off callbacks
            if (onFailureOneOff != null)
            {
                onFailureOneOff(productID, p.ToString());
            }

            //Clear one off callbacks
            onFailureOneOff = null;
            onSuccessOneOff = null;

            MKLog.Log("Purchase Failed for " + productID + " because " + p.ToString());
        }

        //Call on successful purchases
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            //You must add a listener and call ReadyAndProcessPendingPurchases() before attempting purchases
            //ProcessPurchase may be called when attempting a purchase from outsite the app
            if (!ReadyToProcessPurchases)
            {
                MKLog.Log("Not ready to process purchase " + e.purchasedProduct.definition.id + ". Marked as pending");
                pendingPurchases.Add(e);
                return PurchaseProcessingResult.Pending;
            }

            bool validPurchase = true; // Presume valid for platforms with no R.V.

            // Unity IAP's validation logic is only included on these platforms.
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
            if (Debug.isDebugBuild == false)
            {
                // Prepare the validator with the secrets we prepared in the Editor
                // obfuscation window.
                var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
                    AppleTangle.Data(), Application.identifier);

                try
                {
                    // On Google Play, result has a single product ID.
                    // On Apple stores, receipts contain multiple products.
                    var result = validator.Validate(e.purchasedProduct.receipt);
                    // For informational purposes, we list the receipt(s)
                    Debug.Log("Receipt is valid. Contents:");
                    foreach (IPurchaseReceipt productReceipt in result)
                    {
                        //Debug.Log(productReceipt.productID);
                        //Debug.Log(productReceipt.purchaseDate);
                        //Debug.Log(productReceipt.transactionID);
                    }
                }
                catch (IAPSecurityException)
                {
                    MKLog.Log("Invalid receipt, not unlocking content");
                    validPurchase = false;
                }
            }
#endif

            if (!validPurchase)
            {
                //Invalid Purchase, bad reciept
                OnPurchaseFailed(e.purchasedProduct, PurchaseFailureReason.SignatureInvalid);
                return PurchaseProcessingResult.Complete;
            }

            string productID = e.purchasedProduct.definition.id;
            productID = productID.Replace(DISCOUNT_SUFFIX, "");

            SetItemPurchased(productID);
            SavePurchases();

            //Record store purchase with actual id
            MKAnalytics.StorePurchase(UnityEngine.Analytics.Experimental.StoreType.Premium, e.purchasedProduct.definition.id, currentPurchaseLocation);

            //Inform all listeners
            foreach (IPurchaseListener listener in purchaseListeners)
            {
                listener.OnPurchaseSuccess(productID);
            }

            //Inform all listeners
            foreach (IPurchaseEventsListener listener in purchaseEventsListeners)
            {
                listener.OnPurchaseSuccess(productID);
            }

            //Complete one off callbacks
            if (onSuccessOneOff != null)
            {
                onSuccessOneOff(productID);
            }

            //Clear one off callbacks
            onFailureOneOff = null;
            onSuccessOneOff = null;

            MKLog.Log("Purchase Complete for " + e.purchasedProduct.definition.id);

            if (restoringPurchases)
            {
                restoredPurchaseCount++;
            }

            return PurchaseProcessingResult.Complete;
        }

        /// <summary>
        /// Marks the IAP manager as ready to process purchases and then completes pending transactions
        /// Only be call after all purchase listeners have been added
        /// </summary>
        public static void ReadyAndProcessPendingPurchases()
        {
            //Already called
            if (ReadyToProcessPurchases)
                return;

            MKLog.Log("Processing " + pendingPurchases.Count + " purchases");

            if (purchaseListeners.Count == 0 && purchaseEventsListeners.Count == 0)
            {
                MKLog.Log("WARNING! There are no purchase listeners, are you calling ready too early?", "yellow");
            }

            ReadyToProcessPurchases = true;

            //Completes pending purchases
            foreach (var purchase in pendingPurchases)
            {
                _instance.ProcessPurchase(purchase);
            }

            pendingPurchases.Clear();
        }
    }
}