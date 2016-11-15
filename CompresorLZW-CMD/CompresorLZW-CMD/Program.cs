using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompresorLZW_CMD
{
    class Program
    {
        static int Main(string[] args)
        {
            string generalErrorMessage = "Modo de uso: \ncomprimir <x:\\ruta y nombre del\\archivo.extension\ndescomprimir <x:\\ruta y nombre del\\archivo.extension>";
            if (args.Length == 0)
            {
                System.Console.WriteLine(generalErrorMessage);
                return 1;
            }
            else
            {
                if (args.Length >= 2)
                {
                    if (args[0] == "comprimir")
                    {
                        if (File.Exists(@args[1]))
                        {
                            Console.WriteLine("Found... Compressing");
                            return 0;
                        }
                        else
                        {
                            Console.WriteLine("File Not Found");
                            return 1;
                        }
                    }

                    else if (args[0] == "descomprimir")
                    {
                        if (File.Exists(@args[1]))
                        {
                            Console.WriteLine("Found... Decompressing");
                            return 0;
                        }
                        else
                        {
                            Console.WriteLine("File Not Found");
                            return 1;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Comando desconocido");
                        return 1;
                    }
                }
                else
                {
                    System.Console.WriteLine(generalErrorMessage);
                }
            }
            return 1;
        }
    }
}
