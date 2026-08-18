using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CFilter
{
    class CFilter
    {

        #region CONSTANTS

        // file access constants
        private const uint FILE_READ_ACCESS = 0x80000000;
        private const uint FILE_WRITE_ACCESS = 0x40000000;

        // file creation disposition constant
        private const uint OPEN_EXISTING = 0x00000003;

        // file attributes constant
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        // invalid handle constant
        private const int INVALID_HANDLE_VALUE = -1;

        // iocontrol codes as defined in driver's hooh.h
        private const uint IOCTL_FILTERHOOK_LIST_ADAPTERS = 1236996;
        private const uint IOCTL_FILTERHOOK_OPEN_ADAPTER = 1237000;
        private const uint IOCTL_FILTERHOOK_CLOSE_ADAPTER = 1237004;

        #endregion CONSTANTS

        #region MEMBERS

        // Path to cfilter driver
        private string m_sDriverName ="\\\\.\\\\PassThru";

        // IntegerPointer to hold the handle of the driver
        private IntPtr m_iDriverHandle = IntPtr.Zero;

        // Bool to hold whether we have a connection to the driver
        private bool m_bDriverOpened = false;

        #endregion MEMBERS

        # region CONSTRUCTOR

        public CFilter()
        {
            this.m_iDriverHandle = CreateFile(this.m_sDriverName, 
					FILE_READ_ACCESS | FILE_WRITE_ACCESS, 0, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
            if ((int)this.m_iDriverHandle > 0)
            {
                Console.WriteLine("Driver opened");
                m_bDriverOpened = true;
                
                // temporary buffer to store adapter information returned
                // from kernel mode driver
                byte[] kernelBuffer = new byte[1024];
                // length of information returned from kernel mode driver
                uint kernelLength = 0;

                unsafe
                {
                    fixed (void* vpKernelBuffer = kernelBuffer)
                    {
                        DeviceIoControl(this.m_iDriverHandle, IOCTL_FILTERHOOK_LIST_ADAPTERS,
                                                null, 0, vpKernelBuffer, (uint)kernelBuffer.Length,
                                                &kernelLength, 0);
                    }
                }

                string[] myStrings = Encoding.Unicode.GetString(kernelBuffer).Split('\0');

                for (int i = 0; i < myStrings.Length; i++)
                {
                    if (myStrings[i] != "")
                    {
                        Console.WriteLine(myStrings[i]);
                        byte[] deviceName;
                        deviceName = (new UnicodeEncoding()).GetBytes(myStrings[i]);

                        unsafe
                        {
                            fixed (void* vpDeviceName = deviceName)
                            {
                                DeviceIoControl(this.m_iDriverHandle, IOCTL_FILTERHOOK_OPEN_ADAPTER,
                                                        vpDeviceName, (uint)deviceName.Length, null, 0,
                                                        &kernelLength, 0);
                            }
                        }
                    }
                }                
                
            }
            else
            {
                Console.WriteLine("Error opening driver");
                m_bDriverOpened = false;
            }
            
            CloseHandle(this.m_iDriverHandle);
            
        }

        # endregion CONSTRUCTOR

        static void Main(string[] args)
        {
            CFilter filter = new CFilter();
                        
            return;
        }

        #region IMPORTS

        // Import definitions taken from MSDN library

        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe IntPtr CreateFile(
              string FileName,                    // file name
              uint DesiredAccess,                 // access mode
              uint ShareMode,                     // share mode
              uint SecurityAttributes,            // Security Attributes
              uint CreationDisposition,           // how to create
              uint FlagsAndAttributes,            // file attributes
              int hTemplateFile                   // handle to template file
              );

        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe bool ReadFile(
              IntPtr hFile,                       // handle to file
              void* pBuffer,                      // data buffer
              int NumberOfBytesToRead,            // number of bytes to read
              int* pNumberOfBytesRead,            // number of bytes read
              int Overlapped                      // overlapped buffer
              );

        [DllImport("kernel32", SetLastError = true)]
        private static extern unsafe bool WriteFile(
            IntPtr hFile,					// handle to file
            void* pBuffer,				// pointer to the buffer to write
            uint NumberOfBytesToWrite,	// number of bytes to write from the buffer
            uint* NumberOfBytesWritten,	// [out] number of bytes written to the file
            uint Overlapped);			// used for async reading and writing

        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe bool CloseHandle(
              IntPtr hObject   // handle to object
              );

        [DllImport("kernel32", SetLastError = true)]
        private static extern unsafe bool DeviceIoControl(
            IntPtr hDevice,				// handle of the device
            uint IoControlCode,			// IO control code to execute
            void* pBuffer,				// Input buffer for the execution
            uint InBufferSize,			// size of the input buffer
            void* OutBuffer,				// [out] output buffer for the execution
            uint OutBufferSize,			// [size of the output buffer
            uint* BytesReturned,			// [out] number of bytes returned
            uint Overlapped);			// used for async reading and writing

        #endregion IMPORTS

    }
}
