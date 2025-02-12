using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chapter1
{
    class Program
    {
        static void Main(string[] args)
        {
            bool endApp = false;
            Console.WriteLine("Console Calculator in C#\r");
            Console.WriteLine("-----------------------\n");

            while(!endApp)
            {
                string numInput1 = "";
                string numInput2 = "";
                double result = 0;

                Console.Write("Type a number, and then press Enter : ");
                numInput1 = Console.ReadLine();

                double cleanNum1 = 0;
                while (!double.TryParse(numInput1, out cleanNum1))
                {
                    Console.Write("This is not valid input, Please enter and Integer...");
                    numInput1 = Console.ReadLine();
                }

                Console.Write("Type a number, and then press Enter : ");
                numInput2 = Console.ReadLine();

                double cleanNum2 = 0;
                while(!double.TryParse(numInput2,out cleanNum2))
                {
                    Console.Write("This is not valid input. Please enter an integer...");
                    numInput2 = Console.ReadLine();
                }

                Console.WriteLine("Choose and operator from the following list : ");
                Console.WriteLine("\ta - Add");
                Console.WriteLine("\ts - Subtract");
                Console.WriteLine("\tm - Multiply");
                Console.WriteLine("\td - Divide");
                Console.Write("Your option? ");

                string op = Console.ReadLine(); 
                try {
                    result = Calculator.DoOperation(cleanNum1, cleanNum2, op);
                    if (double.IsNaN(result))
                    {
                        Console.WriteLine("This operation will result in a mathmatica....");
                    }
                    else Console.WriteLine("Your result : {0:0.##}\n", result);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Oh no! An exception occured trying to do the...");
                }

                Console.WriteLine("------------------------\n");

                Console.Write("Press 'n' add Enter to close the app, or press any other key");

                if (Console.ReadLine() == "n") endApp = true;

                Console.WriteLine("\n");
                
                return;
            }
        }
    }
}
