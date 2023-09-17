﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Commodore_Retro_Toolbox;
using System.Linq;

namespace Commodore_Repair_Toolbox
{
        
    public partial class Main : Form
    {

        // UI elements
        private CustomPanel panelMain;
        private Panel panelImage;

        Dictionary<string, string> listBoxNameValueMapping = new Dictionary<string, string>();
        // List to hold the actual values of selected items
        List<string> listBoxSelectedActualValues = new List<string>();

        // Main variables
        private float zoomFactor = 1.0f;
        private Point lastMousePosition;
        private bool isResizedByMouseWheel = false;

//        private string hardwareSelected = "Commodore 64 (Breadbin)";
//        private string boardSelected = "250425";
//        private string imageSelected = "Schematics 1 of 2";
//        private string highlightTabColor = "";
//        private string nameTechnical = "";

        // Overlay array in "Main" tab
        List<PictureBox> overlayComponentsList = new List<PictureBox>(); 
        List<PictureBox> overlayComponentsTab = new List<PictureBox>();
        //Dictionary<string, Size> overlayComponentsTabOriginalSizes = new Dictionary<string, Size>();
        //Dictionary<string, Point> overlayComponentsTabOriginalLocations = new Dictionary<string, Point>();
        Dictionary<int, Size> overlayComponentsTabOriginalSizes = new Dictionary<int, Size>();
        Dictionary<int, Point> overlayComponentsTabOriginalLocations = new Dictionary<int, Point>();

        List<Hardware> classHardware = new List<Hardware>();

        private Image image = Image.FromFile(Application.StartupPath + "\\Data\\Commodore 64 Breadbin\\250425\\Schematics 1of2.gif");


        // ---------------------------------------------------------------------------------


        public Main()
        {
            InitializeComponent();

            DataStructure.GetAllData(classHardware);

            // Now you have a list of hardware, each containing a list of associated boards
            foreach (Hardware hardware in classHardware)
            {
                Debug.WriteLine("Hardware Name = " + hardware.Name + ", Folder = " + hardware.Folder);
                foreach (Board board in hardware.Boards)
                {
                    Debug.WriteLine("  Board Name = " + board.Name + ", Folder = " + board.Folder);
                    foreach (ComponentBoard component in board.Components)
                    {
                        Debug.WriteLine("    Component Name = " + component.NameLabel);
                    }
                    foreach (Commodore_Repair_Toolbox.File file in board.Files)
                    {
                        Debug.WriteLine("    File Name = " + file.Name + ", FileName = " + file.FileName);
                        foreach (ComponentBounds component in file.Components)
                        {
                            Debug.WriteLine("      Component Name = " + component.NameLabel);
                            if (component.Overlays != null)
                            {
                                foreach (Overlay overlay in component.Overlays)
                                {
                                    Debug.WriteLine("        Overlay X=" + overlay.Bounds.X + ", Y=" + overlay.Bounds.Y + ", Width=" + overlay.Bounds.Width + ", Height=" + overlay.Bounds.Height);
                                }
                            }
                        }
                    }
                }
            }

            // Create the array that will be used for all overlays
            CreateOverlayArrays("tab");
            CreateOverlayArrays("list");

            // Initialize the two parts in the "Main" tab, the zoomable area and the image list
            InitializeTabMain();
            InitializeList();
            InitializeComponentList();
        }


        // ---------------------------------------------------------------------------------

        private void InitializeComponentList()
        {

            // Search for the correct Hardware and Board
            Hardware foundHardware = classHardware.FirstOrDefault(h => h.Name == "Commodore 64 (Breadbin)");
            if (foundHardware != null)
            {
                Board foundBoard = foundHardware.Boards.FirstOrDefault(b => b.Name == "250425");
                if (foundBoard != null)
                {
                    foreach (ComponentBoard component in foundBoard.Components)
                    {
                        
                        string displayText = component.NameLabel + " 123";
                        string actualValue = component.NameLabel;
                        listBox1.Items.Add(displayText);
                        listBoxNameValueMapping[displayText] = actualValue;
                    }
                }
            }
        }


        private void InitializeTabMain()
        {

            // Initialize main panel, make it part of the "tabMain" and fill the entire size
            panelMain = new CustomPanel
            {
                Size = new Size(panel1.Width - panel2.Width - 25, panel1.Height),
                AutoScroll = true,
                Dock = DockStyle.Fill,
            };
            panel1.Controls.Add(panelMain);

            // Initialize image panel
            panelImage = new Panel
            {
                Size = image.Size,
                BackgroundImage = image,
                BackgroundImageLayout = ImageLayout.Zoom,
                Dock = DockStyle.None,
            };
            panelMain.Controls.Add(panelImage);

            // Enable double buffering for smoother updates
            panelMain.DoubleBuffered(true);
            panelImage.DoubleBuffered(true);

            // Create all overlays defined in the array
            foreach (PictureBox overlayTab in overlayComponentsTab)
            {
                panelImage.Controls.Add(overlayTab);
                overlayTab.DoubleBuffered(true);

                overlayTab.MouseDown += PanelImage_MouseDown;
                overlayTab.MouseUp += PanelImage_MouseUp;
                overlayTab.MouseMove += PanelImage_MouseMove;
                overlayTab.MouseEnter += new EventHandler(this.Overlay_MouseEnter);
                overlayTab.MouseLeave += new EventHandler(this.Overlay_MouseLeave);
            }

            ResizeTabImage();

            // Attach event handlers for mouse events and form shown
            panelMain.CustomMouseWheel += new MouseEventHandler(PanelMain_MouseWheel);
            panelImage.MouseDown += PanelImage_MouseDown;
            panelImage.MouseUp += PanelImage_MouseUp;
            panelImage.MouseMove += PanelImage_MouseMove;
            panelMain.Resize += new EventHandler(this.PanelMain_Resize);

            // Later, to get the actual value based on the selected item
//            string selectedDisplayText = listBox1.SelectedItem.ToString();
//            string selectedActualValue = listBoxNameValueMapping[selectedDisplayText];
            
        }


        // ---------------------------------------------------------------------------------

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxSelectedActualValues.Clear();
            
            // Loop through the SelectedItems collection of the ListBox
            foreach (var selectedItem in listBox1.SelectedItems)
            {
                string selectedDisplayText = selectedItem.ToString();
                if (listBoxNameValueMapping.ContainsKey(selectedDisplayText))
                {
                    string selectedActualValue = listBoxNameValueMapping[selectedDisplayText];
                    listBoxSelectedActualValues.Add(selectedActualValue);
                }
            }

            // Display the actual values (for demonstration purposes)
            MessageBox.Show("Selected Actual Values: " + string.Join(", ", listBoxSelectedActualValues));
        }



        private void InitializeList()
        {

            Panel panelList1;
            Panel panelList2;
            Label labelList1;
            PictureBox overlayList1;

            // Initialize main panel, make it part of the "tabMain" and fill the entire size
            panelList1 = new Panel
            {
                Size = new Size(panel2.Width, panel2.Height),
                AutoScroll = true,
                Dock = DockStyle.Fill,
            };
            panel2.Controls.Add(panelList1);

            // Initialize image panel
            panelList2 = new Panel
            {
                Size = image.Size,
                BackgroundImage = image,
                BackgroundImageLayout = ImageLayout.Zoom,
                Dock = DockStyle.None,
                //BorderStyle = BorderStyle.FixedSingle,
            };
            panelList1.Controls.Add(panelList2);

            // Add the Paint event handler to draw the border
            panelList2.Paint += new PaintEventHandler((sender, e) =>
            {
                float penWidth = 1;
                using (Pen pen = new Pen(Color.Red, penWidth))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = new float[] { 4, 2 };
                    float halfPenWidth = penWidth / 2;
                    e.Graphics.DrawRectangle(pen, halfPenWidth, halfPenWidth, panelList2.Width - penWidth, panelList2.Height - penWidth);
                }
            });

            labelList1 = new Label
            {
                Text = "Schematics 1 of 2",
                Location = new Point(0, 0),
                BorderStyle = BorderStyle.FixedSingle,
                AutoSize = true,
                BackColor = Color.White,
                Padding = new Padding(left:2, top:2, right:2, bottom:2),
            };
            panelList2.Controls.Add(labelList1);

            panelList1.DoubleBuffered(true);
            panelList2.DoubleBuffered(true);
            labelList1.DoubleBuffered(true);

            // Create all overlays defined in the array
            foreach (PictureBox overlayList in overlayComponentsList)
            {
                panelList2.Controls.Add(overlayList);
                overlayList.DoubleBuffered(true);
            }

            // Set the zoom factor for the size of the panel
            float xZoomFactor = (float)panelList1.Width / image.Width;
            float yZoomFactor = (float)panelList1.Height / image.Height;
            float zoomFactor = Math.Min(xZoomFactor, yZoomFactor);

            // Update the image based on the zoom factor
            panelList2.Size = new Size((int)(image.Width * zoomFactor), (int)(image.Height * zoomFactor));

            HighlightOverlays("list", zoomFactor);
        }


        // ---------------------------------------------------------------------------------


        private void HighlightOverlays (string scope, float zoomFactor)
        {

            if (scope == "tab")
            {
                int index = 0;
                foreach (PictureBox overlay in overlayComponentsTab)
                {
                    Size originalSize = overlayComponentsTabOriginalSizes[index];
                    Point originalLocation = overlayComponentsTabOriginalLocations[index];
                    int newWidth = (int)(originalSize.Width * zoomFactor);
                    int newHeight = (int)(originalSize.Height * zoomFactor);
                    overlay.Size = new Size(newWidth, newHeight);
                    overlay.Location = new Point((int)(originalLocation.X * zoomFactor), (int)(originalLocation.Y * zoomFactor));

                    // Dispose the overlay transparent bitmap and create a new one (bitmaps cannot be resized)
                    if (overlay.Image != null)
                    {
                        overlay.Image.Dispose();
                    }
                    Bitmap newBmp = new Bitmap(newWidth, newHeight);
                    using (Graphics g = Graphics.FromImage(newBmp))
                    {
                        g.Clear(Color.FromArgb(128, Color.Red)); // 50% opacity
                    }
                    overlay.Image = newBmp;
                    overlay.DoubleBuffered(true);

                    index++;
                }
            }
            
            
            if (scope == "list")
            {
                foreach (PictureBox overlayList in overlayComponentsList)
                {
                    int newWidth = (int)(overlayList.Width * zoomFactor);
                    int newHeight = (int)(overlayList.Height * zoomFactor);
                    overlayList.Size = new Size(newWidth, newHeight);
                    overlayList.Location = new Point((int)(overlayList.Location.X * zoomFactor), (int)(overlayList.Location.Y * zoomFactor));

                    // Dispose the overlay transparent bitmap and create a new one (bitmaps cannot be resized)
                    if (overlayList.Image != null)
                    {
                        overlayList.Image.Dispose();
                    }
                    Bitmap newBmp = new Bitmap(newWidth, newHeight);
                    using (Graphics g = Graphics.FromImage(newBmp))
                    {
                        g.Clear(Color.FromArgb(128, Color.Red)); // 50% opacity
                    }
                    overlayList.Image = newBmp;
                    overlayList.DoubleBuffered(true);
                }
            }

            
        }


        // ---------------------------------------------------------------------------------


        private void CreateOverlayArrays (string scope)
        {

            // Search for the correct Hardware, Board, File, and ComponentBounds
            Hardware foundHardware = classHardware.FirstOrDefault(h => h.Name == "Commodore 64 (Breadbin)");  
            if (foundHardware != null)
            {
                Board foundBoard = foundHardware.Boards.FirstOrDefault(b => b.Name == "250425");  
                if (foundBoard != null)
                {
                    Commodore_Repair_Toolbox.File foundFile = foundBoard.Files.FirstOrDefault(f => f.Name == "Schematics 1 of 2");
                    if (foundFile != null)
                    {
                        foreach (ComponentBounds component in foundFile.Components)
                        {
                            if (component.Overlays != null)
                            {
                                foreach (Overlay overlay in component.Overlays)
                                {
                                    Debug.WriteLine("        Overlay X=" + overlay.Bounds.X + ", Y=" + overlay.Bounds.Y + ", Width=" + overlay.Bounds.Width + ", Height=" + overlay.Bounds.Height);

                                    // Define the PictureBox
                                    PictureBox overlayPb = new PictureBox
                                    {
                                        Name = $"U{component.NameLabel}",
                                        Location = new Point(overlay.Bounds.X, overlay.Bounds.Y),
                                        Size = new Size(overlay.Bounds.Width, overlay.Bounds.Height),
                                        BackColor = Color.Transparent,
                                    };

                                    // Add the PictureBox to the "Main" image
                                    if (scope == "tab")
                                    {
                                        overlayComponentsTab.Add(overlayPb);
                                        int index = overlayComponentsTab.Count - 1;
                                        overlayComponentsTabOriginalSizes.Add(index, overlayPb.Size);
                                        overlayComponentsTabOriginalLocations.Add(index,overlayPb.Location);
                                    }

                                    // Add the PictureBox to the "List" image (?????????????)
                                    if (scope == "list")
                                    {
                                        overlayComponentsList.Add(overlayPb);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        // ---------------------------------------------------------------------------------


        /*
        private void Main_Shown(object sender, EventArgs e)
        {
            //panelMain.AutoScrollPosition = new Point(750, 400);FitImageToPanel();
        }
        */


        // ---------------------------------------------------------------------------------


        private void ResizeTabImage()
        {
            // Set the zoom factor
            float xZoomFactor = (float)panelMain.Width / image.Width;
            float yZoomFactor = (float)panelMain.Height / image.Height;
            zoomFactor = Math.Min(xZoomFactor, yZoomFactor);

            // Update the image size to the zoom factor
            panelImage.Size = new Size((int)(image.Width * zoomFactor), (int)(image.Height * zoomFactor));

            HighlightOverlays("tab",zoomFactor);
        }


        // ---------------------------------------------------------------------------------


        private void PanelMain_Resize(object sender, EventArgs e)
        {
            if (!isResizedByMouseWheel)
            {
                ResizeTabImage();
            }

            isResizedByMouseWheel = false;
        }


        // ---------------------------------------------------------------------------------


        private void PanelMain_MouseWheel(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("MouseWheel event");
            float oldZoomFactor = zoomFactor;

            Debug.WriteLine("Before: panelMain.Width=" + panelMain.Width + ", panelImage.Width=" + panelImage.Width + ", image.Width=" + image.Width + ", panelMain.AutoScrollPosition.X=" + panelMain.AutoScrollPosition.X);

            Debug.WriteLine("zoomFactor=" + zoomFactor);

            // Change the zoom factor based on the mouse wheel movement.
            bool hasZoomChanged = false;
            if (e.Delta > 0)
            {
                if (zoomFactor <= 5)
                {
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
                isResizedByMouseWheel = true;
                  
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

                //HighlightTabOverlay();
                HighlightOverlays("tab", zoomFactor);

                Debug.WriteLine("After: panelMain.Width=" + panelMain.Width + ", panelImage.Width=" + panelImage.Width + ", image.Width=" + image.Width + ", panelMain.AutoScrollPosition.X=" + panelMain.AutoScrollPosition.X);

            }
        }


        // ---------------------------------------------------------------------------------


        private void PanelImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Debug.WriteLine("MouseDown event");
                lastMousePosition = e.Location;
            }
        }


        // ---------------------------------------------------------------------------------


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


        // ---------------------------------------------------------------------------------


        private void PanelImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Debug.WriteLine("MouseUp event");
                lastMousePosition = Point.Empty;
            }
        }


        // ---------------------------------------------------------------------------------


        private void Overlay_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
            Control control = sender as Control;
            if (control != null)
            {
                label3.Text = control.Name;
            }
            label3.Visible = true;
        }


        // ---------------------------------------------------------------------------------


        private void Overlay_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
            label3.Visible = false;
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

    public class Hardware
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public List<Board> Boards { get; set; }
    }

    public class Board
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public List<File> Files { get; set; }
        public List<ComponentBoard> Components { get; set; }
    }

    public class File
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string HighlightColorTab { get; set; }
        public string HighlightColorList { get; set; }
        public string HighlightOpacityTab { get; set; }
        public string HighlightOpacityList { get; set; }
        public List<ComponentBounds> Components { get; set; }
    }

    public class ComponentBoard
    {
        public string NameLabel { get; set; }
        public string NameTechnical { get; set; }
        public string NameFriendly { get; set; }
    }

    public class ComponentBounds
    {
        public string NameLabel { get; set; }
        public List<Overlay> Overlays { get; set; }
    }

    public class Overlay
    {
        public Rectangle Bounds { get; set; }
    }

}