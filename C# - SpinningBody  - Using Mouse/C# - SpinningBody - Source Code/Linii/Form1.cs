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
        int m_mouseX = 0;
        int m_mouseY = 0;
        int m_mouseStartX = 0;
        int m_mouseStartY = 0;
        bool m_mousePressed = false;
        object m_sender;
        PaintEventArgs m_e;
        int m_rotationOn_X_AxisStart = 20;
        int m_rotationOn_Y_AxisStart = 20;
        int m_rotationOn_X_Axis = 0;
        int m_rotationOn_Y_Axis = 0;

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
            m_sender = sender;
            m_e = e;
            const int gravityCenter_X = 600;
            const int gravityCenter_Y = 400;
            const int maxAngle = 360;
            const int numberOfRotations = 3;
            GeomBody_3D initialBody = Generate_Parallelepiped();
            float X_axis = 180 * ((m_mouseStartY - m_mouseY) / 761);
            float Y_axis = 180 * ((m_mouseX - m_mouseStartX) / 984);
            m_rotationOn_X_Axis = (m_rotationOn_X_AxisStart + (m_mouseY - m_mouseStartY) / 5) % 180;
            m_rotationOn_Y_Axis = (m_rotationOn_Y_AxisStart + (m_mouseX - m_mouseStartX) / 5) % 180;
            int rotationOn_Z_Axis = 0;
            GeomBody_3D theBody = initialBody.DeepCopy();
            theBody.RotateBody(m_rotationOn_X_Axis, m_rotationOn_Y_Axis, rotationOn_Z_Axis);
            theBody.PerspectiveBody(2000);
            var t = theBody.TheSeedFaceID();
            theBody.SortBodysFacesList();
            theBody.ProjectOnDisplay(gravityCenter_X, gravityCenter_Y);
            Draw_GeometricBody(theBody);
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {

            m_mouseStartX = e.X;
            m_mouseStartY = e.Y;
            m_mouseX = e.X;
            m_mouseY = e.Y;
            m_mousePressed = true;
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            m_mousePressed = false;
            m_mouseStartX = e.X;
            m_mouseStartY = e.Y;
            m_rotationOn_X_AxisStart = m_rotationOn_X_Axis;
            m_rotationOn_Y_AxisStart = m_rotationOn_Y_Axis;
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_mousePressed)
            {
                m_mouseX = e.X;
                m_mouseY = e.Y;
            }
            Form1_Paint(m_sender, m_e);
        }
    }
}
