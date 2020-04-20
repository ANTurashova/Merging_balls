using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

namespace Merging_Balls
{
    public class Animator
    {
        private Graphics mainG;
        private int width, height;
        private readonly List<Animation> items = new List<Animation>(); 
        private Thread t;
        private bool stop;
        private BufferedGraphics bg;
        private bool bgChanged;
        public Animator(Graphics g, Rectangle r)
        {
            Update(g, r);
        }

        public void Update(Graphics g, Rectangle r)
        {
            mainG = g;
            width = r.Width; 
            height = r.Height;
            bgChanged = true; 
            bg = BufferedGraphicsManager.Current.Allocate(  
                mainG,
                new Rectangle(0, 0, width, height)
            );
            Monitor.Enter(items);
            foreach (var item in items)
            {
                item.Update(new Rectangle(0, 0, width, height));
            }
            Monitor.Exit(items);
        }

        private void Animate()
        {
            while (!stop)
            {
                bgChanged = false;
                Graphics g = bg.Graphics;
                g.Clear(Color.White);
                Monitor.Enter(items);
                int cnt = items.Count;
                for (int i = 0; i < cnt; i++)
                {
                    if (!items[i].IsAlive)
                    {
                        items.Remove(items[i]);
                        i--;
                        cnt--;
                    }
                }
                foreach (var item in items) { item.Draw(g); }
                Monitor.Exit(items);

                if (!bgChanged)
                {
                    try { bg.Render(); } 
                    catch (Exception) { }
                } 
                Thread.Sleep(30);
            }
        }

        public void Start(Animation item)
        {
            if (t == null || !t.IsAlive)
            {
                stop = false;
                ThreadStart th = Animate;
                t = new Thread(th);
                t.Start();
            }
            Monitor.Enter(items);
            Thread.Sleep(15);
            item.Start();
            items.Add(item);
            Monitor.Exit(items);
        }

        public void Stop()
        {
            stop = true;
            foreach (var item in items) { item.Stop(); }
            items.Clear();
        }
    }

    public abstract class CommonData<O>
    {
        protected static int MaxSize; 
        protected readonly Queue<O>[] Vals;

        public CommonData(int maxSize)
        {
            MaxSize = maxSize;
            Vals = new Queue<O>[3];

            for (int i = 0; i < 3; i++)
            {
                Vals[i] = new Queue<O>();
            }
        }
        public void Add(int index, O value) 
        {
            index = Math.Abs(index % 3);
            var q = Vals[index];
            try
            {
                while (q.Count >= MaxSize) { 
                    Monitor.Wait(q); 
                }
                q.Enqueue(value); 
                Monitor.PulseAll(q);
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }
        }
        public abstract O[] GetNextData();
    }
}
