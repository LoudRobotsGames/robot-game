using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// /////////////////////////////////////////////////////////////////////////////////////////
//                                        TIMING
// 
// This is an improved implementation of coroutines that boasts zero per-frame memory allocations.
//  It serves as the "Time" portion of the Movement / Time plugin, which can be found here:
//  https://www.assetstore.unity3d.com/en/#!/content/50796
// 
// For manual, support, or upgrade guide visit http://trinary.tech/
//
// Created by Teal Rogers
// Trinary Software
// All rights preserved.
// /////////////////////////////////////////////////////////////////////////////////////////

namespace MovementEffects
{
    public class Timing : MonoBehaviour
    {
        private class WaitingProcess
        {
            public Timing Instance;
            public Segment Timing;
            public IEnumerator<float> Process;
            public IEnumerator<float> Trigger; 
            public readonly List<IEnumerator<float>> Tasks = new List<IEnumerator<float>>(); 
        }

        public float TimeBetweenSlowUpdateCalls = 1f / 7f;
        public int NumberOfUpdateCoroutines;
        public int NumberOfFixedUpdateCoroutines;
        public int NumberOfLateUpdateCoroutines;
        public int NumberOfSlowUpdateCoroutines;

        public System.Action<System.Exception, IEnumerator<float>> OnError; 
        public static System.Func<IEnumerator<float>, Segment, IEnumerator<float>> ReplacementFunction;
        private readonly List<WaitingProcess> _waitingProcesses = new List<WaitingProcess>();

        private bool _runningUpdate;
        private bool _runningFixedUpdate;
        private bool _runningLateUpdate;
        private bool _runningSlowUpdate;
        private int _nextUpdateProcessSlot;
        private int _nextLateUpdateProcessSlot;
        private int _nextFixedUpdateProcessSlot;
        private int _nextSlowUpdateProcessSlot;
        private ushort _framesSinceUpdate;

        private float _lastSlowUpdateCallTime;
        private const ushort FramesUntilMaintenance = 64;
        private const int ProcessArrayChunkSize = 128;

        private IEnumerator<float>[] UpdateProcesses = new IEnumerator<float>[ProcessArrayChunkSize * 4];
        private IEnumerator<float>[] LateUpdateProcesses = new IEnumerator<float>[ProcessArrayChunkSize];
        private IEnumerator<float>[] FixedUpdateProcesses = new IEnumerator<float>[ProcessArrayChunkSize];
        private IEnumerator<float>[] SlowUpdateProcesses = new IEnumerator<float>[ProcessArrayChunkSize];

        private static Timing _instance;
        public static Timing Instance
        {
            get
            {
                if (_instance == null || !_instance.gameObject)
                {
                    GameObject instanceHome = GameObject.Find("Movement Effects");

                    if(instanceHome == null)
                    {
                        instanceHome = new GameObject { name = "Movement Effects" };

                        System.Type movementType = System.Type.GetType("MovementEffects.Movement");
                        if(movementType != null)
                            instanceHome.AddComponent(movementType);

                        _instance = instanceHome.AddComponent<Timing>();
                    }
                    else
                    {
                        System.Type movementType = System.Type.GetType("MovementEffects.Movement");
                        if(movementType != null && instanceHome.GetComponent(movementType) == null) 
                            instanceHome.AddComponent(movementType);

                        _instance = instanceHome.GetComponent<Timing>() ?? instanceHome.AddComponent<Timing>();
                    }
                }

                return _instance;
            }

            set { _instance = value; }
        }

        void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

        void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        void Update()
        {
            _runningUpdate = true;

            for (int i = 0; i < _nextUpdateProcessSlot; i++)
            {
                Profiler.BeginSample("Processing Coroutine");

                if (UpdateProcesses[i] != null && !(Time.time < UpdateProcesses[i].Current))
                {
                    try 
                    {
                        if(!UpdateProcesses[i].MoveNext())
                        {
                            UpdateProcesses[i] = null;
                        }
                        else if(float.IsNaN(UpdateProcesses[i].Current))
                        {
                            if (ReplacementFunction == null)
                                UpdateProcesses[i] = null;
                            else
                            {
                                UpdateProcesses[i] = ReplacementFunction(UpdateProcesses[i], Segment.Update);

                                ReplacementFunction = null;
                                i--;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if(OnError == null)
                            Debug.LogError("Exception while running coroutine. (Aborting coroutine)\n" + ex.Message + "\n" + ex.StackTrace);
                        else
                            OnError(ex, UpdateProcesses[i]);

                        UpdateProcesses[i] = null;
                    }
                }

                Profiler.EndSample();
            }

            _runningUpdate = false;


            if(_lastSlowUpdateCallTime + TimeBetweenSlowUpdateCalls < Time.realtimeSinceStartup)
            {
                _runningSlowUpdate = true;
                _lastSlowUpdateCallTime = Time.realtimeSinceStartup;

                for(int i = 0;i < _nextSlowUpdateProcessSlot;i++)
                {
                    Profiler.BeginSample("Processing Coroutine (Slow Update)");

                    if(SlowUpdateProcesses[i] != null && !(Time.time < SlowUpdateProcesses[i].Current))
                    {
                        try
                        {
                            if(!SlowUpdateProcesses[i].MoveNext())
                            {
                                SlowUpdateProcesses[i] = null;
                            }
                            else if(float.IsNaN(SlowUpdateProcesses[i].Current))
                            {
                                if(ReplacementFunction == null)
                                    SlowUpdateProcesses[i] = null;
                                else
                                {
                                    SlowUpdateProcesses[i] = ReplacementFunction(SlowUpdateProcesses[i], Segment.SlowUpdate);

                                    ReplacementFunction = null;
                                    i--;
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            if(OnError == null)
                                Debug.LogError("Exception while running coroutine. (Aborting coroutine)\n" + ex.Message + "\n" + ex.StackTrace);
                            else
                                OnError(ex, SlowUpdateProcesses[i]);

                            SlowUpdateProcesses[i] = null;
                        }
                    }

                    Profiler.EndSample();
                }

                _runningSlowUpdate = false;
            }

            if (++_framesSinceUpdate > FramesUntilMaintenance)
            {
                _framesSinceUpdate = 0;

                Profiler.BeginSample("Maintenance Task");

                RemoveUnused();

                Profiler.EndSample();
            }
        }

        void FixedUpdate()
        {
            _runningFixedUpdate = true;

            for (int i = 0; i < _nextFixedUpdateProcessSlot; i++)
            {
                Profiler.BeginSample("Processing Coroutine");

                if (FixedUpdateProcesses[i] != null && !(Time.time < FixedUpdateProcesses[i].Current))
                {
                    try 
                    {
                        if (!FixedUpdateProcesses[i].MoveNext())
                        {
                            FixedUpdateProcesses[i] = null;
                        }
                        else if (float.IsNaN(FixedUpdateProcesses[i].Current))
                        {
                            if (ReplacementFunction == null)
                                FixedUpdateProcesses[i] = null;
                            else
                            {
                                FixedUpdateProcesses[i] = ReplacementFunction(FixedUpdateProcesses[i], Segment.FixedUpdate);

                                ReplacementFunction = null;
                                i--;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (OnError == null)
                            Debug.LogError("Exception while running coroutine. (Aborting coroutine)\n" + ex.Message + "\n" + ex.StackTrace);
                        else
                            OnError(ex, FixedUpdateProcesses[i]);

                        FixedUpdateProcesses[i] = null;
                    }
                }

                Profiler.EndSample();
            }

            _runningFixedUpdate = false;
        }

        void LateUpdate()
        {
            _runningLateUpdate = true;

            for (int i = 0; i < _nextLateUpdateProcessSlot; i++)
            {
                Profiler.BeginSample("Processing Coroutine");

                if (LateUpdateProcesses[i] != null && !(Time.time < LateUpdateProcesses[i].Current))
                {
                    try 
                    {
                        if (!LateUpdateProcesses[i].MoveNext())
                        {
                            LateUpdateProcesses[i] = null;
                        }
                        else if (float.IsNaN(LateUpdateProcesses[i].Current))
                        {
                            if(ReplacementFunction == null)
                                LateUpdateProcesses[i] = null;
                            else
                            {
                                LateUpdateProcesses[i] = ReplacementFunction(LateUpdateProcesses[i], Segment.LateUpdate);

                                ReplacementFunction = null;
                                i--;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (OnError == null)
                            Debug.LogError("Exception while running coroutine. (Aborting coroutine)\n" + ex.Message + "\n" + ex.StackTrace);
                        else
                            OnError(ex, LateUpdateProcesses[i]);

                        LateUpdateProcesses[i] = null;
                    }
                }

                Profiler.EndSample();
            }

            _runningLateUpdate = false;
        }

        /// <summary>
        /// This will kill all coroutines running on the current MEC instance.
        /// </summary>
        public static void KillAllCoroutines()
        {
            if (_instance != null)
                Destroy(_instance);
        }

        /// <summary>
        /// This will pause all coroutines running on the current MEC instance until ResumeAllCoroutines is called.
        /// </summary>
        public static void PauseAllCoroutines()
        {
            if (_instance != null)
                _instance.enabled = false;
        }

        /// <summary>
        /// This resumes all coroutines on the current MEC instance if they are currently paused, otherwise it has
        /// no effect.
        /// </summary>
        public static void ResumeAllCoroutines()
        {
            if (_instance != null)
                _instance.enabled = true;
        }

        private void RemoveUnused()
        {
            int i, j;
            for(i = j = 0;i < _nextUpdateProcessSlot;i++)
            {
                if(UpdateProcesses[i] != null)
                {
                    if(i != j)
                        UpdateProcesses[j] = UpdateProcesses[i];
                    j++;
                }
            }
            for(i = j;i < _nextUpdateProcessSlot;i++)
                UpdateProcesses[i] = null;

            NumberOfUpdateCoroutines = _nextUpdateProcessSlot = j;

            for(i = j = 0;i < _nextFixedUpdateProcessSlot;i++)
            {
                if(FixedUpdateProcesses[i] != null)
                {
                    if(i != j)
                        FixedUpdateProcesses[j] = FixedUpdateProcesses[i];
                    j++;
                }
            }
            for(i = j;i < _nextFixedUpdateProcessSlot;i++)
                FixedUpdateProcesses[i] = null;

            NumberOfFixedUpdateCoroutines = _nextFixedUpdateProcessSlot = j;

            for(i = j = 0;i < _nextLateUpdateProcessSlot;i++)
            {
                if(LateUpdateProcesses[i] != null)
                {
                    if(i != j)
                        LateUpdateProcesses[j] = LateUpdateProcesses[i];
                    j++;
                }
            }
            for(i = j;i < _nextLateUpdateProcessSlot;i++)
                LateUpdateProcesses[i] = null;

            NumberOfLateUpdateCoroutines = _nextLateUpdateProcessSlot = j;

            for (i = j = 0; i < _nextSlowUpdateProcessSlot; i++)
            {
                if (SlowUpdateProcesses[i] != null)
                {
                    if (i != j)
                        SlowUpdateProcesses[j] = SlowUpdateProcesses[i];
                    j++;
                }
            }
            for (i = j; i < _nextSlowUpdateProcessSlot; i++)
                SlowUpdateProcesses[i] = null;

            NumberOfSlowUpdateCoroutines = _nextSlowUpdateProcessSlot = j;
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        public static IEnumerator<float> RunCoroutine(IEnumerator<float> coroutine)
        {
            return Instance.RunCoroutineOnInstance(coroutine, Segment.Update);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="timing">The segment that the coroutine should run in.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static IEnumerator<float> RunCoroutine(IEnumerator<float> coroutine, Segment timing)
        {
            return Instance.RunCoroutineOnInstance(coroutine, timing);
        }

        /// <summary>
        /// Run a new coroutine on the current Timing instance.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="timing">Whether to run it in the Update, FixedUpdate, or LateUpdate loop.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public IEnumerator<float> RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment timing) 
        {
            if(timing == Segment.Update)
            {
                if(_nextUpdateProcessSlot >= UpdateProcesses.Length)
                {
                    IEnumerator<float>[] oldArray = UpdateProcesses;
                    UpdateProcesses = new IEnumerator<float>[UpdateProcesses.Length + ProcessArrayChunkSize];
                    for(int i = 0;i < oldArray.Length;i++)
                        UpdateProcesses[i] = oldArray[i];
                }

                int currentSlot = _nextUpdateProcessSlot;
                _nextUpdateProcessSlot++;

                UpdateProcesses[currentSlot] = coroutine;

                if(!_runningUpdate)
                {
                    try
                    {
                        if(!UpdateProcesses[currentSlot].MoveNext())
                        {
                            UpdateProcesses[currentSlot] = null;
                        }
                        else if(float.IsNaN(UpdateProcesses[currentSlot].Current))
                        {
                            if(ReplacementFunction == null)
                                UpdateProcesses[currentSlot] = null;
                            else
                            {
                                UpdateProcesses[currentSlot] = ReplacementFunction(UpdateProcesses[currentSlot], timing);

                                ReplacementFunction = null;

                                if(UpdateProcesses[currentSlot] != null)
                                    UpdateProcesses[currentSlot].MoveNext();
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (OnError == null)
                            Debug.LogError("Exception while running coroutine. (Aborting coroutine)\n" + ex.Message + "\n" + ex.StackTrace);
                        else
                            OnError(ex, UpdateProcesses[currentSlot]);

                        UpdateProcesses[currentSlot] = null;
                    }
                }

                return coroutine;
            }
            
            if(timing == Segment.FixedUpdate)
            {
                if (_nextFixedUpdateProcessSlot >= FixedUpdateProcesses.Length)
                {
                    IEnumerator<float>[] oldArray = FixedUpdateProcesses;
                    FixedUpdateProcesses = new IEnumerator<float>[FixedUpdateProcesses.Length + ProcessArrayChunkSize];
                    for (int i = 0; i < oldArray.Length; i++)
                        FixedUpdateProcesses[i] = oldArray[i];
                }

                int currentSlot = _nextFixedUpdateProcessSlot;
                _nextFixedUpdateProcessSlot++;

                FixedUpdateProcesses[currentSlot] = coroutine;

                if(!_runningFixedUpdate)
                {
                    try
                    {
                        if(!FixedUpdateProcesses[currentSlot].MoveNext())
                        {
                            FixedUpdateProcesses[currentSlot] = null;
                        }
                        else if(float.IsNaN(FixedUpdateProcesses[currentSlot].Current))
                        {
                            if(ReplacementFunction == null)
                                FixedUpdateProcesses[currentSlot] = null;
                            else
                            {
                                FixedUpdateProcesses[currentSlot] = ReplacementFunction(FixedUpdateProcesses[currentSlot], timing);

                                ReplacementFunction = null;

                                if(FixedUpdateProcesses[currentSlot] != null)
                                    FixedUpdateProcesses[currentSlot].MoveNext();
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (OnError == null)
                            Debug.LogError("Exception while running coroutine. (Aborting coroutine)\n" + ex.Message + "\n" + ex.StackTrace);
                        else
                            OnError(ex, FixedUpdateProcesses[currentSlot]);

                        FixedUpdateProcesses[currentSlot] = null;
                    }
                }

                return coroutine;
            }
            
            if (timing == Segment.LateUpdate)
            {
                if (_nextLateUpdateProcessSlot >= LateUpdateProcesses.Length)
                {
                    IEnumerator<float>[] oldArray = LateUpdateProcesses;
                    LateUpdateProcesses = new IEnumerator<float>[LateUpdateProcesses.Length + ProcessArrayChunkSize];
                    for (int i = 0; i < oldArray.Length; i++)
                        LateUpdateProcesses[i] = oldArray[i];
                }

                int currentSlot = _nextLateUpdateProcessSlot;
                _nextLateUpdateProcessSlot++;

                LateUpdateProcesses[currentSlot] = coroutine;

                if(!_runningLateUpdate)
                {
                    try
                    {
                        if(!LateUpdateProcesses[currentSlot].MoveNext())
                        {
                            LateUpdateProcesses[currentSlot] = null;
                        }
                        else if(float.IsNaN(LateUpdateProcesses[currentSlot].Current))
                        {
                            if(ReplacementFunction == null)
                                LateUpdateProcesses[currentSlot] = null;
                            else
                            {
                                LateUpdateProcesses[currentSlot] = ReplacementFunction(LateUpdateProcesses[currentSlot], timing);

                                ReplacementFunction = null;

                                if(LateUpdateProcesses[currentSlot] != null)
                                    LateUpdateProcesses[currentSlot].MoveNext();
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (OnError == null)
                            Debug.LogError("Exception while running coroutine. (Aborting coroutine)\n" + ex.Message + "\n" + ex.StackTrace);
                        else
                            OnError(ex, LateUpdateProcesses[currentSlot]);

                        LateUpdateProcesses[currentSlot] = null;
                    }
                }

                return coroutine;
            }

            if (timing == Segment.SlowUpdate)
            {
                if (_nextSlowUpdateProcessSlot >= SlowUpdateProcesses.Length)
                {
                    IEnumerator<float>[] oldArray = SlowUpdateProcesses;
                    SlowUpdateProcesses = new IEnumerator<float>[SlowUpdateProcesses.Length + ProcessArrayChunkSize];
                    for (int i = 0; i < oldArray.Length; i++)
                        SlowUpdateProcesses[i] = oldArray[i];
                }

                int currentSlot = _nextSlowUpdateProcessSlot;
                _nextSlowUpdateProcessSlot++;

                SlowUpdateProcesses[currentSlot] = coroutine;

                if (!_runningSlowUpdate)
                {
                    try
                    {
                        if (!SlowUpdateProcesses[currentSlot].MoveNext())
                        {
                            SlowUpdateProcesses[currentSlot] = null;
                        }
                        else if (float.IsNaN(SlowUpdateProcesses[currentSlot].Current))
                        {
                            if (ReplacementFunction == null)
                                SlowUpdateProcesses[currentSlot] = null;
                            else
                            {
                                SlowUpdateProcesses[currentSlot] = ReplacementFunction(SlowUpdateProcesses[currentSlot], timing);

                                ReplacementFunction = null;

                                if (SlowUpdateProcesses[currentSlot] != null)
                                    SlowUpdateProcesses[currentSlot].MoveNext();
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (OnError == null)
                            Debug.LogError("Exception while running coroutine. (Aborting coroutine)\n" + ex.Message + "\n" + ex.StackTrace);
                        else
                            OnError(ex, SlowUpdateProcesses[currentSlot]);

                        SlowUpdateProcesses[currentSlot] = null;
                    }
                }

                return coroutine;
            }

            return null;
        }

        /// <summary>
        /// Stop the first coroutine in the list of the given type. Use coroutineFunction.GetType() to get the type.
        /// </summary>
        /// <returns>Whether the coroutine was found and stopped.</returns>
        public static bool KillCoroutine(System.Type type)
        {
            return _instance != null && _instance.KillCoroutineOnInstance(type);
        }

        /// <summary>
        /// Stop the given coroutine if it exists.
        /// </summary>
        /// <param name="coroutine">The handle to the coroutine that should be stopped.</param>
        /// <returns>Whether the coroutine was found and stopped.</returns>
        public static bool KillCoroutine(IEnumerator<float> coroutine)
        {
            return _instance != null && _instance.KillCoroutineOnInstance(coroutine);
        }

        /// <summary>
        /// Stop the given coroutine if it exists in the current Timing instance.
        /// </summary>
        /// <param name="type">The type of the coroutine to kill.</param>
        /// <returns>Whether the coroutine was found and stopped.</returns>
        public bool KillCoroutineOnInstance(System.Type type)
        {
            string typeString = type.ToString();

            for (int i = 0; i < _nextUpdateProcessSlot; i++)
            {
                if (UpdateProcesses[i] != null && UpdateProcesses[i].GetType().ToString() == typeString)
                {
                    UpdateProcesses[i] = null;
                    return true;
                }
            }

            for (int i = 0; i < _nextFixedUpdateProcessSlot; i++)
            {
                if (FixedUpdateProcesses[i] != null && FixedUpdateProcesses[i].GetType().ToString() == typeString)
                {
                    FixedUpdateProcesses[i] = null;
                    return true;
                }
            }

            for (int i = 0; i < _nextLateUpdateProcessSlot; i++)
            {
                if (LateUpdateProcesses[i] != null && LateUpdateProcesses[i].GetType().ToString() == typeString)
                {
                    LateUpdateProcesses[i] = null;
                    return true;
                }
            }

            for (int i = 0; i < _nextSlowUpdateProcessSlot; i++)
            {
                if (SlowUpdateProcesses[i] != null && SlowUpdateProcesses[i].GetType().ToString() == typeString)
                {
                    SlowUpdateProcesses[i] = null;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Stop the given coroutine if it exists in the current Timing instance.
        /// </summary>
        /// <param name="coroutine">The handle to the coroutine that should be stopped.</param>
        /// <returns>Whether the coroutine was found and stopped.</returns>
        public bool KillCoroutineOnInstance(IEnumerator<float> coroutine)
        {
            for (int i = 0; i < _nextUpdateProcessSlot; i++)
            {
                if (UpdateProcesses[i] == coroutine)
                {
                    UpdateProcesses[i] = null;
                    return true;
                }
            }

            for (int i = 0; i < _nextFixedUpdateProcessSlot; i++)
            {
                if (FixedUpdateProcesses[i] == coroutine)
                {
                    FixedUpdateProcesses[i] = null;
                    return true;
                }
            }

            for (int i = 0; i < _nextLateUpdateProcessSlot; i++)
            {
                if (LateUpdateProcesses[i] == coroutine)
                {
                    LateUpdateProcesses[i] = null;
                    return true;
                }
            }

            for (int i = 0; i < _nextSlowUpdateProcessSlot; i++)
            {
                if (SlowUpdateProcesses[i] == coroutine)
                {
                    SlowUpdateProcesses[i] = null;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Stop the given coroutine if it exists in the current Timing instance.
        /// </summary>
        /// <param name="coroutine">The handle to the coroutine that should be stopped.</param>
        /// <param name="segmentFoundOn">The segment that the coroutine was found on, if an instance was found.</param>
        /// <returns>Whether the coroutine was found and stopped.</returns>
        public bool KillCoroutineOnInstance(IEnumerator<float> coroutine, out Segment segmentFoundOn)
        {
            for (int i = 0; i < _nextUpdateProcessSlot; i++)
            {
                if (UpdateProcesses[i] == coroutine)
                {
                    UpdateProcesses[i] = null;
                    segmentFoundOn = Segment.Update;
                    return true;
                }
            }

            for (int i = 0; i < _nextFixedUpdateProcessSlot; i++)
            {
                if (FixedUpdateProcesses[i] == coroutine)
                {
                    FixedUpdateProcesses[i] = null;
                    segmentFoundOn = Segment.FixedUpdate;
                    return true;
                }
            }

            for (int i = 0; i < _nextLateUpdateProcessSlot; i++)
            {
                if (LateUpdateProcesses[i] == coroutine)
                {
                    LateUpdateProcesses[i] = null;
                    segmentFoundOn = Segment.LateUpdate;
                    return true;
                }
            }

            for (int i = 0; i < _nextSlowUpdateProcessSlot; i++)
            {
                if (SlowUpdateProcesses[i] == coroutine)
                {
                    SlowUpdateProcesses[i] = null;
                    segmentFoundOn = Segment.SlowUpdate;
                    return true;
                }
            }

            segmentFoundOn = (Segment)(-1); // An invalid value.
            return false;
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(otherCoroutine);" to pause the current 
        /// coroutine until otherCoroutine is done.
        /// </summary>
        /// <param name="otherCoroutine">The coroutine to pause for.</param>
        public static float WaitUntilDone(IEnumerator<float> otherCoroutine)
        {
            return WaitUntilDone(otherCoroutine, true, Instance);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(otherCoroutine);" to pause the current 
        /// coroutine until otherCoroutine is done.
        /// </summary>
        /// <param name="otherCoroutine">The coroutine to pause for.</param>
        /// <param name="warnIfNotFound">Post a warning to the console if no hold action was actually performed.</param>
        public static float WaitUntilDone(IEnumerator<float> otherCoroutine, bool warnIfNotFound)
        {
            return WaitUntilDone(otherCoroutine, warnIfNotFound, Instance);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(otherCoroutine);" to pause the current 
        /// coroutine until the otherCoroutine is done.
        /// </summary>
        /// <param name="otherCoroutine">The coroutine to pause for.</param>
        /// <param name="warnIfNotFound">Post a warning to the console if no hold action was actually performed.</param>
        /// <param name="instance">The instance that the otherCoroutine is attached to. Only use this if you are using 
        /// multiple instances of the Timing object.</param>
        public static float WaitUntilDone(IEnumerator<float> otherCoroutine, bool warnIfNotFound, Timing instance)
        {
            if(instance == null)
                throw new System.ArgumentNullException();

            for(int i = 0;i < instance._waitingProcesses.Count;i++)
            {
                if(instance._waitingProcesses[i].Trigger == otherCoroutine)
                {
                    WaitingProcess proc = instance._waitingProcesses[i];
                    ReplacementFunction = (input, timing) =>
                    {
                        proc.Tasks.Add(input);
                        return null;
                    };

                    return float.NaN;
                }

                for(int j = 0;j < instance._waitingProcesses[i].Tasks.Count;j++)
                {
                    if(instance._waitingProcesses[i].Tasks[j] == otherCoroutine)
                    {
                        WaitingProcess proc = new WaitingProcess();
                        proc.Instance = instance;
                        proc.Timing = instance._waitingProcesses[i].Timing;
                        proc.Trigger = otherCoroutine;
                        proc.Process = _StartWhenDone(proc);

                        instance._waitingProcesses[i].Tasks[j] = proc.Process;

                        proc.Process.MoveNext();

                        ReplacementFunction = (input, timing) =>
                        {
                            proc.Timing = timing;
                            proc.Tasks.Add(input);

                            return null;
                        };

                        return float.NaN;
                    }
                }
            }

            Segment otherCoroutineSegment;

            if(instance.KillCoroutineOnInstance(otherCoroutine, out otherCoroutineSegment))
            {
                ReplacementFunction = (input, timing) =>
                {
                    WaitingProcess proc = new WaitingProcess();
                    proc.Instance = instance;
                    proc.Timing = timing;
                    proc.Trigger = otherCoroutine;
                    proc.Process = _StartWhenDone(proc);
                    proc.Tasks.Add(input);

                    if (timing != otherCoroutineSegment)
                    {
                        instance.RunCoroutineOnInstance(proc.Process, otherCoroutineSegment);
                        return null;
                    }

                    return proc.Process;
                };

                return float.NaN;
            }

            if(warnIfNotFound)
                Debug.LogWarning("WaitUntilDone cannot hold: The coroutine instance that was passed in was not found.");

            return 0f;
        }

        private static IEnumerator<float> _StartWhenDone(WaitingProcess processData)
        {
            processData.Instance._waitingProcesses.Add(processData);

            yield return processData.Trigger.Current;

            while(processData.Trigger.MoveNext())
            {
                yield return processData.Trigger.Current;
            }

            processData.Instance._waitingProcesses.Remove(processData);

            for(int i = 0;i < processData.Tasks.Count;i++)
            {
                processData.Instance.RunCoroutineOnInstance(processData.Tasks[i], processData.Timing);
            }
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(wwwObject);" to pause the current 
        /// coroutine until the wwwObject is done.
        /// </summary>
        /// <param name="wwwObject">The www object to puase for.</param>
        public static float WaitUntilDone(WWW wwwObject)
        {
            ReplacementFunction = (input, timing) => _StartWhenDone(wwwObject, input);
            return float.NaN;
        }

        private static IEnumerator<float> _StartWhenDone(WWW www, IEnumerator<float> pausedProc)
        {
            while (!www.isDone)
                yield return 0f;

            ReplacementFunction = (input, timing) => pausedProc;
            yield return float.NaN;
        }

        /// <summary>
        /// Use in a yield return statement to wait for the specified number of seconds.
        /// </summary>
        /// <param name="waitTime">Number of seconds to wait.</param>
        public static float WaitForSeconds(float waitTime)
        {
            if(float.IsNaN(waitTime)) waitTime = 0f;
            return Time.time + waitTime;
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action.</param>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        public static void CallDelayed<TRef>(TRef reference, float delay, System.Action<TRef> action)
        {
            if(action == null) return;

            if (delay > 0f)
                RunCoroutine(_CallDelayBack(reference, delay, action));
            else
                action(reference);
        }

        private static IEnumerator<float> _CallDelayBack<TRef>(TRef reference, float delay, System.Action<TRef> action)
        {
            yield return Time.time + delay;

            CallDelayed(reference, 0f, action);
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        public static void CallDelayed(float delay, System.Action action)
        {
            if(action == null) return;

            if (delay > 0f)
                RunCoroutine(_CallDelayBack(delay, action));
            else
                action();
        }

        private static IEnumerator<float> _CallDelayBack(float delay, System.Action action)
        {
            yield return Time.time + delay;

            CallDelayed(0f, action);
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        public static void CallPeriodically(float timeframe, float period, System.Action action, System.Action onDone = null)
        {
            if (action != null)
                RunCoroutine(_CallContinuously(timeframe, period, action, onDone), Segment.Update);
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        public static void CallPeriodically(float timeframe, float period, System.Action action, Segment timing, System.Action onDone = null)
        {
            if (action != null)
                RunCoroutine(_CallContinuously(timeframe, period, action, onDone), timing);
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        public static void CallContinuously(float timeframe, System.Action action, System.Action onDone = null)
        {
            if (action != null)
                RunCoroutine(_CallContinuously(timeframe, 0f, action, onDone), Segment.Update);
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        public static void CallContinuously(float timeframe, System.Action action, Segment timing, System.Action onDone = null)
        {
            if (action != null)
                RunCoroutine(_CallContinuously(timeframe, 0f, action, onDone), timing);
        }

        private static IEnumerator<float> _CallContinuously(float timeframe, float period, System.Action action, System.Action onDone)
        {
            float startTime = Time.time;
            while (Time.time <= startTime + timeframe)
            {
                yield return period;

                action();
            }

            if (onDone != null)
                onDone();
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        public static void CallContinuously<T>(T reference, float timeframe, System.Action<T> action, System.Action<T> onDone = null)
        {
            RunCoroutine(_CallContinuously(reference, timeframe, action, onDone), Segment.Update);
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        public static void CallContinuously<T>(T reference, float timeframe, System.Action<T> action, 
            Segment timing, System.Action<T> onDone = null)
        {
            RunCoroutine(_CallContinuously(reference, timeframe, action, onDone), timing);
        }

        private static IEnumerator<float> _CallContinuously<T>(T reference, float timeframe,
            System.Action<T> action, System.Action <T> onDone = null)
        {
            float startTime = Time.time;
            while (Time.time <= startTime + timeframe)
            {
                yield return 0f;

                action(reference);
            }

            if (onDone != null)
                onDone(reference);
        }

        [System.Obsolete("Unity coroutine function, use RunCoroutine instead.")]
        public new Coroutine StartCoroutine(IEnumerator routine)
        {
            return base.StartCoroutine(routine);
        }

        [System.Obsolete("Unity coroutine function, use RunCoroutine instead.")]
        public new Coroutine StartCoroutine(string methodName, object value)
        {
            return base.StartCoroutine(methodName, value);
        }

        [System.Obsolete("Unity coroutine function, use RunCoroutine instead.")]
        public new Coroutine StartCoroutine(string methodName)
        {
            return base.StartCoroutine(methodName);
        }

        [System.Obsolete("Unity coroutine function, use RunCoroutine instead.")]
        public new Coroutine StartCoroutine_Auto(IEnumerator routine)
        {
            return base.StartCoroutine_Auto(routine);
        }

        [System.Obsolete("Unity coroutine function, use KillCoroutine instead.")]
        public new void StopCoroutine(string methodName)
        {
            base.StopCoroutine(methodName);
        }

        [System.Obsolete("Unity coroutine function, use KillCoroutine instead.")]
        public new void StopCoroutine(IEnumerator routine)
        {
            base.StopCoroutine(routine);
        }

        [System.Obsolete("Unity coroutine function, use KillCoroutine instead.")]
        public new void StopCoroutine(Coroutine routine)
        {
            base.StopCoroutine(routine); 
        }

        [System.Obsolete("Unity coroutine function, use KillAllCoroutines instead.")]
        public new void StopAllCoroutines()
        {
            base.StopAllCoroutines();
        }
    }

    public enum Segment
    {
        Update,
        FixedUpdate,
        LateUpdate,
        SlowUpdate
    }
}
