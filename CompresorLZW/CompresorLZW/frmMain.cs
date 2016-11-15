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
        List<string> filesResult = new List<string>();
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
            try
            {
                tvMain.GetNodeAt(0, 0).Expand();
            }
            catch (Exception)
            {
                refreshTvMain(Properties.Settings.Default.tvMainRootPath);
            }
        }

        /// <summary>
        /// Evento del botón Comprimir
        /// </summary>
        private void btnAnalizar_Click(object sender, EventArgs e)
        {
            ejecutar(1);
            filesResult.Clear();
            refreshTvMain();
        }

        /// <summary>
        /// Evento del botón Descomprimir
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            ejecutar(2);
            filesResult.Clear();
            refreshTvMain();
        }

        /// <summary>
        /// metodo que ejecuta el proceso para analizar los archivos y adjuntar el resultado al richTextBox1
        /// </summary>
        private void ejecutar(int modo)
        {

            filesToAnalize.Clear();

            GetCheckedNodes(tvMain.Nodes);
            foreach (string file in filesToAnalize)
            {
                if (modo == 1)
                {
                    filesResult.Add(run_cmd("comprimir \"" + file +"\"")); 
                }
                else
                {
                    filesResult.Add(run_cmd("descomprimir " + file)); 
                }
            }

            //richTextBox1.Text = "Archivos py analizados: " + filesToAnalize.Count() + "\n";
            if (filesToAnalize.Count() > 0)
            {
                foreach (string result in filesResult)
                {
                    richTextBox1.AppendText(result+ "\n");
                }
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
            return a;
        }

        /// <summary>
        /// Corre un script de python (pyFile) desde la consola de python.exe y retorna el resultado en un string.
        /// </summary>
        /// <param name="args">Argumentos del script a correr</param>
        /// <returns>String</returns>
        private string run_cmd(string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            while (!File.Exists(Properties.Settings.Default.LZW_CMD))
            {
                MessageBox.Show("No se ha encontrado el ejecutable CompresorLZW-CMD.exe. Seleccione la ubicación de python.exe a continuación.", "Archivo no encontrado");
                findLZW_CMD();
            }
            start.FileName = Properties.Settings.Default.LZW_CMD; ;//cmd is full path to LZW-CMD.exe
            start.Arguments = args;//args is path to the file to compress
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
            return result;
        }
        

        /// <summary>
        /// Llena la lista filesToAnalize con los elementos del árbol seleccionado
        /// </summary>
        /// <param name="nodes">Nodos a revisar.</param>
        public void GetCheckedNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode aNode in nodes)
            {
                if (aNode.Checked)
                    if (aNode.Tag != null)
                    {
                        filesToAnalize.Add(@aNode.Tag.ToString());                        
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
        /// Abre un cuadro de diálogo para encontrar el ejecutable CompresorLZW_CMD.exe y lo guarda
        /// en las configuraciones de la aplicacion (Properties.Settings.Default.LZW_CMD)
        /// </summary>
        private void findLZW_CMD()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = "CompresorLZW-CMD*";
            openFileDialog1.Filter = "CompresorLZW-CMD.exe|*.exe";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;
            openFileDialog1.ShowDialog();
            Properties.Settings.Default.LZW_CMD = openFileDialog1.FileName;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Abre un cuadro de diálogo para encontrar el archivo astToXML.py y lo guarda
        /// en las configuraciones de la aplicacion (Properties.Settings.Default.astToXML)
        /// </summary>
        

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        //boton de analizar desde el menu de configuracion
        private void analizarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //analizar();
        }

        private void configuracionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            abrirConfiguracion();
        }


    }
}
