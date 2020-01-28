using System;
using System.Collections.Generic;
using System.Windows;
using System.Reflection;

namespace KinectoPhone.Desktop
{
    internal static class Utilities
    {

        // TO GET RADIANS, MULTIPLY BY toRadians. TO REVERSE (AND GET DEGREES) DIVIDE BY toRadians.
        public const double toRadians = Math.PI / 180;

        public static double distanceBetween2Points(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        // USED FOR PULLING OUT PIXEL SHADERS AND ELEMENTS FILED UNDER COMPONENT
        public static Uri GetPackURI(string relativeFile)
        {
            string uriString = "pack://application:,,,/" + AssemblyShortName + ";component/" + relativeFile;
            return new Uri(uriString);
        }

        public static Uri GetPhotoFromResources(string relativePhotoPath)
        {
            return new Uri(@"pack://application:,,,/" + relativePhotoPath, UriKind.RelativeOrAbsolute);
        }

        private static string AssemblyShortName
        {
            get
            {
                if (_assemblyShortName == null)
                {
                    Assembly a = typeof(Utilities).Assembly;

                    // Pull out the short name.
                    _assemblyShortName = a.ToString().Split(',')[0];
                }

                return _assemblyShortName;
            }
        }
        private static string _assemblyShortName;

    }
}
