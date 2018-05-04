
using UnityEngine.Purchasing;

/// <summary>
/// Listens for purchase success events from MKIAPManager
/// </summary>
public interface IPurchaseListener
{
    void OnPurchaseSuccess(string itemName);
}

/// <summary>
/// Listens to all purchase events from MKIAPManager
/// </summary>
public interface IPurchaseEventsListener
{
    void OnPurchasesInitialized();
    void OnPurchaseStart(string itemName);
    void OnPurchaseSuccess(string itemName);
    void OnPurchaseFailure(string itemName, PurchaseFailureReason error);
    void OnRestoreStart();
    void OnPurchasesRestored(bool success, int restoreCount);
}