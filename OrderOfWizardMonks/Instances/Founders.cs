using System;
using System.Collections.Generic;
using WizardMonks.Activities;
using WizardMonks.Core;
using WizardMonks.Decisions.Goals;
using WizardMonks.Models;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Ideas;
using WizardMonks.Models.Laboratories;
using WizardMonks.Models.Projects;
using WizardMonks.Models.Spells;
using WizardMonks.Models.Traditions;
using WizardMonks.Services.Characters;

namespace WizardMonks.Instances
{
    public static class Founders
    {
        public static HermeticMagus Bjornaer { get; private set; }
        public static HermeticMagus Bonisgaus { get; private set; }
        public static HermeticMagus Criamon { get; private set; }
        public static HermeticMagus Diedne { get; private set; }
        public static HermeticMagus Flambeau { get; private set; }
        public static HermeticMagus Guernicus { get; private set; }
        public static HermeticMagus Jerbiton { get; private set; }
        public static HermeticMagus Mercere { get; private set; }
        public static HermeticMagus Merinita { get; private set; }
        public static HermeticMagus Tremere { get; private set; }
        public static HermeticMagus Tytalus { get; private set; }
        public static HermeticMagus Verditius { get; private set; }

        public static IEnumerable<HermeticMagus> GetEnumerator()
        {
            //yield return Bjornaer;
            yield return Bonisgaus;
            /*yield return Criamon;
            yield return Diedne;
            yield return Flambeau;
            yield return Guernicus;
            yield return Jerbiton;
            yield return Mercere;
            yield return Merinita;
            yield return Tremere;
            yield return Tytalus;
            yield return Verditius;*/
        }

        static Founders()
        {
            // Bonisagus must be built first — all other Founders clone from his tradition.
            BuildBonisagus();
            /*BuildBjornaer();
            BuildCriamon();
            BuildDiedne();
            BuildFlambeau();
            BuildGuernicus();
            BuildJerbiton();
            BuildMercere();
            BuildMerinita();
            BuildTremere();
            BuildTytalus();
            BuildVerditius();*/
        }

        /// <summary>
        /// Builds Bonisagus's MagicalTradition — the baseline Hermetic tradition
        /// from which all other Founders' traditions are cloned via OpenGift.
        ///
        /// Ranges, Durations, Targets, and SpellBases represent what Bonisagus
        /// had formalized by the time he began Opening the Founders' Arts (~754 AD).
        /// This is intentionally modest: full Hermetic theory as of 767 was the
        /// product of incorporating the Founders' own traditions, not a prerequisite
        /// to it. Concepts seeded from each Founder's tradition will expand it
        /// over the course of the founding simulation.
        ///
        /// TraditionActivityFormulas cover the activities whose totals differ
        /// structurally from the standard GetLabTotal fallback. Standard lab
        /// activities (InventSpells, LongevityRitual, etc.) use the fallback.
        /// </summary>
        public static void BuildBonisagus()
        {
            // ----------------------------------------------------------------
            // Step 1: Build the MagicalTradition
            // ----------------------------------------------------------------
            var concepts = new List<TraditionConcept>();

            // Ranges — Bonisagus's own tradition; Eye/Sight/Arcane come from other Founders.
            concepts.Add(new TraditionConcept(new RangePrinciple(EffectRanges.Personal)));
            concepts.Add(new TraditionConcept(new RangePrinciple(EffectRanges.Touch)));
            concepts.Add(new TraditionConcept(new RangePrinciple(EffectRanges.Voice)));

            // Durations — Concentration/Ring/Moon come from other Founders.
            concepts.Add(new TraditionConcept(new DurationPrinciple(EffectDurations.Instant)));
            concepts.Add(new TraditionConcept(new DurationPrinciple(EffectDurations.Diameter)));
            concepts.Add(new TraditionConcept(new DurationPrinciple(EffectDurations.Sun)));
            concepts.Add(new TraditionConcept(new DurationPrinciple(EffectDurations.Year)));

            // Targets — Part and above come from other Founders.
            concepts.Add(new TraditionConcept(new TargetPrinciple(EffectTargets.Individual)));

            // Hermetic Magical Abilities — native, no research needed
            concepts.Add(new TraditionConcept(new MagicalAbilityPrinciple(Abilities.MagicTheory)));
            concepts.Add(new TraditionConcept(new MagicalAbilityPrinciple(Abilities.ParmaMagica)));

            // Standard Lab Activities — native Hermetic operations
            concepts.Add(new TraditionConcept(new LabActivityPrinciple(Activity.InventSpells)));
            concepts.Add(new TraditionConcept(new LabActivityPrinciple(Activity.DistillVis)));
            concepts.Add(new TraditionConcept(new LabActivityPrinciple(Activity.StudyVis)));
            concepts.Add(new TraditionConcept(new LabActivityPrinciple(Activity.OpenArts)));
            concepts.Add(new TraditionConcept(new LabActivityPrinciple(Activity.OriginalResearch)));
            concepts.Add(new TraditionConcept(new LabActivityPrinciple(Activity.WriteLabText)));

            // SpellBases — populated from the static SpellBases registry
            foreach (var artPair in new[]
            {
                MagicArtPairs.CrAn, MagicArtPairs.CrAq, MagicArtPairs.CrAu,
                MagicArtPairs.CrCo, MagicArtPairs.CrHe, MagicArtPairs.CrIg,
                MagicArtPairs.CrIm, MagicArtPairs.CrMe, MagicArtPairs.CrTe,
                MagicArtPairs.CrVi, MagicArtPairs.InAn, MagicArtPairs.InAq,
                MagicArtPairs.InAu, MagicArtPairs.InCo, MagicArtPairs.InHe,
                MagicArtPairs.InIg, MagicArtPairs.InIm, MagicArtPairs.InMe,
                MagicArtPairs.InTe, MagicArtPairs.InVi, MagicArtPairs.MuAn,
                MagicArtPairs.MuAq, MagicArtPairs.MuAu, MagicArtPairs.MuCo,
                MagicArtPairs.MuHe, MagicArtPairs.MuIg, MagicArtPairs.MuIm,
                MagicArtPairs.MuMe, MagicArtPairs.MuTe, MagicArtPairs.MuVi,
                MagicArtPairs.PeAn, MagicArtPairs.PeAq, MagicArtPairs.PeAu,
                MagicArtPairs.PeCo, MagicArtPairs.PeHe, MagicArtPairs.PeIg,
                MagicArtPairs.PeIm, MagicArtPairs.PeMe, MagicArtPairs.PeTe,
                MagicArtPairs.PeVi, MagicArtPairs.ReAn, MagicArtPairs.ReAq,
                MagicArtPairs.ReAu, MagicArtPairs.ReCo, MagicArtPairs.ReHe,
                MagicArtPairs.ReIg, MagicArtPairs.ReIm, MagicArtPairs.ReMe,
                MagicArtPairs.ReTe, MagicArtPairs.ReVi
            })
            {
                var bases = SpellBases.GetSpellBasesByArtPair(artPair);
                if (bases == null) continue;
                foreach (var spellBase in bases)
                    concepts.Add(new TraditionConcept(new SpellBasePrinciple(spellBase)));
            }

            // ----------------------------------------------------------------
            // Step 2: Build the TraditionActivityFormulas
            // ----------------------------------------------------------------
            var formulas = new List<TraditionActivityFormula>();

            // DistillVis: CrVi Lab Total / 10
            // Components feed into GetLabTotal's standard fallback for the
            // art-pair portion; this formula handles the /10 divisor.
            // We express the full formula here so GetVisDistillationRate
            // can use it without needing to call GetLabTotal separately.
            formulas.Add(new TraditionActivityFormula(
                Activity.DistillVis,
                components: new[]
                {
                    new FormulaComponent(MagicArts.Creo),
                    new FormulaComponent(MagicArts.Vim),
                    new FormulaComponent(Abilities.MagicTheory),
                    new FormulaComponent(
                        ImmutableMultiton<int, Ability>.GetInstance((int)AttributeType.Intelligence + 200),
                        1.0)  // Intelligence — see note below
                },
                includesAura: true,
                includesLabBonus: true,
                baseBonus: 0,
                divisor: 10.0));
            // NOTE: Attributes are not Abilities and cannot be looked up by
            // Ability directly. The FormulaComponent above uses a placeholder
            // approach. The GetLabTotal fallback handles Intelligence correctly
            // via GetAttribute(AttributeType.Intelligence). For the formula-
            // driven path in GetVisDistillationRate, we fall back to the
            // standard GetLabTotal call rather than evaluating Intelligence
            // through the formula. See MagusLabService.GetVisDistillationRate.
            // TODO: Extend TraditionActivityFormula to support AttributeType
            // components for a fully self-contained formula evaluation.

            // StudyVis: aura strength only (die roll handled at activity layer)
            // No fixed ability components — the quality is stress die + aura.
            formulas.Add(new TraditionActivityFormula(
                Activity.StudyVis,
                components: Array.Empty<FormulaComponent>(),
                includesAura: true,
                includesLabBonus: false,
                baseBonus: 0,
                divisor: 1.0));

            // ----------------------------------------------------------------
            // Step 3: Construct the MagicalTradition
            // ----------------------------------------------------------------
            var bonisagusTradition = new MagicalTradition(
                name: "Hermetic Magic",
                description: "The unified magical theory formulated by Bonisagus, drawing on " +
                             "Mercurian ritual magic, the twin witches' song-magic, and the " +
                             "diverse traditions of the twelve Founders. The most comprehensive " +
                             "magical system in Mythic Europe.",
                lineage: string.Empty,  // Self-developed; no opener.
                spontaneousMagicDivisor: 5.0,
                theoryAbility: Abilities.MagicTheory,
                initialConcepts: concepts,
                activityFormulas: formulas,
                opener: null);

            // ----------------------------------------------------------------
            // Step 4: Build and open the character
            // ----------------------------------------------------------------
            var bonisagusPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                // High Conscientiousness, High Openness, Low Agreeableness - a driven, brilliant, and difficult man.
                [HexacoFacet.Sincerity] = 1.5,
                [HexacoFacet.Fairness] = 1.4,
                [HexacoFacet.GreedAvoidance] = 1.0,
                [HexacoFacet.Modesty] = 0.5,
                [HexacoFacet.Fearfulness] = 1.0,
                [HexacoFacet.Anxiety] = 1.0,
                [HexacoFacet.Dependence] = 0.5,
                [HexacoFacet.Sentimentality] = 0.5,
                [HexacoFacet.SocialSelfEsteem] = 1.8,
                [HexacoFacet.SocialBoldness] = 1.9,
                [HexacoFacet.Sociability] = 0.5,
                [HexacoFacet.Liveliness] = 1.0,
                [HexacoFacet.Forgiveness] = 1.0,
                [HexacoFacet.Gentleness] = 1.0,
                [HexacoFacet.Flexibility] = 1.0,
                [HexacoFacet.Patience] = 0.4,
                [HexacoFacet.Organization] = 1.8,
                [HexacoFacet.Diligence] = 1.8,
                [HexacoFacet.Perfectionism] = 1.9,
                [HexacoFacet.Prudence] = 1.2,
                [HexacoFacet.AestheticAppreciation] = 1.0,
                [HexacoFacet.Inquisitiveness] = 1.9,
                [HexacoFacet.Creativity] = 1.8,
                [HexacoFacet.Unconventionality] = 1.8
            });

            Dictionary<string, double> reputation = new() { { "Magic Theory", 2 } };

            Bonisgaus = new HermeticMagus(HousesEnum.Bonisagus, 160, bonisagusPersonality, reputation)
            {
                Name = "Bonisagus"
            };

            // Open Bonisagus's own Gift with his self-developed tradition.
            // No opener — this tradition was built from first principles.
            Bonisgaus.OpenGift(bonisagusTradition);

            Bonisgaus.GetAttribute(AttributeType.Stamina).BaseValue = 1;
            Bonisgaus.GetAttribute(AttributeType.Strength).BaseValue = -2;
            Bonisgaus.GetAttribute(AttributeType.Dexterity).BaseValue = -2;
            Bonisgaus.GetAttribute(AttributeType.Quickness).BaseValue = -2;
            Bonisgaus.GetAttribute(AttributeType.Intelligence).BaseValue = 5;
            Bonisgaus.GetAttribute(AttributeType.Communication).BaseValue = 2;
            Bonisgaus.GetAttribute(AttributeType.Presence).BaseValue = -2;
            Bonisgaus.GetAttribute(AttributeType.Perception).BaseValue = 0;

            Bonisgaus.GetAbility(MagicArts.Creo).AddExperience(1);
            Bonisgaus.GetAbility(MagicArts.Intellego).AddExperience(1);
            Bonisgaus.GetAbility(MagicArts.Muto).AddExperience(1);
            Bonisgaus.GetAbility(MagicArts.Perdo).AddExperience(1);
            Bonisgaus.GetAbility(MagicArts.Rego).AddExperience(1);
            Bonisgaus.GetAbility(MagicArts.Animal).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Aquam).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Auram).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Corpus).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Herbam).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Ignem).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Mentem).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Terram).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Vim).AddExperience(55);
            Bonisgaus.GetAbility(Abilities.AreaLore).AddExperience(0);
            Bonisgaus.GetAbility(Abilities.ArtesLiberales).AddExperience(15);
            Bonisgaus.GetAbility(Abilities.English).AddExperience(75);
            Bonisgaus.GetAbility(Abilities.Etiquette).AddExperience(45);
            Bonisgaus.GetAbility(Abilities.Latin).AddExperience(75);
            Bonisgaus.GetAbility(Abilities.MagicTheory).AddExperience(75);
            Bonisgaus.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Bonisgaus.GetAbility(Abilities.Penetration).AddExperience(5);
            Bonisgaus.GetAbility(Abilities.Concentration).AddExperience(5);

            // ----------------------------------------------------------------
            // Step 5: Alpine Sanctum
            // ----------------------------------------------------------------
            var alpineAura = new Aura(Domain.Magic, 4, "Bonisagus's Alpine Cave");
            var vimVisSource = new VisSource(alpineAura, MagicArts.Vim, Season.Summer, 1.0);
            alpineAura.VisSources.Add(vimVisSource);

            var auraProfile = new BeliefProfile(SubjectType.Aura, 1.0);
            auraProfile.AddOrUpdateBelief(new Belief(BeliefTopics.Owner, Bonisgaus.Id.GetHashCode()));
            Bonisgaus.AddOrUpdateKnowledge(alpineAura, auraProfile);

            // ----------------------------------------------------------------
            // Step 6: Laboratory
            // ----------------------------------------------------------------
            var lab = new Laboratory(Bonisgaus, alpineAura, 0);
            lab.AddFeature(LabFeatures.HighlyOrganized);
            Bonisgaus.Laboratory = lab;

            // ----------------------------------------------------------------
            // Step 7: Starting vis stock (accumulated over years of solitary work)
            // ----------------------------------------------------------------
            Bonisgaus.VisStock[MagicArts.Vim] = 10;
            Bonisgaus.VisStock[MagicArts.Creo] = 4;

            // ----------------------------------------------------------------
            // Step 8: Pre-Hermetic lab texts
            // ----------------------------------------------------------------
            var detectAuraBase = SpellBases.GetSpellBaseForEffect(TechniqueEffects.Detect, FormEffects.Aura);
            var detectVisBase  = SpellBases.GetSpellBaseForEffect(TechniqueEffects.Detect, FormEffects.Vis);
            var quantifyVisBase = SpellBases.GetSpellBaseForEffect(TechniqueEffects.Quantify, FormEffects.Vis);
            var wardMagicBase  = SpellBases.GetSpellBaseForEffect(TechniqueEffects.Ward, FormEffects.Aura);

            Bonisgaus.LabTextsOwned.Add(new LabText
            {
                Author = Bonisgaus,
                SpellContained = new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Individual,
                    detectAuraBase, 0, false, "Sense the Hidden Aura")
            });
            Bonisgaus.LabTextsOwned.Add(new LabText
            {
                Author = Bonisgaus,
                SpellContained = new Spell(EffectRanges.Touch, EffectDurations.Instant, EffectTargets.Individual,
                    detectVisBase, 0, false, "Sense the Hidden Vis")
            });
            Bonisgaus.LabTextsOwned.Add(new LabText
            {
                Author = Bonisgaus,
                SpellContained = new Spell(EffectRanges.Touch, EffectDurations.Instant, EffectTargets.Individual,
                    quantifyVisBase, 0, false, "Weigh the Power")
            });
            Bonisgaus.LabTextsOwned.Add(new LabText
            {
                Author = Bonisgaus,
                SpellContained = new Spell(EffectRanges.Personal, EffectDurations.Sun, EffectTargets.Individual,
                    wardMagicBase, 0, false, "Aegis of the Self")
            });

            // ----------------------------------------------------------------
            // Step 9: In-progress Parma Magica research
            // ----------------------------------------------------------------
            // As of Spring 730 AD, Bonisagus has accumulated 53 of the 60 breakthrough
            // points required to complete Parma Magica. He has stabilized:
            //   7 magnitude-3 ReVi effects (Personal/Instant/Individual, Level 3 → 3 pts each)
            //   8 magnitude-4 ReVi effects (Touch/Instant/Individual,    Level 4 → 4 pts each)
            //   Total: 7×3 + 8×4 = 21 + 32 = 53 points
            // The project's CurrentPhase is null — the ResearchService will generate the
            // next experimental spell on the first tick once the simulation begins.
            var parmaDef = new ParmaMagicaBreakthrough();
            var parmaProject = new ResearchProject(Bonisgaus, parmaDef);

            // 7 × Magnitude-3 phases (Ward Against Magic, Personal range)
            for (int i = 0; i < 7; i++)
            {
                var spell = new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Individual,
                    wardMagicBase, 0, false, $"Bonisagus's Experimental Ward Study #{i + 1}");
                parmaProject.CompletedPhases.Add(ResearchProjectPhase.CreateCompleted(spell));
            }

            // 8 × Magnitude-4 phases (Ward Against Magic, Touch range adds +1 magnitude)
            for (int i = 0; i < 8; i++)
            {
                var spell = new Spell(EffectRanges.Touch, EffectDurations.Instant, EffectTargets.Individual,
                    wardMagicBase, 0, false, $"Bonisagus's Experimental Extended Ward #{i + 1}");
                parmaProject.CompletedPhases.Add(ResearchProjectPhase.CreateCompleted(spell));
            }

            Bonisgaus.ActiveProjects.Add(parmaProject);

            // Seeding the idea fires the cognitive architecture: AddIdea creates a
            // PursueIdeaGoal Intention, which will schedule OriginalResearchActivity
            // each season until the breakthrough completes.
            Bonisgaus.AddIdea(new BreakthroughIdea(parmaDef));
        }

        /// <summary>
        /// Helper: opens a Founder's Gift with a clone of Bonisagus's tradition,
        /// recording Bonisagus as the opener. All non-Bonisagus Founders were
        /// opened by him directly at the Founding.
        /// </summary>
        private static void OpenFounderArts(HermeticMagus founder)
        {
            var clonedTradition = Bonisgaus.Tradition.CloneForOpening();
            clonedTradition.RecordOpening(Bonisgaus, Bonisgaus.Tradition.Name);
            founder.OpenGift(clonedTradition);
        }

        public static void BuildBjornaer()
        {
            var bjornaerPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                // H: Low. Not driven by human social contracts, Modesty is low due to animal pride.
                [HexacoFacet.Sincerity] = 0.8,
                [HexacoFacet.Fairness] = 0.6,
                [HexacoFacet.GreedAvoidance] = 1.2,
                [HexacoFacet.Modesty] = 0.4,
                // E: High. Driven by instinct, fear, and strong attachments (to pack/beast, not people).
                [HexacoFacet.Fearfulness] = 1.7,
                [HexacoFacet.Anxiety] = 1.3,
                [HexacoFacet.Dependence] = 0.5,
                [HexacoFacet.Sentimentality] = 1.8,
                // X: Very Low. Reclusive and uncomfortable in human society.
                [HexacoFacet.SocialSelfEsteem] = 0.4,
                [HexacoFacet.SocialBoldness] = 0.6,
                [HexacoFacet.Sociability] = 0.2,
                [HexacoFacet.Liveliness] = 1.4,
                // A: Very Low. Territorial, unforgiving, and impatient with social niceties.
                [HexacoFacet.Forgiveness] = 0.3,
                [HexacoFacet.Gentleness] = 0.2,
                [HexacoFacet.Flexibility] = 0.5,
                [HexacoFacet.Patience] = 0.4,
                // C: Low. Relies on instinct and raw power over meticulous planning.
                [HexacoFacet.Organization] = 0.6,
                [HexacoFacet.Diligence] = 1.1,
                [HexacoFacet.Perfectionism] = 0.5,
                [HexacoFacet.Prudence] = 0.7,
                // O: High but focused. Inquisitive about the natural world, but not human arts.
                [HexacoFacet.AestheticAppreciation] = 0.8,
                [HexacoFacet.Inquisitiveness] = 1.8,
                [HexacoFacet.Creativity] = 1.3,
                [HexacoFacet.Unconventionality] = 1.9
            });

            Dictionary<string, double> reputation = new()
            {
                { "Bjornaer Lore", 2 }, { "Heartbeast", 2 }, { "Animal", 2 }
            };

            Bjornaer = new HermeticMagus(HousesEnum.Bjornaer, 80, bjornaerPersonality, reputation)
            {
                Name = "Bjornaer"
            };
            OpenFounderArts(Bjornaer);

            Bjornaer.GetAttribute(AttributeType.Stamina).BaseValue = 3;
            Bjornaer.GetAttribute(AttributeType.Strength).BaseValue = 1;
            Bjornaer.GetAttribute(AttributeType.Dexterity).BaseValue = -2;
            Bjornaer.GetAttribute(AttributeType.Quickness).BaseValue = -2;
            Bjornaer.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Bjornaer.GetAttribute(AttributeType.Communication).BaseValue = 1;
            Bjornaer.GetAttribute(AttributeType.Presence).BaseValue = 2;
            Bjornaer.GetAttribute(AttributeType.Perception).BaseValue = -1;

            Bjornaer.GetAbility(MagicArts.Creo).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Intellego).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Muto).AddExperience(15);
            Bjornaer.GetAbility(MagicArts.Perdo).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Rego).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Animal).AddExperience(15);
            Bjornaer.GetAbility(MagicArts.Aquam).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Auram).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Corpus).AddExperience(15);
            Bjornaer.GetAbility(MagicArts.Herbam).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Ignem).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Mentem).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Terram).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Vim).AddExperience(0);
            Bjornaer.GetAbility(Abilities.AreaLore).AddExperience(45);
            Bjornaer.GetAbility(Abilities.ArtesLiberales).AddExperience(5);
            Bjornaer.GetAbility(Abilities.English).AddExperience(75);
            Bjornaer.GetAbility(Abilities.Etiquette).AddExperience(0);
            Bjornaer.GetAbility(Abilities.Latin).AddExperience(50);
            Bjornaer.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Bjornaer.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Bjornaer.GetAbility(Abilities.Penetration).AddExperience(0);
            Bjornaer.GetAbility(Abilities.Concentration).AddExperience(0);
            Bjornaer.GetAbility(Abilities.Heartbeast).AddExperience(30);
            Bjornaer.GetAbility(Abilities.BjornaerLore).AddExperience(75);
        }

        public static void BuildCriamon()
        {
            var criamonPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                [HexacoFacet.Sincerity] = 1.8,
                [HexacoFacet.Fairness] = 1.4,
                [HexacoFacet.GreedAvoidance] = 1.9,
                [HexacoFacet.Modesty] = 2.0,
                [HexacoFacet.Fearfulness] = 0.1,
                [HexacoFacet.Anxiety] = 0.1,
                [HexacoFacet.Dependence] = 0.1,
                [HexacoFacet.Sentimentality] = 0.2,
                [HexacoFacet.SocialSelfEsteem] = 0.2,
                [HexacoFacet.SocialBoldness] = 0.1,
                [HexacoFacet.Sociability] = 0.1,
                [HexacoFacet.Liveliness] = 0.3,
                [HexacoFacet.Forgiveness] = 1.6,
                [HexacoFacet.Gentleness] = 1.8,
                [HexacoFacet.Flexibility] = 1.5,
                [HexacoFacet.Patience] = 1.9,
                [HexacoFacet.Organization] = 0.4,
                [HexacoFacet.Diligence] = 0.6,
                [HexacoFacet.Perfectionism] = 0.5,
                [HexacoFacet.Prudence] = 0.3,
                [HexacoFacet.AestheticAppreciation] = 1.5,
                [HexacoFacet.Inquisitiveness] = 2.0,
                [HexacoFacet.Creativity] = 1.8,
                [HexacoFacet.Unconventionality] = 2.0
            });

            Dictionary<string, double> reputation = new()
            {
                { "Criamon Lore", 2 }, { "Enigmatic Wisdom", 2 }
            };

            Criamon = new HermeticMagus(HousesEnum.Criamon, 80, criamonPersonality, reputation)
            {
                Name = "Criamon"
            };
            OpenFounderArts(Criamon);

            Criamon.GetAttribute(AttributeType.Stamina).BaseValue = 1;
            Criamon.GetAttribute(AttributeType.Strength).BaseValue = -2;
            Criamon.GetAttribute(AttributeType.Dexterity).BaseValue = -2;
            Criamon.GetAttribute(AttributeType.Quickness).BaseValue = 1;
            Criamon.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Criamon.GetAttribute(AttributeType.Communication).BaseValue = -1;
            Criamon.GetAttribute(AttributeType.Presence).BaseValue = 3;
            Criamon.GetAttribute(AttributeType.Perception).BaseValue = 2;

            Criamon.GetAbility(MagicArts.Creo).AddExperience(0);
            Criamon.GetAbility(MagicArts.Intellego).AddExperience(15);
            Criamon.GetAbility(MagicArts.Muto).AddExperience(0);
            Criamon.GetAbility(MagicArts.Perdo).AddExperience(0);
            Criamon.GetAbility(MagicArts.Rego).AddExperience(0);
            Criamon.GetAbility(MagicArts.Animal).AddExperience(0);
            Criamon.GetAbility(MagicArts.Aquam).AddExperience(0);
            Criamon.GetAbility(MagicArts.Auram).AddExperience(0);
            Criamon.GetAbility(MagicArts.Corpus).AddExperience(0);
            Criamon.GetAbility(MagicArts.Herbam).AddExperience(0);
            Criamon.GetAbility(MagicArts.Ignem).AddExperience(0);
            Criamon.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Criamon.GetAbility(MagicArts.Mentem).AddExperience(0);
            Criamon.GetAbility(MagicArts.Terram).AddExperience(0);
            Criamon.GetAbility(MagicArts.Vim).AddExperience(15);
            Criamon.GetAbility(Abilities.AreaLore).AddExperience(30);
            Criamon.GetAbility(Abilities.ArtesLiberales).AddExperience(30);
            Criamon.GetAbility(Abilities.English).AddExperience(75);
            Criamon.GetAbility(Abilities.Etiquette).AddExperience(15);
            Criamon.GetAbility(Abilities.Latin).AddExperience(50);
            Criamon.GetAbility(Abilities.MagicTheory).AddExperience(50);
            Criamon.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Criamon.GetAbility(Abilities.Penetration).AddExperience(0);
            Criamon.GetAbility(Abilities.Concentration).AddExperience(30);
            Criamon.GetAbility(Abilities.EnigmaticWisdom).AddExperience(15);
            Criamon.GetAbility(Abilities.CriamonLore).AddExperience(30);
        }

        public static void BuildDiedne()
        {
            var diednePersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                [HexacoFacet.Sincerity] = 0.4,
                [HexacoFacet.Fairness] = 0.7,
                [HexacoFacet.GreedAvoidance] = 0.8,
                [HexacoFacet.Modesty] = 0.3,
                [HexacoFacet.Fearfulness] = 1.2,
                [HexacoFacet.Anxiety] = 1.7,
                [HexacoFacet.Dependence] = 1.1,
                [HexacoFacet.Sentimentality] = 1.8,
                [HexacoFacet.SocialSelfEsteem] = 1.9,
                [HexacoFacet.SocialBoldness] = 1.8,
                [HexacoFacet.Sociability] = 1.9,
                [HexacoFacet.Liveliness] = 2.0,
                [HexacoFacet.Forgiveness] = 0.4,
                [HexacoFacet.Gentleness] = 0.6,
                [HexacoFacet.Flexibility] = 0.5,
                [HexacoFacet.Patience] = 0.3,
                [HexacoFacet.Organization] = 0.7,
                [HexacoFacet.Diligence] = 1.2,
                [HexacoFacet.Perfectionism] = 0.8,
                [HexacoFacet.Prudence] = 0.1,
                [HexacoFacet.AestheticAppreciation] = 1.9,
                [HexacoFacet.Inquisitiveness] = 1.4,
                [HexacoFacet.Creativity] = 1.8,
                [HexacoFacet.Unconventionality] = 1.7
            });

            Diedne = new HermeticMagus(HousesEnum.Diedne, 80, diednePersonality, new Dictionary<string, double>())
            {
                Name = "Diedne"
            };
            OpenFounderArts(Diedne);

            Diedne.GetAttribute(AttributeType.Stamina).BaseValue = 1;
            Diedne.GetAttribute(AttributeType.Strength).BaseValue = -2;
            Diedne.GetAttribute(AttributeType.Dexterity).BaseValue = 0;
            Diedne.GetAttribute(AttributeType.Quickness).BaseValue = 0;
            Diedne.GetAttribute(AttributeType.Intelligence).BaseValue = 1;
            Diedne.GetAttribute(AttributeType.Communication).BaseValue = 1;
            Diedne.GetAttribute(AttributeType.Presence).BaseValue = 3;
            Diedne.GetAttribute(AttributeType.Perception).BaseValue = 1;

            Diedne.GetAbility(MagicArts.Creo).AddExperience(12);
            Diedne.GetAbility(MagicArts.Intellego).AddExperience(12);
            Diedne.GetAbility(MagicArts.Muto).AddExperience(12);
            Diedne.GetAbility(MagicArts.Perdo).AddExperience(12);
            Diedne.GetAbility(MagicArts.Rego).AddExperience(12);
            Diedne.GetAbility(MagicArts.Animal).AddExperience(6);
            Diedne.GetAbility(MagicArts.Aquam).AddExperience(6);
            Diedne.GetAbility(MagicArts.Auram).AddExperience(6);
            Diedne.GetAbility(MagicArts.Corpus).AddExperience(6);
            Diedne.GetAbility(MagicArts.Herbam).AddExperience(6);
            Diedne.GetAbility(MagicArts.Ignem).AddExperience(6);
            Diedne.GetAbility(MagicArts.Imaginem).AddExperience(6);
            Diedne.GetAbility(MagicArts.Mentem).AddExperience(6);
            Diedne.GetAbility(MagicArts.Terram).AddExperience(6);
            Diedne.GetAbility(MagicArts.Vim).AddExperience(6);
            Diedne.GetAbility(Abilities.AreaLore).AddExperience(30);
            Diedne.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Diedne.GetAbility(Abilities.English).AddExperience(75);
            Diedne.GetAbility(Abilities.Etiquette).AddExperience(15);
            Diedne.GetAbility(Abilities.Latin).AddExperience(0);
            Diedne.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Diedne.GetAbility(Abilities.ParmaMagica).AddExperience(30);
            Diedne.GetAbility(Abilities.Penetration).AddExperience(30);
            Diedne.GetAbility(Abilities.Concentration).AddExperience(30);
        }

        public static void BuildFlambeau()
        {
            var flambeauPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                [HexacoFacet.Sincerity] = 0.7,
                [HexacoFacet.Fairness] = 1.5,
                [HexacoFacet.GreedAvoidance] = 1.1,
                [HexacoFacet.Modesty] = 0.1,
                [HexacoFacet.Fearfulness] = 0.1,
                [HexacoFacet.Anxiety] = 0.2,
                [HexacoFacet.Dependence] = 0.2,
                [HexacoFacet.Sentimentality] = 0.3,
                [HexacoFacet.SocialSelfEsteem] = 1.8,
                [HexacoFacet.SocialBoldness] = 2.0,
                [HexacoFacet.Sociability] = 1.1,
                [HexacoFacet.Liveliness] = 1.7,
                [HexacoFacet.Forgiveness] = 0.1,
                [HexacoFacet.Gentleness] = 0.1,
                [HexacoFacet.Flexibility] = 0.2,
                [HexacoFacet.Patience] = 0.1,
                [HexacoFacet.Organization] = 1.1,
                [HexacoFacet.Diligence] = 1.8,
                [HexacoFacet.Perfectionism] = 1.4,
                [HexacoFacet.Prudence] = 0.6,
                [HexacoFacet.AestheticAppreciation] = 0.4,
                [HexacoFacet.Inquisitiveness] = 0.5,
                [HexacoFacet.Creativity] = 0.6,
                [HexacoFacet.Unconventionality] = 0.8
            });

            Dictionary<string, double> reputation = new()
            {
                { "Ignem", 2.0 }, { "Penetration", 2.0 }, { "Parma Magica", 2.0 }
            };

            Flambeau = new HermeticMagus(HousesEnum.Flambeau, 80, flambeauPersonality, reputation)
            {
                Name = "Flambeau"
            };
            OpenFounderArts(Flambeau);

            Flambeau.GetAttribute(AttributeType.Stamina).BaseValue = 2;
            Flambeau.GetAttribute(AttributeType.Strength).BaseValue = 2;
            Flambeau.GetAttribute(AttributeType.Dexterity).BaseValue = 2;
            Flambeau.GetAttribute(AttributeType.Quickness).BaseValue = 0;
            Flambeau.GetAttribute(AttributeType.Intelligence).BaseValue = 1;
            Flambeau.GetAttribute(AttributeType.Communication).BaseValue = -2;
            Flambeau.GetAttribute(AttributeType.Presence).BaseValue = 2;
            Flambeau.GetAttribute(AttributeType.Perception).BaseValue = -2;

            Flambeau.GetAbility(MagicArts.Creo).AddExperience(3);
            Flambeau.GetAbility(MagicArts.Intellego).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Muto).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Perdo).AddExperience(3);
            Flambeau.GetAbility(MagicArts.Rego).AddExperience(3);
            Flambeau.GetAbility(MagicArts.Animal).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Aquam).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Auram).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Corpus).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Herbam).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Ignem).AddExperience(21);
            Flambeau.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Mentem).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Terram).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Vim).AddExperience(0);
            Flambeau.GetAbility(Abilities.AreaLore).AddExperience(15);
            Flambeau.GetAbility(Abilities.ArtesLiberales).AddExperience(15);
            Flambeau.GetAbility(Abilities.English).AddExperience(75);
            Flambeau.GetAbility(Abilities.Etiquette).AddExperience(30);
            Flambeau.GetAbility(Abilities.Latin).AddExperience(75);
            Flambeau.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Flambeau.GetAbility(Abilities.ParmaMagica).AddExperience(30);
            Flambeau.GetAbility(Abilities.Penetration).AddExperience(30);
            Flambeau.GetAbility(Abilities.Concentration).AddExperience(30);
        }

        public static void BuildGuernicus()
        {
            var guernicusPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                [HexacoFacet.Sincerity] = 2.0,
                [HexacoFacet.Fairness] = 2.0,
                [HexacoFacet.GreedAvoidance] = 1.8,
                [HexacoFacet.Modesty] = 1.4,
                [HexacoFacet.Fearfulness] = 0.3,
                [HexacoFacet.Anxiety] = 0.2,
                [HexacoFacet.Dependence] = 0.4,
                [HexacoFacet.Sentimentality] = 0.1,
                [HexacoFacet.SocialSelfEsteem] = 1.4,
                [HexacoFacet.SocialBoldness] = 1.1,
                [HexacoFacet.Sociability] = 0.5,
                [HexacoFacet.Liveliness] = 0.6,
                [HexacoFacet.Forgiveness] = 0.1,
                [HexacoFacet.Gentleness] = 0.2,
                [HexacoFacet.Flexibility] = 0.1,
                [HexacoFacet.Patience] = 0.4,
                [HexacoFacet.Organization] = 2.0,
                [HexacoFacet.Diligence] = 2.0,
                [HexacoFacet.Perfectionism] = 1.7,
                [HexacoFacet.Prudence] = 1.9,
                [HexacoFacet.AestheticAppreciation] = 0.5,
                [HexacoFacet.Inquisitiveness] = 0.7,
                [HexacoFacet.Creativity] = 0.3,
                [HexacoFacet.Unconventionality] = 0.1
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Terram", 2.0 }, { "Code of Hermes", 2.0 }, { "Magic Theory", 1.5 }
            };

            Guernicus = new HermeticMagus(HousesEnum.Guernicus, 80, guernicusPersonality, reputationMultipliers)
            {
                Name = "Guernicus"
            };
            OpenFounderArts(Guernicus);

            Guernicus.GetAttribute(AttributeType.Stamina).BaseValue = 2;
            Guernicus.GetAttribute(AttributeType.Strength).BaseValue = 0;
            Guernicus.GetAttribute(AttributeType.Dexterity).BaseValue = -1;
            Guernicus.GetAttribute(AttributeType.Quickness).BaseValue = -2;
            Guernicus.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Guernicus.GetAttribute(AttributeType.Communication).BaseValue = 2;
            Guernicus.GetAttribute(AttributeType.Presence).BaseValue = 1;
            Guernicus.GetAttribute(AttributeType.Perception).BaseValue = 1;

            Guernicus.GetAbility(MagicArts.Creo).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Intellego).AddExperience(15);
            Guernicus.GetAbility(MagicArts.Muto).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Perdo).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Rego).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Animal).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Aquam).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Auram).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Corpus).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Herbam).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Ignem).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Mentem).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Terram).AddExperience(55);
            Guernicus.GetAbility(MagicArts.Vim).AddExperience(0);
            Guernicus.GetAbility(Abilities.AreaLore).AddExperience(15);
            Guernicus.GetAbility(Abilities.ArtesLiberales).AddExperience(30);
            Guernicus.GetAbility(Abilities.English).AddExperience(75);
            Guernicus.GetAbility(Abilities.Etiquette).AddExperience(30);
            Guernicus.GetAbility(Abilities.Latin).AddExperience(75);
            Guernicus.GetAbility(Abilities.MagicTheory).AddExperience(50);
            Guernicus.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Guernicus.GetAbility(Abilities.CodeOfHermes).AddExperience(5);
            Guernicus.GetAbility(Abilities.Concentration).AddExperience(5);
        }

        public static void BuildJerbiton()
        {
            var jerbitonPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                [HexacoFacet.Sincerity] = 1.6,
                [HexacoFacet.Fairness] = 1.3,
                [HexacoFacet.GreedAvoidance] = 1.7,
                [HexacoFacet.Modesty] = 1.5,
                [HexacoFacet.Fearfulness] = 1.1,
                [HexacoFacet.Anxiety] = 1.3,
                [HexacoFacet.Dependence] = 1.4,
                [HexacoFacet.Sentimentality] = 1.8,
                [HexacoFacet.SocialSelfEsteem] = 1.6,
                [HexacoFacet.SocialBoldness] = 1.1,
                [HexacoFacet.Sociability] = 1.8,
                [HexacoFacet.Liveliness] = 1.7,
                [HexacoFacet.Forgiveness] = 1.7,
                [HexacoFacet.Gentleness] = 1.9,
                [HexacoFacet.Flexibility] = 1.6,
                [HexacoFacet.Patience] = 1.8,
                [HexacoFacet.Organization] = 1.3,
                [HexacoFacet.Diligence] = 1.0,
                [HexacoFacet.Perfectionism] = 1.2,
                [HexacoFacet.Prudence] = 1.1,
                [HexacoFacet.AestheticAppreciation] = 2.0,
                [HexacoFacet.Inquisitiveness] = 1.6,
                [HexacoFacet.Creativity] = 1.9,
                [HexacoFacet.Unconventionality] = 1.2
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Etiquette", 2.0 }, { "Artes Liberales", 2.0 },
                { "Finesse", 2.0 }, { "Imaginem", 2.0 }
            };

            Jerbiton = new HermeticMagus(HousesEnum.Jerbiton, 80, jerbitonPersonality, reputationMultipliers)
            {
                Name = "Jerbiton"
            };
            OpenFounderArts(Jerbiton);

            Jerbiton.GetAttribute(AttributeType.Stamina).BaseValue = -1;
            Jerbiton.GetAttribute(AttributeType.Strength).BaseValue = -2;
            Jerbiton.GetAttribute(AttributeType.Dexterity).BaseValue = 0;
            Jerbiton.GetAttribute(AttributeType.Quickness).BaseValue = -1;
            Jerbiton.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Jerbiton.GetAttribute(AttributeType.Communication).BaseValue = 2;
            Jerbiton.GetAttribute(AttributeType.Presence).BaseValue = 2;
            Jerbiton.GetAttribute(AttributeType.Perception).BaseValue = 2;

            Jerbiton.GetAbility(MagicArts.Creo).AddExperience(3);
            Jerbiton.GetAbility(MagicArts.Intellego).AddExperience(1);
            Jerbiton.GetAbility(MagicArts.Muto).AddExperience(3);
            Jerbiton.GetAbility(MagicArts.Perdo).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Rego).AddExperience(3);
            Jerbiton.GetAbility(MagicArts.Animal).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Aquam).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Auram).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Corpus).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Herbam).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Ignem).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Imaginem).AddExperience(55);
            Jerbiton.GetAbility(MagicArts.Mentem).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Terram).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Vim).AddExperience(0);
            Jerbiton.GetAbility(Abilities.AreaLore).AddExperience(15);
            Jerbiton.GetAbility(Abilities.ArtesLiberales).AddExperience(50);
            Jerbiton.GetAbility(Abilities.English).AddExperience(75);
            Jerbiton.GetAbility(Abilities.Etiquette).AddExperience(50);
            Jerbiton.GetAbility(Abilities.Latin).AddExperience(75);
            Jerbiton.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Jerbiton.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Jerbiton.GetAbility(Abilities.Penetration).AddExperience(0);
            Jerbiton.GetAbility(Abilities.Concentration).AddExperience(0);
            Jerbiton.GetAbility(Abilities.Finesse).AddExperience(30);
        }

        public static void BuildMercere()
        {
            var mercerePersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                [HexacoFacet.Sincerity] = 1.8,
                [HexacoFacet.Fairness] = 1.7,
                [HexacoFacet.GreedAvoidance] = 1.5,
                [HexacoFacet.Modesty] = 1.2,
                [HexacoFacet.Fearfulness] = 0.8,
                [HexacoFacet.Anxiety] = 1.1,
                [HexacoFacet.Dependence] = 0.9,
                [HexacoFacet.Sentimentality] = 1.4,
                [HexacoFacet.SocialSelfEsteem] = 1.6,
                [HexacoFacet.SocialBoldness] = 1.4,
                [HexacoFacet.Sociability] = 1.9,
                [HexacoFacet.Liveliness] = 1.6,
                [HexacoFacet.Forgiveness] = 1.5,
                [HexacoFacet.Gentleness] = 1.6,
                [HexacoFacet.Flexibility] = 1.8,
                [HexacoFacet.Patience] = 1.3,
                [HexacoFacet.Organization] = 1.7,
                [HexacoFacet.Diligence] = 1.5,
                [HexacoFacet.Perfectionism] = 1.1,
                [HexacoFacet.Prudence] = 1.4,
                [HexacoFacet.AestheticAppreciation] = 1.1,
                [HexacoFacet.Inquisitiveness] = 1.3,
                [HexacoFacet.Creativity] = 1.2,
                [HexacoFacet.Unconventionality] = 0.9
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Mentem", 2.0 }, { "Creo", 1.5 }
            };

            Mercere = new HermeticMagus(HousesEnum.Mercere, 80, mercerePersonality, reputationMultipliers)
            {
                Name = "Mercere"
            };
            OpenFounderArts(Mercere);

            Mercere.GetAttribute(AttributeType.Stamina).BaseValue = 0;
            Mercere.GetAttribute(AttributeType.Strength).BaseValue = -1;
            Mercere.GetAttribute(AttributeType.Dexterity).BaseValue = 1;
            Mercere.GetAttribute(AttributeType.Quickness).BaseValue = 2;
            Mercere.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Mercere.GetAttribute(AttributeType.Communication).BaseValue = 2;
            Mercere.GetAttribute(AttributeType.Presence).BaseValue = -3;
            Mercere.GetAttribute(AttributeType.Perception).BaseValue = 2;

            Mercere.GetAbility(MagicArts.Creo).AddExperience(15);
            Mercere.GetAbility(MagicArts.Intellego).AddExperience(6);
            Mercere.GetAbility(MagicArts.Muto).AddExperience(15);
            Mercere.GetAbility(MagicArts.Perdo).AddExperience(3);
            Mercere.GetAbility(MagicArts.Rego).AddExperience(15);
            Mercere.GetAbility(MagicArts.Animal).AddExperience(0);
            Mercere.GetAbility(MagicArts.Aquam).AddExperience(0);
            Mercere.GetAbility(MagicArts.Auram).AddExperience(0);
            Mercere.GetAbility(MagicArts.Corpus).AddExperience(0);
            Mercere.GetAbility(MagicArts.Herbam).AddExperience(0);
            Mercere.GetAbility(MagicArts.Ignem).AddExperience(0);
            Mercere.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Mercere.GetAbility(MagicArts.Mentem).AddExperience(55);
            Mercere.GetAbility(MagicArts.Terram).AddExperience(0);
            Mercere.GetAbility(MagicArts.Vim).AddExperience(1);
            Mercere.GetAbility(Abilities.AreaLore).AddExperience(30);
            Mercere.GetAbility(Abilities.ArtesLiberales).AddExperience(30);
            Mercere.GetAbility(Abilities.English).AddExperience(75);
            Mercere.GetAbility(Abilities.Etiquette).AddExperience(30);
            Mercere.GetAbility(Abilities.Latin).AddExperience(50);
            Mercere.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Mercere.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Mercere.GetAbility(Abilities.Penetration).AddExperience(0);
            Mercere.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildMerinita()
        {
            var merinitaPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                [HexacoFacet.Sincerity] = 0.5,
                [HexacoFacet.Fairness] = 0.6,
                [HexacoFacet.GreedAvoidance] = 1.3,
                [HexacoFacet.Modesty] = 0.4,
                [HexacoFacet.Fearfulness] = 1.4,
                [HexacoFacet.Anxiety] = 1.1,
                [HexacoFacet.Dependence] = 1.6,
                [HexacoFacet.Sentimentality] = 2.0,
                [HexacoFacet.SocialSelfEsteem] = 1.5,
                [HexacoFacet.SocialBoldness] = 0.9,
                [HexacoFacet.Sociability] = 1.2,
                [HexacoFacet.Liveliness] = 1.7,
                [HexacoFacet.Forgiveness] = 0.6,
                [HexacoFacet.Gentleness] = 1.7,
                [HexacoFacet.Flexibility] = 0.8,
                [HexacoFacet.Patience] = 0.5,
                [HexacoFacet.Organization] = 0.5,
                [HexacoFacet.Diligence] = 0.8,
                [HexacoFacet.Perfectionism] = 1.4,
                [HexacoFacet.Prudence] = 0.4,
                [HexacoFacet.AestheticAppreciation] = 2.0,
                [HexacoFacet.Inquisitiveness] = 1.7,
                [HexacoFacet.Creativity] = 2.0,
                [HexacoFacet.Unconventionality] = 1.9
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Herbam", 2.0 }, { "Animal", 1.5 }, { "Creo", 1.5 },
                { "Intelligo", 1.5 }, { "Muto", 1.5 }, { "Perdo", 1.5 },
                { "Rego", 1.5 }, { "Merinita Lore", 2.0 }
            };

            Merinita = new HermeticMagus(HousesEnum.Merinita, 80, merinitaPersonality, reputationMultipliers)
            {
                Name = "Merinita"
            };
            OpenFounderArts(Merinita);

            Merinita.GetAttribute(AttributeType.Stamina).BaseValue = 0;
            Merinita.GetAttribute(AttributeType.Strength).BaseValue = -1;
            Merinita.GetAttribute(AttributeType.Dexterity).BaseValue = 0;
            Merinita.GetAttribute(AttributeType.Quickness).BaseValue = 0;
            Merinita.GetAttribute(AttributeType.Intelligence).BaseValue = 1;
            Merinita.GetAttribute(AttributeType.Communication).BaseValue = 1;
            Merinita.GetAttribute(AttributeType.Presence).BaseValue = 3;
            Merinita.GetAttribute(AttributeType.Perception).BaseValue = 0;

            Merinita.GetAbility(MagicArts.Creo).AddExperience(10);
            Merinita.GetAbility(MagicArts.Intellego).AddExperience(15);
            Merinita.GetAbility(MagicArts.Muto).AddExperience(15);
            Merinita.GetAbility(MagicArts.Perdo).AddExperience(10);
            Merinita.GetAbility(MagicArts.Rego).AddExperience(10);
            Merinita.GetAbility(MagicArts.Animal).AddExperience(0);
            Merinita.GetAbility(MagicArts.Aquam).AddExperience(0);
            Merinita.GetAbility(MagicArts.Auram).AddExperience(0);
            Merinita.GetAbility(MagicArts.Corpus).AddExperience(0);
            Merinita.GetAbility(MagicArts.Herbam).AddExperience(55);
            Merinita.GetAbility(MagicArts.Ignem).AddExperience(0);
            Merinita.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Merinita.GetAbility(MagicArts.Mentem).AddExperience(0);
            Merinita.GetAbility(MagicArts.Terram).AddExperience(15);
            Merinita.GetAbility(MagicArts.Vim).AddExperience(0);
            Merinita.GetAbility(Abilities.AreaLore).AddExperience(15);
            Merinita.GetAbility(Abilities.ArtesLiberales).AddExperience(5);
            Merinita.GetAbility(Abilities.English).AddExperience(75);
            Merinita.GetAbility(Abilities.Etiquette).AddExperience(15);
            Merinita.GetAbility(Abilities.Latin).AddExperience(50);
            Merinita.GetAbility(Abilities.MagicTheory).AddExperience(15);
            Merinita.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Merinita.GetAbility(Abilities.Penetration).AddExperience(0);
            Merinita.GetAbility(Abilities.MerinitaLore).AddExperience(50);
        }

        public static void BuildTremere()
        {
            var tremerePersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                [HexacoFacet.Sincerity] = 0.3,
                [HexacoFacet.Fairness] = 0.6,
                [HexacoFacet.GreedAvoidance] = 0.2,
                [HexacoFacet.Modesty] = 0.4,
                [HexacoFacet.Fearfulness] = 1.4,
                [HexacoFacet.Anxiety] = 1.0,
                [HexacoFacet.Dependence] = 1.4,
                [HexacoFacet.Sentimentality] = 0.5,
                [HexacoFacet.SocialSelfEsteem] = 1.3,
                [HexacoFacet.SocialBoldness] = 1.9,
                [HexacoFacet.Sociability] = 1.3,
                [HexacoFacet.Liveliness] = 1.3,
                [HexacoFacet.Forgiveness] = 0.5,
                [HexacoFacet.Gentleness] = 0.4,
                [HexacoFacet.Flexibility] = 0.3,
                [HexacoFacet.Patience] = 1.5,
                [HexacoFacet.Organization] = 1.8,
                [HexacoFacet.Diligence] = 1.9,
                [HexacoFacet.Perfectionism] = 1.6,
                [HexacoFacet.Prudence] = 1.7,
                [HexacoFacet.AestheticAppreciation] = 0.6,
                [HexacoFacet.Inquisitiveness] = 0.8,
                [HexacoFacet.Creativity] = 0.7,
                [HexacoFacet.Unconventionality] = 1.2
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Penetration", 2.0 }, { "Parma Magica", 2.0 }
            };

            Tremere = new HermeticMagus(HousesEnum.Tremere, 80, tremerePersonality, reputationMultipliers)
            {
                Name = "Tremere"
            };
            OpenFounderArts(Tremere);

            Tremere.GetAttribute(AttributeType.Stamina).BaseValue = 0;
            Tremere.GetAttribute(AttributeType.Strength).BaseValue = 0;
            Tremere.GetAttribute(AttributeType.Dexterity).BaseValue = 0;
            Tremere.GetAttribute(AttributeType.Quickness).BaseValue = 1;
            Tremere.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Tremere.GetAttribute(AttributeType.Communication).BaseValue = 1;
            Tremere.GetAttribute(AttributeType.Presence).BaseValue = -1;
            Tremere.GetAttribute(AttributeType.Perception).BaseValue = 2;

            Tremere.GetAbility(MagicArts.Creo).AddExperience(0);
            Tremere.GetAbility(MagicArts.Intellego).AddExperience(0);
            Tremere.GetAbility(MagicArts.Muto).AddExperience(0);
            Tremere.GetAbility(MagicArts.Perdo).AddExperience(0);
            Tremere.GetAbility(MagicArts.Rego).AddExperience(0);
            Tremere.GetAbility(MagicArts.Animal).AddExperience(0);
            Tremere.GetAbility(MagicArts.Aquam).AddExperience(0);
            Tremere.GetAbility(MagicArts.Auram).AddExperience(0);
            Tremere.GetAbility(MagicArts.Corpus).AddExperience(0);
            Tremere.GetAbility(MagicArts.Herbam).AddExperience(0);
            Tremere.GetAbility(MagicArts.Ignem).AddExperience(0);
            Tremere.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Tremere.GetAbility(MagicArts.Mentem).AddExperience(0);
            Tremere.GetAbility(MagicArts.Terram).AddExperience(0);
            Tremere.GetAbility(MagicArts.Vim).AddExperience(0);
            Tremere.GetAbility(Abilities.AreaLore).AddExperience(0);
            Tremere.GetAbility(Abilities.ArtesLiberales).AddExperience(15);
            Tremere.GetAbility(Abilities.English).AddExperience(75);
            Tremere.GetAbility(Abilities.Etiquette).AddExperience(5);
            Tremere.GetAbility(Abilities.Latin).AddExperience(75);
            Tremere.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Tremere.GetAbility(Abilities.ParmaMagica).AddExperience(30);
            Tremere.GetAbility(Abilities.Penetration).AddExperience(30);
            Tremere.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildTytalus()
        {
            var tytalusPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                [HexacoFacet.Sincerity] = 0.3,
                [HexacoFacet.Fairness] = 0.9,
                [HexacoFacet.GreedAvoidance] = 0.5,
                [HexacoFacet.Modesty] = 0.1,
                [HexacoFacet.Fearfulness] = 0.2,
                [HexacoFacet.Anxiety] = 0.3,
                [HexacoFacet.Dependence] = 0.1,
                [HexacoFacet.Sentimentality] = 0.4,
                [HexacoFacet.SocialSelfEsteem] = 1.9,
                [HexacoFacet.SocialBoldness] = 2.0,
                [HexacoFacet.Sociability] = 1.5,
                [HexacoFacet.Liveliness] = 1.6,
                [HexacoFacet.Forgiveness] = 0.1,
                [HexacoFacet.Gentleness] = 0.1,
                [HexacoFacet.Flexibility] = 0.2,
                [HexacoFacet.Patience] = 0.2,
                [HexacoFacet.Organization] = 1.2,
                [HexacoFacet.Diligence] = 1.7,
                [HexacoFacet.Perfectionism] = 1.3,
                [HexacoFacet.Prudence] = 1.6,
                [HexacoFacet.AestheticAppreciation] = 0.9,
                [HexacoFacet.Inquisitiveness] = 1.9,
                [HexacoFacet.Creativity] = 1.6,
                [HexacoFacet.Unconventionality] = 1.5
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Rego", 2.0 }, { "Mentem", 2.0 },
                { "Penetration", 2.0 }, { "Finesse", 2.0 }
            };

            Tytalus = new HermeticMagus(HousesEnum.Tytalus, 80, tytalusPersonality, reputationMultipliers)
            {
                Name = "Tytalus"
            };
            OpenFounderArts(Tytalus);

            Tytalus.GetAttribute(AttributeType.Stamina).BaseValue = 3;
            Tytalus.GetAttribute(AttributeType.Strength).BaseValue = 0;
            Tytalus.GetAttribute(AttributeType.Dexterity).BaseValue = 0;
            Tytalus.GetAttribute(AttributeType.Quickness).BaseValue = -1;
            Tytalus.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Tytalus.GetAttribute(AttributeType.Communication).BaseValue = -2;
            Tytalus.GetAttribute(AttributeType.Presence).BaseValue = -1;
            Tytalus.GetAttribute(AttributeType.Perception).BaseValue = 2;

            Tytalus.GetAbility(MagicArts.Creo).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Intellego).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Muto).AddExperience(10);
            Tytalus.GetAbility(MagicArts.Perdo).AddExperience(15);
            Tytalus.GetAbility(MagicArts.Rego).AddExperience(55);
            Tytalus.GetAbility(MagicArts.Animal).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Aquam).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Auram).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Corpus).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Herbam).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Ignem).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Mentem).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Terram).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Vim).AddExperience(0);
            Tytalus.GetAbility(Abilities.AreaLore).AddExperience(30);
            Tytalus.GetAbility(Abilities.ArtesLiberales).AddExperience(5);
            Tytalus.GetAbility(Abilities.English).AddExperience(75);
            Tytalus.GetAbility(Abilities.Etiquette).AddExperience(15);
            Tytalus.GetAbility(Abilities.Latin).AddExperience(50);
            Tytalus.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Tytalus.GetAbility(Abilities.ParmaMagica).AddExperience(30);
            Tytalus.GetAbility(Abilities.Penetration).AddExperience(30);
            Tytalus.GetAbility(Abilities.Concentration).AddExperience(15);
        }

        public static void BuildVerditius()
        {
            var verditiusPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                [HexacoFacet.Sincerity] = 0.6,
                [HexacoFacet.Fairness] = 0.8,
                [HexacoFacet.GreedAvoidance] = 0.2,
                [HexacoFacet.Modesty] = 0.1,
                [HexacoFacet.Fearfulness] = 0.8,
                [HexacoFacet.Anxiety] = 1.8,
                [HexacoFacet.Dependence] = 1.3,
                [HexacoFacet.Sentimentality] = 1.6,
                [HexacoFacet.SocialSelfEsteem] = 0.7,
                [HexacoFacet.SocialBoldness] = 0.6,
                [HexacoFacet.Sociability] = 0.4,
                [HexacoFacet.Liveliness] = 0.9,
                [HexacoFacet.Forgiveness] = 0.5,
                [HexacoFacet.Gentleness] = 0.4,
                [HexacoFacet.Flexibility] = 0.6,
                [HexacoFacet.Patience] = 0.3,
                [HexacoFacet.Organization] = 1.8,
                [HexacoFacet.Diligence] = 2.0,
                [HexacoFacet.Perfectionism] = 2.0,
                [HexacoFacet.Prudence] = 1.4,
                [HexacoFacet.AestheticAppreciation] = 1.7,
                [HexacoFacet.Inquisitiveness] = 1.6,
                [HexacoFacet.Creativity] = 1.9,
                [HexacoFacet.Unconventionality] = 1.1
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Verditius Lore", 2.0 }, { "Craft", 2.0 },
                { "Philosophae", 1.5 }, { "Terram", 1.5 }
            };

            Verditius = new HermeticMagus(HousesEnum.Verditius, 80, verditiusPersonality, reputationMultipliers)
            {
                Name = "Verditius"
            };
            OpenFounderArts(Verditius);

            Verditius.GetAttribute(AttributeType.Stamina).BaseValue = 1;
            Verditius.GetAttribute(AttributeType.Strength).BaseValue = 0;
            Verditius.GetAttribute(AttributeType.Dexterity).BaseValue = 2;
            Verditius.GetAttribute(AttributeType.Quickness).BaseValue = -3;
            Verditius.GetAttribute(AttributeType.Intelligence).BaseValue = 3;
            Verditius.GetAttribute(AttributeType.Communication).BaseValue = -2;
            Verditius.GetAttribute(AttributeType.Presence).BaseValue = 2;
            Verditius.GetAttribute(AttributeType.Perception).BaseValue = 2;

            Verditius.GetAbility(MagicArts.Creo).AddExperience(15);
            Verditius.GetAbility(MagicArts.Intellego).AddExperience(0);
            Verditius.GetAbility(MagicArts.Muto).AddExperience(0);
            Verditius.GetAbility(MagicArts.Perdo).AddExperience(0);
            Verditius.GetAbility(MagicArts.Rego).AddExperience(15);
            Verditius.GetAbility(MagicArts.Animal).AddExperience(0);
            Verditius.GetAbility(MagicArts.Aquam).AddExperience(0);
            Verditius.GetAbility(MagicArts.Auram).AddExperience(0);
            Verditius.GetAbility(MagicArts.Corpus).AddExperience(0);
            Verditius.GetAbility(MagicArts.Herbam).AddExperience(0);
            Verditius.GetAbility(MagicArts.Ignem).AddExperience(0);
            Verditius.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Verditius.GetAbility(MagicArts.Mentem).AddExperience(0);
            Verditius.GetAbility(MagicArts.Terram).AddExperience(15);
            Verditius.GetAbility(MagicArts.Vim).AddExperience(15);
            Verditius.GetAbility(Abilities.AreaLore).AddExperience(15);
            Verditius.GetAbility(Abilities.ArtesLiberales).AddExperience(30);
            Verditius.GetAbility(Abilities.English).AddExperience(75);
            Verditius.GetAbility(Abilities.Etiquette).AddExperience(30);
            Verditius.GetAbility(Abilities.Latin).AddExperience(75);
            Verditius.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Verditius.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Verditius.GetAbility(Abilities.Penetration).AddExperience(5);
            Verditius.GetAbility(Abilities.Craft).AddExperience(30);
            Verditius.GetAbility(Abilities.Philosophae).AddExperience(5);
        }
    }
}