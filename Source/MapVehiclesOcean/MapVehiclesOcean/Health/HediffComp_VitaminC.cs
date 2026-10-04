using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class HediffComp_VitaminC : HediffComp
{
	protected HediffCompProperties_VitaminC Props => (HediffCompProperties_VitaminC)props;
	
	public void Notify_IngestedThing(Thing ingested, float amount)
	{
		var sources = Props.vitaminSources;
		
		// 生食材を食べた場合
		foreach (var vitaminSource in sources)
		{
			if (vitaminSource.source == ingested.def)
			{
				parent.Severity -= vitaminSource.amount * amount;
				return;
			}
		}
		
		// 料理を食べた場合 (材料のビタミン量の平均)
		if (ingested.TryGetComp<CompIngredients>() is { } ingredients)
		{
			var totalVitamin = 0f;
			foreach (var ingredient in ingredients.ingredients)
			{
				foreach (var vitaminSource in sources)
				{
					if (vitaminSource.source == ingredient)
					{
						totalVitamin += vitaminSource.amount;
						break;
					}
				}
			}
			if (totalVitamin > 0f)
			{
				parent.Severity -= totalVitamin / ingredients.ingredients.Count * amount;
			}
		}
	}
}