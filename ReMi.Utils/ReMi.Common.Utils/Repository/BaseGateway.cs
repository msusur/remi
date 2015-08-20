using System;
using Common.Logging;

namespace ReMi.Common.Utils.Repository
{
    public abstract class BaseGateway
    {
        private bool _disposed;

        protected readonly ILog Logger = LogManager.GetCurrentClassLogger();


        #region IDispose implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        OnDisposing();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void OnDisposing()
        {

        }

        #endregion
    }
}
