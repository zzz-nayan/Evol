namespace Evol;

using Atmosphere = Creature.Atmosphere;

/// <summary>
/// Container and planetary simulator for the Earth environment, its living creatures, and botanical flora.
/// </summary>
public class EarthWorld
{
    public string Name { get; set; } = "Earth";
    public Atmosphere Atmosphere { get; set; } = Atmosphere.StandardEarth;
    public float TimeOfDayHours { get; set; } = 12f;
    public long DayCount { get; set; } = 1;
    public List<Creature> Population { get; } = new();
    public List<Plant> Flora { get; } = new();

    public void Add(Creature creature) => Population.Add(creature);
    public void PlantFlora(Plant plant) => Flora.Add(plant);

    public void StepSimulation(float hours = 1.0f)
    {
        long daysPassed = (long)(hours / 24f);
        float remainingHours = hours % 24f;
        DayCount += daysPassed;

        TimeOfDayHours += remainingHours;
        if (TimeOfDayHours >= 24f)
        {
            TimeOfDayHours %= 24f;
            DayCount++;
        }

        // Day/night temperature oscillation
        float tempVariance = MathF.Sin((TimeOfDayHours / 24f) * MathF.PI * 2f - MathF.PI / 2f) * 6f;
        Atmosphere.TemperatureCelsius = 15f + tempVariance;

        // 1. Flora Photosynthesis & Growth (replenishes oxygen)
        for (int i = Flora.Count - 1; i >= 0; i--)
        {
            var plant = Flora[i];
            plant.Water(0.1f * hours); // Natural soil groundwater & dew absorption
            plant.Tick(hours, Atmosphere);
        }

        // 2. Creature Metabolism, Respiration, Gestation & Life Cycle
        List<Creature> newborns = new();
        for (int i = Population.Count - 1; i >= 0; i--)
        {
            var creature = Population[i];

            // Biosphere Sustenance: wild animals drink and forage in nature
            if (creature.IsAlive)
            {
                if (creature.Thirst > 40f) creature.Drink(new Food.FreshWater());
                if (creature.Hunger > 45f)
                {
                    if (creature is Creature.Horse || creature is Creature.Elephant || creature is Creature.Reptile.Testudines)
                        creature.Eat(new Food.Spinach());
                    else if (creature is Creature.Cat || creature is Creature.Dog || creature is Creature.Bear ||
                             creature is Creature.Reptile.Crocodilia || creature is Creature.Reptile.Squamata.Serpentes ||
                             creature is Creature.Fish.Chondrichthyes || creature is Creature.Cetacean.Odontoceti)
                        creature.Eat(new Food.BeefSteak());
                    else
                        creature.Eat(new Food.Apple());
                }

                // Cold-blooded reptiles bask in sun during daylight hours
                if (creature is Creature.Reptile rep && TimeOfDayHours >= 8f && TimeOfDayHours <= 18f)
                {
                    rep.BaskInSun(hours * 0.25f, Atmosphere.TemperatureCelsius);
                }
            }

            creature.Tick(hours, Atmosphere);

            // Natural childbirth when mother reaches full term / in labor
            if (creature.Womb != null && creature.Womb.InLabor)
            {
                var baby = creature.GiveBirth();
                if (baby != null)
                {
                    newborns.Add(baby);
                    creature.Nurse(baby); // Mother nurses newborn baby with breast milk!
                }
            }
        }

        // Welcome all new births into the living Earth population
        foreach (var baby in newborns)
        {
            Add(baby);
        }

        // 3. Planetary Atmospheric Buffers (Global oceans, phytoplankton & continental air currents)
        Atmosphere.OxygenMoles = Math.Clamp(Atmosphere.OxygenMoles, 20500f, 21500f);
        Atmosphere.CarbonDioxideMoles = Math.Clamp(Atmosphere.CarbonDioxideMoles, 35f, 65f);
        Atmosphere.WaterVaporMoles = Math.Clamp(Atmosphere.WaterVaporMoles, 900f, 1200f);
    }

    public (int Total, int Living, int Humans, int Cats, int Dogs, int Birds, int Fish, int Reptiles, int Arthropods, int Other) GetCensus()
    {
        int total = Population.Count;
        int living = Population.Count(c => c.IsAlive);
        int humans = Population.Count(c => c is Creature.Human && c.IsAlive);
        int cats = Population.Count(c => c is Creature.Cat && c.IsAlive);
        int dogs = Population.Count(c => c is Creature.Dog && c.IsAlive);
        int birds = Population.Count(c => c is Creature.Bird && c.IsAlive);
        int fish = Population.Count(c => c is Creature.Fish && c.IsAlive);
        int reptiles = Population.Count(c => c is Creature.Reptile && c.IsAlive);
        int arthropods = Population.Count(c => c is Creature.Arthropod && c.IsAlive);
        int other = living - (humans + cats + dogs + birds + fish + reptiles + arthropods);

        return (total, living, humans, cats, dogs, birds, fish, reptiles, arthropods, other);
    }

    public static EarthWorld CreatePrimordialEarth()
    {
        var earth = new EarthWorld();

        // 1. Primordial Botanical Flora (Garden of Eden Trees, Cover & Crops)
        earth.PlantFlora(new Plant.AppleTree());
        earth.PlantFlora(new Plant.OakTree());
        earth.PlantFlora(new Plant.StrawberryBush());
        earth.PlantFlora(new Plant.WildGrass());
        earth.PlantFlora(new Plant.WheatCrop());
        earth.PlantFlora(new Plant.Moss());

        // 2. Primordial Human Ancestors (Adam & Eve)
        var adam = new Creature.Human(null, null, "Adam", Creature.SexType.Male);
        var eve = new Creature.Human(null, null, "Eve", Creature.SexType.Female);
        earth.Add(adam);
        earth.Add(eve);

        // 3. Cats
        var tomcat = new Creature.Cat(null, null, "Leo", Creature.SexType.Male);
        var queen = new Creature.Cat(null, null, "Luna", Creature.SexType.Female);
        earth.Add(tomcat);
        earth.Add(queen);

        // 4. Dogs & Wolves
        earth.Add(new Creature.Dog(null, null, "Buddy", Creature.SexType.Male));
        earth.Add(new Creature.Wolf(null, null, Creature.SexType.Male));

        // 5. Avian & Aquatic
        earth.Add(new Creature.Bird(null, null, "Golden Eagle", Creature.SexType.Female));
        earth.Add(new Creature.Fish(null, null, "Atlantic Salmon", Creature.SexType.Female));

        // 6. Herbivores
        earth.Add(new Creature.Horse(null, null, "Spirit", Creature.SexType.Female));

        return earth;
    }

    /// <summary>
    /// Alias class nested inside the master class so you can write EarthWorld.Earth
    /// </summary>
    public class Earth : EarthWorld
    {
    }
}
