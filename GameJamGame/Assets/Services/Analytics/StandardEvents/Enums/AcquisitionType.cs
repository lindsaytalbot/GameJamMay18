namespace UnityEngine.Analytics.Experimental
{
    /// <summary>
    /// The way in which an item, consumable, or currency was acquired.
    /// </summary>
    public enum AcquisitionType
    {
        /// <summary>Not directly purchased with real-world money.</summary>
        Soft = 0,
        /// <summary>Purchased with real-world money.</summary>
        Premium,
    }
}
