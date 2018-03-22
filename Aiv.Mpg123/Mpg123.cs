using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace Aiv.Mpg123
{
    public class Mpg123 : IDisposable
    {
        public class ErrorException : Exception
        {
            public ErrorException(Errors error) : base(PlainStrError(error))
            {
            }
        }

        public enum Text_Encoding
        {
            UNKNOWN = 0,
            UTF8 = 1,
            LATIN1 = 2,
            ICY = 3,
            CP1252 = 4,
            UTF16 = 5,
            UTF16BOM = 6,
            UTF16BE = 7,
            MAX = 8
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Mpg123_text
        {
            char[] lang;
            char[] id;
            IntPtr description;
            IntPtr text;
        }

        public enum Errors
        {
            OK = 0,
        }

        static private bool libraryInitialized;
        static public bool IsLibraryInitialized
        {
            get
            {
                return libraryInitialized;
            }
        }

        public static IEnumerable<string> Decoders
        {
            get
            {
                IntPtr decodersPtr = NativeMethods.NativeMpg123Decoders();
                int offset = 0;
                while (true)
                {
                    IntPtr decoderPtr = Marshal.ReadIntPtr(decodersPtr, offset);
                    if (decoderPtr == IntPtr.Zero)
                    {
                        yield break;
                    }
                    yield return Marshal.PtrToStringAnsi(decoderPtr);
                    offset += Marshal.SizeOf<IntPtr>();
                }
            }
        }

        public static string PlainStrError(Errors error)
        {
            IntPtr errorPtr = NativeMethods.NativeMpg123PlainStrError(error);
            if (errorPtr == IntPtr.Zero)
                return "unknown error";
            string errorMessage = Marshal.PtrToStringAnsi(errorPtr);
            return errorMessage;
        }

        static Mpg123()
        {
            Errors error = NativeMethods.NativeMpg123Init();
            if (error != Errors.OK)
                throw new ErrorException(error);
            libraryInitialized = true;
        }

        public bool HasValidHandle
        {
            get
            {
                return handle != IntPtr.Zero;
            }
        }

        protected IntPtr handle;

        public Mpg123(string decoder = null)
        {
            IntPtr decoderPtr = IntPtr.Zero;
            if (decoder != null)
            {
                decoderPtr = Marshal.StringToHGlobalAnsi(decoder);
            }
            int error = 0;
            handle = NativeMethods.NativeMpg123New(decoderPtr, ref error);
            if (decoderPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(decoderPtr);
            if (handle == IntPtr.Zero)
                throw new ErrorException((Errors)error);
        }

        protected bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool isDisposing)
        {
            if (disposed)
                return;

            if (handle != IntPtr.Zero)
            {
                NativeMethods.NativeMpg123Delete(handle);
                handle = IntPtr.Zero;
            }

            if (isDisposing)
            {
                // cleanup dependancies
            }

            disposed = true;
        }

        ~Mpg123()
        {
            Dispose(false);
        }

        public static bool Store_UTF8(string TargetString, byte[] buffer, uint size)
        {
            IntPtr targetString = IntPtr.Zero;
            IntPtr source = Marshal.AllocHGlobal(buffer.Length);
            IntPtr source_size = new IntPtr(size);
            if (TargetString != null)
            {
                targetString = Marshal.StringToHGlobalAnsi(TargetString);
            }
            int result = NativeMethods.NativeMpg123Store(targetString, Text_Encoding.UTF8, source, source_size);
            if (result == 0)
            {
                return false;
                //on error, mpg123_free_string is called on sb)
            }
            else return true;
        }

        public static Text_Encoding EncFromID3(byte[] bytes)
        {
            IntPtr enc_byte = Marshal.AllocHGlobal(bytes.Length);
            return NativeMethods.NativeMpg123EncFromID3(enc_byte);
        }
    }
}
