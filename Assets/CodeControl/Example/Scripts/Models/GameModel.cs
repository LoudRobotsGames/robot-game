using UnityEngine;
using System.Collections;
using CodeControl;

namespace CodeControl.Example
{
    public class GameModel : Model
    {

        public ModelRef<LevelModel> Level;
        public ModelRefs<RocketModel> Rockets;

    }
}