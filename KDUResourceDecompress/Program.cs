using System;
using System.IO;
using System.Runtime.InteropServices;

namespace KDUResourceDecompress
{
    internal class Program
    {
        [DllImport("msdelta.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ApplyDeltaB(
                DeltaFileType fileType,
                DeltaInput source,
                DeltaInput delta,
                ref DeltaOutput target);

        [DllImport("msdelta.dll", SetLastError = true)]
        public static extern void DeltaFree(IntPtr memory);

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: KDUResourceDecompress.exe <filename>");
                Console.WriteLine("Or use 'KDUResourceDecompress.exe <wildcard>' (e.g. '*.bin') to decompress all .bin files in current directory'");
                return;
            }

            if (args[0].Contains("*")) // Simple wildcard check
            {
                foreach (var file in Directory.EnumerateFiles(Environment.CurrentDirectory, args[0], SearchOption.TopDirectoryOnly))
                    Decompress(file);
            }
            else
                Decompress(args[0]);
        }

        static void Decompress(string fileName)
        {
            var deltaBuffer = File.ReadAllBytes(fileName);

            // Decrypt the source buffer
            var xorKey = 0xF62E6CE0; // PROVIDER_RES_KEY
            EncodeBuffer(deltaBuffer, xorKey);

            var deltaHandle = GCHandle.Alloc(deltaBuffer, GCHandleType.Pinned);

            var sourceInput = new DeltaInput();
            var deltaInput = new DeltaInput
            {
                lpStart = deltaHandle.AddrOfPinnedObject(),
                uSize = (IntPtr)deltaBuffer.Length,
                Editable = false
            };

            var targetOutput = new DeltaOutput();

            var result = ApplyDeltaB(DeltaFileType.Raw, sourceInput, deltaInput, ref targetOutput);
            if (!result)
            {
                var error = Marshal.GetLastWin32Error();
                Console.WriteLine($"ApplyDeltaB failed with error code {error}: " + fileName);
            }
            else
            {
                var targetBuffer = new byte[(int)targetOutput.uSize];
                Marshal.Copy(targetOutput.lpStart, targetBuffer, 0, (int)targetOutput.uSize);

                // Use the decompressed data in targetBuffer
                Console.WriteLine("Decompression successful: " + fileName);
            }

            var decompressed = new byte[(int)targetOutput.uSize];
            Marshal.Copy(targetOutput.lpStart, decompressed, 0, (int)targetOutput.uSize);

            File.WriteAllBytes(fileName + ".dec", decompressed);

            deltaHandle.Free();

            // Clean up the target buffer if necessary
            if (targetOutput.lpStart != IntPtr.Zero)
                DeltaFree(targetOutput.lpStart);
        }

        static void EncodeBuffer(byte[] buffer, uint key)
        {
            if (buffer == null || buffer.Length == 0)
                return;

            var k = key;
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] ^= (byte)k;
                k = RotateLeft(k, 1);
            }
        }

        static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        public enum DeltaFileType : uint
        {
            Raw = 0x01
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeltaInput
        {
            public IntPtr lpStart;
            public IntPtr uSize;
            public bool Editable;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeltaOutput
        {
            public IntPtr lpStart;
            public IntPtr uSize;
        }
    }
}
