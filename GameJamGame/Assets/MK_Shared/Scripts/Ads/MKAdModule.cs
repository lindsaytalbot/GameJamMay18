using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An abstract class that contains all the required functions to Fetch and Show Ads.
/// You should create a new module for each ad provider and override the 
/// </summary>
namespace MightyKingdom
{
    public abstract class MKAdModule
    {
        protected string placementID;
        protected bool fetching = false;
        protected Action adWatchSuccessCallback;
        protected Action<string> adWatchFailureCallback;

        public MKAdModule()
        {
            Configure();
            Fetch();
        }

        /// <summary>
        /// Calls all the necessary configurations methods required to run this module
        /// </summary>
        //This is where you should put all your logic for initializing the providers ad service
        //and settings it's listeners
        public abstract void Configure();

        /// <summary>
        /// Returns true if an ad is available from this provider
        /// </summary>
        /// <returns></returns>
        public abstract bool AdAvailable();

        /// <summary>
        /// Returns true if called on a compatible platform
        /// </summary>
        /// <returns></returns>
        public virtual bool IsCompatiblePlatform()
        {
            return true;
        }

        /// <summary>
        /// Starts a fetch if no ad is avaiable or if force is true
        /// </summary>
        public void Fetch(bool forceFetch = false)
        { 
            if (!AdAvailable() || forceFetch || !fetching)
            {
                fetching = true;
                FetchAd();
            }
        }

        /// <summary>
        /// Actually calls this providers fetch method
        /// </summary>
        protected abstract void FetchAd();

        protected virtual void OnFetchSuccess()
        {
            fetching = false;
        }

        /// <summary>
        /// The fetch failed. This happens often enough. Call another in 30 seconds
        /// </summary>
        protected virtual void OnFetchFailed()
        {
            fetching = false;
            CoroutineHelper.Instance.StartCoroutine(DelayedFetch());
        }

        /// <summary>
        /// Calls Fetch after 30 seconds. Generally called after a fetch has failed
        /// </summary>
        /// <returns></returns>
        protected IEnumerator DelayedFetch()
        {
            yield return new WaitForSeconds(30);
            Fetch();
        }

        /// <summary>
        /// Show an ad from this provider
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        public void ShowAd(string placementID, Action onSuccess, Action<string> onFailure)
        {
            this.placementID = placementID;
            adWatchSuccessCallback = onSuccess;
            adWatchFailureCallback = onFailure;
            MKAnalytics.AdStart(placementID, GetType().Name);
            ShowAd();
        }

        /// <summary>
        /// Calls the providers ShowAd function
        /// </summary>
        protected abstract void ShowAd();

        /// <summary>
        /// Calls the adWatchSuccessCallback set in ShowAd
        /// </summary>
        protected virtual void OnAdWatchSuccess()
        {
            MKAdManager.OnAdWatchComplete();
            MKAnalytics.AdComplete(placementID, GetType().Name);
            if(adWatchSuccessCallback != null)
                adWatchSuccessCallback();
        }

        /// <summary>
        /// Calls the adWatchFailureCallback set in ShowAd
        /// </summary>
        protected virtual void OnAdWatchFailure(string error)
        {
            MKAdManager.OnAdWatchComplete();
            if(adWatchFailureCallback != null)
                adWatchFailureCallback(error);
        }

        /// <summary>
        /// Show this ad providers test suite
        /// </summary>
        public abstract void ShowTestSuite();
    }
}