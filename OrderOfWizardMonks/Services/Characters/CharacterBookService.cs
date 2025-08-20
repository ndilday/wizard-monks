using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Core;
using WizardMonks.Economy;
using WizardMonks.Instances;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;

namespace WizardMonks.Services.Characters
{
    public static class CharacterBookService
    {
        public static void InvalidateWritableTopicsCache(this Character character)
        {
            character.IsWritableTopicsCacheClean = false;
            character.WritableTopicsCache.Clear();
        }
        public static IEnumerable<ABook> GetBooksInCollection(this Character character, Ability ability)
        {
            return character.Books.Where(b => b.Topic == ability);
        }

        public static IEnumerable<ABook> GetReadableBooksInCollection(this Character character, Ability ability)
        {

            return character.ReadableBooks.Where(b => b.Topic == ability);
        }

        public static ABook GetBestBookToRead(this Character character, Ability ability)
        {
            // TODO: may eventually want to take into account reading a slower summa before a higher quality tractatus?
            return character.ReadableBooks
                .Where(b => b.Topic == ability)
                .OrderByDescending(b => character.GetBookLevelGain(b))
                .ThenBy(b => b.Level)
                .FirstOrDefault();
        }

        public static ABook GetBestSummaToRead(this Character character, Ability ability)
        {
            return character.ReadableBooks
                .Where(b => b.Topic == ability & b.Level < 1000)
                .OrderByDescending(b => character.GetBookLevelGain(b))
                .ThenBy(b => b.Level)
                .FirstOrDefault();
        }

        public static IEnumerable<ABook> GetUnneededBooksFromCollection(this Character character)
        {
            return character.Books
                .Where(b => b.Author == character 
                    || character.BooksRead.Contains(b) && b.Level == 1000 
                    || character.GetAbility(b.Topic).Value >= b.Level);
        }

        public static bool ValidToRead(this Character character, ABook book)
        {
            return book.Author != character 
                && (!character.BooksRead.Contains(book) || book.Level != 1000 && character.GetAbility(book.Topic).Value < book.Level);
        }

        public static bool CanWriteTractatus(this Character character, CharacterAbilityBase charAbility)
        {
            return charAbility.GetTractatiiLimit() > character.GetTractatiiWrittenOnTopic(charAbility.Ability);
        }

        public static ushort GetTractatiiWrittenOnTopic(this Character character, Ability topic)
        {
            return (ushort)character.BooksWritten.Where(b => b.Topic == topic && b.Level == 1000).Count();
        }

        public static double GetBookLevelGain(this Character reader, ABook book)
        {
            if (book == null)
            {
                return 0;
            }

            // determine difference in ability using the new book compared to the old book
            return reader.GetAbility(book.Topic).GetValueGain(book.Quality, book.Level);
        }

        public static double GetAbilityMaximumFromReading(this Character reader, Ability ability)
        {
            CharacterAbilityBase charAbility = reader.GetAbility(ability).MakeCopy();
            double value = charAbility.Value;
            var books = reader.GetReadableBooksInCollection(ability);
            ABook summa = books.Where(b => b.Level < 1000).OrderBy(b => b.Level).FirstOrDefault();
            if (summa != null && summa.Level > value)
            {
                charAbility.AddExperience(1000, summa.Level);
            }
            foreach (ABook book in books.Where(b => b.Level == 1000))
            {
                charAbility.AddExperience(book.Quality);
            }
            return charAbility.Value;
        }

        public static bool CanWrite(this Character character)
        {
            return character.WritingLanguageCharacterAbility.Value >= 4 && character.WritingCharacterAbility.Value >= 1;
        }

        public static double RateLifetimeBookValue(this Character reader, ABook book, CharacterAbilityBase charAbility = null)
        {
            // see if it's a tractatus
            if (book.Level == 1000)
            {
                return reader.RateSeasonalExperienceGain(book.Topic, book.Quality);
            }
            if (charAbility == null)
            {
                charAbility = reader.GetAbility(book.Topic);
            }

            // if this book is beneath me, don't pay for it
            if (charAbility.Value >= book.Level)
            {
                return 0;
            }

            //TODO: see if we already have a summa on this topic
            ABook existingBook = reader.GetBestBookToRead(book.Topic);
            double expValue = charAbility.GetExperienceUntilLevel(book.Level);
            double bookSeasons = expValue / book.Quality;

            if (existingBook != null)
            {
                // for now, rate it in terms of marginal value difference
                double gainDifference = reader.RateSeasonalExperienceGain(book.Topic, book.Quality) - reader.RateSeasonalExperienceGain(existingBook.Topic, existingBook.Quality);
                return gainDifference * bookSeasons;
            }
            else if(!MagicArts.IsArt(book.Topic))
            {
                // if this is not a magic art, compare it to the value of practice
                double gainDifference = reader.RateSeasonalExperienceGain(book.Topic, book.Quality) - reader.RateSeasonalExperienceGain(book.Topic, 4);
                return gainDifference * bookSeasons;
            }
            else
            {
                return reader.RateSeasonalExperienceGain(book.Topic, book.Quality) * bookSeasons;
            }
        }

        public static void ReadBook(this Character reader, ABook book)
        {
            reader.Log.Add("Reading " + book.Title);
            CharacterAbilityBase ability = reader.GetAbility(book.Topic);

            // Hashset returns false if the item was already in the set
            bool previouslyRead = !reader.BooksRead.Add(book);

            if (!previouslyRead || book.Level != 1000 && ability.Value < book.Level)
            {
                ability.AddExperience(book.Quality, book.Level);
                if (!previouslyRead)
                {
                    foreach (var belief in book.BeliefPayload)
                    {
                        // Update belief about the author
                        reader.GetBeliefProfile(book.Author).AddOrUpdateBelief(
                            new Belief(belief.Topic, belief.Magnitude));

                        // Update stereotype about the author's house
                        // TODO: this should probably apply to personality, but not levels of skill
                        if (book.Author is Magus magus)
                        {
                            var houseSubject = Houses.GetSubject(magus.House);
                            reader.GetBeliefProfile(houseSubject).AddOrUpdateBelief(
                                new Belief(belief.Topic, belief.Magnitude * 0.20)); // Stereotype is 20% strength
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates the prospective BeliefPayload for a book that has not yet been written.
        /// This is used for valuation purposes in the GetBestBookToWrite method.
        /// </summary>
        public static List<Belief> GenerateProspectiveBeliefPayload(this Character author, ABook book)
        {
            var payload = new List<Belief>();
            var authorComm = author.GetAttributeValue(AttributeType.Communication);

            if (book is Tractatus tract)
            {
                payload.Add(new Belief(tract.Topic.AbilityName, 5));
            }
            else if (book is Summa summa)
            {
                // For a Summa, the prestige is derived from both its Quality and its Level.
                double magnitude = BeliefToReputationNormalizer.ArtFromSumma(summa.Quality, summa.Level);
                payload.Add(new Belief(summa.Topic.AbilityName, magnitude));
            }

            // All books reflect on the author's Communication skill.
            payload.Add(new Belief(BeliefTopics.Communication, BeliefToReputationNormalizer.FromAttributeScore(authorComm)));

            return payload;
        }

        public static ABook GetBestBookToWrite(this Character author)
        {
            if (author.IsBestBookCacheClean) return author.BestBookCache;
            if (GlobalEconomy.MostDesiredBookTopics == null) return null;

            // --- Step 1: Initialize variables for the search ---
            ABook bestBook = null;
            double currentBestBookValue = 0;
            double writingRate = author.GetAttributeValue(AttributeType.Communication) + author.GetAbility(author.WritingLanguage).Value;
            double prestigeMotivation = author.Personality.GetPrestigeMotivation();

            // --- Step 2: Ensure the cache of writable topics is populated ---
            if (!author.IsWritableTopicsCacheClean)
            {
                author.WritableTopicsCache.Clear();
                author.WritableTopicsCache.Union(author.GetAbilities().Where(a => a.Experience >= 15).ToHashSet());
                author.IsWritableTopicsCacheClean = true;
            }

            // --- Step 3: Iterate through topics with known market demand ---
            foreach (Ability ability in GlobalEconomy.MostDesiredBookTopics)
            {
                //var charAbility = _writableTopicsCache.FirstOrDefault(a => a.Ability == ability);
                var charAbility = author.GetAbility(ability);
                if (charAbility == null) continue;

                var desiresForTopic = GlobalEconomy.DesiredBooksByTopic[ability];
                var highestDemandDesire = desiresForTopic.OrderBy(d => d.CurrentLevel).FirstOrDefault(d => d.Character != author);
                if (highestDemandDesire == null) continue;

                // --- Step 4A: Evaluate writing a Summa for this topic ---
                double maxLevel = author.GetAbility(highestDemandDesire.Ability).Value / 2.0;
                for (double l = Math.Floor(maxLevel); l > highestDemandDesire.CurrentLevel; l--)
                {
                    double q = 6 + author.GetAttributeValue(AttributeType.Communication) + maxLevel - l;

                    var prospectiveSumma = new Summa
                    {
                        Author = author,
                        Quality = q,
                        Level = l,
                        Topic = highestDemandDesire.Ability,
                        Title = $"{highestDemandDesire.Ability.AbilityName} L{l:0.0}Q{q:0.0} Summa"
                    };

                    // Calculate Economic Value (based on vis equivalence)
                    double seasonsLeft = Math.Ceiling(l / writingRate);
                    if (!MagicArts.IsArt(prospectiveSumma.Topic)) seasonsLeft *= 5;
                    double economicValue = author.RateLifetimeBookValue(prospectiveSumma) / seasonsLeft;

                    // Calculate Prestige Value
                    var payload = author.GenerateProspectiveBeliefPayload(prospectiveSumma);
                    double prestigeValue = payload.Sum(b => author.CalculateBeliefValue(b));

                    // Calculate Total Value, modulated by personality
                    double totalValue = economicValue + prestigeValue * prestigeMotivation;

                    if (totalValue > currentBestBookValue)
                    {
                        currentBestBookValue = totalValue;
                        prospectiveSumma.Value = totalValue; // Store the combined value
                        bestBook = prospectiveSumma;
                    }
                }

                // --- Step 4B: Evaluate writing a Tractatus for this topic ---
                if (author.CanWriteTractatus(charAbility))
                {
                    ushort previouslyWrittenCount = author.GetTractatiiWrittenOnTopic(highestDemandDesire.Ability);
                    var prospectiveTractatus = new Tractatus
                    {
                        Author = author,
                        Quality = 6 + author.GetAttributeValue(AttributeType.Communication),
                        Topic = charAbility.Ability,
                        Title = $"{author.Name} {charAbility.Ability.AbilityName} T{previouslyWrittenCount}"
                    };

                    // Calculate Economic Value (vis value of 1 season of study)
                    double economicValue = author.RateSeasonalExperienceGain(prospectiveTractatus.Topic, prospectiveTractatus.Quality);

                    // Calculate Prestige Value
                    var payload = author.GenerateProspectiveBeliefPayload(prospectiveTractatus);
                    double prestigeValue = payload.Sum(b => author.CalculateBeliefValue(b));

                    // Calculate Total Value, modulated by personality
                    double totalValue = economicValue + prestigeValue * prestigeMotivation;

                    if (totalValue > currentBestBookValue)
                    {
                        currentBestBookValue = totalValue;
                        prospectiveTractatus.Value = totalValue;
                        bestBook = prospectiveTractatus;
                    }
                }
            }

            // --- Step 5: Cache and return the result ---
            author.IsBestBookCacheClean = true;
            author.BestBookCache = bestBook;
            return bestBook;
        }
    }
}
