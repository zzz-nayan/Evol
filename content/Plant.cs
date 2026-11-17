namespace Evol;

/// <summary>
/// Root abstract base class for all flora, botanical life, and photosynthetic organisms.
/// </summary>
public abstract class Plant
{
    public enum GrowthStage
    {
        Seed,
        Sprout,
        Sapling,
        Mature,
        Flowering,
        Fruiting,
        Withering,
        DeadWood
    }

    public string Name { get; protected set; }
    public GrowthStage Stage { get; protected set; } = GrowthStage.Sapling;
    public float AgeDays { get; protected set; } = 10f;
    public float MaxLifespanDays { get; protected set; } = 365f;
    public float HeightMeters { get; protected set; } = 1.0f;
    public float CanopyRadiusMeters { get; protected set; } = 0.8f;
    public float Health { get; protected set; } = 100f;
    public float MaxHealth { get; protected set; } = 100f;
    public float Hydration { get; protected set; } = 80f; // 0 to 100
    public float PrivacyAndCover { get; protected set; } = 0.5f; // 0 (exposed) to 1 (dense romantic seclusion)
    public bool ProvidesShade => CanopyRadiusMeters >= 1.5f && Stage >= GrowthStage.Mature;
    public bool IsAlive => Health > 0f && Stage != GrowthStage.DeadWood;

    // --- Real Chemical Elemental & Molecular Composition ---
    public Element.Compound.OrganicCompound.ChlorophyllPigment Chlorophyll { get; protected set; } = new();
    public float GlucoseReserveMoles { get; protected set; } = 2.0f;
    public float AbsorbedCo2MolesTotal { get; protected set; } = 0f;
    public float ReleasedOxygenMolesTotal { get; protected set; } = 0f;

    protected static readonly Random _rng = new Random();

    public Plant(string name, float heightMeters = 1.0f, float canopyRadius = 0.8f, float maxLifespanDays = 365f, float privacy = 0.5f)
    {
        Name = name;
        HeightMeters = heightMeters;
        CanopyRadiusMeters = canopyRadius;
        MaxLifespanDays = maxLifespanDays;
        PrivacyAndCover = privacy;
    }

    /// <summary>
    /// Photosynthesis & growth tick: absorbs CO2 & H2O via Chlorophyll, synthesizes Glucose, releases O2!
    /// Biochemical Photosynthesis: 6 CO2 + 6 H2O + Sunlight -> C6H12O6 + 6 O2
    /// </summary>
    public virtual void Tick(float deltaHours, Creature.Atmosphere? env = null)
    {
        if (!IsAlive) return;

        env ??= Creature.Atmosphere.StandardEarth;

        float deltaDays = deltaHours / 24f;
        AgeDays += deltaDays;

        // Sunlight & temperature photosynthesis efficiency
        float sunEfficiency = Math.Clamp(env.TemperatureCelsius / 25f, 0.2f, 1.2f);
        float extentMoles = 0.02f * HeightMeters * (CanopyRadiusMeters / 1.0f) * sunEfficiency * deltaHours;

        // Stoichiometry: 6 moles CO2 and 6 moles H2O per mole of Glucose synthesized
        float co2Needed = extentMoles * 6f;
        float co2Absorbed = env.ConsumeCarbonDioxide(co2Needed);
        float actualExtent = co2Absorbed / 6f;

        if (actualExtent > 0f)
        {
            GlucoseReserveMoles += actualExtent;
            AbsorbedCo2MolesTotal += co2Absorbed;

            // Stoichiometric release of molecular oxygen (6 O2 per glucose mole)
            float o2Produced = actualExtent * 6f;
            env.ProduceOxygen(o2Produced);
            ReleasedOxygenMolesTotal += o2Produced;
        }

        // Water consumption for cellular metabolism & transpiration
        float h2oUsedMoles = actualExtent * 6f;
        float h2oUsedGrams = h2oUsedMoles * 18.015f;
        float hydrationLoss = (h2oUsedGrams / 500f) + (0.5f * deltaDays);
        Hydration = Math.Max(0f, Hydration - hydrationLoss);
        if (Hydration <= 0f)
        {
            Health = Math.Max(0f, Health - 5f * deltaDays);
            if (Health <= 0f) Stage = GrowthStage.DeadWood;
        }

        // Natural growth progression
        UpdateGrowthStage(deltaDays);
    }

    protected virtual void UpdateGrowthStage(float deltaDays)
    {
        float ratio = AgeDays / Math.Max(1f, MaxLifespanDays);
        if (ratio < 0.05f) Stage = GrowthStage.Sprout;
        else if (ratio < 0.20f) Stage = GrowthStage.Sapling;
        else if (ratio < 0.70f) Stage = GrowthStage.Mature;
        else if (ratio < 0.85f) Stage = GrowthStage.Flowering;
        else if (ratio < 0.95f) Stage = GrowthStage.Fruiting;
        else if (ratio < 1.0f) Stage = GrowthStage.Withering;
        else
        {
            Stage = GrowthStage.DeadWood;
            Health = 0f;
        }
    }

    public virtual Food? Harvest() => null;

    public virtual void Water(float amountLiters)
    {
        Hydration = Math.Clamp(Hydration + amountLiters * 10f, 0f, 100f);
        Health = Math.Clamp(Health + amountLiters * 2f, 0f, MaxHealth);
    }

    // =========================================================================
    // LAYER 1: BOTANICAL TAXONOMY / STEM & TISSUE HIERARCHY
    // =========================================================================

    public class Seed(string plantName, float daysToGerminate = 5f)
    {
        public string TargetPlantName { get; } = plantName;
        public float DaysIncubated { get; private set; } = 0f;
        public float RequiredDays { get; } = daysToGerminate;
        public bool IsSprouted => DaysIncubated >= RequiredDays;

        public void Incubate(float days) => DaysIncubated += days;
    }

    public abstract class NonVascularPlant : Plant
    {
        public float SporeCount { get; protected set; } = 1000f;

        public NonVascularPlant(string name, float heightMeters = 0.05f, float cover = 0.2f)
            : base(name, heightMeters, canopyRadius: 0.2f, maxLifespanDays: 180f, privacy: cover)
        {
        }
    }

    public abstract class VascularPlant : Plant
    {
        public float XylemWaterFlowRate { get; protected set; } = 1.0f;
        public float RootDepthMeters { get; protected set; } = 1.0f;

        public VascularPlant(string name, float heightMeters, float canopyRadius, float maxLifespanDays, float privacy, float rootDepth)
            : base(name, heightMeters, canopyRadius, maxLifespanDays, privacy)
        {
            RootDepthMeters = rootDepth;
        }
    }

    // =========================================================================
    // LAYER 2: GROWTH HABIT (HERBACEOUS VS WOODY TREES)
    // =========================================================================

    public abstract class HerbaceousPlant : VascularPlant
    {
        public bool HasThornyDefenses { get; protected set; } = false;

        public HerbaceousPlant(string name, float heightMeters, float canopyRadius, float maxLifespanDays, float privacy, float rootDepth)
            : base(name, heightMeters, canopyRadius, maxLifespanDays, privacy, rootDepth)
        {
        }
    }

    public abstract class WoodyPlant : VascularPlant
    {
        public float TrunkDiameterMeters { get; protected set; } = 0.3f;
        public float WoodHardness { get; protected set; } = 50f;
        public bool CanBeClimbed { get; protected set; } = true;

        public WoodyPlant(string name, float heightMeters, float canopyRadius, float maxLifespanDays, float privacy, float rootDepth, float trunkDiameter)
            : base(name, heightMeters, canopyRadius, maxLifespanDays, privacy, rootDepth)
        {
            TrunkDiameterMeters = trunkDiameter;
        }
    }

    // =========================================================================
    // LAYER 3: VEGETATIVE & CANOPY FAMILIES
    // =========================================================================

    // --- Ground Plants & Cover ---

    public class Moss : NonVascularPlant
    {
        public Moss() : base("Velvet Moss Carpet", heightMeters: 0.03f, cover: 0.1f)
        {
            // Provides a soft natural bed on the earth floor
            PrivacyAndCover = 0.2f;
        }
    }

    public abstract class Grass : HerbaceousPlant
    {
        public float GrazingNutritionPerSquareMeter { get; protected set; } = 50f;

        public Grass(string name, float heightMeters, float nutrition)
            : base(name, heightMeters, canopyRadius: 0.3f, maxLifespanDays: 120f, privacy: 0.2f, rootDepth: 0.2f)
        {
            GrazingNutritionPerSquareMeter = nutrition;
        }
    }

    public abstract class BushAndShrub : HerbaceousPlant
    {
        public int HarvestableFoodCount { get; protected set; } = 12;

        public BushAndShrub(string name, float heightMeters, float canopyRadius, float privacy, bool thorny = false)
            : base(name, heightMeters, canopyRadius, maxLifespanDays: 730f, privacy, rootDepth: 0.8f)
        {
            HasThornyDefenses = thorny;
        }
    }

    public abstract class VegetableCrop : HerbaceousPlant
    {
        public bool IsRipe => Stage >= GrowthStage.Mature;

        public VegetableCrop(string name, float heightMeters)
            : base(name, heightMeters, canopyRadius: 0.4f, maxLifespanDays: 100f, privacy: 0.15f, rootDepth: 0.5f)
        {
        }
    }

    // --- Trees (Canopy, Timber, Seclusion) ---

    public abstract class DeciduousTree : WoodyPlant
    {
        public bool ShedsLeavesInWinter { get; protected set; } = true;

        public DeciduousTree(string name, float heightMeters, float canopyRadius, float maxLifespanDays, float privacy, float trunkDiameter)
            : base(name, heightMeters, canopyRadius, maxLifespanDays, privacy, rootDepth: 3.5f, trunkDiameter)
        {
        }
    }

    public abstract class ConiferousTree : WoodyPlant
    {
        public float PineResinLevel { get; protected set; } = 0.8f;

        public ConiferousTree(string name, float heightMeters, float canopyRadius, float maxLifespanDays, float privacy, float trunkDiameter)
            : base(name, heightMeters, canopyRadius, maxLifespanDays, privacy, rootDepth: 4.0f, trunkDiameter)
        {
            CanBeClimbed = true;
        }
    }

    public abstract class FruitTree : DeciduousTree
    {
        public int ReadyFruitCount { get; protected set; } = 25;

        public FruitTree(string name, float heightMeters, float canopyRadius, float trunkDiameter)
            : base(name, heightMeters, canopyRadius, maxLifespanDays: 365f * 50f, privacy: 0.85f, trunkDiameter)
        {
            Stage = GrowthStage.Fruiting;
        }
    }

    // =========================================================================
    // LAYER 4: CONCRETE EVERYDAY EARTH PLANTS & TREES!
    // =========================================================================

    public class WildGrass : Grass
    {
        public WildGrass() : base("Meadow Wildgrass", heightMeters: 0.4f, nutrition: 75f) { }
    }

    public class WheatCrop : Grass
    {
        public WheatCrop() : base("Golden Wheat", heightMeters: 0.9f, nutrition: 120f) { }

        public override Food? Harvest()
        {
            if (Stage >= GrowthStage.Mature)
            {
                GlucoseReserveMoles = Math.Max(0.1f, GlucoseReserveMoles - 0.2f);
                return new Food.WheatBread(); // Grains ground and baked into bread
            }
            return null;
        }
    }

    public class StrawberryBush : BushAndShrub
    {
        public StrawberryBush() : base("Strawberry Bush", heightMeters: 0.6f, canopyRadius: 0.8f, privacy: 0.6f)
        {
            Stage = GrowthStage.Fruiting;
        }

        public override Food? Harvest()
        {
            if (HarvestableFoodCount > 0)
            {
                HarvestableFoodCount--;
                GlucoseReserveMoles = Math.Max(0.1f, GlucoseReserveMoles - 0.05f);
                return new Food.Strawberry();
            }
            return null;
        }
    }

    public class BlueberryBush : BushAndShrub
    {
        public BlueberryBush() : base("Wild Blueberry Bush", heightMeters: 1.2f, canopyRadius: 1.4f, privacy: 0.75f)
        {
            Stage = GrowthStage.Fruiting;
        }

        public override Food? Harvest()
        {
            if (HarvestableFoodCount > 0)
            {
                HarvestableFoodCount--;
                GlucoseReserveMoles = Math.Max(0.1f, GlucoseReserveMoles - 0.05f);
                return new Food.Blueberry();
            }
            return null;
        }
    }

    public class RoseBush : BushAndShrub
    {
        public RoseBush() : base("Thorny Wild Rose Bush", heightMeters: 1.5f, canopyRadius: 1.6f, privacy: 0.9f, thorny: true)
        {
            // High privacy, but thorny!
        }
    }

    public class CarrotCrop : VegetableCrop
    {
        public CarrotCrop() : base("Carrot Patch", heightMeters: 0.35f)
        {
            Stage = GrowthStage.Mature;
        }

        public override Food? Harvest() => new Food.Carrot();
    }

    public class PotatoCrop : VegetableCrop
    {
        public PotatoCrop() : base("Potato Bed", heightMeters: 0.5f)
        {
            Stage = GrowthStage.Mature;
        }

        public override Food? Harvest() => new Food.Potato();
    }

    // --- Trees: The Ultimate Shelter, Food & Romantic Seclusion ---

    public class AppleTree : FruitTree
    {
        public AppleTree() : base("Grand Apple Tree", heightMeters: 7.5f, canopyRadius: 5.0f, trunkDiameter: 0.65f)
        {
            // Perfect Garden of Eden Apple Tree: massive shade, thick foliage for privacy, drops apples!
            PrivacyAndCover = 0.95f;
        }

        public override Food? Harvest()
        {
            if (ReadyFruitCount > 0)
            {
                ReadyFruitCount--;
                GlucoseReserveMoles = Math.Max(0.1f, GlucoseReserveMoles - 0.1f);
                return new Food.Apple();
            }
            return null;
        }
    }

    public class OrangeTree : FruitTree
    {
        public OrangeTree() : base("Valencia Orange Tree", heightMeters: 6.0f, canopyRadius: 4.2f, trunkDiameter: 0.5f)
        {
            PrivacyAndCover = 0.90f;
        }

        public override Food? Harvest()
        {
            if (ReadyFruitCount > 0)
            {
                ReadyFruitCount--;
                GlucoseReserveMoles = Math.Max(0.1f, GlucoseReserveMoles - 0.1f);
                return new Food.Orange();
            }
            return null;
        }
    }

    public class OakTree : DeciduousTree
    {
        public OakTree() : base("Ancient Majestic Oak", heightMeters: 18.0f, canopyRadius: 12.0f, maxLifespanDays: 365f * 300f, privacy: 0.98f, trunkDiameter: 1.8f)
        {
            // Monumental tree: provides dense shade, rain protection, and secluded privacy
            Stage = GrowthStage.Mature;
        }
    }

    public class PineTree : ConiferousTree
    {
        public PineTree() : base("Evergreen Mountain Pine", heightMeters: 15.0f, canopyRadius: 3.5f, maxLifespanDays: 365f * 150f, privacy: 0.80f, trunkDiameter: 0.7f)
        {
            Stage = GrowthStage.Mature;
        }
    }
}
