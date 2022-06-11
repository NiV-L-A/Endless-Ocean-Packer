using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Endless_Ocean_Packer2
{
    class Program
    {
        public static string InfoPathMessage = "";
        public static string ContPathMessage = "";
        public static string InfoPath = "";
        public static string ContPath = "";
        public static string logPath = "";
        public static bool infofound = false;
        public static bool contfound = false;
        public static bool logfound = false;
        public static bool isEO1 = false;
        public static string SettingsTxTPath = "";
        public static uint lastoff = 0;
        public static uint ENTRIES = 0;
        public static uint lastoffcd = 0;
        public static ushort ALIGN = 0;
        public static FileStream fsInfo = null;
        public static FileStream fsInfoEO1Dec = null;
        public static BinaryReader brInfo = null;
        public static FileStream fsLog = null;
        public static FileStream fsOutput = null;
        public static byte[] First10 = null;
        public static byte[] InfoHeader = new byte[0x2C]
        { 0x00, 0x41, 0x52, 0x4b, 0x5f, 0x43, 0x52, 0x56, 0x30, 0x30, 0x35, 0x5f, 0x44, 0x41, 0x54, 0x41, //ARK_CRV005_DATA
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //00
          0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };//00 align

        static void Main(string[] args)
        {
            Console.Title = "Endless Ocean 1 & 2 Packer";

            if (args.Length == 0) //if no args are passed to the .exe
            {
                ParseTXT();

                if (infofound && contfound)
                {
                    if (logfound)
                    {
                        WriteText(fsLog, $"Log file date and time: {DateTime.Now}\n");
                        WriteText(fsLog, InfoPathMessage);
                        WriteText(fsLog, ContPathMessage);
                    }

                    if (!File.Exists(InfoPath + "\\INFO.DAT"))
                    {
                        WriteError($"!!! INFO.DAT file not found. Path:\n{InfoPath + "\\INFO.DAT"}");
                        Console.Read();
                        return;
                    }

                    if (!Directory.Exists(ContPath))
                    {
                        WriteError($"!!! Content path does not exist. Path:\n{ContPath}");
                        Console.Read();
                        return;
                    }

                    string OutputPath = InfoPath + "\\GAME.DAT";
                    fsInfo = new FileStream(InfoPath + "\\INFO.DAT", FileMode.Open);
                    brInfo = new BinaryReader(fsInfo);
                    File.Delete(OutputPath); //delete GAME.DAT if there's already one
                    fsOutput = new FileStream(OutputPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                    ParseINFO();

                    Console.ReadKey();
                    Stopwatch stopWatch = new();
                    stopWatch.Start();

                    for (int i = 0; i < ENTRIES; i++)
                    {
                        Console.Title = $"Endless Ocean 1 & 2 Packer - Building GAME/INFO.DAT pair - Entry: {i + 1}/{ENTRIES}";
                        string InfoFileName = ReadNullTerminatedString(brInfo, 0x20);
                        InfoFileName = InfoFileName.Replace("/", "\\");
                        string FullPathAndName = $"{ContPath}\\{InfoFileName}";
                        uint length = (uint)new FileInfo(FullPathAndName).Length;
                        WriteUInt32(fsInfo, length);
                        uint sizeWithCD = (length / ALIGN) + 1;

                        if (i != 0)
                        {
                            fsInfo.Seek(-0x30, SeekOrigin.Current);
                            lastoff = brInfo.ReadUInt32();
                            lastoffcd = brInfo.ReadUInt32();
                            fsInfo.Seek(0x28, SeekOrigin.Current);
                        }

                        WriteUInt32(fsInfo, lastoff + lastoffcd);
                        WriteUInt32(fsInfo, sizeWithCD); //offcd
                        WriteUInt32(fsInfo, length); //length2

                        int lengthCD = (int)((sizeWithCD * ALIGN) - length);
                        byte[] arrayFile = File.ReadAllBytes(FullPathAndName);
                        Array.Resize(ref arrayFile, arrayFile.Length + lengthCD);
                        Array.Fill(arrayFile, (byte)0xCD, arrayFile.Length - lengthCD, lengthCD);

                        string text = $"0x{(lastoff + lastoffcd) * ALIGN:X8} | 0x{length:X8} | 0x{arrayFile.Length:X8} | {InfoFileName}\n";
                        Console.Write(text);

                        if (logfound)
                        {
                            WriteText(fsLog, text);
                        }

                        fsOutput.Write(arrayFile, 0, arrayFile.Length); //write to new GAME.DAT
                    }

                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                    elapsedTime = $"Elapsed time: {elapsedTime}";
                    Console.WriteLine("\nEnded\n");

                    if (logfound)
                    {
                        WriteText(fsLog, elapsedTime);
                    }

                    if (isEO1)
                    {
                        Console.WriteLine("Encrypting EO1 INFO.DAT\n");
                        BinaryWriter bw = new(fsInfo);
                        fsInfo.Seek(0x10, SeekOrigin.Begin);
                        EncryptEO1Info(brInfo, bw, First10);
                    }

                    Console.WriteLine(elapsedTime);
                    Console.Title = $"Endless Ocean 1 & 2 Packer - Successfully built GAME/INFO.DAT pair";

                    fsInfo.Dispose();
                    fsInfo.Close();
                    brInfo.Dispose();
                    brInfo.Close();
                    fsOutput.Dispose();
                    fsOutput.Close();

                    if (isEO1)
                    {
                        File.Delete(InfoPath + "\\INFO.DAT");
                        File.Move(InfoPath + "\\INFO_ENC.DAT", InfoPath + "\\INFO.DAT");
                    }

                    if (logfound)
                    {
                        fsLog.Dispose();
                        fsLog.Close();
                    }
                }
                else
                {
                    PrintExample();
                }

                Console.ReadLine();
            }
            else
            {
                WriteError("!!!Do not run the .exe with arguments! Simply place the .txt file in the same folder and run the .exe!!!");
                Console.Read();
                return;
            }
        }

        public static void PrintInfo()
        {
            string info = "Endless Ocean 1 & 2 Packer\n";
            Console.WriteLine(string.Format("{0," + ((Console.WindowWidth / 2) + (info.Length / 2)) + "}", info));
            string aut = "Authors: NiV & MDB\n";
            Console.WriteLine(string.Format("{0," + ((Console.WindowWidth / 2) + (aut.Length / 2)) + "}", aut));
            string aut2 = "Special thanks to Taylor\n";
            Console.WriteLine(string.Format("{0," + ((Console.WindowWidth / 2) + (aut2.Length / 2)) + "}", aut2));
            string ver = "Version 1.0\n";
            Console.WriteLine(string.Format("{0," + ((Console.WindowWidth / 2) + (ver.Length / 2)) + "}", ver));
            string discorda = "If you have any issues, join this discord server and contact NiV-L-A:\n";
            Console.WriteLine(string.Format("{0," + ((Console.WindowWidth / 2) + (discorda.Length / 2)) + "}", discorda));
            string discordb = "https://discord.gg/4hmcsmPMDG\n";
            Console.WriteLine(string.Format("{0," + ((Console.WindowWidth / 2) + (discordb.Length / 2)) + "}", discordb));
        }

        public static void PrintExample()
        {
            Console.WriteLine("Make sure you have the .txt in this format:");
            Console.WriteLine("#Argument 1, INFO.DAT & GAME.DAT folder path (input & output folder)\nInfoFolder=\n#Argument 2, content folder\nContentFolder=\n#If you want, you can also use a 3rd argument to log some information in the same directory as this .exe:\n-log\n#If a line starts with '#', it will be ignored.\n");
            Console.WriteLine("Example:");
            Console.WriteLine("InfoFolder=\"C:\\Users\\Roberto\\Desktop\\EOModding\\Wii\\Endless Ocean Blue World [R4EE01]\\files\"\nContentFolder=\"C:\\Users\\Roberto\\Desktop\\EOModding\\Wii\\Endless Ocean Blue World [R4EE01]\\files\\QuickBMSOutput\n#blahblahblah: this line will be ignored because of the #\n\"");
            Console.WriteLine("Press any key to close this window");
        }

        public static void WriteText(FileStream fsLog, string text)
        {
            byte[] textbytes = Encoding.UTF8.GetBytes(text);
            fsLog.Write(textbytes, 0, textbytes.Length);
            fsLog.Flush();
        }

        public static string ReadNullTerminatedString(BinaryReader stream, uint MaxLength)
        {
            string str = "";
            char ch;
            while ((ch = (char)stream.PeekChar()) != 0 && (str.Length < MaxLength))
            {
                ch = stream.ReadChar();
                str += ch;
            }
            stream.BaseStream.Seek(MaxLength - str.Length, SeekOrigin.Current);
            return str;
        }

        public static void WriteUInt32(FileStream fs, uint value)
        {
            byte[] valueArray = BitConverter.GetBytes(value);
            fs.Write(valueArray, 0x00, 0x04);
        }

        public static void ParseTXT()
        {
            SettingsTxTPath = AppDomain.CurrentDomain.BaseDirectory + "EndlessOceanPackerSettings.txt";
            PrintInfo();

            if (!File.Exists(SettingsTxTPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("!!! Please, create an EndlessOceanPackerSettings.txt file in the same directory as this .exe !!!\n");
                Console.ForegroundColor = ConsoleColor.Gray;
                return;
            }

            IEnumerable<string> enumLines = File.ReadLines(SettingsTxTPath, Encoding.UTF8);
            Console.WriteLine("EndlessOceanPackerSettings.txt found!");

            foreach (var line in enumLines)
            {
                if (line.StartsWith("InfoFolder="))
                {
                    infofound = true;
                    InfoPath = line.Substring(11, line.Length - 11);
                    InfoPath = InfoPath.Replace("\"", "");
                    InfoPathMessage = $"INFO.DAT & GAME.DAT folder path set to: {InfoPath}\n";
                    Console.Write(InfoPathMessage);
                }
                else if (line.StartsWith("ContentFolder="))
                {
                    contfound = true;
                    ContPath = line.Substring(14, line.Length - 14);
                    ContPath = ContPath.Replace("\"", "");
                    ContPathMessage = $"Content folder path set to: {ContPath}\n";
                    Console.Write(ContPathMessage);
                }
                else if (line.StartsWith("-log"))
                {
                    logfound = true;
                    logPath = AppDomain.CurrentDomain.BaseDirectory + "EndlessOceanPackerLogFile.txt";
                    fsLog = new FileStream(logPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[DEBUG] Log file function activated");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

        public static void WriteError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void ParseINFO()
        {
            byte FirstByte = brInfo.ReadByte();
            fsInfo.Seek(0, SeekOrigin.Begin);
            if (FirstByte != 0) //EO1
            {
                isEO1 = true;
                string EO1InfoDecPath = InfoPath + "\\INFO_ENC.dat";
                File.Delete(EO1InfoDecPath); //delete file if it exists already
                BinaryWriter bwinfoEO1Dec = new(File.Open(EO1InfoDecPath, FileMode.Create, FileAccess.ReadWrite));

                First10 = new byte[0x10] { brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte(), brInfo.ReadByte() };

                bwinfoEO1Dec.Write(First10);

                var uVar17 = 0x10;
                var FileSize_param_3 = fsInfo.Length;
                var counter = 0;

                Console.WriteLine("\nDecrypting EO1 INFO.DAT");

                while (uVar17 < FileSize_param_3)
                {
                    GetRow(brInfo, bwinfoEO1Dec, First10, counter);

                    uVar17 += 0x10;
                }

                bwinfoEO1Dec.Flush();
                bwinfoEO1Dec.Close();

                fsInfo.Dispose();
                fsInfo.Close();

                fsInfo = new FileStream(EO1InfoDecPath, FileMode.Open);
                brInfo = new BinaryReader(fsInfo);

                fsInfo.Seek(0x2C, SeekOrigin.Begin);
                ENTRIES = brInfo.ReadUInt32();
                ALIGN = 0x800;

                string fsLogString = "";

                switch(ENTRIES)
                {
                    case 0xC5C:
                        fsLogString = "\nEO1 NTSC (USA) INFO.DAT Detected";
                        Console.WriteLine(fsLogString);
                        break;

                    case 0xC03:
                        fsLogString = "\nEO1 PAL INFO.DAT Detected";
                        Console.WriteLine(fsLogString);
                        break;

                    case 0xAD2:
                        fsLogString = "\nEO1 NTSC-J (JAP) 1.0 INFO.DAT Detected";
                        Console.WriteLine(fsLogString);
                        break;

                    case 0xB46:
                        fsLogString = "\nEO1 NTSC-J (JAP) 1.1 INFO.DAT Detected";
                        Console.WriteLine(fsLogString);
                        break;

                    default: //should never happen
                        fsLogString = "!!!Unknown number of entries, stopping for now...!!!";
                        WriteError(fsLogString);
                        Console.WriteLine("Press any key to close this window");
                        Console.Read();
                        break;
                }

                if (logfound)
                {
                    WriteText(fsLog, fsLogString + "\n");
                }

                string EntriesInfo = $"Entries: {ENTRIES} [0x{ENTRIES:X8}]\n";
                Console.WriteLine(EntriesInfo);
                Console.WriteLine("Press any key to start the Packer\n");
                string Columns = "OFFSET     | SIZE       | REAL SIZE  | FILENAME";
                Console.WriteLine(Columns);

                if (logfound)
                {
                    WriteText(fsLog, EntriesInfo);
                    WriteText(fsLog, Columns + "\n");
                }
            }
            else //EO2
            {
                byte[] array = brInfo.ReadBytes(0x2C);
                fsInfo.Seek(0x24, SeekOrigin.Begin);
                ALIGN = brInfo.ReadUInt16();
                fsInfo.Seek(0x06, SeekOrigin.Current);
                ENTRIES = brInfo.ReadUInt32();
                if (array.SequenceEqual(InfoHeader)) //check INFO.DAT header
                {
                    string fsLogString = "";

                    switch (ENTRIES)
                    {
                        case 0x18A5:
                            fsLogString = "\nEO2 NTSC (USA) / PAL INFO.DAT Detected";
                            Console.WriteLine(fsLogString);
                            break;

                        case 0x1114:
                            fsLogString = "\nEO2 NTSC-J (JAP) INFO.DAT Detected";
                            Console.WriteLine(fsLogString);
                            break;

                        default: //should never happen
                            fsLogString = "!!!Unknown number of entries, stopping for now...!!!";
                            WriteError(fsLogString);
                            Console.WriteLine("Press any key to close this window");
                            Console.Read();
                            break;
                    }

                    if (logfound)
                    {
                        WriteText(fsLog, fsLogString + "\n");
                    }

                    string EntriesInfo = $"Entries: {ENTRIES} [0x{ENTRIES:X8}]\n";
                    Console.WriteLine(EntriesInfo);
                    Console.WriteLine("Press any key to start the Packer\n");
                    string Columns = "OFFSET     | SIZE       | REAL SIZE  | FILENAME";
                    Console.WriteLine(Columns);

                    if (logfound)
                    {
                        WriteText(fsLog, EntriesInfo);
                        WriteText(fsLog, Columns + "\n");
                    }
                }
            }
        }

        public static byte DecryptByte(BinaryReader brinfo, byte[] First10, int counter)
        {
            byte currbyte = brinfo.ReadByte();
            currbyte = (byte)~((currbyte & 0xf) << 0x4 | (currbyte >> 0x4) & 0xFF);
            currbyte = (byte)(currbyte - First10[counter]);
            return currbyte;
        }

        public static byte EncryptByte(BinaryReader brinfo, byte[] First10, int counter)
        {
            byte currbyte = brinfo.ReadByte();
            currbyte = (byte)(currbyte + First10[counter]);
            currbyte = (byte)~((currbyte & 0xf) << 0x4 | (currbyte >> 0x4) & 0xFF);
            brInfo.BaseStream.Seek(-1, SeekOrigin.Current);
            return currbyte;
        }

        public static void GetRow(BinaryReader brinfo, BinaryWriter bw, byte[] First10, int counter)
        {
            var iVar4 = 0x2;
            do
            {
                var tbyte = DecryptByte(brinfo, First10, counter);
                bw.Write(tbyte);

                tbyte = DecryptByte(brinfo, First10, counter + 1);
                bw.Write(tbyte);

                tbyte = DecryptByte(brinfo, First10, counter + 2);
                bw.Write(tbyte);

                tbyte = DecryptByte(brinfo, First10, counter + 3);
                bw.Write(tbyte);

                tbyte = DecryptByte(brinfo, First10, counter + 4);
                bw.Write(tbyte);

                tbyte = DecryptByte(brinfo, First10, counter + 5);
                bw.Write(tbyte);

                tbyte = DecryptByte(brinfo, First10, counter + 6);
                bw.Write(tbyte);

                tbyte = DecryptByte(brinfo, First10, counter + 7);
                bw.Write(tbyte);

                if (counter == 0)
                {
                    counter = 8;
                }
                else if (counter == 8)
                {
                    counter = 0;
                }

                iVar4--;
            } while (iVar4 != 0x0);
        }

        public static void EncryptEO1Info(BinaryReader brinfo, BinaryWriter bw, byte[] First10)
        {
            var uVar17 = 0x10;
            var FileSize_param_3 = fsInfo.Length;
            var counter = 0;

            while (uVar17 < FileSize_param_3)
            {
                var iVar4 = 0x2;
                do
                {
                    var tbyte = EncryptByte(brinfo, First10, counter);
                    bw.Write(tbyte);

                    tbyte = EncryptByte(brinfo, First10, counter + 1);
                    bw.Write(tbyte);

                    tbyte = EncryptByte(brinfo, First10, counter + 2);
                    bw.Write(tbyte);

                    tbyte = EncryptByte(brinfo, First10, counter + 3);
                    bw.Write(tbyte);

                    tbyte = EncryptByte(brinfo, First10, counter + 4);
                    bw.Write(tbyte);

                    tbyte = EncryptByte(brinfo, First10, counter + 5);
                    bw.Write(tbyte);

                    tbyte = EncryptByte(brinfo, First10, counter + 6);
                    bw.Write(tbyte);

                    tbyte = EncryptByte(brinfo, First10, counter + 7);
                    bw.Write(tbyte);

                    if (counter == 0)
                    {
                        counter = 8;
                    }
                    else if (counter == 8)
                    {
                        counter = 0;
                    }

                    iVar4--;
                } while (iVar4 != 0x0);

                uVar17 += 0x10;
            }
        }
    }
}
