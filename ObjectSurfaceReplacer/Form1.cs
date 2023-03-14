using System;
using System.Diagnostics;
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

        enum Object
        {
            EOBJ_VERSION = 0x10,
            // EObjectChunkIDs:
            EOBJ_CHUNK_MAIN = 0x7777,
            EOBJ_CHUNK_VERSION = 0x0900,
            EOBJ_CHUNK_FLAGS = 0x0903,
            EOBJ_CHUNK_SURFACES_0 = 0x0905, // old format
            EOBJ_CHUNK_SURFACES_1 = 0x0906, // old format
            EOBJ_CHUNK_SURFACES_2 = 0x0907,
            EOBJ_CHUNK_MESHES = 0x0910,
            EOBJ_CHUNK_0911 = 0x0911, // ignored by AE(Actor Editor)
            EOBJ_CHUNK_USERDATA = 0x0912,
            EOBJ_CHUNK_BONES_0 = 0x0913, // old format
            EOBJ_CHUNK_MOTIONS = 0x0916,
            EOBJ_CHUNK_SHADERS_0 = 0x0918, // old format
            EOBJ_CHUNK_PARTITIONS_0 = 0x0919, // old format
            EOBJ_CHUNK_TRANSFORM = 0x0920,
            EOBJ_CHUNK_BONES_1 = 0x0921,
            EOBJ_CHUNK_REVISION = 0x0922, // file revision
            EOBJ_CHUNK_PARTITIONS_1 = 0x0923,
            EOBJ_CHUNK_MOTION_REFS = 0x0924,
            EOBJ_CHUNK_LOD_REF = 0x0925, // LOD\Reference
                                         //EObjectClipChunkIDs:
            EOBJ_CLIP_VERSION_CHUNK = 0x9000,
            EOBJ_CLIP_DATA_CHUNK = 0x9001,
            EMESH_VERSION = 0x11,
            // EMeshChunkID:
            EMESH_CHUNK_VERSION = 0x1000,
            EMESH_CHUNK_MESHNAME = 0x1001,
            EMESH_CHUNK_FLAGS = 0x1002,
            EMESH_CHUNK_BBOX = 0x1004,
            EMESH_CHUNK_VERTS = 0x1005,
            EMESH_CHUNK_FACES = 0x1006,
            EMESH_CHUNK_VMAPS_0 = 0x1007,
            EMESH_CHUNK_VMREFS = 0x1008,
            EMESH_CHUNK_SFACE = 0x1009,
            EMESH_CHUNK_OPTIONS = 0x1010,
            EMESH_CHUNK_VMAPS_1 = 0x1011,
            EMESH_CHUNK_VMAPS_2 = 0x1012,
            EMESH_CHUNK_SG = 0x1013,
            BONE_VERSION_1 = 0x1,
            BONE_VERSION_2 = 0x2,
            BONE_VERSION = BONE_VERSION_2,
            //   EboneChunkID:
            BONE_CHUNK_VERSION = 0x0001,
            BONE_CHUNK_DEF = 0x0002,
            BONE_CHUNK_BIND_POSE = 0x0003,
            BONE_CHUNK_MATERIAL = 0x0004,
            BONE_CHUNK_SHAPE = 0x0005,
            BONE_CHUNK_IK_JOINT = 0x0006,
            BONE_CHUNK_MASS_PARAMS = 0x0007,
            BONE_CHUNK_IK_FLAGS = 0x0008,
            BONE_CHUNK_BREAK_PARAMS = 0x0009,
            BONE_CHUNK_FRICTION = 0x0010,
            //Game Materials Library(из кода xrFSL)
            GAMEMTLS_VERSION = 1,
            GAMEMTLS_CHUNK_VERSION = 0x1000,
            GAMEMTLS_CHUNK_AUTOINC = 0x1001,
            GAMEMTLS_CHUNK_MATERIALS = 0x1002,
            GAMEMTLS_CHUNK_MATERIAL_PAIRS = 0x1003,
            GAMEMTL_CHUNK_MAIN = 0x1000,
            GAMEMTL_CHUNK_FLAGS = 0x1001,
            GAMEMTL_CHUNK_PHYSICS = 0x1002,
            GAMEMTL_CHUNK_FACTORS = 0x1003,
            GAMEMTL_CHUNK_FLOTATION = 0x1004,
            GAMEMTL_CHUNK_DESC = 0x1005,
            GAMEMTL_CHUNK_INJURY = 0x1006,
            GAMEMTLPAIR_CHUNK_PAIR = 0x1000,
            GAMEMTLPAIR_CHUNK_BREAKING = 0x1002,
            GAMEMTLPAIR_CHUNK_STEP = 0x1003,
            GAMEMTLPAIR_CHUNK_COLLIDE = 0x1005,
            // SGameMaterial:
            MF_BREAKABLE = 0x00000001,
            MF_BOUNCEABLE = 0x00000004,
            MF_SKIDMARK = 0x00000008,
            MF_BLOODMARK = 0x00000010,
            MF_CLIMABLE = 0x00000020,
            MF_PASSABLE = 0x00000080,
            MF_DYNAMIC = 0x00000100,
            MF_LIQUID = 0x00000200,
            MF_SUPPRESS_SHADOWS = 0x00000400,
            MF_SUPPRESS_WALLMARKS = 0x00000800,
            MF_ACTOR_OBSTACLE = 0x00001000,
            MF_INJURIOUS = 0x10000000,
            MF_SHOOTABLE = 0x20000000,
            MF_TRANSPARENT = 0x40000000,
            MF_SLOW_DOWN = 0x8000000,
            // SGameMaterialPair:
            MPF_BREAKING_SOUNDS = 0x02,
            MPF_STEP_SOUNDS = 0x04,
            MPF_COLLIDE_SOUNDS = 0x10,
            MPF_COLLIDE_PARTICLES = 0x20,
            MPF_COLLIDE_MARKS = 0x40,
            //Engine Shader Library // guessed names
            SHADERS_CHUNK_CONSTANTS = 0,
            SHADERS_CHUNK_MATRICES = 1,
            SHADERS_CHUNK_BLENDERS = 2,
            SHADERS_CHUNK_NAMES = 3
        }

        private string[] searchTextureMaterial(string texture)
        {
            var material = File.ReadLines("materials.ini").Where(line => line.StartsWith(texture));
            if (material.Count() > 0)
            {
                string[] mat = material.First().Split('=');
                string[] all = null;
                string engine = null, compiler = null, game = null, twoSided = null;
                if (mat.Count() < 2)
                    return null;
                if (mat[1].Count() == 0)
                    return null;
                string[] conf = mat[1].Split(';');

                if (conf.Length > 0)
                {
                    if (conf.Length > 0)
                        engine = conf[0].Trim();
                    if (conf.Length > 1)
                        compiler = conf[1].Trim();
                    if (conf.Length > 2)
                        game = conf[2].Trim();
                    if (conf.Length > 3)
                        twoSided = conf[3].Trim();
                    all = new string[] { engine, compiler, game, twoSided };
                }
                return all;
            }
            //else
            //Console.WriteLine(@"material for '{0}' not found", texture);
            return null;
        }

        public static byte[] ReplaceBytes(byte[] src, byte[] search, byte[] repl)
        {
           /* StackFrame frame = new StackTrace(1, true).GetFrame(0);

            // Получаем номер строки и имя файла
            int lineNumber = frame.GetFileLineNumber();
            string fileName = frame.GetFileName();

            Console.WriteLine("Method called from {0}, line {1}", fileName, lineNumber);*/

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
            // throw new ArgumentException("bytes not found");
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

        private void test_btn_Click(object sender, EventArgs e)
        {
            string filePath = @"E:\X-Ray_CoP_SDK\editors\import\trees_palki1.~object";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Object|*.object";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                filePath = openFileDialog.FileName;
            else
                return;
            Reader(filePath);
        }

        private void FindChunk(string filePath)
        {


            int chunkIdToFind = 0x0907;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(fileStream);

                //7777
                Console.WriteLine("block " + BitConverter.ToString(reader.ReadBytes(4)));
                // Console.WriteLine(reader.ReadUInt32());
                reader.ReadBytes(4);

                // Look for the chunk with the specified ID
                while (fileStream.Position < fileStream.Length)
                {
                    int chunkId = reader.ReadInt32();
                    int chunkSize = reader.ReadInt32();

                    if (chunkId == chunkIdToFind)
                    {
                        // Found the chunk we're looking for!
                        Console.WriteLine($"Found chunk {chunkId} (size {chunkSize} bytes)");

                        // Seek to the start of the chunk data
                        long chunkDataStart = fileStream.Position;
                        fileStream.Seek(chunkDataStart, SeekOrigin.Begin);

                        // Create a BinaryWriter to write new data to the file
                        BinaryWriter writer = new BinaryWriter(fileStream);

                        // Write new data to the file
                        writer.Write(42);
                        writer.Write(3.14f);
                        writer.Write("hello world");

                        // Calculate the new size of the chunk
                        int newChunkSize = (int)(fileStream.Position - chunkDataStart);

                        // Update the chunk size in the file
                        fileStream.Seek(chunkDataStart - 4, SeekOrigin.Begin);
                        writer.Write(newChunkSize);

                        // Exit the loop, since we found the chunk we're looking for
                        break;
                    }
                    else
                    {
                        // Not the chunk we're looking for; skip over it
                        fileStream.Seek(chunkSize, SeekOrigin.Current);
                    }
                }
            }
        }

        private void Reader(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(fileStream);

                // Read the main chunk header
                int chunkId = reader.ReadInt32();
                int chunkSize = reader.ReadInt32();
                int mainChunkSize = chunkSize;
                Console.WriteLine($"header id {chunkId} size {chunkSize}");
                int subChunkId;
                int subChunkSize;
                byte[] surfacesDataOld = null;
                byte[] surfacesDataNew = null;
                long surfacesChunkStart = 0;
                byte[] authorDataOld = null;
                byte[] authorDataNew = null;
                long authorChunkStart = 0;

                BinaryWriter writer = new BinaryWriter(fileStream);
                while (chunkSize > 0)
                {
                    // Read the sub-chunk header
                    subChunkId = reader.ReadInt32();
                    subChunkSize = reader.ReadInt32();

                    switch (subChunkId)
                    {
                        case (int)Object.EOBJ_CHUNK_SURFACES_2:

                            // Seek to the start of the chunk data
                            surfacesChunkStart = fileStream.Position;

                            int mat_count = reader.ReadInt32();
                            Console.WriteLine("mat count " + mat_count);

                            // Loop through each sub-chunk
                            for (int i = 0; i < mat_count; i++)
                            {
                                Console.WriteLine("mat #" + i);
                                var mat = reader.ReadBytes(ByteCount(reader));
                                Console.WriteLine(Encoding.ASCII.GetString(mat)); //Материал
                                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                                var m_engine = reader.ReadBytes(ByteCount(reader));
                                Console.WriteLine(Encoding.ASCII.GetString(m_engine)); //движковый
                                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                                var m_compiler = reader.ReadBytes(ByteCount(reader));
                                Console.WriteLine(Encoding.ASCII.GetString(m_compiler)); //компилятор
                                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                                var m_game = reader.ReadBytes(ByteCount(reader));
                                Console.WriteLine(Encoding.ASCII.GetString(m_game)); //игра
                                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                                var m_path = reader.ReadBytes(ByteCount(reader));
                                Console.WriteLine(Encoding.ASCII.GetString(m_path)); //путь
                                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                                var m_texture = reader.ReadBytes(ByteCount(reader));
                                Console.WriteLine(Encoding.ASCII.GetString(m_texture)); //текстура
                                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                                uint m_flags = reader.ReadUInt32();
                                Console.WriteLine("m_flags " + m_flags); //флаги 0x1, если материал двусторонний, иначе 0x0
                                byte[] m_unk = reader.ReadBytes(8); //0x12 0x1 0x0 0x0 0x1 0x0 0x0 0x0	


                                if (surfacesDataOld == null)
                                {
                                    surfacesDataOld = BitConverter.GetBytes(mat_count);
                                    Console.WriteLine("create old surface data");
                                }
                                surfacesDataOld = surfacesDataOld.Concat(mat).ToArray();
                                Array.Resize(ref surfacesDataOld, surfacesDataOld.Length + 1);
                                surfacesDataOld = surfacesDataOld.Concat(m_engine).ToArray();
                                Array.Resize(ref surfacesDataOld, surfacesDataOld.Length + 1);
                                surfacesDataOld = surfacesDataOld.Concat(m_compiler).ToArray();
                                Array.Resize(ref surfacesDataOld, surfacesDataOld.Length + 1);
                                surfacesDataOld = surfacesDataOld.Concat(m_game).ToArray();
                                Array.Resize(ref surfacesDataOld, surfacesDataOld.Length + 1);
                                surfacesDataOld = surfacesDataOld.Concat(m_path).ToArray();
                                Array.Resize(ref surfacesDataOld, surfacesDataOld.Length + 1);
                                surfacesDataOld = surfacesDataOld.Concat(m_texture).ToArray();
                                surfacesDataOld = surfacesDataOld.Concat(BitConverter.GetBytes(m_flags)).ToArray();
                                Array.Resize(ref surfacesDataOld, surfacesDataOld.Length + 1);
                                surfacesDataOld = surfacesDataOld.Concat(m_unk).ToArray();

                                string[] mats = searchTextureMaterial(Encoding.ASCII.GetString(m_path));

                                void newData()
                                {
                                    if (surfacesDataNew == null)
                                    {
                                        //если не было данных добавляем сначала кол-во материалов, потом материалы
                                        surfacesDataNew = BitConverter.GetBytes(mat_count);
                                        Console.WriteLine("create new surface data " + mat_count);
                                    }

                                    byte[] fix_flags = { 0x0, 0x0, 0x0, 0x0 };
                                    Console.WriteLine("m_flags " + m_flags);
                                    if (m_flags == 1)
                                        fix_flags[1] = 0x1;

                                    //далее пишем список материалов
                                    surfacesDataNew = surfacesDataNew.Concat(mat).ToArray();
                                    Array.Resize(ref surfacesDataNew, surfacesDataNew.Length + 1);
                                    surfacesDataNew = surfacesDataNew.Concat(m_engine).ToArray();
                                    Array.Resize(ref surfacesDataNew, surfacesDataNew.Length + 1);
                                    surfacesDataNew = surfacesDataNew.Concat(m_compiler).ToArray();
                                    Array.Resize(ref surfacesDataNew, surfacesDataNew.Length + 1);
                                    surfacesDataNew = surfacesDataNew.Concat(m_game).ToArray();
                                    Array.Resize(ref surfacesDataNew, surfacesDataNew.Length + 1);
                                    surfacesDataNew = surfacesDataNew.Concat(m_path).ToArray();
                                    Array.Resize(ref surfacesDataNew, surfacesDataNew.Length + 1);
                                    surfacesDataNew = surfacesDataNew.Concat(m_texture).ToArray();
                                    surfacesDataNew = surfacesDataNew.Concat(fix_flags).ToArray();
                                    Array.Resize(ref surfacesDataNew, surfacesDataNew.Length + 1);
                                    surfacesDataNew = surfacesDataNew.Concat(m_unk).ToArray();
                                }

                                if (mats == null)
                                {
                                    newData();
                                    continue;
                                }
                                Console.WriteLine(@"m_engine '{0}' m_compiler '{1}' m_game '{2}' two sided '{3}'", mats[0], mats[1], mats[2], mats[3]);
                                Console.WriteLine("mats :" + mats.Length);
                                if (mats[0] != null && mats[0].Length > 0)
                                {
                                    Console.WriteLine("TODO: меняем движковый шейдер на {0}", mats[0]);
                                    m_engine = Encoding.ASCII.GetBytes(mats[0]);
                                }
                                if (mats[1] != null && mats[1].Length > 0)
                                {
                                    Console.WriteLine("TODO: меняем шейдер компилятора на {0}", mats[1]);
                                    m_compiler = Encoding.ASCII.GetBytes(mats[1]);
                                }
                                if (mats[2] != null && mats[2].Length > 0)
                                {
                                    Console.WriteLine("TODO: меняем шейдер игры на {0}", mats[2]);
                                    m_game = Encoding.ASCII.GetBytes(mats[2]);
                                }
                                if (mats[3] != null && mats[3].Length > 0)
                                {
                                    Console.WriteLine("TODO: меняем тип материала на двухсторонний {0}", mats[3]);
                                    if (int.TryParse(mats[3], out int num))
                                        m_flags = Convert.ToUInt32(num);
                                }


                                newData();
                            }

                           /* Console.WriteLine(("new surface data " + Encoding.ASCII.GetString(surfacesDataNew)));
                            Console.WriteLine(("old surface data " + Encoding.ASCII.GetString(surfacesDataOld)));*/


                            //записываем новый размер чанка
                            if (surfacesDataNew != null)
                            {
                                long chunkEnd = fileStream.Position;
                                writer.Seek(Convert.ToInt32(surfacesChunkStart - 4), SeekOrigin.Begin);
                                writer.Write(surfacesDataNew.Length);
                                Console.WriteLine("new surface chunk size " + surfacesDataNew.Length + " old " + surfacesDataOld.Length);
                                fileStream.Position = chunkEnd;
                                Console.WriteLine(fileStream.Position.ToString());
                            }
                            break;
                        case (int)Object.EOBJ_CHUNK_REVISION:

                            // Seek to the start of the chunk data
                            authorChunkStart = fileStream.Position;

                            byte[] creator = reader.ReadBytes(ByteCount(reader));
                            Console.WriteLine(Encoding.ASCII.GetString(creator)); //Создатель
                            reader.BaseStream.Position = reader.BaseStream.Position + 1;

                            uint create_date = reader.ReadUInt32();

                            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            dateTime = dateTime.AddSeconds(create_date).ToLocalTime();
                            Console.WriteLine(dateTime); //дата создания

                            byte[] mod = reader.ReadBytes(ByteCount(reader));
                            Console.WriteLine(Encoding.ASCII.GetString(mod)); //Мод
                            reader.BaseStream.Position = reader.BaseStream.Position + 1;

                            uint mod_date = reader.ReadUInt32();

                            dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            dateTime = dateTime.AddSeconds(mod_date).ToLocalTime();
                            Console.WriteLine(dateTime); //дата мода

                            authorDataOld = creator;
                            Array.Resize(ref authorDataOld, authorDataOld.Length + 1);
                            authorDataOld = authorDataOld.Concat(BitConverter.GetBytes(create_date)).ToArray(); // + create_date
                            authorDataOld = authorDataOld.Concat(mod).ToArray(); // + mod
                            Array.Resize(ref authorDataOld, authorDataOld.Length + 1);
                            authorDataOld = authorDataOld.Concat(BitConverter.GetBytes(mod_date)).ToArray(); // + mod_date

                            uint now = Convert.ToUInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                            Console.WriteLine(now);

                            authorDataNew = creator;
                            Array.Resize(ref authorDataNew, authorDataNew.Length + 1);
                            authorDataNew = authorDataNew.Concat(BitConverter.GetBytes(create_date)).ToArray(); // + create_date
                            authorDataNew = authorDataNew.Concat(Encoding.ASCII.GetBytes("yoba")).ToArray(); // + mod
                            Array.Resize(ref authorDataNew, authorDataNew.Length + 1);
                            authorDataNew = authorDataNew.Concat(BitConverter.GetBytes(now)).ToArray(); // + mod_date

                            //записываем новый размер чанка
                            if (authorDataNew != null)
                            {
                                long chunkEnd = fileStream.Position;
                                writer.Seek(Convert.ToInt32(authorChunkStart - 4), SeekOrigin.Begin);
                                writer.Write(authorDataNew.Length);
                                Console.WriteLine("new surface chunk size " + authorDataNew.Length + " old " + authorDataOld.Length);
                                fileStream.Position = chunkEnd;
                                Console.WriteLine(fileStream.Position.ToString());
                            }
                            break;
                        default:
                            // Skip over any unknown sub-chunks
                            reader.BaseStream.Seek(subChunkSize, SeekOrigin.Current);
                            break;
                    }

                    // Update the chunk size to account for the sub-chunk that was just read
                    chunkSize -= (subChunkSize + 8);
                }

                reader.Close();
                writer.Close();

                byte[] buffer = File.ReadAllBytes(filePath);
                byte[] res = ReplaceBytes(buffer, surfacesDataOld, surfacesDataNew);
                if (buffer != res)
                {
                    /* Console.WriteLine("old surface data: " + BitConverter.ToString(surfacesDataOld, 0, surfacesDataOld.Length));
                     Console.WriteLine("new surface data: " + BitConverter.ToString(surfacesDataNew, 0, surfacesDataNew.Length));*/
                    File.WriteAllBytes(filePath, res);

                }
                buffer = File.ReadAllBytes(filePath);
                res = ReplaceBytes(buffer, authorDataOld, authorDataNew);
                if (buffer != res)
                {
                    /* Console.WriteLine("old surface data: " + BitConverter.ToString(surfacesDataOld, 0, surfacesDataOld.Length));
                     Console.WriteLine("new surface data: " + BitConverter.ToString(surfacesDataNew, 0, surfacesDataNew.Length));*/
                    File.WriteAllBytes(filePath, res);
                }
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    byte[] mainChunkNewSize = BitConverter.GetBytes(fs.Length - 8); // bytes to replace with
                    fs.Seek(4, SeekOrigin.Begin); // move the file pointer to the offset
                    fs.Write(mainChunkNewSize, 0, 4); // write the new bytes


                    Console.WriteLine("update main chunk size {0} old {1}", BitConverter.ToInt64(mainChunkNewSize, 0), mainChunkSize);
                }
            }
        }

        private void findChunkInChunk(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(fileStream);

                // Read the main chunk header
                int chunkId = reader.ReadInt32();
                int chunkSize = reader.ReadInt32();
                int subChunkId;
                int subChunkSize;

                while (chunkSize > 0)
                {
                    // Read the sub-chunk header
                    subChunkId = reader.ReadInt32();
                    subChunkSize = reader.ReadInt32();

                    switch (subChunkId)
                    {
                        case 0x0101: // A sub-chunk of type 0x1000
                                     // Read the data for this sub-chunk
                            byte[] data = reader.ReadBytes(subChunkSize);
                            Console.WriteLine("Sub-chunk of type 0x0101 found, length: {0}", subChunkSize);
                            break;
                        case (int)Object.EOBJ_CHUNK_REVISION:
                            byte[] creator = reader.ReadBytes(ByteCount(reader));
                            Console.WriteLine(Encoding.ASCII.GetString(creator)); //Создатель
                            reader.BaseStream.Position = reader.BaseStream.Position + 1;

                            uint create_date = reader.ReadUInt32();

                            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            dateTime = dateTime.AddSeconds(create_date).ToLocalTime();
                            Console.WriteLine(dateTime); //дата создания

                            byte[] mod = reader.ReadBytes(ByteCount(reader));
                            Console.WriteLine(Encoding.ASCII.GetString(mod)); //Мод
                            reader.BaseStream.Position = reader.BaseStream.Position + 1;

                            uint mod_date = reader.ReadUInt32();

                            dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            dateTime = dateTime.AddSeconds(mod_date).ToLocalTime();
                            Console.WriteLine(dateTime); //дата мода

                            /*  authorDataOld = creator;
                              Array.Resize(ref authorDataOld, authorDataOld.Length + 1);
                              authorDataOld = authorDataOld.Concat(BitConverter.GetBytes(create_date)).ToArray(); // + create_date
                              authorDataOld = authorDataOld.Concat(mod).ToArray(); // + mod
                              Array.Resize(ref authorDataOld, authorDataOld.Length + 1);
                              authorDataOld = authorDataOld.Concat(BitConverter.GetBytes(mod_date)).ToArray(); // + mod_date*/
                            break;
                        default:
                            // Skip over any unknown sub-chunks
                            reader.BaseStream.Seek(subChunkSize, SeekOrigin.Current);
                            break;
                    }

                    // Update the chunk size to account for the sub-chunk that was just read
                    chunkSize -= (subChunkSize + 8);
                }
            }
        }
    }
}
