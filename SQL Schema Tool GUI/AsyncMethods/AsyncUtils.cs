using NLog;

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace Lewis.SST.AsyncMethods
{
	/// <summary>
	/// Summary description for AsyncUtils.
	/// Class taken from article on Codeproject.com, http://www.codeproject.com/csharp/LaunchProcess.asp
	/// </summary>
	/// <summary>
	/// Exception thrown by AsyncUtils.AsyncOperation.Start when an
	/// operation is already in progress.
	/// </summary>
	public class AlreadyRunningException : System.ApplicationException
	{
		/// <summary>
		/// The single method for the AlreadyRunningException class throws an exception using the base inherited class: System.ApplicationException.
		/// </summary>
		public AlreadyRunningException() : base("Asynchronous operation already running")
		{ }
	}

	/// <summary>
	/// The abstract class for AsyncOperation
	/// </summary>
	public abstract class AsyncOperation
	{
        private static Logger logger = LogManager.GetLogger("Lewis.SST.AsyncMethods.AsyncOperation");

		/// <summary>
		/// Initialises an AsyncOperation with an association to the
		/// supplied ISynchronizeInvoke.  All events raised from this
		/// object will be delivered via this target.  (This might be a
		/// Control object, so events would be delivered to that Control's
		/// UI thread.)
		/// </summary>
		/// <param name="target">An object implementing the
		/// ISynchronizeInvoke interface.  All events will be delivered
		/// through this target, ensuring that they are delivered to the
		/// correct thread.</param>
		public AsyncOperation(ISynchronizeInvoke target)
		{
			isiTarget = target;
			isRunning = false;
		}

		/// <summary>
		/// Launch the operation on a worker thread.  This method will
		/// return immediately, and the operation will start asynchronously
		/// on a worker thread.
		/// </summary>
		public void Start()
		{
			lock(this)
			{
				if (isRunning)
				{
					throw new AlreadyRunningException();
				}
				// Set this flag here, not inside InternalStart, to avoid
				// race condition when Start called twice in quick
				// succession.
				isRunning = true;
			}
			new MethodInvoker(InternalStart).BeginInvoke(null, null);
		}


		/// <summary>
		/// Attempt to cancel the current operation.  This returns
		/// immediately to the caller.  No guarantee is made as to
		/// whether the operation will be successfully cancelled.  All
		/// that can be known is that at some point, one of the
		/// three events Completed, Cancelled, or Failed will be raised
		/// at some point.
		/// </summary>
		public void Cancel()
		{
			lock(this)
			{
				cancelledFlag = true;
			}
		}

		/// <summary>
		/// Attempt to cancel the current operation and block until either
		/// the cancellation succeeds or the operation completes.
		/// </summary>
		/// <returns>true if the operation was successfully cancelled
		/// or it failed, false if it ran to completion.</returns>
		public bool CancelAndWait()
		{
			lock(this)
			{
				// Set the cancelled flag

				cancelledFlag = true;


				// Now sit and wait either for the operation to
				// complete or the cancellation to be acknowledged.
				// (Wake up and check every second - shouldn't be
				// necessary, but it guarantees we won't deadlock
				// if for some reason the Pulse gets lost - means
				// we don't have to worry so much about bizarre
				// race conditions.)
				while(!IsDone)
				{
					Monitor.Wait(this, 1000);
				}
			}
			return !HasCompleted;
		}

		/// <summary>
		/// Blocks until the operation has either run to completion, or has
		/// been successfully cancelled, or has failed with an internal
		/// exception.
		/// </summary>
		/// <returns>true if the operation completed, false if it was
		/// cancelled before completion or failed with an internal
		/// exception.</returns>
		public bool WaitUntilDone()
		{
			lock(this)
			{
				// Wait for either completion or cancellation.  As with
				// CancelAndWait, we don't sleep forever - to reduce the
				// chances of deadlock in obscure race conditions, we wake
				// up every second to check we didn't miss a Pulse.
				while (!IsDone)
				{
					Monitor.Wait(this, 1000);
				}
			}
			return HasCompleted;
		}


		/// <summary>
		/// Returns false if the operation is still in progress, or true if
		/// it has either completed successfully, been cancelled
		///  successfully, or failed with an internal exception.
		/// </summary>
		public bool IsDone
		{
			get
			{
				lock(this)
				{
					return completeFlag || cancelAcknowledgedFlag || failedFlag;
				}
			}
		}

		/// <summary>
		/// This event will be fired if the operation runs to completion
		/// without being cancelled.  This event will be raised through the
		/// ISynchronizeTarget supplied at construction time.  Note that
		/// this event may still be received after a cancellation request
		/// has been issued.  (This would happen if the operation completed
		/// at about the same time that cancellation was requested.)  But
		/// the event is not raised if the operation is cancelled
		/// successfully.
		/// </summary>
		public event EventHandler Completed;


		/// <summary>
		/// This event will be fired when the operation is successfully
		/// stoped through cancellation.  This event will be raised through
		/// the ISynchronizeTarget supplied at construction time.
		/// </summary>
		public event EventHandler Cancelled;


		/// <summary>
		/// This event will be fired if the operation throws an exception.
		/// This event will be raised through the ISynchronizeTarget
		/// supplied at construction time.
		/// </summary>
		public event System.Threading.ThreadExceptionEventHandler Failed;


		/// <summary>
		/// The ISynchronizeTarget supplied during construction - this can
		/// be used by deriving classes which wish to add their own events.
		/// </summary>
		protected ISynchronizeInvoke Target
		{
			get { return isiTarget; } 
		}
		private ISynchronizeInvoke isiTarget;


		/// <summary>
		/// To be overridden by the deriving class - this is where the work
		/// will be done.  The base class calls this method on a worker
		/// thread when the Start method is called.
		/// </summary>
		protected abstract void DoWork();


		/// <summary>
		/// Flag indicating whether the request has been cancelled.  Long-
		/// running operations should check this flag regularly if they can
		/// and cancel their operations as soon as they notice that it has
		/// been set.
		/// </summary>
		protected bool CancelRequested
		{
			get
			{
				lock(this) { return cancelledFlag; }
			}
		}
		private bool cancelledFlag;


		/// <summary>
		/// Flag indicating whether the request has run through to
		/// completion.  This will be false if the request has been
		/// successfully cancelled, or if it failed.
		/// </summary>
		protected bool HasCompleted
		{
			get
			{
				lock(this) { return completeFlag; }
			}
		}
		private bool completeFlag;


		/// <summary>
		/// This is called by the operation when it wants to indicate that
		/// it saw the cancellation request and honoured it.
		/// </summary>
		protected void AcknowledgeCancel()
		{
			lock(this)
			{
				cancelAcknowledgedFlag = true;
				isRunning = false;

				// Pulse the event in case the main thread is blocked
				// waiting for us to finish (e.g. in CancelAndWait or
				// WaitUntilDone).
				Monitor.Pulse(this);

				// Using async invocation to avoid a potential deadlock
				// - using Invoke would involve a cross-thread call
				// whilst we still held the object lock.  If the event
				// handler on the UI thread tries to access this object
				// it will block because we have the lock, but using
				// async invocation here means that once we've fired
				// the event, we'll run on and release the object lock,
				// unblocking the UI thread.
				FireAsync(Cancelled, this, EventArgs.Empty);
			}
		}
		private bool cancelAcknowledgedFlag;


		///<summary>
		/// Set to true if the operation fails with an exception.
		///</summary>
		private bool failedFlag;
		///<summary>
		/// Set to true if the operation is running
		///</summary>
		private bool isRunning;


		/// <summary>
		/// This method is called on a worker thread (via asynchronous
		/// delegate invocation).  This is where we call the operation (as
		/// defined in the deriving class's DoWork method).
		/// </summary>
		private void InternalStart()
		{
			// Reset our state - we might be run more than once.
			cancelledFlag = false;
			completeFlag = false;
			cancelAcknowledgedFlag = false;
			failedFlag = false;
			// isRunning is set during Start to avoid a race condition
			try
			{
				DoWork();
			}
			catch (Exception e)
			{
				// Raise the Failed event.  We're in a catch handler, so we
				// had better try not to throw another exception.
				try
				{
					FailOperation(e);
				}
				catch
				{ }

				// The documentation recommends not catching
				// SystemExceptions, so having notified the caller we
				// rethrow if it was one of them.
				if (e is SystemException)
				{
                    logger.Error(SQLSchemaTool.ERRORFORMAT, e.Message, e.Source, e.StackTrace);
				}
			}

			lock(this)
			{
				// If the operation wasn't cancelled (or if the UI thread
				// tried to cancel it, but the method ran to completion
				// anyway before noticing the cancellation) and it
				// didn't fail with an exception, then we complete the
				// operation - if the UI thread was blocked waiting for
				// cancellation to complete it will be unblocked, and
				// the Completion event will be raised.
				if (!cancelAcknowledgedFlag && !failedFlag)
				{
					CompleteOperation();
				}
			}
		}


		/// <summary>
		/// This is called when the operation runs to completion.
		/// (This is private because it is called automatically
		/// by this base class when the deriving class's DoWork
		/// method exits without having cancelled
		/// </summary>
		private void CompleteOperation()
		{
			lock(this)
			{
				completeFlag = true;
				isRunning = false;
				Monitor.Pulse(this);
				// See comments in AcknowledgeCancel re use of
				// Async.
				FireAsync(Completed, this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Fires event when the operation fails.
		/// </summary>
		/// <param name="e">The exception.</param>
		private void FailOperation(Exception e)
		{
			lock(this)
			{
				failedFlag = true;
				isRunning = false;
				Monitor.Pulse(this);
				FireAsync(Failed, this, new ThreadExceptionEventArgs(e));
			}
		}

		/// <summary>
		/// Utility function for firing an event through the target.
		/// It uses C#'s variable length parameter list support
		/// to build the parameter list.
		/// 
		/// This functions presumes that the caller holds the object lock.
		/// (This is because the event list is typically modified on the UI
		/// thread, but events are usually raised on the worker thread.)
		/// </summary>
		/// <param name="dlg">The async thread Delegate.</param>
		/// <param name="pList">The param list array.</param>
		protected void FireAsync(Delegate dlg, params object[] pList)
		{
			if (dlg != null)
			{
                try
                {
                    Target.BeginInvoke(dlg, pList);
                }
                catch (Exception ex)
                {
                    logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
                }
			}
		}
	}
}
