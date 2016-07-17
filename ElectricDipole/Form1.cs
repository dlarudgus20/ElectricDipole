using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ElectricDipole
{
	public partial class Form1 : Form
	{
		const float Coulomb = 9e9f;

		struct Dipole
		{
			public float quantity;
			public float mass;
			public PointF coord;
			public float length;
			public float angle;

			public PointF v;
			public float w;

			public PointF GetMoment()
			{
				return new PointF(
					quantity * (GetPlusCoord().X - GetMinusCoord().X),
					quantity * (GetPlusCoord().Y - GetMinusCoord().Y)
					);
			}

			public PointF GetPlusCoord()
			{
				PointF rot_coord = new PointF(length * (float)Math.Cos(angle) / 2, length * (float)Math.Sin(angle) / 2);
				return new PointF(
					coord.X + rot_coord.X,
					coord.Y + rot_coord.Y);
			}

			public PointF GetMinusCoord()
			{
				PointF rot_coord = new PointF(length * (float)Math.Cos(angle) / 2, length * (float)Math.Sin(angle) / 2);
				return new PointF(
					coord.X - rot_coord.X,
					coord.Y - rot_coord.Y);
			}
		}

		List<Dipole> _lstDipole = new List<Dipole>();

		const float _radius = 30;
		Pen _linePen = new Pen(Color.Black, 5);

		PointF _externalField = new PointF(10000, 0);

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			Random r = new Random();
			for (int x = 80; x <= 1400; x += 120)
			{
				for (int y = 80; y <= 800; y += 120)
				{
					float quantity = (float)(r.NextDouble() * 0.005 + 0.005) / 1000;
					float mass = (float)(r.NextDouble() * 1 + 1) / 1000;
					float angle = (float)(r.NextDouble() * (2 * Math.PI));
					CreateDipole(quantity, mass, new PointF(x, y), 80, angle);
				}
			}
		}

		private void Form1_Paint(object sender, PaintEventArgs e)
		{
			foreach (Dipole d in _lstDipole)
			{
				e.Graphics.DrawLine(_linePen, d.GetPlusCoord(), d.GetMinusCoord());
				e.Graphics.FillEllipse(Brushes.Red, GetRectOfCircle(d.GetPlusCoord(), _radius));
				e.Graphics.FillEllipse(Brushes.Blue, GetRectOfCircle(d.GetMinusCoord(), _radius));

				StringFormat stringFormat = new StringFormat();
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Center;
				e.Graphics.DrawString(
					$"Q={d.quantity * 1000000}uC\nm={d.mass * 1000}g",
					SystemFonts.StatusFont, Brushes.Black, d.coord, stringFormat);
			}
		}

		private void CreateDipole(float quantity, float mass, PointF coord, float length, float angle)
		{
			Dipole ret = new Dipole();
			ret.quantity = quantity;
			ret.mass = mass;
			ret.coord = coord;
			ret.length = length;
			ret.angle = angle;
			ret.v = new PointF(0, 0);
			ret.w = 0;
			_lstDipole.Add(ret);
		}

		private static RectangleF GetRectOfCircle(PointF center, float radius)
		{
			return new RectangleF(
				center.X - radius / 2,
				center.Y - radius / 2,
				radius,
				radius);
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			for (int i = 0; i < _lstDipole.Count; i++)
			{
				Dipole d = _lstDipole[i];
				float dt = timer1.Interval / 1000.0f;

				PointF d_moment = d.GetMoment();
				float torque = d_moment.X * _externalField.Y - d_moment.Y * _externalField.X;

				float i_moment = d.mass * d.length * d.length;
				float angular_a = torque / i_moment;

				d.angle += d.w * dt;
				d.coord = new PointF(d.coord.X + d.v.X * dt, d.coord.Y + d.v.Y * dt);

				d.w += angular_a * dt;

				_lstDipole[i] = d;
			}

			Invalidate();
		}
	}
}
