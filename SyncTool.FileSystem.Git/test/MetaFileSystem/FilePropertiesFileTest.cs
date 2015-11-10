﻿using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace SyncTool.FileSystem.Git
{
    public class FilePropertiesFileTest
    {
        readonly JsonSerializer m_Serializer = new JsonSerializer();

        [Fact]
        public void Open_returns_json_readable_stream()
        {
            var file = new EmptyFile("file1") { LastWriteTime = DateTime.Now, Length = 42};
            var filePropertiesFile = FilePropertiesFile.ForFile(file);

            FileProperties properties;
            using (var jsonReader = new JsonTextReader(new StreamReader(filePropertiesFile.OpenRead())))
            {
                properties = m_Serializer.Deserialize<FileProperties>(jsonReader);
            }

            Assert.NotNull(properties);
            Assert.Equal(file.Name, properties.Name);
            Assert.Equal(file.Length, properties.Length);
            Assert.Equal(file.LastWriteTime, properties.LastWriteTime);

        }        
    }
}