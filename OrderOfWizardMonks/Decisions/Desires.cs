using System.Collections.Generic;
using System.Linq;
using WizardMonks.Economy;
using WizardMonks.Instances;

namespace WizardMonks.Decisions
{
    public class Desires
    {
        public readonly VisDesire[] VisDesires;
        // Properties are now read-only to the outside world
        public IReadOnlyList<BookDesire> BookDesires => _bookDesires;
        public IReadOnlyList<LabTextDesire> LabTextDesires => _labTextDesires;

        // Internal lists for modification
        private readonly List<BookDesire> _bookDesires;
        private readonly List<LabTextDesire> _labTextDesires;

        public Desires()
        {
            _bookDesires = new List<BookDesire>();
            _labTextDesires = new List<LabTextDesire>();

            // Initialize the VisDesire array
            VisDesires = new VisDesire[MagicArts.Count];
            for (int i = 0; i < MagicArts.Count; i++)
            {
                // Create a temporary list of arts to get the correct one by index.
                var artList = MagicArts.GetEnumerator().ToList();
                VisDesires[i] = new VisDesire(artList[i], 0);
            }
        }

        /// <summary>
        /// Adds a desire for a book on a specific topic. If a desire for this topic
        /// already exists, it updates it to the higher of the two minimum levels.
        /// </summary>
        public void AddBookDesire(BookDesire newBookDesire)
        {
            var existingDesire = _bookDesires.FirstOrDefault(d => d.Ability == newBookDesire.Ability && d.Character == newBookDesire.Character);

            if (existingDesire != null)
            {
                // A desire already exists for this topic. Update the level if the new desire is for a higher level.
                if (newBookDesire.CurrentLevel > existingDesire.CurrentLevel)
                {
                    // Remove the old one and add the new one to update the level.
                    // A more complex implementation might have a mutable CurrentLevel property.
                    _bookDesires.Remove(existingDesire);
                    _bookDesires.Add(newBookDesire);
                }
            }
            else
            {
                // No existing desire, so just add it.
                _bookDesires.Add(newBookDesire);
            }
        }

        /// <summary>
        /// Adds a desire for a lab text for a specific spell base. If a desire for this
        /// spell base already exists, it updates it to the higher of the two minimum levels.
        /// </summary>
        public void AddLabTextDesire(LabTextDesire newLabTextDesire)
        {
            var existingDesire = _labTextDesires.FirstOrDefault(d => d.SpellBase == newLabTextDesire.SpellBase && d.Character == newLabTextDesire.Character);

            if (existingDesire != null)
            {
                // Update the level if the new desire is more restrictive
                if (newLabTextDesire.MinimumLevel > existingDesire.MinimumLevel || newLabTextDesire.MaximumLevel < existingDesire.MaximumLevel)
                {
                    _labTextDesires.Remove(existingDesire);
                    _labTextDesires.Add(newLabTextDesire);
                }
            }
            else
            {
                _labTextDesires.Add(newLabTextDesire);
            }
        }

        /// <summary>
        /// Adds a quantity of vis to the desire for a specific magical art.
        /// </summary>
        public void AddVisDesire(Ability art, double quantity)
        {
            if (!MagicArts.IsArt(art))
            {
                // Optionally throw an exception for invalid art types
                return;
            }

            var visDesire = VisDesires.FirstOrDefault(d => d.Art == art);
            if (visDesire != null)
            {
                visDesire.Quantity += quantity;
            }
            // else: This case should not happen due to constructor initialization,
            // but you could add error handling here if needed.
        }
    }
}
