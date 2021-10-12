using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Text;

namespace Genshmup.HelperClasses
{
    public static class ResourceLoader
    {
        private static PrivateFontCollection _privateFontCollection;

        static ResourceLoader()
        {
            _privateFontCollection = new PrivateFontCollection();
        }

        public static Stream? LoadResource(Assembly? a, string searchstring)
        {
            if (a == null) a = Assembly.GetExecutingAssembly();
            return a.GetManifestResourceStream(a.GetManifestResourceNames().ToList().Find(val => val.Contains(searchstring)) ?? "plc");
        }

        public static FontFamily? LoadFont(Assembly? a, string name)
        {
            if (a == null) a = Assembly.GetExecutingAssembly();
            Stream? st = a.GetManifestResourceStream(a.GetManifestResourceNames().ToList().Find(val => val.Contains(name)) ?? "plc");
            if (st == null) return null;
            List<byte> bytes = new();
            while (st.Position < st.Length)
            {
                int j = st.ReadByte();
                if (j == -1) break;
                else bytes.Add((byte)j);
            }
            byte[] fontBytes = bytes.ToArray();
            var handle = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
            IntPtr pointer = handle.AddrOfPinnedObject();
            try
            {
                _privateFontCollection.AddMemoryFont(pointer, fontBytes.Length);
            }
            finally
            {
                handle.Free();
            }
            return _privateFontCollection.Families.FirstOrDefault(x => x.Name == name);
        }
    }
}
