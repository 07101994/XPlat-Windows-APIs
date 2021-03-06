namespace XPlat.Device.Power
{
    using System;

    using Android.Content;
    using Android.OS;

    public sealed class PowerReceiver : BroadcastReceiver
    {
        private readonly object obj = new object();
        
        private BatteryStatus previousStatus = BatteryStatus.NotPresent;

        private int previousRemainingCharge = 0;

        public event BatteryStatusChangedEventHandler Updated;

        /// <inheritdoc />
        public override void OnReceive(Context context, Intent intent)
        {
            if (this.Updated == null)
            {
                return;
            }

            var batteryLevel = intent.GetIntExtra(BatteryManager.ExtraLevel, -1);
            var batteryScale = intent.GetIntExtra(BatteryManager.ExtraScale, -1);
            var batteryStatus = intent.GetIntExtra(BatteryManager.ExtraStatus, -1);

            var status = BatteryStatus.NotPresent;

            switch (batteryStatus)
            {
                case (int)Android.OS.BatteryStatus.Charging:
                    status = BatteryStatus.Charging;
                    break;
                case (int)Android.OS.BatteryStatus.Discharging:
                    status = BatteryStatus.Discharging;
                    break;
                case (int)Android.OS.BatteryStatus.Full:
                case (int)Android.OS.BatteryStatus.NotCharging:
                    status = BatteryStatus.Idle;
                    break;
                case (int)Android.OS.BatteryStatus.Unknown:
                    status = BatteryStatus.NotPresent;
                    break;
            }

            var remainingChargePercent = (int)Math.Floor(batteryLevel * 100D / batteryScale);

            if (status != this.previousStatus || remainingChargePercent != this.previousRemainingCharge)
            {
                lock (this.obj)
                {
                    var eventArgs = new BatteryStatusChangedEventArgs(status, remainingChargePercent);

                    try
                    {
                        this.Updated?.Invoke(this, eventArgs);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
#endif
                    }
                    finally
                    {
                        this.previousStatus = status;
                        this.previousRemainingCharge = remainingChargePercent;
                    }
                }
            }
        }
    }
}