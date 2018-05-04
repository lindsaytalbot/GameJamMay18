using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolAfterSeconds : Poolable
{
    [SerializeField]
    protected float time;

    protected float timeLeft;

	protected void OnEnable()
    {
        timeLeft = time;
    }

    protected void Update()
    {
        timeLeft -= Time.deltaTime;
        if(timeLeft <=0)
        {
            ObjectPool.PoolItem(this);
        }
    }
}
