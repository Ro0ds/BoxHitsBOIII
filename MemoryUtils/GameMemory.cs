using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace BoxHitsBOIII.MemoryUtils
{
    public static class GameMemory
    {
        #region MemoryImports
        public const int PROCESS_WM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);
        #endregion MemoryImports 

        public static Process? GetGameFromMemory(string gameName = "boiii")
        {
            var game = Process.GetProcessesByName(gameName).FirstOrDefault();

            if(game != null)
            {
                return game;
            }

            return null;
        }

        public static IntPtr GetGameBaseMemoryAddress(Process gameProcess)
        {
            if(gameProcess != null)
            {
                return gameProcess.MainModule!.BaseAddress;
            }

            return IntPtr.Zero;
        }

        public static IntPtr GetGameModuleBaseMemoryAddress(Process gameProcess, string moduleName = "BlackOps3.exe")
        {
            if(gameProcess != null && !string.IsNullOrEmpty(moduleName))
            {
                IntPtr BoiiiModuleBaseAddress = 0;

                foreach(ProcessModule module in gameProcess.Modules)
                {
                    if(module.ModuleName.Equals(moduleName))
                    {
                        BoiiiModuleBaseAddress = module.BaseAddress;
                        return BoiiiModuleBaseAddress;
                    }
                }
            }

            return IntPtr.Zero;
        }

        public static long ReadIntFromMemory(Process gameProcess, IntPtr ValueToReadAddress)
        {
            if(gameProcess != null && ValueToReadAddress != IntPtr.Zero)
            {
                var processHandle = OpenProcess(PROCESS_WM_READ, false, gameProcess.Id);
                var buffer = new byte[4];

                bool success = ReadProcessMemory(processHandle, ValueToReadAddress, buffer, buffer.Length, out int bytesRead);

                if(!success)
                {
                    CloseHandle(processHandle);
                }

                var valueRead = BitConverter.ToInt32(buffer, 0);
                CloseHandle(processHandle);

                return valueRead;
            }

            return -1;
        }

        public static string ReadStringFromMemory(Process gameProcess, IntPtr ValueToReadAddress, int byteSize)
        {
            if(gameProcess != null && ValueToReadAddress != IntPtr.Zero)
            {
                var processHandle = OpenProcess(PROCESS_WM_READ, false, gameProcess.Id);
                var buffer = new byte[byteSize];

                bool success = ReadProcessMemory(processHandle, ValueToReadAddress, buffer, buffer.Length, out int bytesRead);

                if(!success)
                {
                    CloseHandle(processHandle);
                }

                int stringLength = Array.IndexOf(buffer, (byte)0);
                if(stringLength == -1)
                {
                    stringLength = buffer.Length;
                }

                var stringRead = Encoding.ASCII.GetString(buffer, 0, stringLength).Trim('\0');
                CloseHandle(processHandle);

                return stringRead;
            }

            return string.Empty;
        }
    }
}
