using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Lines
{

    public class GameLogic
    {
        private bool[,] c;
        public int[,] Field { get; private set; }
        public Random seed { get; private set; }

        private bool[] seedling = new bool[81];

        private int m_available;
        private int AvailableFields { get { return m_available; } set { m_available = value; if (m_available == 0) GameOver.Invoke(this, Score); } }

        private int m_points;
        public int Score { get { return m_points; } private set { m_points = value; PointsChanged(this, new IntPoint { Data = m_points }); } }

        private void Conjure()
        {
            if (AvailableFields == 0) return;
            var index = seed.Next(0, 80);
            
            while (seedling[index] == true) { index++; if (index > 80) index = 0; }
            
            int i = index / 9;
            int j = index % 9;
            
            seedling[index] = true;

            Field[i, j] = seed.Next(1, 7);
            AvailableFields--;
            
            Appeared.Invoke(this, new IntPoint { X = i, Y = j, Data = Field[i,j] });
            AdjustPoints(i, j);
        }

        private void ConjureAt(int i, int j, int data)
        {
            Field[i, j] = data;
            seedling[i * 9 + j] = true;
            AvailableFields--;

            Appeared.Invoke(this, new IntPoint { X = i, Y = j, Data = data });
        }

        private bool AdjustPoints(int i, int j)
        {
            var p = RemoveLines(i, j);
            if (p > 0) Score += 10 + (p - 7) * (p - 7);
            return p != 0;
        }

        private void Destroy(int i, int j)
        {
            Field[i, j] = 0;
            seedling[i * 9 + j] = false;
            AvailableFields++;

            Disappeared.Invoke(this, new IntPoint { X = i, Y = j});
        }

        public bool Move(IntPoint from, IntPoint to)
        {
            c = new bool[9, 9];
            getAFields(to.X, to.Y);
            if (!c[from.X, from.Y]) return false;

            var clr = Field[from.X, from.Y];
            Destroy(from.X, from.Y);
            ConjureAt(to.X, to.Y, clr);

            if (!AdjustPoints(to.X, to.Y))            
                for (int i = 0; i < 3; i++) Conjure();
            
            return true;
        }

        private int RemoveLines(int p, int q) {
            int removed = 0;
            for (int i = 0; i < 4; i++)
                if (Length(p, q, i) + Length(p, q, i + 4) > 5)                
                    removed += Remove(p, q, i) + Remove(p, q, i + 4);
            if (removed > 0) { Destroy(p,q); removed++; }

            return removed;
        }


        private int Length(int i, int j, int direction)
        {
            var res = T28463(i, j, Field[i, j], direction, null);
            return res;
        }

        private int Remove(int i, int j, int direction)
        {
            return T28463(i, j, Field[i, j], direction, false);
        }

        private int T28463(int i, int j, int color, int direction, bool? toRemove)
        {
            try
            {
                if (Field[i, j] != color) return 0;
            }
            catch
            {
                return 0;
            }
            if (toRemove == true) Destroy(i,j);
            if (toRemove != null) toRemove = true;
            switch (direction)
            {
                case 0: return (toRemove != false ? 1 : 0) +T28463(i + 1, j, color, direction, toRemove);
                case 1: return (toRemove != false ? 1 : 0) +T28463(i + 1, j + 1, color, direction, toRemove);
                case 2: return (toRemove != false ? 1 : 0) +T28463(i, j + 1, color, direction, toRemove);
                case 3: return (toRemove != false ? 1 : 0) +T28463(i - 1, j + 1, color, direction, toRemove);
                case 4: return (toRemove != false ? 1 : 0) +T28463(i - 1, j, color, direction, toRemove);
                case 5: return (toRemove != false ? 1 : 0) +T28463(i - 1, j - 1, color, direction, toRemove);
                case 6: return (toRemove != false ? 1 : 0) +T28463(i, j - 1, color, direction, toRemove);
                case 7: return (toRemove != false ? 1 : 0) +T28463(i + 1, j - 1, color, direction, toRemove);
                default: return 0;
            }
        }

        public void Start()
        {
            for (int i = 0; i < 3; i++) Conjure();
        }

        private void getAFields(int i, int j)
        {
            try
            {
                if (c[i, j]) return;
                c[i, j] = true;
                if (Field[i, j] > 0) return;                

                getAFields(i - 1, j); getAFields(i + 1, j);
                getAFields(i, j - 1); getAFields(i, j + 1);
            }
            catch { return; }
        }

        public GameLogic() { c = null; Field = new int[9, 9]; seed = new Random(); AvailableFields = 81; }


        public string Save()
        {
            string result = Score.ToString() + ":";
            for (int i = 0; i < 9; i++) for (int j = 0; j < 9; j++) result += Field[i, j].ToString();
            return result;
        }

        public void Load(string saveData)
        {
            var d = saveData.Split(':');
            Score = int.Parse(d[0]);
            for (int i = 0; i < 9; i++) for (int j = 0; j < 9; j++) if (d[1][i * 9 + j] != '0') ConjureAt(i, j, d[1][i * 9 + j] - '0');
        }

        public void Restart()
        {
            Score = 0;
            for (int i = 0; i < 9; i++) for (int j = 0; j < 9; j++) if (Field[i, j] > 0) Destroy(i, j);
            Start();
        }

        public event EventHandler<IntPoint> Appeared;
        public event EventHandler<IntPoint> Disappeared;
        public event EventHandler<IntPoint> PointsChanged;
        public event EventHandler<int> GameOver;
    }
}
    
