using Microsoft.Extensions.Options;
using Sproutopia.Models;
using System.Diagnostics;

namespace Sproutopia.Managers
{
    public class TickManager : ITickManager
    {

        private readonly int _tickDuration;
        public int CurrentTick { get; private set; }
        private Stopwatch Timer { get; }
        private bool IsStep { get; set; }
        private readonly SproutopiaGameSettings _gameSettings;

        public TickManager(IOptions<SproutopiaGameSettings> settings)
        {
            this.CurrentTick = 0;
            _gameSettings = settings.Value;
            this._tickDuration = settings.Value.TickRate;
            this.Timer = new Stopwatch();
        }

        public void StartTimer() => Timer.Start();
        public void Pause() => Timer.Stop();
        public void Stop() => Timer.Reset(); //TODO: Close off game
        public void Step()
        {
            Timer.Start();
            IsStep = true;
        }
        public void Continue() => StartTimer();
        public bool ShouldContinue()
        {
            if (!Timer.IsRunning) return false;
            if (IsStep)
            {
                Timer.Stop();
                CurrentTick++;
                IsStep = false;
                return true;
            }
            if (Timer.ElapsedMilliseconds < _tickDuration * CurrentTick) return false;
            //Log the difference + the current time

            //TODO: remove after all testing is done
            CurrentTick++;
            return true;
            //Log the difference + the current time, so we can tell if we have extra time
        }
    }
}
