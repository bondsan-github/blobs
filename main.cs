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
    public class vertex
    {
        public PointF begin = new PointF( 0.0f , 0.0f );
        public PointF end = new PointF( 0.0f , 0.0f );
        public PointF delta = new PointF( 0.0f , 0.0f );
        public PointF transition = new PointF( 0.0f , 0.0f );

        public bool expanding  = true;

        public vertex() { }

        public vertex( PointF in_being , PointF in_end , PointF in_delta )
        {
            begin = in_being;
            end   = in_end;
            delta = in_delta;
        }
    }

    public class blob
    {
        private int          quadrant_amount = 10;

        private List<vertex> vertices = new List<vertex>();
        //private List<PointF> vertices_start;
        //private List<PointF> vertices_end;
        //private List<PointF> vertices_delta;

        public float radius { get; }

        private Point        center;
        private float        tension = 0.0f;
        private Pen          pen_blue;
        private float        pen_width = 3.0f;
        private Random       random;
        //private float        variance = 40.0f;
        private int          variance = 40;
        private float        slope_divisions = 60;

        public blob( float in_radius )
        {
            radius   = in_radius;
            random   = new Random();
            pen_blue = new Pen( Color.Blue , pen_width );
        }

        public void set_center( Point in_center ) { center = in_center; }

        public void initialise()
        {
            float pi_double = 3.141592653F * 2F;// full circle in radians

            float theta = pi_double / quadrant_amount; // quadrant angle

            // for each quadrant of a circle
            for ( float circumference_point = 0 ; circumference_point <= pi_double + theta ; circumference_point += theta )
            //for ( float circumference_point = 0 ; circumference_point < pi_double + theta ; circumference_point += theta )
            {
                // generate circle points
                float start_x = center.X + ( radius * (float)Math.Cos(circumference_point) );
                float start_y = center.Y + ( radius * (float)Math.Sin(circumference_point) );

                // add random distance to begin and end vertices
                vertex new_vertex = new vertex();

                new_vertex.begin.X = start_x;// += ( float )random.Next( -variance , variance );
                new_vertex.begin.Y = start_y;// += ( float )random.Next( -variance , variance );

                float radius_random = radius + 50;// ( float )random.Next( -variance , variance );
                new_vertex.end.X = center.X + ( radius_random * (float)Math.Cos(circumference_point) );
                new_vertex.end.Y = center.Y + ( radius_random * (float)Math.Sin(circumference_point) );

                // movement vertex starts at begin vertex
                new_vertex.transition = new_vertex.begin;

                vertices.Add( new_vertex );
            }

            // last vertex = first vertex to ensure a closed loop
            //vertices[ vertices.Count - 1 ] = vertices[ 0 ];

            // calculate the vertex delta begin and end X,Y 
            for ( int index = 0 ; index < vertices.Count ; index++ )
            {
                PointF new_point = new PointF();

                //new_point.X = ( vertices[ index ].begin.X - vertices[ index ].end.X ) / slope_divisions;
                //new_point.Y = ( vertices[ index ].begin.Y - vertices[ index ].end.Y ) / slope_divisions;
                new_point.Y = ( vertices[ index ].end.Y - vertices[ index ].begin.Y ) / slope_divisions;
                new_point.X = ( vertices[ index ].end.X - vertices[ index ].begin.X ) / slope_divisions;

                vertices[ index ].delta = new_point;
            }
        }

        public void paint( object sender , PaintEventArgs event_paint )
        {
            // copy movement vertex to a new list of PointF's for the DrawCurve function
            List<PointF> transition_copy = new List<PointF>();
            // vertices.Select( copy => new PointF(){ copy.transition } ).ToList();

            for ( int index = 0 ; index < vertices.Count() ; index++ )
            {
                transition_copy.Add( vertices[ index ].transition );
                //transition_copy.Add( vertices[ index ].begin );
            }

            event_paint.Graphics.DrawCurve( pen_blue , transition_copy.ToArray() , tension );
        }

        public void update()
        {

            // update transition vertices 
            for ( int index = 0 ; index < vertices.Count() ; index++ )
            {
                // to add += , >= , <= operators to PointF
                // modify transition vertex with delta
                vertices[ index ].transition.X += vertices[ index ].delta.X;
                vertices[ index ].transition.Y += vertices[ index ].delta.Y;

                // if adding delta is past end or start then invert delta
                if ( ( vertices[ index ].transition.X += vertices[ index ].delta.X ) <= vertices[ index ].begin.X ||
                     ( vertices[ index ].transition.Y += vertices[ index ].delta.Y ) <= vertices[ index ].begin.Y ||
                     ( vertices[ index ].transition.X += vertices[ index ].delta.X ) >= vertices[ index ].end.X ||
                     ( vertices[ index ].transition.Y += vertices[ index ].delta.Y ) >= vertices[ index ].end.Y )
                {
                    vertices[ index ].delta.X *= -1;
                    vertices[ index ].delta.Y *= -1;
                }
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
            //this.Paint += form_Paint;
            this.Load += form_Load;
            //this.Resize += form_Resize;
            // timer
            timer_1 = new Timer();
            timer_1.Interval = 16; // milli seconds
            timer_1.Tick += timer_1_tick;
            timer_1.Start();
            // blob
            blob1 = new blob( blob_radius );//, form_center );
            this.Paint += new System.Windows.Forms.PaintEventHandler( this.blob1.paint );

            // Gets a reference to the current BufferedGraphicsContext
            //graphics_buffered_context = BufferedGraphicsManager.Current;
            // Creates a BufferedGraphics instance associated with Form1, and with
            // dimensions the same size as the drawing surface of Form1.
            //graphics_buffer = graphics_buffered_context.Allocate( this.CreateGraphics() , this.DisplayRectangle );
        }

        private void timer_1_tick( object sender , EventArgs e )
        {
            blob1.update();

            this.Invalidate();
        }

        //private void form_Paint(object sender, PaintEventArgs e)

        public void form_Load( object sender , EventArgs event_args )
        {
            form_center.X = ( ClientSize.Width / 2 ) - ( int )blob1.radius / 2;
            form_center.Y = ( ClientSize.Height / 2 ) - ( int )blob1.radius / 2;

            blob1.set_center( form_center );
            blob1.initialise();
        }

        private void form_KeyDown( object sender , KeyEventArgs event_arguments )
        {
            if ( event_arguments.KeyCode == Keys.Escape )
            {
                Application.Exit();
            }
        }

        public static void Main( string[] args )
        {
            Application.Run( new form() );
        }
    }
} // end namespace