namespace Server.Job {
    /// <summary>
    /// 실행시킬 일을 이 객체를 상속한 곳에서 순서대로 실행(멀티쓰레드 환경에서 동기화할 필요 없음)하기 위한 객체
    /// </summary>
    public class JobSerializer {
        JobTimer _timer = new JobTimer();
        Queue<IJob> _jobQueue = new Queue<IJob>();
        object _lock = new object();
        bool _flush = false;

        public IJob PushAfter(int tickAfter, Action action) { return PushAfter(tickAfter, new Job(action)); }
        public IJob PushAfter<T1>(int tickAfter, Action<T1> action, T1 t1) { return PushAfter(tickAfter, new Job<T1>(action, t1)); }
        public IJob PushAfter<T1, T2>(int tickAfter, Action<T1, T2> action, T1 t1, T2 t2) { return PushAfter(tickAfter, new Job<T1, T2>(action, t1, t2)); }
        public IJob PushAfter<T1, T2, T3>(int tickAfter, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { return PushAfter(tickAfter, new Job<T1, T2, T3>(action, t1, t2, t3)); }

        /// <summary>
        /// JobTimer에 특정 시간 후에 실행시킬 Job 삽입
        /// </summary>
        /// <param name="tickAfter">몇 초 뒤에 실행시킬 것인지</param>
        /// <param name="job">실행시킬 Job</param>
        /// <returns></returns>
        public IJob PushAfter(int tickAfter, IJob job) {
            _timer.Push(job, tickAfter);
            return job;
        }

        public void Push(Action action) { Push(new Job(action)); }
        public void Push<T1>(Action<T1> action, T1 t1) { Push(new Job<T1>(action, t1)); }
        public void Push<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Push(new Job<T1, T2>(action, t1, t2)); }
        public void Push<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Push(new Job<T1, T2, T3>(action, t1, t2, t3)); }
        public void Push<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) { Push(new Job<T1, T2, T3, T4>(action, t1, t2, t3, t4)); }
        public void Push<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) { Push(new Job<T1, T2, T3, T4, T5>(action, t1, t2, t3, t4, t5)); }

        /// <summary>
        /// 실행시켜야 하는 일을 Queue에 넣어 순서대로 실행하도록 한다
        /// </summary>
        /// <param name="job">실행시켜야 하는 일</param>
        public void Push(IJob job) {
            lock (_lock) {
                _jobQueue.Enqueue(job);
            }
        }

        /// <summary>
        /// 주기적으로 실행이 되어 JobTimer의 일이 실행시킬 때가 되었는지 확인하고 Queue에 든 일 전부 처리
        /// </summary>
        public void Flush() {
            _timer.Flush();

            while (true) {
                IJob job = Pop();
                if (job == null) {
                    return;
                }

                job.Execute();
            }
        }

        /// <summary>
        /// 실행시켜야 할 일을 Queue에서 Pop하여 확인
        /// </summary>
        /// <returns>실행시켜야 하는 Job 반환</returns>
        IJob Pop() {
            lock (_lock) {
                if (_jobQueue.Count == 0) {
                    _flush = false;
                    return null;
                }
                return _jobQueue.Dequeue();
            }
        }
    }
}
