using System.Collections.Generic;
using WizardMonks.Decisions.Goals;
using WizardMonks.Models.Characters;

namespace WizardMonks.Instances
{
    public static class Founders
    {
        public static Magus Bjornaer { get; private set; }
        public static Magus Bonisgaus { get; private set; }
        public static Magus Criamon { get; private set; }
        public static Magus Diedne { get; private set; }
        public static Magus Flambeau { get; private set; }
        public static Magus Guernicus { get; private set; }
        public static Magus Jerbiton { get; private set; }
        public static Magus Mercere { get; private set; }
        public static Magus Merinita { get; private set; }
        public static Magus Tremere { get; private set; }
        public static Magus Tytalus { get; private set; }
        public static Magus Verditius { get; private set; }

        public static IEnumerable<Magus> GetEnumerator()
        {
            yield return Bjornaer;
            yield return Bonisgaus;
            yield return Criamon;
            yield return Diedne;
            yield return Flambeau;
            yield return Guernicus;
            yield return Jerbiton;
            yield return Mercere;
            yield return Merinita;
            yield return Tremere;
            yield return Tytalus;
            yield return Verditius;
        }

        static Founders()
        {
            BuildBjornaer();
            BuildBonisagus();
            BuildCriamon();
            BuildDiedne();
            BuildFlambeau();
            BuildGuernicus();
            BuildJerbiton();
            BuildMercere();
            BuildMerinita();
            BuildTremere();
            BuildTytalus();
            BuildVerditius();
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
                // O: High but focused. Inquisitive about the natural world, but not human arts. Highly unconventional.
                [HexacoFacet.AestheticAppreciation] = 0.8,
                [HexacoFacet.Inquisitiveness] = 1.8,
                [HexacoFacet.Creativity] = 1.3,
                [HexacoFacet.Unconventionality] = 1.9
            });

            Dictionary<string, double> reputation = new()
            {
                { "Bjornaer Lore", 2 },
                { "Heartbeast", 2 },
                { "Animal", 2}
            };


            Bjornaer = new(HousesEnum.Bjornaer, 80, bjornaerPersonality, reputation)
            {
                Name = "Bjornaer",
            };

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

            ApprenticeGoal apprentice = new(Bjornaer, null, 1);
            Bjornaer.AddGoal(apprentice);

            /*AbilityScoreGoal goal = new(Bjornaer, null, 1, MagicArts.Animal, 20);
            Bjornaer.AddGoal(goal);

            AbilityScoreGoal goal = new AbilityScoreGoal(Bjornaer, null, 1.01, Abilities.BjornaerLore, 5);
            Bjornaer.AddGoal(goal);

            goal = new AbilityScoreGoal(Bjornaer, null, 1.02, Abilities.Heartbeast, 5);
            Bjornaer.AddGoal(goal);*/
        }

        public static void BuildBonisagus()
        {
            HermeticTheory bonisagusTheory = new HermeticTheory("");
            bonisagusTheory.KnownHermeticAbilities.Add(Abilities.ParmaMagica);
            bonisagusTheory.KnownDurations.Add(Models.Spells.Durations.Instantaneous);
            bonisagusTheory.KnownRanges.Add(Models.Spells.Ranges.Touch);
            bonisagusTheory.KnownTargets.Add(Models.Spells.Targets.Individual);

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

            Dictionary<string, double> reputation = new()
            {
                { "Magic Theory", 2 }
            };

            Bonisgaus = new(HousesEnum.Bonisagus, 80, bonisagusPersonality, reputation)
            {
                Name = "Bonisagus"
            };

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

            //Testing to see whether the reputational modifiers make these ability score goals unnecessary
            /*AbilityScoreGoal goal = new(Bonisgaus, null, 1, Abilities.MagicTheory, 10);
            Bonisgaus.AddGoal(goal);*/

            ApprenticeGoal apprentice = new(Bonisgaus, null, 1);
            Bonisgaus.AddGoal(apprentice);
        }

        public static void BuildCriamon()
        {
            var criamonPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                // H: High. Genuinely modest and sincere, though his sincerity is veiled in riddles.
                [HexacoFacet.Sincerity] = 1.8,
                [HexacoFacet.Fairness] = 1.4,
                [HexacoFacet.GreedAvoidance] = 1.9,
                [HexacoFacet.Modesty] = 2.0,
                // E: Extremely Low. The ideal of emotional detachment. Unmoved by fear, anxiety, or sentiment.
                [HexacoFacet.Fearfulness] = 0.1,
                [HexacoFacet.Anxiety] = 0.1,
                [HexacoFacet.Dependence] = 0.1,
                [HexacoFacet.Sentimentality] = 0.2,
                // X: Extremely Low. The ultimate hermit. He has no need for society or social validation.
                [HexacoFacet.SocialSelfEsteem] = 0.2,
                [HexacoFacet.SocialBoldness] = 0.1,
                [HexacoFacet.Sociability] = 0.1,
                [HexacoFacet.Liveliness] = 0.3,
                // A: High. Patient and gentle, as conflict is a worldly distraction.
                [HexacoFacet.Forgiveness] = 1.6,
                [HexacoFacet.Gentleness] = 1.8,
                [HexacoFacet.Flexibility] = 1.5,
                [HexacoFacet.Patience] = 1.9,
                // C: Low. Worldly organization, diligence, and prudence are irrelevant to the Path of the Soul.
                [HexacoFacet.Organization] = 0.4,
                [HexacoFacet.Diligence] = 0.6,
                [HexacoFacet.Perfectionism] = 0.5,
                [HexacoFacet.Prudence] = 0.3,
                // O: Extremely High. The embodiment of seeking new and unconventional experiences and ideas.
                [HexacoFacet.AestheticAppreciation] = 1.5,
                [HexacoFacet.Inquisitiveness] = 2.0,
                [HexacoFacet.Creativity] = 1.8,
                [HexacoFacet.Unconventionality] = 2.0
            });

            Dictionary<string, double> reputation = new()
            {
                { "Criamon Lore", 2 },
                { "Enigmatic Wisdom", 2 }
            };

            Criamon = new(HousesEnum.Criamon, 80, criamonPersonality, reputation)
            {
                Name = "Criamon"
            };

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

            ApprenticeGoal app = new(Criamon, null, 1);
            Criamon.AddGoal(app);
        }

        public static void BuildDiedne()
        {
            var diednePersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                // H: Low. Capricious and proud, her sincerity is fleeting and she is not modest.
                [HexacoFacet.Sincerity] = 0.4,
                [HexacoFacet.Fairness] = 0.7,
                [HexacoFacet.GreedAvoidance] = 0.8,
                [HexacoFacet.Modesty] = 0.3,
                // E: High. Prone to anxiety (paranoia) and powerful sentimental attachments.
                [HexacoFacet.Fearfulness] = 1.2,
                [HexacoFacet.Anxiety] = 1.7,
                [HexacoFacet.Dependence] = 1.1,
                [HexacoFacet.Sentimentality] = 1.8,
                // X: Very High. A charismatic and socially dominant leader who thrives in the spotlight.
                [HexacoFacet.SocialSelfEsteem] = 1.9,
                [HexacoFacet.SocialBoldness] = 1.8,
                [HexacoFacet.Sociability] = 1.9,
                [HexacoFacet.Liveliness] = 2.0,
                // A: Low. Quick to anger, holds grudges, and is inflexible when her authority is challenged.
                [HexacoFacet.Forgiveness] = 0.4,
                [HexacoFacet.Gentleness] = 0.6,
                [HexacoFacet.Flexibility] = 0.5,
                [HexacoFacet.Patience] = 0.3,
                // C: Very Low. Famously imprudent, she acted on passion rather than careful planning.
                [HexacoFacet.Organization] = 0.7,
                [HexacoFacet.Diligence] = 1.2,
                [HexacoFacet.Perfectionism] = 0.8,
                [HexacoFacet.Prudence] = 0.1,
                // O: High. Creative, appreciative of beauty, and deeply unconventional due to her fae ties.
                [HexacoFacet.AestheticAppreciation] = 1.9,
                [HexacoFacet.Inquisitiveness] = 1.4,
                [HexacoFacet.Creativity] = 1.8,
                [HexacoFacet.Unconventionality] = 1.7
            });

            Dictionary<string, double> reputation = new()
            {

            };

            Diedne = new(HousesEnum.Diedne, 80, diednePersonality, reputation)
            {
                Name = "Diedne"
            };

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

            ApprenticeGoal app = new(Diedne, null, 1);
            Diedne.AddGoal(app);
        }

        public static void BuildFlambeau()
        {
            var flambeauPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                // H: Low. Extremely immodest and proud, though he holds to a warrior's code of fairness.
                [HexacoFacet.Sincerity] = 0.7,
                [HexacoFacet.Fairness] = 1.5,
                [HexacoFacet.GreedAvoidance] = 1.1,
                [HexacoFacet.Modesty] = 0.1,
                // E: Very Low. Unflappable, fearless, and not given to sentiment.
                [HexacoFacet.Fearfulness] = 0.1,
                [HexacoFacet.Anxiety] = 0.2,
                [HexacoFacet.Dependence] = 0.2,
                [HexacoFacet.Sentimentality] = 0.3,
                // X: High. Not sociable in a friendly way, but bold, lively, and dominant in social settings.
                [HexacoFacet.SocialSelfEsteem] = 1.8,
                [HexacoFacet.SocialBoldness] = 2.0,
                [HexacoFacet.Sociability] = 1.1,
                [HexacoFacet.Liveliness] = 1.7,
                // A: Rock Bottom. He solves problems through force, not forgiveness, gentleness, or patience.
                [HexacoFacet.Forgiveness] = 0.1,
                [HexacoFacet.Gentleness] = 0.1,
                [HexacoFacet.Flexibility] = 0.2,
                [HexacoFacet.Patience] = 0.1,
                // C: High, but focused on martial pursuits. Diligent in training, but can be imprudent.
                [HexacoFacet.Organization] = 1.1,
                [HexacoFacet.Diligence] = 1.8,
                [HexacoFacet.Perfectionism] = 1.4,
                [HexacoFacet.Prudence] = 0.6,
                // O: Low. He is interested in perfecting the art of destruction, not in new ideas or aesthetics.
                [HexacoFacet.AestheticAppreciation] = 0.4,
                [HexacoFacet.Inquisitiveness] = 0.5,
                [HexacoFacet.Creativity] = 0.6,
                [HexacoFacet.Unconventionality] = 0.8
            });

            Dictionary<string, double> reputation = new()
            {
                {"Ignem", 2.0 },
                { "Penetration", 2.0 },
                { "Parma Magica", 2.0 }
            };

            Flambeau = new(HousesEnum.Flambeau, 80, flambeauPersonality, reputation)
            {
                Name = "Flambeau"
            };

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

            /*AbilityScoreGoal goal = new(Flambeau, null, 1, MagicArts.Ignem, 20);
            Flambeau.AddGoal(goal);

            goal = new AbilityScoreGoal(Flambeau, null, 1, Abilities.Penetration, 5);
            Flambeau.AddGoal(goal);

            goal = new AbilityScoreGoal(Flambeau, null, 1, Abilities.ParmaMagica, 5);
            Flambeau.AddGoal(goal);*/

            ApprenticeGoal app = new(Flambeau, null, 1);
            Flambeau.AddGoal(app);
        }

        public static void BuildGuernicus()
        {
            var guernicusPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                // H: Extremely High. The cornerstone of his character is absolute fairness and sincerity.
                [HexacoFacet.Sincerity] = 2.0,
                [HexacoFacet.Fairness] = 2.0,
                [HexacoFacet.GreedAvoidance] = 1.8,
                [HexacoFacet.Modesty] = 1.4,
                // E: Very Low. An impartial judge cannot be swayed by fear, anxiety, or sentiment.
                [HexacoFacet.Fearfulness] = 0.3,
                [HexacoFacet.Anxiety] = 0.2,
                [HexacoFacet.Dependence] = 0.4,
                [HexacoFacet.Sentimentality] = 0.1,
                // X: Low. He is an authority figure, not a socialite. His presence commands respect, not friendship.
                [HexacoFacet.SocialSelfEsteem] = 1.4,
                [HexacoFacet.SocialBoldness] = 1.1,
                [HexacoFacet.Sociability] = 0.5,
                [HexacoFacet.Liveliness] = 0.6,
                // A: Extremely Low. The law is not gentle, patient, or flexible. He does not forgive, he sentences.
                [HexacoFacet.Forgiveness] = 0.1,
                [HexacoFacet.Gentleness] = 0.2,
                [HexacoFacet.Flexibility] = 0.1,
                [HexacoFacet.Patience] = 0.4,
                // C: Extremely High. Meticulously organized, diligent in his duties, and supremely prudent.
                [HexacoFacet.Organization] = 2.0,
                [HexacoFacet.Diligence] = 2.0,
                [HexacoFacet.Perfectionism] = 1.7,
                [HexacoFacet.Prudence] = 1.9,
                // O: Very Low. He values precedent and tradition (the Code) over creativity and unconventional ideas.
                [HexacoFacet.AestheticAppreciation] = 0.5,
                [HexacoFacet.Inquisitiveness] = 0.7,
                [HexacoFacet.Creativity] = 0.3,
                [HexacoFacet.Unconventionality] = 0.1
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Terram", 2.0 },
                { "Code of Hermes", 2.0 },
                { "Magic Theory", 1.5 }
            };

            Guernicus = new(HousesEnum.Guernicus, 80, guernicusPersonality, reputationMultipliers)
            {
                Name = "Guernicus"            
            };

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

            /*AbilityScoreGoal goal = new(Guernicus, null, 1, MagicArts.Terram, 20);
            Guernicus.AddGoal(goal);

            goal = new AbilityScoreGoal(Guernicus, null, 1.5, Abilities.CodeOfHermes, 5);
            Guernicus.AddGoal(goal);

            goal = new AbilityScoreGoal(Guernicus, null, 1, Abilities.MagicTheory, 5);
            Guernicus.AddGoal(goal);*/

            ApprenticeGoal app = new(Guernicus, null, 1);
            Guernicus.AddGoal(app);
        }

        public static void BuildJerbiton()
        {
            var jerbitonPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                // H: High. Sincere and modest, avoids the greed and arrogance common among magi.
                [HexacoFacet.Sincerity] = 1.6,
                [HexacoFacet.Fairness] = 1.3,
                [HexacoFacet.GreedAvoidance] = 1.7,
                [HexacoFacet.Modesty] = 1.5,
                // E: High. A sensitive soul, emotionally responsive and sentimental.
                [HexacoFacet.Fearfulness] = 1.1,
                [HexacoFacet.Anxiety] = 1.3,
                [HexacoFacet.Dependence] = 1.4,
                [HexacoFacet.Sentimentality] = 1.8,
                // X: High. Sociable, lively, and comfortable in mundane society.
                [HexacoFacet.SocialSelfEsteem] = 1.6,
                [HexacoFacet.SocialBoldness] = 1.1,
                [HexacoFacet.Sociability] = 1.8,
                [HexacoFacet.Liveliness] = 1.7,
                // A: Very High. The diplomat of the Founders; patient, gentle, and flexible.
                [HexacoFacet.Forgiveness] = 1.7,
                [HexacoFacet.Gentleness] = 1.9,
                [HexacoFacet.Flexibility] = 1.6,
                [HexacoFacet.Patience] = 1.8,
                // C: Average. He is not lazy, but his focus is not on ruthless efficiency.
                [HexacoFacet.Organization] = 1.3,
                [HexacoFacet.Diligence] = 1.0,
                [HexacoFacet.Perfectionism] = 1.2,
                [HexacoFacet.Prudence] = 1.1,
                // O: Very High. The patron of the arts is defined by his appreciation for aesthetics and creativity.
                [HexacoFacet.AestheticAppreciation] = 2.0,
                [HexacoFacet.Inquisitiveness] = 1.6,
                [HexacoFacet.Creativity] = 1.9,
                [HexacoFacet.Unconventionality] = 1.2
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Etiquette", 2.0 },
                { "Artes Liberales", 2.0 },
                { "Finesse", 2.0 },
                { "Imaginem", 2.0 }
            };

            Jerbiton = new(HousesEnum.Jerbiton, 80, jerbitonPersonality, reputationMultipliers)
            {
                Name = "Jerbiton"            
            };

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
            Jerbiton.GetAbility(Abilities.Etiquette).AddExperience(30);
            Jerbiton.GetAbility(Abilities.Latin).AddExperience(75);
            Jerbiton.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Jerbiton.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Jerbiton.GetAbility(Abilities.Penetration).AddExperience(0);
            Jerbiton.GetAbility(Abilities.Finesse).AddExperience(15);

            /*AbilityScoreGoal goal = new(Jerbiton, null, 1, MagicArts.Imaginem, 20);
            Jerbiton.AddGoal(goal);

            goal = new AbilityScoreGoal(Jerbiton, null, 0.1, Abilities.ArtesLiberales, 5);
            Jerbiton.AddGoal(goal);

            goal = new AbilityScoreGoal(Jerbiton, null, 0.1, Abilities.Etiquette, 5);
            Jerbiton.AddGoal(goal);

            goal = new AbilityScoreGoal(Jerbiton, null, 1.01, Abilities.Finesse, 5);
            Jerbiton.AddGoal(goal);*/

            ApprenticeGoal app = new(Jerbiton, null, 1);
            Jerbiton.AddGoal(app);
        }

        public static void BuildMercere()
        {
            var mercerePersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                // H: High. Sincerity and fairness are the currency of a trusted messenger.
                [HexacoFacet.Sincerity] = 1.8,
                [HexacoFacet.Fairness] = 1.7,
                [HexacoFacet.GreedAvoidance] = 1.4,
                [HexacoFacet.Modesty] = 1.6,
                // E: Low. Must be fearless to travel the roads, and not overly anxious or dependent.
                [HexacoFacet.Fearfulness] = 0.3,
                [HexacoFacet.Anxiety] = 0.5,
                [HexacoFacet.Dependence] = 0.4,
                [HexacoFacet.Sentimentality] = 0.8,
                // X: Mid-to-High. Sociable enough to deal with people everywhere, but not a boisterous leader.
                [HexacoFacet.SocialSelfEsteem] = 1.2,
                [HexacoFacet.SocialBoldness] = 1.4,
                [HexacoFacet.Sociability] = 1.6,
                [HexacoFacet.Liveliness] = 1.3,
                // A: High. Must be patient and flexible to deal with the demands of his clients and the dangers of the road.
                [HexacoFacet.Forgiveness] = 1.4,
                [HexacoFacet.Gentleness] = 1.3,
                [HexacoFacet.Flexibility] = 1.5,
                [HexacoFacet.Patience] = 1.7,
                // C: Very High. The entire House is built on the diligence and prudence of its members.
                [HexacoFacet.Organization] = 1.6,
                [HexacoFacet.Diligence] = 1.9,
                [HexacoFacet.Perfectionism] = 1.1,
                [HexacoFacet.Prudence] = 1.8,
                // O: Low. He is a practical man focused on his task, not on abstract ideas or art.
                [HexacoFacet.AestheticAppreciation] = 0.8,
                [HexacoFacet.Inquisitiveness] = 0.9,
                [HexacoFacet.Creativity] = 0.7,
                [HexacoFacet.Unconventionality] = 0.6
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Mentem", 2.0 },
                { "Etiquette", 1.5 },
                { "Area Lore", 1.5 }
            };

            Mercere = new(HousesEnum.Mercere, 80, mercerePersonality, reputationMultipliers)
            {
                Name = "Mercere"
            };

            Mercere.GetAttribute(AttributeType.Stamina).BaseValue = 1;
            Mercere.GetAttribute(AttributeType.Strength).BaseValue = 1;
            Mercere.GetAttribute(AttributeType.Dexterity).BaseValue = 1;
            Mercere.GetAttribute(AttributeType.Quickness).BaseValue = 1;
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

            /*AbilityScoreGoal goal = new(Mercere, null, 1, MagicArts.Mentem, 20);
            Mercere.AddGoal(goal);

            goal = new AbilityScoreGoal(Mercere, null, 0.1, Abilities.Etiquette, 5);
            Mercere.AddGoal(goal);*/

            ApprenticeGoal app = new(Mercere, null, 1);
            Mercere.AddGoal(app);
        }

        public static void BuildMerinita()
        {
            var merinitaPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                // H: Low. Faerie morality is not human morality. Sincerity is questionable, and she is proud.
                [HexacoFacet.Sincerity] = 0.5,
                [HexacoFacet.Fairness] = 0.6,
                [HexacoFacet.GreedAvoidance] = 1.3,
                [HexacoFacet.Modesty] = 0.4,
                // E: Very High. Prone to powerful, sweeping emotions, both terrifying and beautiful.
                [HexacoFacet.Fearfulness] = 1.4,
                [HexacoFacet.Anxiety] = 1.1,
                [HexacoFacet.Dependence] = 1.6,
                [HexacoFacet.Sentimentality] = 2.0,
                // X: Mid-range. Can be sociable and lively, but also withdrawn and mysterious.
                [HexacoFacet.SocialSelfEsteem] = 1.5,
                [HexacoFacet.SocialBoldness] = 0.9,
                [HexacoFacet.Sociability] = 1.2,
                [HexacoFacet.Liveliness] = 1.7,
                // A: Low. Can be gentle one moment and unforgivingly cruel the next, like nature itself.
                [HexacoFacet.Forgiveness] = 0.6,
                [HexacoFacet.Gentleness] = 1.7,
                [HexacoFacet.Flexibility] = 0.8,
                [HexacoFacet.Patience] = 0.5,
                // C: Low. Not driven by human concepts of organization or diligence. Acts on whims.
                [HexacoFacet.Organization] = 0.5,
                [HexacoFacet.Diligence] = 0.8,
                [HexacoFacet.Perfectionism] = 1.4,
                [HexacoFacet.Prudence] = 0.4,
                // O: Extremely High. The ultimate font of creativity, beauty, and strange, unconventional ideas.
                [HexacoFacet.AestheticAppreciation] = 2.0,
                [HexacoFacet.Inquisitiveness] = 1.7,
                [HexacoFacet.Creativity] = 2.0,
                [HexacoFacet.Unconventionality] = 1.9
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Herbam", 2.0 },
                { "Animal", 1.5 },
                { "Creo", 1.5 },
                { "Intelligo", 1.5 },
                { "Muto", 1.5 },
                { "Perdo", 1.5 },
                { "Rego", 1.5 },
                { "Merinita Lore", 2.0 }
            };

            Merinita = new(HousesEnum.Merinita, 80, merinitaPersonality, reputationMultipliers)
            {
                Name = "Merinita"
            };

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

            /*AbilityScoreGoal goal = new(Merinita, null, 1, MagicArts.Herbam, 20);
            Merinita.AddGoal(goal);

            goal = new AbilityScoreGoal(Merinita, null, 1, MagicArts.Animal, 15);
            Merinita.AddGoal(goal);

            goal = new AbilityScoreGoal(Merinita, null, 1, MagicArts.Creo, 5);
            Merinita.AddGoal(goal);

            goal = new AbilityScoreGoal(Merinita, null, 1, MagicArts.Intellego, 5);
            Merinita.AddGoal(goal);

            goal = new AbilityScoreGoal(Merinita, null, 1, MagicArts.Muto, 5);
            Merinita.AddGoal(goal);

            goal = new AbilityScoreGoal(Merinita, null, 1, MagicArts.Perdo, 5);
            Merinita.AddGoal(goal);

            goal = new AbilityScoreGoal(Merinita, null, 1, MagicArts.Rego, 5);
            Merinita.AddGoal(goal);

            goal = new AbilityScoreGoal(Merinita, null, 1, Abilities.MerinitaLore, 5);
            Merinita.AddGoal(goal);*/
        }

        public static void BuildTremere()
        {
            var tremerePersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                // H: Low. Politics and ambition require a flexible relationship with sincerity and modesty.
                [HexacoFacet.Sincerity] = 0.3,
                [HexacoFacet.Fairness] = 0.6,
                [HexacoFacet.GreedAvoidance] = 0.2,
                [HexacoFacet.Modesty] = 0.4,
                // E: Low. A successful leader cannot afford to be ruled by fear or anxiety.
                [HexacoFacet.Fearfulness] = 1.4,
                [HexacoFacet.Anxiety] = 1.0,
                [HexacoFacet.Dependence] = 1.4,
                [HexacoFacet.Sentimentality] = 0.5,
                // X: Very High. A master of social maneuvering, bold and self-confident.
                [HexacoFacet.SocialSelfEsteem] = 1.3,
                [HexacoFacet.SocialBoldness] = 1.9,
                [HexacoFacet.Sociability] = 1.3,
                [HexacoFacet.Liveliness] = 1.3,
                // A: Low. Not a gentle or forgiving leader; demands loyalty and is inflexible in his goals.
                [HexacoFacet.Forgiveness] = 0.5,
                [HexacoFacet.Gentleness] = 0.4,
                [HexacoFacet.Flexibility] = 0.3,
                [HexacoFacet.Patience] = 1.5,
                // C: Extremely High. The defining trait of House Tremere is meticulous organization and diligence.
                [HexacoFacet.Organization] = 1.8,
                [HexacoFacet.Diligence] = 1.9,
                [HexacoFacet.Perfectionism] = 1.6,
                [HexacoFacet.Prudence] = 1.7,
                // O: Low. Values established power structures and proven methods over new, risky ideas.
                [HexacoFacet.AestheticAppreciation] = 0.6,
                [HexacoFacet.Inquisitiveness] = 0.8,
                [HexacoFacet.Creativity] = 0.7,
                [HexacoFacet.Unconventionality] = 1.2
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Penetration", 2.0 },
                { "Parma Magica", 2.0 }
            };

            Tremere = new(HousesEnum.Tremere, 80, tremerePersonality, reputationMultipliers)
            {
                Name = "Tremere"
            };

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

            IGoal goal = new ApprenticeGoal(Tremere, null, 5);
            Tremere.AddGoal(goal);

            /*goal = new AbilityScoreGoal(Tremere, null, 1, Abilities.Penetration, 5);
            Tremere.AddGoal(goal);

            goal = new AbilityScoreGoal(Tremere, null, 1, Abilities.ParmaMagica, 5);
            Tremere.AddGoal(goal);*/
        }

        public static void BuildTytalus()
        {
            var tytalusPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                // H: Low. Is not above manipulation (low sincerity) and is exceptionally proud (low modesty).
                [HexacoFacet.Sincerity] = 0.3,
                [HexacoFacet.Fairness] = 0.9,
                [HexacoFacet.GreedAvoidance] = 0.5,
                [HexacoFacet.Modesty] = 0.1,
                // E: Low. Fearless and not prone to anxiety; he thrives on stress.
                [HexacoFacet.Fearfulness] = 0.2,
                [HexacoFacet.Anxiety] = 0.3,
                [HexacoFacet.Dependence] = 0.1,
                [HexacoFacet.Sentimentality] = 0.4,
                // X: High. Seeks out social interaction as a venue for conflict and debate. Extremely bold.
                [HexacoFacet.SocialSelfEsteem] = 1.9,
                [HexacoFacet.SocialBoldness] = 2.0,
                [HexacoFacet.Sociability] = 1.5,
                [HexacoFacet.Liveliness] = 1.6,
                // A: Extremely Low. The core of his philosophy. He is unforgiving, inflexible, impatient, and certainly not gentle.
                [HexacoFacet.Forgiveness] = 0.1,
                [HexacoFacet.Gentleness] = 0.1,
                [HexacoFacet.Flexibility] = 0.2,
                [HexacoFacet.Patience] = 0.2,
                // C: High. Requires diligence and prudence to survive a life of constant conflict.
                [HexacoFacet.Organization] = 1.2,
                [HexacoFacet.Diligence] = 1.7,
                [HexacoFacet.Perfectionism] = 1.3,
                [HexacoFacet.Prudence] = 1.6,
                // O: High. Intellectually curious and creative in finding new ways to challenge himself and others.
                [HexacoFacet.AestheticAppreciation] = 0.9,
                [HexacoFacet.Inquisitiveness] = 1.9,
                [HexacoFacet.Creativity] = 1.6,
                [HexacoFacet.Unconventionality] = 1.5
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Rego", 2.0 },
                { "Mentem", 2.0 },
                { "Penetration", 2.0 },
                { "Finesse", 2.0 }
            };

            Tytalus = new(HousesEnum.Tytalus, 80, tytalusPersonality, reputationMultipliers)
            {
                Name = "Tytalus"
            };

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

            /*AbilityScoreGoal goal = new(Tytalus, null, 1, MagicArts.Rego, 20);
            Tytalus.AddGoal(goal);

            goal = new AbilityScoreGoal(Tytalus, null, 1, MagicArts.Mentem, 10);
            Tytalus.AddGoal(goal);

            goal = new AbilityScoreGoal(Tytalus, null, 1, Abilities.Penetration, 5);
            Tytalus.AddGoal(goal);

            goal = new AbilityScoreGoal(Tytalus, null, 1, Abilities.Finesse, 5);
            Tytalus.AddGoal(goal);*/

            ApprenticeGoal app = new(Tytalus, null, 1);
            Tytalus.AddGoal(app);
        }

        public static void BuildVerditius()
        {
            var verditiusPersonality = new Personality(new Dictionary<HexacoFacet, double>
            {
                // H: Very Low. Plagued by jealousy (low modesty, high greed) and not always sincere.
                [HexacoFacet.Sincerity] = 0.6,
                [HexacoFacet.Fairness] = 0.8,
                [HexacoFacet.GreedAvoidance] = 0.2,
                [HexacoFacet.Modesty] = 0.1,
                // E: High. Anxious about his status and the quality of his work; sentimental about his creations.
                [HexacoFacet.Fearfulness] = 0.8,
                [HexacoFacet.Anxiety] = 1.8,
                [HexacoFacet.Dependence] = 1.3,
                [HexacoFacet.Sentimentality] = 1.6,
                // X: Low. A reclusive craftsman who prefers the workshop to the salon.
                [HexacoFacet.SocialSelfEsteem] = 0.7,
                [HexacoFacet.SocialBoldness] = 0.6,
                [HexacoFacet.Sociability] = 0.4,
                [HexacoFacet.Liveliness] = 0.9,
                // A: Low. Impatient with lesser artisans, inflexible in his methods, and not gentle with his rivals.
                [HexacoFacet.Forgiveness] = 0.5,
                [HexacoFacet.Gentleness] = 0.4,
                [HexacoFacet.Flexibility] = 0.6,
                [HexacoFacet.Patience] = 0.3,
                // C: Extremely High. The ultimate perfectionist, organized and diligent in his craft.
                [HexacoFacet.Organization] = 1.8,
                [HexacoFacet.Diligence] = 2.0,
                [HexacoFacet.Perfectionism] = 2.0,
                [HexacoFacet.Prudence] = 1.4,
                // O: High. Must be creative and inquisitive to be a master inventor and enchanter.
                [HexacoFacet.AestheticAppreciation] = 1.7,
                [HexacoFacet.Inquisitiveness] = 1.6,
                [HexacoFacet.Creativity] = 1.9,
                [HexacoFacet.Unconventionality] = 1.1
            });

            Dictionary<string, double> reputationMultipliers = new()
            {
                { "Verditius Lore", 2.0 },
                { "Craft", 2.0 },
                { "Philosophae", 1.5 },
                { "Terram", 1.5 }
            };

            Verditius = new(HousesEnum.Verditius, 80, verditiusPersonality, reputationMultipliers)
            {
                Name = "Verditius"
            };

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

            /*AbilityScoreGoal goal = new(Verditius, null, 1, MagicArts.Intellego, 5);
            Verditius.AddGoal(goal);

            goal = new AbilityScoreGoal(Verditius, null, 1, MagicArts.Muto, 5);
            Verditius.AddGoal(goal);

            goal = new AbilityScoreGoal(Verditius, null, 1, MagicArts.Perdo, 5);
            Verditius.AddGoal(goal);

            goal = new AbilityScoreGoal(Verditius, null, 1, MagicArts.Rego, 5);
            Verditius.AddGoal(goal);

            goal = new AbilityScoreGoal(Verditius, null, 1, MagicArts.Terram, 20);
            Verditius.AddGoal(goal);

            goal = new AbilityScoreGoal(Verditius, null, 1, Abilities.Philosophae, 4);
            Verditius.AddGoal(goal);

            goal = new AbilityScoreGoal(Verditius, null, 1, Abilities.Craft, 5);
            Verditius.AddGoal(goal);*/

            ApprenticeGoal app = new(Verditius, null, 1);
            Verditius.AddGoal(app);
        }
    }
}
