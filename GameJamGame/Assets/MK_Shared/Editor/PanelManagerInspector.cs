using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PanelManager))]
public class PanelManagerInspector : Editor
{
	private PanelManager manager;

	private void OnEnable()
	{
		// Hook into show/hide events.
		manager = target as PanelManager;
		manager.onPanelShown += PanelCallback;
		manager.onPanelHidden += PanelCallback;
	}

	private void OnDisable()
	{
		// unsub from show/hide events.
		manager.onPanelShown -= PanelCallback;
		manager.onPanelHidden -= PanelCallback;
	}

	public override void OnInspectorGUI()
	{
		// Nothing fancy to override here.
		base.OnInspectorGUI();

		// If in play mode, display current active panels so we can see the state of the UI stack.
		if( EditorApplication.isPlaying )
		{
			List<Panel> panels = manager.GetPanelStack();
			EditorGUILayout.LabelField( "Panel Stack", EditorStyles.boldLabel );
			EditorGUILayout.BeginVertical( "box" );
			{
				for( int i = panels.Count-1; i>=0 ; i-- )
				{
					EditorGUILayout.LabelField( string.Format( "{0}: {1}", ( panels.Count - i ), panels[i].name ) );
				}
			}
			EditorGUILayout.EndVertical();
		}
	}

	/// <summary>
	/// Callback for show/hide events used to trigger repaint of the inspector.
	/// </summary>
	/// <param name="panel">Panel that was shown/hidden.</param>
	private void PanelCallback( Panel panel )
	{
		Repaint();
	}
}
