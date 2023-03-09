using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace blobs
{
    public class blob
    {
        private float arc_amount = 100;
        private float arc_length;
        private List<PointF> verticies = new List<PointF>();
        private float radius;
        private Point center;

        public blob( Point in_center , float in_radius )
        {
            center = in_center;
            radius = in_radius;

            float pi_double = 3.141592653F * 2F;

            float theta = pi_double / arc_amount;
            arc_length = ( theta / pi_double ) * pi_double * radius;

            for( double circumference = 0; circumference < 2 * Math.PI; circumference += arc_length)
            {
                float x = center.X + radius * (float)Math.Cos(theta);
                float y = center.Y + radius * (float)Math.Sin(theta);

                verticies.Add( new PointF( x, y ) );
            }
        }

        public void paint(object sender, PaintEventArgs event_paint)
        {
            //center.X = (size_client.Width / 2) - (int)radius / 2;
            //center.Y = (size_client.Height / 2) - (int)radius / 2;

            Rectangle position = new Rectangle(center, new Size((int)radius, (int)radius));

            event_paint.Graphics.FillEllipse(Brushes.Blue, position);
        }
    }

    class application : Form
    {
        Point form_center;
        Timer timer_50ms;

        blob blob1;

        public application()
        {
            this.WindowState = FormWindowState.Maximized;
            this.KeyDown += application_KeyDown;
            this.Paint += application_Paint;

            timer_50ms = new Timer();
            timer_50ms.Interval = 50;
            timer_50ms.Tick += timer_50ms_tick;
            timer_50ms.Start();
            
            ResizeRedraw = true;

            float blob_size = 50F;

            form_center.X = ( ClientSize.Width  / 2 ) - (int)blob_size / 2;
            form_center.Y = ( ClientSize.Height / 2 ) - (int)blob_size / 2;

            blob1 = new blob( form_center, blob_size );

            this.Paint += new System.Windows.Forms.PaintEventHandler(this.blob1.paint );
        }


        private void timer_50ms_tick(object sender, EventArgs e)
        {
        }

        private void application_Paint(object sender, PaintEventArgs e)
        {
        }


        private void application_KeyDown( object sender, KeyEventArgs event_arguments ) 
        {
            if( event_arguments.KeyCode == Keys.Escape )
            {
                Application.Exit();
            }
        }

        public static void Main( string[] args )
        {
            Application.Run( new application() );
        }
    }

    //InitializeComponent();
}
