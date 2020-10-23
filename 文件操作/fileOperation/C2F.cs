using System.Runtime.InteropServices;

namespace ServiceContentGisqOAStore
{
    class C2F
    {
        [DllImport("c2p_dll.dll",CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OpenDoc(string pszCebFileName, string pszPdfFileNme);
    }
}
