using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Tran = System.Transactions.Transaction;

namespace EFRepository
{
	public class ScopedTransaction : IDisposable
	{
		protected TransactionScope Transaction;


		public IsolationLevel Isolation { get; protected set; }

		public ScopedTransaction()
		{
			Transaction = new TransactionScope(TransactionScopeOption.Required, 
				new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
				TransactionScopeAsyncFlowOption.Enabled);

			Isolation = IsolationLevel.ReadCommitted;

			TransactionManager.DistributedTransactionStarted += TransactionManager_DistributedTransactionStarted;
		}

		private void TransactionManager_DistributedTransactionStarted(object sender, TransactionEventArgs e)
		{
			throw new NotImplementedException();
		}

		public ScopedTransaction(IsolationLevel isolation)
		{
			Transaction = new TransactionScope(TransactionScopeOption.Required, 
				new TransactionOptions { IsolationLevel = isolation },
				TransactionScopeAsyncFlowOption.Enabled);

			Isolation = isolation;
		}

		public ScopedTransaction(IsolationLevel isolation, TransactionScopeOption scopeOption)
		{
			Transaction = new TransactionScope(scopeOption, new TransactionOptions { IsolationLevel = isolation });
			Isolation = isolation;
			// TODO: Finish Me!
		}


		// Want Transaction transacitonToUse
		// Want Timeout
		// 
		/*
		public ScopedTransaction(Transaction transactionToUse);
		public ScopedTransaction(TransactionScopeOption scopeOption, TimeSpan scopeTimeout);
		//public ScopedTransaction(TransactionScopeOption scopeOption, TransactionOptions transactionOptions);
		public ScopedTransaction(Transaction transactionToUse, TimeSpan scopeTimeout);
		//public ScopedTransaction(TransactionScopeOption scopeOption, TransactionOptions transactionOptions, TransactionScopeAsyncFlowOption asyncFlowOption);
		//public ScopedTransaction(TransactionScopeOption scopeOption, TransactionOptions transactionOptions, EnterpriseServicesInteropOption interopOption);
		public ScopedTransaction(Transaction transactionToUse, TimeSpan scopeTimeout, TransactionScopeAsyncFlowOption asyncFlowOption);
		public ScopedTransaction(Transaction transactionToUse, TimeSpan scopeTimeout, EnterpriseServicesInteropOption interopOption);
	*/

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


		public void Commit()
		{
			Transaction.Complete();
		}

		public void Rollback()
		{
			Transaction.Dispose();
			Transaction = null;
		}

		public void Dispose()
		{
			if (Transaction != null)
			{
				Transaction.Dispose();
				Transaction = null;
			}
		}

	}
}
