﻿namespace LibFoxyProxy.Security;

public abstract class NativeRef : IDisposable
{
    public static implicit operator IntPtr(NativeRef obj) => obj.Handle;

    public IntPtr Handle { get; set; }

    public NativeRef(IntPtr handle)
    {
        Handle = handle;

        if (Handle == IntPtr.Zero)
        {
            throw new OpenSslException("NativeRef object failed to be created");
        }
    }

    public virtual void Dispose()
    {
        throw new NotImplementedException();
    }
}
