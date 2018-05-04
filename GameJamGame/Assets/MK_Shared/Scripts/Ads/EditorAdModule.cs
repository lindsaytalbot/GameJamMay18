using MightyKingdom;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorAdModule : MKAdModule
{
    bool adAvailable;
    float failureChance = .2f;

    public override bool AdAvailable()
    {
        return adAvailable;
    }

    public override void Configure()
    {
        //nothing to do here
    }

    public override void ShowTestSuite()
    {
        //nothing to do here
    }

    protected override void FetchAd()
    {
        CoroutineHelper.Instance.StartCoroutine(FakeFetch());
    }

    protected IEnumerator FakeFetch()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(2,5));
        adAvailable = true;
        OnFetchSuccess();
    }

    protected override void ShowAd()
    {
        adAvailable = false;
        CoroutineHelper.Instance.StartCoroutine(FakeShow());
        MKLog.Log("Showing fake ad");
    }

    protected IEnumerator FakeShow()
    {
        yield return new WaitForSecondsRealtime(5);

        if (UnityEngine.Random.Range(0, 1f) > failureChance)
            OnAdWatchSuccess();
        else
            OnAdWatchFailure("Fake Show Ad failure");
    }
}
