using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;

namespace Life
{
    static class Model
    {
        public class FieldParam : INotifyPropertyChanged
        {
            public FieldParam(int height, int width)
            {
                Height = height;
                Width = width;
                Count = 1;
                Delay = 100;
                ThreadsCount = 1;
                Generation = 1;
            }

            private int _height;
            private int _width;
            private int _count;
            private int _thcount;
            private int _delay;
            private int _gen;

            public int Generation
            {
                get { return _gen; }
                set
                {
                    _gen = value;
                    OnPropertyChanged("Generation");
                }
            }
            public int Delay
            {
                get { return _delay; }
                set
                {
                    _delay = value;
                    if (value < 100) _delay = 100;
                    OnPropertyChanged("Delay");
                }
            }
            public int ThreadsCount
            {
                get { return _thcount; }
                set
                {
                    _thcount = value;
                    if (value > 20) _thcount = 20;
                    OnPropertyChanged("ThreadsCount");
                }
            }
            public int Count
            {
                get { return _count; }
                set
                {
                    _count = value;
                    if (value < 1) _count = 1; 
                    if (value > Height * Width) _count = Height * Width;
                    OnPropertyChanged("Count");
                }
            }
            public int Height
            {
                get { return _height; }
                set
                {
                    _height = value;
                    if (value < 10) _height = 10;
                    if (value > 1000) _height = 1000;
                    OnPropertyChanged("Height");
                    OnPropertyChanged("CHeight");
                }
            }
            public int Width
            {
                get { return _width; }
                set
                {
                    _width = value;
                    if (value < 10) _width = 10;
                    if (value > 1000) _width = 1000;
                    OnPropertyChanged("Width");
                    OnPropertyChanged("CWidth");
                }
            }
            public int CHeight
            {
                get { return _height*CellSize; }
            }
            public int CWidth
            {
                get { return _width*CellSize; }
            }

            public void Clear()
            {
                Delay = 100;
                ThreadsCount = 1;
                Generation = 1;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        

        public static bool InProgress = false;
        public static FieldParam fieldParam;
        public static int CellSize = 10;
        public static System.Drawing.Color CellFill = System.Drawing.Color.LightBlue;
        public static NavigationService Navigator;
        public static int[,] currentGen;
        public static List<Point> startCells = new List<Point>();
        public static List<Point?> cells = new List<Point?>();
        public static event EventHandler onGameOver;
        public static event EventHandler Update;
        private static bool isChanged = false;
        private static object locker = new object();



        public static void CreateField()
        {
            currentGen = new int[fieldParam.Width, fieldParam.Height];
            cells.Clear();
            fieldParam.Clear();
        }

        public static void ResetField()
        {
            currentGen = new int[fieldParam.Width, fieldParam.Height];
            cells.Clear();
            foreach (Point p in startCells)
            {
                cells.Add(p);
                currentGen[(int)p.X, (int)p.Y] = 1;
                Set(p);
            }
        }

        public static void Fill()
        {
            Random rnd = new Random();
            for (int i=0; i< fieldParam.Count; i++)
            {
                int x = rnd.Next(fieldParam.Width);
                int y = rnd.Next(fieldParam.Height);
                while (currentGen[x, y] == 1)
                {
                    x = rnd.Next(fieldParam.Width);
                    y = rnd.Next(fieldParam.Height);
                }
                AddCell(x, y);
            }            
        }

        public static void AddCell(int x, int y)
        {
            currentGen[x, y] = 1;
            Point p = new Point(x, y);
            Set(p);
            cells.Add(p);
        }

        public static void NextAddCell(int x, int y)
        {
            currentGen[x, y] = 0;
            Point p = new Point(x, y);
            cells.Add(p);
            isChanged = true;
        }

        public static void StartGame()
        {
            startCells.Clear();
            InProgress = true;
            foreach (Point? p in cells)
                if (p.HasValue) startCells.Add(p.Value);
            
            Thread th = new Thread(new ThreadStart(() =>
            {
                List<Thread> thList = new List<Thread>();
                for (int i = 0; i < fieldParam.ThreadsCount; i++)
                    thList.Add(null);
                int currentThread = 0;
                while (InProgress)
                {
                    isChanged = false;
                    for (int i = 0; i < cells.Count; i++)
                    {
                        if (currentThread >= fieldParam.ThreadsCount) currentThread = 0;
                        if (thList[currentThread] == null || !thList[currentThread].IsAlive)
                        {
                            thList[currentThread] = new Thread(new ParameterizedThreadStart(Worker));
                            thList[currentThread].Start(i);
                            currentThread++;
                        }
                        else if (thList[currentThread].IsAlive)
                        {
                            while (thList[currentThread].IsAlive)
                            {
                                currentThread++;
                                if (currentThread >= fieldParam.ThreadsCount) currentThread = 0;
                            }
                            thList[currentThread] = new Thread(new ParameterizedThreadStart(Worker));
                            thList[currentThread].Start(i);
                            currentThread++;
                        }
                    }

                    foreach (Thread t in thList) t.Join();
                    if (!InProgress) break;

                    if (cells.Count == 0 || !isChanged)
                    {
                        InProgress = false;
                        break;
                    }

                    currentGen = new int[fieldParam.Width, fieldParam.Height];

                    foreach (Point? p in cells)
                    {
                        if (p == null) continue;
                        currentGen[(int)p.Value.X, (int)p.Value.Y] = 1;
                        Set(p.Value);
                    }

                    fieldParam.Generation++;
                    Update.Invoke(null, null);


                    Thread.Sleep(fieldParam.Delay);
                    if (!InProgress) break;
                    for (int i = 0; i < cells.Count; i++)
                        if (cells[i] == null) cells.RemoveAt(i);
                }
                onGameOver.Invoke(null, null);
            }));
            th.Start();
        }

        private static void Worker(object o)
        {
            int i = (int)o;
            lock (locker)
            {
                if (cells[i] == null) return;

                int n = Check(cells[i].Value);

                if (n > 3 || n < 2)
                {
                    cells[i] = null;
                    isChanged = true;
                }
            }
        }

        private static void Set(Point p)
        {
            int x = (int)p.X;
            int y = (int)p.Y;
            int x1 = (int)p.X - 1;
            int y1 = (int)p.Y - 1;
            int x2 = (int)p.X + 1;
            int y2 = (int)p.Y + 1;

            if (x1 < 0) x1 = fieldParam.Width - 1;
            if (y1 < 0) y1 = fieldParam.Height - 1;
            if (x2 >= fieldParam.Width) x2 = 0;
            if (y2 >= fieldParam.Height) y2 = 0;

            if (currentGen[x1, y1] <= 0) currentGen[x1, y1]--;
            if (currentGen[x, y1] <= 0)  currentGen[x, y1]--;
            if (currentGen[x2, y1] <= 0) currentGen[x2, y1]--;
            if (currentGen[x1, y] <= 0)  currentGen[x1, y]--;
            if (currentGen[x2, y] <= 0)  currentGen[x2, y]--;
            if (currentGen[x1, y2] <= 0) currentGen[x1, y2]--;
            if (currentGen[x, y2] <= 0)  currentGen[x, y2]--;
            if (currentGen[x2, y2] <= 0) currentGen[x2, y2]--;
        }

        private static int Check(Point p)
        {
            int neighbours = 0;
            int x = (int)p.X;
            int y = (int)p.Y;
            int x1 = (int)p.X - 1;
            int y1 = (int)p.Y - 1;
            int x2 = (int)p.X + 1;
            int y2 = (int)p.Y + 1;

            if (x1 < 0) x1 = fieldParam.Width-1;
            if (y1 < 0) y1 = fieldParam.Height-1;
            if (x2 >= fieldParam.Width) x2 = 0;
            if (y2 >= fieldParam.Height) y2 = 0;

            if (currentGen[x1, y1] > 0) neighbours++;
            if (currentGen[x, y1] > 0) neighbours++;
            if (currentGen[x2, y1] > 0) neighbours++;
            if (currentGen[x1, y] > 0) neighbours++;
            if (currentGen[x2, y] > 0) neighbours++;
            if (currentGen[x1, y2] > 0) neighbours++;
            if (currentGen[x, y2] > 0) neighbours++;
            if (currentGen[x2, y2] > 0) neighbours++;
            
            if (currentGen[x1, y1] == -3) NextAddCell(x1, y1);
            if (currentGen[x, y1] == -3) NextAddCell(x, y1);
            if (currentGen[x2, y1] == -3) NextAddCell(x2, y1);
            if (currentGen[x1, y] == -3) NextAddCell(x1, y);
            if (currentGen[x2, y] == -3) NextAddCell(x2, y);
            if (currentGen[x1, y2] == -3) NextAddCell(x1, y2);
            if (currentGen[x, y2] == -3) NextAddCell(x, y2);
            if (currentGen[x2, y2] == -3) NextAddCell(x2, y2);

            return neighbours;
        }
    }
}
