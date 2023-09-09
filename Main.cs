﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Commodore_Retro_Toolbox
{
    public partial class Main : Form
    {

        private Image image;
        private float zoomFactor = 1.0f;
        private Point lastMousePosition;
        private TransparentPanel panelOverlay1;
        private CustomPanel panelMain;
        private Panel panelImage;
        private PictureBox overlayPictureBox1;
        private Bitmap bmp1;
        private Size originalOverlaySize;
        private Point originalOverlayLocation;

        public Main()
        {
            InitializeComponent();

            // Initialize main panel with custom settings
            panelMain = new CustomPanel
            {
                Size = new Size(500, 350),
                Location = new Point(0, 0),
                AutoScroll = true,
                Dock = DockStyle.Fill,
            };
            tabPage1.Controls.Add(panelMain);

            // Load an image and initialize image panel
            image = Image.FromFile(Application.StartupPath + "\\Data\\Schematics.gif");
            panelImage = new Panel
            {
                Size = image.Size,
                BackgroundImage = image,
                BackgroundImageLayout = ImageLayout.Zoom,
                Dock = DockStyle.None,
            };
            panelMain.Controls.Add(panelImage);

            // Initialize overlay PictureBox and store its original dimensions
            overlayPictureBox1 = new PictureBox
            {
                Size = new Size(250, 410),
                Location = new Point(1226, 1672),
                BackColor = Color.Transparent,
            };
            panelImage.Controls.Add(overlayPictureBox1);
            originalOverlaySize = overlayPictureBox1.Size;
            originalOverlayLocation = overlayPictureBox1.Location;

            
           

            // Create and set a bitmap for the overlay PictureBox
            bmp1 = new Bitmap(overlayPictureBox1.Width, overlayPictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bmp1))
            {
                g.Clear(Color.FromArgb(128, Color.Red)); // 50% opacity
            }
            overlayPictureBox1.Image = bmp1;

            // Attach event handlers for mouse events and form shown
            panelMain.CustomMouseWheel += new MouseEventHandler(PanelMain_MouseWheel);
            panelImage.MouseDown += PanelImage_MouseDown;
            panelImage.MouseUp += PanelImage_MouseUp;
            panelImage.MouseMove += PanelImage_MouseMove;
            this.Shown += new EventHandler(this.Main_Shown);

            overlayPictureBox1.MouseDown += PanelImage_MouseDown;
            overlayPictureBox1.MouseUp += PanelImage_MouseUp;
            overlayPictureBox1.MouseMove += PanelImage_MouseMove;

            // Enable double buffering for smoother updates
            panelMain.DoubleBuffered(true);
            panelImage.DoubleBuffered(true);
        }

        private void FitImageToPanel()
        {
            float xZoomFactor = (float)panelMain.Width / image.Width;
            float yZoomFactor = (float)panelMain.Height / image.Height;
            zoomFactor = Math.Min(xZoomFactor, yZoomFactor);

            panelImage.Size = new Size((int)(image.Width * zoomFactor), (int)(image.Height * zoomFactor));

            if (overlayPictureBox1 != null)
            {
                int newWidth = (int)(originalOverlaySize.Width * zoomFactor);
                int newHeight = (int)(originalOverlaySize.Height * zoomFactor);
                overlayPictureBox1.Size = new Size(newWidth, newHeight);
                overlayPictureBox1.Location = new Point((int)(originalOverlayLocation.X * zoomFactor), (int)(originalOverlayLocation.Y * zoomFactor));

                if (overlayPictureBox1.Image != null)
                {
                    overlayPictureBox1.Image.Dispose();
                }
                Bitmap newBmp = new Bitmap(newWidth, newHeight);
                using (Graphics g = Graphics.FromImage(newBmp))
                {
                    g.Clear(Color.FromArgb(128, Color.Red)); // 50% opacity
                }
                overlayPictureBox1.Image = newBmp;
            }
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            //            panelMain.AutoScrollPosition = new Point(750, 400);FitImageToPanel();
            FitImageToPanel();
        }

        private void PanelMain_MouseWheel(object sender, MouseEventArgs e)
        {
//            panelMain.SuspendLayout();
//            panelImage.SuspendLayout();
//            overlayPictureBox1.SuspendLayout();

            Debug.WriteLine("MouseWheel event");
            float oldZoomFactor = zoomFactor;

            Debug.WriteLine("Before: panelMain.Width="+ panelMain.Width+", panelImage.Width="+panelImage.Width+", image.Width=" + image.Width+ ", panelMain.AutoScrollPosition.X="+ panelMain.AutoScrollPosition.X);

            Debug.WriteLine("zoomFactor="+ zoomFactor);

            // Change the zoom factor based on the mouse wheel movement.
            bool hasZoomChanged = false;
            if (e.Delta > 0)
            {
                if (zoomFactor <= 5) {
                    Debug.WriteLine("Zoom In");
                    zoomFactor *= 1.5f;
                    hasZoomChanged = true;
                }
            }
            else 
            {
                if (panelImage.Width > panelMain.Width || panelImage.Height > panelMain.Height)
                {
                    Debug.WriteLine("Zoom Out");
                    zoomFactor /= 1.5f;
                    hasZoomChanged = true;
                }
            }

            if (hasZoomChanged)
            {
                // Calculate the new size of the imagePanel.
                Size newSize = new Size((int)(image.Width * zoomFactor), (int)(image.Height * zoomFactor));

                // Calculate the current mouse position relative to the content in the containerPanel.
                Point mousePosition = new Point(e.X - panelMain.AutoScrollPosition.X, e.Y - panelMain.AutoScrollPosition.Y);

                // Calculate what the new scroll position should be so that the content under the mouse stays under the mouse.
                Point newScrollPosition = new Point(
                    (int)(mousePosition.X * (zoomFactor / oldZoomFactor)),
                    (int)(mousePosition.Y * (zoomFactor / oldZoomFactor))
                );

                // Update the size of the imagePanel.
                panelImage.Size = newSize;

                // Update the scroll position of the containerPanel.
                panelMain.AutoScrollPosition = new Point(newScrollPosition.X - e.X, newScrollPosition.Y - e.Y);

                if (overlayPictureBox1 != null)
                {
                    int newWidth = (int)(originalOverlaySize.Width * zoomFactor);
                    int newHeight = (int)(originalOverlaySize.Height * zoomFactor);

                    overlayPictureBox1.Size = new Size(newWidth, newHeight);
                    overlayPictureBox1.Location = new Point((int)(originalOverlayLocation.X * zoomFactor), (int)(originalOverlayLocation.Y * zoomFactor));

                    // Dispose of the old bitmap
                    if (overlayPictureBox1.Image != null)
                    {
                        overlayPictureBox1.Image.Dispose();
                    }

                    // Create a new bitmap with the new dimensions
                    Bitmap newBmp = new Bitmap(newWidth, newHeight);

                    // Perform drawing operations here, if any
                    using (Graphics g = Graphics.FromImage(newBmp))
                    {
                        g.Clear(Color.FromArgb(128, Color.Red)); // 50% opacity
                    }

                    // Set the new bitmap
                    overlayPictureBox1.Image = newBmp;
                }

                Debug.WriteLine("After: panelMain.Width=" + panelMain.Width + ", panelImage.Width=" + panelImage.Width + ", image.Width=" + image.Width + ", panelMain.AutoScrollPosition.X=" + panelMain.AutoScrollPosition.X);

//                panelMain.ResumeLayout();
//                panelImage.ResumeLayout();
//                overlayPictureBox1.ResumeLayout();
            }
        }

        private void PanelImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Debug.WriteLine("MouseDown event");
                lastMousePosition = e.Location;
            }
        }

        private void PanelImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Debug.WriteLine("MouseMove event");
                int dx = e.X - lastMousePosition.X;
                int dy = e.Y - lastMousePosition.Y;

                panelMain.AutoScrollPosition = new Point(-panelMain.AutoScrollPosition.X - dx, -panelMain.AutoScrollPosition.Y - dy);
            }
        }

        private void PanelImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Debug.WriteLine("MouseUp event");
                lastMousePosition = Point.Empty;
            }
        }


    }

    public class CustomPanel : Panel
    {
        public event MouseEventHandler CustomMouseWheel;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            CustomMouseWheel?.Invoke(this, e);
        }
    }

}