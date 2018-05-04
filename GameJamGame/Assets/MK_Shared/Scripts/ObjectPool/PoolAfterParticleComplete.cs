using UnityEngine;
using System.Collections;


[RequireComponent(typeof(ParticleSystem))]
public class PoolAfterParticleComplete : Poolable
{
	// If true, deactivate the object instead of destroying it
	public bool OnlyDeactivate;
	
	void OnEnable()
	{
		StartCoroutine(CheckIfAlive());
	}
	
	IEnumerator CheckIfAlive ()
	{
		ParticleSystem ps = this.GetComponent<ParticleSystem>();
		
		while(true && ps != null)
		{
			yield return new WaitForSeconds(0.2f);

            if (!ps.IsAlive(true))
			{
                if (OnlyDeactivate)
                {
                    this.gameObject.SetActive(false);
                }
                else
                {
                    Pool();
                }
				break;
			}
		}
	}
}
