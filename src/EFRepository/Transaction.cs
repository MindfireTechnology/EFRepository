using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NETSTANDARD1_4
using System.Transactions;
#else
using System.Transactions;
#endif

namespace EFRepository
{
	/// <summary>
	/// A wrapper for TransactionScope that provides a simple abstraction allowing for the
	/// most important parameters to be passed in more easily and ignoring parameters that
	/// exist only for backward compatability.
	/// </summary>
	public class ScopedTransaction : IDisposable
	{
		protected TransactionScope Transaction;

		public ScopedTransaction()
		{
			Transaction = new TransactionScope(TransactionScopeOption.Required,
				new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
				TransactionScopeAsyncFlowOption.Enabled);
		}

		public ScopedTransaction(IsolationLevel isolation)
		{
			Transaction = new TransactionScope(TransactionScopeOption.Required,
				new TransactionOptions { IsolationLevel = isolation },
				TransactionScopeAsyncFlowOption.Enabled);
		}

		public ScopedTransaction(IsolationLevel isolation, TransactionScopeOption scopeOption)
		{
			Transaction = new TransactionScope(scopeOption,
				new TransactionOptions { IsolationLevel = isolation },
				TransactionScopeAsyncFlowOption.Enabled);
		}

		public ScopedTransaction(IsolationLevel isolation, TransactionScopeOption scopeOption, TimeSpan scopeTimeout)
		{
			Transaction = new TransactionScope(scopeOption,
				new TransactionOptions { IsolationLevel = isolation, Timeout = scopeTimeout },
				TransactionScopeAsyncFlowOption.Enabled);
		}

		public ScopedTransaction(Transaction transactionToUse)
		{
			Transaction = new TransactionScope(transactionToUse,
				TransactionScopeAsyncFlowOption.Enabled);
		}

		public ScopedTransaction(Transaction transactionToUse, TransactionScopeOption scopeOption)
		{
			Transaction = new TransactionScope(transactionToUse,
				TransactionScopeAsyncFlowOption.Enabled);
		}

		public ScopedTransaction(Transaction transactionToUse, TransactionScopeOption scopeOption, TimeSpan scopeTimeout)
		{
			Transaction = new TransactionScope(transactionToUse,
				scopeTimeout,
				TransactionScopeAsyncFlowOption.Enabled);
		}

		/// <summary>
		/// Completes the transaction and commits any changes
		/// </summary>
		public void Commit()
		{
			Transaction.Complete();
		}

		/// <summary>
		/// Ends the transaction scope without committing any changes
		/// </summary>
		public void Rollback()
		{
			Transaction.Dispose();
			Transaction = null;
		}

		/// <summary>
		/// Ends the transaction scope. If changes have not been committed, they will be rolled back.
		/// </summary>
		public void Dispose()
		{
			if (Transaction != null)
			{
				Transaction.Dispose();
				Transaction = null;
			}
		}

		protected int GetTransactionCardinality(IsolationLevel level)
		{
			switch (level)
			{
				case IsolationLevel.Unspecified:
					return 0;
				case IsolationLevel.Chaos:
					return 0;
				case IsolationLevel.ReadUncommitted:
					return 1;
				case IsolationLevel.ReadCommitted:
					return 2;
				case IsolationLevel.RepeatableRead:
					return 3;
				case IsolationLevel.Serializable:
					return 4;
				case IsolationLevel.Snapshot:
					return 5;
				default:
					return 0;
			}
		}

	}
}
