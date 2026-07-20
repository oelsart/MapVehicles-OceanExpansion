using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using PawnKindDef = Verse.PawnKindDef;

namespace MapVehiclesOcean;

public static class AggressiveSeaAnimalIncidentUtility
{
  public static bool TryFindAggressiveAnimalKind(float points, PlanetTile tile, out PawnKindDef animalKind)
  {
    if (TryGetHabitatAnimal(tile, out animalKind))
    {
      return true;
    }

    var polluted = ModsConfig.BiotechActive &&
                   tile.Valid &&
                   Rand.Value <
                   WildAnimalSpawner.PollutionAnimalSpawnChanceFromPollutionCurve.Evaluate(Find.WorldGrid[tile]
                     .pollution);
    var list = (from k in tile.Tile.Biomes.SelectMany(b => b.AllWildAnimals)
      where CanArriveWithPollution(k, tile.Tile, polluted) && CanArriveManhunter(k)
      select k).ToList();
    if (polluted && list.Count == 0)
    {
      polluted = false;
      list = (from k in tile.Tile.Biomes.SelectMany(b => b.AllWildAnimals)
        where CanArriveWithPollution(k, tile.Tile, polluted) && CanArriveManhunter(k)
        select k).ToList();
    }

    return TryGetAnimalFromList(points, list, out animalKind) ||
           TryGetAnimalFromList(points, DefDatabase<PawnKindDef>.AllDefs
               .Where(k => CanArriveManhunter(k) &&
                           CanArriveWithPollution(k, tile.Tile, polluted) &&
                           (!tile.Valid ||
                            Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)))
               .ToList(),
             out animalKind);
  }

  private static bool TryGetAnimalFromList(float points, List<PawnKindDef> animals, out PawnKindDef animalKind)
  {
    if (animals.Any())
    {
      if (animals.TryRandomElementByWeight(a => AggressiveAnimalIncidentUtility.AnimalWeight(a, points),
            out animalKind))
      {
        return true;
      }

      if (points > animals.Min(a => a.combatPower) * 2f)
      {
        animalKind = animals.MaxBy(a => a.combatPower);
        return true;
      }
    }

    animalKind = null;
    return false;
  }

  private static bool TryGetHabitatAnimal(PlanetTile tile, out PawnKindDef animalKind)
  {
    animalKind = null;
    if (!ModsConfig.OdysseyActive)
    {
      return false;
    }

    if (!tile.Valid)
    {
      return false;
    }

    if (Find.WorldGrid[tile].Mutators.Contains(TileMutatorDefOf.AnimalHabitat))
    {
      animalKind = ((TileMutatorWorker_AnimalHabitat)TileMutatorDefOf.AnimalHabitat.Worker).GetAnimalKind(tile);
      if (CanArriveManhunter(animalKind) && Rand.Chance(0.5f))
      {
        return true;
      }
    }

    return false;
  }

  private static bool CanArriveManhunter(PawnKindDef kind)
  {
    // Nephropsを登場させるため, RaceProps.AnimalではなくHumanlikeでないことを確認している
    if (!kind.RaceProps.Humanlike && kind.canArriveManhunter)
    {
      return kind.RaceProps.CanPassFences;
    }

    return false;
  }

  private static bool CanArriveWithPollution(PawnKindDef k, Tile tile, bool polluted)
  {
    if (polluted)
    {
      return tile.Biomes.Any(b => b.CommonalityOfPollutionAnimal(k) > 0f);
    }

    return tile.IsCoastal
      ? tile.Biomes.Any(b => b.CommonalityOfAnimal(k) > 0f || b.CommonalityOfCoastalAnimal(k) > 0f)
      : tile.Biomes.Any(b => b.CommonalityOfAnimal(k) > 0f);
  }
}