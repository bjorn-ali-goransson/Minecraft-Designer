using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Minecraft3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Line> VerticalLines { get; } = new List<Line>();
        List<Line> HorizontalLines { get; } = new List<Line>();
        List<Line> MousePositionIndicatorLines { get; } = new List<Line>();

        List<Cube> Cubes { get; } = new List<Cube>();

        int CameraOldMouseX { get; set; }
        int CameraOldMouseY { get; set; }
        int CameraX { get; set; }
        int CameraY { get; set; }

        EventHandler Update;

        public MainWindow()
        {
            InitializeComponent();

            View.Camera = new PerspectiveCamera { Position = new Point3D(0, -10, 10), LookDirection = new Vector3D(0, 10, -10) };
            View.Children.Add(new ModelVisual3D { Content = new DirectionalLight { Direction = new Vector3D(-0.612372, 0.5, -0.612372) } });

            InitCubeCreation();
            InitCameraRotation();
            InitGridLines();
            InitMousePositionIndicator();

            InitUpdateEvent();
        }

        private void InitCubeCreation()
        {
            MouseDown += (_, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point3D point3D = NearestCubeFreePoint();

                    var model = Tools.CreateCube(point3D);

                    View.Children.Add(model);

                    Cubes.Add(new Cube { Position = point3D, Model = model });
                }
            };
        }

        private Point3D NearestCubeFreePoint()
        {
            var range = new LineRange();
            var isValid = ViewportInfo.Point2DtoPoint3D(View, Mouse.GetPosition(View), out range);

            Func<Point3D, string> str = p => $"({p.X.ToString("0.##")}, {p.Y.ToString("0.##")}, {p.Z.ToString("0.##")})";

            Title = $"{str(range.Point1)} => {str(range.Point2)}";

            Point3D point3D = range.PointFromZ(0);

            point3D.X = Math.Floor(point3D.X);
            point3D.Y = Math.Floor(point3D.Y);

            for (var i = Math.Floor(range.Point2.Z); i <= Math.Floor(range.Point1.Z) + 1; i++)
            {
                var p = range.PointFromZ(i);

                p.X = Math.Floor(p.X);
                p.Y = Math.Floor(p.Y);

                var p2 = new Point3D(p.X, p.Y, p.Z);

                if (Cubes.Any(c => c.Position.Equals(p2)))
                {
                    point3D = p;

                    point3D.Z++;
                }
            }

            return point3D;
        }

        private void InitCameraRotation()
        {
            MouseDown += (_, e) =>
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    var mousePosition = Mouse.GetPosition(null);
                    CameraOldMouseX = (int)mousePosition.X;
                    CameraOldMouseY = (int)mousePosition.Y;
                }
            };
            MouseMove += (_, e) =>
            {
                if(e.RightButton == MouseButtonState.Pressed)
                {
                    var mousePosition = Mouse.GetPosition(null);

                    CameraX += (int)mousePosition.X - CameraOldMouseX;
                    CameraY += (int)mousePosition.Y - CameraOldMouseY;

                    CameraOldMouseX = (int)mousePosition.X;
                    CameraOldMouseY = (int)mousePosition.Y;
                }
            };
            Update += (_, __) =>
            {
                View.Camera.Transform = new Transform3DGroup()
                {
                    Children = {
                        new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), CameraY)),
                        new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), CameraX)),
                    }
                };
            };
        }

        private void InitUpdateEvent()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1.0 / 60);
            timer.Tick += (_, __) => Update?.Invoke(null, null);
            timer.Start();
        }

        private void InitMousePositionIndicator()
        {
            for (var i = 0; i < 4; i++)
            {
                var line = new Line { Stroke = Brushes.Black };
                MousePositionIndicatorLines.Add(line);
                CanvasFront.Children.Add(line);
            }

            MouseMove += UpdateMousePositionIndicator;
            Update += UpdateMousePositionIndicator;
        }

        private void UpdateMousePositionIndicator(object sender, EventArgs e)
        {
            var point3D = NearestCubeFreePoint();

            var nw = ViewportInfo.Point3DtoPoint2D(View, new Point3D(point3D.X, point3D.Y, point3D.Z));
            var ne = ViewportInfo.Point3DtoPoint2D(View, new Point3D(point3D.X + 1, point3D.Y, point3D.Z));
            var se = ViewportInfo.Point3DtoPoint2D(View, new Point3D(point3D.X + 1, point3D.Y + 1, point3D.Z));
            var sw = ViewportInfo.Point3DtoPoint2D(View, new Point3D(point3D.X, point3D.Y + 1, point3D.Z));

            MousePositionIndicatorLines[0].X1 = nw.X;
            MousePositionIndicatorLines[0].Y1 = nw.Y;
            MousePositionIndicatorLines[0].X2 = ne.X;
            MousePositionIndicatorLines[0].Y2 = ne.Y;

            MousePositionIndicatorLines[1].X1 = ne.X;
            MousePositionIndicatorLines[1].Y1 = ne.Y;
            MousePositionIndicatorLines[1].X2 = se.X;
            MousePositionIndicatorLines[1].Y2 = se.Y;

            MousePositionIndicatorLines[2].X1 = se.X;
            MousePositionIndicatorLines[2].Y1 = se.Y;
            MousePositionIndicatorLines[2].X2 = sw.X;
            MousePositionIndicatorLines[2].Y2 = sw.Y;

            MousePositionIndicatorLines[3].X1 = sw.X;
            MousePositionIndicatorLines[3].Y1 = sw.Y;
            MousePositionIndicatorLines[3].X2 = nw.X;
            MousePositionIndicatorLines[3].Y2 = nw.Y;
        }

        private void InitGridLines()
        {
            var count = 20;
            for (var i = 0; i < count; i++)
            {
                var line = new Line { Stroke = Brushes.LightGray };

                VerticalLines.Add(line);
                CanvasBack.Children.Add(line);
            }

            for (var i = 0; i < count; i++)
            {
                var line = new Line { Stroke = Brushes.LightGray };

                HorizontalLines.Add(line);
                CanvasBack.Children.Add(line);
            }

            Update += (_, __) =>
            {
                Viewport3DVisual vpv = VisualTreeHelper.GetParent(View.Children[0]) as Viewport3DVisual;
                bool canTransformCoordinates;
                Matrix3D m = MathUtils.TryWorldToViewportTransform(vpv, out canTransformCoordinates);

                if (!canTransformCoordinates)
                {
                    return;
                }

                for (var i = 0; i < VerticalLines.Count; i++)
                {
                    var line = VerticalLines[i];

                    var p1 = m.Transform(new Point3D(i - VerticalLines.Count / 2, -10, 0));
                    var p2 = m.Transform(new Point3D(i - VerticalLines.Count / 2, +10, 0));

                    line.X1 = p1.X;
                    line.Y1 = p1.Y;
                    line.X2 = p2.X;
                    line.Y2 = p2.Y;
                }

                for (var i = 0; i < VerticalLines.Count; i++)
                {
                    var line = HorizontalLines[i];

                    var p1 = m.Transform(new Point3D(-10, i - HorizontalLines.Count / 2, 0));
                    var p2 = m.Transform(new Point3D(10, i - HorizontalLines.Count / 2, 0));

                    line.X1 = p1.X;
                    line.Y1 = p1.Y;
                    line.X2 = p2.X;
                    line.Y2 = p2.Y;
                }
            };
        }
    }
}
