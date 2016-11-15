using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace CompresorLZW
{
    public partial class frmMain : Form
    {
        List<string> filesToAnalize = new List<string>();
        List<int> valoresResultados = new List<int>() 
        {
            {0},{0},{0},{0},{0},{0},{0},{0},{0},{0}
        };
        //los resultados, van a ir en el siguiente orden:
        // 0: cantidad de ciclos anidados
        // 1: mayor profundidad
        // 2: cantidad de instrucciones dentro de ciclos
        // 3: cantidad de funcoines definidas
        // 4: Cantidad de funciones recursivas simples: 
        // 5: Cantidad de funciones recursivas compuestas: 
        // 6: cantidad de instrucciones dentro de funcoines recursivas
        // 7: cantidad de instrucciones totales del programa
        // 8: cantidad de instrucciones en comentario
        // 9: cantidad de instrucciones NO dentro de funciones
        List<bool> opcionesConfiguracion = new List<bool>()
        {
            {true},{true},{true},{true},{true},{true},{true},{true},{true},{true}
        };
        // mismo orde a valores resultados

        public frmMain()
        {
            InitializeComponent();
            if (@Properties.Settings.Default.tvMainRootPath == "")
            {
                Properties.Settings.Default.tvMainRootPath = @"C:\Users\Public\Downloads";
                Properties.Settings.Default.Save();
            }
            refreshTvMain();

        }

        /// <summary>
        /// Agrega el primer elemento la raíz del treeview (y todos sus elementos)
        /// llamando a la función CreateDirectoryNode. Realiza las validaciones de acceso y
        /// existencia del directorio
        /// </summary>
        /// <param name="treeView">TreeView a llenar.</param>
        /// <param name="path">Directorio base.</param>
        private void ListDirectory(TreeView treeView, string path)
        {
            treeView.Nodes.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            if (rootDirectoryInfo.Exists)
            {
                try
                {
                    treeView.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo));
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.ToString());
                    MessageBox.Show("El directorio \"" + @path + "\" o parte de este, no se ha podido acceder. Seleccione otro directorio.", "Directorio no encontrado");
                    changeTvMainRootPath();
                    this.Focus();
                }
            }
            else
            {
                MessageBox.Show("El directorio \"" + @path + "\" no se ha encontrado. Seleccione otro directorio.", "Directorio no encontrado");
                changeTvMainRootPath();
                this.Focus();
            }
        }

        /// <summary>
        /// Agrega al treeView todos los subdirectorios del directorio base.
        /// Únicamente agrega los archivos con extensión .py 
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <returns>TreeNode</returns>
        private static TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeNode(directoryInfo.Name);
            foreach (var directory in directoryInfo.GetDirectories())
                directoryNode.Nodes.Add(CreateDirectoryNode(directory));
            foreach (var file in directoryInfo.GetFiles())
            {
                //if (file.Extension == ".py")
                //{
                    TreeNode node = new TreeNode(file.Name);
                    node.Tag = file.FullName;
                    directoryNode.Nodes.Add(node);
                //}
            }
            return directoryNode;
        }

        /// <summary>
        /// Evento de selección de un checkbox. 
        /// </summary>
        private void HandleOnTreeViewAfterCheck(Object sender,TreeViewEventArgs e)
        {
            CheckTreeViewNode(e.Node, e.Node.Checked);
        }

        /// <summary>
        /// Selecciona o remueve la selección del checkbox de los hijos de un nodo
        /// </summary>
        /// <param name="node">Nodo.</param>
        /// <param name="isChecked">Seleccionar o quitar selección</param>
        private void CheckTreeViewNode(TreeNode node, Boolean isChecked)
        {
            foreach (TreeNode item in node.Nodes)
            {
                item.Checked = isChecked;
                if (item.Nodes.Count > 0)
                {
                    this.CheckTreeViewNode(item, isChecked);
                }
            }
        }

        /// <summary>
        /// Evento del botón Actualizar
        /// </summary>
        private void btnRefreshTree_Click(object sender, EventArgs e)
        {
            refreshTvMain();
        }

        /// <summary>
        /// Evento que abre el form de configuracion con los checkboxes
        /// </summary>
        private void configuracionToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// abre la configuracion de los checkboxes
        /// </summary>
        private void abrirConfiguracion()
        {

            using (var s = new frmSettings(opcionesConfiguracion))
            {

                s.Location = this.Location;
                s.StartPosition = FormStartPosition.Manual;
                s.FormClosing += delegate { this.Enabled = true; };
                //s.Show();
                this.Enabled = false;
                var result = s.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var val = s.opcionesConfiguracion;
                    opcionesConfiguracion = val;
                    Properties.Settings.Default.cantCiclosAnidados = opcionesConfiguracion[0];
                    Properties.Settings.Default.mayorProfundidad = opcionesConfiguracion[1];
                    Properties.Settings.Default.cantInstruccionesDentroCiclos = opcionesConfiguracion[2];
                    Properties.Settings.Default.cantFuncionesDef = opcionesConfiguracion[3];
                    Properties.Settings.Default.cantFuncRecSimp = opcionesConfiguracion[4];
                    Properties.Settings.Default.cantFuncRecComp = opcionesConfiguracion[5];
                    Properties.Settings.Default.cantFuncInFuncRec = opcionesConfiguracion[6];
                    Properties.Settings.Default.cantInsTot = opcionesConfiguracion[7];
                    Properties.Settings.Default.cantInsComent = opcionesConfiguracion[8];
                    Properties.Settings.Default.cantInsNOTinFunc = opcionesConfiguracion[9];
                    Properties.Settings.Default.Save();
                }

            }
        }
        /// <summary>
        /// Evento del elemento Salir del menú Archivo de la barra de menús principal.
        /// </summary>
        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Evento del elemento Ruta inicial del menú Archivo de la barra de menús principal.
        /// </summary>
        private void rutaInicialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTvMainRootPath();
        }

        /// <summary>
        /// Actualiza el árbol del TreeView principal
        /// </summary>
        /// <param name="path">Directorio raíz.</param>
        private void refreshTvMain(string path)
        {
            //En el solution explirer, en Properties//Settings.settings <- !double click! -
            //se pueden crear nuevas opciones a guardar. Muy útil para guardar las configuraciones -
            //de los indicadores.
            Properties.Settings.Default.tvMainRootPath = @path;  //El @ es para qie incluya los backslashes en el string
            Properties.Settings.Default.Save(); //al cambiar una configuración, es necesario guardarlas.
            refreshTvMain();
        }

        /// <summary>
        /// Actualiza el árbol del TreeView principal
        /// </summary>
        private void refreshTvMain()
        {
            ListDirectory(tvMain, @Properties.Settings.Default.tvMainRootPath);
            tvMain.GetNodeAt(0, 0).Expand();
        }

        /// <summary>
        /// Evento del botón Analizar
        /// </summary>
        private void btnAnalizar_Click(object sender, EventArgs e)
        {
            analizar();
        }

        /// <summary>
        /// metodo que ejecuta el proceso para analizar los archivos y adjuntar el resultado al richTextBox1
        /// </summary>
        private void analizar()
        {
            while (!File.Exists(@Properties.Settings.Default.astToXML)) //Mientras no encuentre el archivo, pedirá que lo busque en el fileDialog
            {
                MessageBox.Show("No se ha encontrado el archivo astToXML.py. Seleccione la ubicación de astToXML.py a continuación.", "Archivo no encontrado no encontrado");
                findAstToXML();
            }
            filesToAnalize.Clear();
            clearValoresResultados();

            GetCheckedNodes(tvMain.Nodes);
            foreach (string file in filesToAnalize)
            {
                run_cmd(@Properties.Settings.Default.astToXML, file); //agrega cada xml a la lista fileResults
            }

            richTextBox1.Text = "Archivos py analizados: " + filesToAnalize.Count() + "\n";
            if (filesToAnalize.Count() > 0)
            {
                richTextBox1.AppendText(generateResultString()); //Solo para debug imprime toda la lista de resultados en el richTextBox1
            }
        }

        /// <summary>
        /// pone todos los valores en valoresResultados en 0 (les hace clear)
        /// </summary>
        private void clearValoresResultados()
        {
            for (int i = 0; i < valoresResultados.Count; i++)
            {
                valoresResultados[i] = 0;
            }

        }

        /// <summary>
        /// genera el string que se va a presentar en la pantalla como resultado del analisis,
        /// utiliza los valores en el atributo de la clase VarloesResultados para generar el string
        /// </summary>
        /// <returns></returns>
        private string generateResultString()
        {
            string a = "";
            if (opcionesConfiguracion[0])
            {
                a += "Cantidad de ciclos anidados: ";
                a += valoresResultados[0] + "\n";
            }
            
            if (opcionesConfiguracion[1])
            {
                a += "Mayor Profundidad: ";
                a += valoresResultados[1] + "\n";
            }
            if (opcionesConfiguracion[2])
            {
                a += "Cantidad de instrucciones dentro de ciclos: ";
                a += valoresResultados[2] + "\n";
            }
            if (opcionesConfiguracion[3])
            {
                a += "Cantidad de funciones definidas: ";
                a += valoresResultados[3] + "\n";
            }
            if (opcionesConfiguracion[4])
            {
                a += "Cantidad de funciones recursivas simples: ";
                a += valoresResultados[4] + "\n";
            }
            if (opcionesConfiguracion[5])
            {
                a += "Cantidad de funciones recursivas compuestas: ";
                a += valoresResultados[5] + "\n";
            }
            if (opcionesConfiguracion[6])
            {
                a += "Cantidad de instrucciones dentro de funcoines recursivas: ";
                a += valoresResultados[6] + "\n";
            }

            if (opcionesConfiguracion[7])
            {
                a += "Cantidad de instrucciones totales del programa: ";
                a += valoresResultados[7] + "\n";
            }

            if (opcionesConfiguracion[8])
            {
                a += "Cantidad de instrucciones en comentario: ";
                a += valoresResultados[8] + "\n";
            }

            if (opcionesConfiguracion[9])
            {
                a += "Cantidad de instrucciones NO dentro de funciones: ";
                a += valoresResultados[9] + "\n";
            }
            return a;
        }

        /// <summary>
        /// Corre un script de python (pyFile) desde la consola de python.exe y retorna el resultado en un string.
        /// </summary>
        /// <param name="pyFile">El script de python a correr</param>
        /// <param name="args">Argumentos del script a correr</param>
        /// <returns>String</returns>
        private void run_cmd(string pyFile, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            while (!File.Exists(Properties.Settings.Default.python))
            {
                MessageBox.Show("No se ha encontrado el ejecutable python.exe. Seleccione la ubicación de python.exe a continuación.", "Archivo no encontrado");
                findPython();
            }
            start.FileName = Properties.Settings.Default.python; ;//cmd is full path to python.exe
            start.Arguments = "\""+@pyFile +"\" \""+ args+"\"";//args is path to .py file and any cmd line args
            ;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            string result = "";
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    result = reader.ReadToEnd(); ;
                }
            }

            if (result != "")
            {
                valoresResultados[0] += buscarAnidados(result);
                valoresResultados[1] += contarAnidados(result);
                valoresResultados[2] += contarInsDentroCiclos(result);
                valoresResultados[3] += cantidadDef(result);
                valoresResultados[4] += buscarFuncionesRecurs(result, 1);
                valoresResultados[5] += buscarFuncionesRecurs(result, 2);
                valoresResultados[6] += contarInstFuncionesRecurs(result);
                valoresResultados[7] += cantidadInstrucciones(result);
                valoresResultados[9] += cantidadNOdentroFunciones(result);
                return;
            }

            MessageBox.Show("El script \"astToXML.py\" falló al leer el archivo \"" + args + "\". \nEsto se debe a caracteres inválidos en el código fuente. Por lo tanto, este se ha excluido del análisis", "Error del Script");


        }

        /*
         * Cuenta (junto con su auxiliar) la cantidad de los ciclos anidados
         * param:
                * string datos: Cadena los datos del xml
         * return:
                * string
        */
        private int buscarAnidados(String datos)
        {
            int cant = 0;
            using (var reader = XmlReader.Create(new StringReader(datos))) 
            {
                while (reader.Read()) //lee el siguiente elemento del xml
                {
                    /*
                     * si es elemento (ej. <_list_element>, <something>, etc)
                     * y si el tipo de ese elemento es "_list_element"
                    */
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "_list_element")
                    {
                        //verifica que el nombre asignado a ese elemento, sea un ciclo (for, while)
                        if (reader.GetAttribute("_name") == "For" || 
                            reader.GetAttribute("_name") == "While")
                        {
                            //en caso de que si lo sea, busca en sus hijos por ciclos anidados

                                cant += buscarAnidadosAux(reader.ReadSubtree());
                        }
                    }
                }
            }
            return cant;
        }

        /// <summary>
        /// busca ciclos anidados y los ciclos anidados dentro de ellos
        /// </summary>
        /// <param name="subTree"></param>
        /// <returns></returns>
        private int buscarAnidadosAux(XmlReader subTree)
        {
            int cant = 0;
            subTree.Read(); //lee el siguiente nodo, esto para que no lea el mismo nodo 2 veces 
                            //(en el metodo anterior y este)
            while (subTree.Read())
            {
                
                if (subTree.NodeType == XmlNodeType.Element && subTree.Name == "_list_element")
                {
                   
                    if (subTree.GetAttribute("_name") == "For" ||
                        subTree.GetAttribute("_name") == "While")
                    {
                        //System.Diagnostics.Debug.WriteLine(subTree.Name + " " + subTree.GetAttribute("_name") +
                        //" " + subTree.GetAttribute("lineno") + "\n");
                        cant += 1;
                        cant += buscarAnidadosAux(subTree);
                    }
                }
            }
            return cant;
        }

        /*
         * Cuenta (junto con su auxiliar) la profundidad de los ciclos anidados
         * param:
                * string datos: Cadena los datos del xml
         * return:
                * string
        */
        private int contarAnidados(String datos)
        {
            int mayor = 0;
            int temp = 0;
            using (var reader = XmlReader.Create(new StringReader(datos)))
            {
                while (reader.Read()) //lee el siguiente elemento del xml
                {
                    /*
                     * si es elemento (ej. <_list_element>, <something>, etc)
                     * y si el tipo de ese elemento es "_list_element"
                    */
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "_list_element")
                    {
                        //verifica que el nombre asignado a ese elemento, sea un ciclo (for, while)
                        if (reader.GetAttribute("_name") == "For" ||
                            reader.GetAttribute("_name") == "While")
                        {
                            //en caso de que si lo sea, busca en sus hijos por un ciclo anidado
                            temp = contarAnidadosAux(reader.ReadSubtree());
                            if (temp>mayor)
                            {
                                mayor = temp;
                            }
                        }
                    }
                }
            }
            return mayor;
        }
        private int contarAnidadosAux(XmlReader subTree)
        {
            subTree.Read(); //lee el siguiente nodo, esto para que no lea el mismo nodo 2 veces 
                            //(en el metodo anterior y este)
            int temp = 0;
            int mayor = 0;
            while (subTree.Read())
            {

                if (subTree.NodeType == XmlNodeType.Element && subTree.Name == "_list_element")
                {

                    if (subTree.GetAttribute("_name") == "For" ||
                        subTree.GetAttribute("_name") == "While")
                    {
                        //System.Diagnostics.Debug.WriteLine(subTree.Name + " " + subTree.GetAttribute("_name") +
                        //" " + subTree.GetAttribute("lineno") + "\n");
                        temp = contarAnidadosAux(subTree.ReadSubtree()) + 1;
                        if (temp > mayor)
                        {
                            mayor = temp;
                        }
                    }
                }
            }
            return mayor;
        }

        /*
         * Cuenta (junto con su auxiliar) la cantidad de instrucciones, incluidos condicionales, 
        excepto ciclos anidados, dentro de ciclos
         * param:
                * string datos: Cadena los datos del xml
         * return:
                * string
        */
        private int contarInsDentroCiclos(String datos)
        {
            int cant = 0;
            using (var reader = XmlReader.Create(new StringReader(datos)))
            {
                while (reader.Read()) //lee el siguiente elemento del xml
                {
                    /* si es elemento (ej. <_list_element>, <something>, etc)
                     y si el tipo de ese elemento es "_list_element"
                    */
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "_list_element")
                    {
                        //verifica que el nombre asignado a ese elemento, sea un ciclo (for, while)
                        if (reader.GetAttribute("_name") == "For" ||
                            reader.GetAttribute("_name") == "While")
                        {
                            //en caso de que si lo sea, cuenta la cantidad de instrucciones que hay dentro
                            //de ese ciclo
                            System.Diagnostics.Debug.WriteLine(reader.Name + " " + reader.GetAttribute("_name") +
                            " " + reader.GetAttribute("lineno") + " " + reader.NodeType + "\n");

                            cant += contarInsDentroCiclosAux(reader.ReadSubtree());
                        }
                    }
                }
            }
            return cant;
        }
        private int contarInsDentroCiclosAux(XmlReader subTree)
        {
            subTree.Read(); //lee el siguiente nodo, esto para que no lea el mismo nodo 2 veces 
                            //(en el metodo anterior y este)
            int cant = 0;
            while (subTree.Read())
            {

                if (subTree.NodeType == XmlNodeType.Element && 
                            (subTree.Name == "_list_element" || subTree.Name == "value"))
                {

                    if (subTree.GetAttribute("_name") == "Assign" || subTree.GetAttribute("_name") == "BinOp" ||
                        subTree.GetAttribute("_name") == "If" || subTree.GetAttribute("_name") == "Return")
                    {
                        //System.Diagnostics.Debug.WriteLine(subTree.Name + " " + subTree.GetAttribute("_name") +
                        //" " + subTree.GetAttribute("lineno") + "\n");
                        cant ++;
                    }
                }
            }
            return cant;
        }

        /// <summary>
        /// Cuenta la cantidad de funciones definidas dentro del programa
        /// </summary>
        /// <param name="datos">Cadena con los datos del xml</param>
        /// <returns></returns>
        private int cantidadDef(String datos)
        {
            int cant = 0;
            using (var reader = XmlReader.Create(new StringReader(datos)))
            {
                while (reader.Read()) //lee el siguiente elemento del xml
                {
                    /*
                     * si es elemento (ej. <_list_element>, <something>, etc)
                     * y si el tipo de ese elemento es "_list_element"
                    */
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "_list_element")
                    {
                        //verifica que el nombre asignado a ese elemento, sea un ciclo (for, while)
                        if (reader.GetAttribute("_name") == "FunctionDef")
                        {
                            cant += 1;
                        }
                    }
                }
            }
            return cant;
        }

        /// <summary>
        /// Cuenta la cantidad de funciones recursivas en el archivo de python
        /// </summary>
        /// <param name="datos"> Archivo xml representado en string </param>
        /// <param name="seleccion"> Selecciona que tipo de funciones recursivas desea mostrar.\n1=Recursion Simple, 2=Recursion Multiple, otro=Ambas</param>
        /// <returns></returns>
        int buscarFuncionesRecurs(String datos, int seleccion)
        {
            int cantTemp = 0;
            int simples = 0;
            int compuestas = 0;

            using (var reader = XmlReader.Create(new StringReader(datos)))
            {
                while (reader.Read()) //lee el siguiente elemento del xml
                {
                    /* si es elemento (ej. <_list_element>, <something>, etc)
                     y si el tipo de ese elemento es "_list_element"
                    */
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "_list_element")
                    {
                        //verifica que el nombre asignado a ese elemento, sea un ciclo (for, while)
                        if (reader.GetAttribute("_name") == "FunctionDef")
                        {
                            //en caso de que si lo sea, revisa dentro de la funcion si hay una llamada
                            //a ella misma.
                            cantTemp = buscarFuncionesRecursAux(reader.ReadSubtree(), reader.GetAttribute("name"));
                            if (cantTemp > 1)
                            {
                                compuestas++;
                            }
                            if (cantTemp == 1)
                            {
                                simples++;
                            }
                        }
                    }
                }
            }

            switch (seleccion)
            {
                case 1:
                    return simples;
                case 2:
                    return compuestas;
               default:
                    return simples + compuestas;
            }
        }

        int buscarFuncionesRecursAux(XmlReader subTree, string nombre)
        {
            int cant = 0;

            while (subTree.Read())
            {

                if (subTree.NodeType == XmlNodeType.Element &&
                            (subTree.Name == "func"))
                {

                    if (subTree.GetAttribute("id") == nombre)
                    {
                        //System.Diagnostics.Debug.WriteLine(subTree.Name + " " + subTree.GetAttribute("_name") +
                        //" " + subTree.GetAttribute("lineno") + "\n");
                        cant++;
                    }
                }
            }

            return cant;
        }

        int contarInstFuncionesRecurs(String datos)
        {
            int cant = 0;

            using (var reader = XmlReader.Create(new StringReader(datos)))
            {
                while (reader.Read()) //lee el siguiente elemento del xml
                {
                    /* si es elemento (ej. <_list_element>, <something>, etc)
                     y si el tipo de ese elemento es "_list_element"
                    */
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "_list_element")
                    {
                        //verifica que el nombre asignado a ese elemento, sea un ciclo (for, while)
                        if (reader.GetAttribute("_name") == "FunctionDef")
                        {
                            //en caso de que si lo sea, revisa dentro de la funcion si hay una llamada
                            //a ella misma.
                            cant += contarInstFuncionesRecursAux(reader.ReadSubtree(), reader.GetAttribute("name"));
                        }
                    }
                }
            }

            return cant;
        }

        int contarInstFuncionesRecursAux(XmlReader subTree, string nombre)
        {
            bool recursivo = false;
            int cant = 0;

            while (subTree.Read())
            {

                if (subTree.NodeType == XmlNodeType.Element &&
                            (subTree.Name == "_list_element" ||
                            subTree.Name == "value" ||
                            subTree.Name == "func"))
                {

                    if (subTree.GetAttribute("_name") == "Assign" || subTree.GetAttribute("_name") == "BinOp" ||
                        subTree.GetAttribute("_name") == "If" || subTree.GetAttribute("_name") == "Return")
                    {
                        //System.Diagnostics.Debug.WriteLine(subTree.Name + " " + subTree.GetAttribute("_name") +
                        //" " + subTree.GetAttribute("lineno") + "\n");
                        cant++;
                    }
                    if (subTree.GetAttribute("id") == nombre)
                    {
                        //System.Diagnostics.Debug.WriteLine(subTree.Name + " " + subTree.GetAttribute("_name") +
                        //" " + subTree.GetAttribute("lineno") + "\n");
                        recursivo = true;
                    }
                }
            }
            if (recursivo)
            {
                return cant;
            }
            return 0;
        }

        /// <summary>
        /// Cuenta la cantidad total de instrucciones dentro del programa
        /// </summary>
        /// <param name="datos">Cadena con los datos del xml</param>
        /// <returns>La cantidad de instrucciones dentro del programa</returns>
        private int cantidadInstrucciones(String datos)
        {
            int cant = 0;
            string nombreTemp = "";
            using (var reader = XmlReader.Create(new StringReader(datos)))
            {
                while (reader.Read()) //lee el siguiente elemento del xml
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.GetAttribute("_name") == "FunctionDef")
                        {
                            nombreTemp = reader.GetAttribute("_name");
                        }

                        if (reader.GetAttribute("_name") == "Assign" || reader.GetAttribute("_name") == "BinOp" ||
                            reader.GetAttribute("_name") == "If" || reader.GetAttribute("_name") == "Return" ||
                            reader.GetAttribute("_name") == "For" || reader.GetAttribute("_name") == "While" ||
                            reader.GetAttribute("_name") == "AugAssign" || reader.GetAttribute("id") == "print")
                        {
                            //System.Diagnostics.Debug.WriteLine(reader.Name + " " + reader.GetAttribute("_name") +
                            //" " + reader.GetAttribute("lineno") + " " + cant + "\n");
                            cant ++;
                        }

                        if (reader.Name == "func" && reader.GetAttribute("id") == nombreTemp)
                        {
                            System.Diagnostics.Debug.WriteLine(reader.Name + " " + reader.GetAttribute("_name") +
                            " " + reader.GetAttribute("lineno") + " " + cant + "\n");
                            cant++;
                        }
                    }
                }
            }
            return cant;
        }

        int cantidadNOdentroFunciones(string datos)
        {
            int cant = 0;

            using (var reader = XmlReader.Create(new StringReader(datos)))
            {
                while (reader.Read()) //lee el siguiente elemento del xml
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.GetAttribute("_name") == "FunctionDef")
                        {
                            cantidadNOdentroFuncionesAux(reader.ReadSubtree());
                        }
                        if (reader.GetAttribute("_name") == "Assign" || reader.GetAttribute("_name") == "BinOp" ||
                            reader.GetAttribute("_name") == "If" || reader.GetAttribute("_name") == "Return" ||
                            reader.GetAttribute("_name") == "For" || reader.GetAttribute("_name") == "While" ||
                            reader.GetAttribute("_name") == "AugAssign" || reader.GetAttribute("id") == "print")
                        {
                            //System.Diagnostics.Debug.WriteLine(reader.Name + " " + reader.GetAttribute("_name") +
                            //" " + reader.GetAttribute("lineno") + " " + cant + "\n");
                            cant++;
                        }
                    }
                }
            }
            return cant;
        }

        void cantidadNOdentroFuncionesAux(XmlReader subTree)
        {
            while (subTree.Read())
            {            }
        }
        

        /// <summary>
        /// Llena la lista filesToAnalize con los elementos del árbol seleccionado que tengan extensión .py
        /// </summary>
        /// <param name="nodes">Nodos a revisar.</param>
        public void GetCheckedNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode aNode in nodes)
            {
                if (aNode.Checked)
                    if (aNode.Tag != null)
                    {
                        if (aNode.Tag.ToString().EndsWith(".py"))
                        {
                            filesToAnalize.Add(@aNode.Tag.ToString());
                        }
                    }

                if (aNode.Nodes.Count != 0)
                    GetCheckedNodes(aNode.Nodes);
            }
        }

        /// <summary>
        /// Cambia el directorio raíz del TreeView Principal mediante un cuadro de diálogo de selección de folder.
        /// Si se presiona cancelar, el directorio no cambiará.
        /// </summary>
        private void changeTvMainRootPath()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != "")
            {
                refreshTvMain(fbd.SelectedPath);
            }            
        }

        /// <summary>
        /// Abre un cuadro de diálogo para encontrar el ejecutable python.exe y lo guarda
        /// en las configuraciones de la aplicacion (Properties.Settings.Default.python)
        /// </summary>
        private void findPython()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = "python*";
            openFileDialog1.Filter = "python.exe|*.exe";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;
            openFileDialog1.ShowDialog();
            Properties.Settings.Default.python = openFileDialog1.FileName;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Abre un cuadro de diálogo para encontrar el archivo astToXML.py y lo guarda
        /// en las configuraciones de la aplicacion (Properties.Settings.Default.astToXML)
        /// </summary>
        private void findAstToXML()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = "astToXML*";
            openFileDialog1.Filter = "astToXML.py|*.py";
            openFileDialog1.Multiselect = false;
            openFileDialog1.ShowDialog();
            Properties.Settings.Default.astToXML= openFileDialog1.FileName;
            Properties.Settings.Default.Save();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        //boton de analizar desde el menu de configuracion
        private void analizarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            analizar();
        }

        private void configuracionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            abrirConfiguracion();
        }
    }
}
