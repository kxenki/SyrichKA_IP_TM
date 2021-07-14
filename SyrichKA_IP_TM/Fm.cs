using Bunifu.UI.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyrichKA_IP_TM
{
    public partial class Fm : Form
    {
        private const int laTitleOffsetYX = 20;
        private BunifuLabel laTitle;
        private double suitable;
        private double faulty;
        private double useful;
        private double ES;
        private double EI;
        private double X;
        private double Xnew;
        private double Sigma;
        private Bitmap b;
        private Graphics g;
        private int pxGraphWidth;

        public List<PointF> DrawGraph { get; private set; }
        public List<PointF> FillGraph1 { get; private set; }
        public List<PointF> FillGraph2 { get; private set; }

        public Fm()
        {
            InitializeComponent();

            laTitle = new Bunifu.UI.WinForms.BunifuLabel();
            laTitle.Text = "Расчет процента годных деталей";
            laTitle.Font = new Font("SegoeUI", 30);
            laTitle.ForeColor = Color.White;
            laTitle.Location = new Point(Screen.PrimaryScreen.Bounds.Width / 2 - laTitle.Width / 2, laTitleOffsetYX);
            Controls.Add(laTitle);

            contentPanel.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height - laTitle.Height * 2);
            paParams.Size = new Size(Screen.PrimaryScreen.Bounds.Width / 4, contentPanel.Size.Height);
            paAnswers.Size = paParams.Size;
            pxGraphWidth = Screen.PrimaryScreen.Bounds.Width - paAnswers.Width - paParams.Width;

            ConvertValuesFromTB();

            DeterminationOfSuitable(X);
            DeterminationOfFailty(X);
            DeterminationOfUseful(X);

            SetParamsOnPanel();

            CalcOffsetX();
            SetControlsOnAnswerPanel();


            b = new Bitmap(pxGraphWidth, pxGraph.Height);
            g = Graphics.FromImage(b);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            CreateGraph();
            pxGraph.Paint += (s, e) => e.Graphics.DrawImage(b, 0, 0);

            buBuild.Click += BuBuild_Click;
            buBuildNew.Click += BuBuildNew_Click;
            buExit.Click += (s, e) => Application.Exit();
        }


        private bool CheckParams()
        {
            bool checkX;
            bool checkEI;
            bool checkES;
            if (X > 4 * Sigma)
            {
                tbX.ForeColor = Color.Red;
                checkX = false;
            }
            else
            {
                tbX.ForeColor = Color.Black;
                checkX = true;
            }
            if (EI < X - 4 * Sigma)
            {
                tbEI.ForeColor = Color.Red;
                checkEI = false;
            }
            else
            {
                tbEI.ForeColor = Color.Black;
                checkEI = true;
            }
            if (ES > X + 4 * Sigma)
            {
                tbES.ForeColor = Color.Red;
                checkES = false;
            }
            else
            {
                tbES.ForeColor = Color.Black;
                checkES = true;
            }

            return checkX && checkEI && checkES;
        }

        private void CreateGraph()
        {
            g.Clear(Color.FromArgb(252, 247, 247));
            pxGraph.Invalidate();
            int padding = pxGraphWidth / 8;
            int distanceBetweenXandMiddle = (int)(X * padding / Sigma);
            int ei = (int)(EI * padding / Sigma);
            int es = (int)(ES * padding / Sigma);
            Pen pen1 = new Pen(Color.Black, 1)
            {
                StartCap = LineCap.Custom,
                EndCap = LineCap.ArrowAnchor
            };
            Pen pen2 = new Pen(Color.Black, 1);
            Pen pen3 = new Pen(Color.Black, 1)
            {
                StartCap = LineCap.ArrowAnchor,
                EndCap = LineCap.ArrowAnchor
            };
            // начало оси по y
            int startY = pxGraph.ClientSize.Height / 4 * 3;


            // horizontal line
            g.DrawLine(pen1, padding / 3 * 2, pxGraph.ClientSize.Height / 4 * 3, pxGraphWidth - padding / 3 * 2, pxGraph.ClientSize.Height / 4 * 3);
            // vertical line
            g.DrawLine(pen1, 4 * padding - distanceBetweenXandMiddle, pxGraph.ClientSize.Height / 4 * 3 + padding / 2, 4 * padding - distanceBetweenXandMiddle, padding / 3 * 2);
            label0.Text = "0";
            label0.Location = new Point(4 * padding - distanceBetweenXandMiddle - label0.Width / 2, pxGraph.ClientSize.Height / 4 * 3 + padding / 3 * 2);
            // +-3Sigma
            g.DrawLine(pen2, padding, pxGraph.ClientSize.Height / 4 * 3 + padding / 2, padding, pxGraph.ClientSize.Height / 4 * 3 - padding / 2);
            g.DrawLine(pen2, pxGraphWidth - padding, pxGraph.ClientSize.Height / 4 * 3 + padding / 2, pxGraphWidth - padding, pxGraph.ClientSize.Height / 4 * 3 - padding / 2);
            labelSigmaPlus.Text = $"+3σ";
            labelSigmaMin.Text = $"-3σ";
            labelSigmaMin.Location = new Point(padding / 3 * 2, pxGraph.ClientSize.Height / 4 * 3 - padding / 2);
            labelSigmaPlus.Location = new Point(pxGraphWidth - padding / 3 * 2 - labelSigmaPlus.Width, pxGraph.ClientSize.Height / 4 * 3 - padding / 2);
            laSigmaMinNumber.Text = $"{Math.Round(X - 3 * Sigma, 3)}";
            laSigmaMinNumber.Location = new Point(padding - laSigmaMinNumber.Width / 2, pxGraph.ClientSize.Height / 4 * 3 + padding / 4 * 3);
            laSigmaPlusNumber.Text = $"{Math.Round(X + 3 * Sigma, 3)}";
            laSigmaPlusNumber.Location = new Point(pxGraphWidth - padding - laSigmaPlusNumber.Width / 2, pxGraph.ClientSize.Height / 4 * 3 + padding / 4 * 3);
            //middle line
            g.DrawLine(pen2, 4 * padding, padding, 4 * padding, pxGraph.ClientSize.Height / 4 * 3 + padding / 2);

            // x с чертой
            g.DrawLine(pen3, 4 * padding - distanceBetweenXandMiddle, padding * 4 / 3, 4 * padding, padding * 4 / 3);
            labelX.Text = $"x̄ = {X}";
            labelX.Location = new Point(4 * padding - distanceBetweenXandMiddle / 3 * 2, padding);

            //массив для рисования
            DrawGraph = new List<PointF>();
            FillGraph1 = new List<PointF>();
            FillGraph2 = new List<PointF>();
            int j = 0;
            double ymax = functionGraph(X - 3 * Sigma);
            double ymin = functionGraph(X - 3 * Sigma);
            for (double i = X - 3 * Sigma; i <= X + 3 * Sigma + 10; i += 0.001)
            {
                if (functionGraph(i) > ymax)
                    ymax = functionGraph(i);
                if (functionGraph(i) < ymin)
                    ymin = functionGraph(i);
            }
            double coeff = pxGraph.Height / 2 / (ymax - ymin);
            Debug.WriteLine(ymax);
            Debug.WriteLine(ymin);
            Debug.WriteLine(coeff);
            Debug.WriteLine(pxGraph.Height);

            FillGraph1.Add(new PointF(padding, pxGraph.ClientSize.Height / 4 * 3));
            FillGraph2.Add(new PointF(4 * padding - distanceBetweenXandMiddle + es, startY));
            for (double i = X - 3 * Sigma; i <= X + 3 * Sigma + 10; i += 0.001)
            {
                var point = new PointF();

                point.X = (float)(i * padding / Sigma) + 4 * padding - distanceBetweenXandMiddle;
                point.Y = startY - (float)(functionGraph(i) * coeff);

                DrawGraph.Add(point);
                if (i > -3 * Sigma && i <= EI)
                {
                    FillGraph1.Add(point);
                }
                else if (i > ES && i <= X + 3 * Sigma)
                {
                    FillGraph2.Add(point);
                }
                j++;
            }
            FillGraph1.Add(new PointF(4 * padding - distanceBetweenXandMiddle + ei, startY));
            FillGraph2.Add(new PointF(pxGraphWidth - padding, startY));

            HatchBrush Brusha = new HatchBrush(HatchStyle.Vertical, Color.Black, Color.White);

            g.FillPolygon(Brusha, FillGraph1.ToArray());
            g.FillPolygon(Brusha, FillGraph2.ToArray());
            g.DrawLines(pen2, DrawGraph.ToArray());

            //ei
            g.DrawLine(pen2, 4 * padding - distanceBetweenXandMiddle + ei, pxGraph.ClientSize.Height / 4 * 3 + padding / 2, 4 * padding - distanceBetweenXandMiddle + ei, padding * 2);
            laEInumber.Text = $"ei = {Math.Round(EI, 3)}";
            laEInumber.Location = new Point(4 * padding - distanceBetweenXandMiddle + ei, padding * 2);
            //es
            g.DrawLine(pen2, 4 * padding - distanceBetweenXandMiddle + es, pxGraph.ClientSize.Height / 4 * 3 + padding / 2, 4 * padding - distanceBetweenXandMiddle + es, padding * 2);
            laESnumber.Text = $"es = {Math.Round(ES, 3)}";
            laESnumber.Location = new Point(4 * padding - distanceBetweenXandMiddle + es, padding * 2);
        }

        private void SetControlsOnAnswerPanel()
        {
            int offset = 30;

            laChanges.Location = new Point(paAnswers.Width / 2 - laChanges.Width / 2, offset);
            tbAnswer.Width = paAnswers.Width - offset;
            tbAnswer.Location = new Point(paAnswers.Width / 2 - tbAnswer.Width / 2, laChanges.Location.Y + laChanges.Height + offset);
            tbAnswer.Text = $"Для исключения неисправимого брака необходимо осуществить подналадку. " +
                $"Нужно сместить наладочный размер x̄ на {Math.Round((EI - (X - 3 * Sigma)), 2)} мм до x̄ = {Xnew} мм." + Environment.NewLine +
                $"es - поле допуска исправимого брака;" + Environment.NewLine + 
                $"ei - поле допуска неисправимого брака;" + Environment.NewLine +
                $"x̄ - наладочный размер;" + Environment.NewLine +
                $"σ - среднее квадратичное отклонение случайной.";

            buSuitableRight.Location = new Point(offset, tbAnswer.Location.Y + tbAnswer.Height + offset);
            buUsefulRight.Location = new Point(offset, buSuitableRight.Location.Y + offset);
            buSuitableRight.Text = $"Годных деталей: {Math.Round(DeterminationOfSuitable(Xnew), 2)} %";
            buUsefulRight.Text = $"Исправимого брака: {Math.Round(DeterminationOfUseful(Xnew), 2)} %";

            buBuildNew.Location = new Point(paAnswers.Width / 2 - buBuildNew.Width / 2, buUsefulRight.Location.Y + buUsefulRight.Height + offset);
        }

        private void CalcOffsetX()
        {
            Xnew = X - (X + 3 * Sigma - ES);
        }

        private void BuBuild_Click(object sender, EventArgs e)
        {
            ConvertValuesFromTB();
            if (CheckParams())
            {
                DeterminationOfUseful(X);
                DeterminationOfFailty(X);
                DeterminationOfSuitable(X);
                ChangeResults();
                CalcOffsetX();
                SetControlsOnAnswerPanel();
                CreateGraph();
            }
        }
        private void BuBuildNew_Click(object sender, EventArgs e)
        {
            X = Xnew;
            CreateGraph();
        }

        private double DeterminationOfUseful(double X)
        {
            useful = (F((EI - X) / Sigma) - F((X - 3 * Sigma - X) / Sigma)) * 100;
            useful = useful > 0 ? useful : 0;
            return useful;
        }

        private double DeterminationOfFailty(double X)
        {
            faulty = (F((X + 3 * Sigma - X) / Sigma) - F((ES - X) / Sigma)) * 100;
            faulty = faulty > 0 ? faulty : 0;
            return faulty;
        }

        private double DeterminationOfSuitable(double X)
        {
            suitable = (F((ES - X) / Sigma) - F((EI - X) / Sigma)) * 100;
            suitable = suitable > 0 ? suitable : 0;
            return suitable;
        }

        //метод для вычисления значения интеграла по формуле Симпсона
        private static double Simpson(Func<double, double> f, double a, double b, int n)
        {
            var h = (b - a) / n;
            var sum1 = 0d;
            var sum2 = 0d;
            for (var k = 1; k <= n; k++)
            {
                var xk = a + k * h;
                if (k <= n - 1)
                {
                    sum1 += f(xk);
                }

                var xk_1 = a + (k - 1) * h;
                sum2 += f((xk + xk_1) / 2);
            }

            var result = h / 3d * (1d / 2d * f(a) + sum1 + 2 * sum2 + 1d / 2d * f(b));
            return result;
        }

        double f(double x)
        {
            return Math.Pow(Math.E, (-x * x / 2));
        }
        private double F(double temp)
        {
            double probability;
            probability = 1 / Math.Sqrt(2 * Math.PI) * Simpson(f, 0, temp, 1000);
            return probability;
        }

        private double functionGraph(double temp)
        {
            return 1 / (Sigma * Math.Sqrt(2 * Math.PI)) * Math.Pow(Math.E, Math.Pow(temp - X, 2) / (-2 * Sigma * Sigma));
        }

        private void ConvertValuesFromTB()
        {
            EI = Convert.ToDouble(tbEI.Text);
            ES = Convert.ToDouble(tbES.Text);
            X = Convert.ToDouble(tbX.Text);
            Sigma = Convert.ToDouble(tbSigma.Text);
        }

        private void SetParamsOnPanel()
        {
            int offset = 30;
            laTitleParams.Location = new Point(paParams.Width / 2 - laTitleParams.Width / 2, offset);

            laES.Location = new Point(offset, laTitleParams.Location.Y + offset * 2);
            tbES.Location = new Point(offset + laES.Width + 20, laES.Location.Y - (tbES.Height - laES.Height) / 2);
            laESmm.Location = new Point(tbES.Location.X + tbES.Width + 20, laES.Location.Y);

            laEI.Location = new Point(offset, laES.Location.Y + offset * 2);
            tbEI.Location = new Point(offset + laES.Width + 20, laEI.Location.Y - (tbEI.Height - laEI.Height) / 2);
            laEImm.Location = new Point(tbES.Location.X + tbES.Width + 20, laEI.Location.Y);

            laX.Location = new Point(offset, laEI.Location.Y + offset * 2);
            tbX.Location = new Point(offset + laES.Width + 20, laX.Location.Y - (tbX.Height - laX.Height) / 2);
            laXmm.Location = new Point(tbES.Location.X + tbES.Width + 20, laX.Location.Y);

            laSigma.Location = new Point(offset, laX.Location.Y + offset * 2);
            tbSigma.Location = new Point(offset + laES.Width + 20, laSigma.Location.Y - (tbSigma.Height - laSigma.Height) / 2);
            laSigmamm.Location = new Point(tbES.Location.X + tbES.Width + 20, laSigma.Location.Y);

            buBuild.Location = new Point(paParams.Width / 2 - buBuild.Width / 2, laSigma.Location.Y + tbSigma.Height + offset);

            laTitleValue.Location = new Point(paParams.Width / 2 - laTitleValue.Width / 2, buBuild.Location.Y + buBuild.Height + offset);

            laSuitableParts.Location = new Point(offset, laTitleValue.Location.Y + laTitleValue.Height + offset);
            laFaulty.Location = new Point(offset, laSuitableParts.Location.Y + offset);
            laUseful.Location = new Point(offset, laFaulty.Location.Y + offset);

            ChangeResults();
        }

        private void ChangeResults()
        {
            laSuitableParts.Text = $"Годных деталей: {Math.Round(suitable, 2)} %";
            laFaulty.Text = $"Неисправимого брака: {Math.Round(faulty, 2)} %";
            laUseful.Text = $"Исправимого брака: {Math.Round(useful, 2)} %";
        }
    }
}
