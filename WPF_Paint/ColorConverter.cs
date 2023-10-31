using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Paint
{
    internal static class ColorConverter
    {
        public static void RgbToHsv(int r, int g, int b, out double h, out double s, out double v)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;
            double max = Math.Max(rd, Math.Max(gd, bd)), min = Math.Min(rd, Math.Min(gd, bd));
            double delta = max - min;
            h = 0;
            if (delta != 0)
            {
                if (max == rd)
                    h = (gd - bd) / delta + (gd < bd ? 6 : 0);
                else if (max == gd)
                    h = (bd - rd) / delta + 2;
                else
                    h = (rd - gd) / delta + 4;
            }
            h *= 60;
            s = max == 0 ? 0 : delta / max;
            v = max;
        }

        public static void HsvToRgb(double h, double s, double v, out int r, out int g, out int b)
        {
            double rd, gd, bd;

            double hh = h / 60.0;
            int sector = (int)Math.Floor(hh);
            double frac = hh - sector;
            double p = v * (1 - s);
            double q = v * (1 - s * frac);
            double t = v * (1 - s * (1 - frac));

            switch (sector)
            {
                case 0:
                    rd = v;
                    gd = t;
                    bd = p;
                    break;
                case 1:
                    rd = q;
                    gd = v;
                    bd = p;
                    break;
                case 2:
                    rd = p;
                    gd = v;
                    bd = t;
                    break;
                case 3:
                    rd = p;
                    gd = q;
                    bd = v;
                    break;
                case 4:
                    rd = t;
                    gd = p;
                    bd = v;
                    break;
                default:
                    rd = v;
                    gd = p;
                    bd = q;
                    break;
            }
            r = (int)(rd * 255.0);
            g = (int)(gd * 255.0);
            b = (int)(bd * 255.0);
        }

        public static void RgbToCmyk(int r, int g, int b, out double c, out double m, out double y, out double k)
        {
            c = 1 - r / 255.0;
            m = 1 - g / 255.0;
            y = 1 - b / 255.0;
            k = Math.Min(c, Math.Min(m, y));
            if (k < 1.0)
            {
                double factor = 1 - k;
                c = (c - k) / factor;
                m = (m - k) / factor;
                y = (y - k) / factor;
            }
            else
            {
                c = 0;
                m = 0;
                y = 0;
            }
        }

        public static void CmykToRgb(double c, double m, double y, double k, out int r, out int g, out int b)
        {
            r = (int)((1 - c) * (1 - k) * 255.0);
            g = (int)((1 - m) * (1 - k) * 255.0);
            b = (int)((1 - y) * (1 - k) * 255.0);
        }

        public static string RgbToHex(int r, int g, int b)
        {
            return $"{r:X2}{g:X2}{b:X2}";
        }

        public static void HexToRgb(string hex, out int r, out int g, out int b)
        {
            // Remove the hash at the start if it's there
            hex = hex.TrimStart('#');

            // Convert each set of 2 characters into an integer
            r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }

    }

}
