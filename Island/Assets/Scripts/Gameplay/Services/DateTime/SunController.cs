using Island.Gameplay.Profiles;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.DateTime
{
    public class SunController : NetworkBehaviour
    {
        [SerializeField, Range(-90f, 90f)] private float _latitude = 50f;
        [Inject] private DateTimeProfile _dateTimeProfile;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }

            _dateTimeProfile.Minutes.Subscribe(t => OnDateTimeChanged(_dateTimeProfile.GetDateTime())).AddTo(this);
        }

        private void OnDateTimeChanged(System.DateTime dateTime)
        {
            Vector3 sunDirection = CalculateSunDirection(dateTime, _latitude);

            transform.rotation = Quaternion.LookRotation(sunDirection, Vector3.up);
        }

        private Vector3 CalculateSunDirection(System.DateTime dateTime, float latitudeDeg)
        {
            float hour = dateTime.Hour + dateTime.Minute / 60f + dateTime.Second / 3600f;

            float latRad = latitudeDeg * Mathf.Deg2Rad;

            float declDeg = -23.44f * Mathf.Cos(Mathf.Deg2Rad * (360f / 365f * (dateTime.DayOfYear + 10)));
            float declRad = declDeg * Mathf.Deg2Rad;

            float hourAngleDeg = 15f * (hour - 12f);
            float hourAngleRad = hourAngleDeg * Mathf.Deg2Rad;

            float sinAlt = Mathf.Sin(latRad) * Mathf.Sin(declRad) + Mathf.Cos(latRad) * Mathf.Cos(declRad) * Mathf.Cos(hourAngleRad);
            float altRad = Mathf.Asin(sinAlt);
            float cosAz = (Mathf.Sin(declRad) - Mathf.Sin(altRad) * Mathf.Sin(latRad)) / (Mathf.Cos(altRad) * Mathf.Cos(latRad));

            cosAz = Mathf.Clamp(cosAz, -1f, 1f);
            float azRad = Mathf.Acos(cosAz);

            if (hourAngleRad > 0)
            {
                azRad = 2f * Mathf.PI - azRad;
            }

            float horiz = Mathf.Cos(altRad);
            float x = horiz * Mathf.Sin(azRad);
            float y = Mathf.Sin(altRad);
            float z = horiz * Mathf.Cos(azRad);

            Vector3 dir = new Vector3(x, y, z);

            return dir.normalized;
        }
    }
}