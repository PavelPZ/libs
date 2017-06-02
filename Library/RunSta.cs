using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STALib {

	public static class Lib {

		static Lib() {
			Thread t = new Thread(() => {
				while (true)
					checkThread.Take().doRun();
			});
			t.SetApartmentState(ApartmentState.STA);
			t.Priority = ThreadPriority.Lowest;
			t.Start();
		}

		public static Task<T> Run<T>(RunObject<T> runObj) where T : class {
			var tcs = new TaskCompletionSource<T>();
			runObj.tcs = tcs;
			checkThread.Add(runObj);
			return tcs.Task;
		}

		static BlockingCollection<RunObjectLow> checkThread = new BlockingCollection<RunObjectLow>();
	}

	public interface RunObjectLow {
		void doRun();
	}

	public interface RunObject<T> : RunObjectLow where T : class {
		TaskCompletionSource<T> tcs { get; set; }
		T Run();
	}

}
