using UnityEngine;
using VehicleMapFramework;
using VehicleMapFramework.VMF_HarmonyPatches;
using Vehicles;
using Verse;

namespace MapVehiclesOcean;

[StaticConstructorOnStartup]
public sealed class CompSailEmblem : ThingComp
{
  private static Material material;
  private static MaterialPropertyBlock propertyBlock;
  private static readonly Mesh[] meshes = new Mesh[4];
  private static readonly CachedTexture changeColorTex = new("UI/Commands/ChangeColor");

  private Texture2D emblem;
  private string texturePath;
  private Color color = Color.white;
  
  static CompSailEmblem()
  {
    LongEventHandler.ExecuteWhenFinished(() =>
    {
      propertyBlock = new MaterialPropertyBlock();
      material = new Material(MaterialPool.MatFrom(ShaderDatabase.Transparent))
      {
        renderQueue = 2901
      };
      meshes[0] = LoadMesh("SailEmblem_north");
      meshes[1] = LoadMesh("SailEmblem_east");
      meshes[2] = LoadMesh("SailEmblem_south");
      meshes[3] = LoadMesh("SailEmblem_west");
    });
    return;

    Mesh LoadMesh(string name)
    {
      const string path = "Assets/Data/OELS.MapVehicles.OceanExpansion/Materials/MapVehiclesOcean";
      var name2 = $"{name}.asset";
      foreach (var bundle in MapVehiclesOcean.Mod.Content.assetBundles.loadedAssetBundles)
      {
        if (bundle.LoadAsset<Mesh>(Path.Combine(path, name2)) is { } mesh)
        {
          return mesh;
        }
      }
      throw new InvalidOperationException($"[MapVehiclesOcean] Mesh {name2} not found.");
    }
  }
  
  private CompProperties_SailEmblem Props => (CompProperties_SailEmblem)props;
  
  private CompDrawAdditionalGraphicsOpacity RigGraphicComp => field ??= parent.GetComp<CompDrawAdditionalGraphicsOpacity>();

  public override IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    yield return new Command_Action
    {
      defaultLabel = "MVO_SailEmblemChange".Translate(),
      defaultDesc = "MVO_SailEmblemChangeDesc".Translate(),
      icon = emblem ?? BaseContent.ClearTex,
      action = () =>
      {
        List<FloatMenuGridOption> options =
        [
          new(BaseContent.ClearTex, () =>
          {
            texturePath = null;
            emblem = null;
          })
        ];
        foreach (var tuple in Props.AllEmblems)
        {
          options.Add(new FloatMenuGridOption(tuple.texture, () =>
          {
            texturePath = tuple.path;
            emblem = tuple.texture;
          }, color));
        }
        Find.WindowStack.Add(new FloatMenuGrid(options));
      }
    };
    yield return new Command_ColorIcon
    {
      defaultLabel = "MVO_SailEmblemChangeColor".Translate(),
      defaultDesc = "MVO_SailEmblemChangeColorDesc".Translate(),
      icon = changeColorTex.Texture,
      color = color,
      action = () =>
      {
        Find.WindowStack.Add(new Dialog_ColorWheel(color, c => color = c));
      }
    };
  }

  private static Mesh MeshAt(Rot4 rot)
  {
    return meshes[rot.AsInt];
  }

  public override void PostDraw()
  {
    if (RigGraphicComp is null || emblem is null) return;
    if (RigGraphicComp.Opacity == 0f) return;
    
    var loc = parent.DrawPos;
    var rot = parent.BaseRotationVehicleDraw();
    var extraRotation = 0f;
    var graphic = Props.maskGraphicData.Graphic;
    Patch_Graphic_Draw.Prefix(ref loc,
      ref rot,
      parent,
      ref extraRotation,
      graphic);

    var emblemOffset = Props.DrawOffsetForRot(rot);
    if (parent.IsOnVehicleMapOf(out var vehicle))
    {
      var angle = vehicle.ExtraAngle;
      extraRotation += angle;
      var offset = graphic.DrawOffset(rot);
      var offset2 = offset.RotatedBy(angle);
      loc += new Vector3(offset2.x - offset.x, 0f, offset2.z - offset.z);
      emblemOffset = emblemOffset.RotatedBy(angle);
    }

    var mesh = graphic.MeshAt(rot);
    var quaternion = graphic.QuatFromRot(rot);
    if (extraRotation != 0f)
    {
      quaternion *= Quaternion.Euler(Vector3.up * extraRotation);
    }
    if (graphic.data is { addTopAltitudeBias: true })
    {
      quaternion *= Quaternion.Euler(Vector3.left * 2f);
    }
    loc += graphic.DrawOffset(rot);
    var maskMat = graphic.MatAt(rot, parent);
    loc.y += 0.010006f;
    loc.y -= loc.z * 0.00001f;
    loc.y -= loc.x * 0.000001f;
    Graphics.DrawMesh(mesh, loc, quaternion, maskMat, 0);

    loc += emblemOffset;
    loc.y -= 0.000004f;
    var opacity = rot == Rot4.North ? RigGraphicComp.Opacity * 0.2f : RigGraphicComp.Opacity;
    propertyBlock.SetTexture(AdditionalShaderPropertyIDs.MainTex, emblem);
    propertyBlock.SetColor(ShaderPropertyIDs.Color, color.WithAlpha(opacity));
    var matrix = Matrix4x4.TRS(loc, quaternion, new Vector3(0.1f, 1f, 0.1f));
    Graphics.DrawMesh(MeshAt(rot), matrix, material, 0, null, 0, propertyBlock);
  }

  public override void PostExposeData()
  {
    Scribe_Values.Look(ref texturePath, nameof(texturePath));
    Scribe_Values.Look(ref color, nameof(color), Color.white);
    
    if (Scribe.mode == LoadSaveMode.PostLoadInit)
    {
      if (texturePath is not null)
      {
        LongEventHandler.ExecuteWhenFinished(() =>
        {
          emblem = ContentFinder<Texture2D>.Get(texturePath, false);
        });
      }
    }
  }
}