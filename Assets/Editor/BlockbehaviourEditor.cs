﻿using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(BlockBehaviour))]
public class BlockBehaviourEditor : Editor {

	public static void CreateNeighborExlusionUI(Slot slot) {
		var style = new GUIStyle();

		for (int i = 0; i < 6; i++) {
			GUILayout.Space(10f);
			style.normal.textColor = getColor(i);
			var neighbor = slot.GetNeighbor(i);

			GUILayout.Label(Orientations.Names[i], style);
			if (neighbor == null || !neighbor.Collapsed) {
				GUILayout.Label("(No neighbor)");
				continue;
			}

			GUILayout.Label(neighbor.Module.ToString());

			if (neighbor.Module == null) {
				continue;
			}

			var ownFace = slot.Module.GetFace(i);
			var neighborFace = neighbor.Module.GetFace((i + 3) % 6);

			if (ownFace.ExcludedNeighbours.Contains(neighbor.Module.Prototype) && neighborFace.ExcludedNeighbours.Contains(slot.Module.Prototype)) {
				GUILayout.Label("(Already exlcuded)");
				continue;
			}

			if (GUILayout.Button("Exclude neighbor")) {
				if (!ownFace.ExcludedNeighbours.Contains(neighbor.Module.Prototype)) {
					ownFace.ExcludedNeighbours = ownFace.ExcludedNeighbours.Concat(new ModulePrototype[] { neighbor.Module.Prototype }).ToArray();
				}
				if (!neighborFace.ExcludedNeighbours.Contains(slot.Module.Prototype)) {
					neighborFace.ExcludedNeighbours = neighborFace.ExcludedNeighbours.Concat(new ModulePrototype[] { slot.Module.Prototype }).ToArray();
				}
				Debug.Log("Added exclusion rule.");
			}

			if (neighborFace.Walkable) {
				GUILayout.Label("(Neighbor is walkable)");
				continue;
			}

			if (ownFace.EnforceWalkableNeighbor && !neighborFace.Walkable) {
				GUILayout.Label("(Already exlcuded by walkability constraint)");
				continue;
			}

			if (!ownFace.EnforceWalkableNeighbor && !neighborFace.Walkable && GUILayout.Button("Enforce Walkable neighbor")) {
				ownFace.EnforceWalkableNeighbor = true;
			}
		}
	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		BlockBehaviour block = (BlockBehaviour)target;
		if (block.Slot == null || Application.isPlaying) {
			return;
		}

		BlockBehaviourEditor.CreateNeighborExlusionUI(block.Slot);
	}

	private static Color getColor(int i) {
		switch (i) {
			case 0: return Color.red;
			case 1: return Color.green;
			case 2: return Color.blue;
			case 3: return Color.red;
			case 4: return Color.green;
			case 5: return Color.blue;
			default: throw new System.NotImplementedException();
		}
	}
}