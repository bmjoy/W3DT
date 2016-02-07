﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace W3DT.Formats
{
    public class ADTException : Exception
    {
        public ADTException(string message) : base(message) { }
    }

    public class ADTFile : ChunkedFormatBase
    {
        public ADTFile(string file) : base(file) { }

        public override void storeChunk(Chunk_Base chunk)
        {
            Chunks.Add(chunk);
        }

        public override Chunk_Base lookupChunk(UInt32 magic)
        {
            return new Chunk_Base(this);
        }

        public override string getFormatName()
        {
            return "ADT";
        }
    }
}
