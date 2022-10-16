using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

class Constants
{
    public static double Radical_2 = Math.Sqrt(2) * 3 / 2;
}

namespace Linii
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Width = 1200;
            this.Height = 800;
            this.MaximizeBox = true;
        }
        
        private GeomBody_3D Generate_Parallelepiped()
        {
            int[,] conectionsVisual =
                { { 0, 1, 1, 0, 0, 0, 0, 1 },
                  { 1, 0, 0, 1, 0, 0, 1, 0 },
                  { 1, 0, 0, 1, 1, 0, 0, 0 },
                  { 0, 1, 1, 0, 0, 1, 0, 0 },
                  { 0, 0, 1, 0, 0, 1, 0, 1 },
                  { 0, 0, 0, 1, 1, 0, 1, 0 },
                  { 0, 1, 0, 0, 0, 1, 0, 1 },
                  { 1, 0, 0, 0, 1, 0, 1, 0 }
                };
            //0 -> 1, 2, 7
            //1 -> 3, 6
            //2 -> 3, 4...
            Dictionary<int, List<int>> conections_ = new Dictionary<int, List<int>>();
            for (int contor_1 = 0; contor_1 < 8; contor_1++)
            {
                conections_.Add(contor_1, new List<int>());
                for (int contor_2 = 0; contor_2 < 8; contor_2++)
                    if (1 == conectionsVisual[contor_1, contor_2]) 
                        conections_[contor_1].Add(contor_2);
            }

            bool[,] conections = new bool[8, 8];
            for (int contor_1 = 0; contor_1 < 8; contor_1++)
                for (int contor_2 = 0; contor_2 < 8; contor_2++)
                    conections[contor_1,contor_2]= (1 == conectionsVisual[contor_1,contor_2]);
            int[,] cornerDirection =
                {   { -1, -1, -1},
                    { +1, -1, -1},
                    { -1, +1, -1},
                    { +1, +1, -1},
                    { -1, +1, +1},
                    { +1, +1, +1},
                    { +1, -1, +1},
                    { -1, -1, +1}
                };
            int length = 180;
            int height = 200;
            int depth = 250;
            int[] sidesLengths = { length, height, depth };

            List<Point_3D> cornersList = new List<Point_3D>();
            for (int Contor_i = 0; Contor_i < 8; Contor_i++)
            {
                Point_3D corner = new Point_3D();
                corner.X = cornerDirection[Contor_i, 0] * sidesLengths[0] / 2;
                corner.Y = cornerDirection[Contor_i, 1] * sidesLengths[1] / 2;
                corner.Z = cornerDirection[Contor_i, 2] * sidesLengths[2] / 2;
                cornersList.Add(corner);
            }
            return new GeomBody_3D(cornersList, conections);
        }

        private void Draw_GeometricBody(GeomBody_3D body)
        {
            BufferedGraphicsContext currentContext;
            BufferedGraphics myBuffer;
            currentContext = BufferedGraphicsManager.Current;
            myBuffer = currentContext.Allocate(CreateGraphics(), DisplayRectangle);
            myBuffer.Graphics.Clear(Color.White);
            int contor_1 = 0;
            foreach (List<int> face in body.FaceList)
            {
                byte faceColor = body.faceColor(contor_1);
                contor_1++;
                SolidBrush myBrush = new SolidBrush(Color.FromArgb(255, faceColor, faceColor, faceColor));
                Point[] polygon = new Point[face.Count()];
                int contor_2 = 0;
                foreach (int verticeID in face)
                {
                    polygon[contor_2].X = body.VericesList.ElementAt(verticeID).X;
                    polygon[contor_2].Y = body.VericesList.ElementAt(verticeID).Y;
                    contor_2++;
                }                
                myBuffer.Graphics.FillPolygon(myBrush, polygon);

            }
            
            myBuffer.Render(this.CreateGraphics());
            Thread.Sleep(10);
            myBuffer.Dispose();
        }

        
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            const int gravityCenter_X = 600;
            const int gravityCenter_Y = 400;
            const int maxAngle = 360;
            const int numberOfRotations = 2;
            GeomBody_3D initialBody = Generate_Parallelepiped();
            for (int localContor = 0; localContor < (maxAngle * numberOfRotations + 1); localContor ++)
            {
                int rotationOn_X_Axis = localContor;
                int rotationOn_Y_Axis = localContor;
                int rotationOn_Z_Axis = 0;
                GeomBody_3D theBody = initialBody.DeepCopy();
                theBody.RotateBody(rotationOn_X_Axis, rotationOn_Y_Axis, rotationOn_Z_Axis);
                theBody.PerspectiveBody(2000);
                var t = theBody.TheSeedFaceID();
                theBody.SortBodysFacesList();
                theBody.ProjectOnDisplay(gravityCenter_X, gravityCenter_Y);
                Draw_GeometricBody(theBody);
            }
            
        }
    }
}
