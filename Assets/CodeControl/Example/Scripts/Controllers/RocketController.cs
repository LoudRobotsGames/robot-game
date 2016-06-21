using UnityEngine;
using System.Collections;
using CodeControl;

namespace CodeControl.Example
{
    public class RocketController : Controller<RocketModel>
    {

        private const float SPEED = 1.5f;
        private const float ANGLE = 25.0f * Mathf.Deg2Rad;
        private const float COLLISION_RADIUS = .5f;

        private float distance;
        private Vector3 translation;
        private Vector3 arcCenter;
        private float arcRadius;
        private float startAngle;

        protected override void OnInitialize()
        {
            UpdateViewColor();

            distance = Vector3.Distance(model.StartPosition, model.TargetTurret.Model.Position);
            translation = model.TargetTurret.Model.Position - model.StartPosition;

            Vector3 center = .5f * (model.StartPosition + model.TargetTurret.Model.Position);
            arcCenter = center - Vector3.up * ((.5f * distance) / Mathf.Tan(ANGLE));
            arcRadius = Vector3.Distance(model.StartPosition, arcCenter);
            startAngle = .5f * (Mathf.PI - ANGLE * 2.0f);

            model.TargetTurret.Model.AddDeleteListener(OnTargetTurretDelete);

            Update();
        }

        protected override void OnModelChanged()
        {
            UpdatePosition();
            HandleCollision();
        }

        protected override void OnDestroy()
        {
            if (model.TargetTurret.Model != null)
            {
                model.TargetTurret.Model.RemoveDeleteListener(OnTargetTurretDelete);
            }
            base.OnDestroy();
        }

        private void Update()
        {
            model.Age += Time.deltaTime;
            model.NotifyChange();
        }

        private void UpdatePosition()
        {
            Vector3 currentPosition = transform.position;
            Vector3 newPosition = CalculateNewPosition();
            transform.rotation = Quaternion.LookRotation(newPosition - currentPosition);
            transform.position = newPosition;
        }

        private Vector3 CalculateNewPosition()
        {
            float progression = (model.Age * SPEED) / distance;
            Vector3 newPosition = Vector3.up * model.Altitude + arcCenter +
                                  Vector3.up * Mathf.Sin(startAngle + progression * ANGLE * 2.0f) * arcRadius -
                                  (arcRadius / (.5f * distance) * translation) * .5f * Mathf.Cos(startAngle + progression * ANGLE * 2.0f);
            return newPosition;
        }

        private void HandleCollision()
        {
            if (!ReachedTarget()) { return; }
            Explode();
        }

        private void UpdateViewColor()
        {
            Colorer[] colorers = GetComponentsInChildren<Colorer>();
            foreach (Colorer colorer in colorers)
            {
                colorer.SetColor(model.Color);
            }
        }

        private void OnTargetTurretDelete()
        {
            Explode();
        }

        private bool ReachedTarget()
        {
            float distance = Vector3.Distance(transform.position, model.TargetTurret.Model.Position);
            return distance < COLLISION_RADIUS;
        }

        private void Explode()
        {
            GameObject.Instantiate(Resources.Load("Explosion"), transform.position, Quaternion.identity);
            model.Delete();
        }

    }
}