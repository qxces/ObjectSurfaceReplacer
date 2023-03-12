using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ObjectSurfaceReplacer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ReplaceSurfaces_btn_Click(object sender, EventArgs e)
        {
            //string objectPath = @"E:\X-Ray_CoP_SDK\editors\import\trees_palki11.object";
            string objectPath = @"E:\X-Ray_CoP_SDK\editors\import\trees_palki1.~object";
            //string objectPath = @"E:\X-Ray_CoP_SDK\editors\import\SUKA_BLEAT.object";

            //creating an object of Stream
            FileStream stream = new FileStream(objectPath, FileMode.Open,
            FileAccess.Read, FileShare.ReadWrite);
            //creating BinaryReader using Stream object

            byte[] mainChunkOldSize;
            byte[] mainChunkNewSize;
            byte[] authorChunkOld;
            byte[] authorChunkNew;
            using (BinaryReader reader = new BinaryReader(stream, encoding: System.Text.Encoding.ASCII))
            {
                //7777
                Console.WriteLine("block " + BitConverter.ToString(reader.ReadBytes(4)));
                // Console.WriteLine(reader.ReadUInt32());
                mainChunkOldSize = reader.ReadBytes(4);
                Console.WriteLine(BitConverter.ToString(mainChunkOldSize));

                //00-09-00-00
                Console.WriteLine("block " + BitConverter.ToString(reader.ReadBytes(4)));
                var size = reader.ReadUInt32();
                Console.WriteLine(size);
                Console.WriteLine(BitConverter.ToString(reader.ReadBytes((int)size))); //version

                //12-09-00-00
                Console.WriteLine("block " + BitConverter.ToString(reader.ReadBytes(4)));
                size = reader.ReadUInt32();
                Console.WriteLine(size);
                Console.WriteLine(BitConverter.ToString(reader.ReadBytes((int)size))); // userdata

                //25-09-00-00
                Console.WriteLine("block " + BitConverter.ToString(reader.ReadBytes(4)));
                size = reader.ReadUInt32();
                Console.WriteLine(size);
                Console.WriteLine(reader.ReadString()); // LOD

                //03-09-00-00
                Console.WriteLine("block " + BitConverter.ToString(reader.ReadBytes(4)));
                size = reader.ReadUInt32();
                Console.WriteLine(size);
                Console.WriteLine(BitConverter.ToString(reader.ReadBytes((int)size))); // model type

                //блок с вложением данных о геометрии
                //10-09-00-00
                Console.WriteLine("block " + BitConverter.ToString(reader.ReadBytes(4)));
                size = reader.ReadUInt32();
                Console.WriteLine(size);
                //невозможно прочитать из-за сложной вложенности + reader.ReadString() работает некорректно
                reader.ReadBytes((int)size);

                //07-09-00-00
                Console.WriteLine("block " + BitConverter.ToString(reader.ReadBytes(4)));
                size = reader.ReadUInt32();
                Console.WriteLine(size);
                var mat_cnt = reader.ReadUInt32();
                Console.WriteLine(mat_cnt);
                for (int i = 1; i < mat_cnt + 1; i++)
                {
                    Console.WriteLine("mat #" + i.ToString());
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(ByteCount(reader)))); //Название
                    reader.BaseStream.Position = reader.BaseStream.Position + 1;
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(ByteCount(reader)))); //Характеристика для движка
                    reader.BaseStream.Position = reader.BaseStream.Position + 1;
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(ByteCount(reader)))); //Характеристика для компилятор
                    reader.BaseStream.Position = reader.BaseStream.Position + 1;
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(ByteCount(reader)))); //Характеристика для игры
                    reader.BaseStream.Position = reader.BaseStream.Position + 1;
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(ByteCount(reader)))); //Путь к текстуре
                    reader.BaseStream.Position = reader.BaseStream.Position + 1;
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(ByteCount(reader)))); //Texture
                    reader.BaseStream.Position = reader.BaseStream.Position + 1;
                    Console.WriteLine(reader.ReadUInt32()); //0x1, если материал двусторонний, иначе 0x0
                    Console.WriteLine(BitConverter.ToString(reader.ReadBytes(8))); //0x12 0x1 0x0 0x0 0x1 0x0 0x0 0x0	
                }

                //07-09-00-00
                byte[] authorChunk = reader.ReadBytes(4);
                Console.WriteLine("block " + BitConverter.ToString(authorChunk));
                /*size = reader.ReadUInt32();
                Console.WriteLine(size);
                Console.WriteLine(BitConverter.ToString(reader.ReadBytes((int)size)));*/

                byte[] authorChunksize = reader.ReadBytes(4);
                Console.WriteLine(BitConverter.ToString(authorChunksize));

                Console.WriteLine("chunk size " + BitConverter.ToUInt32(authorChunksize, 0));


                var creator = reader.ReadBytes(ByteCount(reader));
                Console.WriteLine(System.Text.Encoding.ASCII.GetString(creator)); //Создатель
                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                var create_date = reader.ReadInt32();
                Console.WriteLine("create date " + create_date); //дата создания

                byte[] mod = reader.ReadBytes(ByteCount(reader));
                Console.WriteLine("OLD MOD " + System.Text.Encoding.ASCII.GetString(mod)); //Модифицирующий
                Console.WriteLine(BitConverter.ToString(mod));

                byte[] new_mod = Encoding.ASCII.GetBytes("yoba");
                Console.WriteLine(BitConverter.ToString(new_mod));
                Console.WriteLine("new mod " + System.Text.Encoding.ASCII.GetString(new_mod));

                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                var mod_date = reader.ReadInt32();
                Console.WriteLine("mod date " + mod_date); //дата изменения


                byte[] totalOld = creator;
                Array.Resize(ref totalOld, totalOld.Length + 1);
                totalOld = totalOld.Concat(BitConverter.GetBytes(create_date)).ToArray(); // + create_date        
                totalOld = totalOld.Concat(mod).ToArray(); // + mod
                Array.Resize(ref totalOld, totalOld.Length + 1);
                totalOld = totalOld.Concat(BitConverter.GetBytes(mod_date)).ToArray(); // + mod_date

                byte[] oldSize = BitConverter.GetBytes(Buffer.ByteLength(totalOld));

                Console.WriteLine("calc size " + Buffer.ByteLength(totalOld));
                Console.WriteLine(BitConverter.ToString(totalOld));

                /////////
                byte[] totalNew = creator;
                Array.Resize(ref totalNew, totalNew.Length + 1);
                totalNew = totalNew.Concat(BitConverter.GetBytes(create_date)).ToArray(); // + create_date        
                totalNew = totalNew.Concat(new_mod).ToArray(); // + mod
                Array.Resize(ref totalNew, totalNew.Length + 1);
                totalNew = totalNew.Concat(BitConverter.GetBytes(mod_date)).ToArray(); // + mod_date

                byte[] newSize = BitConverter.GetBytes(Buffer.ByteLength(totalNew));

                //Console.WriteLine("calc size " + Buffer.ByteLength(totalNew));
                //Console.WriteLine(BitConverter.ToString(totalNew));
                //////////

                authorChunkOld = authorChunk.Concat(oldSize).Concat(totalOld).ToArray();
               // Console.WriteLine("chunk old " + BitConverter.ToString(authorChunkOld));

                authorChunkNew = authorChunk.Concat(newSize).Concat(totalNew).ToArray();
                //Console.WriteLine("chunk new " + BitConverter.ToString(authorChunkNew));

                mainChunkNewSize = BitConverter.GetBytes(BitConverter.ToUInt32(mainChunkOldSize, 0) - BitConverter.ToUInt32(oldSize, 0) + BitConverter.ToUInt32(newSize, 0));

            }

            byte[] buffer = File.ReadAllBytes(objectPath);
            byte[] res = ReplaceBytes(buffer, authorChunkOld, authorChunkNew);
            byte[] mainChunk = new byte[] { 0x77, 0x77, 0x00, 0x00 };
            byte[] mainChunk1 = mainChunk.Concat(mainChunkOldSize).ToArray();
            Console.WriteLine("chunk old " + BitConverter.ToString(mainChunk1));
            byte[] mainChunk2 = mainChunk.Concat(mainChunkNewSize).ToArray();
            Console.WriteLine("chunk new " + BitConverter.ToString(mainChunk2));
            res = ReplaceBytes(res, mainChunk1, mainChunk2);
            File.WriteAllBytes(@"E:\X-Ray_CoP_SDK\editors\import\SUKA_BLEAT.object", res);
        }

        public static byte[] ReplaceBytes(byte[] src, byte[] search, byte[] repl)
        {
            if (repl == null) return src;
            int index = FindBytes(src, search);
            if (index < 0) return src;
            byte[] dst = new byte[src.Length - search.Length + repl.Length];
            Buffer.BlockCopy(src, 0, dst, 0, index);
            Buffer.BlockCopy(repl, 0, dst, index, repl.Length);
            Buffer.BlockCopy(src, index + search.Length, dst, index + repl.Length, src.Length - (index + search.Length));
            return dst;
        }

        public static int FindBytes(byte[] src, byte[] find)
        {
            if (src == null || find == null || src.Length == 0 || find.Length == 0 || find.Length > src.Length) return -1;
            for (int i = 0; i < src.Length - find.Length + 1; i++)
            {
                if (src[i] == find[0])
                {
                    for (int m = 1; m < find.Length; m++)
                    {
                        if (src[i + m] != find[m]) break;
                        if (m == find.Length - 1) return i;
                    }
                }
            }
            return -1;
        }

        private int ByteCount(BinaryReader reader)
        {
            //узнаем сколько байтов строка
            var b_count = 0;
            while (true)
            {
                if (reader.ReadByte() != 0)
                    b_count++;
                else
                    break;
            }
            //возвращаем позицию на ту откуда начали
            reader.BaseStream.Position = reader.BaseStream.Position - b_count - 1;
            return b_count;
        }
    }
}
