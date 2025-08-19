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
        public float InitialTime { get; set; } = 0f;
        public float ElapsedTime { get; set; } = 0f;
        public Action<bool> Callback { get; set; }
        public Func<bool> TerminationCheck { get; set; }
        public bool Started { get; private set; }

        private static readonly Action<bool> _emptBoolCallback = _ => { };
        private static readonly Func<bool> _emptyBoolCallback = () => false;

        public Timer() { }
        public Timer(string name)
        {
            Name = name;
            InitialTime = ElapsedTime = float.PositiveInfinity;
            Callback = _emptBoolCallback;
            TerminationCheck = _emptyBoolCallback;
        }
        public Timer(string name, float time)
        {
            Name = name;
            InitialTime = ElapsedTime = time;
            Callback = _emptBoolCallback;
            TerminationCheck = _emptyBoolCallback;
        }
        public Timer(string name, float time, Action<bool> callback)
        {
            Name = name;
            InitialTime = ElapsedTime = time;
            Callback = callback;
            TerminationCheck = _emptyBoolCallback;
        }
        public Timer(string name, float time, Action<bool> callback, Func<bool> terminationCheck)
        {
            Name = name;
            InitialTime = ElapsedTime = time;
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
            ElapsedTime = 0f;
            Callback = _emptBoolCallback;
            return this;
        }

        public void SetUnused()
        {
            AllTimers.Remove(this);
            Main.Logger.LogInfo($"Timer {Name} marked as unused, waiting for being collected by GC...");
        }

        public class TimerManager : MonoBehaviour
        {
            void Start()
            {
                ManagerInstance = this;

                name = nameof(TimerManager);
                DontDestroyOnLoad(this);

                AllTimers = new();
                Main.Logger.LogInfo(nameof(TimerManager) + " initialized");
            }

            void Update()
            {
                foreach (var timer in AllTimers.Where(t => t.Started))
                {
                    timer.ElapsedTime -= UnityEngine.Time.deltaTime;

                    if (timer.TerminationCheck())
                    {
                        timer.Pause();
                        Main.Logger.LogInfo($"Timer {timer.Name} terminated");
                        timer.Callback(false);
                    }

                    if (timer.ElapsedTime <= 0)
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