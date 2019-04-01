using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ulid.Cli.Tests
{
    public class TextWriterBridge : TextWriter
    {
        public readonly List<string> Log = new List<string>();

        public TextWriterBridge()
        {
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(string value)
        {
            Log.Add(value);
        }

        public static IDisposable BeginSetConsoleOut(out List<String> log)
        {
            var current = Console.Out;
            var tw = new TextWriterBridge();
            log = tw.Log;
            Console.SetOut(tw);
            return new Scope(current);
        }

        public struct Scope : IDisposable
        {
            TextWriter writer;

            public Scope(TextWriter writer)
            {
                this.writer = writer;
            }

            public void Dispose()
            {
                Console.SetOut(writer);
            }
        }
    }
}
