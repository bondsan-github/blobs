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
        //public float gradient;
        //public float delta_x;
        public PointF delta = new PointF( 0.0f , 0.0f );
        public PointF transition = new PointF( 0.0f , 0.0f );

        public bool expanding  = true;

        public vertex() { }

        public vertex( PointF in_being , PointF in_end )// , float in_delta_x )
        {
            begin = in_being;
            end   = in_end;
            //delta_x = in_delta_x;
        }
    }

    public class blob
    {
        private int          quadrant_amount = 20;

        private List<vertex> vertices = new List<vertex>();
        //private List<PointF> vertices_start;
        //private List<PointF> vertices_end;
        //private List<PointF> vertices_delta;

        public float radius { get; }

        private Point        center;
        private float        tension = 1.0f;
        private Pen          pen_blue;
        private float        pen_width = 3.0f;
        private Random       random;
        //private float        variance = 40.0f;
        private int          variance = 20;
        private float        line_divisions = 60;

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
                vertex new_vertex = new vertex();

                // generate begin circle points
                float radius_random = radius + ( float )random.Next( -variance , variance );

                new_vertex.begin.X = center.X + ( radius_random * (float)Math.Cos(circumference_point) );
                new_vertex.begin.Y = center.Y + ( radius_random * (float)Math.Sin(circumference_point) );

                // generate end circle points
                radius_random = radius + ( float )random.Next( -variance , variance );

                new_vertex.end.X = center.X + ( radius_random * (float)Math.Cos(circumference_point) );
                new_vertex.end.Y = center.Y + ( radius_random * (float)Math.Sin(circumference_point) );

                // movement vertex starts at begin vertex
                new_vertex.transition = new_vertex.begin;

                vertices.Add( new_vertex );
            }

            // last vertex = first vertex to ensure a closed loop
            //vertices[ vertices.Count - 1 ] = vertices[ 0 ];

            // calculate the vertex x and y delta between begin and end 
            for ( int index = 0 ; index < vertices.Count ; index++ )
            {
                // line gradient
                //float gradient = ( vertices[index].end.Y - vertices[index].begin.Y) / ( vertices[index].end.X - vertices[index].begin.X );

                // distance between begin and end
                //double x = Math.Pow( vertices[index].end.X - vertices[index].begin.X , 2.0 );
                //double y = Math.Pow( vertices[index].end.Y - vertices[index].begin.Y , 2.0 );

                //double new_distance = Math.Sqrt( x + y );

                //float new_delta_x = ( vertices[ index ].end.X - vertices[ index ].begin.X ) / line_divisions;

                PointF new_delta = new PointF( 0.0f, 0.0f );

                new_delta.X = ( vertices[ index ].end.X - vertices[ index ].begin.X ) / line_divisions;
                new_delta.Y = ( vertices[ index ].end.Y - vertices[ index ].begin.Y ) / line_divisions;

                //vertices[ index ].delta_x = new_delta_x;
                //vertices[ index ].gradient = gradient;
                
                vertices[ index ].delta = new_delta;
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

            event_paint.Graphics.DrawClosedCurve( pen_blue , transition_copy.ToArray() , tension , System.Drawing.Drawing2D.FillMode.Winding );
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

                //vertices[ index ].transition.X += vertices[ index ].delta_x;
                //vertices[ index ].transition.Y = vertices[ index ].transition.X * vertices[ index ].gradient;

                // if adding delta is past end or start then invert delta
                if( vertices[ index ].transition.X  > vertices[ index ].end.X ) vertices[ index ].delta.X *= -1;
                if( vertices[ index ].transition.X  < vertices[ index ].begin.X ) vertices[ index ].delta.X *= -1;
                
                if( vertices[ index ].transition.Y > vertices[ index ].end.Y ) vertices[ index ].delta.Y *= -1;
                if( vertices[ index ].transition.Y < vertices[ index ].begin.Y ) vertices[ index ].delta.Y *= -1;
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