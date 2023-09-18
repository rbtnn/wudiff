
// https://note.affi-sapo-sv.com/js-diff.php

class WuDiff
{
    public enum FlgValue
    {
        DELETE,
        INSERT,
        COMMON,
    }

    private class FpItem
    {
        public int Previous { set; get; }
        public int Y { set; get; }

        public FpItem()
        {
            Previous = -1;
            Y = -1;
        }
    }

    private class PathItem
    {
        public int EndX { set; get; }
        public int EndY { set; get; }
        public int Length { set; get; }
        public int Previous { set; get; }

        public PathItem(int endX, int endY, int len, int prev)
        {
            EndX = endX;
            EndY = endY;
            Length = len;
            Previous = prev;
        }
    }

    public class ResultItem
    {
        public int S { set; get; }
        public int E { set; get; }
        public FlgValue Flg { set; get; }
        public string[] Target { set; get; }

        public ResultItem(int s, int e, FlgValue flg, string[] t)
        {
            S = s;
            E = e;
            Flg = flg;
            Target = t;
        }

        public void Print()
        {
            for (var i = S; i < E + 1; i++)
            {
                switch (Flg)
                {
                    case FlgValue.DELETE: Console.WriteLine(@"-{0}", Target[i]); break;
                    case FlgValue.INSERT: Console.WriteLine(@"+{0}", Target[i]); break;
                    default: Console.WriteLine(@" {0}", Target[i]); break;
                }
            }
        }
    }

    private readonly string[] A;
    private readonly string[] B;
    private readonly int M;
    private readonly int N;
    private readonly int Offset;
    private readonly int Delta;
    private readonly List<FpItem> Fp;
    private readonly List<PathItem> Path;

    public WuDiff(string[] a, string[] b)
    {
        A = (a.Length < b.Length) ? a : b;
        B = (a.Length < b.Length) ? b : a;
        M = A.Length;
        N = B.Length;
        Offset = M + 1;
        Delta = N - M;
        Fp = new List<FpItem>();
        Path = new List<PathItem>();
    }

    public List<ResultItem> Exec()
    {
        var p = -1;

        Fp.Clear();
        Path.Clear();
        for (var i = 0; i < M + N + 3; i++)
        {
            Fp.Add(new FpItem());
        }

        do
        {
            p++;
            for (var k = -p; k <= Delta - 1; k++)
            {
                Fp[k + Offset].Y = Snake(k, Math.Max(Fp[k - 1 + Offset].Y + 1, Fp[k + 1 + Offset].Y));
            }
            for (var k = Delta + p; k >= Delta + 1; k--)
            {
                Fp[k + Offset].Y = Snake(k, Math.Max(Fp[k - 1 + Offset].Y + 1, Fp[k + 1 + Offset].Y));
            }
            Fp[Delta + Offset].Y = Snake(Delta, Math.Max(Fp[Delta - 1 + Offset].Y + 1, Fp[Delta + 1 + Offset].Y));
        }
        while (Fp[Delta + Offset].Y != N);

        return GetSec();
    }

    private int Snake(int k, int y)
    {
        var x = y - k;
        var sx = x;
        var sy = y;
        while (x < M && y < N && A[x] == B[y])
        {
            ++x;
            ++y;
        }
        var left = Fp[k - 1 + Offset].Y;
        var right = Fp[k + 1 + Offset].Y;
        var leftOrRight = (left > right) ? -1 : 1;
        var prev = Fp[k + leftOrRight + Offset].Previous;
        if (sx != x)
        {
            Fp[k + Offset].Previous = Path.Count;
            Path.Add(new PathItem(x - 1, y - 1, x - sx, prev));
        }
        else
        {
            Fp[k + Offset].Previous = prev;
        }
        return y;
    }

    private List<ResultItem> GetSec()
    {
        var result = new List<ResultItem>();
        var cX = M - 1;
        var cY = N - 1;
        var sX = -1;
        var sY = -1;
        var prev = Fp[Delta + Offset].Previous;

        while (prev != -1)
        {
            var _ = Path[prev];
            sX = _.EndX - _.Length + 1;
            sY = _.EndY - _.Length + 1;
            if (cX != _.EndX) result.Add(new ResultItem(_.EndX + 1, cX, FlgValue.DELETE, A));
            if (cY != _.EndY) result.Add(new ResultItem(_.EndY + 1, cY, FlgValue.INSERT, B));
            result.Add(new ResultItem(sX, _.EndX, FlgValue.COMMON, A));
            cX = sX - 1;
            cY = sY - 1;
            prev = Path[prev].Previous;
        }
        if (sX != 0) result.Add(new ResultItem(0, cX, FlgValue.DELETE, A));
        if (sY != 0) result.Add(new ResultItem(0, cY, FlgValue.INSERT, B));
        result.Reverse();

        return result;
    }
}

class Prog
{
    public static void Main()
    {
        var a = File.ReadAllLines(@"../input1.txt");
        var b = File.ReadAllLines(@"../input2.txt");
        foreach (var x in (new WuDiff(a, b)).Exec())
        {
            x.Print();
        }
    }
}
