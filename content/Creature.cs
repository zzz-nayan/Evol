namespace Evol;

public class Creature
{
    public enum SexType { Male, Female }
    public enum LifeStage { Embryo, Newborn, Child, Adolescent, Adult, Elder, Deceased }
    public enum DentitionStage { Edentulous, BabyTeeth, AdultTeeth }
    public enum DamageType { Physical, Blunt, Slashing, Piercing, Fire, Cold, Poison, Acid, Asphyxiation, Starvation, Dehydration, OldAge, OrganFailure }

    // Lineage & Sex
    public Creature? Father { get; private init; }
    public Creature? Mother { get; private init; }
    public SexType Sex { get; private init; }

    // External Appendages & Senses
    public Hand[] Hands { get; private set; }
    public Leg[] Legs { get; private set; }
    public Eye[] Eyes { get; private set; }
    public Ear[] Ears { get; private set; }
    public Toth[] Teeths { get; private set; }
    public Toth[] Teeth => Teeths;
    public DentitionStage DentalStage { get; protected set; } = DentitionStage.AdultTeeth;
    public int MaxAdultTeethCount { get; protected set; } = 32;
    public int DeciduousTeethCount { get; protected set; } = 20;
    public virtual float TeethingAgeYears => 0.5f;
    public virtual float AdultTeethAgeYears => 6.0f;
    public Lung[] Lungs { get; private set; }
    public Kidney[] Kidneys { get; private set; }
    public Wing[]? Wings { get; protected set; }
    public Gill[]? Gills { get; protected set; }
    public BloodStream Blood { get; private set; }
    public DNA Genome { get; protected set; }

    // Reproductive Anatomy (Genitalia, Uterus/Womb & Mammary Glands/Boobs)
    public Genitalia ReproductiveOrgan { get; private set; }
    public Genitalia Genitals => ReproductiveOrgan;
    public Penis? Dick => ReproductiveOrgan as Penis;
    public Vagina? Pussy => ReproductiveOrgan as Vagina;
    public Uterus? Womb => (ReproductiveOrgan as FemaleGenitalia)?.Womb;
    public bool IsPregnant => Womb?.IsOccupied == true;
    public virtual float SpeciesGestationDays => 280f;
    public MammaryGland? Boobs => (ReproductiveOrgan as FemaleGenitalia)?.Breasts;
    public MammaryGland? Breasts => Boobs;
    public MammaryGland? MammaryGlands => Boobs;

    public virtual bool Nurse(Creature baby)
    {
        if (Boobs == null || !IsAlive || !baby.IsAlive) return false;
        return Boobs.Nurse(baby);
    }

    public virtual bool Breastfeed(Creature baby) => Nurse(baby);

    // Internal Visceral Anatomy & Single Organs
    public BodyAnatomy Body { get; private set; }
    public Brain BrainOrgan => Body.Brain;
    public Heart HeartOrgan => Body.Heart;
    public Nose NoseOrgan => Body.Nose;
    public Mouth MouthOrgan => Body.Mouth;
    public Stomach StomachOrgan => Body.Stomach;
    public Liver LiverOrgan => Body.Liver;
    public Skin SkinOrgan => Body.Skin;
    public Skeleton SkeletonOrgan => Body.Skeleton;
    public Tail? TailOrgan { get => Body.Tail; protected set => Body.Tail = value; }

    // Genetics, Vital Signs & Metabolism
    public LifeStage Stage { get; protected set; } = LifeStage.Adult;
    public bool IsAlive { get; protected set; } = true;
    public bool IsEndothermic { get; protected set; } = true;
    public float AgeYears { get; protected set; } = 0f;
    public float MaxLifespanYears { get; protected set; } = 80f;

    public void AgeBy(float years)
    {
        AgeYears += years;
        UpdateLifeStage();
        UpdateDentition(years);
    }
    public float Health { get; protected set; } = 100f;
    public float MaxHealth { get; protected set; } = 100f;
    public float Energy { get; protected set; } = 100f;
    public float Hunger { get; protected set; } = 0f;
    public float Thirst { get; protected set; } = 0f;
    public float BodyTemperatureCelsius { get; protected set; } = 37.0f;
    public string? CauseOfDeath { get; protected set; }

    protected static readonly Random _rng = new Random();

    public Creature(Creature? father = null, Creature? mother = null, int hands = 2, int handFingers = 5, int legs = 2, int legFingers = 5, int teeths = 32, SexType? sex = null)
    {
        Father = father;
        Mother = mother;
        Sex = sex ?? (_rng.Next(1, 101) <= 50 ? SexType.Male : SexType.Female);

        if (father != null && mother != null)
        {
            Genome = DNA.Combine(father.Genome, mother.Genome, Sex, _rng);
        }
        else
        {
            Genome = DNA.CreateRandom(Sex, _rng);
        }

        MaxLifespanYears = Genome.LongevityPotentialYears;

        // Allocate and build standard body parts
        Hands = new Hand[hands];
        Legs = new Leg[legs];
        Eyes = new Eye[2];
        Ears = new Ear[2];
        Lungs = new Lung[2];
        Kidneys = new Kidney[2];
        MaxAdultTeethCount = teeths;
        DeciduousTeethCount = this switch
        {
            Human => 20,
            Cat => 26,
            Dog => 28,
            Horse => 24,
            _ => Math.Min(20, (int)(teeths * 0.65f))
        };

        MakeHands(hands, handFingers);
        MakeLegs(legs, legFingers);
        MakeEyes();
        MakeEars();

        if (father != null && mother != null)
        {
            // Newborn: 0 teeth at birth! Gums only for breastfeeding
            AgeYears = 0f;
            Stage = LifeStage.Newborn;
            DentalStage = DentitionStage.Edentulous;
            Teeths = Array.Empty<Toth>();
        }
        else
        {
            // Adult starting creature: healthy prime adult with full adult dentition
            AgeYears = Math.Max(1f, MaxLifespanYears * 0.25f);
            Stage = LifeStage.Adult;
            DentalStage = DentitionStage.AdultTeeth;
            Teeths = new Toth[teeths];
            MakeTeeths();
        }

        // Sex-specific genitalia & internal reproductive organs
        if (Sex == SexType.Male)
        {
            ReproductiveOrgan = new Penis { LengthCm = 13f + (_rng.NextSingle() * 6f - 3f) };
        }
        else
        {
            ReproductiveOrgan = new Vagina
            {
                DepthCm = 10f + (_rng.NextSingle() * 4f - 2f),
                Breasts = new MammaryGland(2, MammaryGland.CupSize.C, 350f + (_rng.NextSingle() * 120f - 40f))
            };
        }

        // Visceral anatomy container
        Body = new BodyAnatomy();
        Lungs[0] = new Lung();
        Lungs[1] = new Lung();
        Kidneys[0] = new Kidney();
        Kidneys[1] = new Kidney();
        Blood = new BloodStream();
        Body.Skin.Melanin = _rng.NextSingle();

        // Apply genetic phenotype modifications
        Eyes[0].IrisColor = Genome.EyeColor;
        Eyes[1].IrisColor = Genome.EyeColor;
    }

    public virtual void MakeHands(int hands, int fingers)
    {
        string[] fingerNames = ["Thumb", "Index", "Middle", "Ring", "Little"];
        for (int i = 0; i < hands; i++)
        {
            Finger[] fingers1 = new Finger[fingers];
            for (int j = 0; j < fingers1.Length; j++)
            {
                Nail nail = new Nail(_rng.Next(1, 101) == 1);
                string name = j < fingerNames.Length ? fingerNames[j] : $"Digit_{j + 1}";
                Finger finger = new Finger(name, nail);
                fingers1[j] = finger;
            }
            Hands[i] = new Hand(fingers1);
        }
    }

    public virtual void MakeLegs(int legs, int fingers)
    {
        string[] fingerNames = ["Thumb", "Index", "Middle", "Ring", "Little"];
        for (int i = 0; i < legs; i++)
        {
            Finger[] fingers1 = new Finger[fingers];
            for (int j = 0; j < fingers1.Length; j++)
            {
                Nail nail = new Nail(_rng.Next(1, 101) == 1);
                string toeName = j < fingerNames.Length ? fingerNames[j] : $"Toe_{j + 1}";
                Finger finger = new Finger(toeName, nail);
                fingers1[j] = finger;
            }
            Legs[i] = new Leg(fingers1);
        }
    }

    public virtual void MakeEyes()
    {
        Eyes[0] = new Eye(_rng.Next(1, 10001) == 1);
        Eyes[1] = new Eye(_rng.Next(1, 10001) == 1);
    }

    public virtual void MakeEars()
    {
        Ears[0] = new Ear(_rng.Next(1, 10001) == 1);
        Ears[1] = new Ear(_rng.Next(1, 10001) == 1);
    }

    public virtual void MakeTeeths()
    {
        int count = Teeths.Length;
        if (count == 0) return;

        int incisors = Math.Max(1, (int)Math.Round(count * 0.25f));
        int canines = Math.Max(1, (int)Math.Round(count * 0.125f));
        int premolars = Math.Max(1, (int)Math.Round(count * 0.25f));
        int molars = count - (incisors + canines + premolars);
        if (molars < 0) molars = 0;

        int index = 0;
        for (int i = 0; i < incisors && index < count; i++, index++)
            Teeths[index] = new Toth.Incisors();
        for (int i = 0; i < canines && index < count; i++, index++)
            Teeths[index] = new Toth.Canine();
        for (int i = 0; i < premolars && index < count; i++, index++)
            Teeths[index] = new Toth.Premolar();
        for (int i = 0; i < molars && index < count; i++, index++)
            Teeths[index] = new Toth.Molar();

        while (index < count)
        {
            Teeths[index++] = new Toth.Molar();
        }
    }

    public virtual void EruptBabyTeeth()
    {
        DentalStage = DentitionStage.BabyTeeth;
        Teeths = new Toth[DeciduousTeethCount];
        for (int i = 0; i < DeciduousTeethCount; i++)
        {
            Teeths[i] = new Toth.BabyTooth(AdultTeethAgeYears - TeethingAgeYears, 35f);
        }
    }

    public virtual void EruptAdultTeeth()
    {
        DentalStage = DentitionStage.AdultTeeth;
        Teeths = new Toth[MaxAdultTeethCount];
        MakeTeeths();
    }

    public virtual void UpdateDentition(float deltaYears)
    {
        if (DentalStage == DentitionStage.Edentulous)
        {
            if (AgeYears >= TeethingAgeYears && DeciduousTeethCount > 0)
            {
                EruptBabyTeeth();
            }
        }
        else if (DentalStage == DentitionStage.BabyTeeth)
        {
            foreach (var tooth in Teeths)
            {
                if (tooth is Toth.BabyTooth baby)
                {
                    baby.ProgressAge(deltaYears);
                }
            }

            if (AgeYears >= AdultTeethAgeYears)
            {
                EruptAdultTeeth();
            }
        }
    }

    // =========================================================================
    // --- REPRODUCTIVE ANATOMY: GENITALIA, UTERUS / WOMB & GESTATION ---
    // =========================================================================

    public class Fetus
    {
        public Creature Father { get; }
        public Creature Mother { get; }
        public string? ProspectiveName { get; }
        public DNA Genome { get; }
        public SexType Sex { get; }
        public float GestationDays { get; set; } = 0f;
        public float TargetGestationDays { get; set; } = 280f;
        public bool IsFullyDeveloped => GestationDays >= TargetGestationDays;

        public Fetus(Creature father, Creature mother, string? name, float targetDays)
        {
            Father = father;
            Mother = mother;
            ProspectiveName = name;
            TargetGestationDays = targetDays;
            Sex = _rng.Next(1, 101) <= 50 ? SexType.Male : SexType.Female;
            Genome = DNA.Combine(father.Genome, mother.Genome, Sex, _rng);
        }

        public Creature Deliver()
        {
            // 1. Human Deliveries
            if (Mother is Human humanMother && Father is Human humanFather)
            {
                return new Human(humanFather, humanMother, ProspectiveName ?? $"Human_{_rng.Next(1000, 9999)}", Sex)
                {
                    AgeYears = 0f,
                    Stage = LifeStage.Newborn
                };
            }

            // 2. Feline Hybrids & Specific Breeds (Deeply Nested!)
            if (Father is Cat.Pantherinae.Lion lionFather && Mother is Cat.Pantherinae.Tiger tigerMother)
            {
                return new Cat.Pantherinae.Liger(lionFather, tigerMother, ProspectiveName ?? "Liger Cub", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            }
            if (Father is Cat.Pantherinae.Tiger tigerFather && Mother is Cat.Pantherinae.Lion lionMother)
            {
                return new Cat.Pantherinae.Tigon(tigerFather, lionMother, ProspectiveName ?? "Tigon Cub", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            }
            if (Father is Cat.Pantherinae.Lion fLion && Mother is Cat.Pantherinae.Lion mLion)
                return new Cat.Pantherinae.Lion(fLion, mLion, ProspectiveName ?? "Lion Cub", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            if (Father is Cat.Pantherinae.Tiger fTiger && Mother is Cat.Pantherinae.Tiger mTiger)
                return new Cat.Pantherinae.Tiger(fTiger, mTiger, ProspectiveName ?? "Tiger Cub", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            if (Father is Cat.Felinae.Cheetah fCheetah && Mother is Cat.Felinae.Cheetah mCheetah)
                return new Cat.Felinae.Cheetah(fCheetah, mCheetah, ProspectiveName ?? "Cheetah Cub", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            if (Father is Cat.Felinae.DomesticCat.MaineCoon fCoon && Mother is Cat.Felinae.DomesticCat.MaineCoon mCoon)
                return new Cat.Felinae.DomesticCat.MaineCoon(fCoon, mCoon, ProspectiveName ?? "Maine Coon Kitten", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            if (Father is Cat.Felinae.DomesticCat.Siamese fSiamese && Mother is Cat.Felinae.DomesticCat.Siamese mSiamese)
                return new Cat.Felinae.DomesticCat.Siamese(fSiamese, mSiamese, ProspectiveName ?? "Siamese Kitten", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            if (Father is Cat catFather && Mother is Cat catMother)
            {
                return new Cat(catFather, catMother, ProspectiveName ?? $"Kitten_{_rng.Next(100, 999)}", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            }

            // 3. Canine Hybrids & Specific Breeds (Deeply Nested!)
            if (((Father is Wolf || Father is Dog.WildCanid) && Mother is Dog.DomesticCanid) ||
                (Father is Dog.DomesticCanid && (Mother is Wolf || Mother is Dog.WildCanid)))
            {
                return new Dog.CanineHybrid.Wolfdog(Father, Mother, ProspectiveName ?? "Wolfdog Pup", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            }
            if (Father is Dog.DomesticCanid.GermanShepherd fGs && Mother is Dog.DomesticCanid.GermanShepherd mGs)
                return new Dog.DomesticCanid.GermanShepherd(fGs, mGs, ProspectiveName ?? "German Shepherd Pup", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            if (Father is Dog.DomesticCanid.SiberianHusky fHusky && Mother is Dog.DomesticCanid.SiberianHusky mHusky)
                return new Dog.DomesticCanid.SiberianHusky(fHusky, mHusky, ProspectiveName ?? "Husky Pup", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            if (Father is Dog.DomesticCanid.Bloodhound fBlood && Mother is Dog.DomesticCanid.Bloodhound mBlood)
                return new Dog.DomesticCanid.Bloodhound(fBlood, mBlood, ProspectiveName ?? "Bloodhound Pup", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            if (Father is Dog.DomesticCanid.Chihuahua fChi && Mother is Dog.DomesticCanid.Chihuahua mChi)
                return new Dog.DomesticCanid.Chihuahua(fChi, mChi, ProspectiveName ?? "Chihuahua Pup", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            if (Father is Dog.WildCanid.WolfCanid fWolf && Mother is Dog.WildCanid.WolfCanid mWolf)
                return new Dog.WildCanid.WolfCanid(fWolf, mWolf, ProspectiveName ?? "Wolf Pup", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            if (Father is Dog dogFather && Mother is Dog dogMother)
            {
                return new Dog(dogFather, dogMother, ProspectiveName ?? $"Puppy_{_rng.Next(100, 999)}", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            }

            // 4. Herbivores
            if (Mother is Horse horseMother && Father is Horse horseFather)
            {
                return new Horse(horseFather, horseMother, ProspectiveName ?? "Foal", Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
            }

            return new Creature(Father, Mother, sex: Sex) { AgeYears = 0f, Stage = LifeStage.Newborn };
        }
    }

    public class Uterus
    {
        public Fetus? DevelopingFetus { get; private set; }
        public bool IsOccupied => DevelopingFetus != null;
        public float GestationDays => DevelopingFetus?.GestationDays ?? 0f;
        public float TargetGestationDays => DevelopingFetus?.TargetGestationDays ?? 280f;
        public bool InLabor => DevelopingFetus != null && DevelopingFetus.IsFullyDeveloped;
        public float Trimester => (GestationDays / Math.Max(1f, TargetGestationDays)) * 3f;

        public bool Conceive(Creature father, Creature mother, string? childName, float targetGestationDays)
        {
            if (IsOccupied) return false;
            DevelopingFetus = new Fetus(father, mother, childName, targetGestationDays);
            return true;
        }

        public void AdvanceGestation(float deltaDays)
        {
            if (DevelopingFetus == null) return;
            DevelopingFetus.GestationDays += deltaDays;
        }

        public Creature? Deliver()
        {
            if (DevelopingFetus == null || !InLabor) return null;
            var baby = DevelopingFetus.Deliver();
            DevelopingFetus = null;
            return baby;
        }

        public void Terminate() => DevelopingFetus = null;
    }

    public abstract class Genitalia
    {
        public bool IsDamaged { get; set; } = false;
        public List<STD> ActiveInfections { get; } = new();
        public bool HasSTD => ActiveInfections.Count > 0;

        public void Contract(STD std)
        {
            if (!ActiveInfections.Any(i => i.GetType() == std.GetType()))
            {
                ActiveInfections.Add(std);
            }
        }

        public void ClearInfections() => ActiveInfections.Clear();
    }

    public class MaleGenitalia : Genitalia
    {
        public float LengthCm { get; set; } = 14f;
        public bool IsErect { get; set; } = false;
        public float SpermCountMillions { get; set; } = 80f;

        public void Erect() => IsErect = true;
        public void Flaccid() => IsErect = false;
    }

    public class Penis : MaleGenitalia
    {
    }

    public class FemaleGenitalia : Genitalia
    {
        public float DepthCm { get; set; } = 10f;
        public bool IsFertile { get; set; } = true;
        public Uterus Womb { get; } = new Uterus();
        public bool IsPregnant => Womb.IsOccupied;
        public float GestationDays => Womb.GestationDays;
        public MammaryGland Breasts { get; set; } = new MammaryGland();
        public MammaryGland Boobs => Breasts;
        public MammaryGland MammaryGlands => Breasts;
    }

    public class Vagina : FemaleGenitalia
    {
    }

    // --- Mammary Glands & Breasts (Boobs) Anatomical System ---

    public class MammaryGland
    {
        public enum CupSize { AA, A, B, C, D, DD, E, F, G }

        public CupSize Cup { get; set; } = CupSize.C;
        public float VolumeCc { get; set; } = 350f;
        public float Firmness { get; set; } = 0.85f;
        public bool IsSymmetrical { get; set; } = true;
        public bool IsLactating { get; set; } = false;
        public float MilkStorageMl { get; set; } = 0f;
        public float MaxMilkCapacityMl { get; set; } = 600f;
        public float ProlactinLevel { get; set; } = 0.1f;
        public float OxytocinLevel { get; set; } = 0.2f;
        public Nipple[] Nipples { get; set; }

        public int NippleCount => Nipples.Length;

        public MammaryGland(int nippleCount = 2, CupSize cup = CupSize.C, float volumeCc = 350f)
        {
            Cup = cup;
            VolumeCc = volumeCc;
            Nipples = new Nipple[nippleCount];
            for (int i = 0; i < nippleCount; i++)
            {
                Nipples[i] = new Nipple();
            }
        }

        public void Arouse()
        {
            OxytocinLevel = Math.Clamp(OxytocinLevel + 0.3f, 0f, 1.0f);
            foreach (var n in Nipples) n.Arouse();
        }

        public void Relax()
        {
            foreach (var n in Nipples) n.Relax();
        }

        public void PrepareLactation(float deltaDays)
        {
            ProlactinLevel = Math.Clamp(ProlactinLevel + 0.05f * deltaDays, 0f, 1.0f);
            if (ProlactinLevel > 0.4f)
            {
                IsLactating = true;
                VolumeCc = Math.Min(VolumeCc * 1.002f, 750f);
            }
        }

        public void ProduceMilk(float deltaHours)
        {
            if (!IsLactating) return;

            float synthesisRate = (15f * ProlactinLevel) * deltaHours;
            MilkStorageMl = Math.Clamp(MilkStorageMl + synthesisRate, 0f, MaxMilkCapacityMl);
        }

        public bool Nurse(Creature infant, float requestedMl = 120f)
        {
            if (!IsLactating || MilkStorageMl <= 5f || !infant.IsAlive)
                return false;

            float actualMl = Math.Min(MilkStorageMl, requestedMl);
            MilkStorageMl -= actualMl;

            var breastMilk = new Food.CowMilk();
            infant.Eat(breastMilk);
            infant.Thirst = Math.Max(0f, infant.Thirst - actualMl / 3f);

            infant.Genome.ImmuneStrength = Math.Clamp(infant.Genome.ImmuneStrength + 0.02f, 0.5f, 2.0f);
            OxytocinLevel = Math.Clamp(OxytocinLevel + 0.4f, 0f, 1.0f);
            Arouse();

            return true;
        }

        public class Nipple
        {
            public float DiameterMm { get; set; } = 10f;
            public float AreolaDiameterMm { get; set; } = 35f;
            public bool IsErect { get; set; } = false;
            public bool IsPierced { get; set; } = false;
            public float Sensitivity { get; set; } = 0.85f;

            public void Arouse() => IsErect = true;
            public void Relax() => IsErect = false;
        }
    }

    // --- Sexually Transmitted Diseases / Infections ---

    public abstract class STD
    {
        public string Name { get; protected set; } = "Generic STI";
        public float TransmissionChance { get; protected set; } = 0.5f;
        public float DamagePerDay { get; protected set; } = 0.5f;
        public bool CuredByAntibiotics { get; protected set; } = true;
        public float DaysInfected { get; protected set; } = 0f;

        public abstract STD Clone();

        public virtual void ProgressDisease(Creature host, float deltaHours)
        {
            DaysInfected += deltaHours / 24f;
            if (DamagePerDay > 0f)
            {
                host.TakeDamage(DamagePerDay * (deltaHours / 24f), DamageType.Poison, $"Complications from {Name}");
            }
        }
    }

    public abstract class BacterialSTD : STD
    {
        public BacterialSTD() { CuredByAntibiotics = true; }
    }

    public abstract class ViralSTD : STD
    {
        public ViralSTD() { CuredByAntibiotics = false; }
    }

    public class Syphilis : BacterialSTD
    {
        public Syphilis()
        {
            Name = "Syphilis";
            TransmissionChance = 0.65f;
            DamagePerDay = 1.0f;
        }

        public override STD Clone() => new Syphilis { DaysInfected = 0f };

        public override void ProgressDisease(Creature host, float deltaHours)
        {
            base.ProgressDisease(host, deltaHours);
            if (DaysInfected > 60f)
            {
                host.Body.Brain.IntelligenceQuotient = Math.Max(20f, host.Body.Brain.IntelligenceQuotient - 0.05f * deltaHours);
                host.Body.Brain.Consciousness = Math.Max(0.3f, host.Body.Brain.Consciousness - 0.002f * deltaHours);
            }
        }
    }

    public class Gonorrhea : BacterialSTD
    {
        public Gonorrhea()
        {
            Name = "Gonorrhea";
            TransmissionChance = 0.70f;
            DamagePerDay = 0.8f;
        }

        public override STD Clone() => new Gonorrhea { DaysInfected = 0f };
    }

    public class Chlamydia : BacterialSTD
    {
        public Chlamydia()
        {
            Name = "Chlamydia";
            TransmissionChance = 0.50f;
            DamagePerDay = 0.4f;
        }

        public override STD Clone() => new Chlamydia { DaysInfected = 0f };
    }

    public class Herpes : ViralSTD
    {
        public Herpes()
        {
            Name = "Herpes";
            TransmissionChance = 0.45f;
            DamagePerDay = 0.2f;
        }

        public override STD Clone() => new Herpes { DaysInfected = 0f };
    }

    public class HIV : ViralSTD
    {
        public HIV()
        {
            Name = "HIV";
            TransmissionChance = 0.35f;
            DamagePerDay = 1.5f;
        }

        public override STD Clone() => new HIV { DaysInfected = 0f };

        public override void ProgressDisease(Creature host, float deltaHours)
        {
            base.ProgressDisease(host, deltaHours);
            host.Blood.WhiteBloodCellCountThousandPerUl = Math.Max(0.5f, host.Blood.WhiteBloodCellCountThousandPerUl - 0.02f * deltaHours);
        }
    }

    // --- Internal Body Organ System ---

    public class BodyAnatomy
    {
        public Brain Brain { get; set; } = new Brain();
        public Heart Heart { get; set; } = new Heart();
        public Nose Nose { get; set; } = new Nose();
        public Mouth Mouth { get; set; } = new Mouth();
        public Stomach Stomach { get; set; } = new Stomach();
        public Liver Liver { get; set; } = new Liver();
        public Skin Skin { get; set; } = new Skin();
        public Skeleton Skeleton { get; set; } = new Skeleton();
        public Tail? Tail { get; set; }
    }

    public class Hand(params Finger[] fingers)
    {
        public Finger[] Fingers { get; private init; } = fingers;
        public float GripStrengthKg { get; set; } = 40f;
        public bool IsFunctional => Fingers.Length > 0 && Fingers.Any(f => !f.IsBroken);
    }

    public class Leg(params Finger[] fingers)
    {
        public Finger[] Fingers { get; private init; } = fingers;
        public float KickForceNewtons { get; set; } = 1500f;
        public bool CanWalk => Fingers.Length > 0 && Fingers.Count(f => !f.IsBroken) >= Fingers.Length / 2;
    }

    public class Finger(string name, Nail nail)
    {
        public string Name { get; private set; } = name;
        public float Length { get; private set; } = 7.5f;
        public Nail Nail { get; set; } = nail;
        public bool IsBroken { get; set; } = false;
        public float Flexibility { get; set; } = 1.0f;

        public void Break() => IsBroken = true;
        public void Heal() => IsBroken = false;
        public void SetLength(float len) => Length = len;
    }

    public class Toe(string name, Nail nail) : Finger(name, nail)
    {
    }

    public class Nail(bool damaged)
    {
        public float Length { get; private set; } = 1.2f;
        public bool IsDamaged { get; private set; } = damaged;
        public bool IsRetractable { get; set; } = false;
        public bool IsExtended { get; set; } = true;

        public void Damage() => IsDamaged = true;
        public void Repair() => IsDamaged = false;
        public void SetLength(float len) => Length = len;
        public void Grow(float amt) => Length += amt;
        public void Trim(float amt) => Length = Math.Max(0.2f, Length - amt);
        public void Retract() { if (IsRetractable) IsExtended = false; }
        public void Extend() { IsExtended = true; }
    }

    public class Eye(bool damaged)
    {
        public bool IsDamaged { get; private set; } = damaged;
        public float VisualAcuity { get; set; } = 1.0f;
        public float PupilDilation { get; set; } = 0.5f;
        public string IrisColor { get; set; } = "Brown";
        public bool HasTapetumLucidum { get; set; } = false;
        public bool IsBlind => IsDamaged || VisualAcuity <= 0.05f;

        public void Damage() => IsDamaged = true;
        public void Heal() => IsDamaged = false;
        public void AdjustPupil(float ambientLightLevel) => PupilDilation = Math.Clamp(1.0f - ambientLightLevel, 0.1f, 1.0f);
    }

    public class Ear(bool damaged)
    {
        public bool IsDamaged { get; private set; } = damaged;
        public float HearingAcuity { get; set; } = 1.0f;
        public float MinFrequencyHz { get; set; } = 20f;
        public float MaxFrequencyHz { get; set; } = 20000f;
        public bool IsDeaf => IsDamaged || HearingAcuity <= 0.05f;

        public void Damage() => IsDamaged = true;
        public void Heal() => IsDamaged = false;
        public bool CanHear(float frequencyHz, float decibels) => !IsDeaf && frequencyHz >= MinFrequencyHz && frequencyHz <= MaxFrequencyHz && decibels >= (10f / Math.Max(0.1f, HearingAcuity));
    }

    public class Nose(bool damaged = false)
    {
        public bool IsDamaged { get; private set; } = damaged;
        public float OlfactorySensitivity { get; set; } = 1.0f;
        public bool IsCongested { get; set; } = false;
        public float MucusLevel { get; set; } = 0.1f;

        public void Damage() => IsDamaged = true;
        public void Heal() => IsDamaged = false;
        public void Sneeze() { MucusLevel = 0.05f; IsCongested = false; }
    }

    public class Mouth
    {
        public Tongue Tongue { get; private set; } = new Tongue();
        public bool IsOpen { get; set; } = false;
        public float SalivaProductionRate { get; set; } = 1.0f;
        public float MaxJawForceNewtons { get; set; } = 700f;

        public void Open() => IsOpen = true;
        public void Close() => IsOpen = false;
    }

    public class Tongue
    {
        public float LengthCm { get; set; } = 10f;
        public float TasteSensitivity { get; set; } = 1.0f;
        public bool IsDamaged { get; set; } = false;
    }

    public class Brain
    {
        public float Consciousness { get; set; } = 1.0f;
        public float IntelligenceQuotient { get; set; } = 100f;
        public float StressLevel { get; set; } = 0.1f;
        public float Dopamine { get; set; } = 0.5f;
        public float Serotonin { get; set; } = 0.5f;
        public float Cortisol { get; set; } = 0.2f;
        public float Adrenaline { get; set; } = 0.1f;
        public float Endorphins { get; set; } = 0.3f;
        public bool IsSleeping { get; set; } = false;
        public bool IsDamaged { get; set; } = false;

        public void Sleep() => IsSleeping = true;
        public void Wake() => IsSleeping = false;
        public void TriggerFightOrFlight()
        {
            Adrenaline = Math.Clamp(Adrenaline + 0.5f, 0f, 1f);
            Cortisol = Math.Clamp(Cortisol + 0.3f, 0f, 1f);
        }
        public void CalmDown()
        {
            Adrenaline = Math.Max(0.1f, Adrenaline - 0.2f);
            Cortisol = Math.Max(0.1f, Cortisol - 0.1f);
            Serotonin = Math.Clamp(Serotonin + 0.1f, 0f, 1f);
        }
    }

    public class Heart(int chambers = 4)
    {
        public int Chambers { get; private init; } = chambers;
        public float BeatsPerMinute { get; set; } = 72f;
        public float SystolicPressure { get; set; } = 120f;
        public float DiastolicPressure { get; set; } = 80f;
        public bool IsBeating { get; set; } = true;
        public bool IsDamaged { get; set; } = false;

        public void CardiacArrest() => IsBeating = false;
        public void Resuscitate() => IsBeating = true;
    }

    public class Lung(bool damaged = false)
    {
        public bool IsDamaged { get; private set; } = damaged;
        public float CapacityLiters { get; set; } = 3.0f;
        public float OxygenExchangeEfficiency { get; set; } = 0.98f;

        public void Damage() => IsDamaged = true;
        public void Heal() => IsDamaged = false;
    }

    public class Stomach
    {
        public float MaxCapacityVolumeLiters { get; set; } = 2.0f;
        public float CurrentContentsVolumeLiters { get; set; } = 0f;
        public float AcidPH { get; set; } = 1.8f;
        public float DigestionRatePerHour { get; set; } = 0.25f;
        public float CurrentCalorieContents { get; set; } = 0f;
        public bool IsFull => CurrentContentsVolumeLiters >= MaxCapacityVolumeLiters * 0.9f;

        public bool Ingest(Food food)
        {
            float volume = food.Calories / 600f;
            if (CurrentContentsVolumeLiters + volume > MaxCapacityVolumeLiters)
                return false;
            CurrentContentsVolumeLiters += volume;
            CurrentCalorieContents += food.Calories;
            return true;
        }

        public (float caloriesAbsorbed, float waterAbsorbedMl) Digest(float deltaHours)
        {
            if (CurrentContentsVolumeLiters <= 0f) return (0f, 0f);
            float amountToDigest = Math.Min(CurrentContentsVolumeLiters, DigestionRatePerHour * deltaHours);
            float fraction = amountToDigest / CurrentContentsVolumeLiters;
            float calories = CurrentCalorieContents * fraction;
            CurrentContentsVolumeLiters -= amountToDigest;
            CurrentCalorieContents -= calories;
            return (calories, amountToDigest * 1000f);
        }
    }

    public class Liver
    {
        public float DetoxEfficiency { get; set; } = 1.0f;
        public float GlycogenReserveGrams { get; set; } = 100f;
        public bool IsDamaged { get; set; } = false;

        public float ProcessToxins(float toxinAmount)
        {
            float processed = toxinAmount * DetoxEfficiency;
            if (toxinAmount > 50f) IsDamaged = true;
            return Math.Max(0f, toxinAmount - processed);
        }
    }

    public class Kidney(bool damaged = false)
    {
        public bool IsDamaged { get; private set; } = damaged;
        public float FiltrationRate { get; set; } = 1.0f;

        public void Damage() => IsDamaged = true;
        public void Heal() => IsDamaged = false;
    }

    public class BloodStream
    {
        public float VolumeLiters { get; set; } = 5.0f;
        public float MaxVolumeLiters { get; set; } = 5.0f;
        public float OxygenSaturationSpO2 { get; set; } = 0.99f;
        public float GlucoseMmolPerL { get; set; } = 5.5f;
        public float WhiteBloodCellCountThousandPerUl { get; set; } = 7.0f;
        public float PlateletsThousandPerUl { get; set; } = 250f;
        public float ToxicityLevel { get; set; } = 0f;

        // --- Real Chemical Elemental & Molecular Components ---
        public Element.Compound.OrganicCompound.HemoglobinProtein Hemoglobin { get; } = new();
        public float AtpMolarPool { get; set; } = 1.2f;
        public float BoundOxygenMoles { get; set; } = 0.04f;
        public float DissolvedCarbonDioxideMoles { get; set; } = 0.005f;
        public float IronMineralReserveMg { get; set; } = 4000f;
        public float CalciumElectrolyteMg { get; set; } = 500f;
        public float SodiumChlorideGrams { get; set; } = 45f;

        public bool Bleed(float liters)
        {
            VolumeLiters = Math.Max(0f, VolumeLiters - liters);
            return VolumeLiters < MaxVolumeLiters * 0.5f;
        }

        public void Replenish(float deltaHours)
        {
            if (VolumeLiters < MaxVolumeLiters)
                VolumeLiters = Math.Min(MaxVolumeLiters, VolumeLiters + 0.05f * deltaHours);
        }
    }

    public class Skin
    {
        public float SurfaceAreaM2 { get; set; } = 1.8f;
        public float Melanin { get; set; } = 0.5f;
        public float HealthPercent { get; set; } = 100f;
        public bool IsSweating { get; set; } = false;
        public bool IsShivering { get; set; } = false;

        public void RegulateTemperature(float bodyTempCelsius, float ambientTempCelsius)
        {
            IsSweating = bodyTempCelsius > 37.5f || ambientTempCelsius > 32f;
            IsShivering = bodyTempCelsius < 36.2f || ambientTempCelsius < 10f;
        }
    }

    public class Skeleton
    {
        public int TotalBones { get; set; } = 206;
        public float BoneDensity { get; set; } = 1.0f;
        public List<string> BrokenBones { get; } = new List<string>();

        public void Fracture(string boneName)
        {
            if (!BrokenBones.Contains(boneName))
                BrokenBones.Add(boneName);
        }

        public void Mend(string boneName) => BrokenBones.Remove(boneName);
    }

    public class Tail
    {
        public float LengthCm { get; set; } = 30f;
        public bool IsPrehensile { get; set; } = false;
        public bool IsDamaged { get; set; } = false;
        public float WagSpeed { get; set; } = 0f;

        public void Wag(float intensity) => WagSpeed = intensity;
    }

    public class Wing
    {
        public float WingspanMeters { get; set; } = 1.5f;
        public float FeatherCondition { get; set; } = 1.0f;
        public bool IsDamaged { get; set; } = false;

        public bool CanFly => !IsDamaged && FeatherCondition >= 0.6f;
    }

    public class Gill(bool damaged = false)
    {
        public bool IsDamaged { get; private set; } = damaged;
        public float OxygenExtractionRate { get; set; } = 0.85f;

        public bool FunctionUnderwater => !IsDamaged;
    }

    public class DNA
    {
        public enum BloodGroup { O_Negative, O_Positive, A_Negative, A_Positive, B_Negative, B_Positive, AB_Negative, AB_Positive }
        public BloodGroup BloodType { get; set; } = BloodGroup.O_Positive;
        public string EyeColor { get; set; } = "Brown";
        public string HairColor { get; set; } = "Black";
        public float HeightPotentialMeters { get; set; } = 1.75f;
        public float LongevityPotentialYears { get; set; } = 80f;
        public float MetabolismRate { get; set; } = 1.0f;
        public float ImmuneStrength { get; set; } = 1.0f;
        public string GenomeSequence { get; set; } = "ATCG-EARTH-GENOME";

        public static DNA CreateRandom(SexType sex, Random rng)
        {
            string[] eyeColors = ["Brown", "Blue", "Green", "Hazel", "Amber", "Gray"];
            string[] hairColors = ["Black", "Brown", "Blonde", "Red", "Auburn"];
            Array bloodValues = Enum.GetValues(typeof(BloodGroup));

            return new DNA
            {
                BloodType = (BloodGroup)bloodValues.GetValue(rng.Next(bloodValues.Length))!,
                EyeColor = eyeColors[rng.Next(eyeColors.Length)],
                HairColor = hairColors[rng.Next(hairColors.Length)],
                HeightPotentialMeters = sex == SexType.Male ? 1.78f + (rng.NextSingle() * 0.2f - 0.1f) : 1.65f + (rng.NextSingle() * 0.2f - 0.1f),
                LongevityPotentialYears = 75f + (rng.NextSingle() * 25f - 10f),
                MetabolismRate = 0.9f + rng.NextSingle() * 0.2f,
                ImmuneStrength = 0.85f + rng.NextSingle() * 0.3f
            };
        }

        public static DNA Combine(DNA father, DNA mother, SexType sex, Random rng)
        {
            string eye = rng.Next(2) == 0 ? father.EyeColor : mother.EyeColor;
            string hair = rng.Next(2) == 0 ? father.HairColor : mother.HairColor;
            BloodGroup blood = rng.Next(2) == 0 ? father.BloodType : mother.BloodType;

            if (rng.NextSingle() < 0.02f)
            {
                string[] rareColors = ["Violet", "Golden", "Heterochromia"];
                eye = rareColors[rng.Next(rareColors.Length)];
            }

            float height = ((father.HeightPotentialMeters + mother.HeightPotentialMeters) / 2f) + (rng.NextSingle() * 0.1f - 0.05f);
            float longevity = ((father.LongevityPotentialYears + mother.LongevityPotentialYears) / 2f) + (rng.NextSingle() * 10f - 5f);
            float metabolism = ((father.MetabolismRate + mother.MetabolismRate) / 2f) + (rng.NextSingle() * 0.1f - 0.05f);
            float immune = ((father.ImmuneStrength + mother.ImmuneStrength) / 2f) + (rng.NextSingle() * 0.1f - 0.05f);

            return new DNA
            {
                BloodType = blood,
                EyeColor = eye,
                HairColor = hair,
                HeightPotentialMeters = height,
                LongevityPotentialYears = longevity,
                MetabolismRate = Math.Clamp(metabolism, 0.5f, 2.0f),
                ImmuneStrength = Math.Clamp(immune, 0.2f, 2.0f)
            };
        }
    }

    public abstract class Toth
    {
        public float Cavity { get; private set; } = 0f;
        public bool Damaged { get; private set; } = false;
        public float BiteForce { get; private set; }

        public Toth(float biteForce = 150f)
        {
            BiteForce = biteForce;
        }

        public void AddCavity(float amt)
        {
            if (Damaged) return;
            Cavity += amt;

            float damageChance = 0f;
            if (Cavity >= 0.8f) damageChance += 0.05f;
            if (Cavity >= 0.85f) damageChance += 0.05f;
            if (Cavity >= 0.875f) damageChance += 0.05f;
            if (Cavity >= 0.9f) damageChance += 0.1f;
            if (Cavity >= 0.925f) damageChance += 0.15f;
            if (Cavity >= 0.95f) damageChance += 0.15f;
            if (Cavity >= 0.975f) damageChance += 0.20f;
            if (Cavity >= 1f) damageChance = 1f;

            if (_rng.NextSingle() < damageChance)
                Damaged = true;
        }

        public void CleanCavity(float amt) => Cavity = Math.Max(0f, Cavity - amt);

        public void Repair()
        {
            Damaged = false;
            Cavity = 0f;
        }

        public class BabyTooth : Toth
        {
            public float MaxAge { get; private init; }
            public float CurrentAge { get; private set; } = 0f;

            public BabyTooth(float maxAge = 6f, float biteForce = 35f) : base(biteForce)
            {
                MaxAge = maxAge;
            }

            public void ProgressAge(float deltaYears = 0.01f)
            {
                CurrentAge += deltaYears;
                if (CurrentAge >= MaxAge)
                {
                    Damaged = true;
                }
            }

            public bool IsReadyToFallOut => CurrentAge >= MaxAge;
        }

        public class Incisors : Toth
        {
            public Incisors() : base(60f) { }
            public Incisors(float biteForce) : base(biteForce) { }
        }

        public class Canine : Toth
        {
            public Canine() : base(120f) { }
            public Canine(float biteForce) : base(biteForce) { }
        }

        public class Premolar : Toth
        {
            public Premolar() : base(250f) { }
            public Premolar(float biteForce) : base(biteForce) { }
        }

        public class Molar : Toth
        {
            public Molar() : base(500f) { }
            public Molar(float biteForce) : base(biteForce) { }
        }
    }

    public class Water
    {
        public float VolumeMilliliters { get; set; } = 250f;
        public float Purity { get; set; } = 1.0f;
        public float Contaminants { get; set; } = 0f;
    }

    public class Atmosphere
    {
        public static Atmosphere StandardEarth => new Atmosphere
        {
            TemperatureCelsius = 15f,
            OxygenMoles = 20950f,
            NitrogenMoles = 78080f,
            CarbonDioxideMoles = 40f,
            WaterVaporMoles = 1000f,
            ArgonAndTraceMoles = 930f,
            AtmosphericPressureAtm = 1.0f,
            RelativeHumidity = 0.5f,
            GravityMps2 = 9.80665f
        };

        public float TemperatureCelsius { get; set; } = 15f;
        public float AtmosphericPressureAtm { get; set; } = 1.0f;
        public float RelativeHumidity { get; set; } = 0.5f;
        public float GravityMps2 { get; set; } = 9.80665f;

        // --- Real Molar Gas Reserves in Biome Air Packet (Base 100,000 Moles) ---
        public float OxygenMoles { get; set; } = 20950f;
        public float NitrogenMoles { get; set; } = 78080f;
        public float CarbonDioxideMoles { get; set; } = 40f;
        public float WaterVaporMoles { get; set; } = 1000f;
        public float ArgonAndTraceMoles { get; set; } = 930f;

        public float TotalMoles => Math.Max(1f, OxygenMoles + NitrogenMoles + CarbonDioxideMoles + WaterVaporMoles + ArgonAndTraceMoles);

        public float OxygenPercentage
        {
            get => (OxygenMoles / TotalMoles) * 100f;
            set
            {
                float targetMoles = TotalMoles * (value / 100f);
                OxygenMoles = Math.Max(0f, targetMoles);
            }
        }

        public float NitrogenPercentage
        {
            get => (NitrogenMoles / TotalMoles) * 100f;
            set
            {
                float targetMoles = TotalMoles * (value / 100f);
                NitrogenMoles = Math.Max(0f, targetMoles);
            }
        }

        public float CarbonDioxidePercentage
        {
            get => (CarbonDioxideMoles / TotalMoles) * 100f;
            set
            {
                float targetMoles = TotalMoles * (value / 100f);
                CarbonDioxideMoles = Math.Max(0f, targetMoles);
            }
        }

        public float ConsumeOxygen(float moles)
        {
            float consumed = Math.Min(OxygenMoles, Math.Max(0f, moles));
            OxygenMoles -= consumed;
            return consumed;
        }

        public void ProduceOxygen(float moles)
        {
            OxygenMoles += Math.Max(0f, moles);
        }

        public float ConsumeCarbonDioxide(float moles)
        {
            float consumed = Math.Min(CarbonDioxideMoles, Math.Max(0f, moles));
            CarbonDioxideMoles -= consumed;
            return consumed;
        }

        public void ProduceCarbonDioxide(float moles)
        {
            CarbonDioxideMoles += Math.Max(0f, moles);
        }

        public void ProduceWaterVapor(float moles)
        {
            WaterVaporMoles += Math.Max(0f, moles);
        }

        public float ConsumeWaterVapor(float moles)
        {
            float consumed = Math.Min(WaterVaporMoles, Math.Max(0f, moles));
            WaterVaporMoles -= consumed;
            return consumed;
        }
    }

    // --- Earth Simulation Life Cycle, Metabolism & Homeostasis ---

    public virtual void Tick(float deltaHours, Atmosphere? env = null)
    {
        if (!IsAlive) return;

        env ??= Atmosphere.StandardEarth;

        float deltaDays = deltaHours / 24f;
        float deltaYears = deltaHours / (24f * 365.25f);
        AgeYears += deltaYears;
        UpdateLifeStage();
        UpdateDentition(deltaYears);

        if (AgeYears >= MaxLifespanYears)
        {
            float mortalityChance = (AgeYears - MaxLifespanYears) * 0.001f * deltaHours;
            if (_rng.NextSingle() < mortalityChance)
            {
                Die("Natural causes (Old Age)");
                return;
            }
        }

        if (!Body.Heart.IsBeating)
        {
            Blood.OxygenSaturationSpO2 = Math.Max(0f, Blood.OxygenSaturationSpO2 - 0.05f * deltaHours);
            Body.Brain.Consciousness = 0f;
            if (Blood.OxygenSaturationSpO2 <= 0f)
            {
                Die("Cardiac Arrest");
                return;
            }
        }
        else
        {
            Breathe(env, deltaHours);
        }

        // Advance pregnancy gestation in womb & prepare lactation
        if (IsPregnant && Womb != null)
        {
            Womb.AdvanceGestation(deltaDays);
            Boobs?.PrepareLactation(deltaDays);
            Boobs?.ProduceMilk(deltaHours);

            Hunger = Math.Clamp(Hunger + (1.6f * Genome.MetabolismRate * deltaHours), 0f, 100f);
            Thirst = Math.Clamp(Thirst + (3.0f * Genome.MetabolismRate * deltaHours), 0f, 100f);
        }
        else
        {
            if (Boobs?.IsLactating == true)
            {
                Boobs.ProduceMilk(deltaHours);
            }

            Hunger = Math.Clamp(Hunger + (1.2f * Genome.MetabolismRate * deltaHours), 0f, 100f);
            if (Gills != null)
            {
                Thirst = 0f;
            }
            else
            {
                Thirst = Math.Clamp(Thirst + (2.0f * Genome.MetabolismRate * deltaHours), 0f, 100f);
            }
        }

        var (caloriesAbsorbed, waterAbsorbedMl) = Body.Stomach.Digest(deltaHours);
        if (caloriesAbsorbed > 0f)
            Hunger = Math.Max(0f, Hunger - caloriesAbsorbed / 30f);
        if (waterAbsorbedMl > 0f && Gills == null)
            Thirst = Math.Max(0f, Thirst - waterAbsorbedMl / 50f);

        // Thermoregulation
        if (IsEndothermic)
        {
            Body.Skin.RegulateTemperature(BodyTemperatureCelsius, env.TemperatureCelsius);
            float tempDelta = env.TemperatureCelsius - BodyTemperatureCelsius;
            BodyTemperatureCelsius += tempDelta * 0.02f * deltaHours;

            if (Body.Skin.IsSweating) BodyTemperatureCelsius -= 0.5f * deltaHours;
            if (Body.Skin.IsShivering) BodyTemperatureCelsius += 0.3f * deltaHours;

            if (BodyTemperatureCelsius < 28f)
            {
                TakeDamage(10f * deltaHours, DamageType.Cold, "Severe Hypothermia");
            }
            else if (BodyTemperatureCelsius > 42f)
            {
                TakeDamage(15f * deltaHours, DamageType.Fire, "Extreme Hyperthermia");
            }
        }
        else
        {
            BodyTemperatureCelsius = env.TemperatureCelsius;
            if (BodyTemperatureCelsius < -2f)
            {
                TakeDamage(15f * deltaHours, DamageType.Cold, "Freezing");
            }
            else if (BodyTemperatureCelsius > 45f)
            {
                TakeDamage(15f * deltaHours, DamageType.Fire, "Extreme Overheating");
            }
        }

        if (Hunger >= 100f) TakeDamage(3f * deltaHours, DamageType.Starvation, "Starvation");
        if (Thirst >= 100f) TakeDamage(6f * deltaHours, DamageType.Dehydration, "Dehydration");

        foreach (var std in Genitals.ActiveInfections.ToList())
        {
            std.ProgressDisease(this, deltaHours);
        }

        if (Hunger < 40f && Thirst < 40f && Energy > 50f && Health < MaxHealth && IsAlive)
        {
            Heal(1.0f * deltaHours);
        }

        Blood.Replenish(deltaHours);
    }

    protected virtual void UpdateLifeStage()
    {
        float ratio = AgeYears / Math.Max(1f, MaxLifespanYears);
        if (ratio < 0.05f) Stage = LifeStage.Newborn;
        else if (ratio < 0.20f) Stage = LifeStage.Child;
        else if (ratio < 0.30f) Stage = LifeStage.Adolescent;
        else if (ratio < 0.75f) Stage = LifeStage.Adult;
        else Stage = LifeStage.Elder;
    }

    public virtual bool Breathe(Atmosphere env, float deltaHours = 1.0f)
    {
        if (Gills != null)
        {
            Blood.OxygenSaturationSpO2 = Math.Clamp(Blood.OxygenSaturationSpO2 + 0.01f, 0f, 0.99f);
            return true;
        }

        if (env.OxygenPercentage < 10f || env.OxygenMoles < 1f)
        {
            Blood.OxygenSaturationSpO2 = Math.Max(0f, Blood.OxygenSaturationSpO2 - 0.05f * deltaHours);
            if (Blood.OxygenSaturationSpO2 < 0.60f)
            {
                TakeDamage(10f * deltaHours, DamageType.Asphyxiation, "Oxygen Deprivation");
            }
            return false;
        }

        // Real chemical gas exchange: Inhale O2, bind to Hemoglobin Fe4 centers, exhale CO2 + H2O
        float lungCapacity = Lungs.Length > 0 ? Lungs[0].CapacityLiters : 3.0f;
        float o2Consumed = env.ConsumeOxygen(0.04f * (lungCapacity / 3.0f) * deltaHours);
        Blood.BoundOxygenMoles = Math.Clamp(Blood.BoundOxygenMoles + 0.01f * deltaHours, 0.01f, 0.045f);
        Blood.OxygenSaturationSpO2 = Math.Clamp(Blood.BoundOxygenMoles / 0.045f, 0.90f, 0.99f);

        // Exhale CO2 and moisture back into atmosphere (respiratory quotient ~0.85)
        float co2Exhaled = o2Consumed * 0.85f;
        env.ProduceCarbonDioxide(co2Exhaled);
        env.ProduceWaterVapor(co2Exhaled * 1.1f);

        // Biochemical cellular respiration: C6H12O6 + 6 O2 -> 6 CO2 + 6 H2O + 38 ATP
        PerformCellularRespiration(deltaHours);

        return true;
    }

    protected virtual void PerformCellularRespiration(float deltaHours = 1.0f)
    {
        if (Blood.GlucoseMmolPerL > 2.0f && Blood.BoundOxygenMoles > 0.005f)
        {
            float glucoseConsumedMoles = 0.0005f * deltaHours;
            float o2NeededMoles = glucoseConsumedMoles * 6f;

            if (Blood.BoundOxygenMoles >= o2NeededMoles)
            {
                Blood.BoundOxygenMoles -= o2NeededMoles;
                Blood.GlucoseMmolPerL = Math.Max(3.5f, Blood.GlucoseMmolPerL - 0.01f * deltaHours);

                // 38 ATP synthesized per mole of glucose
                float atpProduced = glucoseConsumedMoles * 38f;
                Blood.AtpMolarPool = Math.Clamp(Blood.AtpMolarPool + atpProduced, 0f, 5.0f);

                // Restore creature energy
                Energy = Math.Clamp(Energy + 2.0f * deltaHours, 0f, 100f);
            }
        }
    }

    public virtual bool Eat(Food food)
    {
        if (!IsAlive) return false;

        if (food.SugarGrams > 5f)
        {
            foreach (var tooth in Teeths)
            {
                tooth.AddCavity(food.SugarGrams * 0.001f);
            }
        }

        bool ingested = Body.Stomach.Ingest(food);
        if (ingested)
        {
            // Absorb chemical nutrients into bloodstream
            Blood.GlucoseMmolPerL = Math.Clamp(Blood.GlucoseMmolPerL + food.GlucoseMoles * 10f, 3.5f, 12.0f);
            Blood.SodiumChlorideGrams = Math.Clamp(Blood.SodiumChlorideGrams + food.SodiumChlorideGrams, 30f, 60f);
            Blood.CalciumElectrolyteMg = Math.Clamp(Blood.CalciumElectrolyteMg + food.CalciumMg, 300f, 800f);
            Blood.IronMineralReserveMg = Math.Clamp(Blood.IronMineralReserveMg + food.IronMg, 2000f, 5000f);

            if (food.ToxinAmount > 0f)
            {
                float remainingToxin = Body.Liver.ProcessToxins(food.ToxinAmount);
                if (remainingToxin > 0f)
                {
                    TakeDamage(remainingToxin, DamageType.Poison, "Food Poisoning");
                }
            }
        }
        return ingested;
    }

    public virtual bool Drink(Water water)
    {
        if (!IsAlive) return false;
        Thirst = Math.Max(0f, Thirst - (water.VolumeMilliliters * water.Purity) / 25f);
        if (water.Contaminants > 0f)
        {
            float toxin = Body.Liver.ProcessToxins(water.Contaminants * 10f);
            if (toxin > 0f) TakeDamage(toxin, DamageType.Poison, "Water Contamination");
        }
        return true;
    }

    public virtual bool Drink(Food.Beverage beverage)
    {
        if (!IsAlive) return false;
        Thirst = Math.Max(0f, Thirst - beverage.WaterContentGrams / 2.5f);
        return true;
    }

    public virtual void Sleep(float hours)
    {
        if (!IsAlive) return;
        Body.Brain.Sleep();
        Energy = Math.Clamp(Energy + hours * 15f, 0f, 100f);
        Body.Brain.CalmDown();
    }

    public virtual void WakeUp()
    {
        Body.Brain.Wake();
    }

    public virtual void TakeDamage(float amt, DamageType type = DamageType.Physical, string? reason = null)
    {
        if (!IsAlive) return;
        Health = Math.Max(0f, Health - amt);
        if (Health <= 0f)
        {
            Die(reason ?? $"Died from {type}");
        }
    }

    public virtual void Heal(float amt)
    {
        if (!IsAlive) return;
        Health = Math.Clamp(Health + amt, 0f, MaxHealth);
    }

    public virtual void Die(string cause)
    {
        IsAlive = false;
        Stage = LifeStage.Deceased;
        CauseOfDeath = cause;
        Body.Heart.CardiacArrest();
        Body.Brain.Consciousness = 0f;
    }

    public void TransmitSTDs(Creature partner)
    {
        foreach (var std in Genitals.ActiveInfections.ToList())
        {
            if (_rng.NextSingle() < std.TransmissionChance)
            {
                partner.Genitals.Contract(std.Clone());
            }
        }

        foreach (var std in partner.Genitals.ActiveInfections.ToList())
        {
            if (_rng.NextSingle() < std.TransmissionChance)
            {
                Genitals.Contract(std.Clone());
            }
        }
    }

    public void TakeAntibiotics()
    {
        Genitals.ActiveInfections.RemoveAll(i => i.CuredByAntibiotics);
    }

    public virtual Creature? GiveBirth()
    {
        if (Womb == null || !Womb.InLabor) return null;
        var baby = Womb.Deliver();
        if (baby != null)
        {
            Energy = Math.Max(10f, Energy - 40f);
            Body.Brain.Endorphins = 1.0f;
            Body.Brain.Dopamine = 1.0f;
            if (Boobs != null)
            {
                Boobs.IsLactating = true;
                Boobs.ProlactinLevel = 1.0f;
                Boobs.OxytocinLevel = 1.0f;
                Boobs.ProduceMilk(24f); // Maternal colostrum & fresh milk ready for nursing
            }
        }
        return baby;
    }

    public virtual bool Reproduce(Creature mate, string? childName = null)
    {
        if (!IsAlive || !mate.IsAlive) return false;
        if (Sex == mate.Sex) return false;

        TransmitSTDs(mate);

        Creature father = Sex == SexType.Male ? this : mate;
        Creature mother = Sex == SexType.Female ? this : mate;

        if (mother.Womb == null || mother.IsPregnant) return false;

        return mother.Womb.Conceive(father, mother, childName, mother.SpeciesGestationDays);
    }

    // =========================================================================
    // --- SPECIES: THE EXTENDED ANIMALIA TAXONOMIC INHERITANCE TREES ---
    // =========================================================================

    // --- 1. HUMAN SPECIES ---

    public class Human : Creature
    {
        public string? Name { get; private set; }
        public string? Occupation { get; set; }
        public float MoralAlignment { get; set; } = 0f;
        public List<string> SpokenLanguages { get; } = new() { "English" };
        public List<string> Inventory { get; } = new();
        public override float SpeciesGestationDays => 280f;
        public override float TeethingAgeYears => 0.5f; // ~6 months for baby teeth
        public override float AdultTeethAgeYears => 6.0f; // ~6 years for adult teeth

        public Human(Human? father = null, Human? mother = null, string? name = null, SexType? sex = null)
            : base(father, mother, teeths: 32, sex: sex)
        {
            Name = name;
            MaxLifespanYears = 80f + _rng.Next(-10, 25);
            if (father == null && mother == null) AgeYears = 25f;
            Body.Brain.IntelligenceQuotient = 100f + _rng.Next(-20, 35);
            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(2, MammaryGland.CupSize.C, 380f);
            }
        }

        public void Fuck()
        {
            Console.WriteLine(Father);
        }

        public bool HaveSexWith(Human partner, string? prospectiveChildName = null)
        {
            return Reproduce(partner, prospectiveChildName);
        }

        public bool HaveSex(Human partner, string? prospectiveChildName = null) => HaveSexWith(partner, prospectiveChildName);
        public bool Fuck(Human partner, string? prospectiveChildName = null) => HaveSexWith(partner, prospectiveChildName);

        public new Human? GiveBirth() => base.GiveBirth() as Human;

        public Food? Forage(Plant plant)
        {
            if (!IsAlive) return null;
            Food? food = plant.Harvest();
            if (food != null)
            {
                Inventory.Add(food.Name);
            }
            return food;
        }

        public void RestUnder(Plant plant)
        {
            if (!IsAlive) return;
            Body.Brain.StressLevel = Math.Max(0f, Body.Brain.StressLevel - plant.PrivacyAndCover * 0.3f);
            Energy = Math.Min(100f, Energy + 5f);
        }

        public void BrushTeeth()
        {
            foreach (var tooth in Teeths)
            {
                tooth.CleanCavity(0.3f);
            }
        }

        public void Speak(string message)
        {
            if (!IsAlive) return;
            Console.WriteLine($"[{Name ?? "Human"}]: \"{message}\"");
        }

        public Food Cook(Food rawFood) => rawFood.Cook();

        public void Exercise(float hours)
        {
            if (!IsAlive) return;
            Energy = Math.Max(0f, Energy - hours * 20f);
            Hunger = Math.Clamp(Hunger + hours * 15f, 0f, 100f);
            Thirst = Math.Clamp(Thirst + hours * 25f, 0f, 100f);
            Body.Heart.BeatsPerMinute = 135f;
            Body.Skin.IsSweating = true;
            Body.Brain.Endorphins = Math.Clamp(Body.Brain.Endorphins + 0.3f, 0f, 1f);
        }
    }

    // =========================================================================
    // --- 2. FELINE (CAT) FAMILY TREE: PANTHERINAE, FELINAE & HYBRIDS ---
    // =========================================================================

    public class Cat : Creature
    {
        public string? Name { get; set; }
        public bool IsPurring { get; protected set; }
        public float PurrFrequencyHz { get; protected set; } = 25f;
        public float NightVisionMultiplier { get; protected set; } = 6.0f;
        public float RawBiteForceN { get; protected set; } = 75f;
        public bool CanRoar { get; protected set; } = false;
        public bool CanPurr { get; protected set; } = true;
        public override float SpeciesGestationDays => 64f;
        public override float TeethingAgeYears => 0.06f; // ~3 weeks for kitten teeth
        public override float AdultTeethAgeYears => 0.45f; // ~5.5 months for adult teeth

        public Cat(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null, int teeths = 30)
            : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 4, teeths: teeths, sex: sex)
        {
            Name = name;
            MaxLifespanYears = 15f + _rng.Next(-3, 6);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 28f };

            if (Eyes.Length >= 2)
            {
                Eyes[0].HasTapetumLucidum = true;
                Eyes[1].HasTapetumLucidum = true;
            }

            foreach (var hand in Hands)
                foreach (var finger in hand.Fingers)
                    finger.Nail.IsRetractable = true;
            foreach (var leg in Legs)
                foreach (var toe in leg.Fingers)
                    toe.Nail.IsRetractable = true;

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(8, MammaryGland.CupSize.A, 35f);
            }
        }

        public virtual void Purr()
        {
            if (!IsAlive || !CanPurr) return;
            IsPurring = true;
            Heal(2.0f);
        }

        public void StopPurring() => IsPurring = false;
        public virtual string Meow() => IsAlive ? $"{Name ?? "Cat"}: Meow~" : "...";

        public virtual bool Climb(Plant.WoodyPlant tree)
        {
            if (!IsAlive || !tree.CanBeClimbed) return false;
            foreach (var hand in Hands)
                foreach (var finger in hand.Fingers)
                    finger.Nail.Extend();
            Purr();
            Console.WriteLine($"[{Name ?? "Cat"}] climbed up into the branches of {tree.Name}!");
            return true;
        }

        public bool Mate(Cat partner, string? kittenName = null) => Reproduce(partner, kittenName);
        public new Cat? GiveBirth() => base.GiveBirth() as Cat;

        // --- SUBFAMILY 1: PANTHERINAE (BIG ROARING CATS) ---

        public abstract class Pantherinae : Cat
        {
            public float RoarDecibels { get; protected set; } = 114f;

            public Pantherinae(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                : base(father, mother, name, sex, teeths: 30)
            {
                CanRoar = true;
                CanPurr = false; // Hyoid apparatus morphology: roaring big cats cannot continuously purr
                NightVisionMultiplier = 8.0f;
            }

            public virtual string Roar()
            {
                if (!IsAlive) return "...";
                return $"[{Name ?? "Big Cat"}]: *ROOOOOOOOAAAR!* (Tremors shake ground at {RoarDecibels} dB!)";
            }

            // --- SPECIES NESTED DIRECTLY INSIDE PANTHERINAE ---

            public class Lion : Pantherinae
            {
                public float ManeVolume { get; set; } = 1.0f;
                public override float SpeciesGestationDays => 110f;

                public Lion(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                    RawBiteForceN = 1000f;
                    MaxLifespanYears = 14f + _rng.Next(-2, 4);
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    Body.Tail = new Tail { LengthCm = 90f };
                }

                public override string Roar() => IsAlive ? $"[{Name ?? "Lion"}]: *GRR-ROOOOAAAR!* (King of the Pride roars!)" : "...";
            }

            public class Tiger : Pantherinae
            {
                public int StripeCount { get; set; } = 100;
                public override float SpeciesGestationDays => 105f;

                public Tiger(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                    RawBiteForceN = 1050f;
                    MaxLifespanYears = 16f + _rng.Next(-2, 5);
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    Body.Tail = new Tail { LengthCm = 100f };
                }

                public void SwimAcrossRiver(float meters)
                {
                    if (!IsAlive) return;
                    Energy = Math.Max(0f, Energy - meters * 0.05f);
                }
            }

            public class Jaguar : Pantherinae
            {
                public override float SpeciesGestationDays => 100f;

                public Jaguar(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                    RawBiteForceN = 1500f;
                    MaxLifespanYears = 15f + _rng.Next(-2, 5);
                }
            }

            public class Leopard : Pantherinae
            {
                public override float SpeciesGestationDays => 95f;

                public Leopard(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                    RawBiteForceN = 900f;
                }

                public void HoistPreyUpTree(Creature prey, Plant.WoodyPlant tree)
                {
                    if (!IsAlive || !tree.CanBeClimbed) return;
                    Console.WriteLine($"[{Name ?? "Leopard"}] effortlessly dragged {prey.GetType().Name} up into {tree.Name}!");
                }
            }

            public class SnowLeopard : Pantherinae
            {
                public float ColdInsulationFat { get; set; } = 1.5f;
                public override float SpeciesGestationDays => 98f;

                public SnowLeopard(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                    Body.Tail = new Tail { LengthCm = 105f };
                }
            }

            // --- FELINE HYBRIDS NESTED DIRECTLY IN PANTHERINAE ---

            public class Liger : Pantherinae
            {
                public float GigantismMultiplier { get; set; } = 1.8f;
                public float WeightKg { get; set; } = 420f;
                public override float SpeciesGestationDays => 108f;

                public Liger(Cat? father, Cat? mother, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                    RawBiteForceN = 1400f;
                    MaxHealth = 200f;
                    Health = 200f;
                }

                public override string Roar() => $"[{Name ?? "Liger"}]: *DEEP EARTH-SHAKING LIGER ROAR!* (Weight: {WeightKg}kg)";
            }

            public class Tigon : Pantherinae
            {
                public bool HasManeWithStripes { get; set; } = true;
                public override float SpeciesGestationDays => 107f;

                public Tigon(Cat? father, Cat? mother, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                    RawBiteForceN = 980f;
                }
            }
        }

        // --- SUBFAMILY 2: FELINAE (PURRING SMALL CATS & BREEDS) ---

        public abstract class Felinae : Cat
        {
            public Felinae(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                : base(father, mother, name, sex)
            {
                CanPurr = true;
                CanRoar = false;
            }

            public class Cheetah : Felinae
            {
                public float TopSpeedKmh { get; set; } = 112f;
                public override float SpeciesGestationDays => 93f;

                public Cheetah(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                    foreach (var hand in Hands)
                        foreach (var f in hand.Fingers)
                            f.Nail.IsRetractable = false;
                }

                public void Sprint(float meters)
                {
                    if (!IsAlive) return;
                    Energy = Math.Max(0f, Energy - meters * 0.1f);
                    Body.Heart.BeatsPerMinute = 220f;
                }
            }

            public class Cougar : Felinae
            {
                public float LeapDistanceMeters { get; set; } = 12f;
                public override float SpeciesGestationDays => 91f;

                public Cougar(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                    RawBiteForceN = 750f;
                }
            }

            public class Lynx : Felinae
            {
                public float EarTuftRadarSensitivity { get; set; } = 2.0f;
                public override float SpeciesGestationDays => 70f;

                public Lynx(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                    Body.Tail = new Tail { LengthCm = 10f };
                }
            }

            // --- DOMESTIC CAT NESTED INSIDE FELINAE ---

            public class DomesticCat : Felinae
            {
                public string Breed { get; protected set; } = "Domestic Shorthair";

                public DomesticCat(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null, string breed = "Domestic Shorthair")
                    : base(father, mother, name, sex)
                {
                    Breed = breed;
                }

                // --- BREEDS NESTED DIRECTLY INSIDE DOMESTICCAT (LEVEL 5 CRAZY NESTING!) ---

                public class MaineCoon : DomesticCat
                {
                    public float WaterResistantFurCoat { get; set; } = 1.0f;

                    public MaineCoon(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name, sex, "Maine Coon")
                    {
                        Body.Tail = new Tail { LengthCm = 40f };
                        MaxHealth = 130f;
                        Health = 130f;
                    }
                }

                public class Persian : DomesticCat
                {
                    public Persian(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name, sex, "Persian")
                    {
                    }
                }

                public class Siamese : DomesticCat
                {
                    public Siamese(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name, sex, "Siamese")
                    {
                        PurrFrequencyHz = 30f;
                    }

                    public override string Meow() => IsAlive ? $"{Name ?? "Siamese"}: *LOUD PIERCING MEEEEOOOOWWW!*" : "...";
                }

                public class Sphynx : DomesticCat
                {
                    public Sphynx(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name, sex, "Sphynx")
                    {
                        Body.Skin.HealthPercent = 95f;
                    }
                }

                public class Bengal : DomesticCat
                {
                    public Bengal(Cat? father = null, Cat? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name, sex, "Bengal (Leopard Cat Hybrid)")
                    {
                        RawBiteForceN = 110f;
                    }
                }
            }
        }
    }

    // =========================================================================
    // --- 3. CANINE (DOG) FAMILY TREE: WILD CANIDS, BREEDS & HYBRIDS ---
    // =========================================================================

    public class Dog : Creature
    {
        public string? Name { get; set; }
        public float Loyalty { get; set; } = 100f;
        public float ScentTrackingMultiplier { get; protected set; } = 40f;
        public float JawClampForceN { get; protected set; } = 320f;
        public override float SpeciesGestationDays => 63f;
        public override float TeethingAgeYears => 0.06f; // ~3 weeks for puppy teeth
        public override float AdultTeethAgeYears => 0.45f; // ~5.5 months for adult teeth

        public Dog(Dog? father = null, Dog? mother = null, string? name = null, SexType? sex = null)
            : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 4, teeths: 42, sex: sex)
        {
            Name = name;
            MaxLifespanYears = 13f + _rng.Next(-2, 5);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 35f };
            Body.Nose.OlfactorySensitivity = 100.0f;

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(8, MammaryGland.CupSize.A, 50f);
            }
        }

        public virtual string Bark() => IsAlive ? $"{Name ?? "Dog"}: Woof! Woof!" : "...";

        public virtual void WagTail()
        {
            if (!IsAlive) return;
            Body.Tail?.Wag(1.0f);
            Body.Brain.Dopamine = Math.Clamp(Body.Brain.Dopamine + 0.2f, 0f, 1f);
        }

        public bool Mate(Dog partner, string? puppyName = null) => Reproduce(partner, puppyName);
        public new Dog? GiveBirth() => base.GiveBirth() as Dog;

        // --- SUBFAMILY 1: WILD CANIDS (NESTED IN DOG) ---

        public abstract class WildCanid : Dog
        {
            public string PackRank { get; set; } = "Beta";

            public WildCanid(Dog? father = null, Dog? mother = null, string? name = null, SexType? sex = null)
                : base(father, mother, name, sex)
            {
                ScentTrackingMultiplier = 80f;
                JawClampForceN = 450f;
            }

            public virtual string Howl() => IsAlive ? $"{Name ?? "Wild Canid"}: Awooooooooooo!" : "...";

            // --- SPECIES NESTED DIRECTLY IN WILDCANID ---

            public class WolfCanid : WildCanid
            {
                public WolfCanid(Dog? father = null, Dog? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                    MaxLifespanYears = 14f + _rng.Next(-2, 4);
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    Body.Tail = new Tail { LengthCm = 42f };
                }

                public override string Howl() => IsAlive ? $"{Name ?? "Apex Wolf"}: *AWOOOOOOOOOOOOOOOOO!* (Echoes across the valley)" : "...";
            }

            public class Coyote : WildCanid
            {
                public Coyote(Dog? father = null, Dog? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                }

                public string Yap() => IsAlive ? $"{Name ?? "Coyote"}: *Yap-yap-yip-awooo!*" : "...";
            }

            public class Dingo : WildCanid
            {
                public Dingo(Dog? father = null, Dog? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                }
            }
        }

        // --- SUBFAMILY 2: DOMESTIC DOG BREEDS (NESTED IN DOG) ---

        public abstract class DomesticCanid : Dog
        {
            public string Breed { get; protected set; } = "Canine";

            public DomesticCanid(Dog? father = null, Dog? mother = null, string? name = null, SexType? sex = null, string breed = "Canine")
                : base(father, mother, name, sex)
            {
                Breed = breed;
            }

            // --- BREEDS NESTED DIRECTLY IN DOMESTICCANID (LEVEL 4 CRAZY NESTING!) ---

            public class GermanShepherd : DomesticCanid
            {
                public GermanShepherd(Dog? father = null, Dog? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex, "German Shepherd")
                {
                    JawClampForceN = 500f;
                    Body.Brain.IntelligenceQuotient = 120f;
                }
            }

            public class SiberianHusky : DomesticCanid
            {
                public SiberianHusky(Dog? father = null, Dog? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex, "Siberian Husky")
                {
                }

                public string HowlChorus() => IsAlive ? $"{Name ?? "Husky"}: *HUUUUUUUSKYYYY AWOOOO!*" : "...";
            }

            public class Bloodhound : DomesticCanid
            {
                public Bloodhound(Dog? father = null, Dog? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex, "Bloodhound")
                {
                    ScentTrackingMultiplier = 200f;
                    Body.Nose.OlfactorySensitivity = 300f;
                }

                public bool TrackScent(Creature target)
                {
                    if (!IsAlive) return false;
                    Console.WriteLine($"[{Name ?? "Bloodhound"}] detected scent trail of {target.GetType().Name}!");
                    return true;
                }
            }

            public class GoldenRetriever : DomesticCanid
            {
                public bool HasSoftMouthGrip { get; set; } = true;

                public GoldenRetriever(Dog? father = null, Dog? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex, "Golden Retriever")
                {
                    Loyalty = 150f;
                }
            }

            public class Chihuahua : DomesticCanid
            {
                public bool IsShivering { get; set; } = true;

                public Chihuahua(Dog? father = null, Dog? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex, "Chihuahua")
                {
                    MaxHealth = 40f;
                    Health = 40f;
                    JawClampForceN = 40f;
                }

                public override string Bark() => IsAlive ? $"{Name ?? "Chihuahua"}: *YAP! YAP! YAP! GRRR!*" : "...";
            }
        }

        // --- SUBFAMILY 3: CANINE HYBRIDS (NESTED IN DOG) ---

        public abstract class CanineHybrid : Dog
        {
            public CanineHybrid(Creature? father, Creature? mother, string? name = null, SexType? sex = null)
                : base(father as Dog, mother as Dog, name, sex)
            {
            }

            public class Wolfdog : CanineHybrid
            {
                public float WolfBloodPercentage { get; set; } = 50f;

                public Wolfdog(Creature? father, Creature? mother, string? name = null, SexType? sex = null)
                    : base(father, mother, name, sex)
                {
                    JawClampForceN = 460f;
                    ScentTrackingMultiplier = 90f;
                }

                public string HowlOrBark() => IsAlive ? $"{Name ?? "Wolfdog"}: *Awooo-Bark!*" : "...";
            }
        }
    }

    // --- Backward Compatible Wolf Alias ---
    public class Wolf : Dog.WildCanid.WolfCanid
    {
        public Wolf(Wolf? father = null, Wolf? mother = null, SexType? sex = null)
            : base(father, mother, "Apex Grey Wolf", sex)
        {
        }
    }

    // --- 4. BIRDS, FISH, HORSES ---

    public class Bird : Creature
    {
        public string? SpeciesName { get; set; } = "Sparrow";
        public bool IsFlying { get; private set; } = false;

        public Bird(Bird? father = null, Bird? mother = null, string? species = "Eagle", SexType? sex = null)
            : base(father, mother, hands: 0, handFingers: 0, legs: 2, legFingers: 4, teeths: 0, sex: sex)
        {
            SpeciesName = species;
            MaxLifespanYears = 10f + _rng.Next(-2, 10);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Wings = [new Wing { WingspanMeters = 2.0f }, new Wing { WingspanMeters = 2.0f }];
            Body.Tail = new Tail { LengthCm = 20f };
            Body.Skeleton.BoneDensity = 0.5f;
        }

        public bool Fly()
        {
            if (!IsAlive || Wings == null || !Wings[0].CanFly || !Wings[1].CanFly) return false;
            IsFlying = true;
            Energy = Math.Max(0f, Energy - 5f);
            return true;
        }

        public void Land() => IsFlying = false;

        public Egg? LayEgg(Bird father)
        {
            if (!IsAlive || Sex != SexType.Female) return null;
            TransmitSTDs(father);
            return new Egg(father, this);
        }

        public class Egg(Bird father, Bird mother)
        {
            public Bird Father { get; } = father;
            public Bird Mother { get; } = mother;
            public float IncubationDays { get; private set; } = 0f;
            public float RequiredDays { get; } = 21f;

            public Bird? Incubate(float days)
            {
                IncubationDays += days;
                if (IncubationDays >= RequiredDays)
                {
                    return new Bird(Father, Mother, Mother.SpeciesName)
                    {
                        AgeYears = 0f,
                        Stage = LifeStage.Newborn
                    };
                }
                return null;
            }
        }

        // --- AVIAN ORDERS NESTED IN BIRD ---

        // ORDER 1: ACCIPITRIFORMES (EAGLES, HAWKS, VULTURES, OSPREYS)
        public class Accipitriformes : Bird
        {
            public float TalonGripForcePsi { get; protected set; } = 300f;
            public float BinocularVisionMagnification { get; protected set; } = 8.0f;

            public Accipitriformes(Bird? father = null, Bird? mother = null, string? species = "Raptor", SexType? sex = null)
                : base(father, mother, species, sex)
            {
                Wings = [new Wing { WingspanMeters = 2.2f }, new Wing { WingspanMeters = 2.2f }];
            }

            public class Accipitridae : Accipitriformes
            {
                public Accipitridae(Bird? father = null, Bird? mother = null, string? species = "Eagle", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class Aquila : Accipitridae
                {
                    public Aquila(Bird? father = null, Bird? mother = null, string? species = "Golden Eagle", SexType? sex = null)
                        : base(father, mother, species, sex) { }

                    public class GoldenEagle : Aquila
                    {
                        public GoldenEagle(Bird? father = null, Bird? mother = null, SexType? sex = null)
                            : base(father, mother, "Golden Eagle (Aquila chrysaetos)", sex)
                        {
                            TalonGripForcePsi = 440f;
                            MaxLifespanYears = 30f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string TalonStrike(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(55f, DamageType.Piercing, "Eagle talon crush");
                            return $"{SpeciesName} descends in 320 km/h dive, clamping {TalonGripForcePsi:F0} PSI talons to crush {prey.GetType().Name}!";
                        }
                    }
                }

                public class Haliaeetus : Accipitridae
                {
                    public Haliaeetus(Bird? father = null, Bird? mother = null, string? species = "Sea Eagle", SexType? sex = null)
                        : base(father, mother, species, sex) { }

                    public class BaldEagle : Haliaeetus
                    {
                        public BaldEagle(Bird? father = null, Bird? mother = null, SexType? sex = null)
                            : base(father, mother, "Bald Eagle (Haliaeetus leucocephalus)", sex)
                        {
                            MaxLifespanYears = 28f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string SnatchFish(Fish fish)
                        {
                            if (!fish.IsAlive) return "...";
                            fish.TakeDamage(60f, DamageType.Piercing, "Bald eagle water snatch");
                            return $"{SpeciesName} skims water surface, locking spiculate talons onto {fish.SpeciesName}!";
                        }
                    }
                }

                public class Buteo : Accipitridae
                {
                    public Buteo(Bird? father = null, Bird? mother = null, string? species = "Hawk", SexType? sex = null)
                        : base(father, mother, species, sex) { }

                    public class RedTailedHawk : Buteo
                    {
                        public RedTailedHawk(Bird? father = null, Bird? mother = null, SexType? sex = null)
                            : base(father, mother, "Red-Tailed Hawk (Buteo jamaicensis)", sex)
                        {
                            MaxLifespanYears = 21f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string CryKreeee() => $"{SpeciesName}: *KREEEEEE-AAAAR!* (Piercing territorial raptor scream)";
                    }
                }
            }

            public class Cathartidae : Accipitriformes
            {
                public Cathartidae(Bird? father = null, Bird? mother = null, string? species = "Vulture", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class AndeanCondor : Cathartidae
                {
                    public AndeanCondor(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Andean Condor (Vultur gryphus)", sex)
                    {
                        Wings = [new Wing { WingspanMeters = 3.3f }, new Wing { WingspanMeters = 3.3f }];
                        MaxLifespanYears = 70f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SoarThermals(float hours = 5f) => $"{SpeciesName} spreads 3.3m wingspan, soaring for {hours:F1} hours across Andean cordilleras without a single wing flap!";
                }
            }

            public class Pandionidae : Accipitriformes
            {
                public Pandionidae(Bird? father = null, Bird? mother = null, string? species = "Osprey", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class WesternOsprey : Pandionidae
                {
                    public WesternOsprey(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Western Osprey (Pandion haliaetus)", sex)
                    {
                        MaxLifespanYears = 25f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ReversibleTalonPlunge() => $"{SpeciesName} rotates outer toe backward (zygodactyly), plunging completely underwater to secure slippery marine prey!";
                }
            }
        }

        // ORDER 2: STRIGIFORMES (OWLS)
        public class Strigiformes : Bird
        {
            public float FacialDiscSoundAmplification { get; protected set; } = 10.0f;

            public Strigiformes(Bird? father = null, Bird? mother = null, string? species = "Owl", SexType? sex = null)
                : base(father, mother, species, sex)
            {
                Wings = [new Wing { WingspanMeters = 1.2f }, new Wing { WingspanMeters = 1.2f }];
            }

            public class Strigidae : Strigiformes
            {
                public Strigidae(Bird? father = null, Bird? mother = null, string? species = "Typical Owl", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class GreatHornedOwl : Strigidae
                {
                    public GreatHornedOwl(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Great Horned Owl (Bubo virginianus)", sex)
                    {
                        MaxLifespanYears = 28f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string Hoot() => $"{SpeciesName}: *Hoo-hoo hooooo hoo-hoo!* (Deep resonant territorial call)";
                }

                public class SnowyOwl : Strigidae
                {
                    public SnowyOwl(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Snowy Owl (Bubo scandiacus)", sex)
                    {
                        MaxLifespanYears = 24f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ArcticTundraHunt() => $"{SpeciesName} utilizes dense foot plumage to walk on Arctic snowdrifts, tracking lemmings under the crust!";
                }
            }

            public class Tytonidae : Strigiformes
            {
                public Tytonidae(Bird? father = null, Bird? mother = null, string? species = "Barn Owl", SexType? sex = null)
                    : base(father, mother, species, sex) { }
            }

            public class BarnOwl : Tytonidae
            {
                public BarnOwl(Bird? father = null, Bird? mother = null, SexType? sex = null)
                    : base(father, mother, "Barn Owl (Tyto alba)", sex)
                {
                    MaxLifespanYears = 15f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string RotateHead(float degrees = 270f) => $"{SpeciesName} swivels cervical vertebrae {degrees:F0}° without moving its body, scanning with asymmetrical ear openings!";
                public string SilentFlight() => $"{SpeciesName} flutters velvet-down serrated feathers, gliding in 0 dB silent flight to ambush prey!";
            }
        }

        // ORDER 3: FALCONIFORMES (TRUE FALCONS)
        public class Falconiformes : Bird
        {
            public Falconiformes(Bird? father = null, Bird? mother = null, string? species = "Falcon", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Falconidae : Falconiformes
            {
                public Falconidae(Bird? father = null, Bird? mother = null, string? species = "Falcon", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class Gyrfalcon : Falconidae
                {
                    public Gyrfalcon(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Gyrfalcon (Falco rusticolus)", sex)
                    {
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ArcticSesh() => $"{SpeciesName} dominates circumpolar Arctic tundra cliffs as the world's largest true falcon!";
                }

                public class AmericanKestrel : Falconidae
                {
                    public AmericanKestrel(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "American Kestrel (Falco sparverius)", sex)
                    {
                        MaxLifespanYears = 14f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string HoverHunt() => $"{SpeciesName} hovers stationary into headwind with rapid wingbeats, tracking ultraviolet rodent urine trails!";
                }
            }

            public class PeregrineFalcon : Falconidae
            {
                public PeregrineFalcon(Bird? father = null, Bird? mother = null, SexType? sex = null)
                    : base(father, mother, "Peregrine Falcon (Falco peregrinus)", sex)
                {
                    MaxLifespanYears = 17f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string DiveStoop(Creature prey)
                {
                    if (!IsAlive || !prey.IsAlive) return "...";
                    prey.TakeDamage(50f, DamageType.Blunt, "High-velocity stoop impact");
                    return $"{SpeciesName} tucks wings and plunges in a 389 km/h terminal stoop, striking {prey.GetType().Name} with concussive talon impact!";
                }
            }
        }

        // ORDER 4: SPHENISCIFORMES (PENGUINS)
        public class Sphenisciformes : Bird
        {
            public Sphenisciformes(Bird? father = null, Bird? mother = null, string? species = "Penguin", SexType? sex = null)
                : base(father, mother, species, sex)
            {
                Wings = [new Wing { WingspanMeters = 0.5f }, new Wing { WingspanMeters = 0.5f }];
            }

            public class Spheniscidae : Sphenisciformes
            {
                public Spheniscidae(Bird? father = null, Bird? mother = null, string? species = "Penguin", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class KingPenguin : Spheniscidae
                {
                    public KingPenguin(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "King Penguin (Aptenodytes patagonicus)", sex)
                    {
                        MaxLifespanYears = 26f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string IncubateFootEgg() => $"{SpeciesName} balances a single egg atop warm webbed feet under dense abdominal brood pouch fold!";
                }

                public class AdeliePenguin : Spheniscidae
                {
                    public AdeliePenguin(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Adelie Penguin (Pygoscelis adeliae)", sex)
                    {
                        MaxLifespanYears = 16f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string PorpoiseJump() => $"{SpeciesName} porpoises out of water at 15 km/h, leaping onto Antarctic sea ice shelf!";
                }
            }

            public class EmperorPenguin : Spheniscidae
            {
                public float BlubberThicknessCm { get; } = 3.5f;

                public EmperorPenguin(Bird? father = null, Bird? mother = null, SexType? sex = null)
                    : base(father, mother, "Emperor Penguin (Aptenodytes forsteri)", sex)
                {
                    MaxLifespanYears = 20f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string DeepOceanDive(float depthMeters) => $"{SpeciesName} insulates with dense blubber, plunging {depthMeters:F0}m into icy Antarctic ocean depths!";
            }
        }

        // ORDER 5: PSITTACIFORMES (PARROTS, MACAWS, COCKATOOS)
        public class Psittaciformes : Bird
        {
            public float BeakCrushingForceN { get; protected set; } = 400f;

            public Psittaciformes(Bird? father = null, Bird? mother = null, string? species = "Parrot", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Psittacidae : Psittaciformes
            {
                public Psittacidae(Bird? father = null, Bird? mother = null, string? species = "True Parrot", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class HyacinthMacaw : Psittacidae
                {
                    public HyacinthMacaw(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Hyacinth Macaw (Anodorhynchus hyacinthinus)", sex)
                    {
                        BeakCrushingForceN = 1400f; // Can shatter Brazil nuts
                        MaxLifespanYears = 60f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string CrackNut(string nutType = "Bocaiuva Palm Nut") => $"{SpeciesName} applies {BeakCrushingForceN} N crushing beak force, shattering rock-hard {nutType} effortlessly!";
                }

                public class AfricanGreyParrot : Psittacidae
                {
                    public AfricanGreyParrot(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "African Grey Parrot (Psittacus erithacus)", sex)
                    {
                        MaxLifespanYears = 65f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SpeakSyntax(string speech) => $"{SpeciesName} displays cognitive numerical comprehension: \"{speech}\"!";
                }
            }

            public class Cacatuidae : Psittaciformes
            {
                public Cacatuidae(Bird? father = null, Bird? mother = null, string? species = "Cockatoo", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class SulphurCrestedCockatoo : Cacatuidae
                {
                    public SulphurCrestedCockatoo(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Sulphur-Crested Cockatoo (Cacatua galerita)", sex)
                    {
                        MaxLifespanYears = 70f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string FlareCrest() => $"{SpeciesName}: *SCREEEEEECH!* (Erects bright sulphur-yellow cephalic crest in emotional display)";
                }
            }

            public class Nestoridae : Psittaciformes
            {
                public Nestoridae(Bird? father = null, Bird? mother = null, string? species = "Kea", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class Kea : Nestoridae
                {
                    public Kea(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Kea Alpine Parrot (Nestor notabilis)", sex)
                    {
                        MaxLifespanYears = 50f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SolvePuzzle() => $"{SpeciesName} dismantles multi-stage mechanical latches with inquisitive alpine problem-solving intelligence!";
                }
            }

            public class ScarletMacaw : Psittacidae
            {
                public ScarletMacaw(Bird? father = null, Bird? mother = null, SexType? sex = null)
                    : base(father, mother, "Scarlet Macaw (Ara macao)", sex)
                {
                    MaxLifespanYears = 50f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string VocalMimic(string phrase) => $"{SpeciesName} vocalizes with zygodactylous dexterity: \"{phrase}!\" with high avian intelligence!";
            }
        }

        // ORDER 6: PASSERIFORMES (SONGBIRDS, CORVIDS, BIRDS OF PARADISE)
        public class Passeriformes : Bird
        {
            public float SyringialMusclesCount { get; } = 6f; // Complex melodic vocal cords

            public Passeriformes(Bird? father = null, Bird? mother = null, string? species = "Songbird", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Corvidae : Passeriformes
            {
                public float CognitiveIq { get; protected set; } = 135f;

                public Corvidae(Bird? father = null, Bird? mother = null, string? species = "Corvid", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class CommonRaven : Corvidae
                {
                    public CommonRaven(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Common Raven (Corvus corax)", sex)
                    {
                        MaxLifespanYears = 25f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string CraftTool() => $"{SpeciesName} trims barbed twig into a hook tool to extract hidden beetle larvae!";
                    public string Croak() => $"{SpeciesName}: *Gronk... Gronk...* (Deep guttural intelligence vocalization)";
                }

                public class BlueJay : Corvidae
                {
                    public BlueJay(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Blue Jay (Cyanocitta cristata)", sex)
                    {
                        MaxLifespanYears = 17f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string CacheAcorns(int count = 50) => $"{SpeciesName} stores {count} acorns in subterranean caches, planting future oak forests!";
                }

                public class EurasianMagpie : Corvidae
                {
                    public EurasianMagpie(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Eurasian Magpie (Pica pica)", sex)
                    {
                        MaxLifespanYears = 15f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string RecognizeSelfInMirror() => $"{SpeciesName} inspects red sticker on its neck in mirror reflection, demonstrating self-awareness!";
                }
            }

            public class Paradisaeidae : Passeriformes
            {
                public Paradisaeidae(Bird? father = null, Bird? mother = null, string? species = "Bird of Paradise", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class GreaterBirdOfParadise : Paradisaeidae
                {
                    public GreaterBirdOfParadise(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Greater Bird of Paradise (Paradisaea apoda)", sex)
                    {
                        MaxLifespanYears = 18f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string CourtshipDance() => $"{SpeciesName} flares yellow and chestnut flank plumes into an iridescent canopy fan, performing lek mating display!";
                }
            }

            public class Hirundinidae : Passeriformes
            {
                public Hirundinidae(Bird? father = null, Bird? mother = null, string? species = "Swallow", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class BarnSwallow : Hirundinidae
                {
                    public BarnSwallow(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Barn Swallow (Hirundo rustica)", sex)
                    {
                        MaxLifespanYears = 8f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string AerialInsectHawking() => $"{SpeciesName} twists in high-G banked turns, snapping winged gnats from mid-air over pastoral fields!";
                }
            }
        }

        // ORDER 7: ANSERIFORMES (WATERFOWL: DUCKS, SWANS, GEESE)
        public class Anseriformes : Bird
        {
            public bool HasUropygialWaterproofGland { get; } = true;

            public Anseriformes(Bird? father = null, Bird? mother = null, string? species = "Waterfowl", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Anatidae : Anseriformes
            {
                public Anatidae(Bird? father = null, Bird? mother = null, string? species = "Duck/Goose", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class MallardDuck : Anatidae
                {
                    public MallardDuck(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Mallard Duck (Anas platyrhynchos)", sex)
                    {
                        MaxLifespanYears = 12f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string Quack() => $"{SpeciesName}: *QUACK! Quack-quack-quack!*";
                    public string DabbleFeed() => $"{SpeciesName} tips tail-up in marsh shallow water, filtering aquatic invertebrates!";
                }

                public class MuteSwan : Anatidae
                {
                    public MuteSwan(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Mute Swan (Cygnus olor)", sex)
                    {
                        Wings = [new Wing { WingspanMeters = 2.4f }, new Wing { WingspanMeters = 2.4f }];
                        MaxLifespanYears = 25f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string GlideGracefully() => $"{SpeciesName} arches immaculate white neck and glides placidly across river waters!";
                }

                public class CanadaGoose : Anatidae
                {
                    public CanadaGoose(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Canada Goose (Branta canadensis)", sex)
                    {
                        MaxLifespanYears = 24f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string FlyVFormation(float altitudeMeters = 2000f) => $"{SpeciesName} leads flock in aerodynamic V-formation at {altitudeMeters:F0}m altitude, reducing drag for trailing birds!";
                    public string Honk() => $"{SpeciesName}: *HONK! HONK!*";
                }
            }
        }

        // ORDER 8: PELECANIFORMES & SULIFORMES (PELICANS, HERONS, BOOBIES)
        public class Pelecaniformes : Bird
        {
            public Pelecaniformes(Bird? father = null, Bird? mother = null, string? species = "Water Bird", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Pelecanidae : Pelecaniformes
            {
                public Pelecanidae(Bird? father = null, Bird? mother = null, string? species = "Pelican", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class AustralianPelican : Pelecanidae
                {
                    public float GularPouchCapacityLiters { get; } = 13.0f;

                    public AustralianPelican(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Australian Pelican (Pelecanus conspicillatus)", sex)
                    {
                        MaxLifespanYears = 25f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ScoopWater(Fish target) => $"{SpeciesName} expands {GularPouchCapacityLiters}L gular pouch, scooping {target.SpeciesName} with 47cm bill!";
                }
            }

            public class Ardeidae : Pelecaniformes
            {
                public Ardeidae(Bird? father = null, Bird? mother = null, string? species = "Heron", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class GreatBlueHeron : Ardeidae
                {
                    public GreatBlueHeron(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Great Blue Heron (Ardea herodias)", sex)
                    {
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SpearFish(Fish fish)
                    {
                        if (!fish.IsAlive) return "...";
                        fish.TakeDamage(40f, DamageType.Piercing, "Heron dagger bill");
                        return $"{SpeciesName} stands motionless in reeds, thrusting dagger bill forward to impale {fish.SpeciesName}!";
                    }
                }
            }
        }

        // ORDER 9: APODIFORMES (HUMMINGBIRDS & SWIFTS)
        public class Apodiformes : Bird
        {
            public float WingBeatFrequencyHz { get; protected set; } = 70f;

            public Apodiformes(Bird? father = null, Bird? mother = null, string? species = "Hummingbird/Swift", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Trochilidae : Apodiformes
            {
                public Trochilidae(Bird? father = null, Bird? mother = null, string? species = "Hummingbird", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class RubyThroatedHummingbird : Trochilidae
                {
                    public bool InTorpor { get; private set; } = false;

                    public RubyThroatedHummingbird(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Ruby-Throated Hummingbird (Archilochus colubris)", sex)
                    {
                        WingBeatFrequencyHz = 80f;
                        MaxLifespanYears = 6f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string HoverDrinkNectar(Plant flower) => $"{SpeciesName} flaps wings {WingBeatFrequencyHz:F0} times/sec in figure-8 pattern, hovering stationary to sip nectar from {flower.Name}!";

                    public string EnterNocturnalTorpor()
                    {
                        InTorpor = true;
                        BodyTemperatureCelsius = 18f;
                        return $"{SpeciesName} drops body temperature from 40°C to 18°C, entering torpor to survive night without starvation!";
                    }
                }
            }

            public class Apodidae : Apodiformes
            {
                public Apodidae(Bird? father = null, Bird? mother = null, string? species = "Swift", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class CommonSwift : Apodidae
                {
                    public CommonSwift(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Common Swift (Apus apus)", sex)
                    {
                        MaxLifespanYears = 21f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SleepOnTheWing() => $"{SpeciesName} ascends to 3,000m, sleeping mid-flight using unihemispheric slow-wave brain states!";
                }
            }
        }

        // ORDER 10: PICIFORMES (WOODPECKERS)
        public class Piciformes : Bird
        {
            public Piciformes(Bird? father = null, Bird? mother = null, string? species = "Woodpecker", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Picidae : Piciformes
            {
                public Picidae(Bird? father = null, Bird? mother = null, string? species = "Woodpecker", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class PileatedWoodpecker : Picidae
                {
                    public float DecelerationToleranceGs { get; } = 1200f;

                    public PileatedWoodpecker(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Pileated Woodpecker (Dryocopus pileatus)", sex)
                    {
                        MaxLifespanYears = 13f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DrumWood(Plant tree) => $"{SpeciesName} hammer-pecks {tree.Name} bark at 20 strikes/sec, cushioned by shock-absorbing skull hyoid apparatus ({DecelerationToleranceGs} Gs)!";
                }
            }
        }

        // ORDER 11: STRUTHIONIFORMES & RATITES (FLIGHTLESS GIANT BIRDS)
        public class Struthioniformes : Bird
        {
            public Struthioniformes(Bird? father = null, Bird? mother = null, string? species = "Ratite", SexType? sex = null)
                : base(father, mother, species, sex)
            {
                Wings = [new Wing { WingspanMeters = 0.8f }, new Wing { WingspanMeters = 0.8f }];
            }

            public class Struthionidae : Struthioniformes
            {
                public Struthionidae(Bird? father = null, Bird? mother = null, string? species = "Ostrich", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class CommonOstrich : Struthionidae
                {
                    public float SprintSpeedKmh { get; } = 70f;

                    public CommonOstrich(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Common Ostrich (Struthio camelus)", sex)
                    {
                        MaxLifespanYears = 45f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string Sprint(float distanceKm) => $"{SpeciesName} bolts across savanna at {SprintSpeedKmh} km/h on two-toed biomechanical elastic tendon legs!";
                    public string DefensiveKick(Creature predator)
                    {
                        if (!IsAlive || !predator.IsAlive) return "...";
                        predator.TakeDamage(70f, DamageType.Blunt, "Ostrich 2000N leg kick");
                        return $"{SpeciesName} delivers a 2,000 N downward forward kick equipped with a 10cm lethal claw!";
                    }
                }
            }

            public class Casuariidae : Struthioniformes
            {
                public Casuariidae(Bird? father = null, Bird? mother = null, string? species = "Cassowary/Emu", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class SouthernCassowary : Casuariidae
                {
                    public SouthernCassowary(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Southern Cassowary (Casuarius casuarius)", sex)
                    {
                        MaxLifespanYears = 40f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DaggerClawSlash(Creature target)
                    {
                        if (!IsAlive || !target.IsAlive) return "...";
                        target.TakeDamage(85f, DamageType.Slashing, "Cassowary 12cm dagger claw");
                        return $"{SpeciesName} leaps 1.5m and slashes with 12cm razor-sharp inner toe dagger claw!";
                    }
                }

                public class Emu : Casuariidae
                {
                    public Emu(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Emu (Dromaius novaehollandiae)", sex)
                    {
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string EnduranceSprint() => $"{SpeciesName} runs 50 km/h across Australian outback with energy-recycling gastrocnemius muscles!";
                }
            }

            public class Apterygidae : Struthioniformes
            {
                public Apterygidae(Bird? father = null, Bird? mother = null, string? species = "Kiwi", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class NorthIslandBrownKiwi : Apterygidae
                {
                    public NorthIslandBrownKiwi(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "North Island Brown Kiwi (Apteryx mantelli)", sex)
                    {
                        MaxLifespanYears = 30f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SniffSubterraneanPrey() => $"{SpeciesName} sniffs moist soil with terminal nostrils at the tip of its bill, detecting buried earthworms!";
                }
            }
        }

        // ORDER 12: PHOENICOPTERIFORMES (FLAMINGOS)
        public class Phoenicopteriformes : Bird
        {
            public Phoenicopteriformes(Bird? father = null, Bird? mother = null, string? species = "Flamingo", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Phoenicopteridae : Phoenicopteriformes
            {
                public Phoenicopteridae(Bird? father = null, Bird? mother = null, string? species = "Flamingo", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class GreaterFlamingo : Phoenicopteridae
                {
                    public GreaterFlamingo(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Greater Flamingo (Phoenicopterus roseus)", sex)
                    {
                        MaxLifespanYears = 40f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string FilterFeedAlgae() => $"{SpeciesName} turns curved bill upside-down in hypersaline lake, filtering spirulina cyanobacteria rich in pink canthaxanthin carotenoids!";
                    public string StandOnOneLeg() => $"{SpeciesName} locks unipedal passive gravitational stay-apparatus, conserving metabolic thermal heat!";
                }
            }
        }

        // ORDER 13: COLUMBIFORMES (PIGEONS, DOVES & DODOS)
        public class Columbiformes : Bird
        {
            public Columbiformes(Bird? father = null, Bird? mother = null, string? species = "Pigeon", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Columbidae : Columbiformes
            {
                public Columbidae(Bird? father = null, Bird? mother = null, string? species = "Pigeon", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class RockDove : Columbidae
                {
                    public RockDove(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Rock Dove Homing Pigeon (Columba livia)", sex)
                    {
                        MaxLifespanYears = 15f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string NavigateGeomagneticField() => $"{SpeciesName} navigates home from 1,800 km away using magnetite crystals in upper beak and cryptochrome retinal compass!";
                    public string RegurgitateCropMilk() => $"{SpeciesName} sloughs prolactin-stimulated epithelial cells from crop sac, producing protein-rich crop milk for squabs!";
                }

                public class Dodo : Columbidae
                {
                    public float WeightKg { get; } = 18.0f;

                    public Dodo(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Dodo of Mauritius (Raphus cucullatus)", sex)
                    {
                        MaxLifespanYears = 25f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SwallowTambalacoqueSeeds() => $"{SpeciesName} crushes fibrous Calvaria tree seeds in muscular gizzard using swallowed volcanic gizzard stones!";
                }
            }
        }

        // ORDER 14: PROCELLARIIFORMES (TUBENOSES: ALBATROSSES & PETRELS)
        public class Procellariiformes : Bird
        {
            public Procellariiformes(Bird? father = null, Bird? mother = null, string? species = "Tubenose", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Diomedeidae : Procellariiformes
            {
                public Diomedeidae(Bird? father = null, Bird? mother = null, string? species = "Albatross", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class WanderingAlbatross : Diomedeidae
                {
                    public float WingspanMeters { get; } = 3.5f;

                    public WanderingAlbatross(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Wandering Albatross (Diomedea exulans)", sex)
                    {
                        MaxLifespanYears = 50f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DynamicSoaringCircumpolar(float distanceKm = 10000f) => $"{SpeciesName} locks shoulder tendon stay-apparatus, dynamically soaring {distanceKm:F0} km over Southern Ocean waves without a single wing flap!";
                    public string ExcreteOceanSalt() => $"{SpeciesName} drips hypersaline brine from tubular nostrils, desalinating seawater via supraorbital salt glands!";
                }
            }

            public class Procellariidae : Procellariiformes
            {
                public Procellariidae(Bird? father = null, Bird? mother = null, string? species = "Petrel", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class NorthernGiantPetrel : Procellariidae
                {
                    public NorthernGiantPetrel(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Northern Giant Petrel (Macronectes halli)", sex)
                    {
                        MaxLifespanYears = 35f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ProjectFoulStomachOil(Creature threat)
                    {
                        if (!threat.IsAlive) return "...";
                        threat.TakeDamage(10f, DamageType.Poison, "Noxious proventricular stomach oil spray");
                        return $"{SpeciesName} vomits foul wax-ester proventricular stomach oil 2 meters with projectile accuracy, destroying {threat.GetType().Name}'s feather waterproofing!";
                    }
                }
            }
        }

        // ORDER 15: CUCULIFORMES (CUCKOOS & ROADRUNNERS)
        public class Cuculiformes : Bird
        {
            public Cuculiformes(Bird? father = null, Bird? mother = null, string? species = "Cuckoo", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Cuculidae : Cuculiformes
            {
                public Cuculidae(Bird? father = null, Bird? mother = null, string? species = "Cuckoo", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class CommonCuckoo : Cuculidae
                {
                    public CommonCuckoo(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Common Cuckoo (Cuculus canorus)", sex)
                    {
                        MaxLifespanYears = 13f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ObligateBroodParasitism(Bird host) => $"{SpeciesName} deposits mimetic egg in {host.SpeciesName}'s nest in 10 seconds, leaving parental care to unwitting host!";
                }

                public class GreaterRoadrunner : Cuculidae
                {
                    public float RunningSpeedKmh { get; } = 32f;

                    public GreaterRoadrunner(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Greater Roadrunner (Geococcyx californianus)", sex)
                    {
                        MaxLifespanYears = 8f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SprintPursuitAndStrikeRattlesnake(Creature rattlesnake)
                    {
                        if (!IsAlive || !rattlesnake.IsAlive) return "...";
                        rattlesnake.TakeDamage(45f, DamageType.Slashing, "Roadrunner dagger beak thrash");
                        return $"{SpeciesName} dashes at {RunningSpeedKmh:F0} km/h, pins {rattlesnake.GetType().Name} with zygodactyl feet, and batters skull against desert caliche rocks!";
                    }

                    public string SunbatheDorsalSkin() => $"{SpeciesName} erects neck feathers to expose pigmented black dorsal apterium skin, absorbing morning desert solar heat!";
                }
            }
        }

        // ORDER 16: CAPRIMULGIFORMES (NIGHTJARS, FROGMOUTHS & POTOOS)
        public class Caprimulgiformes : Bird
        {
            public Caprimulgiformes(Bird? father = null, Bird? mother = null, string? species = "Frogmouth", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Podargidae : Caprimulgiformes
            {
                public Podargidae(Bird? father = null, Bird? mother = null, string? species = "Frogmouth", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class TawnyFrogmouth : Podargidae
                {
                    public TawnyFrogmouth(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Tawny Frogmouth (Podargus strigoides)", sex)
                    {
                        MaxLifespanYears = 14f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string MimicDeadEucalyptBranch() => $"{SpeciesName} stiffens posture diagonally on tree fork, closing yellow eyes to become virtually indistinguishable from a broken tree branch!";
                }
            }

            public class Nyctibiidae : Caprimulgiformes
            {
                public Nyctibiidae(Bird? father = null, Bird? mother = null, string? species = "Potoo", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class CommonPotoo : Nyctibiidae
                {
                    public CommonPotoo(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Common Potoo (Nyctibius griseus)", sex)
                    {
                        MaxLifespanYears = 12f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string PeerThroughEyelidSlits() => $"{SpeciesName} perches motionless atop fencepost, monitoring approaching insects through micro-slits in closed eyelids without breaking camouflage!";
                }
            }
        }

        // ORDER 17: CORACIIFORMES (KINGFISHERS, BEE-EATERS & ROLLER BIRDS)
        public class Coraciiformes : Bird
        {
            public Coraciiformes(Bird? father = null, Bird? mother = null, string? species = "Kingfisher", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Alcedinidae : Coraciiformes
            {
                public Alcedinidae(Bird? father = null, Bird? mother = null, string? species = "Kingfisher", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class CommonKingfisher : Alcedinidae
                {
                    public CommonKingfisher(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Common Kingfisher (Alcedo atthis)", sex)
                    {
                        MaxLifespanYears = 7f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string AerodynamicPlungeDive(Fish fish)
                    {
                        if (!fish.IsAlive) return "...";
                        fish.TakeDamage(35f, DamageType.Piercing, "Hydrodynamic dagger dive plunge");
                        return $"{SpeciesName} dives at 40 km/h with zero splash shockwave thanks to streamline beak, plunging underwater to seize {fish.SpeciesName}!";
                    }
                }

                public class LaughingKookaburra : Alcedinidae
                {
                    public LaughingKookaburra(Bird? father = null, Bird? mother = null, SexType? sex = null)
                        : base(father, mother, "Laughing Kookaburra (Dacelo novaeguineae)", sex)
                    {
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string TerrifyingLaughterChorus() => $"{SpeciesName}: *KOO-KOO-KOO-KAA-KAA-KAA-HAAA!* (Unleashes deafening communal laughter echoing across eucalyptus woodlands!)";
                }
            }
        }
    }

    public class Fish : Creature
    {
        public string? SpeciesName { get; set; } = "Salmon";

        public Fish(Fish? father = null, Fish? mother = null, string? species = "Salmon", SexType? sex = null)
            : base(father, mother, hands: 0, handFingers: 0, legs: 0, legFingers: 0, teeths: 16, sex: sex)
        {
            SpeciesName = species;
            MaxLifespanYears = 7f + _rng.Next(-2, 4);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Gills = [new Gill()];
            Body.Tail = new Tail { LengthCm = 15f };
            IsEndothermic = false;
            BodyTemperatureCelsius = 15.0f;
        }

        public void Swim(float speedKnots)
        {
            if (!IsAlive) return;
            Energy = Math.Max(0f, Energy - speedKnots * 0.5f);
        }

        // --- FISH ORDERS NESTED IN FISH ---

        // 1. Chondrichthyes (Cartilaginous Fish)
        public abstract class Chondrichthyes : Fish
        {
            public bool HasCartilaginousSkeleton { get; } = true;

            public Chondrichthyes(Fish? father = null, Fish? mother = null, string? species = "Cartilaginous Fish", SexType? sex = null)
                : base(father, mother, species, sex)
            {
                Body.Skeleton.BoneDensity = 0.3f; // Lightweight flexible cartilage
            }

            public class Lamnidae : Chondrichthyes
            {
                public Lamnidae(Fish? father = null, Fish? mother = null, string? species = "Mackerel Shark", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class GreatWhiteShark : Lamnidae
                {
                    public GreatWhiteShark(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Great White Shark (Carcharodon carcharias)", sex)
                    {
                        MaxLifespanYears = 70f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SenseElectricField(Creature target) => $"{SpeciesName} detects microvolt cardiac bioelectric pulses of {target.GetType().Name} via Ampullae of Lorenzini!";

                    public string ApexBite(Creature target)
                    {
                        if (!IsAlive || !target.IsAlive) return "...";
                        target.TakeDamage(80f, DamageType.Slashing, "Great white shark bite");
                        return $"{SpeciesName} bites with 18,000 N crushing force, rotating serrated triangular conveyor teeth to sever flesh!";
                    }
                }

                public class ShortfinMakoShark : Lamnidae
                {
                    public float TopSpeedKmh { get; } = 74f;

                    public ShortfinMakoShark(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Shortfin Mako Shark (Isurus oxyrinchus)", sex)
                    {
                        MaxLifespanYears = 32f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string BurstSprint() => $"{SpeciesName} flexes crescent caudal fin, bursting to {TopSpeedKmh} km/h with bristled flexible dermal denticles!";
                }
            }

            public class Sphyrnidae : Chondrichthyes
            {
                public Sphyrnidae(Fish? father = null, Fish? mother = null, string? species = "Hammerhead Shark", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class GreatHammerheadShark : Sphyrnidae
                {
                    public GreatHammerheadShark(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Great Hammerhead Shark (Sphyrna mokarran)", sex)
                    {
                        MaxLifespanYears = 44f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ScanBenthosWithCephalofoil() => $"{SpeciesName} sweeps wide T-shaped cephalofoil like a metal detector, homing in on stingrays buried beneath sand!";
                }
            }

            public class Carcharhinidae : Chondrichthyes
            {
                public Carcharhinidae(Fish? father = null, Fish? mother = null, string? species = "Requiem Shark", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class TigerShark : Carcharhinidae
                {
                    public TigerShark(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Tiger Shark (Galeocerdo cuvier)", sex)
                    {
                        MaxLifespanYears = 50f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string CrushTurtleShell(Reptile.Testudines turtle)
                    {
                        if (!turtle.IsAlive) return "...";
                        turtle.TakeDamage(75f, DamageType.Blunt, "Tiger shark cockscomb bite");
                        return $"{SpeciesName} saws through rock-hard turtle carapace with specialized sideways-pointing serrated cockscomb teeth!";
                    }
                }

                public class BullShark : Carcharhinidae
                {
                    public bool InFreshwater { get; private set; } = false;

                    public BullShark(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Bull Shark (Carcharhinus leucas)", sex)
                    {
                        MaxLifespanYears = 30f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string OsmoregulateFreshwater()
                    {
                        InFreshwater = true;
                        return $"{SpeciesName} adjusts renal rectal gland, retaining sodium and urea to swim 1,000 km up freshwater rivers!";
                    }
                }
            }

            public class Rhincodontidae : Chondrichthyes
            {
                public Rhincodontidae(Fish? father = null, Fish? mother = null, string? species = "Whale Shark", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class WhaleShark : Rhincodontidae
                {
                    public float WeightTonnes { get; } = 20.0f;

                    public WhaleShark(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Whale Shark (Rhincodon typus)", sex)
                    {
                        MaxLifespanYears = 130f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string FilterFeedPlankton(float kgPlankton = 25f) => $"{SpeciesName} opens 1.5m wide mouth, filtering {kgPlankton:F0} kg of macro-algae and krill through spongy gill rakers!";
                }
            }

            public class Somniosidae : Chondrichthyes
            {
                public Somniosidae(Fish? father = null, Fish? mother = null, string? species = "Sleeper Shark", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class GreenlandShark : Somniosidae
                {
                    public GreenlandShark(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Greenland Shark (Somniosus microcephalus)", sex)
                    {
                        MaxLifespanYears = 400f; // Longest-lived vertebrate on Earth
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string CruiseGlacialDepths() => $"{SpeciesName} cruises at 1.2 km/h in -1.5°C Arctic abyssal waters, preserved by toxic trimethylamine oxide antifreeze tissue!";
                }
            }

            public class Mitsukurinidae : Chondrichthyes
            {
                public Mitsukurinidae(Fish? father = null, Fish? mother = null, string? species = "Goblin Shark", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class GoblinShark : Mitsukurinidae
                {
                    public GoblinShark(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Goblin Shark (Mitsukurina owstoni)", sex)
                    {
                        MaxLifespanYears = 35f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SlingshotBite(Creature prey)
                    {
                        if (!IsAlive || !prey.IsAlive) return "...";
                        prey.TakeDamage(40f, DamageType.Piercing, "Goblin shark slingshot jaw");
                        return $"{SpeciesName} thrusts jaws forward at 3.1 m/s on double-hinged mandibular ligaments, capturing {prey.GetType().Name}!";
                    }
                }
            }

            public class Myliobatidae : Chondrichthyes
            {
                public Myliobatidae(Fish? father = null, Fish? mother = null, string? species = "Stingray", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class Stingray : Myliobatidae
                {
                    public Stingray(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Southern Stingray (Dasyatis americana)", sex)
                    {
                        MaxLifespanYears = 25f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string WhipBarbedStinger(Creature target)
                    {
                        if (!IsAlive || !target.IsAlive) return "...";
                        target.TakeDamage(30f, DamageType.Piercing, "Venomous caudal spine");
                        return $"{SpeciesName} whips serrated tail barb into {target.GetType().Name}, discharging proteinaceous cardiotoxin!";
                    }
                }
            }

            public class Mobulidae : Chondrichthyes
            {
                public Mobulidae(Fish? father = null, Fish? mother = null, string? species = "Manta Ray", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class GiantMantaRay : Mobulidae
                {
                    public float DiscSpanMeters { get; } = 7.0f;

                    public GiantMantaRay(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Giant Oceanic Manta Ray (Mobula birostris)", sex)
                    {
                        MaxLifespanYears = 50f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string BarrelRollFilter() => $"{SpeciesName} unfurls 7m pectoral wings and performs acrobatic underwater barrel rolls to funnel zooplankton into its cephalic lobes!";
                }
            }

            public class Torpedinidae : Chondrichthyes
            {
                public Torpedinidae(Fish? father = null, Fish? mother = null, string? species = "Electric Ray", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class PacificElectricRay : Torpedinidae
                {
                    public PacificElectricRay(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Pacific Electric Ray (Tetronarce californica)", sex)
                    {
                        MaxLifespanYears = 24f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DischargeShock(Creature target)
                    {
                        if (!IsAlive || !target.IsAlive) return "...";
                        target.TakeDamage(45f, DamageType.Poison, "Electric ray bio-voltage");
                        return $"{SpeciesName} discharges 220 Volts from kidney-shaped branchial electric organs, paralyzing {target.GetType().Name}!";
                    }
                }
            }
        }

        // 2. Osteichthyes (Bony Fish)
        public abstract class Osteichthyes : Fish
        {
            public bool HasSwimBladder { get; } = true;

            public Osteichthyes(Fish? father = null, Fish? mother = null, string? species = "Bony Fish", SexType? sex = null)
                : base(father, mother, species, sex) { }

            // --- DEEP SEA ABYSSAL BONY FISH ---

            public class Ceratiidae : Osteichthyes
            {
                public Ceratiidae(Fish? father = null, Fish? mother = null, string? species = "Anglerfish", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class HumpbackAnglerfish : Ceratiidae
                {
                    public bool HasLuminescentLure { get; } = true;

                    public HumpbackAnglerfish(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Humpback Anglerfish (Melanocetus johnsonii)", sex)
                    {
                        MaxLifespanYears = 25f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string GlowBioluminescentLure(Creature prey)
                    {
                        if (!IsAlive || !prey.IsAlive) return "...";
                        prey.TakeDamage(35f, DamageType.Slashing, "Anglerfish needle jaw ambush");
                        return $"{SpeciesName} dangles glowing symbiotic photophore esca in midnight bathypelagic abyss, snapping needle fangs around {prey.GetType().Name}!";
                    }
                }
            }

            public class Saccopharyngidae : Osteichthyes
            {
                public Saccopharyngidae(Fish? father = null, Fish? mother = null, string? species = "Gulper Eel", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class PelicanGulperEel : Saccopharyngidae
                {
                    public PelicanGulperEel(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Pelican Gulper Eel (Eurypharynx pelecanoides)", sex)
                    {
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ExpandPelicanJaws(Creature prey) => $"{SpeciesName} unhinges cavernous pouch pharynx, engulfing {prey.GetType().Name} whole at 2,000m depth!";
                }
            }

            public class Opisthoproctidae : Osteichthyes
            {
                public Opisthoproctidae(Fish? father = null, Fish? mother = null, string? species = "Barreleye", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class BarreleyeFish : Opisthoproctidae
                {
                    public BarreleyeFish(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Barreleye Fish (Macropinna microstoma)", sex)
                    {
                        MaxLifespanYears = 15f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string RotateEyesUpward() => $"{SpeciesName} swivels emerald green tubular lens eyes upward through a transparent fluid-filled head shield to detect silhouettes!";
                }
            }

            public class Regalecidae : Osteichthyes
            {
                public Regalecidae(Fish? father = null, Fish? mother = null, string? species = "Oarfish", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class GiantOarfish : Regalecidae
                {
                    public float LengthMeters { get; } = 8.0f;

                    public GiantOarfish(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Giant Oarfish (Regalecus glesne)", sex)
                    {
                        MaxLifespanYears = 30f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string UndulateVertical() => $"{SpeciesName} hovers vertically in water column with crimson dorsal crest, undulating its {LengthMeters}m silver ribbon body!";
                }
            }

            // --- FRESHWATER APEX RIVER TITANS ---

            public class Gymnotidae : Osteichthyes
            {
                public Gymnotidae(Fish? father = null, Fish? mother = null, string? species = "Electric Eel", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class ElectricEel : Gymnotidae
                {
                    public float VoltagePotential { get; } = 860f;

                    public ElectricEel(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Electric Eel (Electrophorus electricus)", sex)
                    {
                        MaxLifespanYears = 15f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DischargeElectricShock(Creature target)
                    {
                        if (!IsAlive || !target.IsAlive) return "...";
                        target.TakeDamage(65f, DamageType.Poison, "Electrogenic shock");
                        return $"{SpeciesName} discharges 860 Volts / 1 Amp from 6,000 electrocytes, stunning {target.GetType().Name} via water conductivity!";
                    }
                }
            }

            public class Serrasalmidae : Osteichthyes
            {
                public Serrasalmidae(Fish? father = null, Fish? mother = null, string? species = "Red-Bellied Piranha", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class RedBelliedPiranha : Serrasalmidae
                {
                    public RedBelliedPiranha(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Red-Bellied Piranha (Pygocentrus nattereri)", sex)
                    {
                        MaxLifespanYears = 10f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string FrenzyBite(Creature target)
                    {
                        if (!IsAlive || !target.IsAlive) return "...";
                        target.TakeDamage(25f, DamageType.Slashing, "Piranha feeding frenzy");
                        return $"{SpeciesName} shears flesh with razor-sharp interlocking blade teeth in rapid feeding frenzy!";
                    }
                }
            }

            public class Arapaimidae : Osteichthyes
            {
                public Arapaimidae(Fish? father = null, Fish? mother = null, string? species = "Arapaima", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class ArapaimaGigantea : Arapaimidae
                {
                    public ArapaimaGigantea(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Arapaima (Arapaima gigas)", sex)
                    {
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SurfaceGaspAir() => $"{SpeciesName} breaches oxygen-depleted Amazonian floodwaters, gulping atmospheric air into modified swim bladder lung!";
                }
            }

            public class Latimeriidae : Osteichthyes
            {
                public Latimeriidae(Fish? father = null, Fish? mother = null, string? species = "Coelacanth", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class WestIndianOceanCoelacanth : Latimeriidae
                {
                    public WestIndianOceanCoelacanth(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Coelacanth Living Fossil (Latimeria chalumnae)", sex)
                    {
                        MaxLifespanYears = 100f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string PaddleLobeFins() => $"{SpeciesName} flexes bone-supported lobed fins in alternating tetrapod trotting gait along volcanic caves!";
                }
            }

            // --- PELAGIC OPEN OCEAN SPEEDSTERS ---

            public class Scombridae : Osteichthyes
            {
                public Scombridae(Fish? father = null, Fish? mother = null, string? species = "Tuna/Mackerel", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class AtlanticBluefinTuna : Scombridae
                {
                    public float WeightKg { get; } = 450f;

                    public AtlanticBluefinTuna(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Atlantic Bluefin Tuna (Thunnus thynnus)", sex)
                    {
                        IsEndothermic = true; // Regional endothermy
                        BodyTemperatureCelsius = 25f;
                        MaxLifespanYears = 35f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SwimHighSpeed(float speedKmh = 70f) => $"{SpeciesName} powers through pelagic waters at {speedKmh} km/h, warming red muscle fibers via vascular countercurrent heat exchange!";
                }
            }

            public class Istiophoridae : Osteichthyes
            {
                public Istiophoridae(Fish? father = null, Fish? mother = null, string? species = "Billfish", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class IndoPacificSailfish : Istiophoridae
                {
                    public float MaxSpeedKmh { get; } = 110f;

                    public IndoPacificSailfish(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Indo-Pacific Sailfish (Istiophorus platypterus)", sex)
                    {
                        MaxLifespanYears = 15f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string UnfurlSailAndSlash(Fish school)
                    {
                        if (!school.IsAlive) return "...";
                        school.TakeDamage(30f, DamageType.Slashing, "Sailfish bill slash");
                        return $"{SpeciesName} raises cobalt sail fin and charges at {MaxSpeedKmh} km/h, slashing through schools of fish with pointed bill!";
                    }
                }
            }

            public class Molidae : Osteichthyes
            {
                public Molidae(Fish? father = null, Fish? mother = null, string? species = "Ocean Sunfish", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class OceanSunfish : Molidae
                {
                    public OceanSunfish(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Ocean Sunfish (Mola mola)", sex)
                    {
                        MaxLifespanYears = 23f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string BaskOnSurface() => $"{SpeciesName} lies flat on ocean surface, warming 2-tonne body in the sun and allowing seabirds to pick parasites!";
                }
            }

            // --- CORAL REEF & MARINE SYMBIOSIS ---

            public class Pomacentridae : Osteichthyes
            {
                public Pomacentridae(Fish? father = null, Fish? mother = null, string? species = "Damselfish", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class Clownfish : Pomacentridae
                {
                    public Clownfish(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Ocellaris Clownfish (Amphiprion ocellaris)", sex)
                    {
                        MaxLifespanYears = 12f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SeekAnemoneShelter() => $"{SpeciesName} secretes mucus devoid of N-acetylneuraminic acid, swimming immune through stinging sea anemone tentacles!";
                }
            }

            public class Scorpaenidae : Osteichthyes
            {
                public Scorpaenidae(Fish? father = null, Fish? mother = null, string? species = "Scorpionfish", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class RedLionfish : Scorpaenidae
                {
                    public RedLionfish(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Red Lionfish (Pterois volitans)", sex)
                    {
                        MaxLifespanYears = 16f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string FlareVenomousSpines(Creature attacker)
                    {
                        attacker.TakeDamage(40f, DamageType.Poison, "Lionfish neuromuscular toxin");
                        return $"{SpeciesName} fans 18 needle-sharp dorsal spines, delivering debilitating acetylcholine toxin to {attacker.GetType().Name}!";
                    }
                }

                public class ReefStonefish : Scorpaenidae
                {
                    public ReefStonefish(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Reef Stonefish (Synanceia verrucosa)", sex)
                    {
                        MaxLifespanYears = 15f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string EnvenomateFoot(Creature victim)
                    {
                        victim.TakeDamage(75f, DamageType.Poison, "Stonefish verrucotoxin");
                        return $"{SpeciesName} delivers verrucotoxin neurotoxin from 13 hypodermic spines: the most excruciating and lethal marine sting in the ocean!";
                    }
                }
            }

            public class Syngnathidae : Osteichthyes
            {
                public Syngnathidae(Fish? father = null, Fish? mother = null, string? species = "Seahorse", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class LinedSeahorse : Syngnathidae
                {
                    public bool HasBroodPouch { get; } = true;

                    public LinedSeahorse(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Lined Seahorse (Hippocampus erectus)", sex)
                    {
                        MaxLifespanYears = 5f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string MaleGestationBirth() => $"{SpeciesName} (Male) contracts abdominal brood pouch muscles, birthing 300 miniature seahorse fry!";
                }
            }

            public class Tetraodontidae : Osteichthyes
            {
                public Tetraodontidae(Fish? father = null, Fish? mother = null, string? species = "Pufferfish", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class PorcupinePufferfish : Tetraodontidae
                {
                    public bool IsInflated { get; private set; } = false;

                    public PorcupinePufferfish(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Porcupine Pufferfish (Diodon hystrix)", sex)
                    {
                        MaxLifespanYears = 15f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string InflateSpines()
                    {
                        IsInflated = true;
                        return $"{SpeciesName} pumps water into expandable stomach, swelling into an impenetrable spiny ball containing lethal tetrodotoxin (TTX)!";
                    }
                }
            }

            public class Muraenidae : Osteichthyes
            {
                public Muraenidae(Fish? father = null, Fish? mother = null, string? species = "Moray Eel", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class GiantMorayEel : Muraenidae
                {
                    public GiantMorayEel(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Giant Moray Eel (Gymnothorax javanicus)", sex)
                    {
                        MaxLifespanYears = 30f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string PharyngealJawStrike(Creature prey)
                    {
                        if (!IsAlive || !prey.IsAlive) return "...";
                        prey.TakeDamage(50f, DamageType.Slashing, "Moray pharyngeal second jaw");
                        return $"{SpeciesName} grips prey with outer teeth while second pharyngeal jaws shoot forward from throat to drag {prey.GetType().Name} down esophagus!";
                    }
                }
            }
        }

        // =====================================================================
        // --- SUBCLASS 3: AGNATHA (JAWLESS FISHES - HAGFISHES & LAMPREYS)
        // =====================================================================
        public abstract class Agnatha : Fish
        {
            public bool LacksJaws { get; } = true;
            public bool HasCartilaginousNotochord { get; } = true;

            public Agnatha(Fish? father = null, Fish? mother = null, string? species = "Jawless Fish", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Myxinidae : Agnatha // Hagfishes
            {
                public Myxinidae(Fish? father = null, Fish? mother = null, string? species = "Hagfish", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class AtlanticHagfish : Myxinidae
                {
                    public AtlanticHagfish(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Atlantic Slime Hagfish (Myxine glutinosa)", sex)
                    {
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DeploySuffocatingSlimeDefense(Creature predator)
                    {
                        if (!predator.IsAlive) return "...";
                        return $"{SpeciesName} discharges 100 slime glands; intermediate filaments interact with seawater, instantly expanding into 20 liters of viscous mucus that suffocates {predator.GetType().Name}'s gills!";
                    }

                    public string TieBodyInOverhandKnot() => $"{SpeciesName} ties its flexible scaleless body into a sliding overhand knot to scrape slime off and gain leverage tearing flesh from whale falls!";
                }
            }

            public class Petromyzontidae : Agnatha // Lampreys
            {
                public Petromyzontidae(Fish? father = null, Fish? mother = null, string? species = "Lamprey", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class SeaLamprey : Petromyzontidae
                {
                    public SeaLamprey(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Sea Lamprey (Petromyzon marinus)", sex)
                    {
                        MaxLifespanYears = 11f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ParasiticOralDiscAttachment(Fish hostFish)
                    {
                        if (!hostFish.IsAlive) return "...";
                        hostFish.TakeDamage(25f, DamageType.Piercing, "Keratin tooth suction rasp");
                        return $"{SpeciesName} clamps circular suction oral disc lined with concentric rows of keratin teeth, rasping host tissue with piston tongue and secreting anticoagulant lamphredin!";
                    }
                }
            }
        }

        // =====================================================================
        // --- SUBCLASS 4: PLACODERMI (ARMORED PREHISTORIC FISHES)
        // =====================================================================
        public abstract class Placodermi : Fish
        {
            public bool HasBonyHeadAndThoraxShield { get; } = true;

            public Placodermi(Fish? father = null, Fish? mother = null, string? species = "Placoderm", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Dunkleosteidae : Placodermi
            {
                public Dunkleosteidae(Fish? father = null, Fish? mother = null, string? species = "Dunkleosteid", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class DunkleosteusTerrelli : Dunkleosteidae
                {
                    public float TotalLengthMeters { get; } = 9.0f;
                    public float BiteForceNewtons { get; } = 5300f;

                    public DunkleosteusTerrelli(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Dunkleosteus terrelli", sex)
                    {
                        MaxLifespanYears = 45f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ShearingGnathalBite(Creature armoredPrey)
                    {
                        if (!IsAlive || !armoredPrey.IsAlive) return "...";
                        armoredPrey.TakeDamage(100f, DamageType.Slashing, "Dunkleosteus self-sharpening gnathal plate bite");
                        return $"{SpeciesName} opens jointed arthrodire head shield in 20 milliseconds, creating massive vacuum before snapping self-sharpening gnathal bone blades with 5,300 N shear force into {armoredPrey.GetType().Name}!";
                    }
                }
            }
        }

        // =====================================================================
        // --- SUBCLASS 5: SARCOPTERYGII (LOBE-FINNED FISHES & TRANSITIONAL TETRAPODOMORPHS)
        // =====================================================================
        public abstract class Sarcopterygii : Fish
        {
            public bool HasLobedMonobasalFins { get; } = true;

            public Sarcopterygii(Fish? father = null, Fish? mother = null, string? species = "Lobe-finned Fish", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class Elpistostegalia : Sarcopterygii // Stem Tetrapods / Fishapods
            {
                public Elpistostegalia(Fish? father = null, Fish? mother = null, string? species = "Fishapod", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class TiktaalikRoseae : Elpistostegalia
                {
                    public TiktaalikRoseae(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Tiktaalik roseae (The Transitional Fishapod)", sex)
                    {
                        MaxLifespanYears = 25f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string PropUpOnWristJoints() => $"{SpeciesName} flexes homologous wrist joints and pectoral fins, propping head and shoulders above Late Devonian shallow swamp waters!";
                    public string RotateMobileNeck() => $"{SpeciesName} turns flat crocodile-like triangular skull independently of pectoral girdle, a revolutionary evolutionary step toward land vertebrates!";
                    public string BreatheViaSpiracles() => $"{SpeciesName} draws atmospheric oxygen through dorsal head spiracles in hypoxic Devonian oxbow ponds!";
                }

                public class EusthenopteronFoordi : Elpistostegalia
                {
                    public EusthenopteronFoordi(Fish? father = null, Fish? mother = null, SexType? sex = null)
                        : base(father, mother, "Eusthenopteron foordi", sex)
                    {
                        MaxLifespanYears = 22f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string InternalChoanaeRespiration() => $"{SpeciesName} circulates air through internal nostrils (choanae) while swimming with internal humerus, radius, and ulna fin bones!";
                }
            }
        }
    }

    public class Horse : Creature
    {
        public string? Breed { get; set; } = "Mustang";
        public override float SpeciesGestationDays => 340f;
        public override float TeethingAgeYears => 0.02f; // ~1 week for foal teeth
        public override float AdultTeethAgeYears => 2.5f; // ~2.5 years for adult teeth

        public Horse(Horse? father = null, Horse? mother = null, string? breed = "Mustang", SexType? sex = null)
            : base(father, mother, hands: 2, handFingers: 1, legs: 2, legFingers: 1, teeths: 40, sex: sex)
        {
            Breed = breed;
            MaxLifespanYears = 28f + _rng.Next(-3, 6);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 80f };

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(4, MammaryGland.CupSize.B, 1200f);
            }
        }

        public void Gallop(float distanceKm)
        {
            if (!IsAlive) return;
            Energy = Math.Max(0f, Energy - distanceKm * 2f);
            Hunger = Math.Clamp(Hunger + distanceKm * 1.5f, 0f, 100f);
            Thirst = Math.Clamp(Thirst + distanceKm * 3.0f, 0f, 100f);
        }

        public void Graze(Plant.Grass grass)
        {
            if (!IsAlive) return;
            Hunger = Math.Max(0f, Hunger - grass.GrazingNutritionPerSquareMeter * 0.2f);
            Energy = Math.Min(100f, Energy + 5f);
        }

        public bool Mate(Horse partner, string? foalName = null) => Reproduce(partner, foalName);
        public new Horse? GiveBirth() => base.GiveBirth() as Horse;
    }

    // =========================================================================
    // --- 5. REPTILIA: SQUAMATA (SERPENTES & LIZARDS), CROCODILIA & TESTUDINES
    // =========================================================================

    public class Reptile : Creature
    {
        public string? Name { get; set; }
        public bool IsBaskingInSun { get; protected set; } = false;
        public float BaskingEfficiency { get; protected set; } = 1.0f;
        public float ScaleHardnessMohs { get; protected set; } = 3.5f;
        public bool SheddingSkin { get; protected set; } = false;
        public override float SpeciesGestationDays => 60f; // Egg incubation

        public Reptile(Creature? father = null, Creature? mother = null, int hands = 2, int handFingers = 5, int legs = 2, int legFingers = 5, int teeths = 20, SexType? sex = null, string? name = null)
            : base(father, mother, hands, handFingers, legs, legFingers, teeths, sex)
        {
            Name = name;
            IsEndothermic = false; // Ectothermic / Poikilothermic (Cold-blooded)
            BodyTemperatureCelsius = 22.0f; // Ambient dependent
            MaxLifespanYears = 25f + _rng.Next(-5, 10);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
        }

        public virtual void BaskInSun(float hours, float ambientTempCelsius = 30f)
        {
            if (!IsAlive) return;
            IsBaskingInSun = true;
            BodyTemperatureCelsius = Math.Min(ambientTempCelsius, BodyTemperatureCelsius + hours * 2.5f * BaskingEfficiency);
            Energy = Math.Min(100f, Energy + hours * 10f);
        }

        public virtual void ShedSkin()
        {
            if (!IsAlive) return;
            SheddingSkin = true;
            Health = Math.Min(100f, Health + 5f);
            SheddingSkin = false;
        }

        // --- SUB-ORDER 1: SQUAMATA (SCALED REPTILES - NESTED IN REPTILE) ---
        public abstract class Squamata : Reptile
        {
            public bool HasJacobsonOrgan { get; protected set; } = true; // Scent tasting via tongue

            public Squamata(Creature? father = null, Creature? mother = null, int hands = 2, int handFingers = 5, int legs = 2, int legFingers = 5, int teeths = 20, SexType? sex = null, string? name = null)
                : base(father, mother, hands, handFingers, legs, legFingers, teeths, sex, name)
            {
            }

            public virtual string FlickTongue()
            {
                if (!IsAlive) return "...";
                return $"{Name ?? "Squamate"} flicks forked tongue into Jacobson's organ, tasting chemical airborne scents!";
            }

            // --- INFRA-ORDER 1: SERPENTES (SNAKES - NESTED IN SQUAMATA) ---
            public abstract class Serpentes : Squamata
            {
                public bool IsVenomous { get; protected set; }
                public float VenomYieldMg { get; protected set; } = 0f;
                public string VenomType { get; protected set; } = "None"; // Hemotoxin, Neurotoxin, Cytotoxin
                public float ConstrictionPressurePsi { get; protected set; } = 0f;
                public bool HasHeatPits { get; protected set; } = true; // Infrared pit organs

                public Serpentes(Creature? father = null, Creature? mother = null, int teeths = 30, SexType? sex = null, string? name = null)
                    : base(father, mother, hands: 0, handFingers: 0, legs: 0, legFingers: 0, teeths: teeths, sex: sex, name: name)
                {
                    Body.Tail = new Tail { LengthCm = 120f };
                }

                public virtual string Slither(float distanceMeters)
                {
                    if (!IsAlive) return "...";
                    Energy = Math.Max(0f, Energy - distanceMeters * 0.1f);
                    return $"{Name ?? "Serpent"} slithers undulating silently across the ground for {distanceMeters}m.";
                }

                // --- FAMILIES NESTED IN SERPENTES ---

                // Family 1: Viperidae (Vipers & Rattlesnakes - Hemotoxic Venom)
                public class Viperidae : Serpentes
                {
                    public bool HasRattle { get; protected set; }
                    public bool IsRattling { get; private set; }

                    public Viperidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, teeths: 24, sex: sex, name: name ?? "Viper")
                    {
                        IsVenomous = true;
                        VenomType = "Hemotoxin (Destroys red blood cells, causes hemorrhaging & tissue necrosis)";
                        VenomYieldMg = 120f;
                        HasRattle = true;
                        MaxLifespanYears = 18f + _rng.Next(-3, 5);
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string RattleTail()
                    {
                        if (!IsAlive || !HasRattle) return "...";
                        IsRattling = true;
                        return $"{Name}: *Ch-ch-ch-ch-ch-ch!* (Vibrates hollow keratin rattle at 50Hz in defensive warning!)";
                    }

                    public bool StrikeAndEnvenomate(Creature target)
                    {
                        if (!IsAlive || !target.IsAlive || VenomYieldMg <= 0) return false;
                        float damage = 35f;
                        target.TakeDamage(damage, DamageType.Poison);
                        target.Blood.OxygenSaturationSpO2 = Math.Max(0f, target.Blood.OxygenSaturationSpO2 - 0.25f);
                        VenomYieldMg = Math.Max(0f, VenomYieldMg - 50f);
                        return true;
                    }

                    public class Rattlesnake : Viperidae
                    {
                        public Rattlesnake(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Diamondback Rattlesnake", sex) { }
                    }

                    public class GaboonViper : Viperidae
                    {
                        public float FangLengthCm { get; } = 5.0f; // Longest fangs of any snake

                        public GaboonViper(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Gaboon Viper (Bitis gabonica)", sex)
                        {
                            VenomYieldMg = 2000f; // Immense venom volume
                            MaxLifespanYears = 20f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string MassiveEnvenomation(Creature target)
                        {
                            if (!IsAlive || !target.IsAlive) return "...";
                            target.TakeDamage(65f, DamageType.Poison, "Gaboon viper cytotoxic hemotoxin");
                            return $"{Name} unhinges jaws to sink {FangLengthCm}cm hypodermic fangs, pumping 2,000mg of cytotoxic venom into {target.GetType().Name}!";
                        }
                    }

                    public class SawScaledViper : Viperidae
                    {
                        public SawScaledViper(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Saw-Scaled Viper (Echis carinatus)", sex)
                        {
                            MaxLifespanYears = 14f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string StridulateWarning() => $"{Name} rubs serrated oblique lateral scales together in figure-8 loops, producing loud sizzling rasps!";
                    }
                }

                // Family 2: Elapidae (Cobras, Mambas, Taipans & Sea Snakes - Neurotoxic Venom)
                public class Elapidae : Serpentes
                {
                    public bool HasHood { get; protected set; } = true;
                    public bool IsHoodExpanded { get; private set; } = false;

                    public Elapidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, teeths: 28, sex: sex, name: name ?? "Elapid")
                    {
                        IsVenomous = true;
                        VenomType = "Neurotoxin (Blocks acetylcholine receptors, shuts down respiratory diaphragm)";
                        VenomYieldMg = 400f;
                        MaxLifespanYears = 20f + _rng.Next(-2, 6);
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string FlareHood()
                    {
                        if (!IsAlive) return "...";
                        IsHoodExpanded = true;
                        return $"{Name}: *HIIIIISSSSSS!* (Rears up 1.5m and expands threatening cervical hood!)";
                    }

                    public bool InjectNeurotoxin(Creature victim)
                    {
                        if (!IsAlive || !victim.IsAlive) return false;
                        victim.TakeDamage(60f, DamageType.Poison, "Elapid neurotoxin");
                        victim.Body.Heart.BeatsPerMinute = Math.Max(10f, victim.Body.Heart.BeatsPerMinute * 0.4f);
                        return true;
                    }

                    public class KingCobra : Elapidae
                    {
                        public KingCobra(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "King Cobra (Ophiophagus hannah)", sex)
                        {
                            Body.Tail = new Tail { LengthCm = 400f };
                        }

                        public string HuntOtherSnake(Serpentes preySnake)
                        {
                            if (!preySnake.IsAlive) return "...";
                            preySnake.TakeDamage(75f, DamageType.Poison, "King cobra ophiophagy strike");
                            return $"{Name} demonstrates specialized ophiophagy, seizing {preySnake.Name} behind the neck and swallowing whole!";
                        }
                    }

                    public class BlackMamba : Elapidae
                    {
                        public float SprintSpeedKmh { get; } = 20f;

                        public BlackMamba(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Black Mamba (Dendroaspis polylepis)", sex)
                        {
                            VenomYieldMg = 280f;
                        }

                        public string GapeBlackMouth() => $"{Name} gapes inky black buccal lining in warning and slithers at {SprintSpeedKmh} km/h!";
                    }

                    public class InlandTaipan : Elapidae
                    {
                        public float ToxicityLd50MgKg { get; } = 0.025f; // Most toxic snake venom on Earth

                        public InlandTaipan(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Inland Taipan (Oxyuranus microlepidotus)", sex)
                        {
                            VenomType = "Taipoxin (Most potent biological neurotoxin known: LD50 0.025 mg/kg)";
                            MaxLifespanYears = 15f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string StrikeMostToxicVenom(Creature target)
                        {
                            if (!IsAlive || !target.IsAlive) return "...";
                            target.TakeDamage(95f, DamageType.Poison, "Inland taipan paradoxin strike");
                            return $"{Name} snaps multiple rapid bites, delivering paradoxin & oxylepitoxin capable of killing 100 adult humans!";
                        }
                    }

                    public class BelcherSeaSnake : Elapidae
                    {
                        public BelcherSeaSnake(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Belcher's Sea Snake (Hydrophis belcheri)", sex)
                        {
                            MaxLifespanYears = 12f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string PaddleSwimReef() => $"{Name} propels with laterally compressed oar-like paddle tail, diving through coral reefs for eels!";
                    }
                }

                // Family 3: Boidae & Pythonidae (Constrictors)
                public class Boidae : Serpentes
                {
                    public float GirthCm { get; protected set; } = 30f;

                    public Boidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, teeths: 60, sex: sex, name: name ?? "Boa")
                    {
                        IsVenomous = false;
                        ConstrictionPressurePsi = 12.0f; // Suffocates prey circulation
                        MaxLifespanYears = 30f + _rng.Next(-5, 10);
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string Constrict(Creature prey)
                    {
                        if (!IsAlive || !prey.IsAlive) return "...";
                        float suffocationDamage = ConstrictionPressurePsi * 3.5f;
                        prey.TakeDamage(suffocationDamage, DamageType.Asphyxiation);
                        prey.Blood.OxygenSaturationSpO2 = 0f;
                        return $"{Name} wraps muscular coils around {prey.GetType().Name}, applying {ConstrictionPressurePsi:F1} PSI of arterial constriction!";
                    }

                    public class GreenAnaconda : Boidae
                    {
                        public GreenAnaconda(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Giant Green Anaconda (Eunectes murinus)", sex)
                        {
                            Body.Tail = new Tail { LengthCm = 650f };
                            ConstrictionPressurePsi = 16.5f;
                        }
                    }

                    public class EmeraldTreeBoa : Boidae
                    {
                        public EmeraldTreeBoa(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Emerald Tree Boa (Corallus caninus)", sex)
                        {
                            ConstrictionPressurePsi = 10.0f;
                        }

                        public string DangleArborealCoils() => $"{Name} dangles in saddle coils from rainforest branches, flashing bright emerald-green scales with white lightning zigzags!";
                    }

                    public class ReticulatedPython : Boidae
                    {
                        public ReticulatedPython(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Reticulated Python (Malayopython reticulatus)", sex)
                        {
                            Body.Tail = new Tail { LengthCm = 850f }; // World's longest snake
                            ConstrictionPressurePsi = 18.0f;
                        }

                        public string AmbushUngulate(Creature ungulate)
                        {
                            if (!IsAlive || !ungulate.IsAlive) return "...";
                            ungulate.TakeDamage(70f, DamageType.Asphyxiation, "Reticulated python constriction");
                            return $"{Name} strikes from foliage with 8.5m body, wrapping coils around {ungulate.GetType().Name}!";
                        }
                    }
                }

                // Family 4: Colubridae (Rear-fanged & Harmless Snakes)
                public class Colubridae : Serpentes
                {
                    public Colubridae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, teeths: 32, sex: sex, name: name ?? "Colubrid") { }

                    public class Boomslang : Colubridae
                    {
                        public Boomslang(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Boomslang (Dispholidus typus)", sex)
                        {
                            IsVenomous = true;
                            VenomType = "Hemotoxin (Rear-fanged prothrombin-activating coagulopathy)";
                            MaxLifespanYears = 14f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string RearFangBite(Creature target)
                        {
                            target.TakeDamage(50f, DamageType.Poison, "Boomslang hemotoxin");
                            return $"{Name} unhinges jaw to 170° and chews rear grooved fangs into {target.GetType().Name}, causing systemic internal hemorrhaging!";
                        }
                    }

                    public class CaliforniaKingsnake : Colubridae
                    {
                        public CaliforniaKingsnake(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "California Kingsnake (Lampropeltis californiae)", sex)
                        {
                            ConstrictionPressurePsi = 14f;
                            MaxLifespanYears = 20f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string SqueezeRattlesnake(Viperidae viper)
                        {
                            viper.TakeDamage(40f, DamageType.Asphyxiation, "Kingsnake constriction");
                            return $"{Name} utilizes innate biochemical immunity to viper venom, overpowering and eating {viper.Name}!";
                        }
                    }
                }
            }

            // --- INFRA-ORDER 2: SAURIA (LIZARDS - NESTED IN SQUAMATA) ---
            public abstract class Sauria : Squamata
            {
                public bool CanAutotomizeTail { get; protected set; } = false;
                public bool HasRegeneratingTail { get; protected set; } = false;

                public Sauria(Creature? father = null, Creature? mother = null, int teeths = 24, SexType? sex = null, string? name = null)
                    : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 5, teeths: teeths, sex: sex, name: name)
                {
                }

                // Lizard Families:
                public class Varanidae : Sauria // Monitor Lizards (Komodo Dragon)
                {
                    public float VenomAnticoagulantPower { get; } = 85f;

                    public Varanidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, teeths: 60, sex: sex, name: name ?? "Varanid Monitor")
                    {
                        MaxLifespanYears = 30f + _rng.Next(-4, 8);
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public class KomodoDragon : Varanidae
                    {
                        public KomodoDragon(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Apex Komodo Dragon (Varanus komodoensis)", sex)
                        {
                            Body.Tail = new Tail { LengthCm = 150f };
                        }

                        public string ToxicBite(Creature target)
                        {
                            if (!IsAlive || !target.IsAlive) return "...";
                            target.TakeDamage(45f, DamageType.Slashing);
                            target.Body.Heart.SystolicPressure = Math.Max(20f, target.Body.Heart.SystolicPressure - 40f); // Septic shock
                            return $"{Name} bites with serrated shark-like teeth and secretes shock-inducing anticoagulant toxins!";
                        }
                    }

                    public class PerentieMonitor : Varanidae
                    {
                        public PerentieMonitor(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Perentie Monitor (Varanus giganteus)", sex)
                        {
                            MaxLifespanYears = 25f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string TripodSurvey() => $"{Name} rears up on hind legs and tail in a bipedal tripod stance, scanning outback plains!";
                    }
                }

                public class Chamaeleonidae : Sauria // Chameleons
                {
                    public string SkinColorHex { get; private set; } = "#2E8B57"; // Forest green
                    public bool EyesIndependent { get; } = true;
                    public float TongueReachMeters { get; } = 0.5f;

                    public Chamaeleonidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, teeths: 18, sex: sex, name: name ?? "Chameleon")
                    {
                        CanAutotomizeTail = false;
                        MaxLifespanYears = 7f + _rng.Next(-1, 3);
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public class VeiledChameleon : Chamaeleonidae
                    {
                        public VeiledChameleon(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Veiled Chameleon (Chamaeleo calyptratus)", sex) { }

                        public string Camouflage(string ambientColorHex)
                        {
                            SkinColorHex = ambientColorHex;
                            return $"{Name} shifts dermal iridophore nanocrystals, camouflaging into {ambientColorHex}!";
                        }
                    }

                    public class PantherChameleon : Chamaeleonidae
                    {
                        public PantherChameleon(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Panther Chameleon (Furcifer pardalis)", sex) { }

                        public string ProjectileTongueStrike(Arthropod bug)
                        {
                            if (!bug.IsAlive) return "...";
                            bug.TakeDamage(15f, DamageType.Blunt, "Chameleon ballistic tongue");
                            return $"{Name} launches ballistic muscular tongue at 26 body lengths/sec, snagging {bug.Name} with suction cup mucus tip!";
                        }
                    }
                }

                public class Gekkonidae : Sauria // Geckos
                {
                    public bool HasVanDerWaalsSetae { get; } = true;

                    public Gekkonidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, teeths: 20, sex: sex, name: name ?? "Gecko")
                    {
                        CanAutotomizeTail = true;
                        MaxLifespanYears = 12f + _rng.Next(-2, 4);
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public class TokayGecko : Gekkonidae
                    {
                        public TokayGecko(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Tokay Gecko (Gekko gecko)", sex) { }

                        public string WalkOnCeiling() => $"{Name} utilizes microscopic toe spatulae and van der Waals attraction to run inverted along ceilings!";
                        public string BarkCall() => $"{Name}: *TO-KAY! TO-KAY!* (Vibrant territorial barking vocalization)";
                    }

                    public class SatanicLeafTailedGecko : Gekkonidae
                    {
                        public SatanicLeafTailedGecko(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Satanic Leaf-Tailed Gecko (Uroplatus phantasticus)", sex) { }

                        public string MimicDecayingLeaf() => $"{Name} flattens leaf-notched tail and mossy fringed skin against tree bark, vanishing entirely into rotting forest litter!";
                    }
                }

                public class Iguanidae : Sauria // Iguanas
                {
                    public Iguanidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, teeths: 80, sex: sex, name: name ?? "Iguana") { }

                    public class MarineIguana : Iguanidae
                    {
                        public MarineIguana(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Galapagos Marine Iguana (Amblyrhynchus cristatus)", sex)
                        {
                            MaxLifespanYears = 20f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string GrazeAlgaeUnderwater(float depthMeters = 10f) => $"{Name} dives {depthMeters:F0}m into Pacific ocean surge, scraping macroalgae off submerged lava rocks!";
                        public string SneezeExcessSalt() => $"{Name}: *Pffft!* (Specialized cranial salt glands sneeze concentrated saline mist!)";
                    }
                }

                public class Helodermatidae : Sauria // Gila Monsters (Venomous)
                {
                    public Helodermatidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, teeths: 40, sex: sex, name: name ?? "Helodermatid") { }

                    public class GilaMonster : Helodermatidae
                    {
                        public GilaMonster(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Gila Monster (Heloderma suspectum)", sex)
                        {
                            MaxLifespanYears = 25f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ChewVenomousSaliva(Creature target)
                        {
                            target.TakeDamage(35f, DamageType.Poison, "Gila monster exendin venom");
                            return $"{Name} clamps jaws and chews grooved teeth, capillary-drawing exendin neurotoxin into {target.GetType().Name}!";
                        }
                    }
                }

                public class Phrynosomatidae : Sauria // Horned Lizards
                {
                    public Phrynosomatidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, teeths: 20, sex: sex, name: name ?? "Horned Lizard") { }

                    public class TexasHornedLizard : Phrynosomatidae
                    {
                        public TexasHornedLizard(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Texas Horned Lizard (Phrynosoma cornutum)", sex)
                        {
                            MaxLifespanYears = 8f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string SquirtBloodFromEyes(Creature predator) => $"{Name} restricts orbital venous blood flow, rupturing eyelid capillaries to squirt a noxious stream of blood 1.5m into {predator.GetType().Name}'s face!";
                    }
                }

                public class Agamidae : Sauria // Dragon Lizards
                {
                    public Agamidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, teeths: 24, sex: sex, name: name ?? "Agamid") { }

                    public class ThornyDevil : Agamidae
                    {
                        public ThornyDevil(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Thorny Devil (Moloch horridus)", sex)
                        {
                            MaxLifespanYears = 18f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ChannelDewToMouth() => $"{Name} absorbs desert condensation across spiny scales, drawing water directly to mouth via capillary micro-grooves!";
                    }

                    public class FrilledNeckLizard : Agamidae
                    {
                        public FrilledNeckLizard(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Frilled-Neck Lizard (Chlamydosaurus kingii)", sex)
                        {
                            MaxLifespanYears = 15f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string DisplayFrillAndSprint() => $"{Name} expands 30cm brightly colored pleated neck ruff, hissing and sprinting bipedally on hind legs!";
                    }
                }
            }
        }

        // --- SUB-ORDER 2: CROCODILIA (NESTED IN REPTILE) ---
        public abstract class Crocodilia : Reptile
        {
            public float BiteForceN { get; protected set; } = 15000f; // Apex crushing force
            public bool HasPalatalValve { get; protected set; } = true;

            public Crocodilia(Creature? father = null, Creature? mother = null, int teeths = 68, SexType? sex = null, string? name = null)
                : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 4, teeths: teeths, sex: sex, name: name)
            {
                ScaleHardnessMohs = 5.0f; // Heavy osteoderm scutes
            }

            public virtual string DeathRoll(Creature target)
            {
                if (!IsAlive || !target.IsAlive) return "...";
                float rollDamage = BiteForceN * 0.005f;
                target.TakeDamage(rollDamage, DamageType.Blunt);
                return $"{Name ?? "Crocodilian"} latches on with {BiteForceN} N clamp force and spins into a violent bone-crushing DEATH ROLL!";
            }

            public class Alligatoridae : Crocodilia
            {
                public Alligatoridae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, teeths: 76, sex: sex, name: name ?? "Alligator")
                {
                    MaxLifespanYears = 50f + _rng.Next(-10, 15);
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public class AmericanAlligator : Alligatoridae
                {
                    public AmericanAlligator(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name ?? "American Alligator", sex)
                    {
                        BiteForceN = 14000f;
                    }
                }

                public class BlackCaiman : Alligatoridae
                {
                    public BlackCaiman(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name ?? "Black Caiman (Melanosuchus niger)", sex)
                    {
                        BiteForceN = 16500f;
                        MaxLifespanYears = 60f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string AmbushAmazonianBank() => $"{Name} lurks submerged with dark armored scutes, reigning as the apex predator of the Amazon river basin!";
                }
            }

            public class Crocodylidae : Crocodilia
            {
                public Crocodylidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, teeths: 66, sex: sex, name: name ?? "Crocodile")
                {
                    MaxLifespanYears = 70f + _rng.Next(-10, 20);
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public class NileCrocodile : Crocodylidae
                {
                    public NileCrocodile(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name ?? "Nile Crocodile", sex)
                    {
                        BiteForceN = 19000f;
                    }
                }

                public class SaltwaterCrocodile : Crocodylidae
                {
                    public float WeightKg { get; } = 1300f;
                    public float LengthMeters { get; } = 6.5f;

                    public SaltwaterCrocodile(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name ?? "Saltwater Crocodile (Crocodylus porosus)", sex)
                    {
                        BiteForceN = 22000f; // Strongest measured bite force in modern animal kingdom
                        MaxLifespanYears = 75f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string CrushLargePrey(Creature prey)
                    {
                        if (!IsAlive || !prey.IsAlive) return "...";
                        prey.TakeDamage(90f, DamageType.Blunt, "Saltwater crocodile clamp");
                        return $"{Name} slams jaws shut with {BiteForceN:F0} N crushing force: the most devastating bite on Earth, pulverizing {prey.GetType().Name}!";
                    }
                }
            }

            public class Gavialidae : Crocodilia
            {
                public Gavialidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, teeths: 110, sex: sex, name: name ?? "Gharial") { }

                public class IndianGharial : Gavialidae
                {
                    public IndianGharial(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name ?? "Indian Gharial (Gavialis gangeticus)", sex)
                    {
                        BiteForceN = 4500f;
                        MaxLifespanYears = 55f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SnapNeedleTeeth(Fish fish)
                    {
                        if (!fish.IsAlive) return "...";
                        fish.TakeDamage(35f, DamageType.Piercing, "Gharial needle jaw snap");
                        return $"{Name} whips elongated slender snout sideways through river currents, snagging {fish.SpeciesName} with 110 interlocking needle teeth!";
                    }
                }
            }
        }

        // --- SUB-ORDER 3: TESTUDINES (TURTLES & TORTOISES - NESTED IN REPTILE) ---
        public abstract class Testudines : Reptile
        {
            public float CarapaceHardnessMohs { get; protected set; } = 6.0f;
            public bool IsRetractedInShell { get; private set; } = false;

            public Testudines(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 5, teeths: 0, sex: sex, name: name)
            {
            }

            public virtual void RetractIntoShell() => IsRetractedInShell = true;
            public virtual void EmergeFromShell() => IsRetractedInShell = false;

            public class Testudinidae : Testudines // Land Tortoises
            {
                public Testudinidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, sex, name: name ?? "Tortoise")
                {
                    MaxLifespanYears = 150f + _rng.Next(-20, 40); // Extreme lifespan
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public class GalapagosTortoise : Testudinidae
                {
                    public GalapagosTortoise(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name ?? "Galapagos Giant Tortoise", sex)
                    {
                    }
                }

                public class AldabraGiantTortoise : Testudinidae
                {
                    public AldabraGiantTortoise(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name ?? "Aldabra Giant Tortoise (Aldabrachelys gigantea)", sex)
                    {
                        MaxLifespanYears = 175f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string StretchNeckToBrowse(Plant vegetation) => $"{Name} extends elongated columnar neck to browse high foliage from {vegetation.Name}!";
                }
            }

            public class Cheloniidae : Testudines // Hard-shelled Sea Turtles
            {
                public Cheloniidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, sex, name: name ?? "Sea Turtle")
                {
                    MaxLifespanYears = 80f + _rng.Next(-10, 20);
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public class GreenSeaTurtle : Cheloniidae
                {
                    public GreenSeaTurtle(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name ?? "Green Sea Turtle", sex) { }

                    public string SwimPelagic(float nauticalMiles) => $"{Name} navigates geomagnetic oceans for {nauticalMiles} nautical miles using flippers!";
                }
            }

            public class Dermochelyidae : Testudines // Leatherback Sea Turtles
            {
                public Dermochelyidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, sex, name: name ?? "Leatherback Turtle") { }

                public class LeatherbackSeaTurtle : Dermochelyidae
                {
                    public float WeightKg { get; } = 700f;

                    public LeatherbackSeaTurtle(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name ?? "Leatherback Sea Turtle (Dermochelys coriacea)", sex)
                    {
                        IsEndothermic = true; // Gigantothermy
                        BodyTemperatureCelsius = 25.5f;
                        MaxLifespanYears = 90f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DeepJellyfishDive(float depthMeters = 1200f) => $"{Name} utilizes flexible leathery carapace and gigantothermy, diving {depthMeters:F0}m to feed on deep-sea jellyfish!";
                }
            }

            public class Chelydridae : Testudines // Snapping Turtles
            {
                public Chelydridae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                    : base(father, mother, sex, name: name ?? "Snapping Turtle") { }

                public class AlligatorSnappingTurtle : Chelydridae
                {
                    public AlligatorSnappingTurtle(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, name ?? "Alligator Snapping Turtle (Macrochelys temminckii)", sex)
                    {
                        MaxLifespanYears = 70f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string WiggleTongueLure(Fish fish)
                    {
                        if (!fish.IsAlive) return "...";
                        fish.TakeDamage(50f, DamageType.Piercing, "Snapping turtle beak clamp");
                        return $"{Name} lies motionless on river bed, wiggling bright pink worm-like tongue lure to snap {fish.SpeciesName} in half with 1,000 N beak!";
                    }
                }
            }
        }

        // --- SUB-ORDER 4: RHYNCHOCEPHALIA (ANCIENT LIVING FOSSILS - TUATARA) ---
        public class Rhynchocephalia : Reptile
        {
            public Rhynchocephalia(Creature? father = null, Creature? mother = null, string? species = "Rhynchocephalian", SexType? sex = null)
                : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 5, teeths: 34, sex: sex, name: species) { }

            public class Sphenodontidae : Rhynchocephalia
            {
                public Sphenodontidae(Creature? father = null, Creature? mother = null, string? species = "Sphenodontid", SexType? sex = null)
                    : base(father, mother, species, sex) { }

                public class Tuatara : Sphenodontidae
                {
                    public bool HasParietalThirdEye { get; } = true;

                    public Tuatara(Creature? father = null, Creature? mother = null, SexType? sex = null)
                        : base(father, mother, "Tuatara (Sphenodon punctatus)", sex)
                    {
                        MaxLifespanYears = 100f; // Extreme slow metabolism
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SenseLightThirdEye() => $"{Name} senses solar UV photoperiods via parietal 'third eye' on crown of skull, maintaining ancestral Triassic diurnal rhythm!";
                }
            }
        }
    }

    // =========================================================================
    // --- 5B. AMPHIBIA: ANURA (FROGS & TOADS), CAUDATA (SALAMANDERS), GYMNOPHIONA (CAECILIANS)
    // =========================================================================

    public class Amphibian : Creature
    {
        public string? Name { get; set; }
        public bool IsCutaneousRespirationActive { get; protected set; } = true;
        public float SkinMoisturePercent { get; protected set; } = 95f;
        public bool HasToxicSkinSecretions { get; protected set; } = false;
        public int HeartChambersCount { get; } = 3;
        public override float SpeciesGestationDays => 21f;
        public override float TeethingAgeYears => 0.05f;
        public override float AdultTeethAgeYears => 0.2f;

        public Amphibian(Creature? father = null, Creature? mother = null, int hands = 2, int legs = 2, int teeths = 20, SexType? sex = null, string? name = null)
            : base(father, mother, hands: hands, handFingers: 4, legs: legs, legFingers: 5, teeths: teeths, sex: sex)
        {
            Name = name;
            IsEndothermic = false;
            BodyTemperatureCelsius = 18f;
            MaxLifespanYears = 12f + _rng.Next(-2, 10);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
        }

        public virtual string CutaneousGasExchange(float o2UptakePercent = 60f)
        {
            if (SkinMoisturePercent < 30f)
                return $"{Name}'s dermal skin is desiccated! Cutaneous gas exchange compromised!";
            return $"{Name} diffuses dissolved oxygen directly across moist vascularized epidermis ({o2UptakePercent:F0}% of total respiratory volume)!";
        }

        // --- ORDER 1: ANURA (FROGS & TOADS - TAILLESS ADULTS, SALTATORIAL LOCOMOTION) ---
        public abstract class Anura : Amphibian
        {
            public float VocalSacAmplificationDecibels { get; protected set; } = 85f;
            public bool HasUrostyle { get; } = true;

            public Anura(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 20)
                : base(father, mother, hands: 2, legs: 2, teeths: teeths, sex: sex, name: name)
            {
                Body.Tail = null; // Tailless adults
            }

            public class Dendrobatidae : Anura // Poison Dart Frogs
            {
                public Dendrobatidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name, teeths: 0)
                {
                    HasToxicSkinSecretions = true;
                }

                public class GoldenPoisonFrog : Dendrobatidae
                {
                    public float BatrachotoxinMicrograms { get; } = 1100f; // Lethal to ~20 adult humans

                    public GoldenPoisonFrog(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Golden Poison Frog (Phyllobates terribilis)")
                    {
                        MaxLifespanYears = 10f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SecreteBatrachotoxin(Creature attacker)
                    {
                        if (!IsAlive || !attacker.IsAlive) return "...";
                        attacker.TakeDamage(95f, DamageType.Poison, "Batrachotoxin steroidal alkaloid");
                        return $"{Name} secretes 1,100 μg of batrachotoxin across dermal granulations, irreversibly opening voltage-gated sodium channels in {attacker.GetType().Name}!";
                    }

                    public string DisplayAposematicColoration() => $"{Name} flaunts intense golden-yellow skin, warning visual predators of lethal neurotoxicity!";
                }

                public class BluePoisonDartFrog : Dendrobatidae
                {
                    public BluePoisonDartFrog(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Blue Poison Dart Frog (Dendrobates tinctorius azureus)")
                    {
                        MaxLifespanYears = 8f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ExudePumiliotoxins(Creature attacker)
                    {
                        if (!IsAlive || !attacker.IsAlive) return "...";
                        attacker.TakeDamage(40f, DamageType.Poison, "Pumiliotoxin alkaloid");
                        return $"{Name} exudes pumiliotoxin B and allopumiliotoxin across blue speckled skin, inducing muscle spasms in {attacker.GetType().Name}!";
                    }
                }
            }

            public class Ranidae : Anura // True Frogs
            {
                public Ranidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name, teeths: 30) { }

                public class AmericanBullfrog : Ranidae
                {
                    public AmericanBullfrog(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "American Bullfrog (Lithobates catesbeianus)")
                    {
                        MaxLifespanYears = 12f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DeepBassCroak() => $"{Name}: *JUG-O-RUM!* (Expands dual circular external vocal sacs, resonating across riparian marshlands!)";

                    public string SwallowWhole(Creature prey)
                    {
                        if (!IsAlive || !prey.IsAlive) return "...";
                        prey.TakeDamage(30f, DamageType.Blunt, "Bullfrog macrophagous gape engulfment");
                        return $"{Name} snaps out sticky projectile tongue and swallows {prey.GetType().Name} whole, depressing eyeballs into orbit to push food down esophagus!";
                    }
                }

                public class GoliathFrog : Ranidae
                {
                    public float WeightKg { get; } = 3.2f;

                    public GoliathFrog(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Goliath Frog (Conraua goliath)")
                    {
                        MaxLifespanYears = 15f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ConstructRockNestPool() => $"{Name} moves heavy 2 kg riverbed stones with muscular legs, constructing clear breeding nursery pools in cascading torrents!";
                    public string GiantLeap(float distanceMeters = 3.0f) => $"{Name} propels 3.2 kg mass {distanceMeters:F1} meters in a single explosive saltatorial leap!";
                }
            }

            public class Hylidae : Anura // Tree Frogs
            {
                public Hylidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name, teeths: 18) { }

                public class RedEyedTreeFrog : Hylidae
                {
                    public RedEyedTreeFrog(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Red-Eyed Tree Frog (Agalychnis callidryas)")
                    {
                        MaxLifespanYears = 5f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string FlashColorationStartle() => $"{Name} abruptly opens scarlet eyes, reveals bright blue flank bars and orange webbed toes, startling visual predators!";
                    public string AdhereToeDiscs() => $"{Name} grips vertical waxy rainforest leaves using hexagonal columnar epithelial cells and capillary mucous adhesion!";
                }

                public class AustralianGreenTreeFrog : Hylidae
                {
                    public AustralianGreenTreeFrog(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Australian Green Tree Frog (Litoria caerulea)")
                    {
                        MaxLifespanYears = 16f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SecreteCaerinPeptides() => $"{Name} secretes caerin antimicrobial and antiviral skin peptides, neutralizing waterborne pathogens!";
                }
            }

            public class Bufonidae : Anura // True Toads
            {
                public Bufonidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name, teeths: 0) // Toads are completely edentulous
                {
                    HasToxicSkinSecretions = true;
                }

                public class CaneToad : Bufonidae
                {
                    public CaneToad(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Cane Marine Toad (Rhinella marina)")
                    {
                        MaxLifespanYears = 15f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DischargeBufotoxin(Creature predator)
                    {
                        if (!IsAlive || !predator.IsAlive) return "...";
                        predator.TakeDamage(45f, DamageType.Poison, "Bufadienolide cardiac glycosides");
                        return $"{Name} compresses post-orbital parotoid glands, squirting milky bufotoxin and marinobufagenin directly into {predator.GetType().Name}'s mouth!";
                    }
                }

                public class CommonToad : Bufonidae
                {
                    public CommonToad(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Common European Toad (Bufo bufo)")
                    {
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string InflateDefensivePosture() => $"{Name} stiffens four legs and inflates lungs to maximum capacity, appearing twice its normal size to deter snakes!";
                }
            }

            public class Pipidae : Anura // Tongueless Aquatic Frogs
            {
                public Pipidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name, teeths: 0) { }

                public class SurinamToad : Pipidae
                {
                    public SurinamToad(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Common Surinam Toad (Pipa pipa)")
                    {
                        MaxLifespanYears = 8f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string BroodDorsalSkin() => $"{Name} embeds fertilized eggs in spongy dorsal epidermal pouches, where froglets emerge fully metamorphosed directly from the mother's back!";
                    public string SenseLateralLine() => $"{Name} detects minute hydro-vibrations with tactile star-shaped fingertip organs and aquatic lateral line neuromasts!";
                }

                public class AfricanClawedFrog : Pipidae
                {
                    public AfricanClawedFrog(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "African Clawed Frog (Xenopus laevis)")
                    {
                        MaxLifespanYears = 15f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ShovelWithClawedHindfeet() => $"{Name} uses black keratinized hind claws to stir muddy pond sediment, capturing invertebrates with sweeping forelimbs!";
                }
            }

            public class Microhylidae : Anura // Narrow-mouthed Frogs
            {
                public Microhylidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name, teeths: 0) { }

                public class DesertRainFrog : Microhylidae
                {
                    public DesertRainFrog(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Desert Rain Frog (Breviceps macrops)")
                    {
                        MaxLifespanYears = 6f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string EmitHighPitchedWarCry() => $"{Name} inflates into an angry translucent spherical ball and emits an adorable high-frequency 4.5 kHz squeaking roar: *MEEEEP!*";
                    public string BurrowInDuneSand() => $"{Name} burrows backwards into moist coastal Namib fog dunes using spade-like keratinized metatarsal tubercles!";
                }
            }
        }

        // --- ORDER 2: CAUDATA (SALAMANDERS & NEWTS - PERSISTENT ADULT TAILS) ---
        public abstract class Caudata : Amphibian
        {
            public bool RetainsLarvalTail { get; } = true;

            public Caudata(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 40)
                : base(father, mother, hands: 2, legs: 2, teeths: teeths, sex: sex, name: name)
            {
                Body.Tail = new Tail { LengthCm = 15f };
            }

            public class Ambystomatidae : Caudata // Mole Salamanders
            {
                public Ambystomatidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Axolotl : Ambystomatidae
                {
                    public bool IsNeotenic { get; } = true;

                    public Axolotl(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Mexican Walking Fish Axolotl (Ambystoma mexicanum)")
                    {
                        MaxLifespanYears = 15f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 12f };
                    }

                    public string RegenerateLimb(string organName = "forelimb") => $"{Name} activates epimorphic regeneration blastema, reconstructing complete cellular architecture of damaged {organName} without scar tissue!";
                    public string OxygenateBranchialGills() => $"{Name} rhythmically flexes 6 feathery external gill rami, extracting dissolved O2 from high-altitude Lake Xochimilco canals!";
                }

                public class TigerSalamander : Ambystomatidae
                {
                    public TigerSalamander(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Eastern Tiger Salamander (Ambystoma tigrinum)")
                    {
                        MaxLifespanYears = 16f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 18f };
                    }

                    public string FossorialBurrow() => $"{Name} excavates deep subterranean burrow shelters up to 60 cm below ground using blunt snout and robust forelimbs!";
                }
            }

            public class Cryptobranchidae : Caudata // Giant Salamanders
            {
                public Cryptobranchidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name, teeths: 60)
                {
                    Body.Tail = new Tail { LengthCm = 50f };
                }

                public class ChineseGiantSalamander : Cryptobranchidae
                {
                    public float LengthMeters { get; } = 1.8f;
                    public float WeightKg { get; } = 50f;

                    public ChineseGiantSalamander(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Chinese Giant Salamander (Andrias davidianus)")
                    {
                        MaxLifespanYears = 60f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 65f };
                    }

                    public string WaveLateralSkinFolds() => $"{Name} undulates wrinkled lateral dermal skin flaps in fast-flowing mountain rivers, maximizing surface area for cutaneous oxygen absorption!";

                    public string SuctionFeedApex(Creature prey)
                    {
                        if (!IsAlive || !prey.IsAlive) return "...";
                        prey.TakeDamage(55f, DamageType.Blunt, "Asymmetrical buccal suction strike");
                        return $"{Name} drops lower jaw in 0.05 seconds, creating immense negative vacuum suction that engulfs {prey.GetType().Name} whole!";
                    }
                }

                public class Hellbender : Cryptobranchidae
                {
                    public Hellbender(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Ozark Hellbender (Cryptobranchus alleganiensis)")
                    {
                        MaxLifespanYears = 30f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 25f };
                    }

                    public string CreepUnderRiverRock() => $"{Name} wedges its flattened dorsoventral body beneath large submerged river slabs, avoiding fast whitewater current!";
                }
            }

            public class Salamandridae : Caudata // True Salamanders & Newts
            {
                public Salamandridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name, teeths: 36)
                {
                    HasToxicSkinSecretions = true;
                }

                public class RoughSkinnedNewt : Salamandridae
                {
                    public RoughSkinnedNewt(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Rough-Skinned Newt (Taricha granulosa)")
                    {
                        MaxLifespanYears = 18f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 9f };
                    }

                    public string UnkenreflexWarning() => $"{Name} arches spine backwards, contorting neck and tail to display vivid warning orange-red ventral belly (Unkenreflex)!";

                    public string DeliverTetrodotoxin(Creature predator)
                    {
                        if (!IsAlive || !predator.IsAlive) return "...";
                        predator.TakeDamage(85f, DamageType.Poison, "Tetrodotoxin (TTX) sodium channel blocker");
                        return $"{Name} exudes pure tetrodotoxin (TTX) across granular skin, completely blocking action potentials in {predator.GetType().Name}!";
                    }
                }

                public class FireSalamander : Salamandridae
                {
                    public FireSalamander(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Fire Salamander (Salamandra salamandra)")
                    {
                        MaxLifespanYears = 24f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 10f };
                    }

                    public string SquirtSamandarinAlkaloid(Creature target)
                    {
                        if (!IsAlive || !target.IsAlive) return "...";
                        target.TakeDamage(35f, DamageType.Poison, "Samandarin neurotoxic alkaloid");
                        return $"{Name} discharges pressurized samandarin steroidal alkaloids from dual parotoid glands, causing hyperventilation and convulsions in {target.GetType().Name}!";
                    }
                }
            }

            public class Plethodontidae : Caudata // Lungless Salamanders
            {
                public Plethodontidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name, teeths: 32)
                {
                    IsCutaneousRespirationActive = true;
                }

                public class RedBackedSalamander : Plethodontidae
                {
                    public RedBackedSalamander(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Eastern Red-Backed Salamander (Plethodon cinereus)")
                    {
                        MaxLifespanYears = 10f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 5f };
                    }

                    public string BallisticTongueStrike(Creature invertebrate)
                    {
                        if (!IsAlive || !invertebrate.IsAlive) return "...";
                        invertebrate.TakeDamage(8f, DamageType.Blunt, "Hyobranchial projectile tongue projection");
                        return $"{Name} fires elastic collagen-powered free hyobranchial tongue out in 4 milliseconds, snagging {invertebrate.GetType().Name}!";
                    }

                    public string CompletelyCutaneousRespire() => $"{Name} relies 100% on capillary cutaneous and buccopharyngeal respiration, lacking both lungs and gills!";
                }
            }

            public class Proteidae : Caudata // Olms & Mudpuppies
            {
                public Proteidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name, teeths: 28)
                {
                    Body.Tail = new Tail { LengthCm = 14f };
                }

                public class Olm : Proteidae
                {
                    public bool IsTroglobiticBlind { get; } = true;

                    public Olm(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Olm Cave Salamander (Proteus anguinus)")
                    {
                        MaxLifespanYears = 100f; // Extreme troglobitic longevity
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 16f };
                    }

                    public string SenseCaveVibrations() => $"{Name} navigates total subterranean cave darkness using ampullary electroreceptors and high-sensitivity mechanoreceptors!";
                    public string EndureSubterraneanStarvation(float years = 8f) => $"{Name} slows basal metabolism to near stasis, surviving {years:F0} years in pitch black cave water without food!";
                }
            }
        }

        // --- ORDER 3: GYMNOPHIONA (CAECILIANS - LIMBLESS FOSSORIAL AMPHIBIANS) ---
        public abstract class Gymnophiona : Amphibian
        {
            public bool IsLimblessVermiform { get; } = true;
            public int AnnuliRingCount { get; protected set; } = 120;

            public Gymnophiona(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int annuli = 120)
                : base(father, mother, hands: 0, legs: 0, teeths: 36, sex: sex, name: name)
            {
                AnnuliRingCount = annuli;
                Body.Tail = new Tail { LengthCm = 2f }; // Extremely short or absent post-cloacal tail
            }

            public class Caeciliidae : Gymnophiona // Common Caecilians
            {
                public Caeciliidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class ThomassCaecilian : Caeciliidae
                {
                    public ThomassCaecilian(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "São Tomé Island Caecilian (Schistometopum thomense)")
                    {
                        MaxLifespanYears = 14f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ProtrudeSensoryTentacles() => $"{Name} extends paired chemosensory tentacles between eye and nostril, probing subterranean soil chemical trails!";
                    public string RamSoilBurrow() => $"{Name} compresses heavily ossified, wedge-shaped cranium forward, boring through compacted volcanic tropical soil!";
                }
            }

            public class Siphonopidae : Gymnophiona // Ringed Caecilians
            {
                public Siphonopidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class RingedCaecilian : Siphonopidae
                {
                    public RingedCaecilian(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Ringed Caecilian (Siphonops annulatus)")
                    {
                        MaxLifespanYears = 12f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string MaternalDermatophagyFeedOffspring() => $"{Name} undergoes maternal dermatophagy: mother transforms outer epidermis into lipid-rich nutritious layer, allowing hatchlings to graze with specialized fetal teeth!";
                }
            }
        }
    }

    // =========================================================================
    // --- 6. ARTHROPODA: INSECTA (BEES, ANTS, BEETLES, MANTIS) & ARACHNIDA (SPIDERS, SCORPIONS)
    // =========================================================================

    public class Arthropod : Creature
    {
        public string? Name { get; set; }
        public float ExoskeletonChitinGrams { get; protected set; } = 5f;
        public bool HasCompoundEyes { get; protected set; } = true;
        public int CompoundEyeOmmatidiaCount { get; protected set; } = 4000;
        public bool IsMoulting { get; protected set; } = false;
        public string HemolymphRespiratoryPigment { get; } = "Hemocyanin (Copper-based, blue blood)";

        public Arthropod(Creature? father = null, Creature? mother = null, int legs = 6, SexType? sex = null, string? name = null)
            : base(father, mother, hands: 0, handFingers: 0, legs: legs, legFingers: 0, teeths: 0, sex: sex)
        {
            Name = name;
            IsEndothermic = false;
            BodyTemperatureCelsius = 20f;
            MaxLifespanYears = 3.5f + _rng.NextSingle() * 2f;
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
        }

        // --- SUB-PHYLUM 1: INSECTA (HEXAPODA - 6 LEGS, 3 BODY SEGMENTS) ---
        public abstract class Insecta : Arthropod
        {
            public enum MetamorphosisStage { Egg, Larva, Pupa, Adult }
            public MetamorphosisStage LifeCycleStage { get; protected set; } = MetamorphosisStage.Adult;
            public int AntennaCount { get; } = 2;
            public int AbdomenSegments { get; } = 11;

            public Insecta(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, legs: 6, sex: sex, name: name)
            {
            }

            // ORDER 1: HYMENOPTERA (BEES & ANTS - NESTED IN INSECTA)
            public abstract class Hymenoptera : Insecta
            {
                public bool IsEusocial { get; protected set; } = true;
                public string ColonyRole { get; set; } = "Worker";

                public Hymenoptera(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Apidae : Hymenoptera // Bees
                {
                    public float HoneyGramsProduced { get; private set; } = 0f;

                    public Apidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Honey Bee")
                    {
                    }

                    public class HoneyBee : Apidae
                    {
                        public HoneyBee(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Western Honey Bee (Apis mellifera)", sex) { }

                        public string PollinateFlower(Plant flower)
                        {
                            HoneyGramsProduced += 0.5f;
                            return $"{Name} collects sweet nectar from {flower.Name}, pollinating flora and synthesizing {HoneyGramsProduced:F1}g pure royal honey!";
                        }

                        public string Sting(Creature enemy)
                        {
                            enemy.TakeDamage(5f, DamageType.Poison, "Bee Sting");
                            Die("Sacrificial autotomy of stinger");
                            return $"{Name} stings {enemy.GetType().Name} with apitoxin formic acid, sacrificially eviscerating stinger for hive defense!";
                        }
                    }
                }

                public class Formicidae : Hymenoptera // Ants
                {
                    public Formicidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Ant")
                    {
                    }

                    public class BulletAnt : Formicidae
                    {
                        public BulletAnt(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Giant Bullet Ant (Paraponera clavata)", sex) { }

                        public string SchmidtSting(Creature target)
                        {
                            target.TakeDamage(15f, DamageType.Poison, "Bullet Ant Poneratoxin");
                            return $"{Name} delivers a 4.0+ Schmidt index sting: pure blistering waves of poneratoxin agony mimicking a gunshot wound!";
                        }

                        public string CarryWeight(float payloadGrams) => $"{Name} hoists {payloadGrams:F1}g of plant foliage (50x its body weight) via mandibular clamp!";
                    }

                    public class LeafcutterAnt : Formicidae
                    {
                        public LeafcutterAnt(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Leafcutter Ant (Atta cephalotes)", sex) { }

                        public string CultivateFungusGarden() => $"{Name} masticates fresh-cut leaves with salivary enzymes into subterranean mulch, cultivating Leucoagaricus gongylophorus fungus gardens!";
                    }

                    public class ArmyAnt : Formicidae
                    {
                        public ArmyAnt(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Eciton Army Ant (Eciton burchellii)", sex) { }

                        public string FormLivingBivouacBridge() => $"{Name} links tarsal claws with 50,000 sister workers, constructing a living architecture suspension bridge across forest ravines!";
                    }
                }

                public class Vespidae : Hymenoptera // Wasps & Hornets
                {
                    public Vespidae(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                        : base(father, mother, sex, name: name ?? "Vespid Wasp") { }

                    public class AsianGiantHornet : Vespidae
                    {
                        public float StingerLengthMm { get; } = 6.2f;

                        public AsianGiantHornet(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Asian Giant Hornet (Vespa mandarinia)", sex) { }

                        public string MandaratoxinSting(Creature target)
                        {
                            target.TakeDamage(25f, DamageType.Poison, "Mandaratoxin tissue-dissolving sting");
                            return $"{Name} plunges {StingerLengthMm}mm hypodermic stinger, injecting cytolytic mandaratoxin that dissolves skin tissue and causes localized necrosis!";
                        }
                    }
                }
            }

            // ORDER 2: COLEOPTERA (BEETLES - NESTED IN INSECTA)
            public abstract class Coleoptera : Insecta
            {
                public bool HasHardenedElytraWings { get; } = true;

                public Coleoptera(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Scarabaeidae : Coleoptera
                {
                    public Scarabaeidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class RhinocerosBeetle : Scarabaeidae
                    {
                        public RhinocerosBeetle(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Rhinoceros Beetle")
                        {
                        }

                        public string LiftWeight(float weightGrams) => $"{Name} locks thoracic chitin horn to hoist {weightGrams:F0}g payload (850x body mass)!";
                    }

                    public class HerculesBeetle : Scarabaeidae
                    {
                        public HerculesBeetle(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name ?? "Hercules Beetle (Dynastes hercules)") { }

                        public string PincerSnap(Creature rival) => $"{Name} clamps 8cm cephalic and pronotal horn pincers with immense mechanical leverage, hurling {rival.GetType().Name} off tree branch!";
                    }

                    public class SacredDungBeetle : Scarabaeidae
                    {
                        public SacredDungBeetle(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name ?? "Sacred Scarab (Scarabaeus sacer)") { }

                        public string NavigateByMilkyWay() => $"{Name} climbs atop rolled brood ball and inspects the galactic plane of the Milky Way, maintaining straight-line celestial course!";
                    }
                }

                public class Lampyridae : Coleoptera
                {
                    public Lampyridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class Firefly : Lampyridae
                    {
                        public bool IsGlowing { get; private set; } = false;

                        public Firefly(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Lightning Bug Firefly")
                        {
                        }

                        public string EmitBioluminescence()
                        {
                            IsGlowing = true;
                            return $"{Name} oxidizes heterocyclic luciferin with ATP & luciferase enzyme, emitting a magical cold green bioluminescent glow!";
                        }
                    }
                }

                public class Carabidae : Coleoptera
                {
                    public Carabidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class BombardierBeetle : Carabidae
                    {
                        public BombardierBeetle(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name ?? "Bombardier Beetle (Brachinus crepitans)") { }

                        public string FireBoilingChemicalCannon(Creature predator)
                        {
                            predator.TakeDamage(20f, DamageType.Fire, "Boiling benzoquinone blast");
                            return $"{Name} mixes hydroquinone and hydrogen peroxide with catalase enzymes in pygidial combustion chamber, detonating a 100°C boiling chemical spray at 500 pulses/sec!";
                        }
                    }
                }

                public class Coccinellidae : Coleoptera
                {
                    public Coccinellidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class SevenSpottedLadybug : Coccinellidae
                    {
                        public SevenSpottedLadybug(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name ?? "Seven-Spot Ladybug (Coccinella septempunctata)") { }

                        public string ReflexBleedAlkaloids() => $"{Name} reflex-bleeds toxic yellow bitter coccinelline alkaloid droplets from tibio-femoral joints to repel birds!";
                    }
                }
            }

            // ORDER 3: LEPIDOPTERA (BUTTERFLIES & MOTHS - NESTED IN INSECTA)
            public abstract class Lepidoptera : Insecta
            {
                public Lepidoptera(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Nymphalidae : Lepidoptera
                {
                    public Nymphalidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class MonarchButterfly : Nymphalidae
                    {
                        public MonarchButterfly(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Monarch Butterfly (Danaus plexippus)")
                        {
                        }

                        public string Metamorphose()
                        {
                            LifeCycleStage = MetamorphosisStage.Adult;
                            return $"{Name} completes holometabolous pupation, unfurling vibrant orange-and-black scaled wings from its chrysalis!";
                        }

                        public string Migrate(float distanceKm) => $"{Name} navigates geomagnetic equatorial air currents on a {distanceKm:F0} km continental migration!";
                    }
                }

                public class Saturniidae : Lepidoptera
                {
                    public Saturniidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class AtlasMoth : Saturniidae
                    {
                        public float WingspanCm { get; } = 25f;

                        public AtlasMoth(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Atlas Moth (Attacus atlas)")
                        {
                            MaxLifespanYears = 3.5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string DisplaySnakeMimicry() => $"{Name} unfurls 25 cm wings displaying serpentine apex wingtip markings to ward off avian predators!";
                    }
                }

                public class Sphingidae : Lepidoptera
                {
                    public Sphingidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class DeathsHeadHawkmoth : Sphingidae
                    {
                        public DeathsHeadHawkmoth(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Death's-head Hawkmoth (Acherontia atropos)")
                        {
                            MaxLifespanYears = 3.5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string SqueakAcousticWarning() => $"{Name} expels air across epipharynx and vibrating labrum, producing an audible 280 Hz piercing squeak!";
                        public string RaidHoneyHive() => $"{Name} secretes olfactory cuticular mimics of Apis mellifera pheromones, raiding beehives undetected!";
                    }
                }
            }

            // ORDER 4: MANTODEA (PRAYING MANTISES - NESTED IN INSECTA)
            public abstract class Mantodea : Insecta
            {
                public Mantodea(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Mantidae : Mantodea
                {
                    public Mantidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class PrayingMantis : Mantidae
                    {
                        public PrayingMantis(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Praying Mantis (Mantis religiosa)")
                        {
                        }

                        public string AmbushStrike(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(20f, DamageType.Slashing, "Raptorial ambush strike");
                            return $"{Name} snaps spined raptorial forelegs in 30 milliseconds, capturing {prey.GetType().Name} with 3D stereoscopic ambush precision!";
                        }
                    }
                }

                public class Hymenopodidae : Mantodea
                {
                    public Hymenopodidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class OrchidMantis : Hymenopodidae
                    {
                        public OrchidMantis(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Orchid Mantis (Hymenopus coronatus)")
                        {
                            MaxLifespanYears = 3.5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string DisguiseAsPetal() => $"{Name} expands walking-leg femoral lobes resembling orchid petals, luring pollinators into strike range!";
                    }
                }
            }

            // ORDER 5: ODONATA (DRAGONFLIES & DAMSELFLIES - NESTED IN INSECTA)
            public abstract class Odonata : Insecta
            {
                public Odonata(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Aeshnidae : Odonata
                {
                    public Aeshnidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class Dragonfly : Aeshnidae
                    {
                        public Dragonfly(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Emperor Dragonfly")
                        {
                        }

                        public string AerobaticIntercept(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(12f, DamageType.Slashing, "Mid-air intercept");
                            return $"{Name} commands 4 independent wings with 95% predation success, intercepting {prey.GetType().Name} in mid-air!";
                        }
                    }
                }

                public class Libellulidae : Odonata
                {
                    public Libellulidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class GlobeSkimmer : Libellulidae
                    {
                        public GlobeSkimmer(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Globe Skimmer Dragonfly (Pantala flavescens)")
                        {
                            MaxLifespanYears = 3.5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string TransOceanicFlight(float distanceKm) => $"{Name} rides intertropical monsoon wind fronts across open ocean covering {distanceKm:F0} km!";
                    }
                }
            }

            // ORDER 6: ORTHOPTERA (GRASSHOPPERS, CRICKETS, LOCUSTS - NESTED IN INSECTA)
            public abstract class Orthoptera : Insecta
            {
                public Orthoptera(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Acrididae : Orthoptera
                {
                    public Acrididae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class DesertLocust : Acrididae
                    {
                        public bool IsGregariousSwarmPhase { get; private set; } = false;

                        public DesertLocust(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Desert Locust (Schistocerca gregaria)")
                        {
                            MaxLifespanYears = 3.5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string TriggerGregariousPhase()
                        {
                            IsGregariousSwarmPhase = true;
                            return $"{Name} experiences hind-leg mechanoreceptor tactile crowding, releasing serotonin and transitioning into voracious migratory swarming phase!";
                        }

                        public string SwarmConsumeFlora(float areaSqKm) => $"{Name} participates in a billion-locust swarm cloud, stripping all vegetative foliage across {areaSqKm:F1} km²!";
                    }
                }

                public class Gryllotalpidae : Orthoptera
                {
                    public Gryllotalpidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class EuropeanMoleCricket : Gryllotalpidae
                    {
                        public EuropeanMoleCricket(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "European Mole Cricket (Gryllotalpa gryllotalpa)")
                        {
                            MaxLifespanYears = 3.5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string DigSubterraneanTrench() => $"{Name} excavates acoustic horn-shaped subterranean burrow chambers with heavily modified dactyl forelegs!";
                        public string ResonantSong() => $"{Name} stridulates within exponential horn burrow, projecting a 92 dB acoustic mating call across 600 meters!";
                    }
                }

                public class Tettigoniidae : Orthoptera
                {
                    public Tettigoniidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class LeafKatydid : Tettigoniidae
                    {
                        public LeafKatydid(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Leaf Katydid (Pseudophyllinae)")
                        {
                            MaxLifespanYears = 3.5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string MimicFoliage() => $"{Name} flattens translucent veined tegmina wings, mimicking necrotic plant leaf margins to vanish from view!";
                    }
                }
            }

            // ORDER 7: HEMIPTERA (TRUE BUGS - ROSTRAL PIERCING-SUCKING MOUTHPARTS)
            public abstract class Hemiptera : Insecta
            {
                public Hemiptera(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Belostomatidae : Hemiptera
                {
                    public Belostomatidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class GiantWaterBug : Belostomatidae
                    {
                        public GiantWaterBug(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Giant Water Bug (Lethocerus americanus)")
                        {
                            MaxLifespanYears = 3.5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string InjectProteolyticEnzymes(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(30f, DamageType.Piercing, "Rostral proteolytic venom");
                            return $"{Name} impales prey with grooved rostrum, injecting digestive salivary enzymes that liquefy interior viscera!";
                        }
                    }
                }

                public class Cicadidae : Hemiptera
                {
                    public Cicadidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class PeriodicalCicada : Cicadidae
                    {
                        public int UndergroundNymphYears { get; } = 17;

                        public PeriodicalCicada(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "17-Year Periodical Cicada (Magicicada septendecim)")
                        {
                            MaxLifespanYears = 18f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string EmergeFromBurrow() => $"{Name} emerges synchronously with billions after 17 subterranean years feeding on tree xylem roots!";
                        public string FlexTymbals() => $"{Name} buckles ribbed abdominal tymbal membranes at 4000 Hz, filling the canopy with 106 dB chorus!";
                    }
                }
            }

            // ORDER 8: DIPTERA (TRUE FLIES & MOSQUITOES - HALTERE GYROSCOPIC STABILIZERS)
            public abstract class Diptera : Insecta
            {
                public Diptera(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Culicidae : Diptera
                {
                    public Culicidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class Mosquito : Culicidae
                    {
                        public Mosquito(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Malaria Mosquito (Anopheles gambiae)")
                        {
                            MaxLifespanYears = 3.5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string FeedBlood(Creature host)
                        {
                            if (!IsAlive || !host.IsAlive) return "...";
                            host.TakeDamage(1f, DamageType.Piercing, "Fascicle proboscis capillary puncture");
                            return $"{Name} pierces dermal capillaries with 6 serrated stylets, secreting anopheline anticoagulants to ingest hematocrit!";
                        }
                    }
                }

                public class Tabanidae : Diptera
                {
                    public Tabanidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class Horsefly : Tabanidae
                    {
                        public Horsefly(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Giant Horsefly (Tabanus bovinus)")
                        {
                            MaxLifespanYears = 3.5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string SlitEpidermis(Creature host)
                        {
                            if (!IsAlive || !host.IsAlive) return "...";
                            host.TakeDamage(5f, DamageType.Slashing, "Scissor-like mandibular laceration");
                            return $"{Name} slices deep capillary pool with blade-like mandibles and laps pooled blood!";
                        }
                    }
                }
            }

            // ORDER 9: BLATTODEA (COCKROACHES & TERMITES - NESTED IN INSECTA)
            public abstract class Blattodea : Insecta
            {
                public Blattodea(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Blaberidae : Blattodea
                {
                    public Blaberidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class MadagascarHissingCockroach : Blaberidae
                    {
                        public MadagascarHissingCockroach(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Madagascar Hissing Cockroach (Gromphadorhina portentosa)")
                        {
                            MaxLifespanYears = 5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ExpelSpiracleHiss() => $"{Name} violently compresses spiracles on 4th abdominal segment, releasing 90 dB acoustic threat hiss!";
                    }
                }

                public class Termitidae : Blattodea
                {
                    public Termitidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class CathedralTermite : Termitidae
                    {
                        public CathedralTermite(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Cathedral Mound Termite (Nasutitermes triodiae)")
                        {
                            MaxLifespanYears = 15f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ShootFontanellarGlue(Creature enemy)
                        {
                            if (!IsAlive || !enemy.IsAlive) return "...";
                            enemy.TakeDamage(10f, DamageType.Poison, "Terpene spray");
                            return $"{Name} soldier fires pressurized noxious terpene glue from frontal fontanellar nozzle, entangling {enemy.GetType().Name}!";
                        }

                        public string ConstructThermoregulatingMound() => $"{Name} architects 5-meter cemented mud spires with passive convection ventilation channels!";
                    }
                }
            }
        }

        // --- SUB-PHYLUM 2: ARACHNIDA (8 LEGS, CEPHALOTHORAX & ABDOMEN) ---
        public abstract class Arachnida : Arthropod
        {
            public int EyesCount { get; } = 8;
            public bool HasCheliceraeFangs { get; } = true;

            public Arachnida(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, legs: 8, sex: sex, name: name)
            {
                MaxLifespanYears = 4f + _rng.Next(0, 8);
                if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            }

            // ORDER 1: ARANEAE (SPIDERS - NESTED IN ARACHNIDA)
            public abstract class Araneae : Arachnida
            {
                public bool SpinsSilkWeb { get; protected set; } = true;

                public Araneae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Theraphosidae : Araneae // Tarantulas
                {
                    public Theraphosidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class Tarantula : Theraphosidae
                    {
                        public Tarantula(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Goliath Bird-Eater Tarantula")
                        {
                            MaxLifespanYears = 20f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string FlickUrticatingHairs() => $"{Name} vigorously kicks microscopic barbed urticating hairs from opisthosoma, blinding attackers!";
                    }
                }

                public class Theridiidae : Araneae // Cobweb Spiders (Black Widow)
                {
                    public Theridiidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class BlackWidow : Theridiidae
                    {
                        public BlackWidow(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Southern Black Widow (Latrodectus mactans)")
                        {
                        }

                        public string BiteLatrotoxin(Creature target)
                        {
                            if (!IsAlive || !target.IsAlive) return "...";
                            target.TakeDamage(50f, DamageType.Poison, "Alpha-latrotoxin neurotoxin");
                            return $"{Name} sinks sharp chelicerae fangs, flooding synapses with alpha-latrotoxin and triggering massive acetylcholine exhaustion!";
                        }

                        public string SpinCobweb() => $"{Name} extrudes proteinaceous dragline silk from spinnerets, constructing high-tensile sticky trap web!";
                    }
                }

                public class Atracidae : Araneae // Funnel-Web Spiders
                {
                    public Atracidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class SydneyFunnelWebSpider : Atracidae
                    {
                        public SydneyFunnelWebSpider(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Sydney Funnel-Web Spider (Atrax robustus)")
                        {
                            MaxLifespanYears = 12f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string DeliverDeltaAtracotoxin(Creature target)
                        {
                            if (!IsAlive || !target.IsAlive) return "...";
                            target.TakeDamage(65f, DamageType.Poison, "Delta-atracotoxin voltage-gated sodium channel inhibitor");
                            return $"{Name} rears in threat display and strikes downward with 5mm fangs, flooding synapses with delta-atracotoxin!";
                        }
                    }
                }

                public class Ctenidae : Araneae // Wandering Spiders
                {
                    public Ctenidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class BrazilianWanderingSpider : Ctenidae
                    {
                        public BrazilianWanderingSpider(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Armed Wandering Spider (Phoneutria nigriventer)")
                        {
                            MaxLifespanYears = 4f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string NeurotoxicStrike(Creature target)
                        {
                            if (!IsAlive || !target.IsAlive) return "...";
                            target.TakeDamage(58f, DamageType.Poison, "PhTx3 neurotoxin cascade");
                            return $"{Name} dances with raised front pairs of legs and delivers PhTx3 venom, causing autonomic storm!";
                        }
                    }
                }

                public class Salticidae : Araneae // Jumping Spiders
                {
                    public Salticidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class PeacockSpider : Salticidae
                    {
                        public PeacockSpider(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Coastal Peacock Spider (Maratus speciosus)")
                        {
                            MaxLifespanYears = 2.5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string CourtshipDisplay() => $"{Name} elevates iridescent structural-color opisthosomal flaps and waves third legs in hypnotic courtship dance!";
                        public string HydraulicLeap(float distanceCm = 20f) => $"{Name} compresses cephalothorax haemolymph hydraulics, catapulting {distanceCm:F0} cm onto target!";
                    }
                }

                public class Sicariidae : Araneae // Six-Eyed Sand Spiders
                {
                    public Sicariidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class SixEyedSandSpider : Sicariidae
                    {
                        public SixEyedSandSpider(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Six-Eyed Sand Spider (Hexophthalma hahni)")
                        {
                            MaxLifespanYears = 15f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string AmbushFromDune(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(70f, DamageType.Poison, "Sphingomyelinase D dermonecrotic venom");
                            return $"{Name} bursts from subterranean dune camouflage, delivering cryonecrotic sphingomyelinase D!";
                        }
                    }
                }
            }

            // ORDER 2: SCORPIONES (SCORPIONS - NESTED IN ARACHNIDA)
            public abstract class Scorpiones : Arachnida
            {
                public bool FluorescesUnderUV { get; } = true;

                public Scorpiones(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Buthidae : Scorpiones
                {
                    public Buthidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class DeathstalkerScorpion : Buthidae
                    {
                        public DeathstalkerScorpion(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Deathstalker Scorpion (Leiurus quinquestriatus)")
                        {
                            MaxLifespanYears = 6f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string StingVenom(Creature target)
                        {
                            if (!IsAlive || !target.IsAlive) return "...";
                            target.TakeDamage(55f, DamageType.Poison, "Chlorotoxin & scyllatoxin");
                            return $"{Name} grasps with chelae pedipalps and whips segmented metasoma telson, injecting lethal chlorotoxin & scyllatoxin cocktail!";
                        }

                        public string GlowUnderUltraviolet() => $"{Name} excites beta-carboline cuticular fluorophores, glowing intense cyan-blue under UV light!";
                    }
                }

                public class Scorpionidae : Scorpiones
                {
                    public Scorpionidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class EmperorScorpion : Scorpionidae
                    {
                        public float ChelaCrushForceNewtons { get; } = 40f;

                        public EmperorScorpion(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Emperor Scorpion (Pandinus imperator)")
                        {
                            MaxLifespanYears = 8f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string CrushWithPedipalps(Creature target)
                        {
                            if (!IsAlive || !target.IsAlive) return "...";
                            target.TakeDamage(25f, DamageType.Blunt, "Granular chelae pincers");
                            return $"{Name} locks massive tuberculate pedipalps with 40N clamping force, immobilizing {target.GetType().Name}!";
                        }
                    }
                }
            }

            // ORDER 3: AMBLYPYGI (WHIP SPIDERS / TAILLESS WHIP SCORPIONS)
            public abstract class Amblypygi : Arachnida
            {
                public Amblypygi(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Phrynidae : Amblypygi
                {
                    public Phrynidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class TaillessWhipScorpion : Phrynidae
                    {
                        public TaillessWhipScorpion(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Florida Tailless Whip Scorpion (Phrynus marginemaculatus)")
                        {
                            MaxLifespanYears = 7f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string SweepAntenniformLegs() => $"{Name} sweeps 25 cm elongated whip-like sensory front legs in total darkness, detecting microscopic air vibrations!";
                    }
                }
            }

            // ORDER 4: SOLIFUGAE (CAMEL SPIDERS / SUN SPIDERS)
            public abstract class Solifugae : Arachnida
            {
                public Solifugae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Galeodidae : Solifugae
                {
                    public Galeodidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class CamelSpider : Galeodidae
                    {
                        public CamelSpider(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Egyptian Giant Solifugid (Galeodes arabs)")
                        {
                            MaxLifespanYears = 3f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ShearWithChelicerae(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(30f, DamageType.Slashing, "Dual two-segmented cheliceral shears");
                            return $"{Name} sprints at 16 km/h, sawing through prey with massive dual cheliceral jaws producing audible stridulation!";
                        }
                    }
                }
            }

            // ORDER 5: IXODIDA (TICKS)
            public abstract class Ixodida : Arachnida
            {
                public Ixodida(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name: name) { }

                public class Ixodidae : Ixodida
                {
                    public Ixodidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class DeerTick : Ixodidae
                    {
                        public DeerTick(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Blacklegged Deer Tick (Ixodes scapularis)")
                        {
                            MaxLifespanYears = 2.5f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string QuestAndAnchor(Creature host)
                        {
                            if (!IsAlive || !host.IsAlive) return "...";
                            host.TakeDamage(1f, DamageType.Piercing, "Hypostome barbed anchoring");
                            return $"{Name} waves foreleg Haller's organs sensing CO2, embedding retrograde barbed hypostome into dermal tissue!";
                        }
                    }
                }
            }
        }

        // --- SUB-PHYLUM 3: CRUSTACEA (MALACOSTRACA, DECAPODS, CIRRIPEDIA) ---
        public abstract class Crustacea : Arthropod
        {
            public bool HasCalcifiedCarapace { get; } = true;
            public bool HasBiramousAppendages { get; } = true;

            public Crustacea(Creature? father = null, Creature? mother = null, int legs = 10, SexType? sex = null, string? name = null)
                : base(father, mother, legs: legs, sex: sex, name: name)
            {
                MaxLifespanYears = 12f + _rng.Next(0, 20);
                if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            }

            // ORDER 1: DECAPODA (CRABS, LOBSTERS, CRAYFISH - NESTED IN CRUSTACEA)
            public abstract class Decapoda : Crustacea
            {
                public Decapoda(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, legs: 10, sex: sex, name: name) { }

                public class Coenobitidae : Decapoda
                {
                    public Coenobitidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class CoconutCrab : Coenobitidae
                    {
                        public float PinchForceNewtons { get; } = 3300f;

                        public CoconutCrab(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Coconut Robber Crab (Birgus latro)")
                        {
                            MaxLifespanYears = 60f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string CrackCoconutShell() => $"{Name} crushes dense fibrous coconut husks with 3,300 N chelae pinch force!";
                    }
                }

                public class Inachidae : Decapoda
                {
                    public Inachidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class JapaneseSpiderCrab : Inachidae
                    {
                        public float LegSpanMeters { get; } = 3.8f;

                        public JapaneseSpiderCrab(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Japanese Spider Crab (Macrocheira kaempferi)")
                        {
                            MaxLifespanYears = 100f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ForageAbyssalBenthos() => $"{Name} stalks abyssal vents on 3.8-meter arm-span stilt legs, scavenging benthic detritus!";
                    }
                }

                public class Homaridae : Decapoda
                {
                    public Homaridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class AmericanLobster : Homaridae
                    {
                        public AmericanLobster(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "American Atlantic Lobster (Homarus americanus)")
                        {
                            MaxLifespanYears = 50f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string CrushWithMajorChela(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(45f, DamageType.Blunt, "Crusher claw molariform tooth strike");
                            return $"{Name} snaps asymmetrical molariform crusher claw, pulverizing {prey.GetType().Name}'s shell!";
                        }
                    }
                }

                public class Astacidae : Decapoda
                {
                    public Astacidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class EuropeanCrayfish : Astacidae
                    {
                        public EuropeanCrayfish(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Noble Crayfish (Astacus astacus)")
                        {
                            MaxLifespanYears = 20f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string AbdominalTailFlipEscape() => $"{Name} flexes heavily muscled abdomen and broad uropods, rocketing backward to evade predator!";
                    }
                }
            }

            // ORDER 2: STOMATOPODA (MANTIS SHRIMPS - NESTED IN CRUSTACEA)
            public abstract class Stomatopoda : Crustacea
            {
                public Stomatopoda(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, legs: 10, sex: sex, name: name) { }

                public class Odontodactylidae : Stomatopoda
                {
                    public Odontodactylidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class PeacockMantisShrimp : Odontodactylidae
                    {
                        public PeacockMantisShrimp(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Peacock Mantis Shrimp (Odontodactylus scyllarus)")
                        {
                            MaxLifespanYears = 6f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string DactylCavitationStrike(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(60f, DamageType.Blunt, "Cavitation dactyl club impact");
                            return $"{Name} unleashes spring-loaded calcified dactyl club at 80 km/h, generating supercavitating vapor bubbles and smashing prey!";
                        }

                        public string AnalyzePolarizedLight() => $"{Name} scans environment with 16 distinct color photoreceptor pigments and hexnocular trinocular stereopsis!";
                    }
                }
            }

            // ORDER 3: ALPHEIDAE (SNAPPING / PISTOL SHRIMPS - NESTED IN CRUSTACEA)
            public abstract class Alpheidae : Crustacea
            {
                public Alpheidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, legs: 10, sex: sex, name: name) { }

                public class Alpheus : Alpheidae
                {
                    public Alpheus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class PistolShrimp : Alpheus
                    {
                        public PistolShrimp(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Bigclaw Snapping Shrimp (Alpheus heterochaelis)")
                        {
                            MaxLifespanYears = 4f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string SnapCavitationBlast(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(35f, DamageType.Blunt, "Sonoluminescent plasma shockwave");
                            return $"{Name} slams enlarged snapper claw at 100 km/h, collapsing a cavitation bubble that reaches 4,700°C and 210 dB!";
                        }
                    }
                }
            }

            // ORDER 4: ISOPODA (PILL BUGS & GIANT ISOPODS - NESTED IN CRUSTACEA)
            public abstract class Isopoda : Crustacea
            {
                public Isopoda(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, legs: 14, sex: sex, name: name) { }

                public class Armadillidiidae : Isopoda
                {
                    public Armadillidiidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class CommonPillBug : Armadillidiidae
                    {
                        public bool IsConglobated { get; private set; } = false;

                        public CommonPillBug(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Common Roly-Poly Pill Bug (Armadillidium vulgare)")
                        {
                            MaxLifespanYears = 3f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ConglobateIntoBall()
                        {
                            IsConglobated = true;
                            return $"{Name} curls into an armored spherical ball (volvation), tucking delicate ventral limbs within armored tergites!";
                        }
                    }
                }

                public class Cirolanidae : Isopoda
                {
                    public Cirolanidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name) { }

                    public class GiantIsopod : Cirolanidae
                    {
                        public GiantIsopod(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Deep-Sea Giant Isopod (Bathynomus giganteus)")
                        {
                            MaxLifespanYears = 15f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string DeepSeaScavenge() => $"{Name} gorges on fallen whale fall carrion at 1,000 meters depth, surviving years between meals!";
                    }
                }
            }
        }

        // --- SUB-PHYLUM 4: MYRIAPODA (CENTIPEDES & MILLIPEDES) ---
        public abstract class Myriapoda : Arthropod
        {
            public int SegmentCount { get; protected set; } = 21;

            public Myriapoda(Creature? father = null, Creature? mother = null, int legs = 42, SexType? sex = null, string? name = null)
                : base(father, mother, legs: legs, sex: sex, name: name)
            {
                MaxLifespanYears = 6f + _rng.Next(0, 8);
                if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            }

            // CLASS 1: CHILOPODA (CENTIPEDES - 1 LEG PAIR PER SEGMENT, FORCIPULES)
            public abstract class Chilopoda : Myriapoda
            {
                public Chilopoda(Creature? father = null, Creature? mother = null, int legs = 42, SexType? sex = null, string? name = null)
                    : base(father, mother, legs: legs, sex: sex, name: name) { }

                public class Scolopendridae : Chilopoda
                {
                    public Scolopendridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, legs: 46, sex: sex, name: name) { }

                    public class AmazonianGiantCentipede : Scolopendridae
                    {
                        public AmazonianGiantCentipede(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Peruvian Giant Centipede (Scolopendra gigantea)")
                        {
                            MaxLifespanYears = 10f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ForcipuleVenomStrike(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(52f, DamageType.Poison, "Cardiotoxic forcipule venom");
                            return $"{Name} anchors with posterior legs from cave ceiling and sinks modified toxicognaths into {prey.GetType().Name}!";
                        }
                    }
                }
            }

            // CLASS 2: DIPLOPODA (MILLIPEDES - 2 LEG PAIRS PER SEGMENT, DIPLODERM)
            public abstract class Diplopoda : Myriapoda
            {
                public Diplopoda(Creature? father = null, Creature? mother = null, int legs = 256, SexType? sex = null, string? name = null)
                    : base(father, mother, legs: legs, sex: sex, name: name) { }

                public class Spirostreptidae : Diplopoda
                {
                    public Spirostreptidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, legs: 256, sex: sex, name: name) { }

                    public class GiantAfricanMillipede : Spirostreptidae
                    {
                        public GiantAfricanMillipede(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, sex, name: name ?? "Giant African Millipede (Archispirostreptus gigas)")
                        {
                            MaxLifespanYears = 7f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ExudeOzoporeBenzoquinones() => $"{Name} rolls into tight spiral coil, exuding caustic defensive benzoquinones from lateral repugnatorial glands!";
                    }
                }
            }
        }
    }

    // =========================================================================
    // --- 7. PRIMATE (HOMINIDAE: CHIMPANZEE, GORILLA)
    // =========================================================================

    public class Primate : Creature
    {
        public string? Name { get; set; }
        public float OpposableThumbDexterity { get; protected set; } = 0.95f;
        public float ToolUseIntelligence { get; protected set; } = 85f;
        public override float SpeciesGestationDays => 240f;
        public override float TeethingAgeYears => 0.25f;
        public override float AdultTeethAgeYears => 5.0f;

        public Primate(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 32)
            : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 5, teeths: teeths, sex: sex)
        {
            Name = name;
            MaxLifespanYears = 45f + _rng.Next(-5, 10);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(2, MammaryGland.CupSize.C, 250f);
            }
        }

        public abstract class Hominidae : Primate
        {
            public Hominidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, sex, name) { }

            public class Chimpanzee : Hominidae
            {
                public Chimpanzee(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Common Chimpanzee (Pan troglodytes)")
                {
                    ToolUseIntelligence = 90f;
                    MaxLifespanYears = 40f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string FashionTool(string branchName) => $"{Name} strips leaves from a {branchName} twig to probe termite mounds for insect protein!";
                public string ScreechAndDisplay() => $"{Name}: *Hoo-hoo-hoo-HAAA-HAAA!* (Pounds chest and drums on tree buttress roots!)";
            }

            public class Gorilla : Hominidae
            {
                public float SilverbackStrengthKg { get; } = 800f;

                public Gorilla(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Mountain Gorilla (Gorilla beringei beringei)")
                {
                    ToolUseIntelligence = 82f;
                    MaxLifespanYears = 45f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string BeatChest() => $"{Name}: *THUMP-THUMP-THUMP-ROAR!* (Silverback pounds massive muscular pectorals in display of dominance!)";
            }

            public class Orangutan : Hominidae
            {
                public float ArmSpanMeters { get; } = 2.2f;

                public Orangutan(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Sumatran Orangutan (Pongo abelii)")
                {
                    ToolUseIntelligence = 88f;
                    MaxLifespanYears = 50f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string ConstructCanopyNest() => $"{Name} weaves living ironwood branches into a complex double-layered canopy sleeping platform with leafy pillow!";
                public string FlangedCheekLongCall() => $"{Name} inflates throat sac between fibrous cheek flanges, echoing a booming resonant long-call across primary peat-swamp canopy!";
            }

            public class Bonobo : Hominidae
            {
                public Bonobo(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Bonobo (Pan paniscus)")
                {
                    ToolUseIntelligence = 89f;
                    MaxLifespanYears = 42f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string ReconcileConflict() => $"{Name} initiates empathetic affiliative social grooming and socio-sexual bonding, defusing troop tension without violence!";
            }
        }

        // FAMILY 2: HYLOBATIDAE (LESSER APES / GIBBONS - ARBOREAL BRACHIATION)
        public abstract class Hylobatidae : Primate
        {
            public Hylobatidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, sex, name) { }

            public class Siamang : Hylobatidae
            {
                public Siamang(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Siamang Gibbon (Symphalangus syndactylus)")
                {
                    MaxLifespanYears = 35f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string BrachiateCanopy(float speedKmh = 55f) => $"{Name} swings hand-over-hand (ricochetal brachiation) across 15-meter canopy gaps at {speedKmh:F0} km/h!";
                public string DuetThroatSacDuet() => $"{Name} inflates massive grey gular sac to head-size, producing booming resonant duet vocalizations audible 3 km away!";
            }
        }

        // FAMILY 3: CERCOPITHECIDAE (OLD WORLD MONKEYS)
        public abstract class Cercopithecidae : Primate
        {
            public Cercopithecidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, sex, name) { }

            public class Mandrill : Cercopithecidae
            {
                public float FightingCaninesCm { get; } = 6.5f;

                public Mandrill(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Mandrill (Mandrillus sphinx)")
                {
                    MaxLifespanYears = 30f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string YawnCanineDisplay() => $"{Name} bares massive 6.5 cm fighting canines, contrasting against vivid cobalt-blue and scarlet paranasal facial ridges!";
            }

            public class JapaneseMacaque : Cercopithecidae
            {
                public JapaneseMacaque(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Japanese Snow Macaque (Macaca fuscata)")
                {
                    MaxLifespanYears = 28f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string BatheInThermalSprings() => $"{Name} submerses in volcanic 41°C geothermal onsen hot springs, thermoregulating against sub-zero alpine snowstorms!";
                public string WashSweetPotatoInOcean() => $"{Name} demonstrates transmitted cultural learning, washing sandy tubers in ocean saltwater for seasoning!";
            }

            public class Gelada : Cercopithecidae
            {
                public Gelada(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Gelada Baboon (Theropithecus gelada)")
                {
                    MaxLifespanYears = 25f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string FlipLipThreatDisplay() => $"{Name} flips upper lip completely over nostrils, displaying white gumline and razor teeth in high-intensity threat!";
                public string PluckAlpineFestucaGrass() => $"{Name} shuffles on ischial callosities, harvesting high-altitude Ethiopian montane grass blades with specialized precision-grip fingers!";
            }
        }

        // FAMILY 4: ATELIDAE (NEW WORLD MONKEYS - PREHENSILE TAILS)
        public abstract class Atelidae : Primate
        {
            public Atelidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, sex, name)
            {
                Body.Tail = new Tail { LengthCm = 75f };
            }

            public class BlackHowlerMonkey : Atelidae
            {
                public BlackHowlerMonkey(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Black Howler Monkey (Alouatta caraya)")
                {
                    MaxLifespanYears = 20f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string RoarThroughHyoidBone() => $"{Name} resonates air across enlarged ossified hyoid vocal chamber, unleashing an ear-splitting 140 dB roar through the jungle!";
            }

            public class GeoffroysSpiderMonkey : Atelidae
            {
                public GeoffroysSpiderMonkey(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Geoffroy's Spider Monkey (Ateles geoffroyi)")
                {
                    MaxLifespanYears = 27f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string SuspendByPrehensileTail() => $"{Name} hangs effortlessly by naked dermal friction ridges of prehensile tail tip, freeing both hands for ripe fruit foraging!";
            }
        }

        // SUBORDER: STREPSIRRHINI (LEMURS, LORISES, AYE-AYES - TOOTHCOMB & RHINARIUM)
        public abstract class Strepsirrhini : Primate
        {
            public bool HasToothcomb { get; } = true;

            public Strepsirrhini(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, sex, name, teeths: 36) { }

            public class Lemuridae : Strepsirrhini
            {
                public Lemuridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class RingTailedLemur : Lemuridae
                {
                    public RingTailedLemur(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Ring-Tailed Lemur (Lemur catta)")
                    {
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 60f };
                    }

                    public string StinkFight(Creature rival) => $"{Name} rubs antebrachial wrist spur secretions along 13-ringed tail and wafts pungent pheromones toward {rival.GetType().Name}!";
                    public string SunbatheLotus() => $"{Name} sits in open-armed 'lotus posture' facing the morning sun, warming white ventral fur!";
                }
            }

            public class Daubentoniidae : Strepsirrhini
            {
                public Daubentoniidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class AyeAye : Daubentoniidae
                {
                    public AyeAye(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Aye-Aye (Daubentonia madagascariensis)")
                    {
                        MaxLifespanYears = 23f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string PercussiveForage() => $"{Name} taps hollow dead branches 8 times/second with elongated wire-thin 3rd digit, locating beetle grubs by micro-echoes!";
                }
            }

            public class Lorisidae : Strepsirrhini
            {
                public Lorisidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class SlowLoris : Lorisidae
                {
                    public SlowLoris(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Bengal Slow Loris (Nycticebus coucang)")
                    {
                        MaxLifespanYears = 22f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string VenomousGroomingBite(Creature attacker)
                    {
                        if (!IsAlive || !attacker.IsAlive) return "...";
                        attacker.TakeDamage(30f, DamageType.Poison, "Brachial gland venom activation");
                        return $"{Name} raises arms, licks brachial gland secretions mixed with saliva, delivering an anaphylactic toxic bite to {attacker.GetType().Name}!";
                    }
                }
            }
        }
    }

    // =========================================================================
    // --- 8. URSIDAE (BEARS: GRIZZLY BEAR, POLAR BEAR)
    // =========================================================================

    public class Bear : Creature
    {
        public string? Name { get; set; }
        public bool IsHibernating { get; private set; } = false;
        public float FatReserveKg { get; protected set; } = 60f;
        public override float SpeciesGestationDays => 220f;
        public override float TeethingAgeYears => 0.15f;
        public override float AdultTeethAgeYears => 2.0f;

        public Bear(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
            : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 5, teeths: 42, sex: sex)
        {
            Name = name;
            MaxLifespanYears = 28f + _rng.Next(-3, 6);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 12f };

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(6, MammaryGland.CupSize.B, 150f);
            }
        }

        public virtual void EnterHibernation() => IsHibernating = true;
        public virtual void WakeFromHibernation() => IsHibernating = false;

        public class Ursus : Bear
        {
            public Ursus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, sex, name) { }

            public class GrizzlyBear : Ursus
            {
                public GrizzlyBear(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Grizzly Bear (Ursus arctos horribilis)")
                {
                    MaxLifespanYears = 30f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string CatchSalmon(Fish salmon)
                {
                    if (!salmon.IsAlive) return "...";
                    salmon.TakeDamage(100f, DamageType.Slashing, "Bear claw strike");
                    FatReserveKg += 2.5f;
                    return $"{Name} swipes massive curved 10cm non-retractable claws at waterfall rapids, seizing {salmon.SpeciesName}!";
                }
            }

            public class PolarBear : Ursus
            {
                public PolarBear(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Polar Bear (Ursus maritimus)")
                {
                    FatReserveKg = 120f;
                    MaxLifespanYears = 26f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string SwimArcticOcean(float distanceKm) => $"{Name} paddles through freezing Arctic sea ice with broad webbed forepaws for {distanceKm:F1} km!";
            }

            public class AsianBlackBear : Ursus
            {
                public AsianBlackBear(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Asian Moon Bear (Ursus thibetanus)")
                {
                    MaxLifespanYears = 25f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string BuildArborealNest() => $"{Name} builds a sturdy sleeping platform high in the oak canopy by bending and snapping fruiting branches!";
            }
        }

        public class Ailuropoda : Bear
        {
            public Ailuropoda(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, sex, name) { }

            public class GiantPanda : Ailuropoda
            {
                public bool HasRadialSesamoidPseudoThumb { get; } = true;

                public GiantPanda(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Giant Panda (Ailuropoda melanoleuca)")
                {
                    MaxLifespanYears = 22f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string StripBambooCulms(float kgBamboo = 14f) => $"{Name} grasps woody bamboo stems with enlarged radial sesamoid 'pseudo-thumb' and crushes {kgBamboo:F0} kg of culms daily!";
            }
        }

        public class Tremarctos : Bear
        {
            public Tremarctos(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, sex, name) { }

            public class SpectacledBear : Tremarctos
            {
                public SpectacledBear(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Spectacled Andean Bear (Tremarctos ornatus)")
                {
                    MaxLifespanYears = 24f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string ForageAndeanBromeliads() => $"{Name} scales mist-shrouded cloud forest trees to harvest epiphytic bromeliads and palm hearts!";
            }
        }

        public class Melursus : Bear
        {
            public Melursus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, sex, name) { }

            public class SlothBear : Melursus
            {
                public SlothBear(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Sloth Bear (Melursus ursinus)")
                {
                    MaxLifespanYears = 20f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string VacuumTermiteMound() => $"{Name} blows away dirt and seals nostrils, using missing upper incisors and hollow palate to vacuum 10,000 termites with 95 dB suction!";
            }
        }

        public class Helarctos : Bear
        {
            public Helarctos(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, sex, name) { }

            public class SunBear : Helarctos
            {
                public float TongueLengthCm { get; } = 25f;

                public SunBear(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Malayan Sun Bear (Helarctos malayanus)")
                {
                    MaxLifespanYears = 25f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string ExtractHoneyWithTongue() => $"{Name} tears apart bee nests with sickle claws and extends 25 cm prehensile tongue deep into tree cavities!";
            }
        }
    }

    // =========================================================================
    // --- 9. PROBOSCIDEA (ELEPHANTS: AFRICAN BUSH, ASIAN, WOOLLY MAMMOTH)
    // =========================================================================

    public class Elephant : Creature
    {
        public string? Name { get; set; }
        public float TrunkMusclesCount { get; } = 40000f;
        public float TuskLengthCm { get; protected set; } = 150f;
        public override float SpeciesGestationDays => 660f; // Longest mammalian gestation (~22 months)
        public override float TeethingAgeYears => 1.0f;
        public override float AdultTeethAgeYears => 14.0f;

        public Elephant(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
            : base(father, mother, hands: 2, handFingers: 4, legs: 2, legFingers: 3, teeths: 26, sex: sex)
        {
            Name = name ?? "Elephant";
            MaxLifespanYears = 65f + _rng.Next(-5, 12);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 110f };

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(2, MammaryGland.CupSize.E, 2500f);
            }
        }

        public class Elephantidae : Elephant
        {
            public Elephantidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, sex, name) { }

            public class AfricanBushElephant : Elephantidae
            {
                public AfricanBushElephant(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "African Bush Elephant (Loxodonta africana)")
                {
                    MaxLifespanYears = 70f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string TrumpetAndRumble() => $"{Name}: *PAAAAUUUUURRRRR!* (Emits 110 dB trumpet blast and 15 Hz infrasonic ground-penetrating seismic rumble!)";
                public string SprayWaterWithTrunk(float liters = 8f) => $"{Name} siphons {liters:F1}L of water into its prehensile proboscis trunk and sprays cooling mist!";
            }

            public class AsianElephant : Elephantidae
            {
                public AsianElephant(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Asian Elephant (Elephas maximus)")
                {
                    MaxLifespanYears = 65f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string ManipulatePrecisionTrunk() => $"{Name} uses single dorsal prehensile finger-tip on trunk to pluck single blades of grass and open latch gates!";
            }

            public class WoollyMammoth : Elephantidae
            {
                public float FurUndercoatThicknessCm { get; } = 8f;

                public WoollyMammoth(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Woolly Mammoth (Mammuthus primigenius)")
                {
                    TuskLengthCm = 420f;
                    MaxLifespanYears = 60f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string PlowSnowWithCurvedTusks() => $"{Name} sweeps massive 4.2-meter spiral ivory tusks like snowplows across frozen tundra steppe to graze subnivean steppe grasses!";
            }
        }
    }

    // =========================================================================
    // --- 10. CETACEA (MARINE MAMMALS: BLUE WHALE, HUMPBACK, SPERM WHALE, ORCA)
    // =========================================================================

    public class Cetacean : Creature
    {
        public string? Name { get; set; }
        public float BlubberThicknessCm { get; protected set; } = 25f;
        public bool HasBlowhole { get; } = true;
        public override float SpeciesGestationDays => 365f;

        public Cetacean(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 0)
            : base(father, mother, hands: 0, handFingers: 0, legs: 0, legFingers: 0, teeths: teeths, sex: sex)
        {
            Name = name;
            MaxLifespanYears = 80f + _rng.Next(-10, 20);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 400f };

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(2, MammaryGland.CupSize.F, 6000f);
            }
        }

        // 1. Mysticeti (Baleen Whales - Filter Feeders)
        public abstract class Mysticeti : Cetacean
        {
            public float BaleenPlateCount { get; } = 300f;

            public Mysticeti(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, sex, name, teeths: 0) { }

            public class Balaenopteridae : Mysticeti
            {
                public Balaenopteridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class BlueWhale : Balaenopteridae
                {
                    public float WeightTonnes { get; } = 190f;

                    public BlueWhale(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Blue Whale (Balaenoptera musculus)")
                    {
                        MaxLifespanYears = 90f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string FilterFeedKrill(float tonnesKrill = 4.0f) => $"{Name} engulfs 100,000L of seawater in expanding ventral pleats, filtering {tonnesKrill:F1} tonnes of Antarctic krill through keratin baleen plates!";
                    public string SpoutBlowhole() => $"{Name} expels pressurized warm vapor 10 meters into the atmosphere from dual blowholes!";
                }

                public class HumpbackWhale : Balaenopteridae
                {
                    public HumpbackWhale(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Humpback Whale (Megaptera novaeangliae)")
                    {
                        MaxLifespanYears = 50f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string BubbleNetCorral() => $"{Name} spirals upward blowing a shimmering cylindrical bubble-net wall, trapping schooling herring!";
                    public string SingComplexWhaleSong() => $"{Name} broadcasts repeating 20-minute rhythmic vocal whale song sequences heard across entire ocean basins!";
                }
            }

            public class Balaenidae : Mysticeti
            {
                public Balaenidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class BowheadWhale : Balaenidae
                {
                    public BowheadWhale(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Bowhead Whale (Balaena mysticetus)")
                    {
                        MaxLifespanYears = 200f; // Longest-lived mammal on Earth
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SmashThroughArcticSeaIce(float iceCm = 60f) => $"{Name} rams reinforced triangular cranium through {iceCm:F0} cm thick polar pack ice to breathe!";
                }
            }
        }

        // 2. Odontoceti (Toothed Whales & Dolphins - Echolocation Predators)
        public abstract class Odontoceti : Cetacean
        {
            public bool UsesEcholocation { get; } = true;

            public Odontoceti(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 48)
                : base(father, mother, sex, name, teeths: teeths) { }

            public class Delphinidae : Odontoceti
            {
                public Delphinidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 48)
                    : base(father, mother, sex, name, teeths: teeths) { }

                public class Orca : Delphinidae
                {
                    public Orca(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Killer Whale Orca (Orcinus orca)", teeths: 48)
                    {
                        MaxLifespanYears = 60f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string EmitEcholocationClicks() => $"{Name} focuses high-frequency acoustic clicks through lipid melon organ, mapping underwater topography with bio-sonar!";

                    public string PodCoordinateHunt(Creature target)
                    {
                        if (!IsAlive || !target.IsAlive) return "...";
                        target.TakeDamage(75f, DamageType.Blunt, "Orca wave-wash pack strike");
                        return $"{Name} coordinates pack maneuver, creating wave-wash breach to dislodge {target.GetType().Name}!";
                    }
                }

                public class BottlenoseDolphin : Delphinidae
                {
                    public BottlenoseDolphin(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Common Bottlenose Dolphin (Tursiops truncatus)", teeths: 88)
                    {
                        MaxLifespanYears = 45f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string BroadcastSignatureWhistle() => $"{Name} emits individualized frequency-modulated signature whistle conveying unique identity to pod!";
                    public string BowRideWaves() => $"{Name} rides hydrodynamic bow waves of oceanic vessels with effortless kinetic efficiency!";
                }
            }

            public class Physeteridae : Odontoceti
            {
                public Physeteridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 50)
                    : base(father, mother, sex, name, teeths: teeths) { }

                public class SpermWhale : Physeteridae
                {
                    public SpermWhale(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Sperm Whale (Physeter macrocephalus)", teeths: 50)
                    {
                        MaxLifespanYears = 70f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DeepAbyssalSquidHunt(float depthMeters = 2500f) => $"{Name} dives to {depthMeters:F0}m into perpetual blackness, using 236 dB acoustic clicks to stun and hunt giant architeuthis squid!";
                }
            }

            public class Monodontidae : Odontoceti
            {
                public Monodontidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 0)
                    : base(father, mother, sex, name, teeths: teeths) { }

                public class Narwhal : Monodontidae
                {
                    public float HelicalTuskLengthMeters { get; } = 2.7f;

                    public Narwhal(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Narwhal (Monodon monoceros)", teeths: 2)
                    {
                        MaxLifespanYears = 50f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ProbeSeaSalinityWithTusk() => $"{Name} senses Arctic seawater salinity gradients and barometric changes with 2.7m spiraled tusk housing 10 million sensory pathways!";
                }

                public class BelugaWhale : Monodontidae
                {
                    public BelugaWhale(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Beluga Sea Canary (Delphinapterus leucas)", teeths: 36)
                    {
                        MaxLifespanYears = 40f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ModulateMelonAcoustics() => $"{Name} visibly deforms flexible lipid melon to modulate bird-like chirps, whistles, and clicks!";
                }
            }
        }
    }

    // =========================================================================
    // --- 11. UNGULATA (HOOFED MAMMALS: ARTIODACTYLA & PERISSODACTYLA)
    // =========================================================================

    public class Ungulate : Creature
    {
        public string? Name { get; set; }
        public bool IsRuminant { get; protected set; } = false;
        public int StomachChambersCount { get; protected set; } = 1;
        public float RunningSpeedKmh { get; protected set; } = 60f;
        public override float SpeciesGestationDays => 280f;
        public override float TeethingAgeYears => 0.1f;
        public override float AdultTeethAgeYears => 2.5f;

        public Ungulate(Creature? father = null, Creature? mother = null, int fingers = 2, int teeths = 32, SexType? sex = null, string? name = null)
            : base(father, mother, hands: 2, handFingers: fingers, legs: 2, legFingers: fingers, teeths: teeths, sex: sex)
        {
            Name = name;
            MaxLifespanYears = 25f + _rng.Next(-3, 8);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 45f };

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(4, MammaryGland.CupSize.D, 1800f);
            }
        }

        // --- ORDER 1: ARTIODACTYLA (EVEN-TOED UNGULATES - CLOVEN HOOVES) ---
        public abstract class Artiodactyla : Ungulate
        {
            public Artiodactyla(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 32)
                : base(father, mother, fingers: 2, teeths: teeths, sex: sex, name: name)
            {
                IsRuminant = true;
                StomachChambersCount = 4; // Rumen, reticulum, omasum, abomasum
            }

            public class Bovidae : Artiodactyla // Cattle, Bison, Buffalo, Antelopes
            {
                public Bovidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class AmericanBison : Bovidae
                {
                    public float ShoulderHumpMuscleKg { get; } = 40f;

                    public AmericanBison(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "American Plains Bison (Bison bison)")
                    {
                        MaxLifespanYears = 22f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string PlowSnowWithHead() => $"{Name} swings massive low-slung horned head side-to-side through 1-meter snowdrifts to uncover forage!";
                    public string StampedeCharge(Creature enemy)
                    {
                        if (!IsAlive || !enemy.IsAlive) return "...";
                        enemy.TakeDamage(65f, DamageType.Blunt, "Bison skull ram");
                        return $"{Name} charges at 55 km/h, ramming thick reinforced frontal bone into {enemy.GetType().Name}!";
                    }
                }

                public class AfricanBuffalo : Bovidae
                {
                    public AfricanBuffalo(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "African Cape Buffalo (Syncerus caffer)")
                    {
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string FormDefensiveHornCircle() => $"{Name} rallies the herd into an unbroken wall of fused keratin horn bosses facing approaching predators!";
                }

                public class AlpineIbex : Bovidae
                {
                    public AlpineIbex(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Alpine Ibex (Capra ibex)")
                    {
                        MaxLifespanYears = 19f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ScaleNearVerticalDamWall() => $"{Name} balances split rubbery hooves on 2-centimeter granite wall projections of sheer 80° dam face to lick mineral salts!";
                }
            }

            public class Cervidae : Artiodactyla // Deer & Moose
            {
                public bool ShedsAntlersAnnually { get; } = true;

                public Cervidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class Moose : Cervidae
                {
                    public float AntlerSpanCm { get; } = 180f;

                    public Moose(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Eurasian / Alaskan Moose (Alces alces)")
                    {
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DiveForAquaticPlants(float depthMeters = 5.5f) => $"{Name} closes fleshy nostrils and dives {depthMeters:F1} meters underwater to graze on sodium-rich water lily rhizomes!";
                }

                public class Reindeer : Cervidae
                {
                    public Reindeer(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Tundra Caribou Reindeer (Rangifer tarandus)")
                    {
                        MaxLifespanYears = 18f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string PerceiveUltravioletLight() => $"{Name}'s tapetum lucidum shifts from golden summer to deep blue winter, detecting UV reflection of predator urine on snow!";
                    public string ClickTendonLocomotion() => $"{Name} flexes sesamoid foot tendons, producing audible clicks that keep herd coordinated in whiteout blizzards!";
                }
            }

            public class Giraffidae : Artiodactyla // Giraffes & Okapis
            {
                public Giraffidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class Giraffe : Giraffidae
                {
                    public float NeckLengthMeters { get; } = 2.4f;

                    public Giraffe(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Southern Giraffe (Giraffa camelopardalis)")
                    {
                        MaxLifespanYears = 28f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Heart.SystolicPressure = 280f; // Extreme arterial pressure to pump blood 2m up to brain
                    }

                    public string BrowseAcaciaThorns() => $"{Name} curls 45 cm melanin-pigmented prehensile tongue around thorny Acacia tortilis branches, protected by thick viscous saliva!";
                    public string NeckWhipFight(Creature rival)
                    {
                        if (!IsAlive || !rival.IsAlive) return "...";
                        rival.TakeDamage(55f, DamageType.Blunt, "Ossicone neck swing");
                        return $"{Name} swings 250 kg muscular neck with kinetic momentum, slamming blunt ossicones into {rival.GetType().Name}!";
                    }
                }

                public class Okapi : Giraffidae
                {
                    public Okapi(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Forest Okapi (Okapia johnstoni)")
                    {
                        MaxLifespanYears = 26f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string InfrasonicForestWhisper() => $"{Name} transmits 14 Hz silent infrasonic vocalizations through dense Ituri rainforest canopy!";
                }
            }

            public class Hippopotamidae : Artiodactyla // Hippopotamuses
            {
                public Hippopotamidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name, teeths: 36)
                {
                    IsRuminant = false; // Pseudoruminant with 3 chambers
                    StomachChambersCount = 3;
                }

                public class Hippopotamus : Hippopotamidae
                {
                    public float OralGapeDegrees { get; } = 150f;
                    public float BiteForceNewtons { get; } = 12000f;

                    public Hippopotamus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Common River Hippopotamus (Hippopotamus amphibius)")
                    {
                        MaxLifespanYears = 45f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ExudeBloodSweatSunscreen() => $"{Name} secretes hipposudoric and norhipposudoric acid droplets, forming a pinkish antibacterial natural sunscreen shield!";

                    public string CrushingTuskChomp(Creature target)
                    {
                        if (!IsAlive || !target.IsAlive) return "...";
                        target.TakeDamage(80f, DamageType.Piercing, "Hippo canine tusk bite");
                        return $"{Name} gapes jaws 150° and clamps down with 12,000 N bite force, driving 50 cm lower canine tusks into {target.GetType().Name}!";
                    }
                }
            }

            public class Camelidae : Artiodactyla // Camels, Llamas, Alpacas
            {
                public Camelidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name, teeths: 34)
                {
                    StomachChambersCount = 3;
                }

                public class DromedaryCamel : Camelidae
                {
                    public float AdiposeHumpFatKg { get; } = 35f;

                    public DromedaryCamel(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Dromedary Camel (Camelus dromedarius)")
                    {
                        MaxLifespanYears = 40f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string TolerateExtremeDehydration() => $"{Name} maintains circulation with unique oval erythrocytes, enduring 30% body water loss in 48°C desert heat without hyperthermia!";
                    public string DrinkRapidly(float liters = 100f) => $"{Name} drinks {liters:F0}L of water in 10 minutes, swelling oval red blood cells to 240% volume safely!";
                }
            }

            public class Suidae : Artiodactyla // Pigs & Boars
            {
                public Suidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name, teeths: 44)
                {
                    IsRuminant = false;
                    StomachChambersCount = 1;
                }

                public class WildBoar : Suidae
                {
                    public WildBoar(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Eurasian Wild Boar (Sus scrofa)")
                    {
                        MaxLifespanYears = 14f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string RootSoilWithSnoutDisc() => $"{Name} drives cartilaginous snout disc into compacted forest loam, excavating truffles and tubers!";
                    public string SlashTusks(Creature target)
                    {
                        if (!IsAlive || !target.IsAlive) return "...";
                        target.TakeDamage(40f, DamageType.Slashing, "Boar whetted tusk upward slash");
                        return $"{Name} whips head upward, slashing razor-sharp continuously self-whetting lower canine tusks into {target.GetType().Name}!";
                    }
                }

                public class Babirusa : Suidae
                {
                    public Babirusa(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "North Sulawesi Babirusa (Babyrousa celebensis)")
                    {
                        MaxLifespanYears = 18f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DisplayCurvedSnoutTusks() => $"{Name} displays bizarre upper canines that penetrate dorsal snout skin and curve backward toward forehead!";
                }
            }
        }

        // --- ORDER 2: PERISSODACTYLA (ODD-TOED UNGULATES - MESAXONIC HOOVES) ---
        public abstract class Perissodactyla : Ungulate
        {
            public Perissodactyla(Creature? father = null, Creature? mother = null, int fingers = 1, SexType? sex = null, string? name = null, int teeths = 40)
                : base(father, mother, fingers: fingers, teeths: teeths, sex: sex, name: name)
            {
                IsRuminant = false; // Hindgut fermenters (large enlarged caecum and colon)
                StomachChambersCount = 1;
            }

            public class Equidae : Perissodactyla // Horses, Zebras, Donkeys
            {
                public Equidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, fingers: 1, sex: sex, name: name, teeths: 40) { }

                public class PlainsZebra : Equidae
                {
                    public PlainsZebra(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Plains Zebra (Equus quagga)")
                    {
                        MaxLifespanYears = 25f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DisruptPredatorDazzle() => $"{Name} runs in synchronized tight herd formations, creating optical motion dazzle that confuses lion depth perception!";
                    public string PowerfulHindKick(Creature predator)
                    {
                        if (!IsAlive || !predator.IsAlive) return "...";
                        predator.TakeDamage(60f, DamageType.Blunt, "Equine dual hind-hoof kick");
                        return $"{Name} lashes out with dual solid keratin hooves with 9,000 N kinetic force, breaking {predator.GetType().Name}'s jaw!";
                    }
                }

                public class PrzewalskisHorse : Equidae
                {
                    public int ChromosomePairs { get; } = 33; // 66 chromosomes (vs 64 in domestic horses)

                    public PrzewalskisHorse(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Przewalski's Wild Horse (Equus ferus przewalskii)")
                    {
                        MaxLifespanYears = 28f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string EndureSteppeWinter() => $"{Name} shelters herd stallions against -40°C Mongolian Gobi blizzards, surviving on coarse desert scrub!";
                }
            }

            public class Rhinocerotidae : Perissodactyla // Rhinoceroses
            {
                public Rhinocerotidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, fingers: 3, sex: sex, name: name, teeths: 30) { }

                public class WhiteRhino : Rhinocerotidae
                {
                    public float AnteriorHornLengthCm { get; } = 90f;

                    public WhiteRhino(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Southern White Rhino (Ceratotherium simum)")
                    {
                        MaxLifespanYears = 45f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string WideMouthGraze() => $"{Name} mows short savanna turf efficiently with wide square-lipped keratinized mouth plates!";
                    public string HornChargeRam(Creature threat)
                    {
                        if (!IsAlive || !threat.IsAlive) return "...";
                        threat.TakeDamage(75f, DamageType.Piercing, "Solid keratin horn charge");
                        return $"{Name} charges at 40 km/h with 2.3 tonnes of inertia, driving front keratin horn into {threat.GetType().Name}!";
                    }
                }

                public class BlackRhino : Rhinocerotidae
                {
                    public BlackRhino(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Hook-Lipped Black Rhino (Diceros bicornis)")
                    {
                        MaxLifespanYears = 40f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string BrowseWithPrehensileLip() => $"{Name} strips leaves and twigs from dense thorny shrubs using pointed prehensile triangular upper lip!";
                }
            }

            public class Tapiridae : Perissodactyla // Tapirs
            {
                public Tapiridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, fingers: 4, sex: sex, name: name, teeths: 42) { }

                public class MalayanTapir : Tapiridae
                {
                    public MalayanTapir(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Malayan Tapir (Tapirus indicus)")
                    {
                        MaxLifespanYears = 30f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SubmergeProboscisSnorkel() => $"{Name} sinks into deep river pools, extending flexible muscular proboscis above water line like a snorkel!";
                }
            }
        }
    }

    // =========================================================================
    // --- 12. CARNIVORA (MUSTELIDS, HYENAS, PINNIPEDS, RACCOONS, SKUNKS)
    // =========================================================================

    public class Carnivoran : Creature
    {
        public string? Name { get; set; }
        public bool HasCarnassialTeethPair { get; } = true;
        public float BiteForceQuotient { get; protected set; } = 120f;
        public override float SpeciesGestationDays => 75f;
        public override float TeethingAgeYears => 0.08f;
        public override float AdultTeethAgeYears => 1.2f;

        public Carnivoran(Creature? father = null, Creature? mother = null, int fingers = 5, int teeths = 34, SexType? sex = null, string? name = null)
            : base(father, mother, hands: 2, handFingers: fingers, legs: 2, legFingers: fingers, teeths: teeths, sex: sex)
        {
            Name = name;
            MaxLifespanYears = 16f + _rng.Next(-2, 6);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 35f };

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(6, MammaryGland.CupSize.B, 200f);
            }
        }

        // FAMILY 1: MUSTELIDAE (WEASELS, BADGERS, OTTERS, WOLVERINES)
        public abstract class Mustelidae : Carnivoran
        {
            public Mustelidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 34)
                : base(father, mother, fingers: 5, teeths: teeths, sex: sex, name: name) { }

            public class HoneyBadger : Mustelidae
            {
                public HoneyBadger(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Honey Badger Ratel (Mellivora capensis)")
                {
                    MaxLifespanYears = 24f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string ShrugOffSnakeVenom() => $"{Name} absorbs neurotoxic puff adder bite; nicotinic acetylcholine receptors mutate resistance, waking uninjured from short venom coma!";
                public string TearBeehiveWithClaws() => $"{Name} tears into African bee nest with 4 cm fossorial claws, protected by 6mm thick rubbery subcutaneous skin shield!";
            }

            public class SeaOtter : Mustelidae
            {
                public float HairDensityPerSqCm { get; } = 150000f; // Densest fur in animal kingdom

                public SeaOtter(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Northern Sea Otter (Enhydra lutris)")
                {
                    MaxLifespanYears = 20f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string CrackClamWithAnvilStone() => $"{Name} balances flat granite anvil stone on chest while floating on back, hammering abalone and clams open!";
                public string WrapInGiantKelp() => $"{Name} drapes giant Macrocystis kelp fronds across body to prevent drifting out to sea during sleep!";
            }

            public class Wolverine : Mustelidae
            {
                public Wolverine(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Wolverine Gulo (Gulo gulo)")
                {
                    MaxLifespanYears = 13f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string ShearFrozenCarcass() => $"{Name} uses backwards-rotated upper molars to crush frozen bone marrow in sub-zero taiga blizzard!";
            }

            public class GiantRiverOtter : Mustelidae
            {
                public GiantRiverOtter(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Giant Amazonian River Otter (Pteronura brasiliensis)")
                {
                    MaxLifespanYears = 15f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string CoordinatePackHuntCaiman(Creature caiman)
                {
                    if (!IsAlive || !caiman.IsAlive) return "...";
                    caiman.TakeDamage(55f, DamageType.Slashing, "Otter pack coordinated bite");
                    return $"{Name} vocalizes with 22 distinct pack calls, teaming up to overpower {caiman.GetType().Name} in Amazonian oxbow lake!";
                }
            }
        }

        // FAMILY 2: HYAENIDAE (HYENAS - EXTREME BONE-CRUSHING MASTICATION)
        public abstract class Hyaenidae : Carnivoran
        {
            public Hyaenidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, fingers: 4, teeths: 34, sex: sex, name: name) { }

            public class SpottedHyena : Hyaenidae
            {
                public float BitePressurePounds { get; } = 1100f; // 4,500 N bone crushing

                public SpottedHyena(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Spotted Hyena (Crocuta crocuta)")
                {
                    MaxLifespanYears = 25f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string PulverizeBoneShafts() => $"{Name} locks massive third premolars with 4,500 N force, pulverizing giraffe and zebra femur bones to digest marrow and calcium phosphate!";
                public string WhoopAndLaughChorus() => $"{Name} emits eerie rising whoops and cackling giggles that echo across 5 km of savanna, rallying clan members!";
            }
        }

        // FAMILY 3: MEPHITIDAE (SKUNKS - THIOL AEROSOL CHEMICAL SPRAY)
        public abstract class Mephitidae : Carnivoran
        {
            public Mephitidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, fingers: 5, teeths: 34, sex: sex, name: name) { }

            public class StripedSkunk : Mephitidae
            {
                public StripedSkunk(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Striped Skunk (Mephitis mephitis)")
                {
                    MaxLifespanYears = 7f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string SprayThiolDefense(Creature threat)
                {
                    if (!IsAlive || !threat.IsAlive) return "...";
                    threat.TakeDamage(15f, DamageType.Poison, "Thiol and thioacetate spray");
                    return $"{Name} stamps forefeet, arches tail, and discharges dual anal papillae with pressurized butyl mercaptan spray blinding {threat.GetType().Name} from 6 meters!";
                }
            }
        }

        // FAMILY 4: PROCYONIDAE (RACCOONS - DEXTEROUS TACTILE HYPERSENSITIVITY)
        public abstract class Procyonidae : Carnivoran
        {
            public Procyonidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, fingers: 5, teeths: 40, sex: sex, name: name) { }

            public class CommonRaccoon : Procyonidae
            {
                public CommonRaccoon(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "North American Raccoon (Procyon lotor)")
                {
                    MaxLifespanYears = 12f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string UnderwaterTactileInspection() => $"{Name} softens tactile mechanoreceptors in water, dexterously manipulating complex multi-step puzzle latches!";
            }
        }

        // SUBORDER: PINNIPEDIA (SEALS, SEA LIONS, WALRUSES - MARINE CARNIVORES)
        public abstract class Pinnipedia : Carnivoran
        {
            public Pinnipedia(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 34)
                : base(father, mother, fingers: 5, teeths: teeths, sex: sex, name: name)
            {
                Body.Tail = new Tail { LengthCm = 15f };
            }

            public class Odobenidae : Pinnipedia // Walruses
            {
                public Odobenidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name, teeths: 18) { }

                public class Walrus : Odobenidae
                {
                    public float TuskLengthCm { get; } = 95f;

                    public Walrus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Pacific Walrus (Odobenus rosmarus divergens)")
                    {
                        MaxLifespanYears = 40f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string VacuumBenthicClams() => $"{Name} roots through muddy sea floor with 400 stiff mystacial vibrissae, using piston-like tongue suction to suck 6,000 clams from shells daily!";
                    public string HaulOutOnSeaIce() => $"{Name} anchors 95 cm ivory canine tusks into ice edge to haul massive 1.5-tonne body onto pack ice!";
                }
            }

            public class Phocidae : Pinnipedia // True Seals (Earless Seals)
            {
                public Phocidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name, teeths: 30) { }

                public class SouthernElephantSeal : Phocidae
                {
                    public float BullWeightTonnes { get; } = 4.0f;

                    public SouthernElephantSeal(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Southern Elephant Seal (Mirounga leonina)")
                    {
                        MaxLifespanYears = 22f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DeepPelagicDive(float depthMeters = 2100f) => $"{Name} dives to {depthMeters:F0}m for 120 minutes on single breath, surviving on massive blood volume and myoglobin-packed muscles!";
                    public string InflateProboscisRoar() => $"{Name} inflates inflatable fleshy trunk proboscis, bellowing resonant guttural challenges across Subantarctic breeding beaches!";
                }

                public class LeopardSeal : Phocidae
                {
                    public LeopardSeal(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Antarctic Leopard Seal (Hydrurga leptonyx)")
                    {
                        MaxLifespanYears = 26f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string AmbushPenguinFromIceFloe(Creature prey)
                    {
                        if (!IsAlive || !prey.IsAlive) return "...";
                        prey.TakeDamage(65f, DamageType.Slashing, "Leopard seal canine and tricuspid molar strike");
                        return $"{Name} explodes from beneath anchor ice floe at 40 km/h, snapping massive tricuspid interlocking teeth onto {prey.GetType().Name}!";
                    }
                }
            }

            public class Otariidae : Pinnipedia // Eared Seals (Sea Lions & Fur Seals)
            {
                public Otariidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name, teeths: 34) { }

                public class CaliforniaSeaLion : Otariidae
                {
                    public CaliforniaSeaLion(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "California Sea Lion (Zalophus californianus)")
                    {
                        MaxLifespanYears = 25f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string PorpoiseSurfWaves() => $"{Name} rotates hind flippers forward to gallop on land, and porpoises rhythmically through coastal Pacific surf breaks!";
                }
            }
        }
    }

    // =========================================================================
    // --- 13. MARSUPIALIA (POUCHED MAMMALS: KANGAROOS, KOALAS, WOMBATS, DEVILS)
    // =========================================================================

    public class Marsupial : Creature
    {
        public string? Name { get; set; }
        public bool HasMarsupiumPouch { get; protected set; } = true;
        public bool IsJoeyInPouch { get; protected set; } = false;
        public override float SpeciesGestationDays => 35f; // Extremely short embryonic gestation
        public override float TeethingAgeYears => 0.05f;
        public override float AdultTeethAgeYears => 1.0f;

        public Marsupial(Creature? father = null, Creature? mother = null, int teeths = 34, SexType? sex = null, string? name = null)
            : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 5, teeths: teeths, sex: sex)
        {
            Name = name;
            MaxLifespanYears = 16f + _rng.Next(-2, 6);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 50f };

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(4, MammaryGland.CupSize.A, 120f);
            }
        }

        public virtual string JoeyNursedInPouch()
        {
            if (Sex != SexType.Female) return $"{Name} lacks a maternal marsupium pouch.";
            IsJoeyInPouch = true;
            return $"{Name} nurses blind embryonic altricial joey anchored firmly to teat inside warm protective marsupium pouch!";
        }

        public class Macropodidae : Marsupial // Kangaroos & Wallabies
        {
            public Macropodidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, teeths: 32, sex: sex, name: name) { }

            public class RedKangaroo : Macropodidae
            {
                public float MaxHopSpeedKmh { get; } = 65f;

                public RedKangaroo(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Red Kangaroo (Osphranter rufus)")
                {
                    MaxLifespanYears = 22f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    Body.Tail = new Tail { LengthCm = 100f };
                }

                public string PentapedalLocomotion() => $"{Name} uses heavy muscular tail as a fifth leg when grazing at slow speeds, pushing body forward!";

                public string HighSpeedHop(float distanceKm = 10f) => $"{Name} bounces across arid outback at {MaxHopSpeedKmh:F0} km/h, storing kinetic energy in elastic Achilles tendons!";

                public string KickboxStrike(Creature rival)
                {
                    if (!IsAlive || !rival.IsAlive) return "...";
                    rival.TakeDamage(55f, DamageType.Blunt, "Kangaroo dual rear-leg kickbox");
                    return $"{Name} leans back on robust tail and dispatches a dual-leg kickbox strike with dagger-like 4th toe claws into {rival.GetType().Name}!";
                }
            }

            public class TreeKangaroo : Macropodidae
            {
                public TreeKangaroo(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Lumholtz's Tree-Kangaroo (Dendrolagus lumholtzi)")
                {
                    MaxLifespanYears = 18f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string ScaleRainforestCanopy() => $"{Name} grips wet mossy rainforest branches with broad curved claws and rough friction-padded soles!";
            }
        }

        public class Phascolarctidae : Marsupial // Koalas
        {
            public Phascolarctidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, teeths: 30, sex: sex, name: name) { }

            public class Koala : Phascolarctidae
            {
                public float DailySleepHours { get; } = 20f;

                public Koala(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Koala (Phascolarctos cinereus)")
                {
                    MaxLifespanYears = 15f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string DetoxifyEucalyptusLeaves() => $"{Name} breaks down toxic eucalyptus cyanogenic terpenes and phenols using 200 cm enlarged caecum microflora!";
                public string GripTreeFork() => $"{Name} locks two opposable thumbs and three fingers with vice-like grip around smooth gum tree trunks!";
            }
        }

        public class Vombatidae : Marsupial // Wombats
        {
            public Vombatidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, teeths: 24, sex: sex, name: name) { }

            public class BareNosedWombat : Vombatidae
            {
                public BareNosedWombat(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Bare-Nosed Common Wombat (Vombatus ursinus)")
                {
                    MaxLifespanYears = 18f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    Body.Tail = new Tail { LengthCm = 3f };
                }

                public string ProduceCubicFeces() => $"{Name} shapes distinctive cubic feces through variable elasticity of horizontal intestinal ridges, marking territory on rocks!";
                public string CrushPredatorOnBurrowRoof(Creature predator)
                {
                    if (!IsAlive || !predator.IsAlive) return "...";
                    predator.TakeDamage(50f, DamageType.Blunt, "Reinforced cartilaginous rump plate crush");
                    return $"{Name} dives headfirst into burrow and uses reinforced dermal bone plate on rump to crush {predator.GetType().Name}'s skull against burrow ceiling!";
                }
            }
        }

        public class Dasyuridae : Marsupial // Carnivorous Marsupials
        {
            public Dasyuridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, teeths: 42, sex: sex, name: name) { }

            public class TasmanianDevil : Dasyuridae
            {
                public float BiteForcePerKg { get; } = 35f; // Highest relative bite force of any mammal

                public TasmanianDevil(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Tasmanian Devil (Sarcophilus harrisii)")
                {
                    MaxLifespanYears = 8f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string BoneCrushingFeast(Creature carcass)
                {
                    if (!IsAlive) return "...";
                    return $"{Name} opens jaw 80° and delivers bone-crushing 553 N bite force, devouring meat, skin, and skull of carcass!";
                }

                public string ScreechBloodcurdlingGrowl() => $"{Name} unhinges jaw, flushes ears red with blood, and emits a terrifying guttural demonic screech across the temperate forest!";
            }
        }

        public class Didelphidae : Marsupial // American Opossums
        {
            public Didelphidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, teeths: 50, sex: sex, name: name) // Most teeth of any North American land mammal
            {
                Body.Tail = new Tail { LengthCm = 30f };
            }

            public class VirginiaOpossum : Didelphidae
            {
                public bool IsThanatosisActive { get; private set; } = false;

                public VirginiaOpossum(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Virginia Opossum (Didelphis virginiana)")
                {
                    MaxLifespanYears = 4f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string FeignDeathThanatosis()
                {
                    IsThanatosisActive = true;
                    return $"{Name} enters involuntary catatonic state (playing possum): body stiffens, tongue protrudes, and foul cadaveric fluid is excreted!";
                }

                public string DevourTicks(int tickCount = 500) => $"{Name} fastidiously grooms coat and consumes {tickCount} parasitic deer ticks, suppressing Lyme disease transmission!";
            }
        }
    }

    // =========================================================================
    // --- 14. MONOTREMATA (EGG-LAYING PROTOTHERIAN MAMMALS: PLATYPUS & ECHIDNA)
    // =========================================================================

    public class Monotreme : Creature
    {
        public string? Name { get; set; }
        public bool HasCloaca { get; } = true;
        public bool LaysLeatheryEggs { get; } = true;
        public bool HasElectroreceptiveSnout { get; protected set; } = true;
        public override float SpeciesGestationDays => 12f; // Short internal gestation before egg laying
        public override float TeethingAgeYears => 0.05f;
        public override float AdultTeethAgeYears => 0.2f;

        public Monotreme(Creature? father = null, Creature? mother = null, int teeths = 0, SexType? sex = null, string? name = null)
            : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 5, teeths: teeths, sex: sex)
        {
            Name = name;
            IsEndothermic = true;
            BodyTemperatureCelsius = 32f; // Low primitive mammalian body temperature
            MaxLifespanYears = 20f + _rng.Next(-2, 10);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 15f };

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(2, MammaryGland.CupSize.A, 50f);
            }
        }

        public virtual string ExudeMilkPatch()
        {
            if (Sex != SexType.Female) return $"{Name} does not lactate.";
            return $"{Name} exudes nutrient-dense antibacterial milk directly onto specialised abdominal skin patches (sweating milk for puggle young to lap)!";
        }

        public class Ornithorhynchidae : Monotreme // Platypus
        {
            public Ornithorhynchidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, teeths: 0, sex: sex, name: name) { }

            public class DuckBilledPlatypus : Ornithorhynchidae
            {
                public bool HasCruralVenomSpurs { get; } = true;

                public DuckBilledPlatypus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Duck-Billed Platypus (Ornithorhynchus anatinus)")
                {
                    MaxLifespanYears = 17f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    Body.Tail = new Tail { LengthCm = 14f }; // Beaver-like fat storage tail
                }

                public string ElectroreceptiveForage() => $"{Name} closes eyes, ears, and nostrils underwater, sweeping rubbery bill with 40,000 electroreceptors to detect bioelectric muscle twitches of freshwater shrimp!";

                public string DeliverCruralSpurVenom(Creature target)
                {
                    if (Sex != SexType.Male) return $"{Name} is female and lacks calcaneus venom spurs.";
                    if (!IsAlive || !target.IsAlive) return "...";
                    target.TakeDamage(45f, DamageType.Poison, "Platypus crural defensin-like peptides");
                    return $"{Name} jams calcaneus tarsal hind-leg spurs into {target.GetType().Name}, injecting debilitating hyperalgesic venom!";
                }
            }
        }

        public class Tachyglossidae : Monotreme // Echidnas
        {
            public Tachyglossidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, teeths: 0, sex: sex, name: name) { }

            public class ShortBeakedEchidna : Tachyglossidae
            {
                public ShortBeakedEchidna(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Short-Beaked Echidna (Tachyglossus aculeatus)")
                {
                    MaxLifespanYears = 40f; // Incredible longevity for small mammal
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string RollIntoSpinyBall() => $"{Name} burrows vertically into soil in seconds, leaving only an impenetrable dome of sharp keratin spines exposed!";
                public string SlurpTermitesWithTongue() => $"{Name} flicks 18 cm sticky vermiform tongue 100 times/minute into termite galleries, grinding prey against horny palate ridges!";
            }
        }
    }

    // =========================================================================
    // --- 15. GLIRES: RODENTIA & LAGOMORPHA (CONTINUOUSLY GROWING INCISORS)
    // =========================================================================

    public class RodentAndLagomorph : Creature
    {
        public string? Name { get; set; }
        public bool HasElodontGrowingIncisors { get; } = true;
        public float CecumVolumeCm3 { get; protected set; } = 150f;
        public override float SpeciesGestationDays => 30f;
        public override float TeethingAgeYears => 0.02f;
        public override float AdultTeethAgeYears => 0.3f;

        public RodentAndLagomorph(Creature? father = null, Creature? mother = null, int teeths = 20, SexType? sex = null, string? name = null)
            : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 5, teeths: teeths, sex: sex)
        {
            Name = name;
            MaxLifespanYears = 8f + _rng.Next(-1, 5);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 20f };

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(6, MammaryGland.CupSize.A, 60f);
            }
        }

        // --- ORDER 1: RODENTIA (SINGLE PAIR OF CONTINUOUSLY GROWING UPPER INCISORS) ---
        public abstract class Rodentia : RodentAndLagomorph
        {
            public Rodentia(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 16)
                : base(father, mother, teeths: teeths, sex: sex, name: name) { }

            public class Castoridae : Rodentia // Beavers
            {
                public Castoridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class NorthAmericanBeaver : Castoridae
                {
                    public NorthAmericanBeaver(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "North American Beaver (Castor canadensis)")
                    {
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 30f }; // Broad scaly paddle tail
                    }

                    public string FellHardwoodTree(string treeSpecies = "Aspen") => $"{Name} gnaws through a 30 cm {treeSpecies} trunk with orange iron-infused enamel incisors, felling it for dam construction!";
                    public string EngineerHydraulicDam() => $"{Name} constructs a 100-meter structural stick-and-mud dam, engineering an entire wetland ecosystem and deep winter lodge!";
                    public string SlapTailAlarm() => $"{Name} slaps flat scaly tail against water surface with a resounding gunshot-like crack, warning family members!";
                }
            }

            public class Hystricidae : Rodentia // Porcupines
            {
                public Hystricidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class CrestedPorcupine : Hystricidae
                {
                    public CrestedPorcupine(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "African Crested Porcupine (Hystrix cristata)")
                    {
                        MaxLifespanYears = 18f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string RattleQuillWarning() => $"{Name} rattles hollow cylindrical tail quills together, creating a dry hissing acoustic warning to leopards!";

                    public string BackwardQuillCharge(Creature predator)
                    {
                        if (!IsAlive || !predator.IsAlive) return "...";
                        predator.TakeDamage(48f, DamageType.Piercing, "Barbed quill impalement");
                        return $"{Name} charges backward, embedding dozens of 35 cm microscopic reverse-barbed keratin quills into {predator.GetType().Name}!";
                    }
                }
            }

            public class Caviidae : Rodentia // Capybaras & Guinea Pigs
            {
                public Caviidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class Capybara : Caviidae
                {
                    public float WeightKg { get; } = 65f; // Largest rodent on Earth

                    public Capybara(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Greater Capybara (Hydrochoerus hydrochaeris)")
                    {
                        MaxLifespanYears = 12f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 2f }; // Vestigial tail
                    }

                    public string InterspeciesPeacefulCoexistence() => $"{Name} lounges serenely by marsh lagoon, allowing birds, turtles, and monkeys to rest comfortably on its broad back!";
                    public string SubmergeSnorkelSwim() => $"{Name} swims semi-aquatically with eyes, ears, and nostrils aligned along the top of head like a hippopotamus!";
                }
            }

            public class Bathyergidae : Rodentia // Mole-rats
            {
                public Bathyergidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class NakedMoleRat : Bathyergidae
                {
                    public NakedMoleRat(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Naked Mole-Rat (Heterocephalus glaber)")
                    {
                        MaxLifespanYears = 32f; // Extraordinary longevity, 10x normal rodent lifespan
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ResistsCancerAndHypoxia() => $"{Name} produces high-molecular-mass hyaluronan preventing oncogenesis, and survives 18 minutes of zero oxygen via fructose glycolysis!";
                    public string DigWithExternalIncisors() => $"{Name} moves extrabuccal protruding incisors independently like chopsticks, tunneling through clay without swallowing dirt!";
                }
            }

            public class Sciuridae : Rodentia // Squirrels
            {
                public Sciuridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name, teeths: 22) { }

                public class EurasianRedSquirrel : Sciuridae
                {
                    public EurasianRedSquirrel(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Eurasian Red Squirrel (Sciurus vulgaris)")
                    {
                        MaxLifespanYears = 7f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ScatterHoardNuts(int count = 40) => $"{Name} caches {count} pine nuts across spatial territory, remembering subterranean hiding sites via hippocampus spatial mapping!";
                }
            }
        }

        // --- ORDER 2: LAGOMORPHA (HARES, RABBITS, PIKAS - DUAL UPPER INCISOR PAIRS) ---
        public abstract class Lagomorpha : RodentAndLagomorph
        {
            public bool HasPegTeethSecondIncisors { get; } = true;

            public Lagomorpha(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, teeths: 28, sex: sex, name: name) { }

            public class Leporidae : Lagomorpha // Hares & Rabbits
            {
                public Leporidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class EuropeanHare : Leporidae
                {
                    public float SprintSpeedKmh { get; } = 72f;

                    public EuropeanHare(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "European Brown Hare (Lepus europaeus)")
                    {
                        MaxLifespanYears = 10f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string ZigZagEscapeSprint() => $"{Name} erupts from shallow depression at {SprintSpeedKmh:F0} km/h, executing 90° zig-zag jinks to outmaneuver predators!";
                    public string CecotrophicReingestion() => $"{Name} re-ingests soft cecotrope pellets directly, extracting essential B-vitamins and bacterial amino acids!";
                }

                public class SnowshoeHare : Leporidae
                {
                    public SnowshoeHare(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Snowshoe Hare (Lepus americanus)")
                    {
                        MaxLifespanYears = 8f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SeasonalCamouflageMolting() => $"{Name} synchronizes fur molt with photoperiod, transforming from brown summer coat to snowy pure white winter camouflage!";
                }
            }
        }
    }

    // =========================================================================
    // --- 16. CHIROPTERA (BATS - TRUE POWERED FLIGHT MAMMALS)
    // =========================================================================

    public class Chiroptera : Creature
    {
        public string? Name { get; set; }
        public float WingspanCm { get; protected set; } = 40f;
        public bool HasEcholocation { get; protected set; } = true;
        public override float SpeciesGestationDays => 60f;
        public override float TeethingAgeYears => 0.04f;
        public override float AdultTeethAgeYears => 0.3f;

        public Chiroptera(Creature? father = null, Creature? mother = null, int teeths = 32, SexType? sex = null, string? name = null)
            : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 5, teeths: teeths, sex: sex)
        {
            Name = name;
            MaxLifespanYears = 25f + _rng.Next(-3, 8); // Long lifespans for small mammals
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 5f };

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(2, MammaryGland.CupSize.A, 30f);
            }
        }

        public virtual string RoostUpsideDown() => $"{Name} hangs upside down by specialized tendon locking mechanisms in curved claws, requiring zero muscular energy!";

        // SUBORDER 1: YINPTEROCHIROPTERA (MEGA-BATS & OLD WORLD FRUIT BATS)
        public abstract class Yinpterochiroptera : Chiroptera
        {
            public Yinpterochiroptera(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 34)
                : base(father, mother, teeths: teeths, sex: sex, name: name)
            {
                HasEcholocation = false; // Navigate primarily by keen eyesight and olfaction
            }

            public class Pteropodidae : Yinpterochiroptera
            {
                public Pteropodidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name) { }

                public class LargeFlyingFox : Pteropodidae
                {
                    public LargeFlyingFox(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Large Flying Fox (Pteropus vampyrus)")
                    {
                        WingspanCm = 150f;
                        MaxLifespanYears = 30f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string FlyOverCanopyPollinating(float kmDistance = 40f) => $"{Name} unfurls 1.5-meter leathery wings, flying {kmDistance:F0} km across rainforest canopy to pollinate durian blossoms!";
                }
            }
        }

        // SUBORDER 2: YANGUCHIROPTERA (MICRO-BATS - LARYNGEAL ULTRASONIC ECHOLOCATION)
        public abstract class Yanguchiroptera : Chiroptera
        {
            public Yanguchiroptera(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 32)
                : base(father, mother, teeths: teeths, sex: sex, name: name)
            {
                HasEcholocation = true;
            }

            public class Phyllostomidae : Yanguchiroptera // Leaf-nosed Bats (including Vampire Bats)
            {
                public Phyllostomidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null, int teeths = 20)
                    : base(father, mother, sex, name, teeths: teeths) { }

                public class CommonVampireBat : Phyllostomidae
                {
                    public CommonVampireBat(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Common Vampire Bat (Desmodus rotundus)", teeths: 20)
                    {
                        MaxLifespanYears = 19f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string HeatSenseCapillaries() => $"{Name} locates warm subcutaneous capillaries using infrared pit organs around nose leaf!";

                    public string LapBloodWithAnticoagulant(Creature host)
                    {
                        if (!IsAlive || !host.IsAlive) return "...";
                        host.TakeDamage(2f, DamageType.Piercing, "Vampire bat painless incisor slice");
                        return $"{Name} makes a microscopic painless incision with razor incisors, lapping blood flowing freely under salivary draculin anticoagulant!";
                    }

                    public string RegurgitateReciprocalMeal(Creature roostMate) => $"{Name} exhibits reciprocal altruism, regurgitating life-saving blood meal for starving roost mate!";
                }
            }

            public class Vespertilionidae : Yanguchiroptera // Evening Bats
            {
                public Vespertilionidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name, teeths: 38) { }

                public class LittleBrownBat : Vespertilionidae
                {
                    public LittleBrownBat(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Little Brown Bat (Myotis lucifugus)")
                    {
                        WingspanCm = 25f;
                        MaxLifespanYears = 34f; // Extreme recorded longevity
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string InterceptMosquitoesBioSonar(int insectsPerHour = 1000) => $"{Name} emits 40-80 kHz frequency-modulated chirps, snatching {insectsPerHour} insects per hour using uropatagium tail scoop!";
                }
            }
        }
    }

    // =========================================================================
    // --- 17. XENARTHRA & PHOLIDOTA (ANTEATERS, SLOTHS, ARMADILLOS, PANGOLINS)
    // =========================================================================

    public class XenarthraAndPholidota : Creature
    {
        public string? Name { get; set; }
        public bool HasExtraVertebralArticulations { get; protected set; } = true;
        public float BasalMetabolicRatePercentOfStandard { get; protected set; } = 45f; // Extremely low metabolic rate
        public override float SpeciesGestationDays => 150f;
        public override float TeethingAgeYears => 0.1f;
        public override float AdultTeethAgeYears => 0.8f;

        public XenarthraAndPholidota(Creature? father = null, Creature? mother = null, int teeths = 0, SexType? sex = null, string? name = null)
            : base(father, mother, hands: 2, handFingers: 5, legs: 2, legFingers: 5, teeths: teeths, sex: sex)
        {
            Name = name;
            MaxLifespanYears = 20f + _rng.Next(-2, 8);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 40f };

            if (Sex == SexType.Female && ReproductiveOrgan is FemaleGenitalia femaleGen)
            {
                femaleGen.Breasts = new MammaryGland(2, MammaryGland.CupSize.A, 100f);
            }
        }

        public class Myrmecophagidae : XenarthraAndPholidota // Anteaters
        {
            public Myrmecophagidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, teeths: 0, sex: sex, name: name) { } // Completely edentulous

            public class GiantAnteater : Myrmecophagidae
            {
                public float TongueLengthCm { get; } = 60f;

                public GiantAnteater(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Giant Anteater (Myrmecophaga tridactyla)")
                {
                    MaxLifespanYears = 25f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    Body.Tail = new Tail { LengthCm = 85f }; // Huge bushy feather-duster tail
                }

                public string RipConcreteTermiteMound() => $"{Name} tears through rock-hard concrete termite mounds with 10 cm recurved third claws!";
                public string FlickTongueRapidly(int insects = 30000) => $"{Name} flicks 60 cm sticky backward-spined tongue 160 times/min, ingesting {insects} ants daily without chewing!";
            }
        }

        public class Bradypodidae : XenarthraAndPholidota // Three-toed Sloths
        {
            public Bradypodidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, teeths: 18, sex: sex, name: name) { }

            public class ThreeToedSloth : Bradypodidae
            {
                public ThreeToedSloth(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Brown-Throated Three-Toed Sloth (Bradypus variegatus)")
                {
                    MaxLifespanYears = 28f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string CultivateAlgalGardenInFur() => $"{Name} hosts Trichophilus green algae in grooved hair shafts, creating mutualistic greenish camouflage that blends into canopy!";
                public string DigestionSlowPace() => $"{Name} takes 30 days to digest a single meal of cecropia leaves in multi-chambered fermentation stomach!";
            }
        }

        public class Dasypodidae : XenarthraAndPholidota // Armadillos
        {
            public Dasypodidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, teeths: 32, sex: sex, name: name) { }

            public class NineBandedArmadillo : Dasypodidae
            {
                public NineBandedArmadillo(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Nine-Banded Armadillo (Dasypus novemcinctus)")
                {
                    MaxLifespanYears = 15f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string ArmorDefenseBuckle() => $"{Name} flexes 9 movable bands of dermal ossified scutes, protecting vulnerable viscera beneath hard bony carapace!";
                public string ObligatePolyembryonyBirth() => $"{Name} gives birth to genetically identical quadruplets from a single fertilized ovum!";
            }
        }

        public class Manidae : XenarthraAndPholidota // Pangolins (Pholidota order)
        {
            public Manidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                : base(father, mother, teeths: 0, sex: sex, name: name) { } // Completely edentulous

            public class SundaPangolin : Manidae
            {
                public SundaPangolin(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, sex, name ?? "Sunda Scaled Pangolin (Manis javanica)")
                {
                    MaxLifespanYears = 20f;
                    if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                }

                public string RollIntoImpenetrableScaledBall() => $"{Name} rolls into a tight spherical ball with razor-sharp overlapping keratin scales, cutting predator paws if manipulated!";
                public string SlurpAntsWithPelvicTongue() => $"{Name} extends specialized sticky tongue anchored deep in pelvic cavity beyond sternum to vacuum ant nests!";
            }
        }
    }

    // =========================================================================
    // --- 18. DINOSAURIA & PREHISTORIC REPTILES (SAURISCHIA, ORNITHISCHIA, PTEROSAURIA, MARINE REPTILES)
    // =========================================================================

    public class Dinosauria : Creature
    {
        public string? Name { get; set; }
        public bool HasPerforatedAcetabulum { get; } = true;
        public bool HasPneumaticBones { get; protected set; } = true;
        public float EstimatedBodyMassTonnes { get; protected set; } = 5.0f;
        public string GeologicPeriod { get; protected set; } = "Mesozoic (Triassic-Jurassic-Cretaceous)";
        public override float SpeciesGestationDays => 75f; // Clutches of hard calcified eggs
        public override float TeethingAgeYears => 0.2f;
        public override float AdultTeethAgeYears => 2.0f;

        public Dinosauria(Creature? father = null, Creature? mother = null, int hands = 2, int legs = 2, int teeths = 60, SexType? sex = null, string? name = null)
            : base(father, mother, hands: hands, handFingers: 3, legs: legs, legFingers: 4, teeths: teeths, sex: sex)
        {
            Name = name;
            IsEndothermic = true; // Mesothermic to fully endothermic
            BodyTemperatureCelsius = 37f;
            MaxLifespanYears = 45f + _rng.Next(-5, 25);
            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
            Body.Tail = new Tail { LengthCm = 350f };
        }

        // =====================================================================
        // --- CLADE 1: SAURISCHIA (LIZARD-HIPPED DINOSAURS)
        // =====================================================================
        public abstract class Saurischia : Dinosauria
        {
            public Saurischia(Creature? father = null, Creature? mother = null, int hands = 2, int legs = 2, int teeths = 60, SexType? sex = null, string? name = null)
                : base(father, mother, hands: hands, legs: legs, teeths: teeths, sex: sex, name: name) { }

            // -----------------------------------------------------------------
            // SUBORDER 1: THEROPODA (BIPEDAL CARNIVORES & PROTO-BIRDS)
            // -----------------------------------------------------------------
            public abstract class Theropoda : Saurischia
            {
                public bool IsObligateBipedal { get; } = true;
                public bool HasProtofeathers { get; protected set; } = true;

                public Theropoda(Creature? father = null, Creature? mother = null, int hands = 2, int teeths = 60, SexType? sex = null, string? name = null)
                    : base(father, mother, hands: hands, legs: 2, teeths: teeths, sex: sex, name: name) { }

                public class Tyrannosauridae : Theropoda // Tyrants
                {
                    public Tyrannosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, hands: 2, teeths: 60, sex: sex, name: name) { }

                    public class TyrannosaurusRex : Tyrannosauridae
                    {
                        public float LengthMeters { get; } = 12.3f;
                        public float BiteForceNewtons { get; } = 35000f; // Bone-crushing bite

                        public TyrannosaurusRex(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Tyrannosaurus rex")
                        {
                            EstimatedBodyMassTonnes = 8.8f;
                            MaxLifespanYears = 30f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 600f };
                        }

                        public string CrushingBoneBite(Creature target)
                        {
                            if (!IsAlive || !target.IsAlive) return "...";
                            target.TakeDamage(110f, DamageType.Piercing, "T-Rex 35,000 N bone-pulverizing jaw bite");
                            return $"{Name} clamps massive skull with 35,000 N bite force, driving 30 cm serrated banana-shaped teeth through {target.GetType().Name}'s bones!";
                        }

                        public string BinocularVisionTracking() => $"{Name} locks 55° binocular stereoscopic vision on prey, estimating distance with hawk-like acuity!";
                        public string InfrasonicApexRoar() => $"{Name} vibrates pneumatic throat chambers, producing a deep 12 Hz ground-trembling infrasonic roar heard for 8 km!";
                    }

                    public class TarbosaurusBataar : Tyrannosauridae
                    {
                        public TarbosaurusBataar(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Tarbosaurus bataar")
                        {
                            EstimatedBodyMassTonnes = 5.0f;
                            MaxLifespanYears = 28f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 500f };
                        }

                        public string RigidJawLockBite(Creature sauropodPrey)
                        {
                            if (!IsAlive || !sauropodPrey.IsAlive) return "...";
                            sauropodPrey.TakeDamage(95f, DamageType.Slashing, "Tarbosaurus rigid mandibular lock");
                            return $"{Name} engages interlocking lower jaw symphysis, preventing skull twist while ripping flesh from {sauropodPrey.GetType().Name}!";
                        }
                    }
                }

                public class Spinosauridae : Theropoda // Sail-backed Semi-Aquatic Titans
                {
                    public Spinosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, hands: 2, teeths: 64, sex: sex, name: name) { }

                    public class SpinosaurusAegyptiacus : Spinosauridae
                    {
                        public float LengthMeters { get; } = 15.0f;
                        public float SailHeightMeters { get; } = 1.8f;

                        public SpinosaurusAegyptiacus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Spinosaurus aegyptiacus")
                        {
                            EstimatedBodyMassTonnes = 7.5f;
                            MaxLifespanYears = 32f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 700f }; // Broad paddle tail
                        }

                        public string PaddleTailAquaticPropulsion() => $"{Name} undulates 7-meter flexible fin-like paddle tail laterally, propelling 7.5-tonne body through Cretaceous river delta!";
                        public string SnoutPressureReceptorStrike(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(85f, DamageType.Piercing, "Conical crocodile-jaw snap");
                            return $"{Name} senses underwater water displacement with neurovascular snout pores, snapping interlocking conical teeth onto {prey.GetType().Name}!";
                        }
                    }

                    public class BaryonyxWalkeri : Spinosauridae
                    {
                        public float ThumbClawLengthCm { get; } = 31f;

                        public BaryonyxWalkeri(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Baryonyx walkeri")
                        {
                            EstimatedBodyMassTonnes = 2.0f;
                            MaxLifespanYears = 26f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string HookFishWithThumbClaw(Creature aquaticPrey)
                        {
                            if (!IsAlive || !aquaticPrey.IsAlive) return "...";
                            aquaticPrey.TakeDamage(60f, DamageType.Slashing, "31cm curved thumb claw gaff");
                            return $"{Name} gaffs giant freshwater fish from river rapids using massive 31 cm sickle thumb claw!";
                        }
                    }
                }

                public class Dromaeosauridae : Theropoda // Raptors
                {
                    public Dromaeosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, hands: 2, teeths: 28, sex: sex, name: name) { }

                    public class VelociraptorMongoliensis : Dromaeosauridae
                    {
                        public float SickleClawLengthCm { get; } = 9.5f;

                        public VelociraptorMongoliensis(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Velociraptor mongoliensis")
                        {
                            EstimatedBodyMassTonnes = 0.015f; // ~15 kg turkey-sized
                            MaxLifespanYears = 14f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 100f }; // Ossified rod tail
                        }

                        public string HyperextensibleSickleClawPin(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(40f, DamageType.Piercing, "Raptor Prey Restraint sickle claw puncture");
                            return $"{Name} leaps onto {prey.GetType().Name}, pinning it with 9.5 cm 2nd toe sickle claw while flapping feathered wings for balance (RPR)!";
                        }

                        public string CoordinatePackAmbush() => $"{Name} coordinates high-speed flanking with pack members through Cretaceous desert dune corridor!";
                    }

                    public class UtahraptorOstrimmaysi : Dromaeosauridae
                    {
                        public float GiantSickleClawLengthCm { get; } = 24f;

                        public UtahraptorOstrimmaysi(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Utahraptor ostrommaysi")
                        {
                            EstimatedBodyMassTonnes = 0.5f; // 500 kg giant raptor
                            MaxLifespanYears = 22f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 300f };
                        }

                        public string GiantSickleSlash(Creature target)
                        {
                            if (!IsAlive || !target.IsAlive) return "...";
                            target.TakeDamage(75f, DamageType.Slashing, "24cm thick slicing sickle claw");
                            return $"{Name} delivers 24 cm killing claw kick with 500 kg momentum, disemboweling {target.GetType().Name}!";
                        }
                    }
                }

                public class Allosauridae : Theropoda // Carnosaurs
                {
                    public Allosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, hands: 2, teeths: 45, sex: sex, name: name) { }

                    public class AllosaurusFragilis : Allosauridae
                    {
                        public AllosaurusFragilis(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Allosaurus fragilis")
                        {
                            EstimatedBodyMassTonnes = 2.2f;
                            MaxLifespanYears = 28f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 450f };
                        }

                        public string HatchetSkullStrike(Creature prey)
                        {
                            if (!IsAlive || !prey.IsAlive) return "...";
                            prey.TakeDamage(70f, DamageType.Slashing, "Hatchet-style kinetic skull strike");
                            return $"{Name} opens jaw to extreme 92° gape, driving upper jaw downward like a battle-axe into {prey.GetType().Name}!";
                        }
                    }

                    public class GiganotosaurusCarolinii : Allosauridae
                    {
                        public GiganotosaurusCarolinii(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Giganotosaurus carolinii")
                        {
                            EstimatedBodyMassTonnes = 8.2f;
                            MaxLifespanYears = 32f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 650f };
                        }

                        public string SlicingZiphodontBite(Creature target)
                        {
                            if (!IsAlive || !target.IsAlive) return "...";
                            target.TakeDamage(90f, DamageType.Slashing, "Thin compressed ziphodont blade teeth slice");
                            return $"{Name} delivers rapid slicing bite with blade-like serrated teeth, causing massive arterial blood loss in {target.GetType().Name}!";
                        }
                    }
                }

                public class Abelisauridae : Theropoda // Horned Sprinting Carnivores
                {
                    public Abelisauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, hands: 2, teeths: 40, sex: sex, name: name) { }

                    public class CarnotaurusSastrei : Abelisauridae
                    {
                        public float SprintSpeedKmh { get; } = 55f;

                        public CarnotaurusSastrei(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Carnotaurus sastrei")
                        {
                            EstimatedBodyMassTonnes = 1.8f;
                            MaxLifespanYears = 25f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 400f };
                        }

                        public string HeadbuttWithBrowHorns(Creature rival)
                        {
                            if (!IsAlive || !rival.IsAlive) return "...";
                            rival.TakeDamage(50f, DamageType.Blunt, "Thick frontal horn ram");
                            return $"{Name} rams 15 cm thick frontal brow horns into {rival.GetType().Name} in high-speed shock collision!";
                        }

                        public string CaudofemoralisSpeedSprint() => $"{Name} contracts massive caudofemoralis longus tail muscles, accelerating to {SprintSpeedKmh:F0} km/h!";
                    }
                }

                public class Troodontidae : Theropoda // High-Intelligence Bird-like Predators
                {
                    public Troodontidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, hands: 2, teeths: 50, sex: sex, name: name) { }

                    public class TroodonFormosus : Troodontidae
                    {
                        public float EncephalizationQuotient { get; } = 5.8f;

                        public TroodonFormosus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Troodon formosus")
                        {
                            EstimatedBodyMassTonnes = 0.05f; // 50 kg
                            MaxLifespanYears = 16f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string NocturnalStereoHunt() => $"{Name} tracks small Cretaceous mammals in polar darkness with enormous forward-facing binocular orbits!";
                    }
                }

                public class Archaeopterygidae : Theropoda // Transitional Urvogel
                {
                    public Archaeopterygidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, hands: 2, teeths: 24, sex: sex, name: name) { }

                    public class ArchaeopteryxLithographica : Archaeopterygidae
                    {
                        public ArchaeopteryxLithographica(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Archaeopteryx lithographica")
                        {
                            EstimatedBodyMassTonnes = 0.001f; // ~1 kg
                            MaxLifespanYears = 10f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 30f }; // Bony vertebrate tail with lateral rectrices
                        }

                        public string FlutterGlidingFlight() => $"{Name} flaps pennaceous asymmetrical flight feathers while gripping limestone cliffs with three wing claws!";
                    }
                }

                public class Therizinosauridae : Theropoda // Scythe-Clawed Giant Herbivores
                {
                    public Therizinosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, hands: 2, teeths: 40, sex: sex, name: name) { }

                    public class TherizinosaurusCheloniformis : Therizinosauridae
                    {
                        public float ScytheClawLengthMeters { get; } = 1.0f; // Longest claws of any animal in history

                        public TherizinosaurusCheloniformis(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Therizinosaurus cheloniformis")
                        {
                            EstimatedBodyMassTonnes = 5.0f;
                            MaxLifespanYears = 40f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 150f };
                        }

                        public string ScytheClawSlash(Creature predator)
                        {
                            if (!IsAlive || !predator.IsAlive) return "...";
                            predator.TakeDamage(90f, DamageType.Slashing, "1-meter curved keratin scythe claw swipe");
                            return $"{Name} sweeps 1-meter long curved manual ungual claws with 5-tonne shoulder power, shearing across {predator.GetType().Name}!";
                        }

                        public string HarvestHighCanopyFoliage() => $"{Name} reaches 9 meters high with 2.5-meter forelimbs, hooking down leafy high-canopy Cretaceous branches into edentulous beak!";
                    }
                }

                public class Ornithomimidae : Theropoda // Ostrich-Mimic Dinosaurs
                {
                    public Ornithomimidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, hands: 2, teeths: 0, sex: sex, name: name) { } // Toothless beaks

                    public class GallimimusBullatus : Ornithomimidae
                    {
                        public float MaxSpeedKmh { get; } = 60f;

                        public GallimimusBullatus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Gallimimus bullatus")
                        {
                            EstimatedBodyMassTonnes = 0.45f;
                            MaxLifespanYears = 22f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 280f };
                        }

                        public string CursorialHerdSprint() => $"{Name} accelerates to {MaxSpeedKmh:F0} km/h on elongate arctometatarsalian feet, dodging predators with lightning agility!";
                        public string FilterFeedInShallowWater() => $"{Name} sifts water column with columnar lamellae along margin of keratinous beak, trapping micro-invertebrates!";
                    }

                    public class DeinocheirusMirificus : Ornithomimidae
                    {
                        public float ForelimbLengthMeters { get; } = 2.4f;

                        public DeinocheirusMirificus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Deinocheirus mirificus")
                        {
                            EstimatedBodyMassTonnes = 6.4f;
                            MaxLifespanYears = 45f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 350f };
                        }

                        public string ForageWetlandPlantsWithSpoonBill() => $"{Name} wades into Cretaceous marshlands on 2.4-meter massive arms, grazing water plants with broad spoon-like duck bill!";
                        public string DisplayDorsalSailHump() => $"{Name} exhibits massive dorsal vertebrae neural spines forming tall fibrous hump sail for fat storage and display!";
                    }
                }

                public class Oviraptorosauria : Theropoda // Crested Feathered Brooders
                {
                    public Oviraptorosauria(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, hands: 2, teeths: 0, sex: sex, name: name) { } // Edentulous shearing jaws

                    public class AnzuWyliei : Oviraptorosauria
                    {
                        public AnzuWyliei(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Anzu wyliei (The Hell Heron)")
                        {
                            EstimatedBodyMassTonnes = 0.3f; // ~300 kg
                            MaxLifespanYears = 20f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 120f };
                        }

                        public string BroodClutchWithPennaceousWings() => $"{Name} crouches symmetrically over a clutch of 20 elongated blue-green eggs, shielding them from wind with broad feathered wings!";
                    }

                    public class GigantoraptorErlianensis : Oviraptorosauria
                    {
                        public float TotalLengthMeters { get; } = 8.0f;

                        public GigantoraptorErlianensis(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Gigantoraptor erlianensis")
                        {
                            EstimatedBodyMassTonnes = 2.0f;
                            MaxLifespanYears = 30f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 250f };
                        }

                        public string ShearHardEggsAndNuts() => $"{Name} shears tough fibrous vegetation and hard dinosaur eggs with massive sharp-edged keratinous beak driven by powerful adductor muscles!";
                    }
                }
            }

            // -----------------------------------------------------------------
            // SUBORDER 2: SAUROPODOMORPHA (LONG-NECKED HERBIVOROUS GIANTS)
            // -----------------------------------------------------------------
            public abstract class Sauropodomorpha : Saurischia
            {
                public float NeckLengthMeters { get; protected set; } = 8.0f;
                public bool HasColumnarGraviportalLimbs { get; } = true;

                public Sauropodomorpha(Creature? father = null, Creature? mother = null, int teeths = 40, SexType? sex = null, string? name = null)
                    : base(father, mother, hands: 2, legs: 2, teeths: teeths, sex: sex, name: name)
                {
                    Body.Tail = new Tail { LengthCm = 1000f };
                }

                public class Brachiosauridae : Sauropodomorpha
                {
                    public Brachiosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 52, sex: sex, name: name) { }

                    public class BrachiosaurusAltithorax : Brachiosauridae
                    {
                        public float TotalHeightMeters { get; } = 13.0f;

                        public BrachiosaurusAltithorax(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Brachiosaurus altithorax")
                        {
                            EstimatedBodyMassTonnes = 40.0f;
                            NeckLengthMeters = 9.0f;
                            MaxLifespanYears = 80f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 800f };
                        }

                        public string HighCanopyConiferBrowse() => $"{Name} raises neck 13 meters above ground on elongated forelimbs, cropping fibrous Jurassic conifer canopy foliage!";
                    }
                }

                public class Titanosauria : Sauropodomorpha
                {
                    public Titanosauria(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 36, sex: sex, name: name) { }

                    public class ArgentinosaurusHuanculensis : Titanosauria
                    {
                        public float TotalLengthMeters { get; } = 35.0f;

                        public ArgentinosaurusHuanculensis(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Argentinosaurus huanculensis")
                        {
                            EstimatedBodyMassTonnes = 75.0f;
                            NeckLengthMeters = 11.0f;
                            MaxLifespanYears = 90f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 1400f };
                        }

                        public string GroundTremorStep() => $"{Name} strides forward on massive columnar pillar limbs; 75 tonnes of mass send seismic ground tremors for 500 meters!";
                        public string PumpMassiveBloodVolume() => $"{Name}'s 400 kg heart pumps arterial blood at 400 mmHg to ascend 10 meters up long neck to brain!";
                    }

                    public class DreadnoughtusSchrani : Titanosauria
                    {
                        public DreadnoughtusSchrani(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Dreadnoughtus schrani")
                        {
                            EstimatedBodyMassTonnes = 50.0f;
                            MaxLifespanYears = 75f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string GraviportalDominance() => $"{Name} stands completely impervious to contemporary predators due to colossal bulk and dense osteoderm dermal scutes!";
                    }
                }

                public class Diplodocidae : Sauropodomorpha
                {
                    public Diplodocidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 44, sex: sex, name: name) { }

                    public class DiplodocusCarnegii : Diplodocidae
                    {
                        public float TotalLengthMeters { get; } = 27.0f;

                        public DiplodocusCarnegii(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Diplodocus carnegii")
                        {
                            EstimatedBodyMassTonnes = 15.0f;
                            NeckLengthMeters = 8.0f;
                            MaxLifespanYears = 65f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 1400f }; // Supersonic whiplash tail
                        }

                        public string SupersonicTailWhipCrack(Creature predator)
                        {
                            if (!IsAlive || !predator.IsAlive) return "...";
                            predator.TakeDamage(80f, DamageType.Blunt, "Supersonic tail whip impact");
                            return $"{Name} whips 80-vertebrae tail tip at 340 m/s, cracking the sound barrier with a 140 dB sonic boom and striking {predator.GetType().Name}!";
                        }

                        public string RakeFernsWithPencilTeeth() => $"{Name} sweeps horizontal neck low over wetlands, raking horsetails and ferns through comb-like pencil teeth!";
                    }

                    public class ApatosaurusAjax : Diplodocidae
                    {
                        public ApatosaurusAjax(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Apatosaurus ajax")
                        {
                            EstimatedBodyMassTonnes = 25.0f;
                            NeckLengthMeters = 6.5f;
                            MaxLifespanYears = 70f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 1200f };
                        }

                        public string HeavyNeckSlam(Creature rival)
                        {
                            if (!IsAlive || !rival.IsAlive) return "...";
                            rival.TakeDamage(65f, DamageType.Blunt, "Reinforced cervical rib neck slam");
                            return $"{Name} swings massive muscular neck reinforced by deep triangular cervical ribs, battering {rival.GetType().Name}!";
                        }
                    }
                }
            }
        }

        // =====================================================================
        // --- CLADE 2: ORNITHISCHIA (BIRD-HIPPED BEAKED HERBIVORES)
        // =====================================================================
        public abstract class Ornithischia : Dinosauria
        {
            public bool HasPredentaryBeak { get; } = true;
            public bool HasOpisthopubicPelvis { get; } = true;

            public Ornithischia(Creature? father = null, Creature? mother = null, int hands = 2, int legs = 2, int teeths = 40, SexType? sex = null, string? name = null)
                : base(father, mother, hands: hands, legs: legs, teeths: teeths, sex: sex, name: name) { }

            // -----------------------------------------------------------------
            // SUBORDER 1: THYREOPHORA (SHIELD-BEARERS: ARMORED & PLATED DINOSAURS)
            // -----------------------------------------------------------------
            public abstract class Thyreophora : Ornithischia
            {
                public Thyreophora(Creature? father = null, Creature? mother = null, int teeths = 30, SexType? sex = null, string? name = null)
                    : base(father, mother, hands: 2, legs: 2, teeths: teeths, sex: sex, name: name) { }

                public class Stegosauridae : Thyreophora // Plated Stegosaurs
                {
                    public Stegosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 30, sex: sex, name: name) { }

                    public class StegosaurusStenops : Stegosauridae
                    {
                        public int DorsalPlatesCount { get; } = 17;

                        public StegosaurusStenops(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Stegosaurus stenops")
                        {
                            EstimatedBodyMassTonnes = 4.5f;
                            MaxLifespanYears = 45f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 400f };
                        }

                        public string FlushDorsalPlatesWithBlood() => $"{Name} engorges 17 alternating vascularized osteoderm dorsal plates with warm blood, displaying crimson warning colors!";

                        public string ThagomizerTailStrike(Creature allosaurPredator)
                        {
                            if (!IsAlive || !allosaurPredator.IsAlive) return "...";
                            allosaurPredator.TakeDamage(85f, DamageType.Piercing, "Thagomizer 4-spike tail impalement");
                            return $"{Name} swings muscular tail at 90 km/h, driving four 90 cm bone spikes (thagomizer) directly into {allosaurPredator.GetType().Name}!";
                        }
                    }

                    public class KentrosaurusAethiopicus : Stegosauridae
                    {
                        public KentrosaurusAethiopicus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Kentrosaurus aethiopicus")
                        {
                            EstimatedBodyMassTonnes = 1.5f;
                            MaxLifespanYears = 35f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ShoulderSpikeDisplay() => $"{Name} points 60 cm lateral parasacral shoulder spines outward, deterring Jurassic theropods from flank attacks!";
                    }
                }

                public class Ankylosauridae : Thyreophora // Armored Club-Tailed Titans
                {
                    public Ankylosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 35, sex: sex, name: name) { }

                    public class AnkylosaurusMagniventris : Ankylosauridae
                    {
                        public float TailClubMassKg { get; } = 60f;

                        public AnkylosaurusMagniventris(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Ankylosaurus magniventris")
                        {
                            EstimatedBodyMassTonnes = 8.0f;
                            MaxLifespanYears = 50f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 350f };
                        }

                        public string HunkerArmorDefense() => $"{Name} crouches low to protect soft underbelly, exposing only fused cranial osteoderms and impenetrable interlocking dorsal armor plates!";

                        public string ShatterBonesWithTailClub(Creature attacker)
                        {
                            if (!IsAlive || !attacker.IsAlive) return "...";
                            attacker.TakeDamage(95f, DamageType.Blunt, "60kg ossified tail club fracture impact");
                            return $"{Name} swings 60 kg rigid ossified distal tail club with kinetic sledgehammer force, shattering {attacker.GetType().Name}'s tibia and metatarsals!";
                        }
                    }

                    public class EuoplocephalusTutus : Ankylosauridae
                    {
                        public EuoplocephalusTutus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Euoplocephalus tutus")
                        {
                            EstimatedBodyMassTonnes = 2.5f;
                            MaxLifespanYears = 40f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ArmoredEyelidsBlink() => $"{Name} closes ossified bony armored eyelid plates over eyes to withstand flying shrapnel and predator claws!";
                    }
                }

                public class Nodosauridae : Thyreophora // Clubless Armored Dinosaurs
                {
                    public Nodosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 32, sex: sex, name: name) { }

                    public class BorealopeltaMarkmitchelli : Nodosauridae
                    {
                        public BorealopeltaMarkmitchelli(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Borealopelta markmitchelli")
                        {
                            EstimatedBodyMassTonnes = 1.3f;
                            MaxLifespanYears = 38f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string CamouflageCountershading() => $"{Name} conceals 1.3 tonnes using reddish-brown pheomelanin dorsal countershading against predatory theropods!";
                    }

                    public class SauropeltaEdwardsi : Nodosauridae
                    {
                        public SauropeltaEdwardsi(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Sauropelta edwardsi")
                        {
                            EstimatedBodyMassTonnes = 1.5f;
                            MaxLifespanYears = 36f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string SpinedCollarThrust() => $"{Name} thrusts massive 50 cm lateral neck spikes to shield jugular vein from dromaeosaur ambushes!";
                    }
                }

                public class Scelidosauridae : Thyreophora // Basal Armored Dinosaurs
                {
                    public Scelidosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 34, sex: sex, name: name) { }

                    public class ScelidosaurusHarrisonii : Scelidosauridae
                    {
                        public ScelidosaurusHarrisonii(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Scelidosaurus harrisonii")
                        {
                            EstimatedBodyMassTonnes = 0.25f;
                            MaxLifespanYears = 25f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ForageLowEarlyJurassicFerns() => $"{Name} browses low-growing cycads along Jurassic shorelines, protected by rows of keeled scutes!";
                    }
                }
            }

            // -----------------------------------------------------------------
            // SUBORDER 2: MARGINOCEPHALIA (CERATOPSIANS & PACHYCEPHALOSAURS)
            // -----------------------------------------------------------------
            public abstract class Marginocephalia : Ornithischia
            {
                public Marginocephalia(Creature? father = null, Creature? mother = null, int teeths = 40, SexType? sex = null, string? name = null)
                    : base(father, mother, hands: 2, legs: 2, teeths: teeths, sex: sex, name: name) { }

                public class Ceratopsidae : Marginocephalia // Horned Dinosaurs
                {
                    public Ceratopsidae(Creature? father = null, Creature? mother = null, int teeths = 40, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: teeths, sex: sex, name: name) { }

                    public class TriceratopsHorridus : Ceratopsidae
                    {
                        public float BrowHornLengthCm { get; } = 100f;

                        public TriceratopsHorridus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, teeths: 800, sex: sex, name: name ?? "Triceratops horridus") // Continuous dental battery
                        {
                            EstimatedBodyMassTonnes = 9.0f;
                            MaxLifespanYears = 50f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 300f };
                        }

                        public string ImpaleWithBrowHorns(Creature theropod)
                        {
                            if (!IsAlive || !theropod.IsAlive) return "...";
                            theropod.TakeDamage(100f, DamageType.Piercing, "Dual 1-meter keratinized brow horn goring");
                            return $"{Name} charges with solid bone parietosquamosal frill forward, driving twin 1-meter brow horns through {theropod.GetType().Name}'s chest cavity!";
                        }

                        public string ShearFibrousPalms() => $"{Name} chops fibrous Cretaceous palms and cycad stems using continuous shearing dental battery of 800 interlocking teeth!";
                    }

                    public class StyracosaurusAlbertensis : Ceratopsidae
                    {
                        public float NasalHornLengthCm { get; } = 60f;

                        public StyracosaurusAlbertensis(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, teeths: 400, sex: sex, name: name ?? "Styracosaurus albertensis")
                        {
                            EstimatedBodyMassTonnes = 3.0f;
                            MaxLifespanYears = 40f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string DisplayFrillSpikeCrown() => $"{Name} lowers head, displaying 60 cm central nose horn and 6 radiating 50 cm parietal frill spikes in territorial confrontation!";
                    }

                    public class CentrosaurusApertus : Ceratopsidae
                    {
                        public CentrosaurusApertus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, teeths: 400, sex: sex, name: name ?? "Centrosaurus apertus")
                        {
                            EstimatedBodyMassTonnes = 2.8f;
                            MaxLifespanYears = 38f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string MegaHerdMigration() => $"{Name} migrates across Cretaceous floodplains within a mega-herd of 10,000 ceratopsians!";
                    }

                    public class ProtoceratopsAndrewsi : Ceratopsidae
                    {
                        public ProtoceratopsAndrewsi(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, teeths: 60, sex: sex, name: name ?? "Protoceratops andrewsi")
                        {
                            EstimatedBodyMassTonnes = 0.18f; // 180 kg sheep-sized
                            MaxLifespanYears = 20f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string BeakClampBite(Creature attacker)
                        {
                            if (!IsAlive || !attacker.IsAlive) return "...";
                            attacker.TakeDamage(35f, DamageType.Piercing, "Protoceratops shearing rostral beak");
                            return $"{Name} clamps razor-sharp keratinous rostral beak onto {attacker.GetType().Name}'s arm with crushing adductor jaw force!";
                        }
                    }
                }

                public class Pachycephalosauridae : Marginocephalia // Dome-Headed Dinosaurs
                {
                    public Pachycephalosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 36, sex: sex, name: name) { }

                    public class PachycephalosaurusWyomingensis : Pachycephalosauridae
                    {
                        public float DomeThicknessCm { get; } = 25f; // Solid bone dome

                        public PachycephalosaurusWyomingensis(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Pachycephalosaurus wyomingensis")
                        {
                            EstimatedBodyMassTonnes = 0.45f;
                            MaxLifespanYears = 28f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string HighImpactCranialRam(Creature rival)
                        {
                            if (!IsAlive || !rival.IsAlive) return "...";
                            rival.TakeDamage(60f, DamageType.Blunt, "Solid bone frontoparietal dome ram");
                            return $"{Name} lowers head and rams 25 cm thick solid bone frontoparietal dome into {rival.GetType().Name} at 30 km/h, absorbed by fibrolamellar trabeculae!";
                        }
                    }

                    public class StygimolochSpinifer : Pachycephalosauridae
                    {
                        public StygimolochSpinifer(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Stygimoloch spinifer")
                        {
                            EstimatedBodyMassTonnes = 0.2f;
                            MaxLifespanYears = 24f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ThreatenWithSquamosalHorns() => $"{Name} shakes head displaying cluster of 10 cm squamosal bone spikes ringing posterior margin of cranial dome!";
                    }
                }
            }

            // -----------------------------------------------------------------
            // SUBORDER 3: ORNITHOPODA (DUCK-BILLED HADROSAURS & RELATIVES)
            // -----------------------------------------------------------------
            public abstract class Ornithopoda : Ornithischia
            {
                public Ornithopoda(Creature? father = null, Creature? mother = null, int teeths = 40, SexType? sex = null, string? name = null)
                    : base(father, mother, hands: 2, legs: 2, teeths: teeths, sex: sex, name: name) { }

                public class Hadrosauridae : Ornithopoda // Duck-billed Dinosaurs
                {
                    public Hadrosauridae(Creature? father = null, Creature? mother = null, int teeths = 1000, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: teeths, sex: sex, name: name) { }

                    public class ParasaurolophusWalkeri : Hadrosauridae
                    {
                        public float CrestLengthMeters { get; } = 1.8f;

                        public ParasaurolophusWalkeri(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, teeths: 960, sex: sex, name: name ?? "Parasaurolophus walkeri")
                        {
                            EstimatedBodyMassTonnes = 3.5f;
                            MaxLifespanYears = 42f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 450f };
                        }

                        public string ResonantCrestCall() => $"{Name} blows air through 1.8-meter internal U-shaped nasal passages inside hollow cranial crest, producing a deep 55 Hz foghorn acoustic blast!";
                    }

                    public class EdmontosaurusAnnectens : Hadrosauridae
                    {
                        public EdmontosaurusAnnectens(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, teeths: 1400, sex: sex, name: name ?? "Edmontosaurus annectens")
                        {
                            EstimatedBodyMassTonnes = 5.0f;
                            MaxLifespanYears = 45f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 500f };
                        }

                        public string GrindConifersWith1400Teeth() => $"{Name} grinds abrasive horsetails and conifers into pulp using composite dental battery of 1,400 diamond-hard self-sharpening teeth!";
                    }

                    public class CorythosaurusCasuarius : Hadrosauridae
                    {
                        public CorythosaurusCasuarius(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, teeths: 800, sex: sex, name: name ?? "Corythosaurus casuarius")
                        {
                            EstimatedBodyMassTonnes = 4.0f;
                            MaxLifespanYears = 40f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string SoundHelmetCrestTrumpet() => $"{Name} resonates calls through circular helmet-shaped crest, coordinating herd movements along Cretaceous coastal lowlands!";
                    }

                    public class MaiasauraPeeblesorum : Hadrosauridae
                    {
                        public MaiasauraPeeblesorum(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, teeths: 700, sex: sex, name: name ?? "Maiasaura peeblesorum (Good Mother Lizard)")
                        {
                            EstimatedBodyMassTonnes = 3.0f;
                            MaxLifespanYears = 35f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string TendColonialNurseryNests() => $"{Name} incubates communal volcanic mud nests lined with decaying vegetative compost, feeding altricial hatchlings chewed plant forage!";
                    }
                }

                public class Iguanodontidae : Ornithopoda // Thumb-Spiked Dinosaurs
                {
                    public Iguanodontidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 58, sex: sex, name: name) { }

                    public class IguanodonBernissartensis : Iguanodontidae
                    {
                        public float ThumbSpikeLengthCm { get; } = 19f;

                        public IguanodonBernissartensis(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Iguanodon bernissartensis")
                        {
                            EstimatedBodyMassTonnes = 4.0f;
                            MaxLifespanYears = 40f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 400f };
                        }

                        public string DefensiveThumbSpikeStab(Creature carnivore)
                        {
                            if (!IsAlive || !carnivore.IsAlive) return "...";
                            carnivore.TakeDamage(65f, DamageType.Piercing, "19cm conical thumb spike defensive puncture");
                            return $"{Name} rears on hindlimbs and stabs 19 cm conical keratin-sheathed thumb spike directly into {carnivore.GetType().Name}'s neck!";
                        }
                    }

                    public class OuranosaurusNigeriensis : Iguanodontidae
                    {
                        public float SailHeightMeters { get; } = 1.0f;

                        public OuranosaurusNigeriensis(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Ouranosaurus nigeriensis")
                        {
                            EstimatedBodyMassTonnes = 2.2f;
                            MaxLifespanYears = 32f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string ThermoregulateThroughDorsalSail() => $"{Name} absorbs Saharan early-morning heat via elongated neural spine dorsal sail to accelerate sluggish metabolism!";
                    }
                }

                public class Hypsilophodontidae : Ornithopoda // Small Agile Runners
                {
                    public Hypsilophodontidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 30, sex: sex, name: name) { }

                    public class HypsilophodonFoxii : Hypsilophodontidae
                    {
                        public HypsilophodonFoxii(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Hypsilophodon foxii")
                        {
                            EstimatedBodyMassTonnes = 0.02f; // ~20 kg gazelle-sized
                            MaxLifespanYears = 14f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string HighAgilityBipedalSprint() => $"{Name} darts through dense Early Cretaceous understory at 45 km/h, balancing with stiff ossified tendon tail!";
                    }
                }

                public class Heterodontosauridae : Ornithopoda // Differentiated Teeth Dinosaurs
                {
                    public Heterodontosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 32, sex: sex, name: name) { }

                    public class HeterodontosaurusTucki : Heterodontosauridae
                    {
                        public HeterodontosaurusTucki(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Heterodontosaurus tucki")
                        {
                            EstimatedBodyMassTonnes = 0.01f; // ~10 kg
                            MaxLifespanYears = 12f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string DisplayCanineTusks() => $"{Name} displays prominent upper and lower canine tusks alongside chiseling incisors and grinding molars!";
                    }
                }
            }
        }

        // =====================================================================
        // --- CLADE 3: PTEROSAURIA (FLYING ARCHOSAURS - POWERED MEMBRANOUS FLIGHT)
        // =====================================================================
        public abstract class Pterosauria : Dinosauria
        {
            public float WingspanMeters { get; protected set; } = 4.0f;
            public bool HasActinofibrilMembrane { get; } = true;
            public bool HasPycnofibers { get; } = true;

            public Pterosauria(Creature? father = null, Creature? mother = null, int teeths = 0, SexType? sex = null, string? name = null)
                : base(father, mother, hands: 2, legs: 2, teeths: teeths, sex: sex, name: name)
            {
                EstimatedBodyMassTonnes = 0.1f;
                Body.Tail = new Tail { LengthCm = 20f };
            }

            public class Azhdarchidae : Pterosauria // Giant Stork-like Azhdarchids
            {
                public Azhdarchidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: 0, sex: sex, name: name) { } // Edentulous

                public class QuetzalcoatlusNorthropi : Azhdarchidae
                {
                    public QuetzalcoatlusNorthropi(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Quetzalcoatlus northropi")
                    {
                        WingspanMeters = 11.0f; // Giraffe-sized flying titan
                        EstimatedBodyMassTonnes = 0.25f;
                        MaxLifespanYears = 35f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string QuadrupedalVaultLaunch() => $"{Name} rocks back on hind feet and launches 250 kg body into the air using explosive forelimb pole-vault vaulting mechanics!";
                    public string TerrestrialStalkSnatch(Creature smallPrey)
                    {
                        if (!IsAlive || !smallPrey.IsAlive) return "...";
                        smallPrey.TakeDamage(60f, DamageType.Piercing, "2.5-meter spear-like azhdarchid beak strike");
                        return $"{Name} strides over Cretaceous floodplain at giraffe height, spearing {smallPrey.GetType().Name} with 2.5-meter edentulous javelin beak!";
                    }
                }

                public class HatzegopteryxThambema : Azhdarchidae
                {
                    public HatzegopteryxThambema(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Hatzegopteryx thambema")
                    {
                        WingspanMeters = 10.5f;
                        EstimatedBodyMassTonnes = 0.28f;
                        MaxLifespanYears = 32f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string IslandApexPredatorSlam() => $"{Name} rules Hațeg Island as apex terrestrial carnivore, using reinforced thick-walled cervical vertebrae to batter dwarf titanosaurs!";
                }
            }

            public class Pteranodontidae : Pterosauria // Oceanic Pelagic Soarers
            {
                public Pteranodontidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: 0, sex: sex, name: name) { }

                public class PteranodonLongiceps : Pteranodontidae
                {
                    public float CranialCrestLengthCm { get; } = 80f;

                    public PteranodonLongiceps(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Pteranodon longiceps")
                    {
                        WingspanMeters = 7.0f;
                        EstimatedBodyMassTonnes = 0.035f; // ~35 kg lightweight soarer
                        MaxLifespanYears = 30f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DynamicOceanSoaring(float distanceKm = 100f) => $"{Name} soars for {distanceKm:F0} km over Western Interior Seaway waves using high aspect-ratio wings and cranial crest aerodynamic yaw rudder!";
                    public string DipNetPelagicFish(Fish fish)
                    {
                        if (!fish.IsAlive) return "...";
                        fish.TakeDamage(40f, DamageType.Piercing, "Pteranodon dive-scoop beak snap");
                        return $"{Name} skim-dips beak into wave crests, scooping {fish.SpeciesName} into expandable throat gular pouch!";
                    }
                }
            }

            public class Rhamphorhynchidae : Pterosauria // Long-Tailed Toothed Pterosaurs
            {
                public Rhamphorhynchidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: 30, sex: sex, name: name) { }

                public class RhamphorhynchusMuensteri : Rhamphorhynchidae
                {
                    public RhamphorhynchusMuensteri(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Rhamphorhynchus muensteri")
                    {
                        WingspanMeters = 1.8f;
                        EstimatedBodyMassTonnes = 0.002f; // ~2 kg
                        MaxLifespanYears = 18f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 70f }; // Long tail with diamond vane
                    }

                    public string SteerWithDiamondTailVane() => $"{Name} executes sharp aerodynamic banking turns over Solnhofen lagoons, guided by vertical diamond-shaped caudal rudder vane!";
                }
            }

            public class Anurognathidae : Pterosauria // Frog-Mouthed Aerial Insectivores
            {
                public Anurognathidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: 24, sex: sex, name: name) { }

                public class AnurognathusAmmoni : Anurognathidae
                {
                    public AnurognathusAmmoni(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Anurognathus ammoni")
                    {
                        WingspanMeters = 0.5f;
                        EstimatedBodyMassTonnes = 0.00004f; // 40 grams
                        MaxLifespanYears = 8f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string HawkInsectsNocturnally() => $"{Name} snaps wide gaping frog-mouth surrounded by sensory rictal bristles, capturing moths in nocturnal Jurassic flight!";
                }
            }
        }

        // =====================================================================
        // --- CLADE 4: PLESIOSAURIA (MESOZOIC MARINE REPTILES - UNDERWATER FLIGHT)
        // =====================================================================
        public abstract class Plesiosauria : Dinosauria
        {
            public bool HasFourHydrofoilFlippers { get; } = true;

            public Plesiosauria(Creature? father = null, Creature? mother = null, int teeths = 40, SexType? sex = null, string? name = null)
                : base(father, mother, hands: 0, legs: 0, teeths: teeths, sex: sex, name: name)
            {
                Body.Tail = new Tail { LengthCm = 200f };
            }

            public class Plesiosauroidea : Plesiosauria // Long-Necked Plesiosaurs
            {
                public Plesiosauroidea(Creature? father = null, Creature? mother = null, int teeths = 40, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: teeths, sex: sex, name: name) { }

                public class ElasmosaurusPlatyurus : Plesiosauroidea
                {
                    public int CervicalVertebraeCount { get; } = 71;
                    public float NeckLengthMeters { get; } = 7.1f;

                    public ElasmosaurusPlatyurus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 60, sex: sex, name: name ?? "Elasmosaurus platyurus")
                    {
                        EstimatedBodyMassTonnes = 2.5f;
                        MaxLifespanYears = 45f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DartHyperElongatedNeck(Fish target)
                    {
                        if (!target.IsAlive) return "...";
                        target.TakeDamage(45f, DamageType.Piercing, "Interlocking fang strike");
                        return $"{Name} darts 7.1-meter neck sideways beneath schooling fish, snapping interlocking needle-fangs into {target.SpeciesName} without disturbing water column!";
                    }

                    public string IngestGastrolithBallast(int stones = 50) => $"{Name} swallows {stones} quartz river stones (gastroliths) to adjust buoyancy and grind ammonite shells in muscular gizzard!";
                }

                public class PlesiosaurusDolichodeirus : Plesiosauroidea
                {
                    public PlesiosaurusDolichodeirus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 48, sex: sex, name: name ?? "Plesiosaurus dolichodeirus")
                    {
                        EstimatedBodyMassTonnes = 0.5f;
                        MaxLifespanYears = 35f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string FlyUnderwaterSubaqueous() => $"{Name} beats four hydrofoil flippers synchronously in figure-eight motion, 'flying' gracefully through Jurassic oceans!";
                }
            }

            public class Pliosauroidea : Plesiosauria // Short-Necked Apex Pliosaurs
            {
                public Pliosauroidea(Creature? father = null, Creature? mother = null, int teeths = 40, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: teeths, sex: sex, name: name) { }

                public class LiopleurodonFerox : Pliosauroidea
                {
                    public float SkullLengthMeters { get; } = 2.5f;

                    public LiopleurodonFerox(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 44, sex: sex, name: name ?? "Liopleurodon ferox")
                    {
                        EstimatedBodyMassTonnes = 5.0f;
                        MaxLifespanYears = 50f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string StereoOlfactorySniff() => $"{Name} channels seawater through internal narial ducts separated by wide palate, detecting prey scent in stereo!";

                    public string ApexPliosaurChomp(Creature marinePrey)
                    {
                        if (!IsAlive || !marinePrey.IsAlive) return "...";
                        marinePrey.TakeDamage(100f, DamageType.Slashing, "Liopleurodon 2.5m skull crushing chomp");
                        return $"{Name} ambushes from below, clamping 2.5-meter jaws with massive trihedral teeth through {marinePrey.GetType().Name}!";
                    }
                }

                public class KronosaurusQueenslandicus : Pliosauroidea
                {
                    public KronosaurusQueenslandicus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 40, sex: sex, name: name ?? "Kronosaurus queenslandicus")
                    {
                        EstimatedBodyMassTonnes = 11.0f;
                        MaxLifespanYears = 55f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string CrushGiantAmmonites() => $"{Name} crushes 1.5-meter giant ammonite shells effortlessly with 30 cm conical non-serrated teeth!";
                }
            }
        }

        // =====================================================================
        // --- CLADE 5: MOSASAUROIDEA (CRETACEOUS APEX MARINE SQUAMATES)
        // =====================================================================
        public abstract class Mosasauroidea : Dinosauria
        {
            public bool HasPterygoidPalatalTeeth { get; } = true;
            public bool HasIntramandibularJoint { get; } = true;

            public Mosasauroidea(Creature? father = null, Creature? mother = null, int teeths = 40, SexType? sex = null, string? name = null)
                : base(father, mother, hands: 0, legs: 0, teeths: teeths, sex: sex, name: name)
            {
                Body.Tail = new Tail { LengthCm = 600f };
            }

            public class Mosasauridae : Mosasauroidea
            {
                public Mosasauridae(Creature? father = null, Creature? mother = null, int teeths = 40, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: teeths, sex: sex, name: name) { }

                public class MosasaurusHoffmannii : Mosasauridae
                {
                    public float TotalLengthMeters { get; } = 17.1f;

                    public MosasaurusHoffmannii(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 48, sex: sex, name: name ?? "Mosasaurus hoffmannii")
                    {
                        EstimatedBodyMassTonnes = 14.0f;
                        MaxLifespanYears = 50f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 850f }; // Hypocercal tail fluke
                    }

                    public string HypocercalTailPropulsion() => $"{Name} undulates powerful downward-canted hypocercal tail fluke, generating explosive 50 km/h predatory bursts!";

                    public string DoubleHingedEngulfment(Creature prey)
                    {
                        if (!IsAlive || !prey.IsAlive) return "...";
                        prey.TakeDamage(105f, DamageType.Piercing, "Mosasaur double-hinged jaw and pterygoid teeth pull");
                        return $"{Name} flexes intramandibular joint to widen gape, using dual rows of pterygoid teeth on palate roof to ratchet {prey.GetType().Name} whole down throat!";
                    }
                }

                public class TylosaurusProriger : Mosasauridae
                {
                    public float HardSnoutRamLengthCm { get; } = 40f;

                    public TylosaurusProriger(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 42, sex: sex, name: name ?? "Tylosaurus proriger")
                    {
                        EstimatedBodyMassTonnes = 8.5f;
                        MaxLifespanYears = 45f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string RamWithCylindricalRostrum(Creature prey)
                    {
                        if (!IsAlive || !prey.IsAlive) return "...";
                        prey.TakeDamage(80f, DamageType.Blunt, "Tylosaurus toothless bone battering ram");
                        return $"{Name} rams prey at high speed with toothless, heavily ossified cylindrical snout battering ram to stun it before consuming!";
                    }
                }
            }
        }

        // =====================================================================
        // --- CLADE 6: ICHTHYOSAURIA (DOLPHIN-LIKE MESOZOIC MARINE REPTILES)
        // =====================================================================
        public abstract class Ichthyosauria : Dinosauria
        {
            public bool GivesLiveBirthViviparous { get; } = true;
            public float ScleroticRingDiameterCm { get; protected set; } = 15f;

            public Ichthyosauria(Creature? father = null, Creature? mother = null, int teeths = 50, SexType? sex = null, string? name = null)
                : base(father, mother, hands: 0, legs: 0, teeths: teeths, sex: sex, name: name)
            {
                Body.Tail = new Tail { LengthCm = 300f };
            }

            public class Shastasauridae : Ichthyosauria // Triassic Giant Ichthyosaurs
            {
                public Shastasauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: 0, sex: sex, name: name) { } // Suction feeding

                public class ShonisaurusPopularis : Shastasauridae
                {
                    public float TotalLengthMeters { get; } = 21.0f;

                    public ShonisaurusPopularis(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Shonisaurus popularis")
                    {
                        EstimatedBodyMassTonnes = 30.0f;
                        ScleroticRingDiameterCm = 28f; // Largest eyes in vertebrate fossil record
                        MaxLifespanYears = 65f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SuctionFeedInMesozoicAbyss() => $"{Name} descends into Triassic deep ocean, using giant 28 cm sclerotic-supported eyes to track and suction-feed on shoals of belemnites!";
                }
            }

            public class Ophthalmosauridae : Ichthyosauria // Big-Eyed Deep Divers
            {
                public Ophthalmosauridae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: 80, sex: sex, name: name) { }

                public class OphthalmosaurusIcenicus : Ophthalmosauridae
                {
                    public OphthalmosaurusIcenicus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Ophthalmosaurus icenicus")
                    {
                        EstimatedBodyMassTonnes = 1.0f;
                        ScleroticRingDiameterCm = 22f;
                        MaxLifespanYears = 30f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string DeepTwilightDive(float depthMeters = 600f) => $"{Name} dives to {depthMeters:F0}m into aphotic ocean zones, gathering faint bioluminescent light with colossal 22 cm eyes!";
                }

                public class IchthyosaurusCommunis : Ophthalmosauridae
                {
                    public IchthyosaurusCommunis(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Ichthyosaurus communis")
                    {
                        EstimatedBodyMassTonnes = 0.1f;
                        MaxLifespanYears = 22f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string TailFirstViviparousBirth() => $"{Name} gives birth to live young underwater tail-first (viviparity), preventing neonate drowning!";
                }
            }
        }

        // =====================================================================
        // --- CLADE 7: SYNAPSIDA (PALEOZOIC STEM-MAMMALS & PRE-DINOSAURIAN APEX AMNIOTES)
        // =====================================================================
        public abstract class Synapsida : Dinosauria
        {
            public bool HasSingleTemporalFenestra { get; } = true;

            public Synapsida(Creature? father = null, Creature? mother = null, int teeths = 40, SexType? sex = null, string? name = null)
                : base(father, mother, hands: 2, legs: 2, teeths: teeths, sex: sex, name: name) { }

            public class Sphenacodontidae : Synapsida // Sail-Backed Pelycosaurs
            {
                public Sphenacodontidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: 38, sex: sex, name: name) { }

                public class DimetrodonGrandis : Sphenacodontidae
                {
                    public float SailHeightMeters { get; } = 1.5f;

                    public DimetrodonGrandis(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Dimetrodon grandis")
                    {
                        EstimatedBodyMassTonnes = 0.25f;
                        MaxLifespanYears = 28f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 150f };
                    }

                    public string RapidThermoregulationSail() => $"{Name} positions 1.5-meter vascularized neural spine sail perpendicular to early Permian sun, raising core body temperature in 30 minutes!";

                    public string DualDifferentiationBite(Creature prey)
                    {
                        if (!IsAlive || !prey.IsAlive) return "...";
                        prey.TakeDamage(55f, DamageType.Piercing, "Dimetrodon differentiated canine and shearing teeth strike");
                        return $"{Name} sinks massive curved canines and shearing cheek teeth, demonstrating earliest amniote dental differentiation on {prey.GetType().Name}!";
                    }
                }
            }

            public class Gorgonopsidae : Synapsida // Permian Apex Saber-Toothed Therapsids
            {
                public Gorgonopsidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: 32, sex: sex, name: name) { }

                public class InostranceviaAlexandri : Gorgonopsidae
                {
                    public float SaberCanineLengthCm { get; } = 15f;

                    public InostranceviaAlexandri(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Inostrancevia alexandri")
                    {
                        EstimatedBodyMassTonnes = 0.6f;
                        MaxLifespanYears = 26f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        Body.Tail = new Tail { LengthCm = 100f };
                    }

                    public string SaberToothThroatSlash(Creature pareiasaurPrey)
                    {
                        if (!IsAlive || !pareiasaurPrey.IsAlive) return "...";
                        pareiasaurPrey.TakeDamage(80f, DamageType.Slashing, "15cm serrated saber-tooth throat bite");
                        return $"{Name} drives 15 cm flattened saber canines with upright parasagittal gait into {pareiasaurPrey.GetType().Name}'s jugular!";
                    }
                }
            }

            public class Dicynodontidae : Synapsida // Tusked Herbivorous Extinction Survivors
            {
                public Dicynodontidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: 2, sex: sex, name: name) { } // Only two upper caniniform tusks

                public class LystrosaurusMurrayi : Dicynodontidae
                {
                    public LystrosaurusMurrayi(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, sex, name ?? "Lystrosaurus murrayi (Permian Extinction Survivor)")
                    {
                        EstimatedBodyMassTonnes = 0.09f; // 90 kg
                        MaxLifespanYears = 20f;
                        if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                    }

                    public string SurvivePermianTriassicExtinction() => $"{Name} burrows into subterranean chambers, surviving the catastrophic 'Great Dying' extinction that wiped out 95% of species!";
                    public string SliceToughRootsWithBeak() => $"{Name} crops fibrous post-extinction Dicroidium vegetation using turtle-like horny beak and twin downward tusks!";
                }
            }

            public class Cynodontia : Synapsida // Advanced Stem-Mammals / Mammal Ancestors
            {
                public bool HasSecondaryBonyPalate { get; } = true;
                public bool HasDiaphragmaticBreathing { get; } = true;

                public Cynodontia(Creature? father = null, Creature? mother = null, int teeths = 32, SexType? sex = null, string? name = null)
                    : base(father, mother, teeths: teeths, sex: sex, name: name) { }

                public class Thrinaxodontidae : Cynodontia
                {
                    public Thrinaxodontidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 30, sex: sex, name: name) { }

                    public class ThrinaxodonLiorhinus : Thrinaxodontidae
                    {
                        public ThrinaxodonLiorhinus(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Thrinaxodon liorhinus (Whiskered Cynodont)")
                        {
                            EstimatedBodyMassTonnes = 0.004f; // ~4 kg cat-sized
                            MaxLifespanYears = 14f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 25f };
                        }

                        public string BreatheWhileChewingWithSecondaryPalate() => $"{Name} breathes continuously while masticating food thanks to fully ossified secondary bony palate separating oral cavity from nasal passage!";
                        public string SenseBurrowWithTactileWhiskers() => $"{Name} navigates Early Triassic underground torpor burrows using neurovascular sensory foramina supporting tactile facial vibrissae whiskers!";
                    }
                }

                public class Traversodontidae : Cynodontia
                {
                    public Traversodontidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 28, sex: sex, name: name) { }

                    public class ExaeretodonFrenguellii : Traversodontidae
                    {
                        public ExaeretodonFrenguellii(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Exaeretodon frenguellii")
                        {
                            EstimatedBodyMassTonnes = 0.12f; // ~120 kg
                            MaxLifespanYears = 22f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string GrindTriassicFoliageWithBroadMolars() => $"{Name} grinds abrasive seed-fern fronds with wide multicuspid expanded cheek teeth powered by enlarged masseter jaw musculature!";
                    }
                }

                public class Morganucodontidae : Cynodontia // Earliest Mammaliaforms
                {
                    public Morganucodontidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 32, sex: sex, name: name) { }

                    public class MorganucodonWatkinsi : Morganucodontidae
                    {
                        public bool HasDiphyodontDentition { get; } = true; // Deciduous milk teeth + adult teeth

                        public MorganucodonWatkinsi(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Morganucodon watkinsi (First Mammaliaform)")
                        {
                            EstimatedBodyMassTonnes = 0.00003f; // ~30 grams shrew-sized
                            MaxLifespanYears = 6f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                        }

                        public string HighAcuityMiddleEarHearing() => $"{Name} detects faint insect footsteps in darkness using evolutionary dual-joint jaw mechanism transitioning articular and quadrate bones into middle-ear ossicles (malleus and incus)!";
                        public string ReplaceBabyTeethWithAdultTeeth() => $"{Name} sheds juvenile deciduous teeth, erupting permanent diphyodont adult dentition!";
                    }
                }

                public class Tritylodontidae : Cynodontia // Highly Specialized Herbivorous Mammal-like Cynodonts
                {
                    public Tritylodontidae(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                        : base(father, mother, teeths: 26, sex: sex, name: name) { }

                    public class KayentatheriumWellesi : Tritylodontidae
                    {
                        public int NeonatesCountInNestClutch { get; } = 38;

                        public KayentatheriumWellesi(Creature? father = null, Creature? mother = null, SexType? sex = null, string? name = null)
                            : base(father, mother, sex, name ?? "Kayentatherium wellesi (Jurassic Maternal Cynodont)")
                        {
                            EstimatedBodyMassTonnes = 0.005f; // ~5 kg badger-sized
                            MaxLifespanYears = 12f;
                            if (father == null && mother == null) AgeYears = MaxLifespanYears * 0.25f;
                            Body.Tail = new Tail { LengthCm = 20f };
                        }

                        public string BroodMassiveNeonateClutch() => $"{Name} curls protective body around a clutch of 38 tiny neonates in an Early Jurassic subterranean nesting den, demonstrating the evolutionary transition to intensive mammalian maternal care!";
                        public string GrindHardJurassicSeeds() => $"{Name} grinds abrasive gymnosperm seeds using rows of multicuspid crescentic molar cusps with longitudinal power stroke!";
                    }
                }
            }
        }
    }
}
