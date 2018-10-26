using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Assign4
{
    public partial class Form1 : Form
    {
        Graphics g;
        int x = -1;
        int y = -1;
        bool move = false;
        Pen mypen = new Pen(Color.Black, 1); //default pen color
        Pen eraser = new Pen(Color.White, 5); //default eraser, being white
        Point current = new Point();
        Point old = new Point();
        int w;
        //SolidBrush mybrush = new SolidBrush(Color.Black);
        Pen mybrush = new Pen(Color.Black,5);
        float width = 3;
        int xd = -1, yd = -1, xU = -1, yU = -1;
        bool brush = false;
        bool myeraser = false;
        bool drawline = false;
        //for undo,redo
        List<Action<Graphics>> actionsUndo = new List<Action<Graphics>>();
        List<Action<Graphics>> actionsRedo = new List<Action<Graphics>>();
        Stack<Action<Graphics>> undo = new Stack<Action<Graphics>>();
        Stack<Action<Graphics>> redo = new Stack<Action<Graphics>>();
        /*Stack<Image> undo = new Stack<Image>();
        Stack<Image> redo = new Stack<Image>();*/
        public Form1()
        {
            InitializeComponent();
            g = pCanvas.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;   //to make the lines much more smoother.
            eraser.SetLineCap(System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.DashCap.Round);
            mybrush.SetLineCap(System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.DashCap.Round);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox34_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "PNG Files|*.png";
            if (o.ShowDialog() == DialogResult.OK)
            {
                pCanvas.BackgroundImage = (Image)Image.FromFile(o.FileName).Clone();
                //pCanvas.BackgroundImageLayout = ImageLayout.Zoom;
            }
        }

        private void pbNew_Click(object sender, EventArgs e)
        {
            if (pCanvas.Controls != null)
            {
                DialogResult dr = MessageBox.Show("Do you want to save changes?","Paint", MessageBoxButtons.OKCancel);
                if (dr == DialogResult.OK)
                {
                    pbSave_Click(sender, e);
                    pCanvas.Refresh();
                    pCanvas.BackgroundImage = null;
                }
                else if (dr == DialogResult.Cancel)
                {
                    pCanvas.Refresh();
                    pCanvas.BackgroundImage = null;
                }
            }
        }

        private void pbSave_Click(object sender, EventArgs e)
        {
            bool flag = true;
            Bitmap bm = new Bitmap(pCanvas.Width, pCanvas.Height);
            Graphics g = Graphics.FromImage(bm);
            Rectangle rec = pCanvas.RectangleToScreen(pCanvas.ClientRectangle);
            g.CopyFromScreen(rec.Location, Point.Empty, pCanvas.Size);
            g.Dispose();

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG Files|*.png";
            sfd.FileName = "Untitled";
            sfd.RestoreDirectory = true;
            if (sfd.FileName != "")
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs;
                    if (!File.Exists(sfd.FileName))
                    {
                        fs = (FileStream)sfd.OpenFile();
                        bm.Save(fs, ImageFormat.Png);
                    }
                    else
                    {
                        sfd.OverwritePrompt = true;
                        fs = (FileStream)sfd.OpenFile();
                        bm.Save(fs, ImageFormat.Png);
                    }
                    fs.Close();
                }
            }
            /*mypanel.Save(s, ImageFormat.Png);
            s += i;
            i++;*/
        }

        private void pbSaveAs_Click(object sender, EventArgs e)
        {
            Bitmap bm = new Bitmap(pCanvas.Width, pCanvas.Height);
            Graphics g = Graphics.FromImage(bm);
            Rectangle rec = pCanvas.RectangleToScreen(pCanvas.ClientRectangle);
            g.CopyFromScreen(rec.Location, Point.Empty, pCanvas.Size);
            g.Dispose();

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG Files|*.png";
            sfd.FileName = "Untitled";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(sfd.FileName))
                {
                    sfd.FileName = sfd.FileName;
                    MessageBox.Show("Please choose a location to save your file.");
                    sfd.OverwritePrompt = true;
                    bm.Save(sfd.FileName, ImageFormat.Png);
                }

            }
        }

        /*Event name: pictureBox1_Click
         * Summary: triggered when picture box is clicked to choose a color
         */
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            mypen.Color = p.BackColor;
            mypen.Width = (float)nudPencilSize.Value;
            mybrush.Color = p.BackColor;
            eraser.Color = p.BackColor;
        }

        /*Event name: pCanvas_MouseDown
         * Summary: triggered when mouse is clicked on the canvas
         */
        private void pCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            old = e.Location;
            w = 5;
            eraser.Width = w;
            mypen.Width = (float)nudPencilSize.Value;
            move = true;
            x = e.X;
            y = e.Y;
            if (drawline == true)
            {
                xd = e.X;
                yd = e.Y;
            }
            if (brush == true && myeraser==false && drawline==false)
                mybrush.Width = (float)nudBrushSize.Value;
        }

        /*Event name: pCanvas_MouseMove
         * Summary: triggered when mouse moves on the canvas
         */
        private void pCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (move == true && x != -1 && y != -1 && brush == false && myeraser==false && drawline==false)
            {
                Console.WriteLine("Pencil");
                g.DrawLine(mypen, new Point(x, y), e.Location);
                actionsUndo.Add(g=>g.DrawLine(mypen, new Point(x, y), e.Location));
                //graph.DrawLine(mypen, new Point(x, y), e.Location);
                x = e.X;
                y = e.Y;
            }
            else if (brush == true &x!=-1 && y!=-1 && myeraser==false && drawline==false)
            {
                Console.WriteLine("Brush");
                current = e.Location;
                g.DrawLine(mybrush, old, current);
                actionsUndo.Add(g => g.DrawLine(mybrush, old, current));
                //g.DrawLine(mybrush, old, current);
                old = current;
                /*mybrush.Width = (float)nudBrushSize.Value;
                g.DrawLine(new Pen(mybrush.Color), new Point(x, y), e.Location);
                //g.FillEllipse(new SolidBrush(mybrush.Color), x,y, (float)nudBrushSize.Value, (float)nudBrushSize.Value);
                x = e.X;
                y = e.Y;*/
            }
            else if(myeraser==true & x!=-1 && y!=-1 && move==true)
            {
                Console.WriteLine("eraser in mouse");
                //g.FillRectangle(new SolidBrush(eraser.Color), x, y, width,width);
                if(e.Button==MouseButtons.Left)
                {
                    Console.WriteLine(eraser.Color);
                    current = e.Location;
                    g.DrawLine(eraser, old, current);
                    actionsUndo.Add(g => g.DrawLine(eraser, old, current));
                    //graph.DrawLine(eraser, old, current);
                    old = current;
                }
            }
        }

        /*Event name: pCanvas_MouseUp
         * Summary: triggered when mouse up
         */
        private void pCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            move = false;
            x = -1;
            y = -1;
            if(drawline==true)
            {
                xU = e.X;
                yU = e.Y;
                g.DrawLine(new Pen(mypen.Color), xd, yd, xU, yU);
                //graph.DrawLine(new Pen(mypen.Color), xd, yd, xU, yU);
            }
        }

        /*Event name: pCanvas_Paint
         * Summary: triggered when cursor enters area of the cursor
         */
        private void pCanvas_Paint(object sender, PaintEventArgs e)
        {
            mypen.Color = Color.Black;
            mypen.Width = (float)nudPencilSize.Value;
            mybrush.Color = Color.Black;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        /*Event name: btnCdb_Click
         * Summary: triggered when color dialog box button is clicked
         */
        private void btnCdb_Click(object sender, EventArgs e)   
        {
            ColorDialog cd = new ColorDialog();
            cd.ShowDialog();
            mypen.Width= (float)nudPencilSize.Value;
            mypen.Color = cd.Color;
            mybrush.Color = cd.Color;
        }

        public static Cursor CreateCursor(Bitmap bm, Size sz)
        {
            bm = new Bitmap(bm, sz);
            bm.MakeTransparent();               //making the background of the bitmap image used to make a cursor transparent
            return new Cursor(bm.GetHicon());
        }
        private void pictureBox29_Click(object sender, EventArgs e)    //event to change the cursor to a pencil
        {
            pCanvas.Cursor = CreateCursor((Bitmap)imageList1.Images[0], new Size(16,18));
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void btnDraw_Click(object sender, EventArgs e)
        {
            Pen d = new Pen(Color.Blue, 5);
            g.DrawLine(d, 45, 65, 120, 120);
            undo.Push(g=>g.DrawLine(d, 45,65, 120, 120));
            g.DrawLine(d, 95, 105, 220, 320);
            undo.Push(g => g.DrawLine(d, 95, 105, 220, 320));
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Console.WriteLine(undo.Count);
            redo.Push(undo.Pop());
            //pCanvas.Refresh();
            Console.WriteLine(redo.Count);
            undo.Pop();
        }

        private void pictureBox30_Click(object sender, EventArgs e)     //event to change the cursor to a brush
        {
            pCanvas.Cursor = CreateCursor((Bitmap)imageList1.Images[1], new Size(16, 18));
            brush = true;
        }

        private void pictureBox31_Click(object sender, EventArgs e)
        {
            pCanvas.Cursor = CreateCursor((Bitmap)imageList1.Images[2], new Size(16, 18));
            eraser.Color = Color.White;
            myeraser = true;
        }

        private void pictureBox32_Click(object sender, EventArgs e)
        {
            drawline = true;
        }
    }
}
