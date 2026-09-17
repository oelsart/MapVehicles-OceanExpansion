using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using RimWorld.Planet;

namespace MapVehiclesOcean;

public static class WorldTerrainUpdater
{
	private static Func<Material, LayerSubMesh> getSubMeshFunc;

	public static void Clear(WorldDrawLayer_Terrain layer)
	{
		layer.RegenerateNow();
		getSubMeshFunc = null;
	}

	public static void AppendUpliftedTiles(WorldDrawLayer_Terrain layer, List<(PlanetTile tile, int distance)> targetTiles)
	{
		getSubMeshFunc ??= AccessTools.MethodDelegate<Func<Material, LayerSubMesh>>(
			AccessTools.Method(typeof(WorldDrawLayerBase), "GetSubMesh", [typeof(Material)]), layer);

		var planetLayer = layer.planetLayer;
		var tileIDToVerts_offsets = planetLayer.UnsafeTileIDToVerts_offsets;
		var verts = planetLayer.UnsafeVerts;

		HashSet<LayerSubMesh> dirtySubMeshes = [];

		foreach (var (pTile, _) in targetTiles)
		{
			var i = pTile.tileId;
			var tile = planetLayer[i];

			var material = tile.PrimaryBiome.DrawMaterial;
			var landmark = tile.Landmark;
			if (landmark != null && landmark.def.drawType == LandmarkDef.LandmarkDrawType.TerrainMask)
			{
				material = MaterialPool.MatFrom(new MaterialRequest(material.mainTexture, material.shader)
				{
					maskTex = landmark.def.Texture,
					secondaryTex = planetLayer.Def.backgroundBiome.DrawMaterial.mainTexture,
					renderQueue = material.renderQueue,
					shaderParameters = landmark.def.terrainParameters
				});
			}

			var subMesh = getSubMeshFunc(material);

			var startOffset = tileIDToVerts_offsets[i];
			var nextOffset = i + 1 < tileIDToVerts_offsets.Length ? tileIDToVerts_offsets[i + 1] : verts.Length;
			var vertCount = nextOffset - startOffset;

			var baseVertCount = subMesh.verts.Count;

			for (var j = startOffset; j < nextOffset; j++)
			{
				var v = verts[j];
				subMesh.verts.Add(v + v.normalized * 0.01f);
				subMesh.uvs.Add(new Vector3(tile.elevation, tile.elevation, 0f));
			}

			for (var j = 0; j < vertCount - 2; j++)
			{
				subMesh.tris.Add(baseVertCount + j + 2);
				subMesh.tris.Add(baseVertCount + j + 1);
				subMesh.tris.Add(baseVertCount);
			}

			dirtySubMeshes.Add(subMesh);
		}

		foreach (var subMesh in dirtySubMeshes)
		{
			subMesh.mesh.Clear();
			subMesh.mesh.SetVertices(subMesh.verts);
			subMesh.mesh.SetUVs(0, subMesh.uvs);
			subMesh.mesh.SetTriangles(subMesh.tris, 0);
			subMesh.mesh.RecalculateNormals();
		}
	}
}