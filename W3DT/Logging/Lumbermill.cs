﻿using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using W3DT.Runners;

namespace W3DT.Logging
{
    public class Lumbermill : RunnerBase
    {
        private TextWriter writer;

        public Lumbermill(TextWriter writer)
        {
            this.writer = writer;
        }

        public override void Work()
        {
            while (true)
            {
                if (Log.Pipe.Count > 0)
                {
                    string message;

                    if (Log.Pipe.TryDequeue(out message))
                    {
                        // Write to the log file.
                        writer.WriteLine(string.Format("[{0}] {1}", getTimestamp(), message));

                        try
                        {
                            writer.Flush();
                        }
                        catch (ObjectDisposedException e)
                        {
                            // Application probably closed before we could be done.
                        }

                        // Write to the debugger.
                        Debug.WriteLine(message);
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private string getTimestamp()
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff");
        }

        private bool isValid()
        {
            return writer != null;
        }

        public new void Kill()
        {
            if (isValid())
                writer.Close();

            if (thread != null && thread.IsAlive)
                thread.Abort();
        }
    }
}
