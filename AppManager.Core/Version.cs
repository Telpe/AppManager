namespace AppManager.Core
{
    public struct Version
    {
        public int Exspansion { get; set; }
        public int Patch { get; set; }
        public int Hotfix { get; set; }
        public int Work { get; set; }

        public override string ToString()
        {
            return $"{Exspansion}.{Patch}.{Hotfix}.{Work}";
        }

        public static Version Parse(string version)
        {
            // Find the first integer and keep the rest of the string from that index
            int firstDigitIndex = -1;
            for (int i = 0; i < version.Length; i++)
            {
                if (char.IsDigit(version[i]))
                {
                    firstDigitIndex = i;
                    break;
                }
            }

            if (firstDigitIndex != -1)
            {
                version = version[firstDigitIndex..];
            }

            string[] parts = version.Split('.');
            Version v = new()
            {
                Exspansion = parts.Length > 0 ? int.Parse(parts[0]) : 0,
                Patch = parts.Length > 1 ? int.Parse(parts[1]) : 0,
                Hotfix = parts.Length > 2 ? int.Parse(parts[2]) : 0,
                Work = parts.Length > 3 ? int.Parse(parts[3]) : 0
            };

            return v;
        }

        public static bool operator >(Version v1, Version v2)
        {
            if (v1.Exspansion != v2.Exspansion) { return v1.Exspansion > v2.Exspansion; }
            if (v1.Patch != v2.Patch){ return v1.Patch > v2.Patch; }
            if (v1.Hotfix != v2.Hotfix) { return v1.Hotfix > v2.Hotfix; }
            return v1.Work > v2.Work;
        }
        public static bool operator <(Version v1, Version v2)
        {
            if (v1.Exspansion != v2.Exspansion) { return v1.Exspansion < v2.Exspansion; }
            if (v1.Patch != v2.Patch) { return v1.Patch < v2.Patch; }
            if (v1.Hotfix != v2.Hotfix) { return v1.Hotfix < v2.Hotfix; }
            return v1.Work < v2.Work;
        }
        public static bool operator >=(Version v1, Version v2)
        {
            return !(v1 < v2);
        }
        public static bool operator <=(Version v1, Version v2)
        {
            return !(v1 > v2);

        }
        public static bool operator ==(Version v1, Version v2)
        {
            return v1.Exspansion == v2.Exspansion &&
                   v1.Patch == v2.Patch &&
                   v1.Hotfix == v2.Hotfix &&
                   v1.Work == v2.Work;
        }
        public static bool operator !=(Version v1, Version v2)
        {
            return !(v1 == v2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Version v)
            {
                return this == v;
            }
            return false;
        }

        public static implicit operator Version(System.Version systemVersion)
        {
            if (systemVersion == null)
                return new Version();

            return new Version
            {
                Exspansion = systemVersion.Major,
                Patch = systemVersion.Minor,
                Hotfix = systemVersion.Build >= 0 ? systemVersion.Build : 0,
                Work = systemVersion.Revision >= 0 ? systemVersion.Revision : 0
            };
        }

        public override int GetHashCode()
        {
            return (Exspansion, Patch, Hotfix, Work).GetHashCode();
        }
    }
}
