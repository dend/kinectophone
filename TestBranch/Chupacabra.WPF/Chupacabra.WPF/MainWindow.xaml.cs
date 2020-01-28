using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Microsoft.Research.Kinect.Nui;

namespace KinectoPhone.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AsynchronousClient aClient;
        DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeGameState();

            aClient = new AsynchronousClient();
            aClient.ResponseReceived += new ResponseReceivedEventHandler(aClient_ResponseReceived);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

        }

        void timer_Tick(object sender, EventArgs e)
        {
            sendData();
        }

        void sendData()
        {
            aClient.SendData(currentIndex);
        }

        void aClient_ResponseReceived(object sender, ResponseReceivedEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                var values = e.Response.Split('|');

                cameraTargetRotation = Convert.ToDouble(values[0]);

            }), DispatcherPriority.Normal);
        }





        /***********************************************************************************************************************
         ***********************************************************************************************************************
         ***********************************************************************************************************************
          BEWARE: BEYOND HERE THERE BE DRAGONS
         ***********************************************************************************************************************
         ***********************************************************************************************************************
         ***********************************************************************************************************************/



        DispatcherTimer dt = new DispatcherTimer();
        Runtime nui;

        private void InitializeGameState() {
            SetupKinect();
            SetupGameUI();
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            this.KeyUp += new KeyEventHandler(MainWindow_KeyUp);
        }

        bool keyDirty = false;

        void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            keyDirty = false;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (keyDirty) return;
            keyDirty = true;
            if (e.Key == Key.Left)
            {
                turn(90);
            }
            if (e.Key == Key.Right)
            {
                turn(-90);
            }
            if (e.Key == Key.Up)
            {
                jump();
            }
        }

        double cameraRotation = 0;
        double cameraTargetRotation = 0;

        private void SetupGameUI()
        {
            gemOff = new SolidColorBrush(Colors.White);
            gemOn = new SolidColorBrush(Colors.Yellow);

            handL.bkg.Fill = new SolidColorBrush(Colors.Red);
            handR.bkg.Fill = new SolidColorBrush(Colors.Blue);
            follow.bkg.Fill = new SolidColorBrush(Colors.Yellow);
            bodyCore.bkg.Fill = new SolidColorBrush(Colors.Green);

            setup3DElements();
            setupTimer();
        }

        private void setupTimer()
        {
            dt.Interval = TimeSpan.FromMilliseconds(20);
            dt.Tick += new EventHandler(update);
            dt.Start();
        }



        public void openGates() {
            Matrix3D mat = Matrix3D.Identity;
            mat.OffsetX = 3.0;
            mat.OffsetY = 0;
            mat.OffsetZ = -3.0;

            mat.RotateAt(new Quaternion(new Vector3D(0, 1, 0), 265), new Point3D(mat.OffsetX, mat.OffsetY, mat.OffsetZ));
            gate.Transform = new MatrixTransform3D(mat);
            floorTiles[13].openTiles = new int[2] { 12,14 }; // THIS CHANGES TO { 12, 14 } ONCE THE WALL IS MOVED
        }

        public void closeGates()
        {
            Matrix3D mat = Matrix3D.Identity;
            mat.OffsetX = 3.0;
            mat.OffsetY = 0;
            mat.OffsetZ = -3.0;
            mat.RotateAt(new Quaternion(new Vector3D(0, 1, 0), 180), new Point3D(mat.OffsetX, mat.OffsetY, mat.OffsetZ));
            gate.Transform = new MatrixTransform3D(mat);
            floorTiles[13].openTiles = new int[1] { 14 }; // THIS CHANGES TO { 12, 14 } ONCE THE WALL IS MOVED
        }

        // *****************************************
        void update(object sender, EventArgs e)
        {
            currentX += (targetX - currentX) * .1;
            currentZ += (targetZ - currentZ) * .1;

            pointLight.Position = new Point3D(currentX, 1, currentZ);

            if (isGrab)
            {
                gemIndex = currentIndex;
                gemPosition.OffsetX = currentX;
                gemPosition.OffsetZ = currentZ;
            }
            gemRotation.Angle += 12;



            //tempTestTurn();
            tempTestJump();
            testGrab();

            SetCamera();
        }


        bool isGrab = false;
        int gemIndex = 3;

        SolidColorBrush gemOn;
        SolidColorBrush gemOff;

        void testGrab()
        {
            if (leftDeep < -.35 && rightDeep < -.35)
            {
                if (!isGrab && currentIndex == gemIndex)
                {
                    isGrab = true;
                    gemMaterial0.AmbientColor = gemOn.Color;
                    gemMaterial1.AmbientColor = gemOn.Color;
                }
            }
            else
            {
                if (isGrab)
                {
                    isGrab = false;
                    gemMaterial0.AmbientColor = gemOff.Color;
                    gemMaterial1.AmbientColor = gemOff.Color;
                    if (gemIndex == 5)
                    {
                        openGates();
                    }
                    else
                    {
                        closeGates();
                    }
                }
            }
        }

        bool jumpDirty = false;

        void tempTestJump()
        {
            var deltaY = (bodyCore.y - follow.y);
            follow.y += deltaY * .1;
            follow.x += (bodyCore.x - follow.x) * .1;
            if (deltaY < -10)
            {
                if (!jumpDirty)
                {
                    jumpDirty = true;
                    jump();
                }
            }
            else
            {
                jumpDirty = false;
            }

        }

        bool turnLDirty = false;
        bool turnRDirty = false;

        void tempTestTurn()
        {
            if (phL.Y > .2 && phR.Y < -.4)
            {
                if (!turnRDirty)
                {
                    turnRDirty = true;
                    cameraTargetRotation -= 45;
                }
            }
            else
            {
                turnRDirty = false;
            }
            if (phL.Y < -.4 && phR.Y > .2)
            {
                if (!turnLDirty)
                {
                    turnLDirty = true;
                    cameraTargetRotation += 45;
                }
            }
            else
            {
                turnLDirty = false;
            }
        }

        private void turn(double delta)
        {
            cameraTargetRotation += delta;
        }

        int currentIndex = 0;

        double currentX = 0;
        double currentZ = 0;
        double targetX = 0;
        double targetZ = 0;

        void jump()
        {
            if (cameraTargetRotation % 90 != 0)
            {
                return;
            }

            int testIndex = currentIndex;
            double testTargetRotation = cameraTargetRotation % 360;
            if (testTargetRotation < 0) testTargetRotation += 360;
            switch ((int)testTargetRotation)
            {
                case 0:
                    testIndex += 1;
                    break;
                case 90:
                    testIndex -= 5;
                    break;
                case 180:
                    testIndex -= 1;
                    break;
                case 270:
                    testIndex += 5;
                    break;
            }

            if (floorTiles[currentIndex].openTiles.Contains(testIndex))
            {
                currentIndex = testIndex;
                targetX = floorTiles[currentIndex].x;
                targetZ = floorTiles[currentIndex].z;
            }

            if (currentIndex == 12 && !isWin)
            {
                isWin = true;
            }
        }

        bool isWin = false;

        private void SetCamera()
        {
            cameraRotation += (cameraTargetRotation - cameraRotation) * .3;

            PerspectiveCamera camera = (PerspectiveCamera)Screen3D.Camera;

            Matrix3D m = Matrix3D.Identity;
            m.OffsetX = currentX;
            m.OffsetY = .4;
            m.OffsetZ = currentZ;
            m.RotateAt(new Quaternion(new Vector3D(0, 1, 0), cameraRotation), new Point3D(m.OffsetX, m.OffsetY, m.OffsetZ));

            camera.Transform = new MatrixTransform3D(m);

        }

        Point phL = new Point();
        Point phR = new Point();

        void SetupKinect()
        {
            nui = new Runtime();

            try
            {
                nui.Initialize(RuntimeOptions.UseSkeletalTracking);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Runtime initialization failed. Please make sure Kinect device is plugged in.");
                return;
            }

            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
        }

        Point pCore = new Point(0, 0);

        double leftDeep = 0;
        double rightDeep = 0;
        double coreDeep = 0;

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;

            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {

                    phL = new Point(data.Joints[JointID.HandLeft].Position.X, data.Joints[JointID.HandLeft].Position.Y);
                    phR = new Point(data.Joints[JointID.HandRight].Position.X, data.Joints[JointID.HandRight].Position.Y);
                    pCore = new Point(data.Joints[JointID.HipCenter].Position.X, data.Joints[JointID.HipCenter].Position.Y);

                    coreDeep = data.Joints[JointID.HipCenter].Position.Z;

                    leftDeep = data.Joints[JointID.HandLeft].Position.Z - coreDeep;
                    rightDeep = data.Joints[JointID.HandRight].Position.Z - coreDeep;

                    handL.x = 640 + 600 * phL.X;
                    handL.y = 350 - 300 * phL.Y;
                    handR.x = 640 + 600 * phR.X;
                    handR.y = 350 - 300 * phR.Y;

                    bodyCore.x = 640 + 30 * pCore.X;
                    bodyCore.y = 350 - 300 * pCore.Y;

                    follow.x = bodyCore.x;
                }
            }
        }

        List<FloorTile> floorTiles = new List<FloorTile>();

        PhotoPanel3D gate;

        private void setup3DElements()
        {
            for (int i = 0; i < 15; i++)
            {
                FloorTile f0 = new FloorTile(i, .5 + Math.Floor(i / 5.0), -.5 - i % 5);
                floorTiles.Add(f0);
            }

            floorTiles[0].openTiles = new int[1] { 1 };
            floorTiles[1].openTiles = new int[2] { 0, 2 };
            floorTiles[2].openTiles = new int[2] { 1, 7 };
            floorTiles[3].openTiles = new int[1] { 4 };
            floorTiles[4].openTiles = new int[2] { 3, 9 };
            floorTiles[5].openTiles = new int[1] { 10 };
            floorTiles[6].openTiles = new int[2] { 7, 11 };
            floorTiles[7].openTiles = new int[3] { 2, 6, 8 };
            floorTiles[8].openTiles = new int[2] { 9, 7 };
            floorTiles[9].openTiles = new int[3] { 4, 8, 14 };
            floorTiles[10].openTiles = new int[2] { 5, 11 };
            floorTiles[11].openTiles = new int[2] { 6, 10 };
            floorTiles[12].openTiles = new int[1] { 13 };
            floorTiles[13].openTiles = new int[1] { 14 }; // THIS CHANGES TO { 12, 14 } ONCE THE WALL IS MOVED
            floorTiles[14].openTiles = new int[2] { 9, 13 };

            currentIndex = 0;

            currentX = 0;
            currentZ = 0;
            targetX = floorTiles[currentIndex].x;
            targetZ = floorTiles[currentIndex].z;



            List<WallData> walls = new List<WallData>();

            // OUTER LEFT WALL
            walls.Add(new WallData(0, 0, 0, 90));
            walls.Add(new WallData(0, 0, -1, 90));
            walls.Add(new WallData(0, 0, -2, 90));
            walls.Add(new WallData(0, 0, -3, 90));
            walls.Add(new WallData(0, 0, -4, 90));

            // OUTER RIGHT WALL
            walls.Add(new WallData(3, 0, 0, 90));
            walls.Add(new WallData(3, 0, -1, 90));
            walls.Add(new WallData(3, 0, -2, 90));
            walls.Add(new WallData(3, 0, -3, 90));
            walls.Add(new WallData(3, 0, -4, 90));

            // OUTER BACK WALL
            walls.Add(new WallData(0, 0, -5, 0));
            walls.Add(new WallData(1, 0, -5, 0));
            walls.Add(new WallData(2, 0, -5, 0));



            // OUTER FRONT WALL
            walls.Add(new WallData(1, 0, 0, 180));
            walls.Add(new WallData(2, 0, 0, 180));
            walls.Add(new WallData(3, 0, 0, 180));

            // EDGES
            walls.Add(new WallData(1, 0, 0, 90));
            walls.Add(new WallData(1, 0, -1, 0));
            walls.Add(new WallData(1, 0, -1, 90));
            walls.Add(new WallData(0, 0, -3, 0));
            walls.Add(new WallData(1, 0, -3, 90));

            walls.Add(new WallData(2, 0, -3, 90));
            walls.Add(new WallData(2, 0, -2, 90));
            walls.Add(new WallData(2, 0, -2, 0));

            Random r = new Random();

            foreach (WallData wd in walls)
            {

                PhotoPanel3D panel = new PhotoPanel3D();

                string picName = "bush" + r.Next(2).ToString("0") + ".png";
                panel.setBaseImage(picName);
                Matrix3D m = panel.Transform.Value;
                m.OffsetX = wd.x;
                m.OffsetY = wd.y;
                m.OffsetZ = wd.z;
                m.RotateAt(new Quaternion(new Vector3D(0, 1, 0), wd.rotation), new Point3D(m.OffsetX, m.OffsetY, m.OffsetZ));
                panel.Transform = new MatrixTransform3D(m);

                Screen3D.Children.Add(panel);
            }

            // GATE
            gate = new PhotoPanel3D();
            gate.setBaseImage("gate.png");
            Matrix3D mat = gate.Transform.Value;
            mat.OffsetX = 3.0;
            mat.OffsetY = 0;
            mat.OffsetZ = -3.0;
            mat.RotateAt(new Quaternion(new Vector3D(0, 1, 0), 180), new Point3D(mat.OffsetX, mat.OffsetY, mat.OffsetZ));
            gate.Transform = new MatrixTransform3D(mat);
            Screen3D.Children.Add(gate);

            // FLOOR
            PhotoPanel3D floor = new PhotoPanel3D();
            floor.setBaseImage("floor.png");
            mat = Matrix3D.Identity;
            mat.OffsetX = 0;
            mat.OffsetY = 0;
            mat.OffsetZ = 0;
            mat.Scale(new Vector3D(3,5,1));
            mat.Rotate(new Quaternion(new Vector3D(1, 0, 0), -90));
            floor.Transform = new MatrixTransform3D(mat);
            Screen3D.Children.Add(floor);

            // TARGET TILE
            PhotoPanel3D tTile = new PhotoPanel3D();
            tTile.setBaseImage("target.png");
            mat = Matrix3D.Identity;
            mat.OffsetX = 1;
            mat.OffsetY = 0;
            mat.OffsetZ = .01;
            mat.Rotate(new Quaternion(new Vector3D(1, 0, 0), -90));
            tTile.Transform = new MatrixTransform3D(mat);
            Screen3D.Children.Add(tTile);

        }
    }

    class WallData
    {
        public double x = 0;
        public double y = 0;
        public double z = 0;
        public double rotation = 0;

        public WallData(double _x, double _y, double _z, double _rotation)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
            this.rotation = _rotation;
        }
    }

    class FloorTile
    {
        public double x = 0;
        public double z = 0;
        public int index = 0;
        public int[] openTiles;

        public FloorTile(int _index, double _x, double _z)
        {
            index = _index;
            x = _x;
            z = _z;
        }
    }

}
