using WizardMonks.Models.Characters;

namespace WizardMonks.Services.Characters
{
    /// <summary>
    /// Determines whether a character should undergo reflection this tick.
    /// Implementations are swappable — the reflection logic itself does not
    /// depend on the cadence trigger.
    /// </summary>
    public interface IReflectionPolicy
    {
        bool ShouldReflect(Character character, CharacterMemoryStream memoryStream, int currentTick);
    }
}