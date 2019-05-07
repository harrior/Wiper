using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;


namespace Wiper1
{
    class Program
    {

        static void ReWriteByte(string filepath, byte val)
        {
            var fileinfo = new FileInfo(filepath);
            
            using (var writer = new BinaryWriter(File.Open(filepath, FileMode.OpenOrCreate)))
            {
                for (long i = 0; i < fileinfo.Length; i++)
                {
                    writer.Write(val);
                }
            }
        }

        //Перезапись файла случайными данными
        static void ReWriteRand(string filepath)
        {
            var fileinfo = new FileInfo(filepath);

            Random rand = new Random();
            byte[] val = new byte[1];

            using (var writer = new BinaryWriter(File.Open(filepath, FileMode.OpenOrCreate)))
            {
                for (long i = 0; i < fileinfo.Length; i++)
                {
                    rand.NextBytes(val);
                    writer.Write(val);
                }
            }  
        }

        //обнуление файла
        static void ReWriteEmpty(string filepath)
        {
            using (var writer = new BinaryWriter(File.Open(filepath, FileMode.Truncate))) { }
            
        }

        //Генератор случайных имен файлов
        static string GenRandomString(int Length)
        {
            string Alphabet = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890";
            Random rnd = new Random();
            StringBuilder sb = new StringBuilder(Length - 1);
            int Position = 0;

            for (int i = 0; i < Length; i++)
            {
                Position = rnd.Next(0, Alphabet.Length - 1);
                sb.Append(Alphabet[Position]);
            }
            return sb.ToString();
        }

        //Основная функция, перезаписывает файл
        static void WipeFile(string filename, byte method)
        {
            //method 0 - один проход random
            if (method == 0)
            {
                ReWriteRand(filename);
            }
            // 1 - DoD 5220.22-M - 3 прохода
            else if (method == 1)
            {
                ReWriteByte(filename, 0x00);
                ReWriteByte(filename, 0xFF);
                ReWriteRand(filename);
            }
            // Метод Шнайдера - 7 проходов
            else if (method == 2)
            {
                ReWriteByte(filename, 0xFF);
                ReWriteByte(filename, 0x00);
                for (int i = 0; i < 5; i++) ReWriteRand(filename);
            }

            ReWriteEmpty(filename);
        }

        //переименовает случайным образом файл и удаляет его.
        static void RenameAndDeleteFile(string filename)
        {
            string newfilename = filename.Substring(0, filename.LastIndexOf('\\') + 1) + GenRandomString(5);
            File.Move(filename, newfilename);
            File.Delete(newfilename);
        }

        //рекурсивно обходит папку и составляет список файлов и директорий
        static List<string> getFileList(string DirName)
        {
           var fileList = Directory.GetFiles(DirName).ToList<string>();
            foreach (string dir in Directory.GetDirectories(DirName))
            {
                fileList.Add(dir);
                fileList.AddRange(getFileList(dir));
            }
            return fileList;
        }

        static List<string> RemoveFiles(List<string> fileList, byte method) {
            var errors = new List<string>();

            // Убиваем файлы
            foreach (string file in fileList)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        WipeFile(file, method);
                        RenameAndDeleteFile(file);
                    }
                }
                catch
                {
                    errors.Add(file);
                }
            }
            return errors;
        }

        static List<string> RemoveDirs(List<string> dirList)
        {
            var errors = new List<string>();
            foreach (string dir in dirList)
            {
                try
                {
                    if (Directory.Exists(dir))
                    {
                        Console.WriteLine(dir);
                        string newdirname = dir.Substring(0, dir.LastIndexOf('\\') + 1) + GenRandomString(5);
                        Directory.Move(dir, newdirname);
                        Directory.Delete(newdirname);
                    }
                }
                catch
                {
                    errors.Add(dir);
                }
            }
            return errors;
        }
        static void AddMenu()
        {
            try
            {
                RegistryKey RegHandle = Registry.ClassesRoot.OpenSubKey(@"\Directory\shell\", RegistryKeyPermissionCheck.ReadWriteSubTree);
                RegHandle = RegHandle.CreateSubKey("Wiper");
                RegHandle.SetValue("", "Secure Delete");
                RegHandle.SetValue("Icon", Application.ExecutablePath);
                RegHandle = RegHandle.CreateSubKey("command");
                RegHandle.SetValue("", Application.ExecutablePath + " \"%1\"");

                RegHandle = Registry.ClassesRoot.OpenSubKey(@"\*\shell\", RegistryKeyPermissionCheck.ReadWriteSubTree);
                RegHandle = RegHandle.CreateSubKey("Wiper");
                RegHandle.SetValue("", "Secure Delete");
                RegHandle.SetValue("Icon", Application.ExecutablePath);
                RegHandle = RegHandle.CreateSubKey("command");
                RegHandle.SetValue("", Application.ExecutablePath + " \"%1\"");
            }
            catch
            {
                MessageBox.Show("Could not add item to menu. Try running the application again with administrator rights.", "Error");
            }
        }

        static void DelMenu()
        {
            try
            {
                RegistryKey RegHandle = Registry.ClassesRoot.OpenSubKey(@"\Directory\shell\", RegistryKeyPermissionCheck.ReadWriteSubTree);
                RegHandle.DeleteSubKeyTree("Wiper");
                RegHandle = Registry.ClassesRoot.OpenSubKey(@"\*\shell\", RegistryKeyPermissionCheck.ReadWriteSubTree);
                RegHandle.DeleteSubKeyTree("Wiper");
            }
            catch
            {
                MessageBox.Show("Could not remove item from menu. Try running the application again with administrator rights.", "Error");
            }
        }

        static int Main(string[] args)
        {
            byte delmethod = 2;
            bool hidemode = false;

            if (args.Length > 0)
            {
                List<String> arg = args.ToList<string>();
                List<String> fileList = new List<string>();

                foreach (string option in args)
                {
                    if (String.Compare(option, "-s") == 0)
                    {
                        Form1 form = new Form1();
                        form.ShowDialog();
                        return 0;
                    }
                    else if (String.Compare(option, "-i") == 0)
                    {
                        AddMenu();
                        return 0;
                    }
                    else if (String.Compare(option, "-u") == 0)
                    {
                        DelMenu();
                        return 0;
                    }
                    else if (option.IndexOf("-m") == 0)
                    {
                        try
                        {
                            Console.WriteLine();
                            delmethod = Convert.ToByte(args[0].Substring(2, 1));
                            if (delmethod < 0 || delmethod > 2) delmethod = 2;
                        }
                        catch
                        {
                            delmethod = 2;
                        }
                    }
                    else if (String.Compare(option, "-h") == 0)
                    {
                        hidemode = true;
                    }
                }

                // Рекурсивно ищем директории.
                foreach (string path in args) { 
                    if (System.IO.Directory.Exists(path))
                    {
                        fileList.Add(path);
                        fileList.AddRange(getFileList(path));
                    }
                    else if (System.IO.File.Exists(path))
                    {
                        fileList.Add(path);
                    }
                }


                var res = DialogResult.No;
                if (fileList.Count <= 1){ hidemode = true; }
                if (!hidemode) { res = MessageBox.Show("Delete " + fileList.Count.ToString() + " objects?", "Shredder", MessageBoxButtons.YesNo); }
                
                if ((res == DialogResult.Yes) || (hidemode)) {

                    var errors = new List<string>();

                    fileList.Reverse(); //костыль

                    // Убиваем файлы
                    errors.AddRange(RemoveFiles(fileList, delmethod));

                    // Убиваем папки
                    errors.AddRange(RemoveDirs(fileList));

                    if ((errors.Count > 0) && (!hidemode)) { MessageBox.Show("Don't delete " + errors.Count.ToString() + "objects"); }
                }
            }
            return 0;
        }
    }
}
