namespace Konbini.License
{
    using System;
    using System.Runtime.CompilerServices;

    public class LicenseInfo
    {
        public string FullName { get; set; }

        public string ProductKey { get; set; }

        public int Day { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public DateTime CheckDate =>
            new DateTime(this.Year, this.Month, this.Day).Date;

        public string Data =>
            $"{this.FullName}#{this.ProductKey}#{this.Day}#{this.Month}#{this.Year}";
    }
}

