using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EdgeDetection {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private string path;

        public MainWindow() {
            InitializeComponent();
        }

        private void OnImageDropped(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0) {
                    path = files[0];
                    try {
                        BitmapImage bitmapImage = new BitmapImage(new Uri(path));
                        droppedImage.Source = bitmapImage;
                    }
                    catch (Exception ex) {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            //Prepare image parameters
            if (!int.TryParse(threadsNumber.Text, out int numChunks) || numChunks < 1 || numChunks > 64) {
                MessageBox.Show("Invalid number of threads.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (path == null) {
                MessageBox.Show("Upload image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Bitmap image = new Bitmap(path);

            //Calculate chunks heights
            int[] heights = new int[numChunks];
            int quotient = image.Height / numChunks;
            int reminder = image.Height % numChunks;
            for (int i = 0; i < numChunks; i++) {
                heights[i] = quotient;
                if (reminder > 0) {
                    heights[i]++;
                    reminder--;
                }
            }

            //Split image
            List<Bitmap> chunks = new List<Bitmap>();
            int yOffset = 0;
            for (int i = 0; i < numChunks; i++) {
                Rectangle rect = new Rectangle(0, yOffset, image.Width, heights[i]);
                Bitmap chunk = image.Clone(rect, image.PixelFormat);
                chunks.Add(chunk);

                yOffset += heights[i];
            }

            //Pick filter
            IProxy proxy;
            if (assembly.IsChecked == true) {
                proxy = new AsmProxy();
            }
            else {
                proxy = new CppProxy();
            }

            int start = Environment.TickCount & Int32.MaxValue;
            //Use filter
            Parallel.ForEach(chunks, chunk => {
                BitmapData bitmapData = chunk.LockBits(
                    new Rectangle(0, 0, chunk.Width, chunk.Height),
                    ImageLockMode.ReadWrite,
                    chunk.PixelFormat
                );
                IntPtr ptr = bitmapData.Scan0;
                int bytes = bitmapData.Stride * bitmapData.Height;
                byte[] values = new byte[bytes];
                byte[] convertedValues = new byte[bytes];
                Marshal.Copy(ptr, values, 0, bytes);

                proxy.executeEdgeDetection(
                    values,
                    bitmapData.Width,
                    bitmapData.Height,
                    bitmapData.Stride / bitmapData.Width,
                    convertedValues
                    );

                Marshal.Copy(convertedValues, 0, ptr, bytes);
                chunk.UnlockBits(bitmapData);
            });
            int stop = Environment.TickCount & Int32.MaxValue;

            //Merge image
            Bitmap mergedImage = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(mergedImage)) {
                yOffset = 0;
                foreach (var chunk in chunks) {
                    g.DrawImage(chunk, 0, yOffset);
                    yOffset += chunk.Height;
                }
            }

            //Convert Bitmap to BitmapImage
            using (MemoryStream memoryStream = new MemoryStream()) {
                mergedImage.Save(memoryStream, ImageFormat.Png);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                droppedImage.Source = bitmapImage;
            }

            MessageBox.Show(
                $"Image processing took {stop - start} ms",
                "Time",
                MessageBoxButton.OK,
                MessageBoxImage.Information
                );
        }
    }
}
