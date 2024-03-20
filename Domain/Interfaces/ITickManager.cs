namespace Sproutopia.Managers
{
    public interface ITickManager
    {
        public int CurrentTick { get; }
        /// <summary>
        /// Start timer
        /// </summary>
        public void StartTimer();

        /// <summary>
        /// Pause the timer. For testing and development only!
        /// </summary>
        public void Pause();

        public void Stop();
        public void Step();

        public void Continue();

        /// <summary>
        /// Update the tick if the specified duration has elapsed
        /// </summary>
        public bool ShouldContinue();

    }
}
