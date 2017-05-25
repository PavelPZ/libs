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
				while (true) {
					var data = checkThread.Take();
					data.tcs.TrySetResult(data.Run());
				}
			});
			t.SetApartmentState(ApartmentState.STA);
			t.Priority = ThreadPriority.Lowest;
			t.Start();
		}

		public static Task Run(RunObject runObj) {
			var tcs = new TaskCompletionSource<Object>();
			runObj.tcs = tcs;
			checkThread.Add(runObj);
			return tcs.Task;
		}

		static BlockingCollection<RunObject> checkThread = new BlockingCollection<RunObject>();
	}

	public class RunObject {
		public TaskCompletionSource<Object> tcs;
		public virtual object Run() { throw new Exception(); }
	}

}
