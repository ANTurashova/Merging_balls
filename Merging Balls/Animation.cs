using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

namespace Merging_Balls
{
    public abstract class Animation
    {
        private Thread thread;
        public bool IsAlive => thread != null && thread.IsAlive;
        protected bool stop;

        protected Animation(Rectangle rectangle)
        {
            Update(rectangle);
        }
        public abstract void Update(Rectangle rectangle);
        public void Start()
        {
            if (thread != null && thread.IsAlive) return;
            stop = false;
            ThreadStart th = Move;
            thread = new Thread(th);
            thread.Start();

        }
        public abstract void Draw(Graphics g); 
        protected abstract void Move();  
        public void Stop()
        {
            stop = true;
        }
        public void Abort()
        {
            try
            {
                thread.Abort();
                thread.Join();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
