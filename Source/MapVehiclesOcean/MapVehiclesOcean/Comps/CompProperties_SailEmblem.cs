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

  public Dictionary<string, Texture2D> AllEmblems
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
            foreach (var (path, texture) in GetEmblemsInFolder(folderPath))
            {
              field[path] = texture;
            }
          }
        }

        if (texturePaths is not null)
        {
          foreach (var texturePath in texturePaths)
          {
            field[texturePath] = ContentFinder<Texture2D>.Get(texturePath);
          }
        }
      }

      return field;
    }
  }
  
  private static IEnumerable<(string path, Texture2D texture)> GetEmblemsInFolder(string folderPath)
  {
    var normalizedFolderPath = folderPath.TrimEnd('/', '\\').Replace('\\', '/');
    var folderPrefix = normalizedFolderPath + "/";
    var mods = LoadedModManager.RunningModsListForReading;
    var contentPath = GenFilePaths.ContentPath<Texture2D>();
    var modsDir = Path.Combine("Assets", "Data");

    for (var i = mods.Count - 1; i >= 0; --i)
    {
      var mod = mods[i];

      var holder = mod.GetContentHolder<Texture2D>();
      if (holder?.contentList != null)
      {
        foreach (var (path, texture) in holder.contentList)
        {
          if (path.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase))
          {
            yield return (path, texture);
          }
        }
      }

      if (mod.assetBundles?.loadedAssetBundles != null)
      {
        var validExtensions = ModAssetBundlesHandler.TextureExtensions;

        for (var j = 0; j < mod.assetBundles.loadedAssetBundles.Count; ++j)
        {
          var assetBundle = mod.assetBundles.loadedAssetBundles[j];
          var trie = mod.AllAssetNamesInBundleTrie(j);
          if (trie == null) continue;

          List<string> rootPaths = [];
          
          var rootFolderName = Path.Combine(Path.Combine(modsDir, mod.FolderName), contentPath).Replace('\\', '/');
          if (!rootFolderName.EndsWith("/")) rootFolderName += "/";
          rootPaths.Add(rootFolderName);

          if (!mod.IsOfficialMod)
          {
            var rootPackageId = Path.Combine(Path.Combine(modsDir, mod.PackageIdPlayerFacing), contentPath).Replace('\\', '/');
            if (!rootPackageId.EndsWith("/")) rootPackageId += "/";
            rootPaths.Add(rootPackageId);
          }

          foreach (var rootPath in rootPaths)
          {
            var bundleSearchPrefix = (rootPath + folderPrefix).ToLower();

            foreach (var fullAssetName in trie.GetByPrefix(bundleSearchPrefix))
            {
              var ext = Path.GetExtension(fullAssetName);
              if (validExtensions.Contains(ext))
              {
                if (fullAssetName.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
                {
                  var itemPathWithExt = fullAssetName[rootPath.Length..];
                  var itemPath = itemPathWithExt[..^ext.Length];

                  var texture = assetBundle.LoadAsset<Texture2D>(fullAssetName);
                  if (texture != null)
                  {
                    yield return (itemPath, texture);
                  }
                }
              }
            }
          }
        }
      }
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