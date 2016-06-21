﻿/// <copyright file="ModelReferencer.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CodeControl.Internal {

    public abstract class ModelReferencer {

        public abstract void Delete();
        internal abstract List<Model> GetReferences();
        internal abstract void CollectReferences();

    }

}