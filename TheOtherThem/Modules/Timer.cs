using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheOtherThem.Modules
{
    public class Timer
    {
        public static TimerManager ManagerInstance { get; private set; }
        public static List<Timer> AllTimers { get; set; }

        public string Name { get; set; }
        public float Time { get; set; }
        public Action<bool> Callback { get; set; }
        public Func<bool> TerminationCheck { get; set; }
        public bool Started { get; private set; }

        private static readonly Action<bool> _emptBoolCallback = _ => { };
        private static readonly Func<bool> _emptyBoolCallback = () => false;

        public Timer() { }
        public Timer(string name)
        {
            Name = name;
            Time = float.PositiveInfinity;
            Callback = _emptBoolCallback;
            TerminationCheck = _emptyBoolCallback;
        }
        public Timer(string name, float time)
        {
            Name = name;
            Time = time;
            Callback = _emptBoolCallback;
            TerminationCheck = _emptyBoolCallback;
        }
        public Timer(string name, float time, Action<bool> callback)
        {
            Name = name;
            Time = time;
            Callback = callback;
            TerminationCheck = _emptyBoolCallback;
        }
        public Timer(string name, float time, Action<bool> callback, Func<bool> terminationCheck)
        {
            Name = name;
            Time = time;
            Callback = callback;
            TerminationCheck = terminationCheck;
        }

        public Timer Start()
        {
            if (!AllTimers.Contains(this))
                AllTimers.Add(this);
            Started = true;

            Main.Logger.LogInfo($"Timer {Name} started");

            return this;
        }

        public Timer Pause()
        {
            Started = false;
            return this;
        }

        public Timer Continue()
        {
            Started = true;
            return this;
        }

        public Timer Reset()
        {
            Started = false;
            Time = 0f;
            Callback = _emptBoolCallback;
            return this;
        }

        public void SetUnused() 
        { 
            AllTimers.Remove(this);
            Main.Logger.LogInfo($"Timer {Name} marked as unused");
        }

        public class TimerManager : MonoBehaviour
        {
            void Start()
            {
                ManagerInstance = this;

                name = nameof(TimerManager);
                AllTimers = new();
                Main.Logger.LogInfo(nameof(TimerManager) + " initialized");
            }

            void Update()
            {
                foreach (var timer in AllTimers.Where(t => t.Started))
                {
                    timer.Time -= UnityEngine.Time.deltaTime;

                    if (timer.TerminationCheck())
                    {
                        timer.SetUnused();
                        timer.Callback(false);
                    }

                    if (timer.Time <= 0)
                    {
                        timer.Pause();
                        Main.Logger.LogInfo($"Timer {timer.Name} gone off");
                        timer.Callback(true);
                    }
                }
            }
        }
    }
}