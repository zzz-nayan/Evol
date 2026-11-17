namespace Evol;

/// <summary>
/// Root abstract base for all ingestible nutritional matter.
/// </summary>
public abstract class Food
{
    public string Name { get; protected set; }
    public float Calories { get; protected set; }
    public float WaterContentGrams { get; protected set; }
    public float SugarGrams { get; protected set; }
    public float ProteinGrams { get; protected set; }
    public float FatGrams { get; protected set; }
    public float FiberGrams { get; protected set; }
    public float ToxinAmount { get; protected set; }
    public float FreshnessPercent { get; protected set; } = 100f;
    public bool IsCooked { get; protected set; } = false;
    public bool IsPerishable { get; protected set; } = true;

    // --- Real Chemical Elemental & Molecular Composition ---
    public float GlucoseMoles => SugarGrams / 180.156f;
    public float WaterMoles => WaterContentGrams / 18.015f;
    public virtual float SodiumChlorideGrams { get; protected set; } = 0.5f;
    public virtual float CalciumMg { get; protected set; } = 15f;
    public virtual float IronMg { get; protected set; } = 1.0f;
    public float SodiumChlorideMoles => SodiumChlorideGrams / 58.44f;
    public float CalciumMoles => (CalciumMg / 1000f) / 40.078f;
    public float IronMoles => (IronMg / 1000f) / 55.845f;

    public Food(string name, float calories, float waterGrams = 0f, float sugarGrams = 0f, float proteinGrams = 0f, float fatGrams = 0f, float fiberGrams = 0f, float toxinAmount = 0f)
    {
        Name = name;
        Calories = calories;
        WaterContentGrams = waterGrams;
        SugarGrams = sugarGrams;
        ProteinGrams = proteinGrams;
        FatGrams = fatGrams;
        FiberGrams = fiberGrams;
        ToxinAmount = toxinAmount;
    }

    public virtual void Spoil(float days)
    {
        if (!IsPerishable) return;
        FreshnessPercent = Math.Max(0f, FreshnessPercent - days * 20f);
        if (FreshnessPercent <= 0f)
        {
            ToxinAmount += 15f; // Bacterial growth & rot
            Name = $"Rotten {Name}";
        }
    }

    public virtual Food Cook()
    {
        if (IsCooked) return this;
        IsCooked = true;
        Name = $"Cooked {Name}";
        Calories *= 1.2f; // Thermal cooking breaks down starches & proteins for bioavailability
        ToxinAmount = 0f; // Heat denatures bacteria, salmonella & parasites
        return this;
    }

    // =========================================================================
    // LAYER 1: BIOLOGICAL KINGDOM / ORIGIN CLASSIFICATION
    // =========================================================================

    public abstract class PlantBasedFood : Food
    {
        public float VitaminC_Mg { get; protected set; }
        public float ChlorophyllLevel { get; protected set; } = 0.5f;
        public bool HasSeeds { get; protected set; } = false;

        public PlantBasedFood(string name, float calories, float waterGrams, float sugarGrams, float proteinGrams, float fatGrams, float fiberGrams, float vitaminC = 0f)
            : base(name, calories, waterGrams, sugarGrams, proteinGrams, fatGrams, fiberGrams)
        {
            VitaminC_Mg = vitaminC;
        }
    }

    public abstract class AnimalBasedFood : Food
    {
        public float CholesterolMg { get; protected set; }
        public bool RequiresCooking { get; protected set; } = true;

        public AnimalBasedFood(string name, float calories, float waterGrams, float sugarGrams, float proteinGrams, float fatGrams, float cholesterolMg = 50f, float rawToxin = 5f)
            : base(name, calories, waterGrams, sugarGrams, proteinGrams, fatGrams, fiberGrams: 0f, toxinAmount: rawToxin)
        {
            CholesterolMg = cholesterolMg;
        }
    }

    public abstract class ProcessedFood : Food
    {
        public float ProcessingTier { get; protected set; } = 1f;
        public bool HasPreservatives { get; protected set; } = false;

        public ProcessedFood(string name, float calories, float waterGrams, float sugarGrams, float proteinGrams, float fatGrams, float fiberGrams = 0f, float processingTier = 1f)
            : base(name, calories, waterGrams, sugarGrams, proteinGrams, fatGrams, fiberGrams)
        {
            ProcessingTier = processingTier;
        }
    }

    public abstract class Beverage : Food
    {
        public float ElectrolytesMg { get; protected set; }

        public Beverage(string name, float calories, float waterGrams, float sugarGrams = 0f, float electrolytesMg = 10f)
            : base(name, calories, waterGrams, sugarGrams, proteinGrams: 0f, fatGrams: 0f, fiberGrams: 0f)
        {
            ElectrolytesMg = electrolytesMg;
            IsPerishable = false;
        }
    }

    // =========================================================================
    // LAYER 2: BOTANICAL, ANATOMICAL & CULINARY GROUPINGS
    // =========================================================================

    // --- Plant Branch ---

    public abstract class Fruit : PlantBasedFood
    {
        public float FructoseRatio { get; protected set; } = 0.7f;

        public Fruit(string name, float calories, float waterGrams, float sugarGrams, float fiberGrams, float vitaminC)
            : base(name, calories, waterGrams, sugarGrams, proteinGrams: 0.8f, fatGrams: 0.2f, fiberGrams, vitaminC)
        {
            HasSeeds = true;
        }
    }

    public abstract class Vegetable : PlantBasedFood
    {
        public bool IsRootCrop { get; protected set; } = false;

        public Vegetable(string name, float calories, float waterGrams, float fiberGrams, float proteinGrams = 1.5f, float vitaminC = 10f)
            : base(name, calories, waterGrams, sugarGrams: 2.0f, proteinGrams, fatGrams: 0.1f, fiberGrams, vitaminC)
        {
        }
    }

    public abstract class GrainAndCereal : PlantBasedFood
    {
        public float StarchGrams { get; protected set; }

        public GrainAndCereal(string name, float calories, float carbsGrams, float proteinGrams, float fiberGrams)
            : base(name, calories, waterGrams: 10f, sugarGrams: 1f, proteinGrams, fatGrams: 1.5f, fiberGrams)
        {
            StarchGrams = carbsGrams;
            IsPerishable = false;
        }
    }

    // --- Animal Branch ---

    public abstract class Meat : AnimalBasedFood
    {
        public Meat(string name, float calories, float proteinGrams, float fatGrams, float ironMg, float rawToxin = 6f)
            : base(name, calories, waterGrams: 60f, sugarGrams: 0f, proteinGrams, fatGrams, cholesterolMg: 75f, rawToxin)
        {
            IronMg = ironMg;
            SodiumChlorideGrams = 1.0f;
        }
    }

    public abstract class Seafood : AnimalBasedFood
    {
        public float Omega3Grams { get; protected set; }

        public Seafood(string name, float calories, float proteinGrams, float fatGrams, float omega3)
            : base(name, calories, waterGrams: 70f, sugarGrams: 0f, proteinGrams, fatGrams, cholesterolMg: 55f, rawToxin: 4f)
        {
            Omega3Grams = omega3;
            SodiumChlorideGrams = 1.5f;
            IronMg = 1.5f;
            CalciumMg = 30f;
        }
    }

    public abstract class Dairy : AnimalBasedFood
    {
        public float LactoseGrams { get; protected set; }

        public Dairy(string name, float calories, float waterGrams, float sugarGrams, float proteinGrams, float fatGrams, float calciumMg)
            : base(name, calories, waterGrams, sugarGrams, proteinGrams, fatGrams, cholesterolMg: 25f, rawToxin: 1f)
        {
            CalciumMg = calciumMg;
            LactoseGrams = sugarGrams;
            SodiumChlorideGrams = 0.8f;
        }
    }

    public abstract class EggFood : AnimalBasedFood
    {
        public float CholineMg { get; protected set; }

        public EggFood(string name, float calories, float proteinGrams, float fatGrams, float cholineMg)
            : base(name, calories, waterGrams: 40f, sugarGrams: 0.5f, proteinGrams, fatGrams, cholesterolMg: 185f, rawToxin: 3f)
        {
            CholineMg = cholineMg;
        }
    }

    // --- Processed Branch ---

    public abstract class BakedGood : ProcessedFood
    {
        public bool IsLeavened { get; protected set; } = true;

        public BakedGood(string name, float calories, float carbGrams, float fatGrams, float proteinGrams)
            : base(name, calories, waterGrams: 35f, sugarGrams: 4f, proteinGrams, fatGrams, fiberGrams: 2.5f, processingTier: 2f)
        {
        }
    }

    public abstract class Confectionery : ProcessedFood
    {
        public Confectionery(string name, float calories, float sugarGrams, float fatGrams)
            : base(name, calories, waterGrams: 5f, sugarGrams, proteinGrams: 1f, fatGrams, fiberGrams: 0.5f, processingTier: 3f)
        {
            IsPerishable = false;
        }
    }

    // =========================================================================
    // LAYER 3: SPECIALIZED FAMILIES & TYPES
    // =========================================================================

    public abstract class TreeFruit : Fruit
    {
        public TreeFruit(string name, float calories, float waterGrams, float sugarGrams, float fiberGrams, float vitaminC)
            : base(name, calories, waterGrams, sugarGrams, fiberGrams, vitaminC)
        {
        }
    }

    public abstract class PomeFruit : TreeFruit
    {
        public PomeFruit(string name, float calories, float waterGrams, float sugarGrams, float fiberGrams, float vitaminC)
            : base(name, calories, waterGrams, sugarGrams, fiberGrams, vitaminC)
        {
        }
    }

    public abstract class CitrusFruit : TreeFruit
    {
        public float CitricAcidGrams { get; protected set; }

        public CitrusFruit(string name, float calories, float waterGrams, float sugarGrams, float fiberGrams, float vitaminC, float citricAcid)
            : base(name, calories, waterGrams, sugarGrams, fiberGrams, vitaminC)
        {
            CitricAcidGrams = citricAcid;
        }
    }

    public abstract class BerryFruit : Fruit
    {
        public BerryFruit(string name, float calories, float waterGrams, float sugarGrams, float fiberGrams, float vitaminC)
            : base(name, calories, waterGrams, sugarGrams, fiberGrams, vitaminC)
        {
        }
    }

    public abstract class RootVegetable : Vegetable
    {
        public float StarchGrams { get; protected set; }

        public RootVegetable(string name, float calories, float waterGrams, float starchGrams, float fiberGrams)
            : base(name, calories, waterGrams, fiberGrams)
        {
            IsRootCrop = true;
            StarchGrams = starchGrams;
        }
    }

    public abstract class LeafyGreen : Vegetable
    {
        public LeafyGreen(string name, float calories, float waterGrams, float fiberGrams, float ironMg)
            : base(name, calories, waterGrams, fiberGrams, proteinGrams: 2.5f, vitaminC: 25f)
        {
            IronMg = ironMg;
        }
    }

    public abstract class RedMeat : Meat
    {
        public RedMeat(string name, float calories, float proteinGrams, float fatGrams, float ironMg)
            : base(name, calories, proteinGrams, fatGrams, ironMg, rawToxin: 6f)
        {
        }
    }

    public abstract class PoultryMeat : Meat
    {
        public PoultryMeat(string name, float calories, float proteinGrams, float fatGrams)
            : base(name, calories, proteinGrams, fatGrams, ironMg: 1.2f, rawToxin: 8f) // High raw salmonella risk
        {
        }
    }

    public abstract class Bread : BakedGood
    {
        public Bread(string name, float calories, float carbGrams, float proteinGrams)
            : base(name, calories, carbGrams, fatGrams: 2f, proteinGrams)
        {
        }
    }

    public abstract class Pastry : BakedGood
    {
        public Pastry(string name, float calories, float sugarGrams, float fatGrams)
            : base(name, calories, carbGrams: 30f, fatGrams, proteinGrams: 4f)
        {
            SugarGrams = sugarGrams;
        }
    }

    public abstract class Chocolate : Confectionery
    {
        public float CocoaPercentage { get; protected set; }

        public Chocolate(string name, float calories, float sugarGrams, float fatGrams, float cocoaPercent)
            : base(name, calories, sugarGrams, fatGrams)
        {
            CocoaPercentage = cocoaPercent;
        }
    }

    // =========================================================================
    // LAYER 4: CONCRETE EVERYDAY FOOD ITEMS!
    // =========================================================================

    // --- Fruits ---

    public class Apple : PomeFruit
    {
        public Apple() : base("Apple", calories: 95f, waterGrams: 85f, sugarGrams: 19f, fiberGrams: 4.4f, vitaminC: 8.4f) { }
    }

    public class Pear : PomeFruit
    {
        public Pear() : base("Pear", calories: 101f, waterGrams: 84f, sugarGrams: 17f, fiberGrams: 5.5f, vitaminC: 7.5f) { }
    }

    public class Orange : CitrusFruit
    {
        public Orange() : base("Orange", calories: 62f, waterGrams: 87f, sugarGrams: 12f, fiberGrams: 3.1f, vitaminC: 70f, citricAcid: 1.5f) { }
    }

    public class Lemon : CitrusFruit
    {
        public Lemon() : base("Lemon", calories: 29f, waterGrams: 89f, sugarGrams: 2.5f, fiberGrams: 2.8f, vitaminC: 53f, citricAcid: 4.8f) { }
    }

    public class Strawberry : BerryFruit
    {
        public Strawberry() : base("Strawberry", calories: 49f, waterGrams: 91f, sugarGrams: 7f, fiberGrams: 3.0f, vitaminC: 89f) { }
    }

    public class Blueberry : BerryFruit
    {
        public Blueberry() : base("Blueberry", calories: 84f, waterGrams: 84f, sugarGrams: 15f, fiberGrams: 3.6f, vitaminC: 14f) { }
    }

    // --- Vegetables ---

    public class Carrot : RootVegetable
    {
        public Carrot() : base("Carrot", calories: 41f, waterGrams: 88f, starchGrams: 9.6f, fiberGrams: 2.8f) { }
    }

    public class Potato : RootVegetable
    {
        public Potato() : base("Potato", calories: 163f, waterGrams: 75f, starchGrams: 37f, fiberGrams: 4.7f) { }
    }

    public class Spinach : LeafyGreen
    {
        public Spinach() : base("Spinach", calories: 23f, waterGrams: 91f, fiberGrams: 2.2f, ironMg: 2.7f) { }
    }

    public class Lettuce : LeafyGreen
    {
        public Lettuce() : base("Lettuce", calories: 15f, waterGrams: 96f, fiberGrams: 1.3f, ironMg: 0.9f) { }
    }

    // --- Meats ---

    public class BeefSteak : RedMeat
    {
        public BeefSteak() : base("Beef Steak", calories: 679f, proteinGrams: 62f, fatGrams: 48f, ironMg: 6.5f) { }
    }

    public class PorkChop : RedMeat
    {
        public PorkChop() : base("Pork Chop", calories: 257f, proteinGrams: 26f, fatGrams: 16f, ironMg: 1.1f) { }
    }

    public class ChickenBreast : PoultryMeat
    {
        public ChickenBreast() : base("Chicken Breast", calories: 284f, proteinGrams: 53f, fatGrams: 6.2f) { }
    }

    public class TurkeyLeg : PoultryMeat
    {
        public TurkeyLeg() : base("Turkey Leg", calories: 414f, proteinGrams: 56f, fatGrams: 18f) { }
    }

    public class SalmonFillet : Seafood
    {
        public SalmonFillet() : base("Salmon Fillet", calories: 412f, proteinGrams: 40f, fatGrams: 27f, omega3: 4.5f) { }
    }

    // --- Dairy & Eggs ---

    public class CowMilk : Dairy
    {
        public CowMilk() : base("Cow Milk", calories: 149f, waterGrams: 215f, sugarGrams: 12f, proteinGrams: 8f, fatGrams: 8f, calciumMg: 276f) { }
    }

    public class CheddarCheese : Dairy
    {
        public CheddarCheese() : base("Cheddar Cheese", calories: 402f, waterGrams: 37f, sugarGrams: 1.3f, proteinGrams: 25f, fatGrams: 33f, calciumMg: 721f) { }
    }

    public class ChickenEgg : EggFood
    {
        public ChickenEgg() : base("Chicken Egg", calories: 72f, proteinGrams: 6.3f, fatGrams: 4.8f, cholineMg: 147f) { }
    }

    // --- Baked Goods & Confectionery ---

    public class WheatBread : Bread
    {
        public WheatBread() : base("Wheat Bread", calories: 138f, carbGrams: 26f, proteinGrams: 6f) { }
    }

    public class Croissant : Pastry
    {
        public Croissant() : base("Croissant", calories: 231f, sugarGrams: 6f, fatGrams: 12f) { }
    }

    public class Lollipop : Confectionery
    {
        public Lollipop() : base("Lollipop", calories: 50f, sugarGrams: 13f, fatGrams: 0f) { }
    }

    public class MilkChocolateBar : Chocolate
    {
        public MilkChocolateBar() : base("Milk Chocolate Bar", calories: 535f, sugarGrams: 52f, fatGrams: 30f, cocoaPercent: 35f) { }
    }

    public class DarkChocolateBar : Chocolate
    {
        public DarkChocolateBar() : base("Dark Chocolate Bar", calories: 546f, sugarGrams: 14f, fatGrams: 43f, cocoaPercent: 85f) { }
    }

    // --- Beverages ---

    public class FreshWater : Beverage
    {
        public FreshWater() : base("Fresh Water", calories: 0f, waterGrams: 250f, sugarGrams: 0f, electrolytesMg: 15f) { }
    }

    public class OrangeJuice : Beverage
    {
        public OrangeJuice() : base("Orange Juice", calories: 112f, waterGrams: 220f, sugarGrams: 21f, electrolytesMg: 45f) { }
    }
}
