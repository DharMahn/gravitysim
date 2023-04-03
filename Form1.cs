using System.Collections.Generic;
using System.Drawing;
using System;
using System.Numerics;
using System.Windows.Forms;
using System.Diagnostics;

namespace solarsystem
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Random r = new Random();
            InitializeComponent();
            //for (int i = 0; i < 3; i++)
            //{
            //    curDots.Add(new Dot
            //    {
            //        Position = new Vector2(r.Next(ClientRectangle.Width), r.Next(ClientRectangle.Height))
            //    });
            //}
            int slice = 5;
            for (int i = 0; i < slice; i++)
            {
                float posX = 200 + (float)Math.Cos(((Math.PI * 2) / slice) * i) * 150;
                float posY = 200 + (float)Math.Sin(((Math.PI * 2) / slice) * i) * 150;
                curDots.Add(new Dot
                {
                    Position = new Vector2(posX, posY),
                });
            }
            //curDots.Add(mouseDot);
            mouseDot.Position.X = PointToClient(Cursor.Position).X;
            mouseDot.Position.Y = PointToClient(Cursor.Position).Y;
            DoubleBuffered = true;
        }
        class Dot
        {
            public Vector2 Position;
            public Vector2 Velocity;
        }
        List<Dot> curDots = new List<Dot>();
        List<Dot> newDots = new List<Dot>();
        Dot mouseDot = new Dot();
        Stopwatch sw = new Stopwatch();
        private void timer1_Tick(object sender, EventArgs e)
        {
            sw.Start();

            newDots = new List<Dot>();
            
            for (int i = 0; i < curDots.Count; i++)
            {
                Vector2 posDelta = Vector2.Zero;
                for (int j = 0; j < curDots.Count; j++)
                {
                    if (curDots[i].Equals(curDots[j])) continue;
                    posDelta -= (curDots[i].Position - curDots[j].Position) / Vector2.Distance(curDots[i].Position, curDots[j].Position);
                }
                curDots[i].Velocity += posDelta / 10;
                newDots.Add(curDots[i]);
                
            }
            for (int i = 0; i < newDots.Count; i++)
            {
                newDots[i].Position += newDots[i].Velocity;
            }
            if (click)
            {
                mouseDot.Position.X = PointToClient(Cursor.Position).X;
                mouseDot.Position.Y = PointToClient(Cursor.Position).Y;
            }
            curDots = newDots;
            Invalidate();
            sw.Stop();
            elapsed = sw.ElapsedMilliseconds;
        }
        float elapsed;
        protected override void OnPaint(PaintEventArgs e)
        {
            foreach (var item in curDots)
            {
                e.Graphics.FillEllipse(Brushes.Black, item.Position.X - 10, item.Position.Y - 10, 20, 20);
            }
        }
        bool click = false;
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            click = true;
            curDots.Add(mouseDot);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            click = false;
            curDots.Remove(mouseDot);
        }
    }
}