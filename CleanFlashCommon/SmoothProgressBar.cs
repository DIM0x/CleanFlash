using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CleanFlashCommon {
    public class SmoothProgressBar : UserControl {
        int min = 0;
        int max = 100;
        int val = 0;
        Color Color1 = Color.Black;
        Color Color2 = Color.White;

        protected override void OnResize(EventArgs e) {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            using (Graphics graphics = e.Graphics) {
                using (Brush brush = new LinearGradientBrush(ClientRectangle, Color1, Color2, 0.0F)) {
                    float percent = (val - min) / (float)(max - min);
                    Rectangle rect = ClientRectangle;

                    // Calculate area for drawing the progress.
                    rect.Width = (int)(rect.Width * percent);

                    // Draw the progress meter.
                    graphics.FillRectangle(brush, rect);
                }

                // Draw a three-dimensional border around the control.
                Draw3DBorder(graphics);
            }
        }

        public int Minimum {
            get {
                return min;
            }

            set {
                min = Math.Max(0, Math.Min(max, value));

                if (val < min) {
                    val = min;
                }

                Invalidate();
            }
        }

        public int Maximum {
            get {
                return max;
            }

            set {
                if (value < min) {
                    min = value;
                }

                max = value;

                if (val > max) {
                    val = max;
                }

                Invalidate();
            }
        }

        public int Value {
            get {
                return val;
            }

            set {
                int oldValue = val;

                // Make sure that the value does not stray outside the valid range.
                if (value < min) {
                    val = min;
                } else if (value > max) {
                    val = max;
                } else {
                    val = value;
                }

                // Invalidate only the changed area.
                float percent;

                Rectangle newValueRect = ClientRectangle;
                Rectangle oldValueRect = ClientRectangle;

                // Use a new value to calculate the rectangle for progress.
                percent = (val - min) / (float)(max - min);
                newValueRect.Width = (int)(newValueRect.Width * percent);

                // Use an old value to calculate the rectangle for progress.
                percent = (oldValue - min) / (float)(max - min);
                oldValueRect.Width = (int)(oldValueRect.Width * percent);

                Rectangle updateRect = new Rectangle();

                // Find only the part of the screen that must be updated.
                if (newValueRect.Width > oldValueRect.Width) {
                    updateRect.X = oldValueRect.Size.Width;
                    updateRect.Width = newValueRect.Width - oldValueRect.Width;
                } else {
                    updateRect.X = newValueRect.Size.Width;
                    updateRect.Width = oldValueRect.Width - newValueRect.Width;
                }

                updateRect.Height = Height;

                // Invalidate the intersection region only.
                Invalidate(updateRect);
            }
        }

        public Color ProgressBarColor1 {
            get {
                return Color1;
            }

            set {
                Color1 = value;

                // Invalidate the control to get a repaint.
                Invalidate();
            }
        }

        public Color ProgressBarColor2 {
            get {
                return Color2;
            }

            set {
                Color2 = value;

                // Invalidate the control to get a repaint.
                Invalidate();
            }
        }

        private void Draw3DBorder(Graphics g) {
            int PenWidth = (int)Pens.White.Width;

            g.DrawLine(Pens.DarkGray,
            new Point(ClientRectangle.Left, ClientRectangle.Top),
            new Point(ClientRectangle.Width - PenWidth, ClientRectangle.Top));
            g.DrawLine(Pens.DarkGray,
            new Point(ClientRectangle.Left, ClientRectangle.Top),
            new Point(ClientRectangle.Left, ClientRectangle.Height - PenWidth));
            g.DrawLine(Pens.White,
            new Point(ClientRectangle.Left, ClientRectangle.Height - PenWidth),
            new Point(ClientRectangle.Width - PenWidth, ClientRectangle.Height - PenWidth));
            g.DrawLine(Pens.White,
            new Point(ClientRectangle.Width - PenWidth, ClientRectangle.Top),
            new Point(ClientRectangle.Width - PenWidth, ClientRectangle.Height - PenWidth));
        }
    }
}
