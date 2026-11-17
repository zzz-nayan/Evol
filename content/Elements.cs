namespace Evol;

/// <summary>
/// Root master class for the entire chemical universe.
/// Every single inheritance relationship is strictly nested inside its parent class.
/// </summary>
public abstract class Element
{
    public enum PhaseOfMatter
    {
        Solid,
        Liquid,
        Gas,
        Plasma,
        Aqueous
    }

    public enum BondType
    {
        CovalentNonPolar,
        CovalentPolar,
        Ionic,
        Metallic,
        Hydrogen
    }

    public string Name { get; protected set; }
    public string Symbol { get; protected set; }
    public int AtomicNumber { get; protected set; }
    public float AtomicMassU { get; protected set; }
    public int Protons => AtomicNumber;
    public int Neutrons { get; protected set; }
    public int Electrons { get; protected set; }
    public int ValenceElectrons { get; protected set; }
    public int Period { get; protected set; }
    public int Group { get; protected set; }
    public float ElectronegativityPauling { get; protected set; }
    public float MeltingPointCelsius { get; protected set; }
    public float BoilingPointCelsius { get; protected set; }
    public float DensityGcm3 { get; protected set; }
    public PhaseOfMatter Phase { get; protected set; } = PhaseOfMatter.Solid;
    public float TemperatureCelsius { get; set; } = 20.0f;
    public float MassGrams { get; set; } = 1.0f;
    public bool IsRadioactive { get; protected set; } = false;

    public Element(
        string name = "Element",
        string symbol = "E",
        int atomicNumber = 0,
        float atomicMassU = 1.0f,
        int valenceElectrons = 0,
        int period = 1,
        int group = 1,
        float electronegativity = 0f,
        float meltingC = 0f,
        float boilingC = 100f,
        float density = 1.0f,
        PhaseOfMatter stpPhase = PhaseOfMatter.Solid)
    {
        Name = name;
        Symbol = symbol;
        AtomicNumber = atomicNumber;
        AtomicMassU = atomicMassU;
        Neutrons = atomicNumber > 0 ? (int)Math.Round(atomicMassU) - atomicNumber : 0;
        Electrons = atomicNumber;
        ValenceElectrons = valenceElectrons;
        Period = period;
        Group = group;
        ElectronegativityPauling = electronegativity;
        MeltingPointCelsius = meltingC;
        BoilingPointCelsius = boilingC;
        DensityGcm3 = density;
        Phase = stpPhase;
        MassGrams = atomicMassU;
    }

    public override string ToString() => $"[{Symbol}] {Name} (Z={AtomicNumber}, A={AtomicMassU:F2}u, Valence e⁻={ValenceElectrons})";

    // =========================================================================
    // BRANCH 1: NON-METALS (NESTED IN ELEMENT)
    // =========================================================================

    public abstract class NonMetal : Element
    {
        public bool IsElectricalInsulator { get; protected set; } = true;

        public NonMetal(string name, string symbol, int z, float mass, int valence, int period, int group, float en, float melt, float boil, float density, PhaseOfMatter phase)
            : base(name, symbol, z, mass, valence, period, group, en, melt, boil, density, phase)
        {
        }

        // --- SUBFAMILY 1.1: DIATOMIC NON-METALS (NESTED IN NONMETAL) ---
        public abstract class DiatomicNonMetal : NonMetal
        {
            public DiatomicNonMetal(string name, string symbol, int z, float mass, int valence, int period, int group, float en, float melt, float boil, float density)
                : base(name, symbol, z, mass, valence, period, group, en, melt, boil, density, PhaseOfMatter.Gas)
            {
            }

            // Concrete species nested directly in DiatomicNonMetal!
            public class Hydrogen : DiatomicNonMetal
            {
                public Hydrogen() : base("Hydrogen", "H", 1, 1.008f, 1, 1, 1, 2.20f, -259.16f, -252.87f, 0.00008988f) { }
            }

            public class Nitrogen : DiatomicNonMetal
            {
                public Nitrogen() : base("Nitrogen", "N", 7, 14.007f, 5, 2, 15, 3.04f, -210.0f, -195.79f, 0.0012506f) { }
            }

            public class Oxygen : DiatomicNonMetal
            {
                public Oxygen() : base("Oxygen", "O", 8, 15.999f, 6, 2, 16, 3.44f, -218.79f, -182.96f, 0.001429f) { }
            }
        }

        // --- SUBFAMILY 1.2: POLYATOMIC NON-METALS (NESTED IN NONMETAL) ---
        public abstract class PolyatomicNonMetal : NonMetal
        {
            public PolyatomicNonMetal(string name, string symbol, int z, float mass, int valence, int period, int group, float en, float melt, float boil, float density)
                : base(name, symbol, z, mass, valence, period, group, en, melt, boil, density, PhaseOfMatter.Solid)
            {
            }

            // Concrete species nested directly in PolyatomicNonMetal!
            public class Carbon : PolyatomicNonMetal
            {
                public Carbon() : base("Carbon", "C", 6, 12.011f, 4, 2, 14, 2.55f, 3550.0f, 4027.0f, 2.267f) { }
            }

            public class Phosphorus : PolyatomicNonMetal
            {
                public Phosphorus() : base("Phosphorus", "P", 15, 30.974f, 5, 3, 15, 2.19f, 44.15f, 280.5f, 1.823f) { }
            }

            public class Sulfur : PolyatomicNonMetal
            {
                public Sulfur() : base("Sulfur", "S", 16, 32.06f, 6, 3, 16, 2.58f, 115.21f, 444.6f, 2.07f) { }
            }
        }

        // --- SUBFAMILY 1.3: HALOGENS (NESTED IN NONMETAL) ---
        public abstract class Halogen : NonMetal
        {
            public Halogen(string name, string symbol, int z, float mass, int valence, int period, int group, float en, float melt, float boil, float density, PhaseOfMatter phase)
                : base(name, symbol, z, mass, valence, period, group, en, melt, boil, density, phase)
            {

            }

            public class Chlorine : Halogen
            {
                public Chlorine() : base("Chlorine", "Cl", 17, 35.45f, 7, 3, 17, 3.16f, -101.5f, -34.04f, 0.0032f, PhaseOfMatter.Gas) { }
            }
        }

        // --- SUBFAMILY 1.4: NOBLE GASES (NESTED IN NONMETAL) ---
        public abstract class NobleGas : NonMetal
        {
            public NobleGas(string name, string symbol, int z, float mass, int valence, int period, int group, float melt, float boil, float density)
                : base(name, symbol, z, mass, valence, period, group, 0f, melt, boil, density, PhaseOfMatter.Gas)
            {
            }

            public class Helium : NobleGas
            {
                public Helium() : base("Helium", "He", 2, 4.0026f, 2, 1, 18, -272.2f, -268.93f, 0.0001786f) { }
            }
        }
    }

    // =========================================================================
    // BRANCH 2: METALS (NESTED IN ELEMENT)
    // =========================================================================

    public abstract class Metal : Element
    {
        public float ElectricalConductivityMSm { get; protected set; }
        public float ThermalConductivityWmK { get; protected set; }
        public bool IsMalleable { get; protected set; } = true;
        public bool IsDuctile { get; protected set; } = true;

        public Metal(string name, string symbol, int z, float mass, int valence, int period, int group, float en, float melt, float boil, float density, float elecCond, float thermCond)
            : base(name, symbol, z, mass, valence, period, group, en, melt, boil, density, PhaseOfMatter.Solid)
        {
            ElectricalConductivityMSm = elecCond;
            ThermalConductivityWmK = thermCond;
        }

        // --- SUBFAMILY 2.1: ALKALI METALS (NESTED IN METAL) ---
        public abstract class AlkaliMetal : Metal
        {
            public AlkaliMetal(string name, string symbol, int z, float mass, int period, float en, float melt, float boil, float density, float elec, float therm)
                : base(name, symbol, z, mass, 1, period, 1, en, melt, boil, density, elec, therm)
            {
            }

            // Concrete species nested directly in AlkaliMetal!
            public class Sodium : AlkaliMetal
            {
                public Sodium() : base("Sodium", "Na", 11, 22.990f, 3, 0.93f, 97.79f, 882.9f, 0.968f, 21.0f, 142.0f) { }
            }

            public class Potassium : AlkaliMetal
            {
                public Potassium() : base("Potassium", "K", 19, 39.098f, 4, 0.82f, 63.5f, 759.0f, 0.89f, 14.0f, 102.4f) { }
            }
        }

        // --- SUBFAMILY 2.2: ALKALINE EARTH METALS (NESTED IN METAL) ---
        public abstract class AlkalineEarthMetal : Metal
        {
            public AlkalineEarthMetal(string name, string symbol, int z, float mass, int period, float en, float melt, float boil, float density, float elec, float therm)
                : base(name, symbol, z, mass, 2, period, 2, en, melt, boil, density, elec, therm)
            {
            }

            // Concrete species nested directly in AlkalineEarthMetal!
            public class Magnesium : AlkalineEarthMetal
            {
                public Magnesium() : base("Magnesium", "Mg", 12, 24.305f, 3, 1.31f, 650.0f, 1090.0f, 1.738f, 22.0f, 156.0f) { }
            }

            public class Calcium : AlkalineEarthMetal
            {
                public Calcium() : base("Calcium", "Ca", 20, 40.078f, 4, 1.00f, 842.0f, 1484.0f, 1.55f, 29.0f, 201.0f) { }
            }
        }

        // --- SUBFAMILY 2.3: TRANSITION METALS (NESTED IN METAL) ---
        public abstract class TransitionMetal : Metal
        {
            public TransitionMetal(string name, string symbol, int z, float mass, int valence, int period, int group, float en, float melt, float boil, float density, float elec, float therm)
                : base(name, symbol, z, mass, valence, period, group, en, melt, boil, density, elec, therm)
            {
            }

            // Concrete species nested directly in TransitionMetal!
            public class Iron : TransitionMetal
            {
                public Iron() : base("Iron", "Fe", 26, 55.845f, 2, 4, 8, 1.83f, 1538.0f, 2862.0f, 7.874f, 10.0f, 80.4f) { }
            }
        }
    }

    // =========================================================================
    // BRANCH 3: COMPOUNDS & MOLECULES (NESTED IN ELEMENT)
    // =========================================================================

    public abstract class Compound : Element
    {
        public string ChemicalFormula { get; protected set; }
        public float MolecularWeightU { get; protected set; }
        public BondType PrimaryBond { get; protected set; }
        public Dictionary<string, int> Stoichiometry { get; } = new();

        public Compound(string name, string formula, float molecularWeightU, BondType bond, PhaseOfMatter phase)
            : base(name, formula, atomicNumber: 0, molecularWeightU, valenceElectrons: 0, period: 0, group: 0, electronegativity: 0f, 0f, 100f, 1.0f, phase)
        {
            ChemicalFormula = formula;
            MolecularWeightU = molecularWeightU;
            PrimaryBond = bond;
            MassGrams = molecularWeightU;
        }

        protected void AddElement<T>(int count) where T : Element, new()
        {
            var elem = new T();
            Stoichiometry[elem.Symbol] = count;
        }

        public override string ToString() => $"{Name} ({ChemicalFormula}, MW={MolecularWeightU:F2} g/mol, Phase={Phase})";

        // --- SUBFAMILY 3.1: INORGANIC COMPOUNDS (NESTED IN COMPOUND) ---
        public abstract class InorganicCompound : Compound
        {
            public InorganicCompound(string name, string formula, float molecularWeightU, BondType bond, PhaseOfMatter phase)
                : base(name, formula, molecularWeightU, bond, phase)
            {
            }

            // Concrete inorganic molecules nested directly inside InorganicCompound!
            public class WaterMolecule : InorganicCompound
            {
                public WaterMolecule() : base("Water", "H2O", 18.015f, BondType.CovalentPolar, PhaseOfMatter.Liquid)
                {
                    AddElement<NonMetal.DiatomicNonMetal.Hydrogen>(2);
                    AddElement<NonMetal.DiatomicNonMetal.Oxygen>(1);
                }
            }

            public class CarbonDioxide : InorganicCompound
            {
                public CarbonDioxide() : base("Carbon Dioxide", "CO2", 44.01f, BondType.CovalentNonPolar, PhaseOfMatter.Gas)
                {
                    AddElement<NonMetal.PolyatomicNonMetal.Carbon>(1);
                    AddElement<NonMetal.DiatomicNonMetal.Oxygen>(2);
                }
            }

            public class MolecularOxygenGas : InorganicCompound
            {
                public MolecularOxygenGas() : base("Oxygen Gas", "O2", 31.998f, BondType.CovalentNonPolar, PhaseOfMatter.Gas)
                {
                    AddElement<NonMetal.DiatomicNonMetal.Oxygen>(2);
                }
            }

            public class MolecularNitrogenGas : InorganicCompound
            {
                public MolecularNitrogenGas() : base("Nitrogen Gas", "N2", 28.014f, BondType.CovalentNonPolar, PhaseOfMatter.Gas)
                {
                    AddElement<NonMetal.DiatomicNonMetal.Nitrogen>(2);
                }
            }

            public class MolecularHydrogenGas : InorganicCompound
            {
                public MolecularHydrogenGas() : base("Hydrogen Gas", "H2", 2.016f, BondType.CovalentNonPolar, PhaseOfMatter.Gas)
                {
                    AddElement<NonMetal.DiatomicNonMetal.Hydrogen>(2);
                }
            }

            public class SodiumChloride : InorganicCompound
            {
                public SodiumChloride() : base("Sodium Chloride (Table Salt)", "NaCl", 58.44f, BondType.Ionic, PhaseOfMatter.Solid)
                {
                    AddElement<Metal.AlkaliMetal.Sodium>(1);
                    AddElement<NonMetal.Halogen.Chlorine>(1);
                }
            }

            public class HydrochloricAcid : InorganicCompound
            {
                public HydrochloricAcid() : base("Hydrochloric Acid (Gastric Acid)", "HCl", 36.46f, BondType.CovalentPolar, PhaseOfMatter.Aqueous)
                {
                    AddElement<NonMetal.DiatomicNonMetal.Hydrogen>(1);
                    AddElement<NonMetal.Halogen.Chlorine>(1);
                }
            }

            public class SodiumHydroxide : InorganicCompound
            {
                public SodiumHydroxide() : base("Sodium Hydroxide (Caustic Soda)", "NaOH", 39.997f, BondType.Ionic, PhaseOfMatter.Solid)
                {
                    AddElement<Metal.AlkaliMetal.Sodium>(1);
                    AddElement<NonMetal.DiatomicNonMetal.Oxygen>(1);
                    AddElement<NonMetal.DiatomicNonMetal.Hydrogen>(1);
                }
            }

            public class CalciumCarbonate : InorganicCompound
            {
                public CalciumCarbonate() : base("Calcium Carbonate (Chalk/Limestone)", "CaCO3", 100.086f, BondType.Ionic, PhaseOfMatter.Solid)
                {
                    AddElement<Metal.AlkalineEarthMetal.Calcium>(1);
                    AddElement<NonMetal.PolyatomicNonMetal.Carbon>(1);
                    AddElement<NonMetal.DiatomicNonMetal.Oxygen>(3);
                }
            }

            public class CalciumChloride : InorganicCompound
            {
                public CalciumChloride() : base("Calcium Chloride", "CaCl2", 110.98f, BondType.Ionic, PhaseOfMatter.Solid)
                {
                    AddElement<Metal.AlkalineEarthMetal.Calcium>(1);
                    AddElement<NonMetal.Halogen.Chlorine>(2);
                }
            }

            public class FerricOxide : InorganicCompound
            {
                public FerricOxide() : base("Ferric Oxide (Rust)", "Fe2O3", 159.69f, BondType.Ionic, PhaseOfMatter.Solid)
                {
                    AddElement<Metal.TransitionMetal.Iron>(2);
                    AddElement<NonMetal.DiatomicNonMetal.Oxygen>(3);
                }
            }

            public class MagnesiumOxide : InorganicCompound
            {
                public MagnesiumOxide() : base("Magnesium Oxide", "MgO", 40.304f, BondType.Ionic, PhaseOfMatter.Solid)
                {
                    AddElement<Metal.AlkalineEarthMetal.Magnesium>(1);
                    AddElement<NonMetal.DiatomicNonMetal.Oxygen>(1);
                }
            }
        }

        // --- SUBFAMILY 3.2: ORGANIC COMPOUNDS (NESTED IN COMPOUND) ---
        public abstract class OrganicCompound : Compound
        {
            public OrganicCompound(string name, string formula, float molecularWeightU, BondType bond, PhaseOfMatter phase)
                : base(name, formula, molecularWeightU, bond, phase)
            {
            }

            // Concrete organic bio-molecules nested directly inside OrganicCompound!
            public class Glucose : OrganicCompound
            {
                public float CaloricEnergyKcal => 3.75f;

                public Glucose() : base("D-Glucose (Dextrose)", "C6H12O6", 180.156f, BondType.CovalentPolar, PhaseOfMatter.Solid)
                {
                    AddElement<NonMetal.PolyatomicNonMetal.Carbon>(6);
                    AddElement<NonMetal.DiatomicNonMetal.Hydrogen>(12);
                    AddElement<NonMetal.DiatomicNonMetal.Oxygen>(6);
                }
            }

            public class AdenosineTriphosphate : OrganicCompound
            {
                public float UsableEnergyKjPerMol => 30.5f;

                public AdenosineTriphosphate() : base("Adenosine Triphosphate (ATP)", "C10H16N5O13P3", 507.18f, BondType.CovalentPolar, PhaseOfMatter.Aqueous)
                {
                    AddElement<NonMetal.PolyatomicNonMetal.Carbon>(10);
                    AddElement<NonMetal.DiatomicNonMetal.Hydrogen>(16);
                    AddElement<NonMetal.DiatomicNonMetal.Nitrogen>(5);
                    AddElement<NonMetal.DiatomicNonMetal.Oxygen>(13);
                    AddElement<NonMetal.PolyatomicNonMetal.Phosphorus>(3);
                }
            }

            public class ChlorophyllPigment : OrganicCompound
            {
                public string CentralMetalCation => "Mg2+ (Magnesium)";

                public ChlorophyllPigment() : base("Chlorophyll a", "C55H72MgN4O5", 893.5f, BondType.CovalentPolar, PhaseOfMatter.Solid)
                {
                    AddElement<NonMetal.PolyatomicNonMetal.Carbon>(55);
                    AddElement<NonMetal.DiatomicNonMetal.Hydrogen>(72);
                    AddElement<Metal.AlkalineEarthMetal.Magnesium>(1);
                    AddElement<NonMetal.DiatomicNonMetal.Nitrogen>(4);
                    AddElement<NonMetal.DiatomicNonMetal.Oxygen>(5);
                }
            }

            public class HemoglobinProtein : OrganicCompound
            {
                public string ActiveBindingCore => "Fe2+ (Heme Iron)";
                public int OxygenBindingSites => 4;

                public HemoglobinProtein() : base("Hemoglobin", "C2952H4664Fe4N812O832S8", 64500f, BondType.CovalentPolar, PhaseOfMatter.Aqueous)
                {
                    AddElement<Metal.TransitionMetal.Iron>(4);
                    AddElement<NonMetal.PolyatomicNonMetal.Carbon>(2952);
                    AddElement<NonMetal.DiatomicNonMetal.Nitrogen>(812);
                    AddElement<NonMetal.DiatomicNonMetal.Oxygen>(832);
                    AddElement<NonMetal.PolyatomicNonMetal.Sulfur>(8);
                }
            }
        }
    }

    // =========================================================================
    // BRANCH 4: REACTION ENGINE (NESTED IN ELEMENT)
    // =========================================================================

    public class ReactionParticipant
    {
        public Type SubstanceType { get; }
        public string Name { get; }
        public int MolarCoefficient { get; }

        public ReactionParticipant(Type substanceType, string name, int coefficient)
        {
            SubstanceType = substanceType;
            Name = name;
            MolarCoefficient = coefficient;
        }
    }

    public class ReactionResult
    {
        public bool Success { get; set; }
        public string Summary { get; set; } = string.Empty;
        public float HeatReleasedJoules { get; set; }
        public List<Element> ProducedSubstances { get; } = new();
    }

    public abstract class Reaction
    {
        public string ReactionName { get; protected set; }
        public string EquationString { get; protected set; }
        public List<ReactionParticipant> Reactants { get; } = new();
        public List<ReactionParticipant> Products { get; } = new();
        public float EnthalpyDeltaKjPerMol { get; protected set; }
        public float ActivationEnergyKj { get; protected set; }
        public string? Catalyst { get; protected set; }

        public bool IsExothermic => EnthalpyDeltaKjPerMol < 0f;
        public bool IsEndothermic => EnthalpyDeltaKjPerMol > 0f;

        public Reaction(string name, string equation, float enthalpyDeltaKj, float activationEnergyKj = 10f, string? catalyst = null)
        {
            ReactionName = name;
            EquationString = equation;
            EnthalpyDeltaKjPerMol = enthalpyDeltaKj;
            ActivationEnergyKj = activationEnergyKj;
            Catalyst = catalyst;
        }

        protected void AddReactant<T>(int coefficient) where T : Element, new()
        {
            Reactants.Add(new ReactionParticipant(typeof(T), new T().Name, coefficient));
        }

        protected void AddProduct<T>(int coefficient) where T : Element, new()
        {
            Products.Add(new ReactionParticipant(typeof(T), new T().Name, coefficient));
        }

        public abstract ReactionResult React(float reactantMoles = 1.0f);

        // --- SUBFAMILY 4.1: SYNTHESIS (NESTED IN REACTION) ---
        public abstract class SynthesisReaction : Reaction
        {
            public SynthesisReaction(string name, string equation, float enthalpyDeltaKj, float activationEnergyKj = 10f, string? catalyst = null)
                : base(name, equation, enthalpyDeltaKj, activationEnergyKj, catalyst)
            {
            }

            public class WaterSynthesisReaction : SynthesisReaction
            {
                public WaterSynthesisReaction()
                    : base("Water Synthesis (Oxyhydrogen Combustion)", "2 H2 + O2 -> 2 H2O", enthalpyDeltaKj: -285.8f, activationEnergyKj: 25f)
                {
                    AddReactant<Compound.InorganicCompound.MolecularHydrogenGas>(2);
                    AddReactant<Compound.InorganicCompound.MolecularOxygenGas>(1);
                    AddProduct<Compound.InorganicCompound.WaterMolecule>(2);
                }

                public override ReactionResult React(float reactantMoles = 1.0f)
                {
                    float molesWaterProduced = reactantMoles;
                    var water = new Compound.InorganicCompound.WaterMolecule { MassGrams = molesWaterProduced * 18.015f };
                    var res = new ReactionResult
                    {
                        Success = true,
                        HeatReleasedJoules = molesWaterProduced * 285800f,
                        Summary = $"Synthesized {molesWaterProduced:F2} moles of {water.Name} ({water.MassGrams:F1}g) releasing {molesWaterProduced * 285.8f:F1} kJ of thermal energy!"
                    };
                    res.ProducedSubstances.Add(water);
                    return res;
                }
            }

            public class RustFormationReaction : SynthesisReaction
            {
                public RustFormationReaction()
                    : base("Iron Oxidation (Rust Synthesis)", "4 Fe + 3 O2 -> 2 Fe2O3", enthalpyDeltaKj: -824.2f, activationEnergyKj: 15f)
                {
                    AddReactant<Metal.TransitionMetal.Iron>(4);
                    AddReactant<Compound.InorganicCompound.MolecularOxygenGas>(3);
                    AddProduct<Compound.InorganicCompound.FerricOxide>(2);
                }

                public override ReactionResult React(float reactantMoles = 1.0f)
                {
                    float molesRust = reactantMoles * 0.5f;
                    var rust = new Compound.InorganicCompound.FerricOxide { MassGrams = molesRust * 159.69f };
                    var res = new ReactionResult
                    {
                        Success = true,
                        HeatReleasedJoules = molesRust * 824200f,
                        Summary = $"Oxidized metallic Iron into {rust.Name} ({rust.MassGrams:F1}g of flaky orange-brown rust)!"
                    };
                    res.ProducedSubstances.Add(rust);
                    return res;
                }
            }

            public class MagnesiumOxidationReaction : SynthesisReaction
            {
                public MagnesiumOxidationReaction()
                    : base("Magnesium Flash Oxidation", "2 Mg + O2 -> 2 MgO", enthalpyDeltaKj: -601.7f, activationEnergyKj: 50f)
                {
                    AddReactant<Metal.AlkalineEarthMetal.Magnesium>(2);
                    AddReactant<Compound.InorganicCompound.MolecularOxygenGas>(1);
                    AddProduct<Compound.InorganicCompound.MagnesiumOxide>(2);
                }

                public override ReactionResult React(float reactantMoles = 1.0f)
                {
                    var mgo = new Compound.InorganicCompound.MagnesiumOxide { MassGrams = reactantMoles * 40.304f };
                    return new ReactionResult
                    {
                        Success = true,
                        HeatReleasedJoules = reactantMoles * 601700f,
                        Summary = $"Ignited Magnesium ribbon producing blinding white flash and {mgo.Name} ({mgo.MassGrams:F1}g)!"
                    };
                }
            }
        }

        // --- SUBFAMILY 4.2: DECOMPOSITION (NESTED IN REACTION) ---
        public abstract class DecompositionReaction : Reaction
        {
            public DecompositionReaction(string name, string equation, float enthalpyDeltaKj, float activationEnergyKj = 20f, string? catalyst = null)
                : base(name, equation, enthalpyDeltaKj, activationEnergyKj, catalyst)
            {
            }

            public class WaterElectrolysisReaction : DecompositionReaction
            {
                public WaterElectrolysisReaction()
                    : base("Water Electrolysis", "2 H2O -> 2 H2 + O2", enthalpyDeltaKj: 285.8f, activationEnergyKj: 120f)
                {
                    AddReactant<Compound.InorganicCompound.WaterMolecule>(2);
                    AddProduct<Compound.InorganicCompound.MolecularHydrogenGas>(2);
                    AddProduct<Compound.InorganicCompound.MolecularOxygenGas>(1);
                }

                public override ReactionResult React(float reactantMoles = 1.0f)
                {
                    var h2 = new Compound.InorganicCompound.MolecularHydrogenGas { MassGrams = reactantMoles * 2.016f };
                    var o2 = new Compound.InorganicCompound.MolecularOxygenGas { MassGrams = (reactantMoles / 2f) * 31.998f };
                    var res = new ReactionResult
                    {
                        Success = true,
                        HeatReleasedJoules = -reactantMoles * 285800f,
                        Summary = $"Electrolyzed water into {h2.MassGrams:F2}g Hydrogen gas and {o2.MassGrams:F2}g pure Oxygen gas!"
                    };
                    res.ProducedSubstances.Add(h2);
                    res.ProducedSubstances.Add(o2);
                    return res;
                }
            }

            public class LimestoneThermalDecomposition : DecompositionReaction
            {
                public LimestoneThermalDecomposition()
                    : base("Calcium Carbonate Calcination", "CaCO3 -> CaO + CO2", enthalpyDeltaKj: 178.0f, activationEnergyKj: 80f)
                {
                    AddReactant<Compound.InorganicCompound.CalciumCarbonate>(1);
                    AddProduct<Compound.InorganicCompound.CarbonDioxide>(1);
                }

                public override ReactionResult React(float reactantMoles = 1.0f)
                {
                    var co2 = new Compound.InorganicCompound.CarbonDioxide { MassGrams = reactantMoles * 44.01f };
                    var res = new ReactionResult
                    {
                        Success = true,
                        HeatReleasedJoules = -reactantMoles * 178000f,
                        Summary = $"Thermal decomposition released {co2.MassGrams:F1}g of gaseous Carbon Dioxide!"
                    };
                    res.ProducedSubstances.Add(co2);
                    return res;
                }
            }
        }

        // --- SUBFAMILY 4.3: SINGLE DISPLACEMENT (NESTED IN REACTION) ---
        public abstract class SingleDisplacementReaction : Reaction
        {
            public SingleDisplacementReaction(string name, string equation, float enthalpyDeltaKj, float activationEnergyKj = 10f)
                : base(name, equation, enthalpyDeltaKj, activationEnergyKj)
            {
            }

            public class SodiumInWaterReaction : SingleDisplacementReaction
            {
                public SodiumInWaterReaction()
                    : base("Alkali Metal Water Displacement", "2 Na + 2 H2O -> 2 NaOH + H2", enthalpyDeltaKj: -368.0f, activationEnergyKj: 5f)
                {
                    AddReactant<Metal.AlkaliMetal.Sodium>(2);
                    AddReactant<Compound.InorganicCompound.WaterMolecule>(2);
                    AddProduct<Compound.InorganicCompound.SodiumHydroxide>(2);
                    AddProduct<Compound.InorganicCompound.MolecularHydrogenGas>(1);
                }

                public override ReactionResult React(float reactantMoles = 1.0f)
                {
                    var naoh = new Compound.InorganicCompound.SodiumHydroxide { MassGrams = reactantMoles * 39.997f };
                    var h2 = new Compound.InorganicCompound.MolecularHydrogenGas { MassGrams = (reactantMoles / 2f) * 2.016f };
                    var res = new ReactionResult
                    {
                        Success = true,
                        HeatReleasedJoules = reactantMoles * 368000f,
                        Summary = $"*BOOM!* Metallic Sodium violently displaced Hydrogen from water, creating alkaline {naoh.Name} and combustible {h2.Name}!"
                    };
                    res.ProducedSubstances.Add(naoh);
                    res.ProducedSubstances.Add(h2);
                    return res;
                }
            }

            public class IronAcidDisplacementReaction : SingleDisplacementReaction
            {
                public IronAcidDisplacementReaction()
                    : base("Iron-Acid Hydrogen Displacement", "Fe + 2 HCl -> FeCl2 + H2", enthalpyDeltaKj: -87.9f, activationEnergyKj: 15f)
                {
                    AddReactant<Metal.TransitionMetal.Iron>(1);
                    AddReactant<Compound.InorganicCompound.HydrochloricAcid>(2);
                    AddProduct<Compound.InorganicCompound.MolecularHydrogenGas>(1);
                }

                public override ReactionResult React(float reactantMoles = 1.0f)
                {
                    var h2 = new Compound.InorganicCompound.MolecularHydrogenGas { MassGrams = reactantMoles * 2.016f };
                    var res = new ReactionResult
                    {
                        Success = true,
                        HeatReleasedJoules = reactantMoles * 87900f,
                        Summary = $"Gastric acid attacked metallic iron, bubbling off {h2.MassGrams:F2}g of Hydrogen gas!"
                    };
                    res.ProducedSubstances.Add(h2);
                    return res;
                }
            }
        }

        // --- SUBFAMILY 4.4: DOUBLE DISPLACEMENT (NESTED IN REACTION) ---
        public abstract class DoubleDisplacementReaction : Reaction
        {
            public DoubleDisplacementReaction(string name, string equation, float enthalpyDeltaKj, float activationEnergyKj = 5f)
                : base(name, equation, enthalpyDeltaKj, activationEnergyKj)
            {
            }

            public class AcidBaseNeutralizationReaction : DoubleDisplacementReaction
            {
                public AcidBaseNeutralizationReaction()
                    : base("Acid-Base Neutralization", "HCl + NaOH -> NaCl + H2O", enthalpyDeltaKj: -57.3f, activationEnergyKj: 2f)
                {
                    AddReactant<Compound.InorganicCompound.HydrochloricAcid>(1);
                    AddReactant<Compound.InorganicCompound.SodiumHydroxide>(1);
                    AddProduct<Compound.InorganicCompound.SodiumChloride>(1);
                    AddProduct<Compound.InorganicCompound.WaterMolecule>(1);
                }

                public override ReactionResult React(float reactantMoles = 1.0f)
                {
                    var salt = new Compound.InorganicCompound.SodiumChloride { MassGrams = reactantMoles * 58.44f };
                    var water = new Compound.InorganicCompound.WaterMolecule { MassGrams = reactantMoles * 18.015f };
                    var res = new ReactionResult
                    {
                        Success = true,
                        HeatReleasedJoules = reactantMoles * 57300f,
                        Summary = $"Strong acid (HCl) neutralized strong base (NaOH), producing harmless {salt.Name} ({salt.MassGrams:F1}g) and pure Water!"
                    };
                    res.ProducedSubstances.Add(salt);
                    res.ProducedSubstances.Add(water);
                    return res;
                }
            }

            public class CalciumPrecipitationReaction : DoubleDisplacementReaction
            {
                public CalciumPrecipitationReaction()
                    : base("Calcium Carbonate Mineral Precipitation", "CaCl2 + Na2CO3 -> CaCO3 (s) + 2 NaCl", enthalpyDeltaKj: -12.5f, activationEnergyKj: 5f)
                {
                    AddReactant<Compound.InorganicCompound.CalciumChloride>(1);
                    AddProduct<Compound.InorganicCompound.CalciumCarbonate>(1);
                    AddProduct<Compound.InorganicCompound.SodiumChloride>(2);
                }

                public override ReactionResult React(float reactantMoles = 1.0f)
                {
                    var chalk = new Compound.InorganicCompound.CalciumCarbonate { MassGrams = reactantMoles * 100.086f };
                    var salt = new Compound.InorganicCompound.SodiumChloride { MassGrams = reactantMoles * 2f * 58.44f };
                    var res = new ReactionResult
                    {
                        Success = true,
                        HeatReleasedJoules = reactantMoles * 12500f,
                        Summary = $"Dissolved calcium ions precipitated out as solid white {chalk.Name} ({chalk.MassGrams:F1}g, bone/shell mineral)!"
                    };
                    res.ProducedSubstances.Add(chalk);
                    res.ProducedSubstances.Add(salt);
                    return res;
                }
            }
        }

        // --- SUBFAMILY 4.5: COMBUSTION (NESTED IN REACTION) ---
        public abstract class CombustionReaction : Reaction
        {
            public CombustionReaction(string name, string equation, float enthalpyDeltaKj, float activationEnergyKj = 40f)
                : base(name, equation, enthalpyDeltaKj, activationEnergyKj)
            {
            }

            public class GlucoseCombustionReaction : CombustionReaction
            {
                public GlucoseCombustionReaction()
                    : base("Complete Glucose Combustion", "C6H12O6 + 6 O2 -> 6 CO2 + 6 H2O", enthalpyDeltaKj: -2805.0f, activationEnergyKj: 30f)
                {
                    AddReactant<Compound.OrganicCompound.Glucose>(1);
                    AddReactant<Compound.InorganicCompound.MolecularOxygenGas>(6);
                    AddProduct<Compound.InorganicCompound.CarbonDioxide>(6);
                    AddProduct<Compound.InorganicCompound.WaterMolecule>(6);
                }

                public override ReactionResult React(float reactantMoles = 1.0f)
                {
                    var co2 = new Compound.InorganicCompound.CarbonDioxide { MassGrams = reactantMoles * 6f * 44.01f };
                    var water = new Compound.InorganicCompound.WaterMolecule { MassGrams = reactantMoles * 6f * 18.015f };
                    var res = new ReactionResult
                    {
                        Success = true,
                        HeatReleasedJoules = reactantMoles * 2805000f,
                        Summary = $"Burned {reactantMoles * 180.156f:F1}g of Glucose, releasing massive heat ({reactantMoles * 2805f:F0} kJ) with {co2.MassGrams:F1}g CO2 and {water.MassGrams:F1}g H2O!"
                    };
                    res.ProducedSubstances.Add(co2);
                    res.ProducedSubstances.Add(water);
                    return res;
                }
            }
        }

        // --- SUBFAMILY 4.6: BIOCHEMICAL (NESTED IN REACTION) ---
        public abstract class BiochemicalReaction : Reaction
        {
            public string CellularOrganelle { get; protected set; }

            public BiochemicalReaction(string name, string equation, float enthalpyDeltaKj, string organelle, string catalyst)
                : base(name, equation, enthalpyDeltaKj, activationEnergyKj: 15f, catalyst)
            {
                CellularOrganelle = organelle;
            }

            public class PhotosynthesisReaction : BiochemicalReaction
            {
                public PhotosynthesisReaction()
                    : base(
                        "Photosynthesis (Light & Calvin Cycle)",
                        "6 CO2 + 6 H2O + Solar Energy -> C6H12O6 + 6 O2",
                        enthalpyDeltaKj: 2805.0f,
                        organelle: "Chloroplast (Thylakoid Membrane)",
                        catalyst: "Chlorophyll a (Mg2+ porphyrin core)")
                {
                    AddReactant<Compound.InorganicCompound.CarbonDioxide>(6);
                    AddReactant<Compound.InorganicCompound.WaterMolecule>(6);
                    AddProduct<Compound.OrganicCompound.Glucose>(1);
                    AddProduct<Compound.InorganicCompound.MolecularOxygenGas>(6);
                }

                public override ReactionResult React(float reactantMoles = 1.0f)
                {
                    float glucoseMoles = reactantMoles / 6.0f;
                    var glucose = new Compound.OrganicCompound.Glucose { MassGrams = glucoseMoles * 180.156f };
                    var o2 = new Compound.InorganicCompound.MolecularOxygenGas { MassGrams = reactantMoles * 31.998f };

                    var res = new ReactionResult
                    {
                        Success = true,
                        HeatReleasedJoules = -reactantMoles * (2805000f / 6f),
                        Summary = $"[Photosynthesis] Chlorophyll trapped sunlight: consumed {reactantMoles * 44.01f:F1}g CO2 and generated {glucose.MassGrams:F1}g Glucose sugar + {o2.MassGrams:F1}g pure O2 gas!"
                    };
                    res.ProducedSubstances.Add(glucose);
                    res.ProducedSubstances.Add(o2);
                    return res;
                }
            }

            public class CellularRespirationReaction : BiochemicalReaction
            {
                public CellularRespirationReaction()
                    : base(
                        "Aerobic Cellular Respiration",
                        "C6H12O6 + 6 O2 -> 6 CO2 + 6 H2O + 38 ATP",
                        enthalpyDeltaKj: -2805.0f,
                        organelle: "Mitochondria (Citric Acid Cycle & Electron Transport Chain)",
                        catalyst: "Cytochrome c oxidase (Fe/Cu complex)")
                {
                    AddReactant<Compound.OrganicCompound.Glucose>(1);
                    AddReactant<Compound.InorganicCompound.MolecularOxygenGas>(6);
                    AddProduct<Compound.InorganicCompound.CarbonDioxide>(6);
                    AddProduct<Compound.InorganicCompound.WaterMolecule>(6);
                    AddProduct<Compound.OrganicCompound.AdenosineTriphosphate>(38);
                }

                public override ReactionResult React(float reactantMoles = 1.0f)
                {
                    float glucoseMoles = reactantMoles;
                    var atp = new Compound.OrganicCompound.AdenosineTriphosphate { MassGrams = glucoseMoles * 38f * 507.18f };
                    var co2 = new Compound.InorganicCompound.CarbonDioxide { MassGrams = glucoseMoles * 6f * 44.01f };
                    var water = new Compound.InorganicCompound.WaterMolecule { MassGrams = glucoseMoles * 6f * 18.015f };

                    var res = new ReactionResult
                    {
                        Success = true,
                        HeatReleasedJoules = glucoseMoles * 2805000f,
                        Summary = $"[Cellular Respiration] Mitochondria metabolized {glucoseMoles * 180.156f:F1}g Glucose with inhaled O2: produced {glucoseMoles * 38:F0} moles of ATP energy, exhaling {co2.MassGrams:F1}g CO2 and {water.MassGrams:F1}g moisture!"
                    };
                    res.ProducedSubstances.Add(atp);
                    res.ProducedSubstances.Add(co2);
                    res.ProducedSubstances.Add(water);
                    return res;
                }
            }
        }
    }

    // =========================================================================
    // BRANCH 5: REACTION VESSEL (NESTED IN ELEMENT)
    // =========================================================================

    public class ReactionVessel
    {
        public string Name { get; set; } = "Chemical Reactor";
        public float TemperatureCelsius { get; set; } = 25.0f;
        public float PressureAtm { get; set; } = 1.0f;
        public Dictionary<Type, float> Moles { get; } = new();

        public void AddMoles<T>(float moles) where T : Element, new()
        {
            var type = typeof(T);
            Moles[type] = Moles.GetValueOrDefault(type, 0f) + moles;
        }

        public float GetMoles<T>() where T : Element => Moles.GetValueOrDefault(typeof(T), 0f);

        public ReactionResult TriggerReaction<TReaction>(float extentMoles = 1.0f) where TReaction : Reaction, new()
        {
            var reaction = new TReaction();

            // Verify reactants are present
            foreach (var reactant in reaction.Reactants)
            {
                float requiredMoles = reactant.MolarCoefficient * extentMoles;
                float available = Moles.GetValueOrDefault(reactant.SubstanceType, 0f);
                if (available < requiredMoles)
                {
                    return new ReactionResult
                    {
                        Success = false,
                        Summary = $"Cannot execute {reaction.ReactionName}: Insufficient {reactant.Name} (Need {requiredMoles:F2} mol, have {available:F2} mol)"
                    };
                }
            }

            // Consume reactants
            foreach (var reactant in reaction.Reactants)
            {
                float requiredMoles = reactant.MolarCoefficient * extentMoles;
                Moles[reactant.SubstanceType] -= requiredMoles;
            }

            // Execute reaction
            var result = reaction.React(extentMoles);

            // Add products
            foreach (var product in reaction.Products)
            {
                float producedMoles = product.MolarCoefficient * extentMoles;
                Moles[product.SubstanceType] = Moles.GetValueOrDefault(product.SubstanceType, 0f) + producedMoles;
            }

            // Thermal enthalpy exchange
            if (result.HeatReleasedJoules != 0f)
            {
                TemperatureCelsius += result.HeatReleasedJoules / 5000f;
            }

            return result;
        }
    }

    /// <summary>
    /// Plural alias nested inside the master class so both Element and Element.Elements work seamlessly!
    /// </summary>
    public abstract class Elements : Element
    {
    }
}
