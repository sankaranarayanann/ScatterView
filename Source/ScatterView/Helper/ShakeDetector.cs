using Android.Hardware;
using Java.Lang;

namespace ScatterView.Helper
{
    /// <summary>
    /// Class implements Sensor events
    /// </summary>
    public class ShakeDetector : Object, ISensorEventListener
    {
        #region Fields
        private const float SHAKE_THRESHOLD_GRAVITY = 2.7F;
        private const int SHAKE_SLOP_TIME_MS = 500;
        private const int SHAKE_COUNT_RESET_TIME_MS = 3000;

        private long mShakeTimestamp;
        private int mShakeCount;

        public delegate void OnshakeHandler(object sender, int shakeCount);
        public event OnshakeHandler Shaked;

        #endregion

        #region ctor
        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {

        }
        #endregion

        #region Methods
        public void OnSensorChanged(SensorEvent e)
        {
            var x = e.Values[0];
            var y = e.Values[1];
            var z = e.Values[2];

            var gX = x / SensorManager.GravityEarth;
            var gY = y / SensorManager.GravityEarth;
            var gZ = z / SensorManager.GravityEarth;


            // gForce will be close to 1 when there is no movement.
            var gForce = Java.Lang.Math.Sqrt(gX * gX + gY * gY + gZ * gZ);

            if (!(gForce > SHAKE_THRESHOLD_GRAVITY))
                return;

            var now = JavaSystem.CurrentTimeMillis();


            // Ignore shake events too close to each other (500ms)
            if (mShakeTimestamp + SHAKE_SLOP_TIME_MS > now)
            {
                return;
            }


            // Reset the shake count after 3 seconds of no shakes
            if (mShakeTimestamp + SHAKE_COUNT_RESET_TIME_MS < now)
            {
                mShakeCount = 0;
            }

            mShakeTimestamp = now;
            mShakeCount++;

            Shaked?.Invoke(this, mShakeCount);
        }

        #endregion
    }
}