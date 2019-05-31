using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Report.RDLReference
{
    public class PrintBarCode
    {
        //public Bitmap generateBarCode()
        //{
        //    Bitmap b = new Bitmap(200, 200);
        //    Graphics g = Graphics.FromImage(b);
        //    Font font = new Font("Code39AzaleaRegular2", 32);
        //    g.DrawString("123456", font, Brushes.Black, new PointF(100, 100));
        //    return b;
        //}
        public string generateBarCode()
        {
            return "1232434";
        }

        public string generateBarCode2(string value)
        {
            return value+"1232434";
        }
    }
}
