using Cysharp.Threading.Tasks;
using Island.Common.Services;
using Island.Gameplay.Configs.DateTime;
using Island.Gameplay.Profiles;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Island.Gameplay.Services.DateTime
{
    public class SunController : NetworkBehaviour
    {
        [Inject] private DateTimeService _dateTimeService;
        [Inject] private DateTimeProfile _dateTimeProfile;
        [Inject] private DateTimeConfig _dateTimeConfig;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }

            _dateTimeProfile.Minutes.Subscribe(OnDateTimeChanged).AddTo(this);
        }

        private void OnDateTimeChanged(float minutes)
        {
            var dateTime = _dateTimeService.GetDateTime(minutes);
            var sunDirection = CalculateSunDirection(dateTime, _dateTimeConfig.Latitude);

            transform.rotation = Quaternion.LookRotation(sunDirection, Vector3.up);
        }

        private Vector3 CalculateSunDirection(System.DateTime dateTime, float latitudeDeg)
        {
            var hour = dateTime.Hour + dateTime.Minute / 60f + dateTime.Second / 3600f;

            var latRad = latitudeDeg * Mathf.Deg2Rad;

            var declDeg = -23.44f * Mathf.Cos(Mathf.Deg2Rad * (360f / 365f * (dateTime.DayOfYear + 10)));
            var declRad = declDeg * Mathf.Deg2Rad;

            var hourAngleDeg = 15f * (hour - 12f);
            var hourAngleRad = hourAngleDeg * Mathf.Deg2Rad;

            var sinAlt = Mathf.Sin(latRad) * Mathf.Sin(declRad) + Mathf.Cos(latRad) * Mathf.Cos(declRad) * Mathf.Cos(hourAngleRad);
            var altRad = Mathf.Asin(sinAlt);
            var cosAz = (Mathf.Sin(declRad) - Mathf.Sin(altRad) * Mathf.Sin(latRad)) / (Mathf.Cos(altRad) * Mathf.Cos(latRad));

            cosAz = Mathf.Clamp(cosAz, -1f, 1f);
            var azRad = Mathf.Acos(cosAz);

            if (hourAngleRad > 0)
            {
                azRad = 2f * Mathf.PI - azRad;
            }

            var horiz = Mathf.Cos(altRad);
            var x = horiz * Mathf.Sin(azRad);
            var y = Mathf.Sin(altRad);
            var z = horiz * Mathf.Cos(azRad);

            var dir = new Vector3(x, y, z);

            return dir.normalized;
        }
    }
}