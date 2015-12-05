﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using W3DT.CASC;

namespace W3DT.Events
{
    class FileExploreHitArgs : EventArgs
    {
        public string ID { get; private set; }
        public StringHashPair File { get; private set; }

        public FileExploreHitArgs(string identifier, StringHashPair file)
        {
            ID = identifier;
            File = file;
        }
    }
}