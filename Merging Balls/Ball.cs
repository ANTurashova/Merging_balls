using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

namespace Merging_Balls
{
    class BallConsumer : Animation
    {
        Point position;
        int radius;
        Color color = Color.White;
        Pen pen;
        Brush brush;
        Rectangle rectangle;
        BallCommonData data;
        Animator animator;
        public BallConsumer(BallCommonData data, Rectangle rect, int x, int y, int radius, Animator animator) : base(rect)  
        {
            rectangle = rect;
            this.radius = radius;
            this.animator = animator;
            this.data = data;
            position = new Point(x, y);
            pen = new Pen(Color.Black, 5);
            brush = new SolidBrush(color);
        }

        public override void Draw(Graphics g) 
        {
            Monitor.Enter(g);
            g.DrawEllipse(pen, position.X - radius * 2, position.Y - radius * 2, radius * 6, radius * 6);
            g.FillEllipse(brush, position.X - radius * 2, position.Y - radius * 2, radius * 6, radius * 6);
            Monitor.Exit(g);
        }

        public override void Update(Rectangle rectangle)
        {
            this.rectangle = rectangle;
        }

        protected override void Move()
        {
            while (true)
            {
                var value = data.GetNextData();
                color = Color.FromArgb(value[0].Color.A, value[1].Color.A, value[2].Color.A);
                brush = new SolidBrush(color);
                animator.Start(new Ring(color, rectangle));
            }
        }
    }

    class Ball : Animation
    { 
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Radius { get; private set; }
        public Color Color { get; private set; }

        private Point destination;

        private Brush brush;

        private Rectangle rectangle;
        public Ball(Rectangle rectangle, int x, int y, int radius, Point destination, Color color) : base(rectangle)
        {
            X = x; Y = y; Radius = radius;
            this.destination = destination;

            Random rnd = new Random((int)DateTime.Now.Ticks);
            this.Color = Color.FromArgb(rnd.Next(0, 255), color.R, color.G, color.B);
            this.brush = new SolidBrush(Color);
        }
        public override void Draw(Graphics g)
        {
            Monitor.Enter(g);
            g.FillEllipse(brush, X, Y, Radius * 2, Radius * 2);
            Monitor.Exit(g);
        }

        public override void Update(Rectangle rectangle)
        {
            this.rectangle = rectangle;
        }

        protected override void Move()
        {
            while (!stop)
            {
                Thread.Sleep(7);
                int dx = X - destination.X;
                int dy = Y - destination.Y;
                if (dx != 0) X += 1 * Math.Sign(-dx);
                if (dy != 0) Y += 1 * Math.Sign(-dy);
            }
        }
    }

    class BallCommonData : CommonData<Ball>
    {

        private Point Destination;

        public BallCommonData(int maxSize, Animator animator, Point destination) : base(maxSize)
        {

            Destination = destination; 

        }

        public override Ball[] GetNextData()
        {
            Ball[] res = new Ball[3];
            for (int i = 0; i < Vals.Length; i++)
            {
                var q = Vals[i];
                Monitor.Enter(q);
                try
                {
                    while (q.Count == 0 || q.Peek().X != Destination.X || q.Peek().Y != Destination.Y)
                    {
                        Monitor.Wait(q, 5); 
                    }
                    res[i] = q.Dequeue();
                    Monitor.PulseAll(q);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                finally
                {
                    Monitor.Exit(q);
                }
            }
            return res;
        }
    }
    class BallProducer : Animation
    {

        Point position;
        Random rand;
        int radius;
        private static Animator _animator;
        Rectangle rectangle;
        BallCommonData data;
        Point destination;
        Color color;
        int id;
        public BallProducer(BallCommonData data, int id, int x, int y, int radius, Rectangle rect, Animator animator, Point destination, Color color) : base(rect) 
        {
            position = new Point(x, y);
            this.destination = destination;
            this.data = data;
            this.color = color;
            this.radius = radius;
            this.id = id;
            if (_animator == null) _animator = animator;
            rand = new Random((int)DateTime.Now.Ticks); 
            Start();
        }

        protected void Produce()
        {
            var ball = new Ball(rectangle, position.X, position.Y, radius, destination, color);
            data.Add(id, ball);
            Monitor.Enter(_animator);
            _animator.Start(ball);
            Monitor.Exit(_animator);
            var waitTime = rand.Next(1500, 6000);
            Thread.Sleep(waitTime);
        }

        public override void Update(Rectangle rectangle)
        {
            this.rectangle = rectangle;
        }

        Pen pen = new Pen(Color.Black);
        public override void Draw(Graphics g)
        { 
            Monitor.Enter(g);
     
            g.DrawRectangle(pen, position.X, position.Y, radius * 2, radius * 2);
            Monitor.Exit(g); 
        }

        protected override void Move()
        {
            while (true)
            {
                Produce();
            };
        }
    }
}
