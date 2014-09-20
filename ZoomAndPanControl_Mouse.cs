using System;
using System.Windows;
using System.Windows.Input;

namespace ZoomAndPan
{
    /// <summary>
    /// A class that wraps up zooming and panning of it's content.
    /// </summary>
    public partial class ZoomAndPanControl
    {
        public delegate double DZoomScale(ZoomAndPanControl control);
        public event DZoomScale ZoomScale;
        public event DZoomScale ZoomInScale;
        public event DZoomScale ZoomOutScale;

        /// <summary>
        /// Specifies the current state of the mouse handling logic.
        /// </summary>
        private MouseHandlingMode mouseHandlingMode = MouseHandlingMode.None;

        /// <summary>
        /// The point that was clicked relative to the ZoomAndPanControl.
        /// </summary>
        private Point origZoomAndPanControlMouseDownPoint;

        /// <summary>
        /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
        /// </summary>
        private Point origContentMouseDownPoint;

        /// <summary>
        /// Records which mouse button clicked during mouse dragging.
        /// </summary>
        private MouseButton mouseButtonDown;

        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            content.Focus();
            Keyboard.Focus(content);

            mouseButtonDown = e.ChangedButton;
            origZoomAndPanControlMouseDownPoint = e.GetPosition(this);
            origContentMouseDownPoint = e.GetPosition(content);

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
                (e.ChangedButton == MouseButton.Left ||
                 e.ChangedButton == MouseButton.Right))
            {
                // Shift + left- or right-down initiates zooming mode.
                mouseHandlingMode = MouseHandlingMode.Zooming;
            }
            else if (mouseButtonDown == MouseButton.Left)
            {
                // Just a plain old left-down initiates panning mode.
                mouseHandlingMode = MouseHandlingMode.Panning;
            }

            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                // Capture the mouse so that we eventually receive the mouse up event.
                this.CaptureMouse();
                e.Handled = true;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                if (mouseHandlingMode == MouseHandlingMode.Zooming)
                {
                    if (mouseButtonDown == MouseButton.Left)
                    {
                        // Shift + left-click zooms in on the content.
                        ZoomIn(e);
                    }
                    else if (mouseButtonDown == MouseButton.Right)
                    {
                        // Shift + left-click zooms out from the content.
                        ZoomOut(e);
                    }
                }

                this.ReleaseMouseCapture();
                mouseHandlingMode = MouseHandlingMode.None;
                e.Handled = true;
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mouseHandlingMode == MouseHandlingMode.Panning)
            {
                //
                // The user is left-dragging the mouse.
                // Pan the viewport by the appropriate amount.
                //
                Point curContentMousePoint = e.GetPosition(content);
                Vector dragOffset = curContentMousePoint - origContentMouseDownPoint;

                this.ContentOffsetX -= dragOffset.X;
                this.ContentOffsetY -= dragOffset.Y;

                e.Handled = true;
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (e.Delta > 0)
            {
                ZoomIn(e);
            }
            else if (e.Delta < 0)
            {
                ZoomOut(e);
            }
            base.OnMouseWheel(e);
        }

        /// <summary>
        /// Zoom the viewport out, centering on the specified point (in content coordinates).
        /// </summary>
        protected virtual void ZoomOut(Point contentZoomCenter)
        {
            double scale = 0.1;
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                scale = 0.01;

            // Make changes less noticeable than the great leaps it otherwise would be
            if (ContentScale < ContentScaleNormal)
                scale /= 5;
            else if (ContentScale < ContentScaleNormal * 1.25)
                scale /= 2;

            if (null != ZoomOutScale)
            {
                var Event = ZoomOutScale;
                scale = Event(this);
            }
            else if (null != ZoomScale)
            {
                var Event = ZoomScale;
                scale = Event(this);
            }
            if (scale <= 0)
                throw new ArgumentOutOfRangeException("Scale cannot be zero or less for zooming");

            ZoomAboutPoint(this.ContentScale - scale, contentZoomCenter);
        }

        /// <summary>
        /// Zoom the viewport in, centering on the specified point (in content coordinates).
        /// </summary>
        protected virtual void ZoomIn(Point contentZoomCenter)
        {
            double scale = 0.1;
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                scale = 0.01;

            // Make changes less noticeable than the great leaps it otherwise would be
            if (ContentScale < ContentScaleNormal)
                scale /= 5;
            else if (ContentScale < ContentScaleNormal * 1.25)
                scale /= 2;

            if (null != ZoomInScale)
            {
                var Event = ZoomInScale;
                scale = Event(this);
            }
            else if (null != ZoomScale)
            {
                var Event = ZoomScale;
                scale = Event(this);
            }
            if (scale <= 0)
                throw new ArgumentOutOfRangeException("Scale cannot be zero or less for zooming");

            ZoomAboutPoint(this.ContentScale + scale, contentZoomCenter);
        }

        /// <summary>
        /// Zoom the viewport out, centering on the mouse location (in content coordinates).
        /// </summary>
        protected virtual void ZoomOut(MouseEventArgs e)
        {
            ZoomOut(e.GetPosition(content));
        }

        /// <summary>
        /// Zoom the viewport in, centering on the mouse location (in content coordinates).
        /// </summary>
        protected virtual void ZoomIn(MouseEventArgs e)
        {
            ZoomIn(e.GetPosition(content));
        }
    }
}
