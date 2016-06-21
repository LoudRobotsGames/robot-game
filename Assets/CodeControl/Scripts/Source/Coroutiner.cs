/// <copyright file="Coroutiner.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;

namespace CodeControl.Internal {

    public class Coroutiner : MonoBehaviour {

        private static Coroutiner instance;

        public static void Start(IEnumerator routine) {
            GetInstance().StartLocalCoroutine(routine);
        }

        public void StartLocalCoroutine(IEnumerator routine) {
            StartCoroutine(routine);
        }

        private static Coroutiner GetInstance() {
            if (instance == null) {
                GameObject go = new GameObject("Coroutiner");
                instance = go.AddComponent<Coroutiner>();
            }
            return instance;
        }

    }

}