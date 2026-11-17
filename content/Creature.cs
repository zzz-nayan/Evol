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

        public class Strigiformes : Bird // Owls
        {
            public Strigiformes(Bird? father = null, Bird? mother = null, string? species = "Barn Owl", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class BarnOwl : Strigiformes
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

        public class Falconiformes : Bird // Falcons
        {
            public Falconiformes(Bird? father = null, Bird? mother = null, string? species = "Peregrine Falcon", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class PeregrineFalcon : Falconiformes
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

        public class Sphenisciformes : Bird // Penguins
        {
            public Sphenisciformes(Bird? father = null, Bird? mother = null, string? species = "Emperor Penguin", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class EmperorPenguin : Sphenisciformes
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

        public class Psittaciformes : Bird // Parrots
        {
            public Psittaciformes(Bird? father = null, Bird? mother = null, string? species = "Scarlet Macaw", SexType? sex = null)
                : base(father, mother, species, sex) { }

            public class ScarletMacaw : Psittaciformes
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
                public Lamnidae(Fish? father = null, Fish? mother = null, string? species = "Great White Shark", SexType? sex = null)
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
        }

        // 2. Osteichthyes (Bony Fish)
        public abstract class Osteichthyes : Fish
        {
            public bool HasSwimBladder { get; } = true;

            public Osteichthyes(Fish? father = null, Fish? mother = null, string? species = "Bony Fish", SexType? sex = null)
                : base(father, mother, species, sex) { }

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

            public class Pomacentridae : Osteichthyes
            {
                public Pomacentridae(Fish? father = null, Fish? mother = null, string? species = "Clownfish", SexType? sex = null)
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
                }

                // Family 2: Elapidae (Cobras & Mambas - Neurotoxic Venom)
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
                        victim.TakeDamage(60f, DamageType.Poison);
                        victim.Body.Heart.BeatsPerMinute = Math.Max(10f, victim.Body.Heart.BeatsPerMinute * 0.4f);
                        return true;
                    }

                    public class KingCobra : Elapidae
                    {
                        public KingCobra(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "King Cobra", sex) { }
                    }

                    public class BlackMamba : Elapidae
                    {
                        public float SprintSpeedKmh { get; } = 20f;
                        public BlackMamba(Creature? father = null, Creature? mother = null, string? name = null, SexType? sex = null)
                            : base(father, mother, name ?? "Black Mamba", sex)
                        {
                            VenomYieldMg = 280f;
                        }
                    }
                }

                // Family 3: Boidae (Boas & Anacondas - Non-venomous Apex Constrictors)
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
                            : base(father, mother, name ?? "Giant Green Anaconda", sex)
                        {
                            Body.Tail = new Tail { LengthCm = 650f };
                            ConstrictionPressurePsi = 16.5f;
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
                            : base(father, mother, name ?? "Apex Komodo Dragon", sex)
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
                            : base(father, mother, name ?? "Veiled Chameleon", sex) { }

                        public string Camouflage(string ambientColorHex)
                        {
                            SkinColorHex = ambientColorHex;
                            return $"{Name} shifts dermal iridophore nanocrystals, camouflaging into {ambientColorHex}!";
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
                            : base(father, mother, name ?? "Tokay Gecko", sex) { }

                        public string WalkOnCeiling() => $"{Name} utilizes microscopic toe spatulae and van der Waals attraction to run inverted along ceilings!";
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
            }

            public class Cheloniidae : Testudines // Sea Turtles
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
            }

            // ORDER 5: ODONATA (DRAGONFLIES - NESTED IN INSECTA)
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
        }
    }

    // =========================================================================
    // --- 9. PROBOSCIDEA (ELEPHANTS: AFRICAN BUSH ELEPHANT)
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
        }
    }

    // =========================================================================
    // --- 10. CETACEA (MARINE MAMMALS: BLUE WHALE, ORCA)
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
            }
        }
    }
}
