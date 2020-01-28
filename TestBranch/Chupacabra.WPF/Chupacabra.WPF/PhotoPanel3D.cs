using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using _3DTools;

namespace KinectoPhone.Desktop
{
    class PhotoPanel3D : InteractiveVisual3D
    {

        #region Positioning Logic (x,y,z)

        public double X
        {
            get { return Transform.Value.OffsetX; }
            set
            {
                Matrix3D matrix = Transform.Value;
                matrix.OffsetX = value;
                Transform = new MatrixTransform3D(matrix);
            }
        }

        public double Y
        {
            get { return Transform.Value.OffsetY; }
            set
            {
                Matrix3D matrix = Transform.Value;
                matrix.OffsetY = value;
                Transform = new MatrixTransform3D(matrix);
            }
        }

        public double Z
        {
            get { return Transform.Value.OffsetZ; }
            set
            {
                Matrix3D matrix = Transform.Value;
                matrix.OffsetZ = value;
                Transform = new MatrixTransform3D(matrix);
            }
        }
        #endregion

        public PhotoPanel3D()
        {
            Geometry = getSurfaceSquare();
            IsBackVisible = true;
            Transform = new MatrixTransform3D(Matrix3D.Identity);
        }

        public void setBaseImage(string _photoPath)
        {
            PanelCover cover = new PanelCover();
            cover.ImageFile = Utilities.GetPhotoFromResources(_photoPath);
            Visual = (Visual)cover;
        }

        public double Opacity
        {
            get { return (double)(Visual as Image).Opacity; }
            set { (Visual as Image).Opacity = value; }
        }

        // THIS SHOULD PROBABLY GET MOVED TO A STATIC 3D BULDER FILE
        internal MeshGeometry3D getSurfaceSquare()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(0, 1, 0));
            mesh.Positions.Add(new Point3D(0, 0, 0));
            mesh.Positions.Add(new Point3D(1, 0, 0));
            mesh.Positions.Add(new Point3D(1, 1, 0));
            mesh.TriangleIndices = new Int32Collection(new int[] { 0, 1, 2, 0, 2, 3 });
            mesh.TextureCoordinates.Add(new Point(0, 0));
            mesh.TextureCoordinates.Add(new Point(0, 1));
            mesh.TextureCoordinates.Add(new Point(1, 1));
            mesh.TextureCoordinates.Add(new Point(1, 0));
            mesh.Freeze();
            return mesh;
        }

    
    }
}
