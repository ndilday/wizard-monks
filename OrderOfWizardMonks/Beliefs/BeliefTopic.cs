using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMonks.Beliefs
{
    public class BeliefTopic
    {
        public string Name { get; private set; }
        public BeliefTopic(string name)
        {
            Name = name;
        }
    }

    public static class BeliefTopics
    {
        // Magic Art
        public static readonly BeliefTopic Creo = new("Creo");
        public static readonly BeliefTopic Intellego = new("Intellego");
        public static readonly BeliefTopic Muto = new("Muto");
        public static readonly BeliefTopic Perdo = new("Perdo");
        public static readonly BeliefTopic Rego = new("Rego");
        public static readonly BeliefTopic Animal = new("Animal");
        public static readonly BeliefTopic Aquam = new("Aquam");
        public static readonly BeliefTopic Auram = new("Auram");
        public static readonly BeliefTopic Corpus = new("Corpus");
        public static readonly BeliefTopic Herbam = new("Herbam");
        public static readonly BeliefTopic Ignem = new("Ignem");
        public static readonly BeliefTopic Imaginem = new("Imaginem");
        public static readonly BeliefTopic Mentem = new("Mentem");
        public static readonly BeliefTopic Terram = new("Terram");
        public static readonly BeliefTopic Vim = new("Vim");

        // Ability
        public static readonly BeliefTopic ArtesLiberales = new("ArtesLiberales");
        public static readonly BeliefTopic Latin = new("Latin");
        public static readonly BeliefTopic Athletics = new("Athletics");
        public static readonly BeliefTopic Awareness = new("Awareness");
        public static readonly BeliefTopic Brawl = new("Brawl");
        public static readonly BeliefTopic Concentration = new("Concentration");
        public static readonly BeliefTopic Craft = new("Craft");
        public static readonly BeliefTopic Etiquette = new("Etiquette");
        public static readonly BeliefTopic Finesse = new("Finesse");
        public static readonly BeliefTopic Guile = new("Guile");
        public static readonly BeliefTopic Leadership = new("Leadership");
        public static readonly BeliefTopic MagicTheory = new("MagicTheory");
        public static readonly BeliefTopic Medicine = new("Medicine");
        public static readonly BeliefTopic Occult = new("Occult");
        public static readonly BeliefTopic Ride = new("Ride");
        public static readonly BeliefTopic Stealth = new("Stealth");
        public static readonly BeliefTopic Survival = new("Survival");
        public static readonly BeliefTopic Teaching = new("Teaching");
        public static readonly BeliefTopic ThrownWeapons = new("ThrownWeapons");
        public static readonly BeliefTopic AnimalHandling = new("AnimalHandling");

        // Personality
        public static readonly BeliefTopic Sincerity = new("Sincerity");
        public static readonly BeliefTopic Fairness = new("Fairness");
        public static readonly BeliefTopic GreedAvoidance = new("GreedAvoidance");
        public static readonly BeliefTopic Modesty = new("Modesty");
        public static readonly BeliefTopic Fearfulness = new("Fearfulness");
        public static readonly BeliefTopic Anxiety = new("Anxiety");
        public static readonly BeliefTopic Dependence = new("Dependence");
        public static readonly BeliefTopic Sentimentality = new("Sentimentality");
        public static readonly BeliefTopic SocialSelfEsteem = new("SocialSelfEsteem");
        public static readonly BeliefTopic SocialBoldness = new("SocialBoldness");
        public static readonly BeliefTopic Sociability = new("Sociability");
        public static readonly BeliefTopic Liveliness = new("Liveliness");
        public static readonly BeliefTopic Forgiveness = new("Forgiveness");
        public static readonly BeliefTopic Gentleness = new("Gentleness");
        public static readonly BeliefTopic Flexibility = new("Flexibility");
        public static readonly BeliefTopic Patience = new("Patience");
        public static readonly BeliefTopic Organization = new("Organization");
        public static readonly BeliefTopic Diligence = new("Diligence");
        public static readonly BeliefTopic Perfectionism = new("Perfectionism");
        public static readonly BeliefTopic Prudence = new("Prudence");
        public static readonly BeliefTopic AestheticAppreciation = new("Aesthetic Appreciation");
        public static readonly BeliefTopic Inquisitiveness = new("Inquisitiveness");
        public static readonly BeliefTopic Creativity = new("Creativity");
        public static readonly BeliefTopic Unconventionality = new("Unconventionality");

        // Attribute
        public static readonly BeliefTopic Strength = new("Strength");
        public static readonly BeliefTopic Dexterity = new("Dexterity");
        public static readonly BeliefTopic Stamina = new("Stamina");
        public static readonly BeliefTopic Quickness = new("Quickness");
        public static readonly BeliefTopic Intelligence = new("Intelligence");
        public static readonly BeliefTopic Communication = new("Communication");
        public static readonly BeliefTopic Perception = new("Perception");
        public static readonly BeliefTopic Presence = new("Presence");
        
        // ... etc. This can be expanded infinitely.
    }
}
