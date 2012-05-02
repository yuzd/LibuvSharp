using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public enum FileAccess
	{
		Read = 0,
		Write = 1,
		ReadWrite = 3,
	}

	// TODO:
	// 1. fix stat
	// 2. uv_fs_SYMLINK
	// 3. uv_fs_readlink
	// 4. uv_fs_utime uv_fs_futime

	public class File
	{
		public File(Loop loop, IntPtr handle)
		{
			DefaultLoop = loop;
			FileHandle = handle;
		}

		public Loop DefaultLoop { get; protected set; }
		public IntPtr FileHandle { get; protected set; }

		[DllImport("uv")]
		private static extern int uv_fs_open(IntPtr loop, IntPtr req, string path, int flags, int mode, Action<IntPtr> callback);

		public static void Open(Loop loop, string path, FileAccess access, Action<Exception, File> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => {
				File file = null;
				if (fsr.Result != IntPtr.Zero) {
					file = new File(loop, fsr.Result);
				}
				callback(ex, file);
			};
			int r = uv_fs_open(loop.Handle, fsr.Handle, path, (int)access, 0, fsr.End);
			Ensure.Success(r, loop);
		}
		public static void Open(string path, FileAccess access, Action<Exception, File> callback)
		{
			Open(Loop.Default, path, access, callback);
		}

		[DllImport("uv")]
		private static extern int uv_fs_close(IntPtr loop, IntPtr req, IntPtr file, Action<IntPtr> callback);

		public void Close(Loop loop, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_close(loop.Handle, fsr.Handle, FileHandle, fsr.End);
			Ensure.Success(r, loop);
		}
		public void Close(Loop loop)
		{
			Close(loop, null);
		}
		public void Close(Action<Exception> callback)
		{
			Close(Loop.Default, callback);
		}
		public void Close()
		{
			Close(Loop.Default);
		}

		[DllImport("uv")]
		private static extern int uv_fs_read(IntPtr loop, IntPtr req, IntPtr file, IntPtr buf, IntPtr length, long offset, Action<IntPtr> callback);

		public void Read(Loop loop, byte[] data, int length, int offset, Action<Exception, int> callback)
		{
			GCHandle datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var fsr = new FileSystemRequest();
			fsr.Callback += (ex, fsr2) => {
				callback(ex, fsr.Result.ToInt32());
				datagchandle.Free();
			};
			int r = uv_fs_read(loop.Handle, fsr.Handle, FileHandle, datagchandle.AddrOfPinnedObject(), (IntPtr)length, offset, fsr.End);
			Ensure.Success(r, loop);
		}
		public void Read(byte[] data, int length, int offset, Action<Exception, int> callback)
		{
			Read(Loop.Default, data, length, offset, callback);
		}

		[DllImport("uv")]
		private static extern int uv_fs_write(IntPtr loop, IntPtr req, IntPtr file, IntPtr buf, int length, int offset);

		public void Write(Loop loop, byte[] data, int length, int offset, Action<Exception, int> callback)
		{
			var datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var fsr = new FileSystemRequest();
			fsr.Callback += (ex, fsr2) => {
				callback(ex, (int)fsr.Result);
				datagchandle.Free();
			};
			int r = uv_fs_write(loop.Handle, fsr.Handle, FileHandle, datagchandle.AddrOfPinnedObject(), length, offset);
			Ensure.Success(r, loop);
		}
		public void Write(Loop loop, byte[] data, int length, int offset)
		{
			Write(loop, data, length, offset, null);
		}
		public void Write(Loop loop, byte[] data, int length)
		{
			Write(loop, data, length, -1);
		}
		public void Write(Loop loop, byte[] data)
		{
			Write(loop, data, data.Length);
		}
		public void Write(Loop loop, byte[] data, int length, Action<Exception, int> callback)
		{
			Write(loop, data, length, -1, callback);
		}
		public void Write(Loop loop, byte[] data, Action<Exception, int> callback)
		{
			Write(loop, data, data.Length, callback);
		}
		public void Write(byte[] data, int length, int offset, Action<Exception, int> callback)
		{
			Write(Loop.Default, data, length, offset, callback);
		}
		public void Write(byte[] data, int length, int offset)
		{
			Write(data, length, offset, null);
		}
		public void Write(byte[] data, int length)
		{
			Write(data, length, -1);
		}
		public void Write(byte[] data)
		{
			Write(data, data.Length);
		}
		public void Write(byte[] data, int length, Action<Exception, int> callback)
		{
			Write(data, length, -1, callback);
		}
		public void Write(byte[] data, Action<Exception, int> callback)
		{
			Write(data, data.Length, callback);
		}

		[DllImport("uv")]
		unsafe private static extern void uv_fs_req_stat(IntPtr req, lin_stat *stat);

		[DllImport("uv")]
		private static extern int uv_fs_stat(IntPtr loop, IntPtr req, string path, Action<IntPtr> callback);

		unsafe public static void Stat(Loop loop, string path, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => {
				lin_stat stats = new lin_stat();
				uv_fs_req_stat(fsr.Handle, &stats);
				Console.WriteLine (stats);
				callback(ex);
			};
			int r = uv_fs_stat(loop.Handle, fsr.Handle, path, fsr.End);
			Ensure.Success(r, loop);
		}

		[DllImport("uv")]
		private static extern int uv_fs_fstat(IntPtr loop, IntPtr req, IntPtr file, Action<IntPtr> callback);

		unsafe public void Stat(Loop loop, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => {
				lin_stat stats = new lin_stat();
				uv_fs_req_stat(fsr.Handle, &stats);
				Console.WriteLine (stats);
				callback(ex);
			};
			int r = uv_fs_fstat(loop.Handle, fsr.Handle, FileHandle, fsr.End);
			Ensure.Success(r, loop);
		}

		[DllImport("uv")]
		private static extern int uv_fs_fsync(IntPtr loop, IntPtr req, IntPtr file, Action<IntPtr> callback);

		public void Sync(Loop loop, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_fsync(loop.Handle, fsr.Handle, FileHandle, fsr.End);
			Ensure.Success(r, loop);
		}
		public void Sync(Loop loop)
		{
			Sync(loop, null);
		}
		public void Sync(Action<Exception> callback)
		{
			Sync(Loop.Default, callback);
		}
		public void Sync()
		{
			Sync((Action<Exception>)null);
		}

		[DllImport("uv")]
		private static extern int uv_fs_fdatasync(IntPtr loop, IntPtr req, IntPtr file, Action<IntPtr> callback);

		public void DataSync(Loop loop, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_fdatasync(loop.Handle, fsr.Handle, FileHandle, fsr.End);
			Ensure.Success(r, loop);
		}
		public void DataSync(Loop loop)
		{
			DataSync(loop, null);
		}
		public void DataSync(Action<Exception> callback)
		{
			DataSync(Loop.Default, callback);
		}
		public void DataSync()
		{
			DataSync((Action<Exception>)null);
		}

		[DllImport("uv")]
		private static extern int uv_fs_ftruncate(IntPtr loop, IntPtr req, IntPtr file, int offset, Action<IntPtr> callback);

		public void Truncate(Loop loop, int offset, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_ftruncate(loop.Handle, fsr.Handle, FileHandle, offset, fsr.End);
			Ensure.Success(r, loop);
		}
		public void Truncate(Loop loop, int offset)
		{
			Truncate(loop, offset);
		}
		public void Truncate(int offset, Action<Exception> callback)
		{
			Truncate(Loop.Default, offset, callback);
		}
		public void Truncate(int offset)
		{
			Truncate(offset, null);
		}

		[DllImport("uv")]
		private static extern int uv_fs_sendfile(IntPtr loop, IntPtr req, IntPtr out_fd, IntPtr in_fd, int offset, int length, Action<IntPtr> callback);

		public void Send(Loop loop, Tcp socket, int offset, int length, Action<Exception, int> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex, (int)fsr.Result); };
			int r = uv_fs_sendfile(loop.Handle, fsr.Handle, socket.handle, FileHandle, offset, length, fsr.End);
			Ensure.Success(r, loop);
		}
		public void Send(Tcp socket, int offset, int length, Action<Exception, int> callback)
		{
			Send(Loop.Default, socket, offset, length, callback);
		}

		[DllImport("uv")]
		private static extern int uv_fs_fchmod(IntPtr loop, IntPtr req, IntPtr file, int mode, Action<IntPtr> callback);

		public void Chmod(Loop loop, int mode, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_fchmod(loop.Handle, fsr.Handle, FileHandle, mode, fsr.End);
			Ensure.Success(r, loop);
		}
		public void Chmod(Loop loop, int mode)
		{
			Chmod(loop, mode, null);
		}
		public void Chmod(int mode, Action<Exception> callback)
		{
			Chmod(Loop.Default, mode, callback);
		}
		public void Chmod(int mode)
		{
			Chmod(mode, null);
		}

		[DllImport("uv")]
		private static extern int uv_fs_chmod(IntPtr loop, IntPtr req, string path, int mode, Action<IntPtr> callback);

		public static void Chmod(Loop loop, string path, int mode, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_chmod(loop.Handle, fsr.Handle, path, mode, fsr.End);
			Ensure.Success(r, loop);
		}
		public static void Chmod(Loop loop, string path, int mode)
		{
			Chmod(loop, path, mode, null);
		}
		public static void Chmod(string path, int mode, Action<Exception> callback)
		{
			Chmod(Loop.Default, path, mode, callback);
		}
		public static void Chmod(string path, int mode)
		{
			Chmod(path, mode, null);
		}

		[DllImport("uv")]
		private static extern int uv_fs_chown(IntPtr loop, IntPtr req, string path, int uid, int gid, Action<IntPtr> callback);

		public static void Chown(Loop loop, string path, int uid, int gid, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_chown(loop.Handle, fsr.Handle, path, uid, gid, fsr.End);
			Ensure.Success(r, loop);
		}
		public static void Chown(Loop loop, string path, int uid, int gid)
		{
			Chown(loop, path, uid, gid, null);
		}
		public static void Chown(string path, int uid, int gid, Action<Exception> callback)
		{
			Chown(Loop.Default, path, uid, gid, callback);
		}
		public static void Chown(string path, int uid, int gid)
		{
			Chown(path, uid, gid, null);
		}

		[DllImport("uv")]
		private static extern int uv_fs_fchown(IntPtr loop, IntPtr req, IntPtr file, int uid, int gid, Action<IntPtr> callback);

		public void Chown(Loop loop, int uid, int gid, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_fchown(loop.Handle, fsr.Handle, FileHandle, uid, gid, fsr.End);
			Ensure.Success(r, loop);
		}
		public void Chown(Loop loop, int uid, int gid)
		{
			Chown(loop, uid, gid, null);
		}
		public void Chown(int uid, int gid, Action<Exception> callback)
		{
			Chown(Loop.Default, uid, gid, callback);
		}
		public void Chown(int uid, int gid)
		{
			Chown(uid, gid, null);
		}

		[DllImport("uv")]
		private static extern int uv_fs_unlink(IntPtr loop, IntPtr req, string path, Action<IntPtr> callback);

		public static void Unlink(Loop loop, string path, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_unlink(loop.Handle, fsr.Handle, path, fsr.End);
			Ensure.Success(r, loop);
		}
		public static void Unlink(Loop loop, string path)
		{
			Unlink(loop, path, null);
		}
		public static void Unlink(string path, Action<Exception> callback)
		{
			Unlink(Loop.Default, path, callback);
		}
		public static void Unlink(string path)
		{
			Unlink(path, null);
		}

		[DllImport("uv")]
		private static extern int uv_fs_link(IntPtr loop, IntPtr req, string path, string newPath, Action<IntPtr> callback);

		public static void Link(Loop loop, string path, string newPath, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_link(loop.Handle, fsr.Handle, path, newPath, fsr.End);
			Ensure.Success(r, loop);
		}
		public static void Link(Loop loop, string path, string newPath)
		{
			Link(loop, path, newPath, null);
		}
		public static void Link(string path, string newPath, Action<Exception> callback)
		{
			Link(Loop.Default, path, newPath, callback);
		}
		public static void Link(string path, string newPath)
		{
			Link(path, newPath, null);
		}
	}

	public class Directory
	{
		[DllImport("uv")]
		private static extern int uv_fs_mkdir(IntPtr loop, IntPtr req, string path, int mode, Action<IntPtr> callback);

		public static void Create(Loop loop, string path, int mode, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_mkdir(loop.Handle, fsr.Handle, path, mode, fsr.End);
			Ensure.Success(r, loop);
		}
		public static void Create(Loop loop, string path, int mode)
		{
			Create(loop, path, mode, null);
		}
		public static void Create(string path, int mode, Action<Exception> callback)
		{
			Create(Loop.Default, path, mode, callback);
		}
		public static void Create(string path, int mode)
		{
			Create(path, mode, null);
		}

		[DllImport("uv")]
		private static extern int uv_fs_rmdir(IntPtr loop, IntPtr req, string path, Action<IntPtr> callback);

		public static void Delete(Loop loop, string path, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_rmdir(loop.Handle, fsr.Handle, path, fsr.End);
			Ensure.Success(r, loop);
		}
		public static void Delete(Loop loop, string path)
		{
			Delete(loop, path, null);
		}
		public static void Delete(string path, Action<Exception> callback)
		{
			Delete(Loop.Default, path, callback);
		}
		public static void Delete(string path)
		{
			Delete(path, null);
		}

		[DllImport("uv")]
		private static extern int uv_fs_rename(IntPtr loop, IntPtr req, string path, string newPath, Action<IntPtr> callback);

		public static void Rename(Loop loop, string path, string newPath, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => { callback(ex); };
			int r = uv_fs_rename(loop.Handle, fsr.Handle, path, newPath, fsr.End);
			Ensure.Success(r, loop);
		}
		public static void Rename(Loop loop, string path, string newPath)
		{
			Rename(loop, path, newPath, null);
		}
		public static void Rename(string path, string newPath, Action<Exception> callback)
		{
			Rename(Loop.Default, path, newPath, callback);
		}
		public static void Rename(string path, string newPath)
		{
			Rename(path, newPath, null);
		}

		[DllImport("__Internal")]
		private static extern int strlen(IntPtr ptr);

		[DllImport("uv")]
		private static extern int uv_fs_readdir(IntPtr loop, IntPtr req, string path, int flags, Action<IntPtr> callback);

		unsafe public static void Read(Loop loop, string path, Action<Exception, List<string>> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex, fsr2) => {
				if (ex != null) {
					callback(ex, null);
					return;
				}

				int length = (int)fsr.Result;
				List<string> list = new List<string>(length);
				sbyte *ptr = (sbyte *)fsr.Pointer;
				for (int i = 0; i < length; i++) {
					list.Add(new string(ptr));
					ptr += strlen((IntPtr)ptr) + 1;
				}
				callback(ex, list);
			};
			uv_fs_readdir(loop.Handle, fsr.Handle, path, 0, fsr.End);
		}
		public static void Read(string path, Action<Exception, List<string>> callback)
		{
			Read(Loop.Default, path, callback);
		}

	}
}

