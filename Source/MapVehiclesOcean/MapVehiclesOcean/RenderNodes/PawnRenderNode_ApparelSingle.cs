using Verse;

namespace MapVehiclesOcean;

public class PawnRenderNode_ApparelSingle(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree renderTree)
  : PawnRenderNode(pawn, props, renderTree)
{
  public override Graphic GraphicFor(Pawn pawn)
  {
    return GraphicDatabase.Get<Graphic_Single>(props.texPath, props.shaderTypeDef.Shader,
      apparel.DrawSize ,apparel.DrawColor);
  }
}