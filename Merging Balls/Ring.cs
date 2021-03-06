﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

namespace Merging_Balls
{
    class Ring : Animation
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        private int width, height;
        public Color RingColor { get; private set; }
        private Brush brush;

        private int maxRadius
        {
            get { return (width > height) ? width : height; }
            set { }
        }

        public int Radius { get; private set; }
        public Ring(Color c, Rectangle r) : base(r)
        {
            width = r.Width;
            height = r.Height;
            X = width/2; //центр колец
            Y = height/10*9; //центр колец
            RingColor = c;
            Radius = 0;
            brush = new SolidBrush(RingColor);
        }

        public override void Update(Rectangle rectangle)
        {
            width = rectangle.Width;
            height = rectangle.Height;
        }

        public override void Draw(Graphics g)
        {
            g.FillEllipse(brush, X, Y, 2 * Radius, 2 * Radius);
        }

        protected override void Move()
        {
            while (!stop && Radius < maxRadius)
            {
                X -= 1;
                Y -= 1;
                Radius += 1;
                Thread.Sleep(5);
                int alpha = Math.Abs((int)((1.0 - (float)Radius / maxRadius) * 255)) % 255;
                RingColor = Color.FromArgb(
                    (int)(alpha),
                    RingColor.R,
                    RingColor.G,
                    RingColor.B
                );
                brush = new SolidBrush(RingColor);
            }
        }
    }
}
