using UnityEngine;
using System.Collections;
using CodeControl;

namespace CodeControl.Example
{
    public class LevelController : Controller<LevelModel>
    {

        protected override void OnInitialize()
        {
            GameObject hud = GameObject.Instantiate(Resources.Load("LevelHud")) as GameObject;
            hud.transform.SetParent(transform, false);

            foreach (TurretModel turretModel in model.Turrets)
            {
                Controller.Instantiate<TurretController>("Turret", turretModel, transform);
            }

            Message.AddListener<PlaceTurretMessage>(OnPlaceTurret);
        }

        protected override void OnDestroy()
        {
            Message.RemoveListener<PlaceTurretMessage>(OnPlaceTurret);
            base.OnDestroy();
        }

        private void OnPlaceTurret(PlaceTurretMessage message)
        {
            TurretModel turretModel = new TurretModel();
            turretModel.Color = TurretColors.GetRandom();
            turretModel.Position = message.Position;
            turretModel.TargetTurret = new ModelRef<TurretModel>(null);

            Controller.Instantiate<TurretController>("Turret", turretModel, transform);
            model.Turrets.Add(turretModel);
        }

    }
}