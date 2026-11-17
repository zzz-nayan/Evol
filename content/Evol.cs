namespace Evol;

public class Evol
{
    public static void Main(string[] args)
    {
        Console.WriteLine("================================================================================");
        Console.WriteLine("                   EVOL: THE LIVING EARTH SIMULATION                            ");
        Console.WriteLine("        Deep Inheritance Hierarchy: Taxonomy, Anatomy, Gestation & Hybrids      ");
        Console.WriteLine("================================================================================");

        // 1. Initialize Primordial Earth
        EarthWorld earth = EarthWorld.CreatePrimordialEarth();
        Console.WriteLine($"\n[Earth Initialized] Biome Atmosphere: {earth.Atmosphere.TemperatureCelsius:F1}°C, {earth.Atmosphere.OxygenPercentage:F2}% O2, Gravity: {earth.Atmosphere.GravityMps2} m/s²");

        var initialCensus = earth.GetCensus();
        Console.WriteLine($"Initial Population: {initialCensus.Total} creatures ({initialCensus.Humans} humans, {initialCensus.Cats} cats, {initialCensus.Dogs} dogs, {initialCensus.Birds} birds, {initialCensus.Fish} fish)");
        Console.WriteLine($"Initial Flora: {earth.Flora.Count} botanical plants & trees planted.");

        // 2. Discovering Flora & Trees: Shade, Privacy & Ecology
        var appleTree = (Plant.AppleTree)earth.Flora.First(p => p is Plant.AppleTree);
        var oakTree = (Plant.OakTree)earth.Flora.First(p => p is Plant.OakTree);
        var strawberryBush = (Plant.StrawberryBush)earth.Flora.First(p => p is Plant.StrawberryBush);
        var meadowGrass = (Plant.WildGrass)earth.Flora.First(p => p is Plant.WildGrass);

        Console.WriteLine($"\n--- Botanical Flora & Forests of Earth ---");
        Console.WriteLine($"Tree 1: {oakTree.Name} (Height={oakTree.HeightMeters}m, CanopyRadius={oakTree.CanopyRadiusMeters}m, PrivacyCover={oakTree.PrivacyAndCover * 100:F0}%)");
        Console.WriteLine($"Tree 2: {appleTree.Name} (Height={appleTree.HeightMeters}m, ApplesReady={appleTree.ReadyFruitCount}, Shade={appleTree.ProvidesShade})");
        Console.WriteLine($"Bush:   {strawberryBush.Name} (BerriesAvailable={strawberryBush.HarvestableFoodCount})");

        // 3. Meet Adam & Eve (Humans)
        var adam = (Creature.Human)earth.Population.First(c => c is Creature.Human && ((Creature.Human)c).Name == "Adam");
        var eve = (Creature.Human)earth.Population.First(c => c is Creature.Human && ((Creature.Human)c).Name == "Eve");

        Console.WriteLine($"\n--- Human Anatomy & Reproductive Organs ---");
        Console.WriteLine($"Adam: Sex={adam.Sex}, BloodType={adam.Genome.BloodType}, EyeColor={adam.Genome.EyeColor}, Teeth={adam.Teeths.Length}");
        Console.WriteLine($"Eve:  Sex={eve.Sex}, BloodType={eve.Genome.BloodType}, EyeColor={eve.Genome.EyeColor}, Teeth={eve.Teeths.Length}");
        Console.WriteLine($"Adam's Male Genitalia (Dick): Length={adam.Dick?.LengthCm:F1}cm, Erect={adam.Dick?.IsErect}, SpermCount={adam.Dick?.SpermCountMillions:F0}M");
        Console.WriteLine($"Eve's Female Genitalia (Pussy): Depth={eve.Pussy?.DepthCm:F1}cm, Fertile={eve.Pussy?.IsFertile}");
        Console.WriteLine($"Eve's Internal Uterus (Womb): Occupied={eve.Womb?.IsOccupied}, Pregnant={eve.IsPregnant}");
        Console.WriteLine($"Eve's Breasts (Boobs): Cup={eve.Boobs?.Cup}, Volume={eve.Boobs?.VolumeCc:F0}cc, Nipples={eve.Boobs?.NippleCount}, Lactating={eve.Boobs?.IsLactating}");

        // Original parameterless method preservation:
        Console.Write("Adam calls original Fuck(): ");
        adam.Fuck();

        // Shelter & Conception of Cain
        adam.RestUnder(oakTree);
        eve.RestUnder(oakTree);
        bool humanConceived = adam.HaveSexWith(eve, "Cain");
        Console.WriteLine($"\n[Human Intercourse] Adam and Eve had sex under {oakTree.Name}. Conception successful? {humanConceived}");
        Console.WriteLine($"Eve's Womb State: Pregnant={eve.IsPregnant}, GestationDays={eve.Womb?.GestationDays:F1}/{eve.Womb?.TargetGestationDays} days");

        // 4. STD Transmission & Medical Treatment Demonstration
        Console.WriteLine($"\n--- STD Transmission & Antibiotic Cure Demonstration ---");
        var patientZero = new Creature.Human(null, null, "Romeo", Creature.SexType.Male);
        var juliet = new Creature.Human(null, null, "Juliet", Creature.SexType.Female);
        patientZero.Genitals.Contract(new Creature.Gonorrhea());
        patientZero.Genitals.Contract(new Creature.Syphilis());
        Console.WriteLine($"Romeo contracted: {string.Join(", ", patientZero.Genitals.ActiveInfections.Select(i => i.Name))}");
        Console.WriteLine($"Juliet STDs before intercourse: {juliet.Genitals.ActiveInfections.Count}");
        patientZero.HaveSexWith(juliet);
        Console.WriteLine($"Juliet STDs after intercourse with Romeo: {string.Join(", ", juliet.Genitals.ActiveInfections.Select(i => i.Name))}");
        juliet.TakeAntibiotics();
        Console.WriteLine($"Juliet took antibiotics! Remaining infections: {juliet.Genitals.ActiveInfections.Count} (Cured: Gonorrhea & Syphilis!)");

        // 5. The Feline (Cat) Family Tree: Pantherinae, Felinae & Hybrids (Crazy Nested Hierarchy!)
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine("       FELINE (CAT) TAXONOMIC HIERARCHY & HYBRID CROSS-BREEDING (LIGER)         ");
        Console.WriteLine("================================================================================");

        var simbaLion = new Creature.Cat.Pantherinae.Lion(null, null, "Simba", Creature.SexType.Male);
        var indiraTiger = new Creature.Cat.Pantherinae.Tiger(null, null, "Indira", Creature.SexType.Female);
        var cheetah = new Creature.Cat.Felinae.Cheetah(null, null, "Flash", Creature.SexType.Male);
        var siamese = new Creature.Cat.Felinae.DomesticCat.Siamese(null, null, "Cleo", Creature.SexType.Female);
        var maineCoon = new Creature.Cat.Felinae.DomesticCat.MaineCoon(null, null, "Thor", Creature.SexType.Male);

        earth.Add(simbaLion);
        earth.Add(indiraTiger);
        earth.Add(cheetah);
        earth.Add(siamese);
        earth.Add(maineCoon);

        Console.WriteLine($"1. Lion: {simbaLion.Roar()} (Type: {simbaLion.GetType().FullName})");
        Console.WriteLine($"2. Tiger: StripeCount={indiraTiger.StripeCount}. Swimming 200m across river...");
        indiraTiger.SwimAcrossRiver(200f);
        Console.WriteLine($"3. Cheetah: TopSpeed={cheetah.TopSpeedKmh} km/h (Semi-retractable sprint claws={cheetah.Hands[0].Fingers[0].Nail.IsRetractable})");
        Console.WriteLine($"4. Siamese Cat: {siamese.Meow()} (Type: {siamese.GetType().FullName})");
        maineCoon.Climb(appleTree);
        Console.WriteLine($"5. Maine Coon: BushyTailLength={maineCoon.Body.Tail?.LengthCm}cm, CoatWaterResistant={maineCoon.WaterResistantFurCoat}");

        // Feline Cross-Breeding: Lion Father + Tiger Mother => Liger!
        Console.WriteLine($"\n[Feline Cross-Breeding] Simba (Lion) mates with Indira (Tiger)...");
        bool ligerConceived = simbaLion.Mate(indiraTiger, "Hercules");
        Console.WriteLine($"Indira conceived hybrid? {ligerConceived}. Target gestation: {indiraTiger.SpeciesGestationDays} days.");

        // 6. The Canine (Dog) Family Tree: Wild Canids, Breeds & Hybrids (Wolfdog)
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine("       CANINE (DOG) TAXONOMIC HIERARCHY & HYBRID CROSS-BREEDING (WOLFDOG)       ");
        Console.WriteLine("================================================================================");

        var ghostWolf = new Creature.Dog.WildCanid.WolfCanid(null, null, "Ghost", Creature.SexType.Male);
        var bellaGsd = new Creature.Dog.DomesticCanid.GermanShepherd(null, null, "Bella", Creature.SexType.Female);
        var husky = new Creature.Dog.DomesticCanid.SiberianHusky(null, null, "Balto", Creature.SexType.Male);
        var bloodhound = new Creature.Dog.DomesticCanid.Bloodhound(null, null, "Sherlock", Creature.SexType.Male);
        var chihuahua = new Creature.Dog.DomesticCanid.Chihuahua(null, null, "Pepe", Creature.SexType.Male);

        earth.Add(ghostWolf);
        earth.Add(bellaGsd);
        earth.Add(husky);
        earth.Add(bloodhound);
        earth.Add(chihuahua);

        Console.WriteLine($"1. Apex Wolf: {ghostWolf.Howl()} (Type: {ghostWolf.GetType().FullName})");
        Console.WriteLine($"2. German Shepherd: Breed={bellaGsd.Breed}, IQ={bellaGsd.Body.Brain.IntelligenceQuotient}, ClampForce={bellaGsd.JawClampForceN}N");
        Console.WriteLine($"3. Siberian Husky: {husky.HowlChorus()}");
        Console.WriteLine($"4. Bloodhound: NoseSensitivity={bloodhound.Body.Nose.OlfactorySensitivity}x!");
        bloodhound.TrackScent(indiraTiger);
        Console.WriteLine($"5. Chihuahua: {chihuahua.Bark()} (Shivering={chihuahua.IsShivering}, Health={chihuahua.Health}HP)");

        // Canine Cross-Breeding: Wolf Father + Domestic Dog Mother => Wolfdog!
        Console.WriteLine($"\n[Canine Cross-Breeding] Ghost (Wolf) mates with Bella (German Shepherd)...");
        bool wolfdogConceived = ghostWolf.Mate(bellaGsd, "Shadow");
        Console.WriteLine($"Bella conceived hybrid? {wolfdogConceived}. Target gestation: {bellaGsd.SpeciesGestationDays} days.");

        // 7. Simulating the 280-Day Gestation Period Across Species
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine("       SIMULATING GESTATION TIMELINE ACROSS SPECIES ON EARTH                    ");
        Console.WriteLine("================================================================================");

        for (int month = 1; month <= 9; month++)
        {
            float daysInMonth = (month == 9) ? 40f : 30f; // Total = 280 days
            for (int d = 0; d < (int)daysInMonth; d++)
            {
                earth.StepSimulation(24f); // 1 full day with automatic biosphere sustenance & gestation
            }

            Console.WriteLine($"[Day {earth.DayCount,3}] Month {month}:");
            Console.WriteLine($"   - Eve (Human):     Gestation={eve.Womb?.GestationDays ?? 280:F0}/280 days | Pregnant={eve.IsPregnant}");
            Console.WriteLine($"   - Indira (Tiger):  Gestation={indiraTiger.Womb?.GestationDays ?? 105:F0}/105 days | Pregnant={indiraTiger.IsPregnant} | Alive={indiraTiger.IsAlive} ({indiraTiger.CauseOfDeath})");
            Console.WriteLine($"   - Bella (GSD):     Gestation={bellaGsd.Womb?.GestationDays ?? 63:F0}/63 days  | Pregnant={bellaGsd.IsPregnant}");
        }

        // 8. Deliveries & Hybrid Offspring Verification
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine("             BIRTH CELEBRATION: DELIVERIES & HYBRID ANALYSIS                    ");
        Console.WriteLine("================================================================================");

        var cain = (Creature.Human?)earth.Population.FirstOrDefault(c => c is Creature.Human && ((Creature.Human)c).Name == "Cain");
        if (cain != null)
        {
            Console.WriteLine($"[1. Human Birth] Cain delivered! Sex={cain.Sex}, BloodType={cain.Genome.BloodType}, Teeth={cain.Teeths.Length} (Dentition: {cain.DentalStage}), Health={cain.Health}%");
            Console.WriteLine($"[Maternal Nursing] Eve nurses Cain with her breasts (Cup {eve.Boobs?.Cup}, {eve.Boobs?.VolumeCc:F0}cc):");
            bool nursed = eve.Nurse(cain);
            Console.WriteLine($"    Cain Nursed={nursed} | Eve Milk Remaining={eve.Boobs?.MilkStorageMl:F1}ml | Cain Thirst={cain.Thirst:F1} | Cain Immunity={cain.Genome.ImmuneStrength:F2}");

            // Biological Teething Progression with Age
            Console.WriteLine($"\n--- Biological Teething Progression Across Human Development ---");
            Console.WriteLine($"1. At Birth (Newborn):    Teeth={cain.Teeths.Length} ({cain.DentalStage}) - Edentulous gums! Ideal for nursing.");
            
            // Age Cain to 6 months: baby teeth erupt!
            cain.AgeBy(0.5f);
            Console.WriteLine($"2. At 6 Months (Infant):  Teeth={cain.Teeths.Length} ({cain.DentalStage}) - {cain.DeciduousTeethCount} primary baby teeth erupted! ({cain.Teeths[0].GetType().Name})");

            // Age Cain to 6 years: baby teeth shed, permanent adult teeth erupt!
            cain.AgeBy(5.5f);
            Console.WriteLine($"3. At 6 Years (Child):    Teeth={cain.Teeths.Length} ({cain.DentalStage}) - Baby teeth shed, {cain.Teeths.Length} permanent adult teeth erupted!");
        }

        var hercules = (Creature.Cat.Pantherinae.Liger?)earth.Population.FirstOrDefault(c => c is Creature.Cat.Pantherinae.Liger);
        if (hercules != null)
        {
            Console.WriteLine($"[2. Feline Hybrid Birth] Hercules the LIGER born!");
            Console.WriteLine($"    Full Type: {hercules.GetType().FullName}");
            Console.WriteLine($"    Species: {hercules.GetType().Name} (Father={hercules.Father?.GetType().Name}, Mother={hercules.Mother?.GetType().Name})");
            Console.WriteLine($"    Gigantism Multiplier: {hercules.GigantismMultiplier}x | Adult Weight Potential: {hercules.WeightKg} kg!");
            Console.WriteLine($"    Call: {hercules.Roar()}");
            indiraTiger.Nurse(hercules);
            Console.WriteLine($"    Maternal Care: Indira nurses Hercules from her {indiraTiger.Boobs?.NippleCount} feline teats! Milk remaining: {indiraTiger.Boobs?.MilkStorageMl:F1}ml");
        }

        var shadow = (Creature.Dog.CanineHybrid.Wolfdog?)earth.Population.FirstOrDefault(c => c is Creature.Dog.CanineHybrid.Wolfdog);
        if (shadow != null)
        {
            Console.WriteLine($"[3. Canine Hybrid Birth] Shadow the WOLFDOG born!");
            Console.WriteLine($"    Full Type: {shadow.GetType().FullName}");
            Console.WriteLine($"    Species: {shadow.GetType().Name} (Father={shadow.Father?.GetType().Name}, Mother={shadow.Mother?.GetType().Name})");
            Console.WriteLine($"    Wolf Blood Percentage: {shadow.WolfBloodPercentage}% | Clamp Force: {shadow.JawClampForceN}N");
            Console.WriteLine($"    Vocalization: {shadow.HowlOrBark()}");
            bellaGsd.Nurse(shadow);
            Console.WriteLine($"    Maternal Care: Bella nurses Shadow from her {bellaGsd.Boobs?.NippleCount} canine teats! Milk remaining: {bellaGsd.Boobs?.MilkStorageMl:F1}ml");
        }

        // 9. REPTILIAN TAXONOMIC HIERARCHY (CRAZY NESTING: SQUAMATA, CROCODILIA, TESTUDINES)
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine("    REPTILIAN CLADE: SQUAMATA (SERPENTES, SAURIA), CROCODILIA & TESTUDINES       ");
        Console.WriteLine("================================================================================");

        var rattlesnake = new Creature.Reptile.Squamata.Serpentes.Viperidae.Rattlesnake(null, null, "Western Diamondback");
        var cobra = new Creature.Reptile.Squamata.Serpentes.Elapidae.KingCobra(null, null, "Naja King");
        var anaconda = new Creature.Reptile.Squamata.Serpentes.Boidae.GreenAnaconda(null, null, "Amazon Titan");
        var komodo = new Creature.Reptile.Squamata.Sauria.Varanidae.KomodoDragon(null, null, "Komodo Sovereign");
        var chameleon = new Creature.Reptile.Squamata.Sauria.Chamaeleonidae.VeiledChameleon(null, null, "Pascal");
        var gecko = new Creature.Reptile.Squamata.Sauria.Gekkonidae.TokayGecko(null, null, "Tokay Climber");
        var alligator = new Creature.Reptile.Crocodilia.Alligatoridae.AmericanAlligator(null, null, "Swamp Behemoth");
        var tortoise = new Creature.Reptile.Testudines.Testudinidae.GalapagosTortoise(null, null, "Lonesome George");

        earth.Add(rattlesnake);
        earth.Add(cobra);
        earth.Add(anaconda);
        earth.Add(komodo);
        earth.Add(chameleon);
        earth.Add(gecko);
        earth.Add(alligator);
        earth.Add(tortoise);

        Console.WriteLine($"1. Viperidae (Rattlesnake): {rattlesnake.RattleTail()}");
        Console.WriteLine($"   Venom Type: {rattlesnake.VenomType} | Yield={rattlesnake.VenomYieldMg}mg");
        Console.WriteLine($"2. Elapidae (King Cobra):   {cobra.FlareHood()}");
        Console.WriteLine($"   Venom Type: {cobra.VenomType}");
        Console.WriteLine($"3. Boidae (Anaconda):       {anaconda.Constrict(new Creature.Dog(null, null, "Stray Dog", Creature.SexType.Male))}");
        Console.WriteLine($"4. Varanidae (Komodo):      {komodo.ToxicBite(new Creature.Fish(null, null, "River Carp", Creature.SexType.Female))}");
        Console.WriteLine($"5. Chamaeleonidae:          {chameleon.Camouflage("#556B2F (Dark Olive Green)")}");
        Console.WriteLine($"6. Gekkonidae (Gecko):      {gecko.WalkOnCeiling()}");
        Console.WriteLine($"7. Crocodilia (Alligator):  {alligator.DeathRoll(new Creature.Fish(null, null, "River Gar", Creature.SexType.Female))}");
        tortoise.RetractIntoShell();
        Console.WriteLine($"8. Testudines (Tortoise):   Shell Retracted={tortoise.IsRetractedInShell} | Expected Longevity={tortoise.MaxLifespanYears:F0} years!");

        // 10. ARTHROPOD CLADE: INSECTA (HEXAPODA) & ARACHNIDA (SPIDERS, SCORPIONS)
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine("    ARTHROPODA CLADE: INSECTA (HEXAPODA) & ARACHNIDA (SPIDERS, SCORPIONS)       ");
        Console.WriteLine("================================================================================");

        var bee = new Creature.Arthropod.Insecta.Hymenoptera.Apidae.HoneyBee(null, null, "Apis Worker");
        var bulletAnt = new Creature.Arthropod.Insecta.Hymenoptera.Formicidae.BulletAnt(null, null, "Paraponera Scout");
        var rhinoBeetle = new Creature.Arthropod.Insecta.Coleoptera.Scarabaeidae.RhinocerosBeetle(null, null, "Dynastes Titan");
        var firefly = new Creature.Arthropod.Insecta.Coleoptera.Lampyridae.Firefly(null, null, "Photinus Spark");
        var butterfly = new Creature.Arthropod.Insecta.Lepidoptera.Nymphalidae.MonarchButterfly(null, null, "Monarch Migrator");
        var mantis = new Creature.Arthropod.Insecta.Mantodea.Mantidae.PrayingMantis(null, null, "Predator Mantis");
        var dragonfly = new Creature.Arthropod.Insecta.Odonata.Aeshnidae.Dragonfly(null, null, "Anax Hunter");
        var tarantula = new Creature.Arthropod.Arachnida.Araneae.Theraphosidae.Tarantula(null, null, "Goliath Bird-Eater");
        var blackWidow = new Creature.Arthropod.Arachnida.Araneae.Theridiidae.BlackWidow(null, null, "Latrodectus Matriarch");
        var scorpion = new Creature.Arthropod.Arachnida.Scorpiones.Buthidae.DeathstalkerScorpion(null, null, "Leiurus Stalker");

        earth.Add(bee);
        earth.Add(bulletAnt);
        earth.Add(rhinoBeetle);
        earth.Add(firefly);
        earth.Add(butterfly);
        earth.Add(mantis);
        earth.Add(dragonfly);
        earth.Add(tarantula);
        earth.Add(blackWidow);
        earth.Add(scorpion);

        Console.WriteLine($"1. Honey Bee:         {bee.PollinateFlower(appleTree)}");
        Console.WriteLine($"2. Bullet Ant:        {bulletAnt.CarryWeight(0.25f)} | Schmidt Pain Index: 4.0+");
        Console.WriteLine($"3. Rhinoceros Beetle: {rhinoBeetle.LiftWeight(4250f)}");
        Console.WriteLine($"4. Firefly:           {firefly.EmitBioluminescence()}");
        Console.WriteLine($"5. Monarch Butterfly: {butterfly.Metamorphose()}");
        Console.WriteLine($"6. Praying Mantis:    {mantis.AmbushStrike(new Creature.Fish(null, null, "Minnow", Creature.SexType.Female))}");
        Console.WriteLine($"7. Dragonfly:         {dragonfly.AerobaticIntercept(new Creature.Fish(null, null, "Water Strider", Creature.SexType.Female))}");
        Console.WriteLine($"8. Tarantula:         {tarantula.FlickUrticatingHairs()} | Leg Count: {tarantula.Legs.Length}");
        Console.WriteLine($"9. Black Widow:       {blackWidow.SpinCobweb()}");
        Console.WriteLine($"10. Scorpion:         {scorpion.GlowUnderUltraviolet()}");

        // 11. AVIAN & AQUATIC APEX PREDATORS
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine("    AVIAN & AQUATIC CLADES: OWLS, FALCONS, SHARKS & ELECTRIC EELS               ");
        Console.WriteLine("================================================================================");

        var owl = new Creature.Bird.Strigiformes.BarnOwl(null, null, Creature.SexType.Female);
        var falcon = new Creature.Bird.Falconiformes.PeregrineFalcon(null, null, Creature.SexType.Male);
        var penguin = new Creature.Bird.Sphenisciformes.EmperorPenguin(null, null, Creature.SexType.Female);
        var shark = new Creature.Fish.Chondrichthyes.Lamnidae.GreatWhiteShark(null, null, Creature.SexType.Female);
        var eel = new Creature.Fish.Osteichthyes.Gymnotidae.ElectricEel(null, null, Creature.SexType.Male);

        earth.Add(owl);
        earth.Add(falcon);
        earth.Add(penguin);
        earth.Add(shark);
        earth.Add(eel);

        Console.WriteLine($"1. Barn Owl:          {owl.RotateHead(270f)} | {owl.SilentFlight()}");
        Console.WriteLine($"2. Peregrine Falcon:  {falcon.DiveStoop(new Creature.Bird(null, null, "Feral Pigeon", Creature.SexType.Male))}");
        Console.WriteLine($"3. Emperor Penguin:   {penguin.DeepOceanDive(450f)} (Blubber={penguin.BlubberThicknessCm}cm)");
        Console.WriteLine($"4. Great White Shark: {shark.SenseElectricField(adam)}");
        Console.WriteLine($"5. Electric Eel:      {eel.DischargeElectricShock(new Creature.Fish(null, null, "River Catfish", Creature.SexType.Male))}");

        // 12. MAMMALIAN TITANS: PRIMATES, BEARS, ELEPHANTS & CETACEANS
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine("    MAMMALIAN TITANS: PRIMATES, URSIDAE, PROBOSCIDEA & CETACEA                   ");
        Console.WriteLine("================================================================================");

        var chimp = new Creature.Primate.Hominidae.Chimpanzee(null, null, Creature.SexType.Male, "Caesar");
        var gorilla = new Creature.Primate.Hominidae.Gorilla(null, null, Creature.SexType.Male, "Koko-Silverback");
        var grizzly = new Creature.Bear.Ursus.GrizzlyBear(null, null, Creature.SexType.Female, "Ursa Major");
        var elephant = new Creature.Elephant.Elephantidae.AfricanBushElephant(null, null, Creature.SexType.Female, "Tembo");
        var blueWhale = new Creature.Cetacean.Mysticeti.Balaenopteridae.BlueWhale(null, null, Creature.SexType.Female, "Leviathan");
        var orca = new Creature.Cetacean.Odontoceti.Delphinidae.Orca(null, null, Creature.SexType.Female, "Freya");

        earth.Add(chimp);
        earth.Add(gorilla);
        earth.Add(grizzly);
        earth.Add(elephant);
        earth.Add(blueWhale);
        earth.Add(orca);

        Console.WriteLine($"1. Chimpanzee:        {chimp.FashionTool(oakTree.Name)} | Opposable Dexterity={chimp.OpposableThumbDexterity * 100:F0}%");
        Console.WriteLine($"2. Mountain Gorilla:  {gorilla.BeatChest()} | Strength={gorilla.SilverbackStrengthKg}kg");
        Console.WriteLine($"3. Grizzly Bear:      {grizzly.CatchSalmon(new Creature.Fish(null, null, "Chinook Salmon", Creature.SexType.Female))} | Fat={grizzly.FatReserveKg}kg");
        Console.WriteLine($"4. African Elephant:  {elephant.TrumpetAndRumble()} | Trunk Muscles={elephant.TrunkMusclesCount:N0}");
        Console.WriteLine($"5. Blue Whale:        {blueWhale.FilterFeedKrill(3.8f)} | Blowhole: {blueWhale.SpoutBlowhole()}");
        Console.WriteLine($"6. Orca (Killer Whale):{orca.EmitEcholocationClicks()} | {orca.PodCoordinateHunt(new Creature.Fish(null, null, "Sea Lion", Creature.SexType.Male))}");

        // 13. Planetary Chemistry & Reaction Engine Verification
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine("    PERIODIC TABLE (BIG 12) & MOLECULAR REACTION ENGINE DEMONSTRATION           ");
        Console.WriteLine("================================================================================");

        // A. Inspecting the Big 12 Elements of Biology & Earth (Fully Nested inside parent families!)
        Element[] big12 = [
            new Element.NonMetal.DiatomicNonMetal.Hydrogen(),
            new Element.NonMetal.PolyatomicNonMetal.Carbon(),
            new Element.NonMetal.DiatomicNonMetal.Nitrogen(),
            new Element.NonMetal.DiatomicNonMetal.Oxygen(),
            new Element.NonMetal.PolyatomicNonMetal.Phosphorus(),
            new Element.NonMetal.PolyatomicNonMetal.Sulfur(),
            new Element.NonMetal.Halogen.Chlorine(),
            new Element.Metal.AlkaliMetal.Sodium(),
            new Element.Metal.AlkaliMetal.Potassium(),
            new Element.Metal.AlkalineEarthMetal.Magnesium(),
            new Element.Metal.AlkalineEarthMetal.Calcium(),
            new Element.Metal.TransitionMetal.Iron()
        ];

        Console.WriteLine("--- The Big 12 Elements of Earth & Biology ---");
        foreach (var elem in big12)
        {
            Console.WriteLine($" {elem.Symbol,-2} | {elem.Name,-11} | Z={elem.AtomicNumber,2} | Mass={elem.AtomicMassU,6:F2}u | Val_e⁻={elem.ValenceElectrons} | Phase={elem.Phase,-6} | Family={elem.GetType().BaseType?.Name}");
        }

        // B. Testing All 6 Reaction Types in Chemical Reaction Vessel
        Console.WriteLine($"\n--- Testing All Reaction Types (Displacement, Synthesis, Bio, etc.) ---");
        var vessel = new Element.ReactionVessel { Name = "Planetary Test Reactor" };

        // 1. Synthesis Reaction (2 H2 + O2 -> 2 H2O)
        vessel.AddMoles<Element.Compound.InorganicCompound.MolecularHydrogenGas>(4.0f);
        vessel.AddMoles<Element.Compound.InorganicCompound.MolecularOxygenGas>(2.0f);
        var r1 = vessel.TriggerReaction<Element.Reaction.SynthesisReaction.WaterSynthesisReaction>(2.0f);
        Console.WriteLine($"1. Synthesis:           {r1.Summary}");

        // 2. Decomposition Reaction (2 H2O -> 2 H2 + O2)
        vessel.AddMoles<Element.Compound.InorganicCompound.WaterMolecule>(2.0f);
        var r2 = vessel.TriggerReaction<Element.Reaction.DecompositionReaction.WaterElectrolysisReaction>(2.0f);
        Console.WriteLine($"2. Decomposition:       {r2.Summary}");

        // 3. Single Displacement Reaction (2 Na + 2 H2O -> 2 NaOH + H2)
        vessel.AddMoles<Element.Metal.AlkaliMetal.Sodium>(2.0f);
        vessel.AddMoles<Element.Compound.InorganicCompound.WaterMolecule>(2.0f);
        var r3 = vessel.TriggerReaction<Element.Reaction.SingleDisplacementReaction.SodiumInWaterReaction>(1.0f);
        Console.WriteLine($"3. Single Displacement: {r3.Summary}");

        // 4. Double Displacement Reaction (HCl + NaOH -> NaCl + H2O)
        vessel.AddMoles<Element.Compound.InorganicCompound.HydrochloricAcid>(1.0f);
        vessel.AddMoles<Element.Compound.InorganicCompound.SodiumHydroxide>(1.0f);
        var r4 = vessel.TriggerReaction<Element.Reaction.DoubleDisplacementReaction.AcidBaseNeutralizationReaction>(1.0f);
        Console.WriteLine($"4. Double Displacement: {r4.Summary}");

        // 5. Combustion Reaction (C6H12O6 + 6 O2 -> 6 CO2 + 6 H2O)
        vessel.AddMoles<Element.Compound.OrganicCompound.Glucose>(1.0f);
        vessel.AddMoles<Element.Compound.InorganicCompound.MolecularOxygenGas>(6.0f);
        var r5 = vessel.TriggerReaction<Element.Reaction.CombustionReaction.GlucoseCombustionReaction>(1.0f);
        Console.WriteLine($"5. Combustion:          {r5.Summary}");

        // 6. Biochemical Reactions: Photosynthesis & Respiration
        vessel.AddMoles<Element.Compound.InorganicCompound.CarbonDioxide>(12.0f);
        vessel.AddMoles<Element.Compound.InorganicCompound.WaterMolecule>(12.0f);
        var photoRes = vessel.TriggerReaction<Element.Reaction.BiochemicalReaction.PhotosynthesisReaction>(2.0f);
        Console.WriteLine($"6. Photosynthesis:      {photoRes.Summary}");

        vessel.AddMoles<Element.Compound.OrganicCompound.Glucose>(1.0f);
        vessel.AddMoles<Element.Compound.InorganicCompound.MolecularOxygenGas>(6.0f);
        var respRes = vessel.TriggerReaction<Element.Reaction.BiochemicalReaction.CellularRespirationReaction>(1.0f);
        Console.WriteLine($"7. Cellular Respiration:{respRes.Summary}");

        Console.WriteLine($"Reactor Final State: Temp={vessel.TemperatureCelsius:F1}°C | Moles H2O={vessel.GetMoles<Element.Compound.InorganicCompound.WaterMolecule>():F1} | Moles O2={vessel.GetMoles<Element.Compound.InorganicCompound.MolecularOxygenGas>():F1} | Moles NaCl={vessel.GetMoles<Element.Compound.InorganicCompound.SodiumChloride>():F1}");

        // 14. Real Biochemical Closed Loop: Plant Photosynthesis -> Fruit -> Eating -> Respiration
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine("    REAL CHEMICAL LIFE-CYCLE: CO2, H2O, PHOTOSYNTHESIS, GLUCOSE & RESPIRATION  ");
        Console.WriteLine("================================================================================");
        Console.WriteLine($"Earth Atmosphere: O2={earth.Atmosphere.OxygenMoles:F1} mol ({earth.Atmosphere.OxygenPercentage:F2}%) | CO2={earth.Atmosphere.CarbonDioxideMoles:F2} mol | H2O(v)={earth.Atmosphere.WaterVaporMoles:F1} mol");

        // Plant ticks with real chemical photosynthesis
        float initialO2 = earth.Atmosphere.OxygenMoles;
        float initialCO2 = earth.Atmosphere.CarbonDioxideMoles;
        appleTree.Tick(24f, earth.Atmosphere);
        Console.WriteLine($"[Apple Tree Photosynthesis] Chlorophyll ({appleTree.Chlorophyll.ChemicalFormula}) absorbed {appleTree.AbsorbedCo2MolesTotal:F3} mol CO2, released {appleTree.ReleasedOxygenMolesTotal:F3} mol O2!");
        Console.WriteLine($"Apple Tree Reserves: Glucose={appleTree.GlucoseReserveMoles:F3} mol | Hydration={appleTree.Hydration:F1}%");

        // Harvest real chemical fruit
        var harvestedApple = appleTree.Harvest()!;
        Console.WriteLine($"\n[Harvested Fruit] {harvestedApple.Name}: Glucose={harvestedApple.GlucoseMoles:F3} mol ({harvestedApple.SugarGrams}g sugar) | H2O={harvestedApple.WaterMoles:F2} mol ({harvestedApple.WaterContentGrams}g water)");

        // Adam eats the apple
        Console.WriteLine($"\n[Creature Ingestion] Adam eats {harvestedApple.Name}...");
        Console.WriteLine($"Adam Pre-Meal: Blood Glucose={adam.Blood.GlucoseMmolPerL:F2} mmol/L | ATP Pool={adam.Blood.AtpMolarPool:F2} mol");
        adam.Eat(harvestedApple);
        Console.WriteLine($"Adam Post-Meal: Blood Glucose={adam.Blood.GlucoseMmolPerL:F2} mmol/L (Nutrient absorbed!)");

        // Adam breathes and metabolizes glucose via cellular respiration
        Console.WriteLine($"\n[Creature Respiration] Adam inhales atmospheric O2, Hemoglobin ({adam.Blood.Hemoglobin.ChemicalFormula}) binds oxygen, exhales CO2 & H2O:");
        adam.Breathe(earth.Atmosphere);
        Console.WriteLine($"Adam Post-Respiration: Oxygen SpO2={adam.Blood.OxygenSaturationSpO2 * 100:F1}% | Bound O2={adam.Blood.BoundOxygenMoles:F4} mol | ATP Pool={adam.Blood.AtpMolarPool:F2} mol | Energy={adam.Energy:F1}");
        Console.WriteLine($"Earth Atmosphere After Cycle: O2={earth.Atmosphere.OxygenMoles:F1} mol | CO2={earth.Atmosphere.CarbonDioxideMoles:F2} mol | H2O(v)={earth.Atmosphere.WaterVaporMoles:F1} mol");

        // 15. Final Planetary Census
        var finalCensus = earth.GetCensus();
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine($"Earth Census at Day {earth.DayCount}: {finalCensus.Living}/{finalCensus.Total} living creatures thriving across all biomes.");
        Console.WriteLine($"Humans: {finalCensus.Humans} | Cats: {finalCensus.Cats} | Dogs: {finalCensus.Dogs} | Birds: {finalCensus.Birds} | Fish: {finalCensus.Fish} | Reptiles: {finalCensus.Reptiles} | Arthropods: {finalCensus.Arthropods} | Other Mammals: {finalCensus.Other}");
        Console.WriteLine($"Botanical Flora: {earth.Flora.Count(p => p.IsAlive)}/{earth.Flora.Count} thriving plants and trees.");
        Console.WriteLine("================================================================================");
    }
}
