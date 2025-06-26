using System.Windows.Forms;

namespace Simulated_Annealing
{
    public partial class Form1 : Form
    {
        private int N, K;
        private List<PointF> points = new List<PointF>();
        public Form1()
        {
            InitializeComponent();
            buttonLoadFile.Click += ButtonLoadFile_Click;
            buttonCalculate.Click += ButtonCalculate_Click;
        }

        private void ButtonLoadFile_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                openFileDialog.Title = "Select an Input File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    (N, K, points) = LoadPointsFromFile(openFileDialog.FileName);
                    MessageBox.Show("File loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ButtonCalculate_Click(object? sender, EventArgs e)
        {
            if (points.Count == 0)
            {
                MessageBox.Show("Please load a file first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Draw();
        }
        private void DrawPolygonAndPoints(List<PointF> polygon, List<PointF> points)
        {
            // Set up drawing area
            Bitmap bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Clear background
                g.Clear(Color.White);

                // Draw points
                foreach (PointF point in points)
                {
                    g.FillEllipse(Brushes.Red, point.X * 100 + 50, point.Y * 100 + 50, 5, 5);
                }

                // Draw polygon
                for (int i = 0; i < polygon.Count; i++)
                {
                    PointF p1 = polygon[i];
                    PointF p2 = polygon[(i + 1) % polygon.Count];
                    g.DrawLine(Pens.Blue, p1.X * 100 + 50, p1.Y * 100 + 50, p2.X * 100 + 50, p2.Y * 100 + 50);
                }
            }

            // Display result in PictureBox
            pictureBox.Image = bitmap;
        }

        public void Draw()
        {
            List<PointF> initialPolygon = InitializePolygon(points, K);
            int maxIterations = 1000;
            float epsilon = 0.01f;
            int maxRestarts = 10;

            List<PointF> bestPolygon = SimulatedAnnealing(points, initialPolygon, K, maxIterations, epsilon, maxRestarts);

            listBoxResults.Items.Clear();
            listBoxResults.Items.Add("Optimal convex hull:");
            foreach (PointF point in bestPolygon)
            {
                string pointText = $"({point.X:F2}, {point.Y:F2})";
                listBoxResults.Items.Add(pointText);
            }

            DrawPolygonAndPoints(bestPolygon, points);
        }

        static (int, int, List<PointF>) LoadPointsFromFile(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            int N = int.Parse(lines[0].Trim());
            int K = int.Parse(lines[1].Trim());

            List<PointF> points = new List<PointF>();
            for (int i = 2; i < lines.Length; i++)
            {
                string[] coords = lines[i].Trim().Split(',');
                float x = float.Parse(coords[0]);
                float y = float.Parse(coords[1]);
                points.Add(new PointF(x, y));
            }

            return (N, K, points);
        }

        static List<PointF> InitializePolygon(List<PointF> points, int K)
        {
            if (points.Count == K)
            {
                return new List<PointF>(points);
            }

            // Calculate centroid
            float centroidX = points.Average(p => p.X);
            float centroidY = points.Average(p => p.Y);
            PointF centroid = new PointF(centroidX, centroidY);

            // Calculate maximum distance to the centroid
            float maxDistance = points.Max(p => Distance(p, centroid));

            // Place polygon points in a circle
            List<PointF> polygon = new List<PointF>();
            for (int i = 0; i < K; i++)
            {
                double angle = 2 * Math.PI * i / K;
                float x = centroidX + (float)(maxDistance * Math.Cos(angle));
                float y = centroidY + (float)(maxDistance * Math.Sin(angle));
                polygon.Add(new PointF(x, y));
            }

            return polygon;
        }

        static float Distance(PointF a, PointF b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        static float CalculatePerimeter(List<PointF> polygon)
        {
            float perimeter = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                perimeter += Distance(polygon[i], polygon[(i + 1) % polygon.Count]);
            }
            return perimeter;
        }

        static bool ContainsAllPoints(List<PointF> polygon, List<PointF> points)
        {
            foreach (PointF point in points)
            {
                if (!IsPointInsidePolygon(point, polygon))
                {
                    return false;
                }
            }
            return true;
        }

        static bool IsPointInsidePolygon(PointF point, List<PointF> polygon)
        {
            int j = polygon.Count - 1;
            bool inside = false;
            for (int i = 0; i < polygon.Count; i++)
            {
                if ((polygon[i].Y < point.Y && polygon[j].Y >= point.Y ||
                     polygon[j].Y < point.Y && polygon[i].Y >= point.Y) &&
                    (polygon[i].X <= point.X || polygon[j].X <= point.X))
                {
                    if (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < point.X)
                    {
                        inside = !inside;
                    }
                }
                j = i;
            }
            return inside;
        }

        static List<PointF> SimulatedAnnealing(List<PointF> points, List<PointF> initialPolygon, int K, int maxIterations, float epsilon, int maxRestarts)
        {
            List<PointF>? bestPolygon = null;
            float bestPerimeter = float.MaxValue;

            Random random = new Random();
            HashSet<string> visitedPolygons = new HashSet<string>(); // HashSet itt legyen!

            for (int restart = 0; restart < maxRestarts; restart++)
            {
                // Új poligon inicializálása
                List<PointF> currentPolygon = InitializePolygon(points, K);
                float currentPerimeter = CalculatePerimeter(currentPolygon);

                string polygonHash = HashPolygon(currentPolygon);

                // **Ha ezt a poligont már láttuk, ugorjunk a következõ restart-ra**
                if (visitedPolygons.Contains(polygonHash))
                {
                    continue;
                }
                visitedPolygons.Add(polygonHash); // Új poligon hozzáadása

                for (int iteration = 0; iteration < maxIterations; iteration++)
                {
                    bool improved = false;
                    float temperature = TemperatureFunction(iteration, maxIterations);

                    // Iterálunk a csúcsokon
                    for (int i = 0; i < K; i++)
                    {
                        PointF originalPoint = currentPolygon[i];

                        // Szomszédos pontok generálása és vizsgálata
                        foreach (PointF neighbor in GenerateNeighbors(originalPoint, epsilon))
                        {
                            currentPolygon[i] = neighbor;

                            if (ContainsAllPoints(currentPolygon, points))
                            {
                                float newPerimeter = CalculatePerimeter(currentPolygon);
                                float delta = newPerimeter - currentPerimeter;

                                if (delta < 0 || random.NextDouble() < AcceptanceProbability(delta, temperature))
                                {
                                    currentPerimeter = newPerimeter;
                                    improved = true;

                                    // **Új hash kiszámítása, csak ha változás történt**
                                    polygonHash = HashPolygon(currentPolygon);
                                    visitedPolygons.Add(polygonHash);

                                    break;
                                }
                            }
                        }

                        if (!improved)
                        {
                            currentPolygon[i] = originalPoint;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (!improved)
                    {
                        break;
                    }
                }

                if (currentPerimeter < bestPerimeter)
                {
                    bestPerimeter = currentPerimeter;
                    bestPolygon = new List<PointF>(currentPolygon);
                }

                Console.WriteLine("Restart: , Best Perimeter: ", restart, bestPerimeter);
            }

            return bestPolygon ?? new List<PointF>();
        }

        static float TemperatureFunction(int iteration, int maxIterations)
        {
            // Exponential decay temperature function
            float initialTemperature = 1000.0f; // Initial temperature
            float finalTemperature = 0.1f;      // Final temperature
            return initialTemperature * (float)Math.Pow(finalTemperature / initialTemperature, (float)iteration / maxIterations);
        }

        static double AcceptanceProbability(float delta, float temperature)
        {
            // Boltzmann probability function
            return Math.Exp(-delta / temperature);
        }

        static IEnumerable<PointF> GenerateNeighbors(PointF point, float epsilon)
        {
            // Generate four possible neighbors: up, down, left, right
            yield return new PointF(point.X + epsilon, point.Y);
            yield return new PointF(point.X - epsilon, point.Y);
            yield return new PointF(point.X, point.Y + epsilon);
            yield return new PointF(point.X, point.Y - epsilon);
        }

        static string HashPolygon(List<PointF> polygon)
        {
            // Create a string representation of the polygon to store in the Tabu list
            return string.Join(";", polygon.Select(p => string.Format("{0:F2},{1:F2}", p.X, p.Y)));
        }
    }
}

/*
using System.Drawing.Drawing2D;

namespace Smallest_Boundary_Polygon
{
    public partial class Form3 : Form
    {
        private List<PointF> inputPoints = new List<PointF>();
        private List<PointF> bestPolygon = new List<PointF>();
        private int polygonSides;
        private Random rand = new Random();

        public Form3()
        {
            InitializeComponent();
            this.Width = 800;
            this.Height = 600;
            polygonTypeComboBox.SelectedIndex = 0;

            loadButton.Click += (s, e) => LoadPointsFromFile();
            runButton.Click += (s, e) =>
            {
                if (polygonTypeComboBox.SelectedItem is string selectedPolygon)
                {
                    polygonSides = selectedPolygon switch
                    {
                        "Triangle" => 3,
                        "Quadrilateral" => 4,
                        "Pentagon" => 5,
                        "Hexagon" => 6,
                        "Heptagon" => 7,
                        "Octagon" => 8,
                        _ => 3
                    };
                    RunSimulatedAnnealing();
                    bestPolygon = GenerateInitialPolygon();
                }
                else
                {
                    MessageBox.Show("Válassz ki egy érvényes oldal számot!");
                }
            };

            Controls.Add(loadButton);
            Controls.Add(runButton);
            Controls.Add(polygonTypeLabel);
            Controls.Add(polygonTypeComboBox);
            this.Paint += DrawPointsAndPolygon;
        }

        private void LoadPointsFromFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                inputPoints.Clear();
                string[] lines = File.ReadAllLines(ofd.FileName);
                string[] parts = lines[0].Split();
                int N = int.Parse(parts[0]);
                polygonSides = int.Parse(parts[1]);

                for (int i = 1; i <= N; i++)
                {
                    string[] coord = lines[i].Split();
                    inputPoints.Add(new PointF(float.Parse(coord[0]) * 50 + 100, float.Parse(coord[1]) * 50 + 100));
                }

                MessageBox.Show($"Betöltve: {N} pont, {polygonSides} oldalú poligon");
                RunSimulatedAnnealing();
                Invalidate();
            }
        }

        private List<PointF> GenerateInitialPolygon()
        {
            PointF center = new PointF(this.Width / 2, this.Height / 2);  // Kör középpontja

            // Kiválasztjuk a kör sugárát, amely az elsõ pontok távolsága alapján lesz meghatározva
            float radius = 200;  // Fix sugár (beállítható kívánság szerint)

            // Az alakzat csúcsait számoljuk ki a körön
            return Enumerable.Range(0, polygonSides).Select(i =>
            {
                double angle = 2 * Math.PI * i / polygonSides;  // Egyenletes szögosztás
                return new PointF(
                    center.X + (float)(radius * Math.Cos(angle)),
                    center.Y + (float)(radius * Math.Sin(angle))
                );
            }).ToList();
        }

        private float CrossProduct(PointF a, PointF b, PointF c)
        {
            float abX = b.X - a.X;
            float abY = b.Y - a.Y;
            float bcX = c.X - b.X;
            float bcY = c.Y - b.Y;
            return abX * bcY - abY * bcX;
        }
        private List<PointF> ConvexHull(List<PointF> points)
        {
            // Pontok rendezése
            List<PointF> sortedPoints = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            List<PointF> lowerHull = new List<PointF>();

            foreach (PointF p in sortedPoints)
            {
                while (lowerHull.Count >= 2 && CrossProduct(lowerHull[lowerHull.Count - 2], lowerHull[lowerHull.Count - 1], p) <= 0)
                {
                    lowerHull.RemoveAt(lowerHull.Count - 1);
                }
                lowerHull.Add(p);
            }

            List<PointF> upperHull = new List<PointF>();
            for (int i = sortedPoints.Count - 1; i >= 0; i--)
            {
                PointF p = sortedPoints[i];
                while (upperHull.Count >= 2 && CrossProduct(upperHull[upperHull.Count - 2], upperHull[upperHull.Count - 1], p) <= 0)
                {
                    upperHull.RemoveAt(upperHull.Count - 1);
                }
                upperHull.Add(p);
            }

            upperHull.RemoveAt(upperHull.Count - 1);
            lowerHull.RemoveAt(lowerHull.Count - 1);

            lowerHull.AddRange(upperHull);
            return lowerHull;
        }

        private void RunSimulatedAnnealing()
        {
            const int maxIterations = 1000;
            const float initialTemperature = 1000f;
            const float alpha = 0.98f;
            const float maxRotation = 5f; // fokban
            bestPolygon = ConvexHull(inputPoints);
            double bestCost = Perimeter(bestPolygon);
            double temperature = initialTemperature;

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                float angleDeg = (float)((rand.NextDouble() * 2 - 1) * maxRotation);
                double rotation = angleDeg * Math.PI / 180.0;

                List<PointF> candidate = bestPolygon.Select(p =>
                {
                    double angle = Math.Atan2(p.Y - this.Height / 2, p.X - this.Width / 2) + rotation;
                    float radius = (float)Distance(p, new PointF(this.Width / 2, this.Height / 2));
                    return new PointF(this.Width / 2 + radius * (float)Math.Cos(angle), this.Height / 2 + radius * (float)Math.Sin(angle));
                }).ToList();

                if (!ContainsAllPoints(candidate, inputPoints))
                    continue;

                double candidateCost = Perimeter(candidate);
                double deltaE = candidateCost - bestCost;

                if (deltaE < 0 || AcceptWithProbability(deltaE, temperature))
                {
                    bestPolygon = candidate;
                    bestCost = candidateCost;
                }

                temperature *= alpha;
                if (temperature < 1e-3) break;
            }

            using (StreamWriter writer = new StreamWriter("bestPolygon_simulated_annealing.txt"))
            {
                foreach (PointF p in bestPolygon)
                {
                    writer.WriteLine($"{p.X:F2} {p.Y:F2}");
                }
            }

            if (bestPolygon.Count == 0)
            {
                MessageBox.Show("Nem találtunk olyan sokszöget, amely lefedi az összes pontot.");
            }
            else
            {
                MessageBox.Show($"Kész! Minimális kerület (szimulált hûtéssel): {bestCost:F2}");
            }

            Invalidate();
        }

        private bool AcceptWithProbability(double deltaE, double temperature)
        {
            if (deltaE < 0) return true; // Always accept better solutions

            double probability = Math.Exp(-deltaE / temperature);
            double randomValue = rand.NextDouble();
            return randomValue < probability;
        }

        private bool ContainsAllPoints(List<PointF> poly, List<PointF> points)
        {
            return points.All(p => PointInPolygon(poly, p));
        }

        private bool PointInPolygon(List<PointF> poly, PointF p)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddPolygon(poly.ToArray());
                return path.IsVisible(p) || path.IsOutlineVisible(p, new Pen(Color.Black, 1));
            }
        }

        private double Perimeter(List<PointF> poly)
        {
            double sum = 0;
            for (int i = 0; i < poly.Count; i++)
            {
                sum += Distance(poly[i], poly[(i + 1) % poly.Count]);
            }
            return sum;
        }

        private double Distance(PointF a, PointF b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        // Handle the Paint event to draw the circle, points, and polygons
        private void DrawPointsAndPolygon(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            PointF center = new PointF(this.Width / 2, this.Height / 2);  // Circle center

            // Draw the circle
            const int radius = 200;  // Fixed radius for the circle
            g.DrawEllipse(Pens.Black, center.X - radius, center.Y - radius, 2 * radius, 2 * radius);

            // Draw the points
            SolidBrush brush = new SolidBrush(Color.Red);
            foreach (PointF p in inputPoints)
            {
                g.FillEllipse(brush, p.X - 4, p.Y - 4, 8, 8);  // Red points
            }

            // Draw the polygon (the best one found by simulated annealing)
            if (bestPolygon.Count > 1)
            {
                Pen pen = new Pen(Color.Blue, 2);
                for (int i = 0; i < bestPolygon.Count; i++)
                {
                    PointF a = bestPolygon[i];
                    PointF b = bestPolygon[(i + 1) % bestPolygon.Count];
                    g.DrawLine(pen, a, b);  // Polygon side
                }
            }
        }
    }
}
*/