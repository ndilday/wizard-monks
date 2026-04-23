using WizardMonks.Models.Characters;

namespace WizardMonks.Services.Characters
{
    /// <summary>
    /// Triggers reflection every tick. Appropriate at seasonal granularity.
    /// </summary>
    public sealed class TickSynchronizedReflectionPolicy : IReflectionPolicy
    {
        public bool ShouldReflect(Character character, CharacterMemoryStream memoryStream, int currentTick)
            => true;
    }
}