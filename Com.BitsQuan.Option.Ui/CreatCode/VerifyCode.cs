using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.CreatCode
{
    public class VerifyCode
    {

        public string checkCode = String.Empty;
        public byte[] BuildImg()
        {
            int number;

            System.Random random2 = new Random();
            for (int i = 0; i < 4; i++) //字符和数字的混合.长度为4。其实i的大小可以自由设置
            {
                number = random2.Next();
                checkCode += ((number % 2 == 0) ? (char)('0' + (char)(number % 10)) : (char)('A' + (char)(number % 26))).ToString();
            }
            System.Drawing.Bitmap image = new System.Drawing.Bitmap((int)Math.Ceiling((checkCode.Length * 14.5)), 22);//用指定的大小初始化Bitmap类创建图像对象
            Graphics g = Graphics.FromImage(image);//表示在指定的图像上写字
            try
            {
                Random random = new Random();
                //清空图片背景色
                g.Clear(Color.White);
                //画图片的背景噪音线
                for (int i = 0; i < 25; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }//用银色笔在图像区域内划线形成背景噪音线
                Font font = new System.Drawing.Font("Arial", 14, (System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic));//设置验证码的字体属性
                System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true);
                //用画笔画出高级的2D向量图形字体
                g.DrawString(checkCode, font, brush, 2, 2);//以字符串的形式输出
                //画图片的前景噪音点
                for (int i = 0; i < 100; i++)//i的大小可以自由设置
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);

                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                } //随机设置图像的像素
                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                //用银色笔画出边框线
                System.IO.MemoryStream ms = new System.IO.MemoryStream();//申明一个内存流对象
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms.ToArray();
            }
            finally
            {
                g.Dispose();//释放绘图对象
                image.Dispose();//释放图像对象
            }
        }


    }
}