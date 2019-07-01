using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace RozpoznawanieLiter
{
    public partial class Form1 : Form
    {
        private Graphics g;
        private Pen rys = new Pen(Color.Black);
        private Brush brush = new SolidBrush(Color.Black);

        private int ileEpok;
        private double uczenie;
        private double bladMax;
        private int[,] wzor = new int[100, 64];

        private int numerWzorca = 0;
        private Point p = Point.Empty;

        private int dlugoscO;
        private int dlugoscG;
        private int dlugoscR;

        double[,] w = new double[3, 64];
        double[] y = new double[3];

        private Boolean klik = true;
        private Boolean maluj = false;


        public Form1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(512, 512);  //Tworzenie pustej bitmapy w PictureBoxie
            g = Graphics.FromImage(pictureBox1.Image);

        }

        private void Wagi() //Obliczenie wag dla wspólczynnika odpowiadającego każdej literze
        {
            Random r = new Random();

            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 64; ++j)
                    w[i, j] = (0.1 - 0.01) * r.NextDouble() + 0.01;
        }

        private void Skalowanie() //skalowanie obrazka żeby zajmowal cały PictureBox
        {
            int left = 512;
            int right = 0;
            int up = 512;
            int down = 0;

            Bitmap bitmap = new Bitmap(pictureBox1.Image);

            for (int i = 0; i < 512; ++i)
                for (int j = 0; j < 512; ++j)
                {
                    Color kolor = bitmap.GetPixel(i, j);
                    if (kolor.R == 0 && kolor.G == 0 && kolor.B == 0)
                    {
                        if (left > i) left = i;
                        if (right < i) right = i;
                        if (up > j) up = j;
                        if (down < j) down = j;
                    }
                }

            Bitmap bitmapPainted = new Bitmap(right - left + 1, down - up + 1);

            for (int i = 0; i <= right - left; ++i)
            {
                for (int j = 0; j <= down - up; ++j)
                {
                    bitmapPainted.SetPixel(i, j, bitmap.GetPixel(left + i, up + j));
                }
            }

            Graphics g3 = Graphics.FromImage(pictureBox1.Image);

            g3.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;  //rysowanie przeskalowanej bitmapy
            g3.DrawImage(bitmapPainted, 0, 0, 512, 512);
            pictureBox1.Refresh();
        }

        private double ObliczWartoscNeuronu(int numPattern, int numNeuron) //przeliczenie wartości neuronów
        {
            double z0 = 0;
            double z1 = 0;
            double z2 = 0;

            if (numPattern <= dlugoscO) z0 = 1;
            else if (numPattern <= dlugoscO + dlugoscG) z1 = 1;
            else if (numPattern <= dlugoscO + dlugoscG + dlugoscR) z2 = 1;


            if (numNeuron == 0) return z0 - y[0];
            else if (numNeuron == 1) return z1 - y[1];
            else return z2 - y[2];
        }

        private void ZmianaWag(int numPattern, int epoch)  //zmiana wartości wag (proces uczenia)
        {
            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 64; ++j)
                {
                    double deltaW = uczenie * ObliczWartoscNeuronu(numPattern, i) * ((1 - y[i]) * y[i]) * wzor[numPattern, j];
                    w[i, j] += deltaW;
                }
        }

        private void LoadWzor()  //załadowanie wzorcowych obrazów w formacie jpg z folderu ObrazyWzorcowe
        {
            string[] literaO = Directory.GetFiles(@"obrazyWzorcowe\O\");
            string[] literaG = Directory.GetFiles(@"obrazyWzorcowe\G\");
            string[] literaR = Directory.GetFiles(@"obrazyWzorcowe\R\");

            dlugoscO = literaO.Length;
            dlugoscG = literaG.Length;
            dlugoscR = literaR.Length;

            for (int f = 0; f < dlugoscO; ++f)
            {
                ++numerWzorca;

                ReadImage(literaO[f]);

                for (int i = 0; i < 64; i++)
                    wzor[numerWzorca, i] = wzor[0, i];
            }

            for (int f = 0; f < dlugoscG; ++f)
            {
                ++numerWzorca;

                ReadImage(literaG[f]);

                for (int i = 0; i < 64; i++)
                    wzor[numerWzorca, i] = wzor[0, i];
            }

            for (int f = 0; f < dlugoscR; ++f)
            {
                ++numerWzorca;

                ReadImage(literaR[f]);

                for (int i = 0; i < 64; i++)
                    wzor[numerWzorca, i] = wzor[0, i];
            }
        }

        private void ReadImage(string imgPath)  //sprawdzanie czy dany fragment wzorcowego obrazka jest zamalowany
        {
            Bitmap bitmap = new Bitmap(Image.FromFile(@imgPath));

            for (int i = 0; i < 64; i++)
                wzor[0, i] = 0;

            for (int i = 0; i < 512; ++i)
            {
                for (int j = 0; j < 512; ++j)
                {
                    Color kolor = bitmap.GetPixel(i, j);
                    if (kolor.R != 255 || kolor.G != 255 || kolor.B != 255)
                        wzor[0, i / 64 + j / 64 * 8] = 1;
                }
            }
        }

        private void TworzenieCiagu()  //budowanie ciągu '0' '1' zależnie od zamalowania obrazka we wzorcu, ciąg wzorcowy
        {
            Bitmap bitmap = new Bitmap(pictureBox1.Image);

            for (int i = 0; i < 64; i++)
                wzor[0, i] = 0;

            for (int i = 0; i < 512; ++i)
            {
                for (int j = 0; j < 512; ++j)
                {
                    Color kolor = bitmap.GetPixel(i, j);
                    if (kolor.R != 255 || kolor.G != 255 || kolor.B != 255)
                        wzor[0, i / 64 + j / 64 * 8] = 1;
                }
            }
        }

        private void ComputeOutputs(int numerWzorca)  //obliczanie wyjścia z neuronu na podstawie wag i wzorca
        {
            for (int i = 0; i < y.Length; ++i)
            {
                y[i] = 0;
                for (int j = 0; j < 64; ++j) y[i] += wzor[numerWzorca, j] * w[i, j];
                y[i] = Aktywacji(y[i]);
            }
        }

        private double ComputeErrorNetwork()
        {
            double sumErrorsInNetwork = 0;

            for (int i = 1; i <= numerWzorca; ++i)
            {
                ComputeOutputs(i);
                double sumErrorsInPattern = 0;
                for (int j = 0; j < 3; ++j) sumErrorsInPattern += Math.Abs(ObliczWartoscNeuronu(i, j));
                sumErrorsInNetwork += sumErrorsInPattern / 3;
            }

            return sumErrorsInNetwork / numerWzorca;
        }

        private bool IsBadErrorNetwork(int numPattern)
        {
            double z0 = 0;
            double z1 = 0;
            double z2 = 0;

            double errorZ0;
            double errorZ1;
            double errorZ2;

            if (numPattern <= dlugoscO) z0 = 1;
            else if (numPattern <= dlugoscO + dlugoscG) z1 = 1;
            else if (numPattern <= dlugoscO + dlugoscG + dlugoscR) z2 = 1;

            errorZ0 = Math.Abs(z0 - y[0]);
            errorZ1 = Math.Abs(z1 - y[1]);
            errorZ2 = Math.Abs(z2 - y[2]);

            if (errorZ0 > bladMax || errorZ1 > bladMax || errorZ2 > bladMax) return true;
            return false;
        }

        private double Aktywacji(double y)  //Funkcja aktywacji neuronu (sigmoidalna)
        {
            double e = 2.72;
            double B = 1;
            return 1.0 / (1.0 + Math.Pow(e, -B * y));
        }

        public void a()  //uczenie
        {
            ileEpok = 1000;
            uczenie = 0.1;
            bladMax = 0.01;
            LoadWzor();
            Wagi();

            int epoka = 0;

            while (epoka < ileEpok) //kolejne epoki uczenia
            {
                
                bool badError = false;
                for (int i = 1; i <= numerWzorca; ++i)
                {
                    ComputeOutputs(i); //obliczenie wyjścia neuronu

                    if (IsBadErrorNetwork(i) == true)
                    {
                        ZmianaWag(i, epoka); //zmiana wag i powrót na poczatek (sprzeżenie zwrotne)
                        badError = true;
                    }
                }
                if (badError == false)
                {
                    break;
                }
                
                ++epoka;
                if (epoka >= 1000) { label4.Invoke(new Action(delegate() { label4.Text = "Już nauczone"; })); }
            }
            
            MessageBox.Show("Sieć nauczona", "Nauczanie zakończone");
            
        }

        private void button1_Click(object sender, EventArgs e) 
        {
            label4.Text = "Prosze czekać";
            System.Threading.Thread newThread =
            new System.Threading.Thread(a);
            newThread.Start();
            
        }

        private void button2_Click(object sender, EventArgs e) //rozpoznanie konkretnej litery i przypisanie do TextBoxów stopnia podobieństwa do danej litery
        {
            Skalowanie();
            TworzenieCiagu();
            ComputeOutputs(0);
            for (int i = 0; i < 3; i++ )
            {
                y[i] = y[i] * 100;
                y[i] = Math.Floor(y[i]);
                y[i] = y[i] / 100;
            }
                textBox1.Text = Convert.ToString(y[0]);
            textBox2.Text = Convert.ToString(y[1]);
            textBox3.Text = Convert.ToString(y[2]);
            
            if ((y[0] >= y[1]) && (y[0] >= y[2])) MessageBox.Show("Wybrana litera to O", "Wynik");
            else if ((y[1] >= y[0]) && (y[1] >= y[2])) MessageBox.Show("Wybrana litera to G", "Wynik");
            else if ((y[2] >= y[0]) && (y[2] >= y[1])) MessageBox.Show("Wybrana litera to R", "Wynik");
        }

        private void button3_Click(object sender, EventArgs e) //wyczyszczenie zawartości obrazka
        {
            g.Clear(Color.White);
            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)  //rysowanie czarnej linii na PictureBoxie
        {
            if (klik)
            {
                g.Clear(Color.White);
                pictureBox1.Refresh();
                klik = false;
            }

            if (e.Button == MouseButtons.Left)
            {
                maluj = true;
                p = e.Location;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (maluj && e.Button == MouseButtons.Left)
            {
                g.DrawLine(rys, p, e.Location);
                p = e.Location;
                pictureBox1.Refresh();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            maluj = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Skalowanie();
            pictureBox1.Image.Save(@"obrazyWzorcowe\(new)");
        }
    }
}
