namespace Mke.Helpers
{
    using System.Collections.Generic;
    public class PairComparer : IComparer<Pair>
    {
        public int N { get; set; }

        public int Compare(Pair x, Pair y)
        {
            if (x.First * N + x.Second > y.First * N + y.Second)
            {
                return 1;
            }

            if (x.First * N + x.Second < y.First * N + y.Second)
            {
                return -1;
            }

            return 0;
        }
    }
}