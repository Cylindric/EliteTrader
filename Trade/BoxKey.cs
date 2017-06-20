using System.Diagnostics;

namespace Trade
{
    [DebuggerDisplay("{X},{Z}")]
    public struct BoxKey
    {
        public int X { get; set; }
        public int Z { get; set; }

        public BoxKey(int x, int y)
        {
            X = x;
            Z = y;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is BoxKey))
            {
                return false;
            }
            else
            {
                return X == ((BoxKey)obj).X && Z == ((BoxKey)obj).Z;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                int multi = 486187739;
                hash = hash * multi + X.GetHashCode();
                hash = hash * multi + Z.GetHashCode();
                return hash;
            }
        }
    }
}
