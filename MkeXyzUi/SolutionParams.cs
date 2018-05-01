namespace MkeXyzUi
{
    using System.Runtime.Serialization;

    [DataContract]
    public class SolutionParams
    {
        public int N => x.Length * y.Length * z.Length;

        [DataMember]
        public double Lambda { get; set; } = 1;

        [DataMember]
        public double Gamma { get; set; } = 1;

        [DataMember]
        public double Beta { get; set; } = 1;

        [DataMember]
        public double[] x { get; set; } = { 1, 2, 3, 4, 5 };

        [DataMember]
        public double[] y { get; set; } = { 1, 2, 3, 4, 5 };

        [DataMember]
        public double[] z { get; set; } = { 1, 2, 3, 4, 5 };

        [DataMember]
        public bool TopFirst { get; set; }

        [DataMember]
        public bool TopSecond { get; set; }

        [DataMember]
        public bool TopThird { get; set; }

        [DataMember]
        public bool BottomFirst { get; set; }

        [DataMember]
        public bool BottomSecond { get; set; }

        public bool BottomThird { get; set; }

        [DataMember]
        public bool LeftFirst { get; set; }

        [DataMember]
        public bool LeftSecond { get; set; }

        [DataMember]
        public bool LeftThird { get; set; }

        [DataMember]
        public bool RightFirst { get; set; }

        [DataMember]
        public bool RightSecond { get; set; }

        [DataMember]
        public bool RightThird { get; set; }

        [DataMember]
        public bool FrontFirst { get; set; }

        [DataMember]
        public bool FrontSecond { get; set; }

        [DataMember]
        public bool FrontThird { get; set; }

        [DataMember]
        public bool BackFirst { get; set; }

        [DataMember]
        public bool BackSecond { get; set; }

        [DataMember]
        public bool BackThird { get; set; }
    }
}
