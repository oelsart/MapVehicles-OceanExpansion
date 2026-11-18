using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MapVehiclesOcean;

public class WorldDrawLayer_UpliftedTerrain : WorldDrawLayer
{
	public void AppendUpliftedTiles(List<(PlanetTile tile, int distance)> targetTiles)
	{
		var tileIDToVerts_offsets = planetLayer.UnsafeTileIDToVerts_offsets;
		var verts = planetLayer.UnsafeVerts;

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

			var subMesh = GetSubMesh(material);

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
		}

		FinalizeMesh(MeshParts.All);
	}

	public void Clear()
	{
		RegenerateNow();
		subMeshes.Clear();
	}
}