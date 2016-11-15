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

        /*
        IMPORTANTE 
        CODIGOS DE ERROR DE RETORNO:
        formato:   <numero retorno> = <motivo por el que es lanzado el código>
        ---
                -2 = se introdujeron menos de 2 argumentos
                -1 = Comando desconocido: args[0] != "comprimir" && args[0] != "descomprimir"
                 0 = TODO BIEN
        //LZW.cs 1 = IOException: No se puede abrir el archivo, principalmente está en uso
        //LZW.cs 2 = FormatException: El archivo no tiene números, por lo tanto no se puede descomprimir
                 3 = File.Exists == false: El archivo que se proporcionó en la línea de comandos no existe (args[1])
        //LZW.cs 4 = UnauthorizedAccessException: No tiene privilegios de admin

        */
        string C_EXT = ".lzw";
        string D_EXT = ".dlzw";
        string generalErrorMessage = "Modo de uso: \ncomprimir <x:\\ruta y nombre del\\archivo.extension\ndescomprimir <x:\\ruta y nombre del\\archivo.extension>";
            
            if (args.Length >= 2)
            {
                if (args[0] == "comprimir")
                {
                    if (File.Exists(@args[1]))
                    {
                        //Console.WriteLine("Found... Compressing");
                        int runCode = LZW.compress(args[1]);
                        if (runCode == 0)
                        {
                            Console.WriteLine(args[1] + ": comprimido!");
                            Console.WriteLine("Se ha generado el archivo: " + LZW.getFileName(args[1], C_EXT));
                            return runCode;
                        }
                        else if (runCode == 1)
                        {
                            Console.WriteLine("Error, el archivo " +args[1] + " está actualmente en uso.");
                            return runCode;
                        }
                        else
                        {
                            Console.WriteLine("Error, no tiene suficentes permisos para acceder al archivo");
                            return runCode;
                        }
                            
                    }
                    else
                    {
                        Console.WriteLine("File Not Found");
                        return 3;
                    }
                }

                else if (args[0] == "descomprimir")
                {
                    if (File.Exists(@args[1]))
                    {
                        int runCode = LZW.decompress(@args[1]);
                        if (runCode == 0)
                        {
                            Console.WriteLine(args[1] + ": descomprimido!");
                            Console.WriteLine("Se ha generado el archivo: " + LZW.getFileName(args[1], D_EXT));
                            return runCode;
                        }
                        else if (runCode == 1)
                        {
                            Console.WriteLine("Error, el archivo " + args[1] + "  está actualmente en uso.");
                            return runCode;
                        }
                        else if (runCode == 2)
                        {
                            Console.WriteLine("Error, el archivo " + args[1] + " no contiene datos descomprimibles.");
                            return runCode;
                        }
                        else
                        {
                            Console.WriteLine("Error, no tiene suficentes permisos para acceder al archivo");
                            return runCode;
                        }

                    }
                    else
                    {
                        Console.WriteLine("File Not Found");
                        return 3;
                    }
                }
                else
                {
                    Console.WriteLine("Comando desconocido");
                    return -1;
                }
            }
            else
            {
                System.Console.WriteLine(generalErrorMessage);
                return -2;
            }
        }
        

    }
}
