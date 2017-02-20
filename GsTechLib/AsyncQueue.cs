using System;
using System.Collections;

namespace GsTechLib
{
	public delegate void InvokeFinish(object content, IAsyncResult ar);
	public delegate void InvokeAsyncFunc(object content);

    /// <summary>
    /// 异步队列
    /// </summary>
	public class AsyncQueue
	{
		AsyncQueue()
		{
		}

        /// <summary>
        /// 异步队列工作单元，内容在其他线程执行，调度时检查如果执行完成则回调
        /// </summary>
		class Job
		{
			IAsyncResult _result;
			InvokeFinish _callback;
			object _content;

			public Job(IAsyncResult result, InvokeFinish callback, object content)
			{
				_result = result;
				_callback = callback;
				_content = content;
			}

			public bool Check(ArrayList exceptions)
			{
				if(_result.IsCompleted)
				{
					try
					{
                        if(_callback != null)
						    _callback(_content, _result);
					}
					catch(Exception ex)
					{
						if (exceptions != null)
						{
							exceptions.Add(ex);
						}
					}
					return true;
				}
				return false;
			}	
			
		}

        /// <summary>
        /// 异步队列工作单元，内容在调度时执行
        /// </summary>
		class AsyncJob
		{
			InvokeAsyncFunc _func;
            public InvokeAsyncFunc DoFunc
            {
                get
                {
                    return _func;
                }
            }
			object _content;

			public AsyncJob(InvokeAsyncFunc func, object content)
			{
				_func = func;
				_content = content;
			}

			public Exception Do()
			{
				try
				{
					_func(_content);
					return null;
				}
				catch(Exception ex)
				{
					return ex;
				}
			}		
		}

		ArrayList _jobList = new ArrayList();
		ArrayList _jobList2 = new ArrayList();

		static AsyncQueue _instance = new AsyncQueue();

		public static AsyncQueue Instance
		{
			get
			{
				return _instance;
			}
		}

        /// <summary>
        /// 添加一个异步队列工作单元，该工作单元的内容在其他线程执行
        /// </summary>
        /// <param name="result">异步调用接口</param>
        /// <param name="callback">异步队列回调接口</param>
        /// <param name="content">异步队列回调接口参数</param>
		public void AddJob(IAsyncResult result, InvokeFinish callback, object content)
		{
            lock (this)
            {
                _jobList.Add(new Job(result, callback, content));
            }
		}

        /// <summary>
        /// 添加一个异步队列工作单元，该工作单元的内容在调度时执行
        /// </summary>
        /// <param name="func">需要异步调度的内容</param>
        /// <param name="content">调度的参数</param>
		public void AddJob(InvokeAsyncFunc func, object content)
		{
            lock (this)
            {

                _jobList2.Add(new AsyncJob(func, content));
            }
		}

        public delegate void PrintMemoryFluxDele(string place);

        /// <summary>
        /// 调度异步队列
        /// </summary>
		public void Schedule()
		{
            lock (this)
            {
                for (int i = _jobList.Count - 1; i >= 0; --i)
                {
                    if (((Job)_jobList[i]).Check(null))
                    {
                        _jobList.RemoveAt(i);
                    }
                }
                for (int i = _jobList2.Count - 1; i >= 0; --i)
                {
                    ((AsyncJob)_jobList2[i]).Do();
                    _jobList2.RemoveAt(i);
                }
            }
		}

        /// <summary>
        /// 调度异步队列
        /// </summary>
        /// <param name="exceptions">输出异常列表</param>
        /// <param name="pmdel">内存统计调用接口</param>
        public void Schedule(ArrayList exceptions, PrintMemoryFluxDele pmdel)
		{
            lock (this)
            {
                Exception ex;
                for (int i = _jobList.Count - 1; i >= 0; --i)
                {
                    if (((Job)_jobList[i]).Check(exceptions))
                    {
                        _jobList.RemoveAt(i);
                    }
                }
                for (int i = _jobList2.Count - 1; i >= 0; --i)
                {
                    if (pmdel != null && ((AsyncJob)_jobList2[i]).DoFunc != null)
                        pmdel("SortedParamTimer.Before." + ((AsyncJob)_jobList2[i]).DoFunc.Method.Name);
                    
                    ex = ((AsyncJob)_jobList2[i]).Do();

                    if (pmdel != null && ((AsyncJob)_jobList2[i]).DoFunc != null)
                        pmdel("AsyncQueue.After." + ((AsyncJob)_jobList2[i]).DoFunc.Method.Name);
                    
                    if (ex != null)
                    {
                        if (exceptions != null)
                        {
                            exceptions.Add(ex);
                        }
                    }
                    _jobList2.RemoveAt(i);
                }
            }
		}
	}
}
