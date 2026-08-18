/*
 *    Copyright 2007 Travis Jones
 * 
 *    This file is part of SIFT.
 *
 *    SIFT is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    SIFT is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with SIFT.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Sift.Resources
{
    public class Impersonator : IDisposable
    {
        private WindowsImpersonationContext _impersonationContext = null;

        #region TYPES

        private enum LogonType
        {
            Interactive = 2,
            Network = 3,
            Batch = 4,
            Service = 5,
            Unlock = 7,
            NetworkClearText = 8,
            NewCredentials = 9
        }

        private enum LogonProviderType
        {
            Default = 0
        }


        #endregion

        #region IMPORTS

        // Import definitions taken from MSDN library

        [DllImport("advapi32", SetLastError = true)]
        private static extern unsafe bool LogonUser(
            string UserName,        // The username for impersonation
            string Domain,          // The domain or machine name for impersonation
            string Password,        // The password for impersonation (from all documentation, this seems to be sent in clear text)
            int LogonType,          // The type of interraction required for the logged in session
            int LogonProvider,      // The provider to use. Only the default provider exists
            ref IntPtr UserHandle   // The pointer to the logged in user context
            );

        [DllImport("kernel32", SetLastError = true)]
        private static extern unsafe bool CloseHandle(
              IntPtr hObject   // handle to object
              );

        #endregion

        #region CONSTRUCTOR/DESTRUCTOR/DISPOSE

        public Impersonator( string Username, string Domain, string Password)
        {
            if (Username == string.Empty)
            {
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                _impersonationContext = identity.Impersonate();
            }
            else
            {
                IntPtr userHandle = IntPtr.Zero;

                try
                {
                    bool IsLoggedOn = false;

                    unsafe
                    {
                        IsLoggedOn = LogonUser(Username, Domain, Password, (int)LogonType.Interactive, (int)LogonProviderType.Default, ref userHandle);
                    }

                    if (IsLoggedOn)
                    {
                        _impersonationContext = WindowsIdentity.Impersonate(userHandle);
                    }
                    else
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                }
                finally
                {
                    if (userHandle != IntPtr.Zero)
                    {
                        unsafe
                        {
                            CloseHandle(userHandle);
                        }
                    }
                }
            }
        }

        ~Impersonator()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (_impersonationContext != null)
            {
                _impersonationContext.Undo();
            }
        }

        #endregion                
    }
}
