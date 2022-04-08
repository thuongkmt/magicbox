﻿namespace Konbini.License
{
    using System;
    using System.Runtime.CompilerServices;

    public class KeyValuesClass
    {
        public byte Header { get; set; }

        public byte Footer { get; set; }

        public byte ProductCode { get; set; }

        public Konbini.License.Edition Edition { get; set; }

        public byte Version { get; set; }

        public LicenseType Type { get; set; }

        public DateTime Expiration { get; set; }
    }
}

