using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snail.ImageThumb
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始运行");
            ImageThumb it = new ImageThumb();
            it.Run();
            Console.WriteLine("结束运行，点击任意键退出。");
            Console.ReadKey();
        }

    }
}
