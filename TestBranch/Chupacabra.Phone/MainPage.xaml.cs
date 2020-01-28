using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Devices.Sensors;
using System.Diagnostics;
using System.Windows.Threading;

namespace KinectoPhone.Phone
{
    public partial class MainPage : PhoneApplicationPage
    {
        Accelerometer accelerometer;
        DispatcherTimer timer;

        SocketHandler handler;

        int previousPos;
        int currentPos;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            handler = new SocketHandler("192.168.1.6", 13001);
            handler.ResponseReceived += new ResponseReceivedEventHandler(handler_ResponseReceived);

            accelerometer = new Accelerometer();
            accelerometer.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(accel_ReadingChanged);
            accelerometer.Start();

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Start();

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            curTriangleAngle += (targetTriangleAngle - curTriangleAngle) *.3;
            playerRotation.Angle = -curTriangleAngle;
            setPosition();
        }

        double curX = 0;
        double curY = 0;

        void setPosition()
        {
            targetX = 320 - Math.Floor(targetIndex/5) * 160;
            targetY = (targetIndex % 5) * 160;
            curX += (targetX - curX) * .3;
            curY += (targetY - curY) * .3;
            player.SetValue(Canvas.LeftProperty, curX);
            player.SetValue(Canvas.TopProperty, curY);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            handler.SendData(string.Format("p|{0}", curTriangleAngle.ToString("0")));
        }

        double targetX = 0;
        double targetY = 0;
        int targetIndex = 0;

        void handler_ResponseReceived(object sender, ResponseReceivedEventArgs e)
        {
            previousPos = currentPos;

            string[] data = e.response.Split('|');
            int.TryParse(data[1], out currentPos);

            if (currentPos != previousPos)
            {
                targetIndex = currentPos;
            }
        }

        double curTriangleAngle = 0;
        double targetTriangleAngle = 0;
        bool isTurnDirty = false;

        double previousY = 0;
        double currentY = 0;

        DateTime lastTurnTime = DateTime.Now;
        DateTime currentTurnTime;

        void Turn(double direction)
        {
            currentTurnTime = DateTime.Now;
            TimeSpan diffResult = currentTurnTime.Subtract(lastTurnTime);
            if (diffResult.TotalSeconds > 1)
            {
                lastTurnTime = currentTurnTime;
                targetTriangleAngle += direction * 90.0;
            }
        }


        void accel_ReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                previousY = currentY;
                currentY = e.Y;

                if (e.Y > .7 || e.Y < -.7)
                {
                    if (e.Y > .7 && !isTurnDirty)
                    {
                        isTurnDirty = true;
                        Turn(1);
                    }
                    else if (e.Y < -.7 && !isTurnDirty)
                    {
                        isTurnDirty = true;
                        Turn(-1);
                    }
                }
                else
                {
                    isTurnDirty = false;
                }
             
            }));
        }
    }
}