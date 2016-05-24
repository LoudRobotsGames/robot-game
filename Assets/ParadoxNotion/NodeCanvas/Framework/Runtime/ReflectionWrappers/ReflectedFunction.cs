using System;
using ParadoxNotion;

namespace NodeCanvas.Framework.Internal {

    /// <summary>
    /// A Wrapped reflected method call of return type TResult
    /// </summary>
    [Serializable] [ParadoxNotion.Design.SpoofAOT]
    public class ReflectedFunction<TResult> : ReflectedFunctionWrapper
    {
        private Func<TResult> call;
        [BlackboardOnly]
        public BBParameter<TResult> result = new BBParameter<TResult>();
        public override BBParameter[] GetVariables() { return new BBParameter[] { result }; }
        public override void Init(object instance){
            call = GetMethod().RTCreateDelegate<Func<TResult>>(instance);
        }
        public override object Call() { return result.value = call(); }
    }

    [Serializable]
    public class ReflectedFunction<TResult, T1> : ReflectedFunctionWrapper
    {
        private Func<T1, TResult> call;
        public BBParameter<T1> p1 = new BBParameter<T1>();
        [BlackboardOnly]
        public BBParameter<TResult> result = new BBParameter<TResult>();
        public override BBParameter[] GetVariables(){
            return new BBParameter[] { result, p1 };
        }
        public override void Init(object instance) {
            call = GetMethod().RTCreateDelegate<Func<T1, TResult>>(instance);
        }
        public override object Call() { return result.value = call(p1.value); }
    }

    [Serializable]
    public class ReflectedFunction<TResult, T1, T2> : ReflectedFunctionWrapper
    {
        private Func<T1, T2, TResult> call;
        public BBParameter<T1> p1 = new BBParameter<T1>();
        public BBParameter<T2> p2 = new BBParameter<T2>();
        [BlackboardOnly]
        public BBParameter<TResult> result = new BBParameter<TResult>();
        public override BBParameter[] GetVariables(){
            return new BBParameter[] { result, p1, p2 };
        }
        public override void Init(object instance){
            call = GetMethod().RTCreateDelegate<Func<T1, T2, TResult>>(instance);
        }
        public override object Call() { return result.value = call(p1.value, p2.value); }
    }

    [Serializable]
    public class ReflectedFunction<TResult, T1, T2, T3> : ReflectedFunctionWrapper
    {
        private Func<T1, T2, T3, TResult> call;
        public BBParameter<T1> p1 = new BBParameter<T1>();
        public BBParameter<T2> p2 = new BBParameter<T2>();
        public BBParameter<T3> p3 = new BBParameter<T3>();
        [BlackboardOnly]
        public BBParameter<TResult> result = new BBParameter<TResult>();
        public override BBParameter[] GetVariables(){
            return new BBParameter[] { result, p1, p2, p3 };
        }
        public override void Init(object instance){
            call = GetMethod().RTCreateDelegate<Func<T1, T2, T3, TResult>>(instance);
        }
        public override object Call() { return result.value = call(p1.value, p2.value, p3.value); }
    }

}