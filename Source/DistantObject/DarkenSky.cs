/*
		This file is part of Distant Object Enhancement /L
			© 2021-2022 LisiasT
			© 2019-2021 TheDarkBadger
			© 2014-2019 MOARdV
			© 2014 Rubber Ducky

		Distant Object Enhancement /L is double licensed, as follows:

		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
		* GPL 2.0 : https://www.gnu.org/licenses/gpl-2.0.txt

		And you are allowed to choose the License that better suit your needs.

		Distant Object Enhancement /L is distributed in the hope that it will
		be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
		of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

		You should have received a copy of the SKL Standard License 1.0
		along with Distant Object Enhancement /L.
		If not, see <https://ksp.lisias.net/SKL-1_0.txt>.

		You should have received a copy of the GNU General Public License 2.0
		along with Distant Object Enhancement /L.
		If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using KSPe.Annotations;
using UnityEngine;

namespace DistantObject
{
    //Peachoftree: It was EveryScene so the sky would darken in places like the starting menu and the tracking center, not just flight and map veiw 
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class DarkenSky : MonoBehaviour
    {
		private static DarkenSky INSTANCE = null;
		internal static DarkenSky Instance => INSTANCE;

        private Color galaxyColor = Color.black;
        private float glareFadeLimit = 0.0f;
        private bool restorableGalaxyCube = false;

		[UsedImplicitly]
        private void Awake()
        {
            INSTANCE = this;

            restorableGalaxyCube = false;

            DistantObjectSettings.Instance.LoadConfig();

            if (GalaxyCubeControl.Instance != null)
            {
                restorableGalaxyCube = true;
                galaxyColor = GalaxyCubeControl.Instance.maxGalaxyColor;
                glareFadeLimit = GalaxyCubeControl.Instance.glareFadeLimit;

                if (DistantObjectSettings.Instance.SkyboxBrightness.changeSkybox)
                {
                    GalaxyCubeControl.Instance.maxGalaxyColor = new Color(DistantObjectSettings.Instance.SkyboxBrightness.maxBrightness, DistantObjectSettings.Instance.SkyboxBrightness.maxBrightness, DistantObjectSettings.Instance.SkyboxBrightness.maxBrightness);
                    GalaxyCubeControl.Instance.glareFadeLimit = 1f;
                }
            }
        }

		[UsedImplicitly]
		private void Start()
		{
			DistantObjectSettings.Instance.Commit();
		}

		[UsedImplicitly]
		private void OnDestroy()
        {
            if (GalaxyCubeControl.Instance != null && restorableGalaxyCube)
            {
                GalaxyCubeControl.Instance.maxGalaxyColor = galaxyColor;
                GalaxyCubeControl.Instance.glareFadeLimit = glareFadeLimit;
                restorableGalaxyCube = false;
            }

            INSTANCE = null;
        }

		[UsedImplicitly]
        private void Update()
        {
            if (null == GalaxyCubeControl.Instance) return;
            if (MapView.MapIsEnabled)
            {
				GalaxyCubeControl.Instance.maxGalaxyColor = this.galaxyColor;
				GalaxyCubeControl.Instance.glareFadeLimit = this.glareFadeLimit;
                return;
            }

            Color color = new Color(DistantObjectSettings.Instance.SkyboxBrightness.maxBrightness, DistantObjectSettings.Instance.SkyboxBrightness.maxBrightness, DistantObjectSettings.Instance.SkyboxBrightness.maxBrightness);
            Vector3d camPos = FlightCamera.fetch.mainCamera.transform.position;
            float camFov = FlightCamera.fetch.mainCamera.fieldOfView;
            Vector3d camAngle = FlightCamera.fetch.mainCamera.transform.forward;

            for (int i = 0; i < FlightGlobals.Bodies.Count; ++i)
            {
                double bodyRadius = FlightGlobals.Bodies[i].Radius;
                double bodyDist = FlightGlobals.Bodies[i].GetAltitude(camPos) + bodyRadius;
                float bodySize = Mathf.Acos((float)(Math.Sqrt(bodyDist * bodyDist - bodyRadius * bodyRadius) / bodyDist)) * Mathf.Rad2Deg;

                if (bodySize > 1.0f)
                {
                    Vector3d bodyPosition = FlightGlobals.Bodies[i].position;
                    Vector3d targetVectorToSun = FlightGlobals.Bodies[0].position - bodyPosition;
                    Vector3d targetVectorToCam = camPos - bodyPosition;

                    float targetRelAngle = (float)Vector3d.Angle(targetVectorToSun, targetVectorToCam);
                    targetRelAngle = Mathf.Max(targetRelAngle, bodySize);
                    targetRelAngle = Mathf.Min(targetRelAngle, 100.0f);
                    targetRelAngle = 1.0f - ((targetRelAngle - bodySize) / (100.0f - bodySize));

                    float CBAngle = Mathf.Max(0.0f, Vector3.Angle((bodyPosition - camPos).normalized, camAngle) - bodySize);
                    CBAngle = 1.0f - Mathf.Min(1.0f, Math.Max(0.0f, (CBAngle - (camFov / 2.0f)) - 5.0f) / (camFov / 4.0f));
                    bodySize = Mathf.Min(bodySize, 60.0f);

                    float colorScalar = 1.0f - (targetRelAngle * (Mathf.Sqrt(bodySize / 60.0f)) * CBAngle);
                    color.r *= colorScalar;
                    color.g *= colorScalar;
                    color.b *= colorScalar;
                }
            }

            GalaxyCubeControl.Instance.maxGalaxyColor = color;
        }

		internal void SetActiveTo(bool renderVessels)
		{
			if (renderVessels)
				this.Activate();
			else
				this.Deactivate();
		}

		private void Activate()
		{
			Log.trace("DarkenSky enabled");
			this.enabled = true;
		}

		private void Deactivate()
		{
			Log.trace("DarkenSky disabled");
			this.enabled = false;

			if (this.restorableGalaxyCube && null != GalaxyCubeControl.Instance)
			{
				GalaxyCubeControl.Instance.maxGalaxyColor = this.galaxyColor;
				GalaxyCubeControl.Instance.glareFadeLimit = this.glareFadeLimit;
			}
		}
	}
}
