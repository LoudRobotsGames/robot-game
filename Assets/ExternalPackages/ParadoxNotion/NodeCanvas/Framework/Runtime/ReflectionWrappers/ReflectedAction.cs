using System;
using ParadoxNotion;


namespace NodeCanvas.Framework.Internal {

    /// <summary>
    /// Wraps a reflected method call of return type void
    /// </summary>
    [Serializable]
    public class ReflectedAction : ReflectedActionWrapper
    {
        private Action call;
        public override BBParameter[] GetVariables() { return new BBParameter[0]; }
        public override void Init(object instance){
            call = GetMethod().RTCreateDelegate<Action>(instance);
        }
        public override void Call() { call(); }
    }

    [Serializable] [ParadoxNotion.Design.SpoofAOT]
    public class ReflectedAction<T1> : ReflectedActionWrapper
    {
        private Action<T1> call;
        public BBParameter<T1> p1 = new BBParameter<T1>();
        public override BBParameter[] GetVariables(){
            return new BBParameter[] { p1 };
        }
        public override void Init(object instance){
            call = GetMethod().RTCreateDelegate<Action<T1>>(instance);
        }
        public override void Call() { call(p1.value); }
    }

    [Serializable]
    public class ReflectedAction<T1, T2> : ReflectedActionWrapper
    {
        private Action<T1, T2> call;
        public BBParameter<T1> p1 = new BBParameter<T1>();
        public BBParameter<T2> p2 = new BBParameter<T2>();
        public override BBParameter[] GetVariables(){
            return new BBParameter[] { p1, p2 };
        }
        public override void Init(object instance){
            call = GetMethod().RTCreateDelegate<Action<T1, T2>>(instance);
        }
        public override void Call() { call(p1.value, p2.value); }
    }

    [Serializable]
    public class ReflectedAction<T1, T2, T3> : ReflectedActionWrapper
    {
        private Action<T1, T2, T3> call;
        public BBParameter<T1> p1 = new BBParameter<T1>();
        public BBParameter<T2> p2 = new BBParameter<T2>();
        public BBParameter<T3> p3 = new BBParameter<T3>();
        public override BBParameter[] GetVariables(){
            return new BBParameter[] { p1, p2, p3 };
        }
        public override void Init(object instance){
            call = GetMethod().RTCreateDelegate<Action<T1, T2, T3>>(instance);
        }
        public override void Call() { call(p1.value, p2.value, p3.value); }
    }
}

