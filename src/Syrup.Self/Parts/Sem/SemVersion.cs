using System;

namespace Syrup.Self.Parts.Sem
{
    public class SemVersion : IComparable<SemVersion>
    {
        protected bool Equals(SemVersion other)
        {
            return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SemVersion) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Major;
                hashCode = (hashCode*397) ^ Minor;
                hashCode = (hashCode*397) ^ Patch;
                return hashCode;
            }
        }

        public SemVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }


        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }

        public int CompareTo(SemVersion other)
        {
            if (other == null) return 1;
            if (Major == other.Major)
                if (Minor == other.Minor)
                    if (Patch == other.Patch) return 0;
                    else if (Patch < other.Patch) return -1;
                    else return 1;
                else if (Minor < other.Minor) return -1;
                else return 1;
            if (Major < other.Major) return -1;
            return 1;
        }

        public static bool operator <(SemVersion v1, SemVersion v2)
        {
            return v1.CompareTo(v2) < 0;
        }

        public static bool operator >(SemVersion v1, SemVersion v2)
        {
            return v1.CompareTo(v2) > 0;
        }

        public static bool operator ==(SemVersion v1, SemVersion v2)
        {
            if (ReferenceEquals(v1, null) && ReferenceEquals(v2, null)) return true;
            if (ReferenceEquals(v1, null) ) return false;
            if (ReferenceEquals(v2, null)) return false;

            return v1.CompareTo(v2) == 0;
        }

        public static bool operator !=(SemVersion v1, SemVersion v2)
        {
            return !(v1 == v2);
        }

        public static SemVersion Parse(string txt)
        {
            var a = txt.Split('.');
            if (a.Length != 3) throw new ArgumentException($"Invalid sem version string: '{txt}'");
            var major = int.Parse(a[0]);
            var minor = int.Parse(a[1]);
            var patch = int.Parse(a[2]);
            return new SemVersion(major, minor, patch);
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }
    }
}