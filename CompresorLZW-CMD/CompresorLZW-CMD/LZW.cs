using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompresorLZW_CMD
{
    public static class LZW
    {
        private static readonly string  C_EXT = ".lzw";
        private static readonly string D_EXT = ".dlzw";
        private static List<int> CompressAUX(string uncompressed)
        {
            // build the dictionary
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(((char)i).ToString(), i);

            string w = string.Empty;
            List<int> compressed = new List<int>();

            foreach (char c in uncompressed)
            {
                string wc = w + c;
                if (dictionary.ContainsKey(wc))
                {
                    w = wc;
                }
                else
                {
                    // write w to output
                    compressed.Add(dictionary[w]);
                    // wc is a new sequence; add it to the dictionary
                    dictionary.Add(wc, dictionary.Count);
                    w = c.ToString();
                }
            }

            // write remaining output if necessary
            if (!string.IsNullOrEmpty(w))
                compressed.Add(dictionary[w]);

            return compressed;
        }

        private static string DecompressAUX(List<int> compressed)
        {
            // build the dictionary
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(i, ((char)i).ToString());

            string w = dictionary[compressed[0]];
            compressed.RemoveAt(0);
            StringBuilder decompressed = new StringBuilder(w);

            foreach (int k in compressed)
            {
                string entry = null;
                if (dictionary.ContainsKey(k))
                    entry = dictionary[k];
                else if (k == dictionary.Count)
                    entry = w + w[0];

                decompressed.Append(entry);

                // new sequence; add it to the dictionary
                dictionary.Add(dictionary.Count, w + entry[0]);

                w = entry;
            }

            return decompressed.ToString();
        }


        public static string getFileName(string fileURL, string extension)
        {
            string newFilename = fileURL;
            int contador = 1;
            while (File.Exists(newFilename + extension))
            {
                newFilename = fileURL + "(" + contador.ToString() + ")";
                contador++;
            }
            return newFilename + extension;
        }

        private static String convertIntArrayToHex(List<int> intArray)
        {
            String result = "";
            foreach (int number in intArray)
            {
                result += number.ToString("X") + " ";
            }
            return result.TrimEnd();
        }

            /*
            IMPORTANTE 
            CODIGOS DE ERROR DE RETORNO:
            formato:   <numero retorno> = <motivo por el que es lanzado el código>
            ---

                     0 = TODO BIEN
            //LZW.cs 1 = IOException: No se puede abrir el archivo, principalmente está en uso
            //LZW.cs 2 = FormatException: El archivo no tiene números, por lo tanto no se puede descomprimir
            //LZW.cs 4 = UnauthorizedAccessException: No tiene privilegios de admin
        
            */

        public static int compress(string fileURL)
        {
            string text = "";
            try
            {
                text = File.ReadAllText(fileURL);
                string compressed = convertIntArrayToHex(CompressAUX(text)).TrimEnd();
                string newFileURL = getFileName(fileURL, C_EXT);
                File.WriteAllText(newFileURL, compressed);
                return 0;
            }
            catch (IOException)
            {
                return 1;
            }
            catch (UnauthorizedAccessException)
            {
                return 4;
            }
            
        }

        public static int decompress(string fileURL)
        {
            string text = "";
            try
            {
                text = File.ReadAllText(fileURL);
                List<string> hexNumbers = text.Split(' ').ToList();
                List<int> numbers = new List<int>();
                foreach (string hexNum in hexNumbers)
                {
                    numbers.Add(Convert.ToInt32(hexNum, 16));
                }
                string decompressed = DecompressAUX(numbers);
                string newFileURL = getFileName(fileURL, D_EXT);
                File.WriteAllText(newFileURL, decompressed);
                return 0;

            }
            catch (IOException)
            {
                return 1;
            }
            
            catch (FormatException e)
            {
                return 2;
            }

            catch (UnauthorizedAccessException)
            {
                return 4;
            }

        }
    }
}
