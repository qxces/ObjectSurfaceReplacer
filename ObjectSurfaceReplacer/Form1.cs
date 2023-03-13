using System;
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
            var material = File.ReadLines("materials.ini").Where(line => line.Contains(texture));
            if (material.Count() > 0)
            {
                string[] mat = material.First().Split('=');
                string[] all = null;
                string engine = null, compiler = null, game = null, twoSided = null;
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
            else
                Console.WriteLine(@"material for '{0}' not found", texture);
            return null;
        }

        private void ReplaceSurfaces_btn_Click(object sender, EventArgs e)
        {

            //string objectPath = @"E:\X-Ray_CoP_SDK\editors\import\trees_palki11.object";
            string objectPath = @"E:\X-Ray_CoP_SDK\editors\import\trees_palki1.~object";
            //string objectPath = @"E:\X-Ray_CoP_SDK\editors\import\SUKA_BLEAT.object";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Object|*.object";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                objectPath = openFileDialog.FileName;
            else
                return;

            //creating an object of Stream
            FileStream stream = new FileStream(objectPath, FileMode.Open,
            FileAccess.Read, FileShare.Read);
            //creating BinaryReader using Stream object

            byte[] mainChunkOldSize;
            byte[] mainChunkNewSize;
            byte[] texturesChunkOldSize;
            byte[] texturesChunkNewSize;
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
                //невозможно прочитать из-за сложной вложенности, пропускаем
                reader.ReadBytes((int)size);

                //07-09-00-00
                Console.WriteLine("block " + BitConverter.ToString(reader.ReadBytes(4)));
                texturesChunkOldSize = reader.ReadBytes(4);
                var mat_cnt = reader.ReadUInt32();
                Console.WriteLine(mat_cnt);

                //TODO: скорее всего здесь будет общая строка к которой плюсуются все материалы

                for (int i = 1; i < mat_cnt + 1; i++)
                {
                    Console.WriteLine("texture #" + i.ToString());
                    byte[] material = reader.ReadBytes(ByteCount(reader));
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(material)); //Название

                    reader.BaseStream.Position = reader.BaseStream.Position + 1;
                    byte[] m_engine = reader.ReadBytes(ByteCount(reader)); //Характеристика для движка
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(m_engine));
                    reader.BaseStream.Position = reader.BaseStream.Position + 1;
                    byte[] m_compiler = reader.ReadBytes(ByteCount(reader));
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(m_compiler)); //Характеристика для компилятор
                    reader.BaseStream.Position = reader.BaseStream.Position + 1;
                    byte[] m_game = reader.ReadBytes(ByteCount(reader));
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(m_game)); //Характеристика для игры
                    reader.BaseStream.Position = reader.BaseStream.Position + 1;
                    var texture = reader.ReadBytes(ByteCount(reader));
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(texture)); //Путь к текстуре
                    reader.BaseStream.Position = reader.BaseStream.Position + 1;
                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(ByteCount(reader)))); //Texture
                    reader.BaseStream.Position = reader.BaseStream.Position + 1;
                    Console.WriteLine(reader.ReadUInt32()); //0x1, если материал двусторонний, иначе 0x0
                    Console.WriteLine(BitConverter.ToString(reader.ReadBytes(8))); //0x12 0x1 0x0 0x0 0x1 0x0 0x0 0x0

                    string[] mats = searchTextureMaterial(System.Text.Encoding.ASCII.GetString(texture));
                    Console.WriteLine(@"m_engine '{0}' m_compiler '{1}' m_game '{2}' two sided '{3}'", mats[0], mats[1], mats[2], mats[3]);
                    if (mats[0] != null)
                        Console.WriteLine("TODO: меняем движковый шейдер на {0}", mats[0]);
                    if (mats[1] != null)
                        Console.WriteLine("TODO: меняем шейдер компилятора на {0}", mats[1]);
                    if (mats[2] != null)
                        Console.WriteLine("TODO: меняем шейдер игры на {0}", mats[2]);
                    if (mats[3] != null)
                        Console.WriteLine("TODO: меняем тип материала на двухсторонний {0}", mats[3]);

                    //TODO: собрать новые строки, не забыть про нули между ними
                }
                /* texturesChunkNewSize = BitConverter.GetBytes(BitConverter.ToUInt32(texturesChunkOldSize, 0) - BitConverter.ToUInt32(oldSize, 0) + BitConverter.ToUInt32(newSize, 0));*/

                //07-09-00-00
                byte[] authorChunk = reader.ReadBytes(4);
                Console.WriteLine("block " + BitConverter.ToString(authorChunk));

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

            /*
            byte[] buffer = File.ReadAllBytes(objectPath);
            byte[] res = ReplaceBytes(buffer, authorChunkOld, authorChunkNew);
            byte[] mainChunk = new byte[] { 0x77, 0x77, 0x00, 0x00 };
            byte[] mainChunk1 = mainChunk.Concat(mainChunkOldSize).ToArray();
            Console.WriteLine("chunk old " + BitConverter.ToString(mainChunk1));
            byte[] mainChunk2 = mainChunk.Concat(mainChunkNewSize).ToArray();
            Console.WriteLine("chunk new " + BitConverter.ToString(mainChunk2));
            res = ReplaceBytes(res, mainChunk1, mainChunk2);
            File.WriteAllBytes(@"E:\X-Ray_CoP_SDK\editors\import\SUKA_BLEAT.object", res);
            */
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
                byte[] authorDataOld = null;
                byte[] authorDataNew = null;
                long authorChunkStart = 0;

                while (chunkSize > 0)
                {
                    // Read the sub-chunk header
                    subChunkId = reader.ReadInt32();
                    subChunkSize = reader.ReadInt32();

                    switch (subChunkId)
                    {
                        case (int)Object.EOBJ_CHUNK_VERSION:
                            byte[] data = reader.ReadBytes(subChunkSize);
                            Console.WriteLine("version: {0}", BitConverter.ToInt16(data, 0));
                            break;
                        case (int)Object.EOBJ_CHUNK_USERDATA:
                            data = reader.ReadBytes(subChunkSize);
                            Console.WriteLine("userdata " + System.Text.Encoding.ASCII.GetString(data));
                            break;
                        case (int)Object.EOBJ_CHUNK_LOD_REF:
                            data = reader.ReadBytes(subChunkSize);
                            Console.WriteLine("lod " + System.Text.Encoding.ASCII.GetString(data));
                            break;
                        case (int)Object.EOBJ_CHUNK_FLAGS:
                            data = reader.ReadBytes(subChunkSize);
                            Console.WriteLine("type " + BitConverter.ToInt32(data, 0));
                            break;

                        //хз как потом вылезти из этой вложенности
                        /*     case (int)Object.EOBJ_CHUNK_MESHES: // A sub-chunk of type 0x2000
                                 int emptyChunk = reader.ReadInt32();
                                 int emptyChunkSize = reader.ReadInt32();
                                 byte[] emptyChunkData = reader.ReadBytes(subChunkSize);
                                 //Console.WriteLine($"{emptyChunk} {emptyChunkSize} {BitConverter.ToString(emptyChunkData)}");

                                 int numSubChunks = reader.ReadInt32();
                                 Console.WriteLine("Sub-chunk of type 0x0910 found, number of sub-chunks: {0}", numSubChunks);


                                 // Loop through each sub-chunk
                                 for (int i = 0; i < numSubChunks; i++)
                                 {
                                     var mesh = reader.ReadBytes(ByteCount(reader));
                                     Console.WriteLine(System.Text.Encoding.ASCII.GetString(mesh)); //Материал
                                     reader.BaseStream.Position = reader.BaseStream.Position + 1;
                                     // Read the sub-sub-chunk header
                                     // int subSubChunkId = reader.ReadInt32();
                                     // int subSubChunkSize = reader.ReadInt32();

                                     //  Read the data for the sub-sub-chunk
                                     // byte[] subSubChunkData = reader.ReadBytes(subSubChunkSize);
                                     // Console.WriteLine("Sub-sub-chunk found, length: {0}", subSubChunkSize);
                                 }

                                 break;*/
                        case (int)Object.EOBJ_CHUNK_SURFACES_2:
                            int mat_count = reader.ReadInt32();
                            Console.WriteLine("mat count " + mat_count);

                            // Loop through each sub-chunk
                            for (int i = 0; i < mat_count; i++)
                            {
                                Console.WriteLine("mat #" + i);
                                var mat = reader.ReadBytes(ByteCount(reader));
                                Console.WriteLine(System.Text.Encoding.ASCII.GetString(mat)); //Материал
                                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                                var m_engine = reader.ReadBytes(ByteCount(reader));
                                Console.WriteLine(System.Text.Encoding.ASCII.GetString(m_engine)); //движковый
                                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                                var m_compiler = reader.ReadBytes(ByteCount(reader));
                                Console.WriteLine(System.Text.Encoding.ASCII.GetString(m_compiler)); //компилятор
                                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                                var m_game = reader.ReadBytes(ByteCount(reader));
                                Console.WriteLine(System.Text.Encoding.ASCII.GetString(m_game)); //игра
                                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                                var m_path = reader.ReadBytes(ByteCount(reader));
                                Console.WriteLine(System.Text.Encoding.ASCII.GetString(m_path)); //путь
                                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                                var m_texture = reader.ReadBytes(ByteCount(reader));
                                Console.WriteLine(System.Text.Encoding.ASCII.GetString(m_texture)); //текстура
                                reader.BaseStream.Position = reader.BaseStream.Position + 1;

                                uint flags = reader.ReadUInt32();
                                Console.WriteLine(flags); //флаги 0x1, если материал двусторонний, иначе 0x0
                                reader.ReadBytes(8); //0x12 0x1 0x0 0x0 0x1 0x0 0x0 0x0	
                            }
                            break;
                        case (int)Object.EOBJ_CHUNK_REVISION:

                            // Seek to the start of the chunk data
                            authorChunkStart = fileStream.Position;

                            byte[] creator = reader.ReadBytes(ByteCount(reader));
                            Console.WriteLine(System.Text.Encoding.ASCII.GetString(creator)); //Создатель
                            reader.BaseStream.Position = reader.BaseStream.Position + 1;

                            uint create_date = reader.ReadUInt32();

                            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            dateTime = dateTime.AddSeconds(create_date).ToLocalTime();
                            Console.WriteLine(dateTime); //дата создания

                            byte[] mod = reader.ReadBytes(ByteCount(reader));
                            Console.WriteLine(System.Text.Encoding.ASCII.GetString(mod)); //Мод
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
                            BinaryWriter w = new BinaryWriter(fileStream);
                            w.Seek(Convert.ToInt32(authorChunkStart - 4), SeekOrigin.Begin);
                            w.Write(authorDataNew.Length);

                            break;
                        default:
                            // Skip over any unknown sub-chunks
                            reader.BaseStream.Seek(subChunkSize, SeekOrigin.Current);
                            break;
                    }

                    // Update the chunk size to account for the sub-chunk that was just read
                    chunkSize -= (subChunkSize + 8);
                }

                //после всех операций получаем размер главного блока и перезаписываем его
                long mainChunkNewSize = fileStream.Length - 8;
                BinaryWriter writer = new BinaryWriter(fileStream);

                writer.Seek(4, SeekOrigin.Begin);
                writer.Write(mainChunkNewSize);

                reader.Close();
                writer.Close();

                //перезаписываем блок с данными автора
                if (authorDataNew != null)
                {
                    byte[] buffer = File.ReadAllBytes(filePath);
                    byte[] res = ReplaceBytes(buffer, authorDataOld, authorDataNew);
                   // File.WriteAllBytes(@"E:\X-Ray_CoP_SDK\editors\import\SUKA_BLEAT.object", res);
                    File.WriteAllBytes(filePath, res);
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
                            Console.WriteLine(System.Text.Encoding.ASCII.GetString(creator)); //Создатель
                            reader.BaseStream.Position = reader.BaseStream.Position + 1;

                            uint create_date = reader.ReadUInt32();

                            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            dateTime = dateTime.AddSeconds(create_date).ToLocalTime();
                            Console.WriteLine(dateTime); //дата создания

                            byte[] mod = reader.ReadBytes(ByteCount(reader));
                            Console.WriteLine(System.Text.Encoding.ASCII.GetString(mod)); //Мод
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
