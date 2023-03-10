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
        private int          quadrant_amount = 20;
        
        private List<PointF> vertices_start;
        //public  List<PointF> Vertices_Start { get; set; }
        
        private List<PointF> vertices_end;

        private List<PointF> vertices_slope;

        public  float        radius { get; }

        private Point        center;
        private float        tension = 1.0f;
        private Pen          pen_blue;
        private float        pen_width = 1.0f;
        private Random       random;
        private float        variance = 40.0f;

        private float        slope_divisions = 60;

        public blob( float in_radius )//, Point in_center )
        {
            //center   = in_center;
            radius   = in_radius;
            random   = new Random();
            pen_blue = new Pen( Color.Blue , pen_width );

            vertices_start = new List<PointF>();
            vertices_end   = new List<PointF>();
            vertices_slope = new List<PointF>();
        }

        public void set_center( Point in_center ) { center = in_center; }

        public void initialise()
        {
            float pi_double = 3.141592653F * 2F;// full circle in radians

            float theta = pi_double / quadrant_amount; // quadrant angle

            for ( float circumference_point = 0; circumference_point <= pi_double + theta; circumference_point += theta )
            {
                float x = center.X + ( radius * (float)Math.Cos(circumference_point) );
                float y = center.Y + ( radius * (float)Math.Sin(circumference_point) );

                PointF new_vertex_start = new PointF();
                PointF new_vertex_end = new PointF();
                
                new_vertex_start.X = x += ( float )random.NextDouble() * variance;
                new_vertex_start.Y = y += ( float )random.NextDouble() * variance;

                new_vertex_end.X = x += (float)random.NextDouble() * variance;
                new_vertex_end.Y = y += (float)random.NextDouble() * variance;

                vertices_start.Add(new_vertex_start);
                vertices_end.Add(new_vertex_end);
            }

            // last vertex = first vertex
            vertices_start[ vertices_start.Count - 1 ] = vertices_start[ 0 ];
            vertices_end  [ vertices_end.Count - 1   ] = vertices_end[ 0 ];

            for ( int index = 0 ; index < vertices_start.Count ; index++ )
            {
                PointF new_point = new PointF();
                
                new_point.X = ( vertices_start[ index ].X - vertices_end[ index ].X ) / slope_divisions;
                new_point.Y = ( vertices_start[ index ].Y - vertices_end[ index ].Y ) / slope_divisions;

                vertices_slope.Add( new_point );
            }
        }

        public void paint( object sender, PaintEventArgs event_paint )
        {
            event_paint.Graphics.DrawCurve( pen_blue , vertices_start.ToArray() , tension );
        }

        public void update()
        {
            for ( int index = 0 ; index < vertices_start.Count ; index++ )
            {
                PointF new_point = vertices_start[index];

                new_point.X += vertices_slope[ index ].X;
                new_point.Y += vertices_slope[ index ].Y;

                vertices_start[ index ] = new_point;
            }
        }

        public void recenter( PointF new_center )
        {
            //foreach ( var vertex in Vertices_Start )
        }
    }

    class form : Form
    {
        Point form_center;
        Timer timer_1;
        BufferedGraphicsContext graphics_buffered_context;
        BufferedGraphics graphics_buffer;

        float blob_radius = 150.0f;
        blob blob1;

        public form()
        {
            // form state
            this.WindowState = FormWindowState.Maximized;
            ResizeRedraw = true;
            DoubleBuffered = true;
            // event handlers
            this.KeyDown += form_KeyDown;
            this.Paint += form_Paint;
            this.Load += form_Load;
            //this.Resize += form_Resize;
            // timer
            timer_1 = new Timer();
            timer_1.Interval = 16;
            timer_1.Tick += timer_1_tick;
            timer_1.Start();
            // blob
            
            //form_center.X = ( ClientSize.Width / 2 ) - ( int )blob_radius / 2;
            //form_center.Y = ( ClientSize.Height / 2 ) - ( int )blob_radius / 2;

            blob1 = new blob( blob_radius );//, form_center );
            this.Paint += new System.Windows.Forms.PaintEventHandler( this.blob1.paint );

            // Gets a reference to the current BufferedGraphicsContext
            graphics_buffered_context = BufferedGraphicsManager.Current;
            // Creates a BufferedGraphics instance associated with Form1, and with
            // dimensions the same size as the drawing surface of Form1.
            graphics_buffer = graphics_buffered_context.Allocate( this.CreateGraphics() , this.DisplayRectangle );
        }


        private void timer_1_tick(object sender, EventArgs e)
        {
            blob1.update();
        }

        private void form_Paint(object sender, PaintEventArgs e)
        {
           
        }

        public void form_Load(object sender, EventArgs event_args)
        {
            form_center.X = ( ClientSize.Width / 2 ) - ( int )blob1.radius / 2;
            form_center.Y = ( ClientSize.Height / 2 ) - ( int )blob1.radius / 2;

            blob1.set_center( form_center );
            blob1.initialise();
        }

        private void form_KeyDown( object sender, KeyEventArgs event_arguments ) 
        {
            if( event_arguments.KeyCode == Keys.Escape )
            {
                Application.Exit();
            }
        }

        public static void Main( string[] args )
        {
            Application.Run( new form() );
        }
    }

    //InitializeComponent();
}
