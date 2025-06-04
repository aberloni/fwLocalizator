using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localizator
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public class CsvSerializer
    {
        public const string parserExtDot = "." + parserExt;
        public const string parserExt = "parser";

        public static void save(CsvParser csv, string path)
        {
            Debug.Assert(path.Length > 0, "path is empty ?");

            byte[] bytes = null;

            try
            {
                bytes = serializeObject(csv);
            }
            catch
            {
                Debug.LogWarning("can't serialize : " + csv);
            }

            if (bytes == null) return;

            saveBytesToFile(path, bytes);

            Debug.Log("saved csv @ " + path);

            CsvParser.refreshCache();
        }

        public static CsvParser load(string path)
        {
            //Debug.Log("load parser @ " + path);

            byte[] fileBytes;
            loadBytesFromFile(path, out fileBytes);

            if (fileBytes == null)
            {
                Debug.LogWarning("loading @failed " + path);
                return null;
            }

            object fileDeserial = deserializeObject(fileBytes);
            return fileDeserial as CsvParser;
        }

        public static byte[] serializeObject(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();

            try
            {
                bf.Serialize(stream, obj);
            }
            catch (Exception e)
            {
                Debug.LogWarning("issue during serialization : " + obj);
                Debug.LogError(e);
                return null;
            }

            return stream.GetBuffer();
        }

        public static object deserializeObject(byte[] buffer)
        {
            MemoryStream stream = new MemoryStream(buffer);
            BinaryFormatter bf = new BinaryFormatter();

            object output = null;
            try
            {
                output = bf.Deserialize(stream);
            }
            catch (Exception e)
            {
                Debug.LogWarning("issue during deserialization : " + buffer);
                Debug.LogError(e);
                output = null;
            }

            return output;
        }

        static bool saveBytesToFile(string path, byte[] bytes)
        {
            if (bytes == null)
            {
                Debug.LogError("bytes[] is null ?");
                return false;
            }

            checkFolderExists(path);
            File.WriteAllBytes(path, bytes);
            return true;
        }

        static bool loadBytesFromFile(string path, out byte[] result)
        {
            checkFolderExists(path);
            //result = new byte[];
            try
            {
                result = File.ReadAllBytes(path);
            }
            catch
            {
                Debug.LogWarning("can't read bytes of " + path);
                result = null;
            }

            return result != null;
        }


        /// <summary>
        /// true = folder is there
        /// </summary>
        static bool checkFolderExists(string path)
        {

            if (path == null)
            {
                Debug.LogError("empty path given");
                return false;
            }

            path = path.Trim();

            if (path.Length < 3)
            {
                Debug.LogError("path is too small ? " + path.Length);
                return false;
            }

            //TiniesLogs.logSave(path);

            if (path.Contains("."))
            {
                // folder path from full file path
                path = path.Substring(0, path.LastIndexOf("/"));
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return true;
        }

    }
}
