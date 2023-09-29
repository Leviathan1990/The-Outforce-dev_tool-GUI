/*
 *              The Outforce - Developer tools
 *            Designed for the game "The Outforce"
 *     Part of the upcoming mod " Conflict in the Epsilon System".
 *              
 *              Product version : V2.6
 *              Date            : 2023.Aug.8.
 *              Moddb           : https://www.moddb.com/games/the-outforce
 *              Discord         : https://discord.gg/7RbzqN9
 *              Contact         : krisztiankispeti1990@gmail.com
 *              Developed by    : Krisztian Kispeti
 *              This file was last modified on: 2023.aug.23. 23:20PM
 */
using Microsoft.VisualBasic.Logging;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
using System.Linq.Expressions;
using NAudio.Wave;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.CodeDom.Compiler;
using System.Security;

namespace boxUnpackerGUI
{
    public partial class Form1 : Form
    {

        private WaveOutEvent waveOut;
        private AudioFileReader audioFile;

        private Dictionary<string, string> wordDetails = new Dictionary<string, string>();

        private List<BoxItem> toc = new List<BoxItem>();
        private string boxFileName = "";
        private BoxItem[] boxItems;
        private int numFiles;
        private string folderPath = "";

        private long totalSize = 0;

        private bool isImageDragging = false;
        private Point startPoint;
        private Point startLocation;



        public Form1()
        {
            InitializeComponent();
            InitializeWordDetails();

            waveOut = new WaveOutEvent();

            radioButton11.CheckedChanged += radioButton11_CheckedChanged;
            radioButton12.CheckedChanged += radioButton12_CheckedChanged;
            radioButton12.Checked = false;

        }
        //          KELL OMS EDITORHOZ!
        private bool IsReadableFile(string extension)
        {
            return extension.Equals(".TXT", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".OMS", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".CFG", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".AI", StringComparison.OrdinalIgnoreCase);
        }

        //Enable OMSEDITOR Highlighting
        private void EnableHighLighting()
        {
            OMSEDITOR.TextChanged -= OMSEDITOR_TextChanged;
            HighlightText();
            OMSEDITOR.TextChanged += OMSEDITOR_TextChanged;
        }
        //Disable OMSEDITOR Highlighting
        private void DisableHighlighting()
        {
            int selectionStart = OMSEDITOR.SelectionStart;
            int selectionLength = OMSEDITOR.SelectionLength;
            OMSEDITOR.SelectAll();
            OMSEDITOR.SelectionColor = Color.Black;
            OMSEDITOR.Select(selectionStart, selectionLength);
        }

        //Highlight texts to OMSEDITOR
        private void HighlightText()
        {
            string[] wordsToColorBlue = { "Create", "Env_", "int", "float", "string", "cost" };
            string[] wordsToColorGreen = { "asteroid_Chrystal_", "asteroid_Chrystal_1", "asteroid_Chrystal_2", "asteroid_Chrystal_3", "asteroid_Chrystal_4", "asteroid_Chrystal_5",
        "asteroid_Chrystal_6", "Asteroid_size6_green", "Asteroid_size6", "b_tutorial", "B_Skri", "Game_Resource_Spot", "Stone1", "Stone2", "Stone3", "b_mission", "b_tutorial", "planet", "MH_", "HM_", "Debris",
        "ssm_toroid_size_" };
            string[] wordsToColorRed = { "M_DefineStartPositions", "M_SetLayerPosition", "M_SelectLayer", "M_SetStartPosition" };

            int selectionStart = OMSEDITOR.SelectionStart;
            int selectionLength = OMSEDITOR.SelectionLength;
            OMSEDITOR.SelectAll();
            OMSEDITOR.SelectionColor = Color.Black;

            foreach (string word in wordsToColorBlue)
            {
                int startPos = 0;
                while ((startPos = OMSEDITOR.Text.IndexOf(word, startPos, StringComparison.CurrentCultureIgnoreCase)) != -1)
                {
                    OMSEDITOR.Select(startPos, word.Length);
                    OMSEDITOR.SelectionColor = Color.Blue;
                    startPos += word.Length;
                }
            }

            foreach (string word in wordsToColorGreen)
            {
                int startPos = 0;
                while ((startPos = OMSEDITOR.Text.IndexOf(word, startPos, StringComparison.CurrentCultureIgnoreCase)) != -1)
                {
                    OMSEDITOR.Select(startPos, word.Length);
                    OMSEDITOR.SelectionColor = Color.Green;
                    startPos += word.Length;
                }
            }

            foreach (string word in wordsToColorRed)
            {
                int startPos = 0;
                while ((startPos = OMSEDITOR.Text.IndexOf(word, startPos, StringComparison.CurrentCultureIgnoreCase)) != -1)
                {
                    OMSEDITOR.Select(startPos, word.Length);
                    OMSEDITOR.SelectionColor = Color.Red;
                    startPos += word.Length;
                }
            }

            OMSEDITOR.Select(selectionStart, selectionLength);
        }

        private OpenFileDialog ofd = new OpenFileDialog();

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The Outforce - developer tools\n Version: V2.0\n Developed by: Krisztian Kispeti.\n", "Product information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void closeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool IsImageFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp" || extension == ".gif";
        }

        private void Form1_Load(object sender, EventArgs e)
        {               // comboBox2 listBox1 items filter!
            comboBox2.Items.AddRange(new string[] { "", ".ai", ".bik", ".bin", ".bmp",".box", ".cfg",".cof",".gui",".GUI",".id",".jbf", ".jpg", ".OPF",
                ".mp3", ".oms",".scc", ".CBaseClass", ".CGridMember", ".CUnit", ".CUnitWeapon" });   // Case-sensitive!

            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
            radioButton12.Checked = false;

        }

        private void InitializeWordDetails()
        {
            // wordDetails loaded from data.words binary dat. container file.
            wordDetails.Add("M_Resources", "Max resources you can harvest on map\n usage: m_Resources(int);");
            wordDetails.Add("M_SetGameSceneRect", "Sets and allocates the game scene. Measured in world units\n usage: M_SetGameSceneRect(int x, int y);");
            wordDetails.Add("STARTMP3", "StartMP3(str FileName);\n The list of the available MP3 files are included with this program!");
            wordDetails.Add("si_Map_Description", "Short description of your map. \n usage: si_Map_Description(str description);");
            wordDetails.Add("si_Map_Author", "Author of the map. \n Usage: si_Map_Author(String Author name)\n max 8 characters long!");
            wordDetails.Add("si_Map_Players", "The number of players this map was designed for \n Usage: si_Map_Players(int NumPlayers);\n");
            wordDetails.Add("si_Map_Width", "The width of the selected map \n Usage: si_Map_Width(int Width);");
            wordDetails.Add("si_Map_Height", "The height of the selected map \n Usage: si_Map_Height(int Height);");
            wordDetails.Add("m_EditMode", "Sets the game in map editor mode \n usage: m_Editmode (bool);");
            wordDetails.Add("m_Devmap", "Goes into development mode of map specified \n Usage: m_Devmap mapname");
            wordDetails.Add("m_DevMapMode", "Sets or unsets outforce in development mode of map \n usage: m_Devmapmode = bool;");
            wordDetails.Add("M_SetStartPositionByMouse", "Sets the start position of a player by the mouse\n  usage: M_SetStartPositionByMouse();  ");
            wordDetails.Add("M_SetStartPosition", "Sets a start position of a player \n usage: M_SetStartPosition(int PlayerID, float X, float Z); ");
            wordDetails.Add("M_DefineStartPositions", "Defines a number of start positions \n usage: M_DefineStartPositions(int NumberOfPosition); ");
            wordDetails.Add("m_SetTimer", "Sets the countdown timer. \n usage: m_SetTimer(int seconds);");
            wordDetails.Add("M_SetLayerPosition", "Sets the position of the specified layer \n usage: M_SetLayerPosition(int Layer, float Position);");
            wordDetails.Add("M_SelectLayer", "Selects which layer to work with in map editor mode \n usage: M_SelectLayer(int Layer); ");
        }

        private void UpdateTotalSize()
        {
            long totalSize = 0;
            foreach (var item in listBox2.Items)
            {
                if (item is string file)
                {
                    string fullPath = Path.Combine(folderPath, file);
                    if (File.Exists(fullPath)) // Ellenõrizzük, hogy a fájl létezik-e
                    {
                        FileInfo fileInfo = new FileInfo(fullPath);
                        totalSize += fileInfo.Length;
                    }
                }
            }
            textBox7.Text = $"{totalSize / 1024} KB";
        }

        //          ÁT KELL NÉZNI, H. KELL-E EZ A KÓDRÉSZ!

        private void button2_Click(object sender, EventArgs e)
        {
            if (toc.Count > 0 && listBox1.SelectedIndex >= 0)
            {
                BoxItem selectedItem = toc[listBox1.SelectedIndex];
                string boxFileName = ofd.FileName;

                string outputDirectory = Path.GetDirectoryName(boxFileName) ?? "";

                Directory.CreateDirectory(outputDirectory);

                ExtractFileFromBox(boxFileName, selectedItem, outputDirectory);
                MessageBox.Show("Selected file successfully extracted!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("An unknown error has occured and I could not extract any files!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //      ÁT KELL NÉZNI, H. KELL-E EZ  SZAR KÓD!!
        private void ExtractSelectedFile(int selectedIndex)
        {
            if (selectedIndex < toc.Count)
            {
                BoxItem selectedItem = toc[selectedIndex];
                string boxFileName = ofd.FileName; // Replace 'ofd' with the actual OpenFileDialog object

                string outputDirectory = Path.GetDirectoryName(boxFileName) ?? string.Empty;
                outputDirectory = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(boxFileName) ?? "defaultDirectoryName");

                Directory.CreateDirectory(outputDirectory);

                ExtractFileFromBox(boxFileName, selectedItem, outputDirectory);
            }
        }

        private void ExtractFileFromBox(string boxFileName, BoxItem selectedItem, string outputDirectory)
        {
            using (FileStream fs = new FileStream(boxFileName, FileMode.Open))
            {
                fs.Seek(selectedItem.Offset, SeekOrigin.Begin);

                byte[] fileData = new byte[selectedItem.Size];
                fs.Read(fileData, 0, (int)selectedItem.Size);

                string outputPath = Path.Combine(outputDirectory, selectedItem.Filename.Replace('\\', Path.DirectorySeparatorChar));

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)); // Mappa létrehozása, ha még nem létezik

                File.WriteAllBytes(outputPath, fileData);
            }
        }
        private List<BoxItem> ExtractFilesFromBox(FileStream fs, out uint directoryOffset)
        {
            List<BoxItem> toc = new List<BoxItem>();

            byte[] numFilesBuffer = new byte[4];
            fs.Seek(-4, SeekOrigin.End);
            fs.Read(numFilesBuffer, 0, 4);
            directoryOffset = BitConverter.ToUInt32(numFilesBuffer, 0);

            fs.Seek(directoryOffset, SeekOrigin.Begin);
            byte[] numFilesBytes = new byte[4];
            fs.Read(numFilesBytes, 0, 4);
            uint numFiles = BitConverter.ToUInt32(numFilesBytes, 0);

            using (BinaryReader reader = new BinaryReader(fs))
            {
                for (int i = 0; i < numFiles; i++)
                {
                    BoxItem item = new BoxItem();
                    item.Filename = ReadNullTerminatedString(reader);
                    item.Offset = reader.ReadUInt32();
                    item.Size = reader.ReadUInt32();
                    item.IsImage = IsImageFile(item.Filename);
                    toc.Add(item);
                }
            }

            return toc;
        }

        private string ReadNullTerminatedString(BinaryReader reader)
        {
            List<byte> bytes = new List<byte>();
            byte b;

            while ((b = reader.ReadByte()) != 0)
            {
                bytes.Add(b);
            }

            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        private void aboutToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("\tThe Outforce - Developer tools. V2.6.\n Designed for the game, The Outforce \n Developed by: Krisztian Kispeti\n Iconset by :Icons8 com",
                "Product information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Update();
            listBox1.Refresh();

        }

        private async void openArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ofd.Title = "The Outforce - *.box& *.opf archive";
            ofd.Filter = "Box archive | *.box";   //ofd.Filter = "Box archive (*.box, *.opf)|*.box;*.opf"; 
            ofd.RestoreDirectory = true;
            ofd.FileName = ".box";
            ofd.InitialDirectory = "";


            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string boxFilePath = ofd.FileName;
                boxItems = LoadItemsFromBox(boxFilePath);

                LoadBoxItems(boxFilePath);

                boxFileName = ofd.FileName;


                try
                {
                    using (FileStream fs = new FileStream(boxFileName, FileMode.Open))
                    {
                        toc = ExtractFilesFromBox(fs, out uint directoryOffset);
                        textBox9.Text = directoryOffset.ToString();

                    }

                    listBox1.Items.Clear();
                    foreach (var item in toc)
                    {
                        listBox1.Items.Add(item.Filename);

                    }

                    UpdateFileInfo(boxFilePath);
                    textBox3.Text = $"{toc.Count} files";
                    MessageBox.Show("*.box archive has been opened and its content was loaded successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading *.box archive contents: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
            else
            {
                MessageBox.Show("An unknown error has occurred! Could not open *.box archive to read.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadBoxItems(string boxFilePath)
        {
            boxItems = LoadItemsFromBox(boxFilePath);
            listBox1.Items.Clear();
            listBox1.Items.AddRange(boxItems.Select(item => item.Filename).ToArray());
            numFiles = boxItems.Length;
            // UpdateFileInfo(boxFilePath);
        }

        private void UpdateFileInfo(string boxFilePath)
        {
            FileInfo fileInfo = new FileInfo(boxFilePath);
            textBox1.Text = Path.GetFileName(boxFilePath);
            textBox2.Text = $"{fileInfo.Length / 1024} KB"; // Betöltött fájl mérete KB-ban
            //  textBox3.Text = $"{toc.Count} files"; // Betöltött fájlok száma
        }
        private int GetNumberOfFilesInBox(string boxFilePath)
        {
            int numFiles = 0;

            using (FileStream fs = new FileStream(boxFilePath, FileMode.Open))
            {

            }
            return numFiles;
        }

        private BoxItem[] LoadItemsFromBox(string boxFilePath)
        {
            List<BoxItem> boxItems = new List<BoxItem>();

            return boxItems.ToArray(); // Visszatérés a betöltött elemekkel
        }

        //ListBox1-bõl kijelölt 1 elem kinyerése *.box archívumból
        private void extractSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            listBox1.ClearSelected(); // Töröljük az összes korábbi kijelölést

            string searchText = textBox4.Text;

            if (!string.IsNullOrEmpty(searchText))
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    if (listBox1.Items[i] is string item && item.Contains(searchText))
                    {
                        listBox1.SetSelected(i, true); // Kijelöljük a találatot
                        listBox1.TopIndex = i; // Görgetés a találatra
                        return; // Kilépünk, hogy csak az elsõ találatot jelölje ki
                    }
                }
            }
        }
        private void PerformSearchAndSelect()
        {
            listBox1.ClearSelected();

            string searchText = textBox4.Text;
            if (!string.IsNullOrEmpty(searchText))
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    if (listBox1.Items[i] is BoxItem item && item.Filename.Contains(searchText))
                    {
                        listBox1.SelectedIndex = i;
                        listBox1.TopIndex = i; // Görgetés, ha az elem kívül esik a látható területen
                        return; // Kilépünk, hogy csak az elsõ találatot jelölje ki
                    }
                }
            }
        }
        private byte[] ExtractImageBytesFromBox(uint offset, uint size)
        {
            using (FileStream fs = new FileStream(boxFileName, FileMode.Open))
            {
                fs.Seek(offset, SeekOrigin.Begin);

                byte[] imageBytes = new byte[size];
                int bytesRead = fs.Read(imageBytes, 0, (int)size);

                if (bytesRead == size)
                {
                    return imageBytes;
                }
                else
                {
                    return null; // Sikertelen olvasás esetén null visszatérés
                }
            }
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
        }
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //For image inspector

            if (listBox1.SelectedIndex >= 0)
            {
                BoxItem selectedItem = toc[listBox1.SelectedIndex];
                string extension = Path.GetExtension(selectedItem.Filename);

                if (selectedItem.IsImage) // Vizsgáld meg, hogy a kiválasztott elem képfájl-e
                {
                    byte[] imageBytes = ExtractImageBytesFromBox(selectedItem.Offset, selectedItem.Size);

                    if (imageBytes != null)
                    {
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            string imagePath = Path.Combine(folderPath, selectedItem.Filename);
                            pictureBox1.Image = Image.FromStream(ms);
                            textBox10.Text = Path.GetFileName(imagePath);
                        }
                    }
                    else
                    {
                        pictureBox1.Image = null;
                        textBox10.Clear();
                    }
                }
                else
                {
                    pictureBox1.Image = null;
                    textBox10.Clear();
                }

                OMSEDITOR.Clear();

            }

            //          |--->        End of for image inspector



            if (listBox1.SelectedIndex >= 0)
            {
                BoxItem selectedItem = toc[listBox1.SelectedIndex];
                long offset = selectedItem.Offset;

                textBox8.Text = offset.ToString(); // Offset kiíratása a textBox8-ba
            }
            else
            {
                textBox8.Clear();
            }

            if (listBox1.SelectedItem != null)
            {
                string selectedItem = listBox1.SelectedItem.ToString();
                string selectedFileName = Path.GetFileName(selectedItem);
                if (selectedItem.Equals("Units\\PackedProject.OPF", StringComparison.OrdinalIgnoreCase))

                {
                    toolStripStatusLabel2.Visible = true;

                }
                else
                {
                    toolStripStatusLabel2.Visible = false;
                }
            }

            if (listBox1.SelectedIndex >= 0)
            {
                BoxItem selectedItem = toc[listBox1.SelectedIndex];
                long sizeInBytes = selectedItem.Size;
                string sizeText;

                if (sizeInBytes > 1024)
                {
                    double sizeInKB = (double)sizeInBytes / 1024; ;
                    sizeText = $"{sizeInKB:N2} KB";
                }
                else
                {
                    sizeText = $"{sizeInBytes} bytes";
                }

                textBox5.Text = sizeText;

            }
            else
            {

                textBox5.Clear();
            }

        }
        private void button2_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofd2 = new OpenFileDialog();
            ofd2.Multiselect = true;

            if (ofd2.ShowDialog() == DialogResult.OK)
            {
                listBox2.Items.Clear();
                totalSize = 0;
                foreach (string filePath in ofd2.FileNames)
                {
                    string fileName = Path.GetFileName(filePath);
                    FileInfo fileInfo = new FileInfo(filePath);
                    totalSize += fileInfo.Length; //!
                    listBox2.Items.Add(fileName);
                }
                textBox7.Text = $"{totalSize / 1024} KB";
            }
        }
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        private void button3_Click_1(object sender, EventArgs e)
        {
            int selectedIndex = listBox2.SelectedIndex; // Elmentjük az aktuális kiválasztott indexet

            if (selectedIndex >= 0)
            {

                listBox2.Items.RemoveAt(selectedIndex);
                UpdateTotalSize();

                if (selectedIndex < listBox2.Items.Count) // Ellenõrizzük, hogy a kiválasztott index még érvényes-e
                {
                    listBox2.SelectedIndex = selectedIndex; // Visszaállítjuk a kiválasztott indexet

                }
                else if (listBox2.Items.Count > 0) // Ha nincs több elem, de még vannak elemek a listában
                {
                    listBox2.SelectedIndex = listBox2.Items.Count - 1; // Kiválasztjuk az utolsó elemet

                }

            }
            else
            {
                MessageBox.Show("Select a file to remove.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        //      *.box Archívum készítése Button-on keresztül. Fájlok listBox-ba való betöltése, "Create" buttonnal, textBox-al nevet adva *.box archívum elkészítése, kimentése...  


        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox6.Text))
            {
                MessageBox.Show("Archive filename can not be empty!\n Please add name to your *.box arcive with out extension!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("No files to archive.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string boxFileName = $"{textBox6.Text}.box";

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.FileName = boxFileName;
                sfd.Filter = "Box archive | *.box";
                if (sfd.ShowDialog() == DialogResult.OK)
                {

                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
                    using (BinaryWriter writer = new BinaryWriter(fs, Encoding.ASCII))  //Kódolás
                    {
                        foreach (string filePath in listBox2.Items)
                        {

                            byte[] fileData = File.ReadAllBytes(filePath);
                            string fileName = Path.GetFileName(filePath);
                            //writer.Write((uint)(fileName.Length));
                            writer.Write(Encoding.ASCII.GetBytes(fileName));
                            writer.Write((byte)0);

                            writer.Write((uint)(fileData.Length));
                            writer.Write(fileData);
                        }
                    }

                    MessageBox.Show("Archive created successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        // Minden fájl kinyerése a megnyitott *.box archívumból
        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {

            if (listBox2.SelectedIndex != 0)
            {

                listBox2.Items.Clear();
                UpdateTotalSize();
            }
            else
            {
                MessageBox.Show("Nothing to do now... No files are loaded.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
            pictureBox1.Refresh();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.Refresh();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBox1.Refresh();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox1.Refresh();
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image seleceted to display!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {

                //Ha üres a picturebox
                bool rbSelection = radioButton5.Checked;

                if (pictureBox1.Image != null && radioButton5.Checked == rbSelection)
                {
                    pictureBox1.Image.RotateFlip(RotateFlipType.Rotate90FlipX);
                    pictureBox1.Refresh();
                    pictureBox1.Update();
                }
            }
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image seleceted to display!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {

                //Ha üres a picturebox
                bool rbSelection = radioButton6.Checked;

                if (pictureBox1.Image != null && radioButton6.Checked == rbSelection)
                {
                    pictureBox1.Image.RotateFlip(RotateFlipType.Rotate180FlipX);
                    pictureBox1.Refresh();
                    pictureBox1.Update();
                }
            }
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image seleceted to display!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                //Ha üres a picturebox
                bool rbSelection = radioButton7.Checked;

                if (pictureBox1.Image != null && radioButton7.Checked == rbSelection)
                {
                    pictureBox1.Image.RotateFlip(RotateFlipType.Rotate270FlipX);
                    pictureBox1.Refresh();
                    pictureBox1.Update();
                }
            }
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image seleceted to display!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {

                //Ha üres a picturebox
                bool rbSelection = radioButton8.Checked;

                if (pictureBox1.Image != null && radioButton8.Checked == rbSelection)
                {
                    pictureBox1.Image.RotateFlip(RotateFlipType.Rotate90FlipY);
                    pictureBox1.Refresh();
                    pictureBox1.Update();
                }
            }
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image seleceted to display!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {

                //Ha üres a picturebox
                bool rbSelection = radioButton9.Checked;

                if (pictureBox1.Image != null && radioButton9.Checked == rbSelection)
                {
                    pictureBox1.Image.RotateFlip(RotateFlipType.Rotate180FlipY);
                    pictureBox1.Refresh();
                    pictureBox1.Update();
                }
            }
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image seleceted to display!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {

                //Ha üres a picturebox
                bool rbSelection = radioButton10.Checked;

                if (pictureBox1.Image != null && radioButton10.Checked == rbSelection)
                {
                    pictureBox1.Image.RotateFlip(RotateFlipType.Rotate270FlipY);
                    pictureBox1.Refresh();
                    pictureBox1.Update();
                }
            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedExtension = comboBox2.SelectedItem.ToString();

            if (string.IsNullOrEmpty(selectedExtension))
            {

                listBox1.Items.Clear();
                foreach (var item in toc)
                {
                    listBox1.Items.Add(item.Filename);
                }
            }

            else
            {

                List<BoxItem> filteredItems = toc.Where(item => Path.GetExtension(item.Filename) == selectedExtension).ToList();

                listBox1.Items.Clear();
                foreach (var item in filteredItems)
                {
                    listBox1.Items.Add(item.Filename);
                }
            }
            textBox12.Text = $"{listBox1.Items.Count} items";
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {

        }

        private void button7_Click_2(object sender, EventArgs e)
        {

        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            OMSEDITOR.AppendText("\r\n");
            OMSEDITOR.ScrollToCaret();
            OMSEDITOR.SelectedText = comboBox5.SelectedItem.ToString();
        }

        private void OMSEDITOR_TextChanged(object? sender, EventArgs e)
        {
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            OMSEDITOR.AppendText("\r\n");
            OMSEDITOR.ScrollToCaret();
            OMSEDITOR.SelectedText = comboBox3.SelectedItem.ToString();
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            OMSEDITOR.AppendText("\r\n");
            OMSEDITOR.ScrollToCaret();
            OMSEDITOR.SelectedText = comboBox4.SelectedItem.ToString();
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            OMSEDITOR.AppendText("\r\n");
            OMSEDITOR.ScrollToCaret();
            OMSEDITOR.SelectedText = comboBox6.SelectedItem.ToString();
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            OMSEDITOR.AppendText("\r\n");
            OMSEDITOR.ScrollToCaret();
            OMSEDITOR.SelectedText = comboBox8.SelectedItem.ToString();
        }

        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            OMSEDITOR.AppendText("\r\n");
            OMSEDITOR.ScrollToCaret();
            OMSEDITOR.SelectedText = comboBox9.SelectedItem.ToString();
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            OMSEDITOR.AppendText("\r\n");
            OMSEDITOR.ScrollToCaret();
            OMSEDITOR.SelectedText = comboBox10.SelectedItem.ToString();
        }

        private void comboBox11_SelectedIndexChanged(object sender, EventArgs e)
        {
            OMSEDITOR.AppendText("\r\n");
            OMSEDITOR.ScrollToCaret();
            OMSEDITOR.SelectedText = comboBox11.SelectedItem.ToString();
        }

        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {
            OMSEDITOR.AppendText("\r\n");
            OMSEDITOR.ScrollToCaret();
            OMSEDITOR.SelectedText = comboBox12.SelectedItem.ToString();
        }

        private void comboBox13_SelectedIndexChanged(object sender, EventArgs e)
        {
            OMSEDITOR.AppendText("\r\n");
            OMSEDITOR.ScrollToCaret();
            OMSEDITOR.SelectedText = comboBox13.SelectedItem.ToString();
        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            string searchTerm = comboBox7.Text;
            string detailedInfo = GetDetailedInfoForSearchTerm(searchTerm);
            textBox13.Text = detailedInfo;
        }

        private string GetDetailedInfoForSearchTerm(string searchTerm)
        {
            if (wordDetails.ContainsKey(searchTerm))
            {
                return wordDetails[searchTerm];
            }
            return "No info available...";
        }

        private void visualomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Save the visual.oms file
            if (String.IsNullOrEmpty(OMSEDITOR.Text))
            {
                MessageBox.Show("\tVisual.oms file:.\n OMSEDITOR is can not be empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                saveFileDialog1.Filter = "Oms files(.oms)|*.oms";
                saveFileDialog1.Title = "Save .oms files";
                saveFileDialog1.FileName = "visual";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.InitialDirectory = "C:\\Users\\krisz\\Desktop\\boxUnpacker_GUI\\boxUnpackerGUI\\bin\\Debug\\net6.0-windows\\BoxBuilder\\visual_oms"; // Needed to be fixed this => sav.visual.oms directory!

                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    System.IO.File.WriteAllText(saveFileDialog1.FileName, OMSEDITOR.Text);
                    MessageBox.Show("Visual.oms file saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }

            }
        }
        private void initomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(OMSEDITOR.Text))
            {


                MessageBox.Show("\tInit.oms file:.\n OMSEDITOR  is can not be empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            else
            {
                saveFileDialog1.InitialDirectory = "C:\\Users\\krisz\\Desktop\\boxUnpacker_GUI\\boxUnpackerGUI\\bin\\Debug\\net6.0-windows\\BoxBuilder\\init_oms";
                saveFileDialog1.Filter = "Oms files(.oms)|*.oms";
                saveFileDialog1.Title = "Save .oms files";
                saveFileDialog1.FileName = "init";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    System.IO.File.WriteAllText(saveFileDialog1.FileName, OMSEDITOR.Text);
                }
            }
        }

        private void informationomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //informationeditorBox

            if (String.IsNullOrEmpty(OMSEDITOR.Text))
            {
                MessageBox.Show("\tInformation.oms file:.\n OMSEDITOR is can not be empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                saveFileDialog1.InitialDirectory = "C:\\Users\\krisz\\Desktop\\boxUnpacker_GUI\\boxUnpackerGUI\\bin\\Debug\\net6.0-windows\\BoxBuilder\\information_oms";
                saveFileDialog1.Filter = "Oms files(.oms)|*.oms";
                saveFileDialog1.Title = "Save .oms files";
                saveFileDialog1.FileName = "information";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    System.IO.File.WriteAllText(saveFileDialog1.FileName, OMSEDITOR.Text);
                }
            }
        }

        private void exportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void radioButton11_CheckedChanged(object? sender, EventArgs e)
        {
            if (radioButton11.Checked)
            {
                EnableHighLighting();
            }
        }

        private void radioButton12_CheckedChanged(object? sender, EventArgs e)
        {
            if (radioButton12.Checked)
            {
                DisableHighlighting();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            string originalText = OMSEDITOR.Text;
            if (checkBox1.Checked)
            {
                string extractedText = "";
                string[] lines = originalText.Split('\n');
                bool insideMultilineComment = false;
                foreach (string line in lines)
                {
                    // Kiszûri az üres sort és az üres szóközöket a sor elejérõl és végérõl
                    string trimmedLine = line.Trim();
                    // Kiszûri az üres sort
                    if (string.IsNullOrEmpty(trimmedLine))
                    {
                        continue;
                    }
                    // Ellenõrzi, hogy kezdõdik-e a sor egysoros kommenttel
                    if (!insideMultilineComment && trimmedLine.StartsWith("//"))
                    {
                        continue;
                    }
                    // Ellenõrzi, hogy kezdõdik-e a sor többsoros kommenttel
                    if (!insideMultilineComment && trimmedLine.StartsWith("/*"))
                    {
                        insideMultilineComment = true;
                        continue;
                    }
                    // Ellenõrzi, hogy végzõdik-e a többsoros komment
                    if (insideMultilineComment && trimmedLine.EndsWith("*/"))
                    {
                        insideMultilineComment = false;
                        continue;
                    }
                    // Ha semmilyen komment nem található a sorban, hozzáadjuk a kimeneti szöveghez
                    if (!insideMultilineComment)
                    {
                        // Az összes "//" jelet és a mögötte lévõ szöveget eltávolítjuk
                        int index = trimmedLine.IndexOf("//");
                        if (index != -1)
                        {
                            trimmedLine = trimmedLine.Substring(0, index);
                        }
                        extractedText += trimmedLine + "\n";
                    }
                }
                extractedText = extractedText.TrimEnd(); // Az utolsó felesleges sortörlés eltávolítása
                OMSEDITOR.Text = extractedText;
            }
            else
            {
                OMSEDITOR.Text = originalText;
            }
        }

        private void moddbToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = "https://www.moddb.com/games/the-outforce";
            proc.Start();
        }

        private void liveHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = "https://discord.gg/7RbzqN9";
            proc.Start();
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            // OPEN FILE DIALOG SETTINGS for OMSEDITOR
            openFileDialog1.InitialDirectory = "\\Data\\BoxBuilder\\";
            openFileDialog1.Filter = "OMS Files|*.oms";
            openFileDialog1.Title = "Load an *.oms file";
            openFileDialog1.FileName = "";
            openFileDialog1.AddExtension = true;
            openFileDialog1.RestoreDirectory = true;

            // OPEN FILE DIALOG FOR INIT.OMS FILES
            if (openFileDialog1.ShowDialog() == DialogResult.OK &&
               openFileDialog1.FileName.Length > 0)
            {
                string filename = openFileDialog1.FileName;
                string[] lines = System.IO.File.ReadAllLines(filename);

                toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
                toolStripProgressBar1.Minimum = 0;
                toolStripProgressBar1.Maximum = lines.Length;
                toolStripProgressBar1.Value = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    toolStripProgressBar1.Value = i + 1; // ProgressBar folyamat +1
                }

                MessageBox.Show(".oms file loaded successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OMSEDITOR.LoadFile(openFileDialog1.FileName, RichTextBoxStreamType.PlainText);

                toolStripProgressBar1.Value = toolStripProgressBar1.Minimum;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            OMSEDITOR.Clear();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            OMSEDITOR.Refresh();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {
        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void cfgConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Save the visual.oms file
            if (String.IsNullOrEmpty(OMSEDITOR.Text))
            {
                MessageBox.Show("\t Configuration file:.\n OMSEDITOR is can not be empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                saveFileDialog1.Filter = "Configuration files(.cfg)|*.cfg";
                saveFileDialog1.Title = "Save .cfg files";
                saveFileDialog1.FileName = "visual";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.InitialDirectory = "C:\\Users\\krisz\\Desktop\\boxUnpacker_GUI\\boxUnpackerGUI\\bin\\Debug\\net6.0-windows\\BoxBuilder\\configuration"; // Needed to be fixed this => sav.visual.oms directory!

                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    System.IO.File.WriteAllText(saveFileDialog1.FileName, OMSEDITOR.Text);
                    MessageBox.Show("Configuration file saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }

            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            // Export edited jpg/bmp/png/jpeg to \\BoxBuilder\\Misc as export.jpg
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("There's nothing to save. No image opened", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select a file format.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string selectedFormat = comboBox1.SelectedItem.ToString();
                ImageFormat imageFormat = ImageFormat.Jpeg; // Default to JPEG

                if (selectedFormat == ".bmp")
                {
                    imageFormat = ImageFormat.Bmp;
                }
                else if (selectedFormat == ".png")
                {
                    imageFormat = ImageFormat.Png;
                }
                else if (selectedFormat == ".jpg" || selectedFormat == ".jpeg")
                {
                    imageFormat = ImageFormat.Jpeg;
                }

                string fileName = textBox11.Text + selectedFormat;

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.FileName = fileName;
                    sfd.Filter = "Image Files|" + selectedFormat;
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        using (Bitmap bitmap = new Bitmap(pictureBox1.Image))
                        {
                            bitmap.Save(sfd.FileName, imageFormat);
                            MessageBox.Show("Image successfully saved to " + sfd.FileName, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void extractSelectedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (toc.Count > 0 && listBox1.SelectedIndex >= 0)
            {
                BoxItem selectedItem = toc[listBox1.SelectedIndex];
                string boxFileName = ofd.FileName;
                string outputDirectory = Path.Combine(Application.StartupPath, "ExtractedFile");
                // string outputDirectory = Path.GetDirectoryName(boxFileName) ?? "";
                Directory.CreateDirectory(outputDirectory);

                ExtractFileFromBox(boxFileName, selectedItem, outputDirectory);
                MessageBox.Show("Selected file successfully extracted!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("I could not extract any files. Please open a *.box archive to add its content to listbox!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void extractAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (toc.Count > 0)
            {
                string boxFileName = ofd.FileName;
                //  string outputDirectory = Path.GetDirectoryName(boxFileName) ?? "ExtractedFiles";
                string outputDirectory = Path.Combine(Application.StartupPath, "ExtractedFiles");

                toolStripProgressBar1.Minimum = 0;
                toolStripProgressBar1.Maximum = toc.Count;
                toolStripProgressBar1.Value = 0;

                foreach (BoxItem item in toc)
                {
                    ExtractFileFromBox(boxFileName, item, outputDirectory);
                    toolStripProgressBar1.Value++;
                }

                //  toolStripProgressBar1.Value = 0;
                MessageBox.Show("All files successfully extracted!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                toolStripProgressBar1.Value = 0;
            }
            else
            {
                MessageBox.Show("No files to extract!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void boxArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 3;
        }

        private void restartProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void tabPage1_Click_1(object sender, EventArgs e)
        {

        }
    }
}

public struct BoxItem
{
    public string Filename;
    public uint Offset;
    public uint Size;
    public bool IsImage;
}