//
//  This file is a part of Hushpuppy <https://github.com/piedar/Hushpuppy>.
//
//  Author(s):
//       Bennjamin Blast <bennjamin.blast@gmail.com>
//
//  Copyright (c) 2015 Bennjamin Blast
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;

namespace Hushpuppy.Http
{
	[Serializable]
	class MagicException : Exception
	{
		public MagicException(String message, Int32 errorCode)
			: base(String.Format("libmagic returned {0}: {1}", errorCode, message))
		{
			ErrorCode = errorCode;
		}

		public Int32 ErrorCode
		{
			get; private set;
		}

		protected MagicException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	[Flags]
	enum MagicFlags
	{
		MAGIC_NONE              = 0,		// No flags
		MAGIC_DEBUG             = 1,		// Turn on debugging
		MAGIC_SYMLINK           = 1 << 1,	// Follow symlinks
		MAGIC_COMPRESS          = 1 << 2,	// Check inside compressed files
		MAGIC_DEVICES           = 1 << 3,	// Look at the contents of devices
		MAGIC_MIME_TYPE         = 1 << 4,	// Return only the MIME type
		MAGIC_CONTINUE          = 1 << 5,	// Return all matches
		MAGIC_CHECK             = 1 << 6,	// Print warnings to stderr
		MAGIC_PRESERVE_ATIME    = 1 << 7,	// Restore access time on exit
		MAGIC_RAW               = 1 << 8,	// Don't translate unprint chars
		MAGIC_ERROR             = 1 << 9,	// Handle ENOENT etc as real errors
		MAGIC_MIME_ENCODING     = 1 << 10,	// Return only the MIME encoding
		MAGIC_MIME              = (MAGIC_MIME_TYPE | MAGIC_MIME_ENCODING),
		MAGIC_NO_CHECK_COMPRESS = 1 << 12,	// Don't check for compressed files
		MAGIC_NO_CHECK_TAR      = 1 << 13,	// Don't check for tar files
		MAGIC_NO_CHECK_SOFT     = 1 << 14,	// Don't check magic entries
		MAGIC_NO_CHECK_APPTYPE  = 1 << 15,	// Don't check application type
		MAGIC_NO_CHECK_ELF      = 1 << 16,	// Don't check for elf details
		MAGIC_NO_CHECK_ASCII    = 1 << 17,	// Don't check for ascii files
		MAGIC_NO_CHECK_TOKENS   = 1 << 20,	// Don't check ascii/tokens

		// Defined for backwards compatibility; do nothing
		MAGIC_NO_CHECK_FORTRAN  = 0,		// Don't check ascii/fortran
		MAGIC_NO_CHECK_TROFF    = 0			// Don't check ascii/troff
	}

	enum MagicActions
	{
		FILE_LOAD    = 0,
		FILE_CHECK   = 1,
		FILE_COMPILE = 2,
		FILE_LIST    = 3,
	}

	/// <summary>
	/// Managed wrapper around libmagic.
	/// </summary>
	class Magic : IDisposable
	{
		private IntPtr _magic = IntPtr.Zero;

		public Magic(MagicFlags flags)
		{
			flags = flags | MagicFlags.MAGIC_PRESERVE_ATIME;
			_magic = magic_open(flags);
			magic_load(_magic, null); // load default database
			ThrowIfError();
		}

		~Magic()
		{
			Dispose();
		}

		public void Dispose()
		{
			magic_close(_magic);
			_magic = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}

		public String Lookup(String filename)
		{
			String text = Marshal.PtrToStringAuto(magic_file(_magic, filename));
			ThrowIfError();
			return text;
		}

		public String Lookup(Byte[] data)
		{
			String text = Marshal.PtrToStringAuto(magic_buffer(_magic, data, data.Length));
			ThrowIfError();
			return text;
		}

		private void ThrowIfError()
		{
			Int32 errorCode = magic_errno(_magic);
			String message = Marshal.PtrToStringAuto(magic_error(_magic));
			if (errorCode != 0 || !String.IsNullOrEmpty(message))
			{
				throw new MagicException(message, errorCode);
			}
		}


		[DllImport("magic", EntryPoint="magic_open")]
		private static extern IntPtr magic_open(MagicFlags flags);

		[DllImport("magic", EntryPoint="magic_close")]
		private static extern void magic_close(IntPtr magic_handle);

		[DllImport("magic", EntryPoint="magic_load")]
		private static extern Int32 magic_load(IntPtr magic_handle, String filename);

		[DllImport("magic", EntryPoint="magic_file")]
		private static extern IntPtr magic_file(IntPtr magic_handle, String filename);

		[DllImport("magic", EntryPoint="magic_buffer")]
		private static extern IntPtr magic_buffer(IntPtr magic_handle, Byte[] data, Int32 dataLength);

		[DllImport("magic", EntryPoint="magic_error")]
		private static extern IntPtr magic_error(IntPtr magic_handle);

		[DllImport("magic", EntryPoint="magic_errno")]
		private static extern Int32 magic_errno(IntPtr magic_handle);

		/*
		[DllImport("magic", EntryPoint="magic_setflags")]
		private static extern Int32 magic_setflags(IntPtr magic_handle, MagicFlags flags);

		[DllImport("magic", EntryPoint="magic_descriptor")]
		private static extern IntPtr magic_descriptor(IntPtr magic_handle, Int32 fd);

		[DllImport("magic", EntryPoint="magic_compile")]
		private static extern Int32 magic_compile(IntPtr magic_handle, String filename);

		[DllImport("magic", EntryPoint="magic_check")]
		private static extern Int32 magic_check(IntPtr magic_handle, String filename);

		[DllImport("magic", EntryPoint="magic_version")]
		private static extern Int32 magic_version();

		[DllImport("magic", EntryPoint="magic_list")]
		private static extern Int32 magic_list(IntPtr magic_handle, String filename);

		[DllImport("magic", EntryPoint="magic_getpath")]
		private static extern Int32 magic_getpath(String filename, MagicActions action);
		*/
	}
}

