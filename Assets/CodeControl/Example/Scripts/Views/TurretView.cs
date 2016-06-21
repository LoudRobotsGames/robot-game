using UnityEngine;
using System.Collections;

namespace CodeControl.Example
{
    public class TurretView : MonoBehaviour
    {

        public float AimAlititude
        {
            get
            {
                return gunJoint.position.y;
            }
        }

        private const float ROTATE_DURATION = .2f;
        private const float AIM_DURATION = .4f;
        private const float SHOOT_DURATION = 0.8f;
        private const float AIM_ACTIVE_ANGLE = -25.0f;
        private const float AIM_INACTIVE_ANGLE = 25.0f;

        [SerializeField]
        private Transform rotatorJoint;
        [SerializeField]
        private Transform neckJoint;
        [SerializeField]
        private Transform gunJoint;
        [SerializeField]
        private AnimationCurve shootCurve;

        public void SetColor(Color color)
        {
            Colorer[] colorers = GetComponentsInChildren<Colorer>();
            foreach (Colorer colorer in colorers)
            {
                colorer.SetColor(color);
            }
        }

        public void LookAt(Vector3 position, bool instant)
        {
            StopCoroutine("LookAtOverTime");
            StopCoroutine("AimOverTime");

            if (instant)
            {
                rotatorJoint.rotation = Quaternion.LookRotation(position - transform.position, Vector3.up);
                neckJoint.localRotation = Quaternion.Euler(AIM_ACTIVE_ANGLE, 0.0f, 0.0f);
                return;
            }

            StartCoroutine("LookAtOverTime", position);
            StartCoroutine("AimOverTime", AIM_ACTIVE_ANGLE);
        }

        public void TurnOff(bool instant)
        {
            StopCoroutine("AimOverTime");

            if (instant)
            {
                neckJoint.localRotation = Quaternion.Euler(AIM_INACTIVE_ANGLE, 0.0f, 0.0f);
                return;
            }

            StartCoroutine("AimOverTime", AIM_INACTIVE_ANGLE);
        }

        public void Shoot()
        {
            StopCoroutine("PlayShootAnimation");
            StartCoroutine("PlayShootAnimation");
        }

        private IEnumerator LookAtOverTime(Vector3 position)
        {
            Vector3 direction = position - transform.position;

            Quaternion startRotation = rotatorJoint.rotation;
            Quaternion endRotation = Quaternion.LookRotation(direction, Vector3.up);

            if (startRotation == endRotation)
            {
                yield break;
            }

            float startTime = Time.time;

            while (Time.time - startTime < ROTATE_DURATION)
            {
                float easedProgress = MathHelper.EaseInOutSin((Time.time - startTime) / ROTATE_DURATION);
                rotatorJoint.rotation = Quaternion.Slerp(startRotation, endRotation, easedProgress);
                yield return new WaitForEndOfFrame();
            }

            rotatorJoint.rotation = endRotation;
        }

        private IEnumerator AimOverTime(float targetAngle)
        {
            Quaternion startRotation = neckJoint.localRotation;
            Quaternion endRotation = Quaternion.Euler(targetAngle, 0.0f, 0.0f);

            if (startRotation == endRotation)
            {
                yield break;
            }

            float startTime = Time.time;

            while (Time.time - startTime < AIM_DURATION)
            {
                float progress = (Time.time - startTime) / AIM_DURATION;
                neckJoint.localRotation = Quaternion.Slerp(startRotation, endRotation, progress);
                yield return new WaitForEndOfFrame();
            }

            neckJoint.localRotation = endRotation;
        }

        private IEnumerator PlayShootAnimation()
        {
            float startTime = Time.time;
            while (Time.time - startTime < SHOOT_DURATION)
            {
                gunJoint.localPosition = Vector3.forward * shootCurve.Evaluate((Time.time - startTime) / SHOOT_DURATION);
                yield return new WaitForEndOfFrame();
            }
            gunJoint.localPosition = Vector3.zero;
        }

    }
}