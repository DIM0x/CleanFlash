using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CleanFlashCommon {
    public class GradientButton : Button {
        public Color Color1 { get; set; }
        public Color Color2 { get; set; }
        public double HoverAlpha { get; set; }
        public double DisableAlpha { get; set; }

        private bool Hovered = false;

        public GradientButton() {
            Color1 = Color.Black;
            Color2 = Color.White;
            HoverAlpha = 0.875;
            DisableAlpha = 0.644;
        }

        protected override void OnMouseDown(MouseEventArgs mevent) {
            Hovered = false;
            base.OnMouseDown(mevent);
            Refresh();
        }

        protected override void OnMouseUp(MouseEventArgs mevent) {
            Hovered = true;
            base.OnMouseUp(mevent);
            Refresh();
        }

        protected override void OnMouseEnter(EventArgs e) {
            Hovered = true;
            base.OnMouseEnter(e);
            Refresh();
        }

        protected override void OnMouseLeave(EventArgs e) {
            Hovered = false;
            base.OnMouseLeave(e);
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e) {
            Color c1 = Color1;
            Color c2 = Color2;
            Color c3 = BackColor;
            Color c4 = ForeColor;

            if (!Enabled) {
                c1 = Color.FromArgb(255, (int)(c1.R * DisableAlpha), (int)(c1.G * DisableAlpha), (int)(c1.B * DisableAlpha));
                c2 = Color.FromArgb(255, (int)(c2.R * DisableAlpha), (int)(c2.G * DisableAlpha), (int)(c2.B * DisableAlpha));
                c3 = Color.FromArgb(255, (int)(c3.R * DisableAlpha), (int)(c3.G * DisableAlpha), (int)(c3.B * DisableAlpha));
                c4 = Color.FromArgb(255, (int)(c4.R * DisableAlpha), (int)(c4.G * DisableAlpha), (int)(c4.B * DisableAlpha));
            } else if (!Hovered) {
                c1 = Color.FromArgb(255, (int)(c1.R * HoverAlpha), (int)(c1.G * HoverAlpha), (int)(c1.B * HoverAlpha));
                c2 = Color.FromArgb(255, (int)(c2.R * HoverAlpha), (int)(c2.G * HoverAlpha), (int)(c2.B * HoverAlpha));
            }

            SizeF size = e.Graphics.MeasureString(Text, Font);

            using (Brush brush = new LinearGradientBrush(ClientRectangle, c1, c2, 90.0F)) {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            int thickness = 1;
            int halfThickness = thickness / 2;

            using (Pen pen = new Pen(c3, thickness)) {
                e.Graphics.DrawRectangle(
                    pen, new Rectangle(
                        halfThickness, halfThickness,
                        ClientRectangle.Width - thickness, ClientRectangle.Height - thickness
                    )
                );
            }

            Point point = new Point(
                (ClientRectangle.Width - (int)size.Width) / 2,
                (ClientRectangle.Height - (int)size.Height) / 2
            );

            using (Brush brush = new SolidBrush(c4)) {
                e.Graphics.DrawString(Text, Font, new SolidBrush(c3), new Point(point.X + 1, point.Y + 1));
                e.Graphics.DrawString(Text, Font, brush, point);
            }
        }
    }
}
