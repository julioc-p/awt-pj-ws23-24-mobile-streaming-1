namespace awt-pj-ws23-24-mobile-streaming-1.Model
{
    public class Measurement
    {
        public double TotalPower { get; private set; }
        private short _isComplete = 0; // lowest 3 Bits represent CPU, GPU, ANE Power written => ...00000111 == TotalPower is complete
        public bool IsComplete
        {
            get => _isComplete == 7;
        }
        public bool VideoPlaying { get; set; }
        public DateTime Time { get; set; }

        public Measurement() { }

        public void CollectMeasurementMacOs(string possibleMeasurement)
        {
            var data = possibleMeasurement.Trim();
            if (
                data.Length < 10
                || !(data[data.Length - 1] == 'W')
                || !(data[0] == 'C' || data[0] == 'G' || data[0] == 'A' || data[0] == 'I')
                || !(
                    data.StartsWith("Intel energy model derived package power")
                    || data.StartsWith("CPU Power")
                    || data.StartsWith("GPU Power")
                    || data.StartsWith("ANE Power")
                )
            )
            {
                return;
            }
            if (data[0] == 'I')
            { // Intel
                ParseIntelOutput(data);
            }
            else
            {
                ParseAppleSiliconOutput(data);
            }
        }

        public void CollectMeasurementWindowsIntel(IEnumerable<double> measurements)
        {
            this.TotalPower = double.Round(measurements.Sum(), 2);
#if DEBUG
            System.Console.WriteLine(
                $"{System.DateTime.Now.ToLongTimeString()}: {this.TotalPower}"
            );
#endif
            Time = DateTime.Now;
            this._isComplete = 7;
        }

        private void ParseAppleSiliconOutput(string data)
        {
            // Format is expected to be in the form of "___ Power: xyz mW"
            var splitData = data.Split(':');
            var tag = splitData[0];
            var val = splitData[1].Trim().Split(' '); // contains num + unit
            var num = val[0];
            var unit = val[1];
            if (!double.TryParse(num, out double power))
            {
                return;
            }
            switch (tag[0])
            {
                // On Apple Silicon devices CPU, GPU and ANE power are reported separately and in mW
                case 'C':
                    TotalPower += power / 1000;
                    _isComplete |= 1;
                    return;
                case 'G':
                    TotalPower += power / 1000;
                    _isComplete |= 2;
                    return;
                case 'A':
                    TotalPower += power / 1000;
                    _isComplete |= 4;
                    return;
                default:
                    break;
            }
        }

        private void ParseIntelOutput(string data)
        {
            // Format is expected to be in the form of "Intel energy model ...: __.__W"
            var splitData = data.Split(':');
            var val = splitData.LastOrDefault()?.Trim();
            var num = val?.Substring(0, val.Length - 2);
            if (!double.TryParse(num, out double power))
            {
                return;
            }
            TotalPower = power;
            _isComplete = 7;
        }

        public void ResetMeasurement()
        {
            _isComplete = 0;
            TotalPower = 0;
        }
    }
}
