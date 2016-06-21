using UnityEngine;
using System.Collections;
using CodeControl;

namespace CodeControl.Example
{
    public class TurretController : Controller<TurretModel>
    {

        private const float SHOOT_INTERVAL = 1.0f;

        private Hoverable hoverable;
        private TurretView view;

        protected override void OnInitialize()
        {
            transform.position = model.Position;
            UpdateView(true);
        }

        protected override void OnModelChanged()
        {
            UpdateView(false);
        }

        private void Awake()
        {
            hoverable = GetComponent<Hoverable>();
            view = GetComponent<TurretView>();

            hoverable.OnMouseOver += OnHoverableMouseOver;
            hoverable.OnMouseExit += OnHoverableMouseExit;
        }

        private void Update()
        {
            HandleClick();
            UpdateShooting();
        }

        private void HandleClick()
        {
            if (Input.GetMouseButtonDown(0) && hoverable.IsMouseOver)
            {
                TurretMessage message = new TurretMessage();
                message.TurretModel = model;
                Message.Send<TurretMessage>("click", message);
            }
        }

        private void OnHoverableMouseOver()
        {
            TurretMessage message = new TurretMessage();
            message.TurretModel = model;
            Message.Send<TurretMessage>("mouse_over", message);
        }

        private void OnHoverableMouseExit()
        {
            TurretMessage message = new TurretMessage();
            message.TurretModel = model;
            Message.Send<TurretMessage>("mouse_exit", message);
        }

        private void UpdateShooting()
        {
            model.TimeSinceLastShot += Time.deltaTime;
            if (model.TargetTurret.Model != null)
            {
                if (model.TimeSinceLastShot > SHOOT_INTERVAL)
                {
                    ShootRocket(model.TargetTurret.Model);
                }
            }
        }

        private void ShootRocket(TurretModel target)
        {
            model.TimeSinceLastShot = 0.0f;
            RocketModel rocketModel = new RocketModel();
            rocketModel.Age = 0.0f;
            rocketModel.Color = model.Color;
            rocketModel.Altitude = view.AimAlititude;
            rocketModel.StartPosition = transform.position;
            rocketModel.TargetTurret = new ModelRef<TurretModel>(target);

            Controller.Instantiate<RocketController>("Rocket", rocketModel);
            Model.First<GameModel>().Rockets.Add(rocketModel);

            view.Shoot();
        }

        private void UpdateView(bool instant)
        {
            if (model.TargetTurret.Model != null)
            {
                view.LookAt(model.TargetTurret.Model.Position, instant);
            }
            else {
                view.TurnOff(instant);
            }
            view.SetColor(model.Color);
        }

    }
}