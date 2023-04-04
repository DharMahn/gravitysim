using System.Collections.Generic;
using System.Drawing;
using System;
using System.Numerics;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace solarsystem
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Random r = new();
            InitializeComponent();
            //for (int i = 0; i < 3; i++)
            //{
            //    curDots.Add(new Dot
            //    {
            //        Position = new Vector2(r.Next(ClientRectangle.Width), r.Next(ClientRectangle.Height))
            //    });
            //}
            Bounds = Screen.PrimaryScreen.Bounds;
            int slice = 100;
            for (int i = 0; i < slice; i++)
            {
                float posX = 960 + r.Next(-50, 50) + (float)Math.Cos(((Math.PI * 2) / slice) * i) * 20;
                float posY = 520 + r.Next(-50, 50) + (float)Math.Sin(((Math.PI * 2) / slice) * i) * 20;
                curDots.Add(new Dot
                {
                    Position = new Vector2(posX, posY),
                    Mass = 2,
                    Color = new SolidBrush(Color.FromArgb(
                        r.Next(256),
                        r.Next(256), r.Next(256))),
                });
            }
            MouseWheel += Form1_MouseWheel;
            mouseDot.Mass = 10;
            mouseDot.Color = new SolidBrush(Color.FromArgb(
                        r.Next(0),
                        r.Next(0), r.Next(0)));
            //curDots[0].Mass = 100;
            //curDots.Add(mouseDot);
            mouseDot.Position.X = PointToClient(Cursor.Position).X;
            mouseDot.Position.Y = PointToClient(Cursor.Position).Y;
            DoubleBuffered = true;
            numericUpDown1.Maximum = curDots.Count;
            
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            zoomScale += e.Delta / 120;
            zoomScale = Math.Max(zoomScale, 1);
            Console.WriteLine(zoomScale);
        }
        float zoomScale = 1f;
        private uint GetMonitorRefreshRate()
        {
            // Get the primary screen.
            Screen primaryScreen = Screen.PrimaryScreen;

            // Get the device name of the primary screen.
            string deviceName = primaryScreen.DeviceName;

            // Get the device settings of the primary screen.
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr hdc = graphics.GetHdc();
                Win32.DISPLAY_DEVICE displayDevice = new();
                displayDevice.cb = (uint)System.Runtime.InteropServices.Marshal.SizeOf(displayDevice);

                if (Win32.EnumDisplayDevices(deviceName, 0, ref displayDevice, 0))
                {
                    Win32.DEVMODE devMode = new();
                    devMode.dmSize = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(devMode);

                    if (Win32.EnumDisplaySettings(deviceName, Win32.ENUM_CURRENT_SETTINGS, ref devMode))
                    {
                        return devMode.dmDisplayFrequency;
                    }
                }
                graphics.ReleaseHdc(hdc);
            }
            return 0;
        }
        const int circleRadius = 1;
        class Dot
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Mass;
            public Brush Color;
        }
        List<Dot> curDots = new();
        List<Dot> newDots = new();
        readonly Dot mouseDot = new();
        readonly Stopwatch sw = new();
        private void RunTick()
        {
            sw.Start();

            newDots = new List<Dot>();

            for (int i = 0; i < curDots.Count; i++)
            {
                Vector2 posDelta = Vector2.Zero;
                for (int j = 0; j < curDots.Count; j++)
                {
                    if (curDots[i].Equals(curDots[j])) continue;
                    posDelta -= curDots[j].Mass * curDots[i].Mass * (curDots[i].Position - curDots[j].Position) / Vector2.DistanceSquared(curDots[i].Position, curDots[j].Position);
                }
                curDots[i].Velocity += posDelta;
                newDots.Add(curDots[i]);
            };
            for (int i = 0; i < newDots.Count; i++)
            {
                newDots[i].Position += newDots[i].Velocity;
            }
            if (click)
            {
                if (numericUpDown1.Value == -1)
                {
                    mouseDot.Position.X = PointToClient(Cursor.Position).X;
                    mouseDot.Position.Y = PointToClient(Cursor.Position).Y;
                }
                else
                {
                    mouseDot.Position.X = PointToClient(Cursor.Position).X + newDots[(int)numericUpDown1.Value].Position.X;
                    mouseDot.Position.Y = PointToClient(Cursor.Position).Y + newDots[(int)numericUpDown1.Value].Position.Y;
                }
            }
            totalEnergy = 0;
            //for (int i = 0; i < curDots.Count; i++)
            //{
            //    double speedSquared = curDots[i].Velocity.LengthSquared();
            //    double kineticEnergy = 0.5 * curDots[i].Mass * speedSquared;
            //    totalEnergy += kineticEnergy;
            //}

            //// Calculate potential energy
            //for (int i = 0; i < curDots.Count; i++)
            //{
            //    for (int j = i + 1; j < curDots.Count; j++)
            //    {
            //        double distance = Vector2.Distance(curDots[i].Position, curDots[j].Position);
            //        double potentialEnergy = -(curDots[i].Mass * curDots[j].Mass / distance);
            //        totalEnergy += potentialEnergy;
            //    }
            //}
            //label1.Text = totalEnergy.ToString("0.000");
            curDots = newDots;
            Invalidate();
            sw.Stop();
            elapsed = (float)sw.Elapsed.TotalSeconds;
            sw.Reset();
        }
        float elapsed = 1f;
        double totalEnergy = 0.0;
        protected override void OnPaint(PaintEventArgs e)
        {
            //e.Graphics.ResetTransform();
            Vector2 centeredDotPosition;
            if (numericUpDown1.Value > -1)
            {
                centeredDotPosition = curDots[(int)numericUpDown1.Value].Position;
                e.Graphics.TranslateTransform(ClientSize.Width / 2.0f, ClientSize.Height / 2.0f);
                e.Graphics.ScaleTransform(zoomScale, zoomScale);
                e.Graphics.TranslateTransform(-centeredDotPosition.X, -centeredDotPosition.Y);
                e.Graphics.DrawString(totalEnergy.ToString("0.000"), SystemFonts.DefaultFont, Brushes.Black, centeredDotPosition.X - (ClientSize.Width / 2) / zoomScale, centeredDotPosition.Y - (ClientSize.Height / 2) / zoomScale);

            }
            else
            {
                e.Graphics.TranslateTransform(0, 0);
                e.Graphics.ScaleTransform(zoomScale, zoomScale);
                e.Graphics.DrawString(totalEnergy.ToString("0.000"), SystemFonts.DefaultFont, Brushes.Black, 0, 0);

            }
            foreach (var item in curDots)
            {
                e.Graphics.FillEllipse(item.Color, item.Position.X - circleRadius, item.Position.Y - circleRadius, (circleRadius << 1) + 1, (circleRadius << 1) + 1);
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

        private void button1_Click(object sender, EventArgs e)
        {
            int refreshrate = (int)GetMonitorRefreshRate();
            Task.Run(() =>
            {
                while (true)
                {
                    RunTick();
                    Thread.Sleep((1000 / refreshrate));
                }
            });
        }
    }
    public static class Win32
    {
        public const int ENUM_CURRENT_SETTINGS = -1;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            public uint cb;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public uint StateFlags;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }
        public struct DEVMODE
        {
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public uint dmDisplayOrientation;
            public uint dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public ushort dmLogPixels;
            public uint dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;
            public uint dmDisplayFlags;
            public uint dmDisplayFrequency;
            public uint dmICMMethod;
            public uint dmICMIntent;
            public uint dmMediaType;
            public uint dmDitherType;
            public uint dmReserved1;
            public uint dmReserved2;
            public uint dmPanningWidth;
            public uint dmPanningHeight;
        }
    }
}