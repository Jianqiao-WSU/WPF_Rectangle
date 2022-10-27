using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point startPoint;
        private Rectangle? rect;
        bool drag = false;
        // The drag's last point.
        private Point LastPoint;
        private enum HitType
        {
            None, Body, UL, UR, LR, LL, L, R, T, B
        };

        // The part of the rectangle under the mouse.
        HitType MouseHitType = HitType.None;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e) // Upload image
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All Images Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
            if (op.ShowDialog() == true)
            {
                img.Source = new BitmapImage(new Uri(op.FileName));
                // Set the new img width
                img.Width = img.Source.Width / img.Source.Height * img.Height;
                // Set the height and width of canvas
                canvas.Height = img.Height;
                canvas.Width = img.Width;
            }

        }
        private void Canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) // Draw rectangle
        {
            Point point = Mouse.GetPosition(canvas);
            startPoint = e.GetPosition(canvas);
            rect = new Rectangle
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.LightBlue
            };
            rect.MouseDown += rectangle_MouseDown;
            rect.MouseMove += rectangle_MouseMove;
            rect.MouseUp += rectangle_MouseUp;
            Canvas.SetLeft(rect, startPoint.X);
            Canvas.SetTop(rect, startPoint.Y);
            
            // Whether the point is in the rectangles
            bool isInside = false;
            foreach (Rectangle rects in canvas.Children.OfType<Rectangle>())
            {
                isInside = IsInsideRectangle(rects, point);
                if (isInside) break;
            }
            if (!isInside)
            {
                canvas.Children.Add(rect);
            }
            else
            {
                isInside = true;
            }
        }
        public bool IsInsideRectangle(UIElement element, Point pos)
        {
            double top = Canvas.GetTop(element);
            double left = Canvas.GetLeft(element);
            double right = left + (element as Rectangle).Width;
            double bottom = top + (element as Rectangle).Height;
            return pos.X > left && pos.X < right && pos.Y > top && pos.Y < bottom;
        }
        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) // Drag mouse to drag rectangle
        {
            if (e.LeftButton == MouseButtonState.Released || rect == null)
                return;

            if (!drag)
            {
                var pos = e.GetPosition(canvas);
                var x = Math.Min(pos.X, startPoint.X);
                var y = Math.Min(pos.Y, startPoint.Y);
                var width = Math.Max(pos.X, startPoint.X) - x;
                var height = Math.Max(pos.Y, startPoint.Y) - y;
                rect.Width = width;
                rect.Height = height;
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
            }
        }
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rect = null;
        }
        private void rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // start dragging
            drag = true;
            // save start point of dragging
            /*Cursor = Cursors.SizeAll;*/
            /*startPoint = Mouse.GetPosition(canvas);*/
            Rectangle rect = sender as Rectangle;
            MouseHitType = SetHitType(rect, Mouse.GetPosition(canvas));
            SetMouseCursor();
            if (MouseHitType == HitType.None) return;

            LastPoint = Mouse.GetPosition(canvas);
        }
        private void rectangle_MouseMove(object sender, MouseEventArgs e) // drag the rectangle
        {
            /*// if dragging, then adjust rectangle position based on mouse movement
            if (drag)
            {
                Rectangle draggedRectangle = sender as Rectangle;
                Point newPoint = Mouse.GetPosition(canvas);
                double left = Canvas.GetLeft(draggedRectangle);
                double top = Canvas.GetTop(draggedRectangle);
                Canvas.SetLeft(draggedRectangle, left + (newPoint.X - startPoint.X));
                Canvas.SetTop(draggedRectangle, top + (newPoint.Y - startPoint.Y));

                startPoint = newPoint;
            }*/
            Rectangle rect = sender as Rectangle;
            /*Trace.WriteLine(rect.Width);*/
            if (!drag)
            {
                MouseHitType = SetHitType(rect, Mouse.GetPosition(canvas));
                SetMouseCursor();
            }
            else
            {
                // See how much the mouse has moved.
                Point point = Mouse.GetPosition(canvas);
                double offset_x = point.X - LastPoint.X;
                double offset_y = point.Y - LastPoint.Y;

                // Get the rectangle's current position.
                double new_x = Canvas.GetLeft(rect);
                double new_y = Canvas.GetTop(rect);
                double new_width = rect.Width;
                double new_height = rect.Height;

                // Update the rectangle.
                switch (MouseHitType)
                {
                    case HitType.Body:
                        new_x += offset_x;
                        new_y += offset_y;
                        break;
                    case HitType.UL:
                        new_x += offset_x;
                        new_y += offset_y;
                        new_width -= offset_x;
                        new_height -= offset_y;
                        break;
                    case HitType.UR:
                        new_y += offset_y;
                        new_width += offset_x;
                        new_height -= offset_y;
                        break;
                    case HitType.LR:
                        new_width += offset_x;
                        new_height += offset_y;
                        break;
                    case HitType.LL:
                        new_x += offset_x;
                        new_width -= offset_x;
                        new_height += offset_y;
                        break;
                    case HitType.L:
                        new_x += offset_x;
                        new_width -= offset_x;
                        break;
                    case HitType.R:
                        new_width += offset_x;
                        break;
                    case HitType.B:
                        new_height += offset_y;
                        break;
                    case HitType.T:
                        new_y += offset_y;
                        new_height -= offset_y;
                        break;
                }

                // Don't use negative width or height.
                if ((new_width > 0) && (new_height > 0))
                {
                    // Update the rectangle.
                    Canvas.SetLeft(rect, new_x);
                    Canvas.SetTop(rect, new_y);
                    rect.Width = new_width;
                    rect.Height = new_height;

                    // Save the mouse's new location.
                    LastPoint = point;
                }
            }
        }

        private void rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // stop dragging
            drag = false;
        }
        // Return a HitType value to indicate what is at the point.
        private HitType SetHitType(object sender, Point point)
        {
            Rectangle rect = sender as Rectangle;
            double left = Canvas.GetLeft(rect);
            double top = Canvas.GetTop(rect);
            double right = left + rect.Width;
            double bottom = top + rect.Height;
            if (point.X < left) {
                return HitType.None; 
            }
            if (point.X > right) return HitType.None;
            if (point.Y < top) return HitType.None;
            if (point.Y > bottom) return HitType.None;

            const double GAP = 10;
            if (point.X - left < GAP)
            {
                // Left edge.
                if (point.Y - top < GAP) return HitType.UL;
                if (bottom - point.Y < GAP) return HitType.LL;
                return HitType.L;
            }
            if (right - point.X < GAP)
            {
                // Right edge.
                if (point.Y - top < GAP) return HitType.UR;
                if (bottom - point.Y < GAP) return HitType.LR;
                return HitType.R;
            }
            if (point.Y - top < GAP) return HitType.T;
            if (bottom - point.Y < GAP) return HitType.B;
            return HitType.Body;
        }

        // Set a mouse cursor appropriate for the current hit type.
        private void SetMouseCursor()
        {
            // See what cursor we should display.
            Cursor desired_cursor = Cursors.Arrow;
            switch (MouseHitType)
            {
                case HitType.None:
                    desired_cursor = Cursors.Arrow;
                    break;
                case HitType.Body:
                    desired_cursor = Cursors.ScrollAll;
                    break;
                case HitType.UL:
                case HitType.LR:
                    desired_cursor = Cursors.SizeNWSE;
                    break;
                case HitType.LL:
                case HitType.UR:
                    desired_cursor = Cursors.SizeNESW;
                    break;
                case HitType.T:
                case HitType.B:
                    desired_cursor = Cursors.SizeNS;
                    break;
                case HitType.L:
                case HitType.R:
                    desired_cursor = Cursors.SizeWE;
                    break;
            }
            // Display the desired cursor.
            if (Cursor != desired_cursor) Cursor = desired_cursor;
        }
    }
}
