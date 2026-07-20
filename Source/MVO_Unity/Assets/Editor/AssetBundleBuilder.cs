using System;
using System.IO;
using System.Threading.Tasks;
using OELSMods;
using UnityEditor;
using UnityEngine;

namespace OELSMods
{
  public static class ModIds
  {
    public const string MapVehiclesOcean = "OELS.MapVehicles.OceanExpansion";
  }
}

namespace SmashTools
{
    public class AssetBundleBuilder : MonoBehaviour
    {
        private const string TextureFolderName = "Textures";
        private const string SoundFolderName = "Sounds";

        private const string ShaderFileName = "Shaders";

        // RimWorld stores Shaders in Materials/ so asset bundle paths have to match it for their
        // loader to be able to find the content.
        private const string ShaderFolderName = "Materials";
        private const string MeshFolderName = "Materials";

        private const string OutputPath = "../../Common/AssetBundles";

        private const string DefaultBundleName = "AssetBundles";

        private static readonly BuildTarget[] BuildTargets =
        {
            BuildTarget.StandaloneWindows64, BuildTarget.StandaloneOSX, BuildTarget.StandaloneLinux64
        };

        private static string PlatformSuffix(BuildTarget buildTarget)
        {
            return buildTarget switch
            {
                BuildTarget.StandaloneWindows64 => "_win",
                BuildTarget.StandaloneOSX => "_mac",
                BuildTarget.StandaloneLinux64 => "_linux",
                _ => throw new NotSupportedException(buildTarget.ToString())
            };
        }

        private static string[] GetAssetPaths<T>(string packageId)
        {
            string folderName = FolderName();

            string[] guids =
                AssetDatabase.FindAssets($"t:{typeof(T).Name}",
                    new[]
                    {
                        $"Assets/Data/{packageId}/{folderName}"
                    });

            string[] paths = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                paths[i] = path;
            }
            return paths;

            string FolderName()
            {
                if (typeof(T) == typeof(Texture2D))
                    return TextureFolderName;
                if (typeof(T) == typeof(AudioClip))
                    return SoundFolderName;
                if (typeof(T) == typeof(Shader))
                    return ShaderFolderName;
                if (typeof(T) == typeof(Mesh))
                    return MeshFolderName;

                throw new NotImplementedException();
            }
        }

        [MenuItem("Assets/Build AssetBundles/MapVehiclesOcean")]
        private static void BuildAssetBundles()
        {
            BuildForMod(ModIds.MapVehiclesOcean, OutputPath);
        }
        
        [MenuItem("Assets/Build AssetBundles/MapVehiclesOcean - Shaders")]
        private static void BuildShaderAssetBundles()
        {
            BuildShaders(ModIds.MapVehiclesOcean, OutputPath);
        }

        public static void BuildForMod(string packageId, string outputPath)
        {
            const string TextureBundleName = "oels_mapvehiclesocean_textures";
            const string SoundBundleName = "oels_mapvehiclesocean_sounds";

            // Start fresh for build folder
            if (!Directory.Exists(outputPath))
                throw new DirectoryNotFoundException(outputPath);

            Directory.Delete(outputPath, true);
            Directory.CreateDirectory(outputPath);

            // Platform independent
            AssetBundleBuild[] textureBundles = new AssetBundleBuild[1];
            textureBundles[0].assetBundleName = TextureBundleName;
            textureBundles[0].assetNames = GetAssetPaths<Texture2D>(packageId);

            BuildPipeline.BuildAssetBundles(outputPath,
                textureBundles,
                BuildAssetBundleOptions.ChunkBasedCompression,
                BuildTarget.StandaloneWindows64);
            
            AssetBundleBuild[] soundBundles = new AssetBundleBuild[1];
            soundBundles[0].assetBundleName = SoundBundleName;
            soundBundles[0].assetNames = GetAssetPaths<AudioClip>(packageId);

            BuildPipeline.BuildAssetBundles(outputPath,
                soundBundles,
                BuildAssetBundleOptions.ChunkBasedCompression,
                BuildTarget.StandaloneWindows64);

            BuildShaders(packageId, outputPath);
        }

        private static void BuildShaders(string packageId, string outputPath)
        {
            const string ShaderBundleName = "oels_mapvehiclesocean_shaders";
            const string MeshBundleName = "oels_mapvehiclesocean_meshes";
            // Platform dependent
            AssetBundleBuild[] platformBundles = new AssetBundleBuild[2];
            platformBundles[0].assetBundleName = ShaderBundleName;
            platformBundles[0].assetNames = GetAssetPaths<Shader>(packageId);
            platformBundles[1].assetBundleName = MeshBundleName;
            platformBundles[1].assetNames = GetAssetPaths<Mesh>(packageId);

            BuildForPlatform(outputPath,
                platformBundles,
                BuildAssetBundleOptions.ChunkBasedCompression);
            
            if (File.Exists($"{outputPath}/{DefaultBundleName}"))
                File.Delete($"{outputPath}/{DefaultBundleName}");
            if (File.Exists($"{outputPath}/{DefaultBundleName}.manifest"))
                File.Delete($"{outputPath}/{DefaultBundleName}.manifest");
        }

        private static void BuildForPlatform(string directoryPath, AssetBundleBuild[] bundles,
            BuildAssetBundleOptions bundleOptions)
        {
            foreach (BuildTarget buildTarget in BuildTargets)
            {
                AssetBundleBuild[] platformBundles =
                    new AssetBundleBuild[bundles.Length];
                for (int i = 0; i < bundles.Length; i++)
                {
                    AssetBundleBuild bundle = bundles[i];
                    AssetBundleBuild platformBundle = bundle;
                    platformBundle.assetBundleName = bundle.assetBundleName + PlatformSuffix(buildTarget);
                    platformBundles[i] = platformBundle;
                }
                BuildPipeline.BuildAssetBundles(directoryPath, platformBundles, bundleOptions, buildTarget);
            }
        }
    }
}