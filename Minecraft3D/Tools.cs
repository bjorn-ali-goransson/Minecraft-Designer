using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Minecraft3D
{
    public static class Tools
    {
        public static GeometryModel3D CreateStandingPlane(Point3D p1, Point3D p2)
        {
            return new GeometryModel3D
            {
                Geometry = new MeshGeometry3D
                {
                    Positions = { new Point3D(p1.X, p1.Y, p1.Z + 1), new Point3D(p1.X, p1.Y, p1.Z), new Point3D(p2.X, p2.Y, p2.Z), new Point3D(p2.X, p2.Y, p2.Z + 1) },
                    TriangleIndices = { 0, 2, 1, 0, 3, 2 },
                    TextureCoordinates = { new Point(1, 0), new Point(1, 1), new Point(0, 1), new Point(0, 0), },
                },
                Material = new DiffuseMaterial(new ImageBrush(Properties.Resources.tile_3.CreateBitmapSource())),
            };
        }

        public static GeometryModel3D CreateLyingPlane(Point3D position)
        {
            var x = position.X;
            var y = position.Y;
            var z = position.Z;

            return new GeometryModel3D
            {
                Geometry = new MeshGeometry3D
                {
                    Positions = { new Point3D(x, y, z), new Point3D(x, y + 1, z), new Point3D(x + 1, y + 1, z), new Point3D(x + 1, y, z) },
                    TriangleIndices = { 0, 2, 1, 0, 3, 2 },
                    TextureCoordinates = { new Point(1, 1), new Point(0, 1), new Point(0, 0), new Point(1, 0), },
                },
                Material = new DiffuseMaterial(new ImageBrush(Properties.Resources.tile_146.CreateBitmapSource())),
            };
        }

        public static ModelVisual3D CreateCube(Point3D position)
        {
            var x = position.X;
            var y = position.Y;
            var z = position.Z;

            var group = new Model3DGroup { Children = {
                CreateStandingPlane(new Point3D(x + 1, y + 0, z), new Point3D(x + 0, y + 0, z)),
                CreateStandingPlane(new Point3D(x + 0, y + 1, z), new Point3D(x + 1, y + 1, z)),
                CreateStandingPlane(new Point3D(x + 0, y + 0, z), new Point3D(x + 0, y + 1, z)),
                CreateStandingPlane(new Point3D(x + 1, y + 1, z), new Point3D(x + 1, y + 0, z)),
                CreateLyingPlane(new Point3D(x + 0, y + 0, z + 1)),
            } };

            return new ModelVisual3D { Content = group };
        }
    }
}
