using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace MapVehiclesOcean;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CompProperties_SailEmblem : CompProperties
{
  public List<string> folderPaths;
  public List<string> texturePaths;

  public Vector3 drawOffsetNorth;
  public Vector3 drawOffsetEast;
  public Vector3 drawOffsetSouth;
  public Vector3 drawOffsetWest;

  public GraphicData maskGraphicData;

  public CompProperties_SailEmblem()
  {
    compClass = typeof(CompSailEmblem);
  }

  public List<(string path, Texture2D texture)> AllEmblems
  {
    get
    {
      if (field is null)
      {
        field = [];
        if (folderPaths is not null)
        {
          foreach (var folderPath in folderPaths)
          {
            foreach (var texture in ContentFinder<Texture2D>.GetAllInFolder(folderPath))
            {
              field.Add((Path.Combine(folderPath, texture.name), texture));
            }
          }
        }

        if (texturePaths is not null)
        {
          foreach (var texturePath in texturePaths)
          {
            field.Add((texturePath, ContentFinder<Texture2D>.Get(texturePath)));
          }
        }
      }

      return field;
    }
  }
  
  public Vector3 DrawOffsetForRot(Rot4 rot)
  {
    return rot.AsInt switch
    {
      Rot4.NorthInt => drawOffsetNorth,
      Rot4.EastInt => drawOffsetEast,
      Rot4.SouthInt => drawOffsetSouth,
      Rot4.WestInt => drawOffsetWest,
      _ => Vector3.zero
    };
  }
}