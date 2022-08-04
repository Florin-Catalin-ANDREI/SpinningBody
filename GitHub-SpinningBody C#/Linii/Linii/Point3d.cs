using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace Linii
{
    //**************************************************************************************
    //                                                                                     *
    //                         POINT_3D      -     CLASS                                   *
    //                                                                                     *
    //**************************************************************************************
    public class Point_3D
    {
        //********************************************************
        //                 DECLARATIONS
        //********************************************************
        private int _x;
        private int _y;
        private int _z;
        public int X { get { return _x; } set { _x = value; } }
        public int Y { get { return _y; } set { _y = value; } }
        public int Z { get { return _z; } set { _z = value; } }
        public Point_3D() { }
        public Point_3D(int x, int y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        //********************************************************
        //                 PUBLIC METHODS
        //********************************************************
        public void ApplyRotationOnThreeAxis(int rotation_X, int rotation_Y, int rotation_Z)
        {
            static void ApplyRotationOnOneAx(int rotationAngle, ref int x_Axis, ref int y_Axis)
            {
                int distance_R = (int)Math.Round(Math.Sqrt(x_Axis * x_Axis + y_Axis * y_Axis));
                int tethaAngle = 0;
                if (x_Axis < 0) tethaAngle = 180 + (int)Math.Round(Math.Atan((double)y_Axis / x_Axis) * (180 / Math.PI));
                if ((x_Axis > 0) && (y_Axis >= 0)) tethaAngle = (int)Math.Round(Math.Atan((double)y_Axis / x_Axis) * ((double)180 / Math.PI));
                if ((x_Axis > 0) && (y_Axis < 0)) tethaAngle = 360 + (int)Math.Round(Math.Atan((double)y_Axis / x_Axis) * ((double)180 / Math.PI));
                if ((x_Axis == 0) && (y_Axis >= 0)) tethaAngle = 90;
                if ((x_Axis == 0) && (y_Axis <= 0)) tethaAngle = 270;
                tethaAngle += rotationAngle % 360;
                x_Axis = (int)Math.Round(distance_R * Math.Cos(tethaAngle * (Math.PI / 180)));
                y_Axis = (int)Math.Round(distance_R * Math.Sin(tethaAngle * (Math.PI / 180)));
            }
            ApplyRotationOnOneAx(rotation_X, ref _y, ref _z);
            ApplyRotationOnOneAx(rotation_Y, ref _x, ref _z);
            ApplyRotationOnOneAx(rotation_Z, ref _x, ref _y);
        }

        public void ApplayPerspectiveDistortion(int distanceToTheObservationPoint)
        {
            _x = (distanceToTheObservationPoint * _x) / (distanceToTheObservationPoint + _z);
            _y = (distanceToTheObservationPoint * _y) / (distanceToTheObservationPoint + _z);
        }

        public void ComputeCornerProjectionOnDisplay(int center_X, int center_Y)
        {
            _x = center_X + _x;
            _y = center_Y + _y;
        }
    }
    //**************************************************************************************
    //                                                                                     *
    //                       GEOMBODY_3D      -     CLASS                                  *
    //                                                                                     *
    //**************************************************************************************
    public class GeomBody_3D
    {
        //********************************************************
        //                 DECLARATIONS
        //********************************************************
        private List<Point_3D> _verticesList;
        private bool[,] _conectionMatrix;
        private List<List<int>> _facesList;

        public List<Point_3D> VericesList { get { return _verticesList; } }
        public List<List<int>> FaceList { get { return _facesList; } }
        public GeomBody_3D() { }

        public GeomBody_3D(List<Point_3D> listOfVortices, bool[,] conectionMatrix)
        {
            _verticesList = listOfVortices;
            _conectionMatrix = conectionMatrix;
            _facesList = ComputeFacesList();
        }
        public GeomBody_3D DeepCopy()
        {
            GeomBody_3D theCopy = (GeomBody_3D)this.MemberwiseClone();

            theCopy._verticesList = new List<Point_3D>();
            foreach (Point_3D point in _verticesList)
            {
                Point_3D copyPoint = new Point_3D(point.X, point.Y, point.Z);
                theCopy._verticesList.Add(copyPoint);
            }

            theCopy._conectionMatrix = (bool[,])_conectionMatrix.Clone();

            theCopy._facesList = new List<List<int>>(_facesList);

            return theCopy;
        }
        //********************************************************
        //                 LOCAL METHODS
        //********************************************************
        private List<List<int>> ComputeFacesList()
        {
            void FindNextVortex(int contor_1, int contor_2, int contor_3, ref List<int> faceVorticesList)
            {
                for (int contor_4 = contor_2 + 1; contor_4 < _verticesList.Count; contor_4++)
                    if ((contor_4 != contor_3) && _conectionMatrix[contor_2, contor_4] && BelongsToFace(contor_1, contor_2, contor_3, contor_4))
                    {
                        faceVorticesList.Add(contor_4);
                        FindNextVortex(contor_2, contor_3, contor_4, ref faceVorticesList);
                    }



            }
            bool BelongsToFace(int i, int j, int k, int m)
            {
                return 0 == (_verticesList[m].X - _verticesList[k].X) * ((_verticesList[i].Y - _verticesList[k].Y) * (_verticesList[j].Z - _verticesList[k].Z) - (_verticesList[j].Y - _verticesList[k].Y) * (_verticesList[i].Z - _verticesList[k].Z)) +
                            (_verticesList[m].Y - _verticesList[k].Y) * ((_verticesList[i].X - _verticesList[k].X) * (_verticesList[j].Z - _verticesList[k].Z) - (_verticesList[j].X - _verticesList[k].X) * (_verticesList[i].Z - _verticesList[k].Z)) +
                            (_verticesList[m].Z - _verticesList[k].Z) * ((_verticesList[i].X - _verticesList[k].X) * (_verticesList[j].Y - _verticesList[k].Y) - (_verticesList[j].X - _verticesList[k].X) * (_verticesList[i].Y - _verticesList[k].Y));
            }
            List<List<int>> faces = new List<List<int>>();
            for (int contor_1 = 0; contor_1 < (_verticesList.Count - 2); contor_1++)
                for (int contor_2 = contor_1 + 1; contor_2 < (_verticesList.Count - 1); contor_2++)
                    if (_conectionMatrix[contor_1, contor_2])
                        for (int contor_3 = contor_2 + 1; contor_3 < _verticesList.Count; contor_3++)
                            if (_conectionMatrix[contor_1, contor_3])
                            {
                                List<int> faceVorticesList = new List<int>();
                                faceVorticesList.Add(contor_2);
                                faceVorticesList.Add(contor_1);
                                faceVorticesList.Add(contor_3);
                                FindNextVortex(contor_1, contor_2, contor_3, ref faceVorticesList);
                                faceVorticesList.Add(faceVorticesList[0]);
                                faces.Add(faceVorticesList);
                            }
            return faces;
        }
        public int TheSeedFaceID()
        {
            int[] visibleVerticeID = new int[3];
            int z_min = int.MaxValue;
            for (int contor = 0; contor < _verticesList.Count(); contor++)
                if (_verticesList[contor].Z < z_min)
                {
                    visibleVerticeID[0] = contor;
                    z_min = _verticesList[contor].Z;
                }

            int max = int.MinValue;
            int maxID = -1;
            for (int contor = 0; contor < _verticesList.Count(); contor++)
            {
                if (_conectionMatrix[visibleVerticeID[0], contor])
                    if (_verticesList[contor].Z != _verticesList[visibleVerticeID[0]].Z)
                    {
                        double tempVar = (_verticesList[contor].X - _verticesList[visibleVerticeID[0]].X) / (_verticesList[contor].Z - _verticesList[visibleVerticeID[0]].Z);
                        int intersection_x = Math.Abs((int)(_verticesList[visibleVerticeID[0]].X - _verticesList[visibleVerticeID[0]].Z * tempVar));
                        if (intersection_x > max)
                        {
                            max = intersection_x;
                            maxID = contor;
                        }
                    }
                    else
                    {
                        maxID = contor;
                        contor = _verticesList.Count();
                    }
                var i = 0;
            }
            visibleVerticeID[1] = maxID;
            int[] visibleFaceID = new int[2];
            int contor_1 = 0;
            for (int contor = 0; contor < _facesList.Count(); contor++)
                if(_facesList[contor].Contains(visibleVerticeID[0]) && _facesList[contor].Contains(visibleVerticeID[1]))
                {
                    visibleFaceID[contor_1] = contor;
                    contor_1++;
                }
            var m = _facesList[visibleFaceID[0]].IndexOf(visibleVerticeID[0]);
            var n = _facesList[visibleFaceID[0]].IndexOf(visibleVerticeID[1]);
            var o = _facesList[visibleFaceID[1]].IndexOf(visibleVerticeID[0]);
            var p = _facesList[visibleFaceID[1]].IndexOf(visibleVerticeID[1]);

            return 0;
        }



        //********************************************************
        //                 PUBLIC METHODS
        //********************************************************

        public void SortBodysFacesList()
        {
            int[] sortingKey = new int[_facesList.Count];
            int[] sortingLink = new int[_facesList.Count];
            int contor_1 = 0;
            foreach (List<int> face in _facesList)
            {
                int max_Z = int.MinValue;
                int min_Z = int.MaxValue;
                foreach (int vertexId in face)
                {
                    if (_verticesList[vertexId].Z > max_Z) max_Z = _verticesList[vertexId].Z;
                    if (_verticesList[vertexId].Z < min_Z) min_Z = _verticesList[vertexId].Z;
                }
                sortingLink[contor_1] = contor_1;
                sortingKey[contor_1] = -100000 * max_Z - min_Z;
                contor_1++;
            }
            Array.Sort(sortingKey, sortingLink);
            List<List<int>> sortedBodysFacesLis = new List<List<int>>();
            for (int contor = 0; contor < _facesList.Count; contor++) sortedBodysFacesLis.Add(_facesList[sortingLink[contor]]);
            _facesList = sortedBodysFacesLis;
        }

        public void RotateBody(int rotationOn_X, int rotationOn_Y, int rotationOn_Z)
        {
            foreach (Point_3D vertice in _verticesList)
            {
                vertice.ApplyRotationOnThreeAxis(rotationOn_X, rotationOn_Y, rotationOn_Z);
            }
        }
        public void PerspectiveBody(int distanceToTheObservationPoint)
        {
            foreach (Point_3D vertice in _verticesList)
            {
                vertice.ApplayPerspectiveDistortion(distanceToTheObservationPoint);
            }
        }

        public void ProjectOnDisplay(int center_X, int center_Y)
        {
            foreach (Point_3D vertice in _verticesList)
            {
                vertice.ComputeCornerProjectionOnDisplay(center_X, center_Y);
            }
        }
        public byte faceColor(int faceIndex)
        {
            List<int> faceVertices = _facesList[faceIndex];
            long element_A = (_verticesList[faceVertices[0]].Y - _verticesList[faceVertices[2]].Y) * (_verticesList[faceVertices[1]].Z - _verticesList[faceVertices[2]].Z) - (_verticesList[faceVertices[1]].Y - _verticesList[faceVertices[2]].Y) * (_verticesList[faceVertices[0]].Z - _verticesList[faceVertices[2]].Z);
            long element_B = (_verticesList[faceVertices[0]].X - _verticesList[faceVertices[2]].X) * (_verticesList[faceVertices[1]].Z - _verticesList[faceVertices[2]].Z) - (_verticesList[faceVertices[1]].X - _verticesList[faceVertices[2]].X) * (_verticesList[faceVertices[0]].Z - _verticesList[faceVertices[2]].Z);
            long element_C = (_verticesList[faceVertices[0]].X - _verticesList[faceVertices[2]].X) * (_verticesList[faceVertices[1]].Y - _verticesList[faceVertices[2]].Y) - (_verticesList[faceVertices[1]].X - _verticesList[faceVertices[2]].X) * (_verticesList[faceVertices[0]].Y - _verticesList[faceVertices[2]].Y);
            return Convert.ToByte(225 - Math.Round((decimal)(int)Math.Round(Math.Acos(Math.Abs((double)(3 * element_A + element_B - 5 * element_C) / (Math.Sqrt(35) * Math.Sqrt(element_A * element_A + element_B * element_B + element_C * element_C)))) * (180 / Math.PI)) / 2));
        }

    }
    
}
